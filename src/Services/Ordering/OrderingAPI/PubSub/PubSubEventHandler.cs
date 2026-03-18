using System.Text.Json;
using BuildingBlocks.CQRS;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Microsoft.Extensions.Options;

namespace Ordering.API.PubSub
{
    public class PubSubEventHandler : IEventPublisher
    {
        private readonly PubSubOptions _options;

        public PubSubEventHandler(IOptions<PubSubOptions> options)
        {
            _options = options.Value;
        }

        public async Task PublishAsync<T>(T message, string topicName, CancellationToken cancellationToken)
        {
            var publisher = await PublisherClient.CreateAsync(
                TopicName.FromProjectTopic(_options.ProjectId, topicName));

            string json = JsonSerializer.Serialize(message);
            var pubsubMessage = new PubsubMessage
            {
                Data = ByteString.CopyFromUtf8(json)
            };

            await publisher.PublishAsync(pubsubMessage);
        }
    }
}
