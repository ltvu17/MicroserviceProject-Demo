
using Play.Common.MassTransit;
using Play.Common.MongoDB;
using Play.Inventory.Service;
using Play.Inventory.Service.Entities;
using Polly;
using Polly.Timeout;

var builder = WebApplication.CreateBuilder(args);
Random jitterer = new Random();

// Add services to the container.
builder.AddMongo()
        .AddRepositoryService<InventoryItem>("inventoryitems")
        .AddRepositoryService<CatalogItem>("CatalogItems")
        .AddMassTransitWithRabbitMQ();

AddCatalogClient(builder, jitterer);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

static void AddCatalogClient(WebApplicationBuilder builder, Random jitterer)
{
    builder.Services.AddHttpClient<CatalogClient>(client =>
    {
        client.BaseAddress = new Uri("https://localhost:7122");
    })
    .AddTransientHttpErrorPolicy(option => option.Or<TimeoutRejectedException>().WaitAndRetryAsync(5,
        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)),
        onRetry: (outcome, timespan, retryAttempt) =>
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            serviceProvider.GetService<ILogger<CatalogClient>>()?
            .LogWarning($"Delaying for {timespan.TotalSeconds} seconds, then making retry {retryAttempt}");

        }
    ))
    .AddTransientHttpErrorPolicy(option => option.Or<TimeoutRejectedException>().CircuitBreakerAsync(
        3,
        TimeSpan.FromSeconds(15),
        onBreak: (outcome, timespan) =>
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            serviceProvider.GetService<ILogger<CatalogClient>>()?
            .LogWarning($"Opening the circuit for {timespan.TotalSeconds}");
        },
        onReset: () =>
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            serviceProvider.GetService<ILogger<CatalogClient>>()?
            .LogWarning($"Close the circuit");
        }
    ))
    .AddPolicyHandler(policy: Policy.TimeoutAsync<HttpResponseMessage>(1));
}