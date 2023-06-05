#!/usr/bin/env -S dotnet fsi

open System
open System.Linq
open System.IO

#r "nuget: Fsdk, 0.6.0--date20230602-0434.git-ad36c88"

open Fsdk
open Fsdk.Process

let rootDir = DirectoryInfo(Path.Combine(__SOURCE_DIRECTORY__, ".."))

let IsStable miniVersion =
    (int miniVersion % 2) = 0

let args = Misc.FsxOnlyArguments()

let suppliedVersion =
    if args.Length > 0 then
        if args.Length > 1 then
            Console.Error.WriteLine "Only one argument supported, not more"
            Environment.Exit 1
            failwith "Unreachable"
        else
            let full = (Version args.Head)

            if not(IsStable full.Build) then
                Console.Error.WriteLine
                    "Mini-version (previous-to-last number, e.g. 2 in 0.1.2.3) should be an even (stable) number"

                Environment.Exit 2
                failwith "Unreachable"

            if full.Revision = 0 then
                Console.Error.WriteLine
                    "Revision number (last number, e.g. 3 in 0.1.2.3) should not be zero (iOS restrictions...)"

                Environment.Exit 2
                failwith "Unreachable"

            Some full
    else
        None

let filesToBumpMiniVersion: seq<FileInfo> = [] :> seq<FileInfo>

let filesToBumpFullVersion: seq<FileInfo> =
    Seq.append
        filesToBumpMiniVersion
        [
            Path.Combine(rootDir.FullName, "Directory.Build.props") |> FileInfo
            Path.Combine(
                rootDir.FullName,
                "src/Frontend/Platforms/iOS/Info.plist"
            )
            |> FileInfo
            Path.Combine(
                rootDir.FullName,
                "src/Frontend/Platforms/Android/AndroidManifest.xml"
            )
            |> FileInfo
        ]

let filesToGitAdd: seq<FileInfo> = filesToBumpFullVersion

let Replace file fromStr toStr =
    Misc.ReplaceTextInFile file fromStr toStr

let Bump(toStable: bool) : Version * Version =
    let fullVersion = Misc.GetCurrentVersion rootDir
    let androidVersion = fullVersion.Build // 0.1.2.3 -> 2

    if toStable && IsStable androidVersion then
        failwith
            "bump script expects you to be in unstable version currently, but we found a stable"

    if (not toStable) && (not(IsStable androidVersion)) then
        failwith
            "sanity check failed, post-bump should happen in a stable version"

    let newFullVersion, newVersion =
        match suppliedVersion, toStable with
        | (Some full), true -> full, full.Build
        | _ ->
            let newVersion = androidVersion + 1

            let full =
                match fullVersion.Revision with
                | -1 ->
                    Version(
                        sprintf
                            "%i.%i.%i"
                            fullVersion.Major
                            fullVersion.Minor
                            newVersion
                    )
                | _ ->
                    Version(
                        sprintf
                            "%i.%i.%i.%i"
                            fullVersion.Major
                            fullVersion.Minor
                            newVersion
                            fullVersion.Revision
                    )

            full, newVersion

    for file in filesToBumpFullVersion do
        Replace file (fullVersion.ToString()) (newFullVersion.ToString())

    for file in filesToBumpFullVersion do
        Replace
            file
            (sprintf "versionCode=\"%s\"" (androidVersion.ToString()))
            (sprintf "versionCode=\"%s\"" (newVersion.ToString()))

    fullVersion, newFullVersion


let GitCommit (fullVersion: Version) (newFullVersion: Version) =
    for file in filesToGitAdd do
        let gitAdd =
            {
                Command = "git"
                Arguments = sprintf "add %s" file.FullName
            }

        Process.Execute(gitAdd, Echo.Off).UnwrapDefault() |> ignore<string>

    let commitMessage =
        sprintf
            "Bump version: %s -> %s"
            (fullVersion.ToString())
            (newFullVersion.ToString())

    let finalCommitMessage =
        if IsStable fullVersion.Build then
            sprintf "(Post)%s" commitMessage
        else
            commitMessage

    let gitCommit =
        {
            Command = "git"
            Arguments = sprintf "commit -m \"%s\"" finalCommitMessage
        }

    Process
        .Execute(gitCommit, Echo.Off)
        .UnwrapDefault()
    |> ignore<string>

let GitTag(newFullVersion: Version) =
    if not(IsStable newFullVersion.Build) then
        failwith
            "something is wrong, this script should tag only even(stable) mini-versions, not odd(unstable) ones"

    let gitDeleteTag =
        {
            Command = "git"
            Arguments = sprintf "tag --delete %s" (newFullVersion.ToString())
        }

    Process.Execute(gitDeleteTag, Echo.Off) |> ignore

    let gitCreateTag =
        {
            Command = "git"
            Arguments = sprintf "tag %s" (newFullVersion.ToString())
        }

    Process
        .Execute(gitCreateTag, Echo.Off)
        .UnwrapDefault()
    |> ignore<string>

let GitDiff() =

    let gitDiff =
        {
            Command = "git"
            Arguments = "diff"
        }

    let gitDiffProc = Process.Execute(gitDiff, Echo.Off)

    if gitDiffProc.UnwrapDefault().Length > 0 then
        Console.Error.WriteLine "git status is not clean"
        Environment.Exit 1

GitDiff()

Console.WriteLine "Bumping..."
let fullUnstableVersion, newFullStableVersion = Bump true
GitCommit fullUnstableVersion newFullStableVersion
GitTag newFullStableVersion

Console.WriteLine(
    sprintf "Version bumped to %s." (newFullStableVersion.ToString())
)

Console.WriteLine "Post-bumping..."
let fullStableVersion, newFullUnstableVersion = Bump false
GitCommit fullStableVersion newFullUnstableVersion

Console.WriteLine(
    sprintf
        "Version bumping finished. Remember to push via `git push origin master && git push origin %s`"
        (newFullStableVersion.ToString())
)
