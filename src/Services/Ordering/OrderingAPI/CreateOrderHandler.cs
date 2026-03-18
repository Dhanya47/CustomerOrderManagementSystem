using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Google.Cloud.Spanner.Data;
using MediatR;
using Microsoft.Extensions.Options;
using Ordering.API;

namespace OrderingAPI
{
    public class CreateOrderHandler : ICommandHandler<CreateOrderCommand, CreateOrderResult>
    {
        private readonly string _connectionString;
        private readonly IEventPublisher _publisher;
        private readonly PubSubOptions _options;

        public CreateOrderHandler(IConfiguration config,IEventPublisher publisher, IOptions<PubSubOptions> options)
        {
            // You can store projectId, instanceId, databaseId in appsettings.json
            string projectId = "sudheerproject-489306";
            string instanceId = "testinstance";
            string databaseId = "my-test-db";

            _connectionString = $"Data Source=projects/{projectId}/instances/{instanceId}/databases/{databaseId}";
            _publisher = publisher;
            _options = options.Value;
        }

        public async Task<CreateOrderResult> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
        {
            try
            {
                using var connection = new SpannerConnection(_connectionString);
                await connection.OpenAsync();

                using var transaction = await connection.BeginTransactionAsync();

                var orderCmd = connection.CreateInsertCommand("Orders",
                    new SpannerParameterCollection
                    {
                { "OrderId", SpannerDbType.Int64, command.OrderId },
                { "CustomerId", SpannerDbType.Int64, command.CustomerId },
                { "OrderDate", SpannerDbType.Date, command.OrderDate },
                { "Status", SpannerDbType.String, command.Status },
                { "TotalAmount", SpannerDbType.Float64, command.TotalAmount }
                    });
                orderCmd.Transaction = transaction;
                await orderCmd.ExecuteNonQueryAsync();

                //foreach (var item in command.Items)
                //{
                //    var itemCmd = connection.CreateInsertCommand("OrderItems",
                //        new SpannerParameterCollection
                //        {
                //        { "OrderItemId", SpannerDbType.Int64, item.OrderItemId },
                //        { "OrderId", SpannerDbType.Int64, command.OrderId },
                //        { "ProductName", SpannerDbType.String, item.ProductName },
                //        { "Quantity", SpannerDbType.Int64, item.Quantity },
                //        { "UnitPrice", SpannerDbType.Float64, item.UnitPrice }
                //        });
                //    itemCmd.Transaction = transaction;
                //    await itemCmd.ExecuteNonQueryAsync();
                //}

                await transaction.CommitAsync();
                

                var result =  new CreateOrderResult(command.OrderId, command.Status);
                await _publisher.PublishAsync(result, _options.Topics.OrderCreated, cancellationToken);
                return result;
            }
            catch (SpannerException ex)
            {
                // Database‑specific error
                throw new DomainException($"Spanner insert failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                // General error
                throw new DomainException($"Unexpected error: {ex.Message}");
            }

        }
    }
}
