namespace DataModel

open System
open System.Collections.Generic

type RegisterNewAppInstallRequest() =
    class
    end

type GpsLocation =
    {
        Latitude: double
        Longitude: double
    }

type NonEvent =
    {
        DateTimeUtc: DateTime
        DateTime: DateTime
        GpsLocation: GpsLocation
    }

type Race =
    | Asian = 1
    | Black = 2
    | Brown = 3 // "brown" asian
    | Latin = 4 // "brown" american
    | Caucasian = 5

type EventInfo =
    {
        // same as NonEvent 1st (let's not use inheritance, as non idiomatic F#)
        DateTimeUtc: DateTime
        DateTime: DateTime
        GpsLocation: GpsLocation

        // person details
        Race: Race
        Rate: decimal
        Age: int
        AgeIsExact: bool

        // interaction details
        Notes: string
    }

type UpdateGpsLocationRequest =
    {
        UserId: int
        Latitude: double
        Longitude: double
    }

type AddFriendRequest =
    {
        UserId: int
        FriendId: int
    }

type AddFriendStatusCode =
    | FriendedSuccess = 1
    | FriendedCompleted = 2
    | AlreadyDone = 3

type AddFriendResponse =
    {
        StatusCode: AddFriendStatusCode
    }

type GetFriendsRequest =
    {
        UserId: int
    }

type UpdateClosenessRequest =
    {
        UserId: int
        FriendId: int
        NewCloseness: int
    }

type GetFriendsResponse =
    {
        Friends: Dictionary<int, int>
    }
