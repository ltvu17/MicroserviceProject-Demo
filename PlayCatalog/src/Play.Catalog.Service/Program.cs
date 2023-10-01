
using MassTransit;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Play.Common.MongoDB;
using Play.Common.MassTransit;
using Play.Common.Settings;

namespace Play.Catalog.Service;

public class Program
{   
    private ServiceSettings serviceSettings;
    public static void Main(string[] args)
    {   
        
        var builder = WebApplication.CreateBuilder(args);
        // Add services to the container.
        builder.AddMongo()
                .AddRepositoryService<Item>("items")
                .AddMassTransitWithRabbitMQ();
    
        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddControllers(option=>{
            option.SuppressAsyncSuffixInActionNames = false;
        });
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
    }
}
