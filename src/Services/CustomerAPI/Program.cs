using CustomerAPI.Data;
using CustomerAPI.Repositories;
using CustomerAPI.PubSub;
var builder = WebApplication.CreateBuilder(args);
// Add Controllers
builder.Services.AddControllers();
// Swagger (optional but useful)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

// Register Spanner DB Context
builder.Services.AddSingleton<SpannerDbContext>();
// Register Repository
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
// Register PubSub Publisher
builder.Services.AddSingleton<PubSubPublisher>();

var app = builder.Build();

// Enable Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();