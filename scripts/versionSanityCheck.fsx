open System
open System.Text
open System.Diagnostics

#load "VersionSanityCheck.fs"
open DG.Scripts

type ProcessResult =
    {
        ExitCode: int
        StdOut: string
        StdErr: string
    }

// from: https://stackoverflow.com/questions/3065409/starting-a-process-synchronously-and-streaming-the-output
let ExecuteProcess(exe, args) =
    let psi =
        ProcessStartInfo(
            exe,
            args,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        )

    let createdProcess = Process.Start psi
    let output = StringBuilder()
    let error = StringBuilder()

    createdProcess.OutputDataReceived.Add(fun args ->
        output.Append(sprintf "%s\n" args.Data) |> ignore
    )

    createdProcess.ErrorDataReceived.Add(fun args ->
        error.Append(sprintf "%s\n" args.Data) |> ignore
    )

    createdProcess.BeginErrorReadLine()
    createdProcess.BeginOutputReadLine()
    createdProcess.WaitForExit()

    {
        ExitCode = createdProcess.ExitCode
        StdOut = output.ToString()
        StdErr = error.ToString()
    }

let GetTags() =
    // --sort=-creatordate will make newest tags at the top
    let tags =
        ExecuteProcess("git", "tag --sort=-creatordate")
            .StdOut
            .Split(
                [| "\r\n"; "\n" |],
                StringSplitOptions.RemoveEmptyEntries
            )

    tags

let allTagsSortedByCreateDate = GetTags()

if allTagsSortedByCreateDate.Length = 0 then
    failwith "Something went wrong, no tags found in the repository"

let latestTag = allTagsSortedByCreateDate.[0]

if allTagsSortedByCreateDate.Length = 1 then
    failwithf
        "Something went wrong, only 1 tag was found in the repository: %s"
        latestTag

let prevToLatestTag = allTagsSortedByCreateDate.[1]

let result = Versioning.VersionSanityCheck prevToLatestTag latestTag

match result with
| Ok _ -> printfn "Success! New git tag is valid."
| Error versionComparisonError ->
    match versionComparisonError with
    | Versioning.InvalidVersionNumber ->
        Console.Error.WriteLine(
            sprintf
                "A version number was used that is invalid (proper format is A.B.C where A, B and C are unsigned integers): %s or %s"
                latestTag
                prevToLatestTag
        )

        exit 2
    | Versioning.VersionSequenceWasNotRespected ->
        Console.Error.WriteLine(
            sprintf
                "A version number was used (%s) that doesn't follow the sequence from the previous tag number (%s); it should respect at least odd/even sequence of last subversion number"
                latestTag
                prevToLatestTag
        )

        exit 1
