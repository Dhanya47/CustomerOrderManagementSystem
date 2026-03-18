namespace Ordering.API
{
    public class PubSubOptions
    {
        public string ProjectId { get; set; } = string.Empty;
        public TopicsOptions Topics { get; set; } = new();

        public class TopicsOptions
        {
            public string OrderCreated { get; set; } = string.Empty;
        }
    }
}
