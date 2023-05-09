module DG.Tests.Unit

open System
open System.Linq
open NUnit.Framework

open DG.Scripts

[<SetUp>]
let Setup() =
    ()

[<Test>]
let BasicFailTest() =
    let prevVersion = "0.1"
    let newVersion = "thisIsNotAVersion!"

    Assert.That(
        Versioning.VersionSanityCheck prevVersion newVersion,
        Is.EqualTo(
            Error Versioning.VersionComparisonError.InvalidVersionNumber
            :> Result<unit, Versioning.VersionComparisonError>
        )
    )

[<Test>]
let BasicFailTest2() =
    let prevVersion = "thisIsNotAVersion!"
    let newVersion = "0.1"

    Assert.That(
        Versioning.VersionSanityCheck prevVersion newVersion,
        Is.EqualTo(
            Error Versioning.VersionComparisonError.InvalidVersionNumber
            :> Result<unit, Versioning.VersionComparisonError>
        )
    )

[<Test>]
let BasicFailTestWithOddVersionNumbers() =
    let prevVersion = "0.1"
    let newVersion = "0.3"

    Assert.That(
        Versioning.VersionSanityCheck prevVersion newVersion,
        Is.EqualTo(
            Error
                Versioning.VersionComparisonError.VersionSequenceWasNotRespected
            :> Result<unit, Versioning.VersionComparisonError>
        )
    )

[<Test>]
let BasicFailTestWithEvenVersionNumbers() =
    let prevVersion = "0.2"
    let newVersion = "0.4"

    Assert.That(
        Versioning.VersionSanityCheck prevVersion newVersion,
        Is.EqualTo(
            Error
                Versioning.VersionComparisonError.VersionSequenceWasNotRespected
            :> Result<unit, Versioning.VersionComparisonError>
        )
    )

[<Test>]
let BasicFailTestWithCorrectVersionNumbers() =
    let prevVersion = "0.1"
    let newVersion = "0.2"

    Assert.That(
        Versioning.VersionSanityCheck prevVersion newVersion,
        Is.EqualTo(Ok() :> Result<unit, Versioning.VersionComparisonError>)
    )

[<Test>]
let BasicFailTestWithCorrectVersionNumbers2() =
    let prevVersion = "0.3"
    let newVersion = "1.0"

    Assert.That(
        Versioning.VersionSanityCheck prevVersion newVersion,
        Is.EqualTo(Ok() :> Result<unit, Versioning.VersionComparisonError>)
    )
