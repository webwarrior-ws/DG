module Backend.Tests

open NUnit.Framework

open DataModel

// let's not remove this placeholder below, as we might need it at some point:
[<SetUp>]
let Setup() =
    ()

let serializedLocationUpdateReq =
    $"{{\"Version\":\"{VersionHelper.CURRENT_VERSION}\",\"TypeName\":\"DataModel.UpdateGpsLocationRequest\",\"Value\":"
    + "{\"UserId\":1,\"Latitude\":6,\"Longitude\":9}}"

let updateGpsLocationRequest =
    {
        UserId = 1
        Latitude = 6.
        Longitude = 9.
    }

[<Test>]
let SerializationWorks() =
    let json = DataModel.Marshaller.Serialize updateGpsLocationRequest
    Assert.That(json, Is.EqualTo serializedLocationUpdateReq)

[<Test>]
let NormalDeserializationWorks() =
    let deserializedUpdateGpsLocationReq =
        DataModel.Marshaller.Deserialize<UpdateGpsLocationRequest>
            serializedLocationUpdateReq

    Assert.IsFalse(
        System.Object.ReferenceEquals(deserializedUpdateGpsLocationReq, null)
    )

    Assert.That(
        deserializedUpdateGpsLocationReq.UserId,
        Is.EqualTo updateGpsLocationRequest.UserId
    )

    Assert.That(
        deserializedUpdateGpsLocationReq.Latitude,
        Is.EqualTo updateGpsLocationRequest.Latitude
    )

    Assert.That(
        deserializedUpdateGpsLocationReq.Longitude,
        Is.EqualTo updateGpsLocationRequest.Longitude
    )
