using Carter;
using MediatR;
using Microsoft.AspNetCore.Routing;

namespace OrderingAPI
{
    public class CreateOrderModule : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/orders", async (CreateOrderRequest request, ISender sender) =>
            {
                var command = new CreateOrderCommand(
                request.OrderId,
                request.CustomerId,
                request.OrderDate,
                request.Status,
                request.TotalAmount
            );

                var result = await sender.Send(command);

                var response = new CreateOrderResponse(result.Id, result.Status);

                return Results.Created($"/orders/{response.Id}", response);
            })
            .WithName("CreateOrder")
            .Produces<CreateOrderResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Create Order")
            .WithDescription("Create a new order with items");
        }
    }
}
