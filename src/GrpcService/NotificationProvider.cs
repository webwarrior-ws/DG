using System.Collections.Concurrent;
using System.Threading.Channels;

namespace GrpcService.Services;

public class NotificationProvider
{
    ConcurrentDictionary<int, ChannelWriter<Notification>> channels = new ConcurrentDictionary<int, ChannelWriter<Notification>>();

    public void AddChannel(int userId, ChannelWriter<Notification> channelWriter)
    {
        channels.AddOrUpdate(userId, channelWriter, (_, _) => channelWriter);
    }

    public void RemoveChannel(int userId, ChannelWriter<Notification> channelWriter)
    {
        channels.TryRemove(new KeyValuePair<int, ChannelWriter<Notification>>(userId, channelWriter));
    }

    public async Task SendNotification(int destUser, string notificationTypeId)
    {
        if (channels.TryGetValue(destUser, out var notificationChannel))
        {
            await notificationChannel.WriteAsync(new Notification
            {
                TypeId = notificationTypeId
            });
        }
    }
}