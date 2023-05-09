namespace DG.Scripts

open System
open System.Linq

module Versioning =
    type VersionComparisonError =
        | VersionSequenceWasNotRespected
        | InvalidVersionNumber

    let VersionSanityCheck
        (prevVersionStr: string)
        (newVersionStr: string)
        : Result<unit, VersionComparisonError> =
        let lastVersionInPrevVersion = prevVersionStr.Split('.').Last()
        let lastVersionInNewVersion = newVersionStr.Split('.').Last()

        match Int32.TryParse lastVersionInNewVersion with
        | (false, _) -> Error VersionComparisonError.InvalidVersionNumber
        | (true, newVersion) ->
            match Int32.TryParse(lastVersionInPrevVersion) with
            | (false, _) -> Error VersionComparisonError.InvalidVersionNumber
            | (true, prevVersion) ->
                if ((prevVersion + newVersion) % 2 <> 0) then
                    Ok()
                else
                    Error VersionComparisonError.VersionSequenceWasNotRespected
