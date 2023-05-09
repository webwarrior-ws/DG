namespace BackendDataLayer

open System.Linq

open FSharp.Data.Sql

open DataModel

type MutableDictionary<'T, 'G> = System.Collections.Generic.Dictionary<'T, 'G>

type SQL =
    SqlDataProvider<Common.DatabaseProviderTypes.POSTGRESQL, Constants.DevelopmentConnStr, Owner="public, admin, references", UseOptionTypes=true, ResolutionPath=Constants.ResolutionPath>

type Access(runtimeConnectionString: string) =

    let GetDataContext() =
        typeof<Npgsql.NpgsqlConnection>.Assembly |> ignore
        SQL.GetDataContext runtimeConnectionString

    member _.UpdateGpsLocation(gpsLocationDetails: UpdateGpsLocationRequest) =
        let ctx = GetDataContext()

        let maybeFoundUser =
            query {
                for user in ctx.Public.Users do
                    where(user.UserId = gpsLocationDetails.UserId)
                    select(Some user)
                    exactlyOneOrDefault
            }

        match maybeFoundUser with
        | Some user ->
            user.GpsLatitude <- Some gpsLocationDetails.Latitude
            user.GpsLongitude <- Some gpsLocationDetails.Longitude
            ctx.SubmitUpdates()
        | None -> failwithf "User %i not found" gpsLocationDetails.UserId

    member _.GetFriendsLocation(userId: int) =
        let ctx = GetDataContext()

        let friendIds =
            query {
                for relationship in ctx.Public.Relationships do
                    where(relationship.UserId = userId)
                    select(relationship.AssigneeId)
            }

        let users =
            query {
                for user in ctx.Public.Users do
                    where(friendIds.Contains(user.UserId))
                    select(user.UserId, user.GpsLatitude, user.GpsLongitude)
            }

        users |> Seq.toArray

    member _.CreateNewUserId() =
        let ctx = GetDataContext()
        let newUser = ctx.Public.Users.Create()
        ctx.SubmitUpdates()
        newUser.UserId

    member _.CreateRelationship (userId: int) (friendId: int) =
        let ctx = GetDataContext()

        let createRelationshipObj(userId: int, friendId: int) =
            let relationshipObj = ctx.Public.Relationships.Create()
            relationshipObj.UserId <- userId
            relationshipObj.AssigneeId <- friendId
            relationshipObj.Closeness <- 0

        // Returns true if exception indicates uniqueness problem
        let isDuplicatePrimaryKeyError(ex: Npgsql.PostgresException) =
            let uniqueViolationErrorCode = "23505"

            ex.ConstraintName = "Relationships_pkey"
            && ex.SqlState = uniqueViolationErrorCode

        try
            createRelationshipObj(userId, friendId)
            createRelationshipObj(friendId, userId)

            ctx.SubmitUpdates()

            AddFriendStatusCode.FriendedSuccess
        with
        | :? Npgsql.PostgresException as ex when isDuplicatePrimaryKeyError ex ->
            try
                ctx.ClearUpdates() |> ignore<List<Common.SqlEntity>>
                createRelationshipObj(userId, friendId)
                ctx.SubmitUpdates()
                AddFriendStatusCode.FriendedCompleted
            with
            | :? Npgsql.PostgresException as ex when
                isDuplicatePrimaryKeyError ex
                ->
                AddFriendStatusCode.AlreadyDone


    member __.GetFriends(userId: int) =
        let ctx = GetDataContext()

        query {
            for relationship in ctx.Public.Relationships do
                where(relationship.UserId = userId)
                select(relationship.AssigneeId, relationship.Closeness)
        }
        |> dict
        |> MutableDictionary<int, int>

    member _.UpdateCloseness(updateClosenessDetails: UpdateClosenessRequest) =
        let ctx = GetDataContext()

        let maybeFoundRelationship =
            query {
                for relationship in ctx.Public.Relationships do
                    where(
                        relationship.UserId = updateClosenessDetails.UserId
                        && relationship.AssigneeId = updateClosenessDetails.FriendId
                    )

                    select(Some relationship)
                    exactlyOneOrDefault
            }

        match maybeFoundRelationship with
        | Some relationship ->
            relationship.Closeness <- updateClosenessDetails.NewCloseness
            ctx.SubmitUpdates()
        | None ->
            failwithf
                "Relationship %i, %i not found"
                updateClosenessDetails.UserId
                updateClosenessDetails.FriendId

    member _.TestRelationships() : option<_> =
        let ctx = GetDataContext()

        let firstRelRow =
            query {
                for e in ctx.Public.Relationships do
                    select(e.UserId, e.AssigneeId)
            }
            |> Seq.tryHead

        firstRelRow
