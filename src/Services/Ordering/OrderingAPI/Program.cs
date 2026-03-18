using System.Diagnostics;
using System.Linq;
using Google.Cloud.Spanner.Data;
using Ordering.API;
using Ordering.Application;
using OrderingInfrastructure;

var builder = WebApplication.CreateBuilder(args);

// ✅ Register required services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

builder.Services.AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration)
    .AddAPIServices();
    
app.MapGet("/", () => "Hello World test again!");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ordering API V1");
    });
}

// Spanner test endpoint (unchanged)
app.MapGet("/spanner-test", async () =>
{
    string projectId = "mymainproject-490008";
    string instanceId = "db-2";
    string databaseId = "spannerdb-1";
    string connectionString =
        $"Data Source=projects/{projectId}/instances/{instanceId}/"
        + $"databases/{databaseId}";

    try
    {
         var connection = new SpannerConnection(connectionString);
        //var cmd = connection.CreateSelectCommand(@"SELECT ""Hello World"" as test");

        //using var reader = await cmd.ExecuteReaderAsync();
        //if (await reader.ReadAsync())
        //{
        //    var value = reader.GetFieldValue<string>("test");
        //    return Results.Ok(new { result = value });
        //}

        return Results.NotFound();
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, title: "Spanner query failed");
    }
});

// Start the host, then open the browser automatically to Swagger (local dev convenience).
await app.StartAsync();

var launchUrl = app.Urls.FirstOrDefault() ?? builder.Configuration["applicationUrl"] ?? "http://localhost:5003";
var openUrl = $"{launchUrl.TrimEnd('/')}/swagger";

try
{
    Process.Start(new ProcessStartInfo { FileName = openUrl, UseShellExecute = true });
}
catch
{
    // ignore failures to open browser
}

await app.WaitForShutdownAsync();