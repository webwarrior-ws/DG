
using System;
using System.Threading.Tasks;

using Grpc.Net.Client;
using GrpcService;
using DataModel;

namespace GrpcClient
{
    public class Instance
    {
        public RunIntoMeGrpcService.RunIntoMeGrpcServiceClient Connect()
        {
            var serverFqdn =
#if DEBUG
                "localhost"
#else
                "grpcserver.runinto.me"
#endif
                ;
            var channel = GrpcChannel.ForAddress($"http://{serverFqdn}:8080");
            var client = new RunIntoMeGrpcService.RunIntoMeGrpcServiceClient(channel);
            return client;
        }

        public async Task<int> RegisterNewAppInstall()
        {
            var client = Connect();
            var reply = await client.GenericMethodAsync(
                new GenericInputParam { MsgIn = Marshaller.Serialize(new RegisterNewAppInstallRequest()) }
            );
            int newUserId;
            if (!int.TryParse(reply.MsgOut, out newUserId) || newUserId < 1)
            {
                throw new InvalidOperationException("Unexpected newUserId type or value: " + reply.MsgOut);
            }
            return newUserId;
        }

        public async Task UpdateGpsLocation(UpdateGpsLocationRequest gpsLocationUpdateDetails)
        {
            var client = Connect();
            await client.GenericMethodAsync(
                new GenericInputParam { MsgIn = Marshaller.Serialize(gpsLocationUpdateDetails) }
            );
        }

        public async Task<AddFriendResponse> AddFriend(AddFriendRequest addFriendRequest)
        {
            var client = Connect();
            var addFriendResponseMsg =
                await client.GenericMethodAsync(
                    new GenericInputParam { MsgIn = Marshaller.Serialize(addFriendRequest) }
                );

            var addFriendResponse =
                Marshaller.Deserialize<AddFriendResponse>(addFriendResponseMsg.MsgOut);

            return addFriendResponse;
        }

        public async Task<GetFriendsResponse> GetFriends(GetFriendsRequest getFriendsRequest)
        {
            var client = Connect();
            var getFriendsResponseMsg =
                await client.GenericMethodAsync(
                    new GenericInputParam { MsgIn = Marshaller.Serialize(getFriendsRequest) }
                );

            var getFriendsResponse = Marshaller.Deserialize<GetFriendsResponse>(getFriendsResponseMsg.MsgOut);
            return getFriendsResponse;
        }
        public async Task UpdateCloseness(UpdateClosenessRequest updateClosenessRequest)
        {
            var client = Connect();
            await client.GenericMethodAsync(
                new GenericInputParam { MsgIn = Marshaller.Serialize(updateClosenessRequest) }
            );
        }
    }
}
