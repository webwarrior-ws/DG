#!/usr/bin/env -S dotnet fsi

open System
open System.Net
open System.Net.Http
open System.Net.Http.Headers

#r "nuget: FSharp.Data, Version=5.0.2"
open FSharp.Data

#r "nuget: Fsdk, Version=0.6.0--date20230214-0422.git-1ea6f62"

open Fsdk
open Fsdk.Process
open Fsdk.FSharpUtil

let githubRepository = Environment.GetEnvironmentVariable "GITHUB_REPOSITORY"

if String.IsNullOrEmpty githubRepository then
    Console.Error.WriteLine
        "This script is meant to be used only within a GitHubCI pipeline."

    Environment.Exit 1

let githubToken = Environment.GetEnvironmentVariable "GITHUB_TOKEN"

let githubTokenErrMsg =
    """Please add the 'GITHUB_TOKEN' environment variable to the GitHubCI file:
```
    env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```
"""

if String.IsNullOrEmpty githubToken then
    Console.Error.WriteLine githubTokenErrMsg

    Environment.Exit 2

let previousCommitHash =
    Fsdk
        .Process
        .Execute(
            {
                Command = "git"
                Arguments = "log --pretty=format:%H --max-count 2"
            },
            Echo.Off
        )
        .UnwrapDefault()
        .Trim()
        .Split(
        Environment.NewLine
    ).[1]

type GithubArtifactsType =
    JsonProvider<"""
{
  "total_count": 1,
  "artifacts": [
    {
      "id": 667505922,
      "node_id": "MDg6QXJ0aWZhY3Q2Njc1MDU5MjI=",
      "name": "publishedPackages",
      "size_in_bytes": 151303891,
      "url": "https://api.github.com/repos/aaarani/RunIntoMe/actions/artifacts/667505922",
      "archive_download_url": "https://api.github.com/repos/aaarani/RunIntoMe/actions/artifacts/667505922/zip",
      "expired": false,
      "created_at": "2023-04-26T22:57:37Z",
      "updated_at": "2023-04-26T22:57:38Z",
      "expires_at": "2023-07-25T22:47:52Z",
      "workflow_run": {
        "id": 4813959132,
        "repository_id": 632336429,
        "head_repository_id": 632336429,
        "head_branch": "wip/WelcomePage",
        "head_sha": "1357c79f43c12189c7aa4c46b5661400b704c211"
      }
    }
  ]
}
""">

type ApiAction =
    | Get
    | Delete

let GitHubApiQuery
    (url: string)
    (action: ApiAction)
    (acceptMediaTypeOpt: Option<string>)
    =
    let userAgent = ".NET App"
    let xGitHubApiVersion = "2022-11-28"

    use client = new HttpClient()
    client.DefaultRequestHeaders.Accept.Clear()

    match acceptMediaTypeOpt with
    | Some acceptMediaType ->
        client.DefaultRequestHeaders.Accept.Add(
            MediaTypeWithQualityHeaderValue acceptMediaType
        )
    | None -> ()

    client.DefaultRequestHeaders.Add("User-Agent", userAgent)
    client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", xGitHubApiVersion)

    if not(String.IsNullOrEmpty githubToken) then
        client.DefaultRequestHeaders.Add(
            "Authorization",
            $"Bearer {githubToken}"
        )

    Console.WriteLine(sprintf "Calling %s ..." url)

    try
        match action with
        | Get ->
            client.GetStringAsync url
            |> Async.AwaitTask
            |> Async.RunSynchronously
        | Delete ->
            (client.DeleteAsync url |> Async.AwaitTask |> Async.RunSynchronously)
                .ToString()
    with
    | ex ->
        match FindException<HttpRequestException> ex with
        | Some httpRequestException ->
            match httpRequestException.StatusCode |> Option.ofNullable with
            | Some statusCode when statusCode = HttpStatusCode.NotFound ->
                failwith githubTokenErrMsg
            | _ -> reraise()
        | _ -> reraise()

let artifactsApiQueryResult =
    let mediaTypeWithQuality = "application/vnd.github+json" |> Some

    let url =
        $"https://api.github.com/repos/{githubRepository}/actions/artifacts"

    GitHubApiQuery url Get mediaTypeWithQuality

let parsedJsonObj = GithubArtifactsType.Parse artifactsApiQueryResult

let artifactIds =
    parsedJsonObj.Artifacts
    |> Seq.filter(fun item -> item.WorkflowRun.HeadSha = previousCommitHash)
    |> Seq.map(fun artifact -> artifact.Id.ToString())

if Seq.isEmpty artifactIds then
    Environment.Exit 0

let DeleteArtifact (githubRepository: string) (artifactId: string) =
    let url =
        $"https://api.github.com/repos/{githubRepository}/actions/artifacts/{artifactId}"

    GitHubApiQuery url Delete None |> ignore

artifactIds
|> Seq.iter(fun artifactId -> DeleteArtifact githubRepository artifactId)
