namespace Ordering.API.DTOs
{
    public record CreateOrderRequest(
    long OrderId,
    long CustomerId,
    DateTime OrderDate,
    string Status,
    double TotalAmount);
    //List<CreateOrderItemRequest> Items);

    public record CreateOrderItemRequest(
        long OrderItemId,
        string ProductName,
        int Quantity,
        double UnitPrice);

    public record CreateOrderResponse(long Id, string Status);
}
