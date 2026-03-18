using BuildingBlocks.CQRS;
using MediatR;
namespace OrderingAPI
{
    public record CreateOrderCommand(
        long OrderId,
        long CustomerId,
        DateTime OrderDate,
        string Status,
        double TotalAmount) : ICommand<CreateOrderResult>;
    //List<CreateOrderItemRequest> Items) : IRequest<CreateOrderResult>;

    public record CreateOrderResult(long Id, string Status);
}
