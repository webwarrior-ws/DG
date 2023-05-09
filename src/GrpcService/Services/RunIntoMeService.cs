using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Core;

using Grpc.Core;

using DataModel;
using GrpcService.Utils;

namespace GrpcService.Services
{
    public class RunIntoMeService : RunIntoMeGrpcService.RunIntoMeGrpcServiceBase
    {
        // Capacity of 5 is set here as a limit on how many unsent
        // notifications can the channel hold before it starts dropping
        // old ones. I don't think this ever happens but it's better
        // than having an unbounded channel.
        private const int notificationChannelCapacity = 5;


        private readonly NotificationProvider notificationProvider;
        private readonly PushNotificationProvider pushNotificationProvider;
        private readonly ILogger<RunIntoMeService> logger;
        private readonly BackendDataLayer.Access access;

        private const int MaximumDistanceForSendingNotificationsInMeters = 100;

        public RunIntoMeService(NotificationProvider notificationProvider, PushNotificationProvider pushNotificationProvider, ILogger<RunIntoMeService> logger, IConfiguration configuration)
        {
            this.notificationProvider = notificationProvider;
            this.pushNotificationProvider = pushNotificationProvider;
            this.logger = logger;
            access = new BackendDataLayer.Access(configuration.GetConnectionString("MainDB"));
        }

        public override async Task<GenericOutputParam> GenericMethod(GenericInputParam request, ServerCallContext context)
        {
            Console.WriteLine("__got a new request");
            var type = Marshaller.ExtractType(request.MsgIn);

            Console.WriteLine("__type: " + type.FullName);
            var deserializedRequest = Marshaller.DeserializeAbstract(request.MsgIn, type);

            if (deserializedRequest is RegisterNewAppInstallRequest _)
            {
                Console.WriteLine("__got a new RegisterNewAppInstall request");
                var newUserId = access.CreateNewUserId();
                try
                {
                    return new GenericOutputParam
                    {
                        MsgOut = newUserId.ToString()
                    };
                }
                finally
                {
                    Console.WriteLine("__served a new RegisterNewAppInstall request");
                }
            }
            else if (deserializedRequest is UpdateGpsLocationRequest gpsLocationDetails)
            {
                Console.WriteLine("__got a new UpdatedGpsLocation request");
                access.UpdateGpsLocation(gpsLocationDetails);

                var friends = access.GetFriendsLocation(gpsLocationDetails.UserId);
                foreach (var (friendId, friendLatitude, friendLongitude) in friends)
                {
                    if (FSharpOption<double>.get_IsNone(friendLongitude) || FSharpOption<double>.get_IsNone(friendLatitude))
                        continue;

                    var distance = GpsUtil.GetDistanceInMeters(friendLongitude.Value, friendLatitude.Value, gpsLocationDetails.Longitude, gpsLocationDetails.Latitude);
                    if (distance < MaximumDistanceForSendingNotificationsInMeters)
                    {
                        async Task sendPushNotification(int userId, int friendId)
                        {
                            var pushNotificationTitle = "Alert!";
                            string pushNotificationMessage(int friendId)
                            {
                                return $"Friend #{friendId} is close to you!";
                            }

                            await pushNotificationProvider.SendTextPushNotification(userId, pushNotificationTitle, pushNotificationMessage(friendId));
                        }

                        await sendPushNotification(friendId, gpsLocationDetails.UserId);
                        await sendPushNotification(gpsLocationDetails.UserId, friendId);
                    }
                }

                try
                {
                    return new GenericOutputParam { MsgOut = String.Empty };
                }
                finally
                {
                    Console.WriteLine("__served a new UpdatedGpsLocation request");
                }
            }
            else if (deserializedRequest is AddFriendRequest addFriendRequest)
            {
                Console.WriteLine("__got a new AddFriend request");
                var statusCode = access.CreateRelationship(addFriendRequest.UserId, addFriendRequest.FriendId);

                // Only notify the friend if they were not our friend before
                if (statusCode != AddFriendStatusCode.AlreadyDone)
                    await notificationProvider.SendNotification(addFriendRequest.FriendId, NotificationIdentifiers.AddFriendSuccessNotificationId);

                try
                {
                    return new GenericOutputParam
                    {
                        MsgOut = Marshaller.Serialize(new AddFriendResponse(statusCode))
                    };
                }
                finally
                {
                    Console.WriteLine("__served a new AddFriend request");
                }
            }
            else if (deserializedRequest is GetFriendsRequest getFriendsRequest)
            {
                Console.WriteLine("__got a new GetFriends request");
                GetFriendsResponse response =
                    new(access.GetFriends(getFriendsRequest.UserId));
                try
                {
                    return new GenericOutputParam
                    {
                        MsgOut = Marshaller.Serialize(response)
                    };
                }
                finally
                {
                    Console.WriteLine("__served a new GetFriendsGetFriends request");
                }
            }
            else if (deserializedRequest is UpdateClosenessRequest updateClosenessRequest)
            {
                Console.WriteLine("__got a new UpdateCloseness request");
                access.UpdateCloseness(updateClosenessRequest);
                try
                {
                    return new GenericOutputParam { MsgOut = String.Empty };
                }
                finally
                {
                    Console.WriteLine("__served a new UpdateCloseness request");
                }
            }
            else
            {
                throw new InvalidOperationException("Unable to deserialize request: " + request.MsgIn);
            }

        }

        public override async Task GetNotifications(GetNotificationRequest request, IServerStreamWriter<Notification> responseStream, ServerCallContext context)
        {
            var channel = Channel.CreateBounded<Notification>(
                new BoundedChannelOptions(capacity: notificationChannelCapacity)
                {
                    FullMode = BoundedChannelFullMode.DropOldest
                }
            );

            try
            {
                notificationProvider.AddChannel(request.UserId, channel.Writer);

                await foreach (var message in channel.Reader.ReadAllAsync(context.CancellationToken))
                {
                    await responseStream.WriteAsync(message);
                }
            }
            finally
            {
                notificationProvider.RemoveChannel(request.UserId, channel.Writer);
            }
        }
    }
}
