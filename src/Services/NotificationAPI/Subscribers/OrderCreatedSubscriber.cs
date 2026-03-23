namespace NotificationAPI.Subscribers
{
    public class OrderCreatedSubscriber
    {
        private readonly string _projectId = "customerordermanagementsystem";
        private readonly string _subscriptionId = "order-created-subscription";

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            SubscriptionName subscriptionName = SubscriptionName.FromProjectSubscription(_projectId, _subscriptionId);
            SubscriberClient subscriber = await SubscriberClient.CreateAsync(subscriptionName);

            // Handle incoming messages
            await subscriber.StartAsync((PubsubMessage message, CancellationToken ct) =>
            {
                string data = message.Data.ToStringUtf8();
                Console.WriteLine($"Received OrderCreated event: {data}");

                // TODO: Deserialize JSON into OrderCreated model
                // var order = JsonSerializer.Deserialize<OrderCreated>(data);

                return Task.FromResult(SubscriberClient.Reply.Ack);
            });
        }
    }
}
