using MediatR;
namespace OrderingAPI
{
    public record CreateOrderCommand(
        long OrderId,
        long CustomerId,
        DateTime OrderDate,
        string Status,
        double TotalAmount) : IRequest<CreateOrderResult>;
    //List<CreateOrderItemRequest> Items) : IRequest<CreateOrderResult>;

    public record CreateOrderResult(long Id, string Status);
}
