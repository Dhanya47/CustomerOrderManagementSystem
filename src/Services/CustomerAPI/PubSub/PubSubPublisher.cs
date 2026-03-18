using Google.Cloud.PubSub.V1;
using System.Text;
namespace CustomerAPI.PubSub
{
    public class PubSubPublisher
    {
        private readonly string _projectId = "customer-project-490417";
        private readonly string _topicId = "customer-events";
        public async Task PublishMessage(string message)
        {
            TopicName topicName = TopicName.FromProjectTopic(_projectId, _topicId);
            PublisherClient publisher = await PublisherClient.CreateAsync(topicName);
            await publisher.PublishAsync(message);
        }
    }
}