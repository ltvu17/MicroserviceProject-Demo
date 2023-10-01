using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Play.Common.Settings;

namespace Play.Common.MongoDB
{
    public static class Extentions
    {
        public static WebApplicationBuilder AddMongo(this WebApplicationBuilder builder){
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
            BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

            builder.Services.AddSingleton(serviceProvider=>
            {
                var mongoClient = new MongoClient(
                    builder.Configuration.GetSection(nameof(MongoDBSettings)).Get<MongoDBSettings>().ConnectionString
                );
                    return mongoClient.GetDatabase(
                    builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>().ServiceName
                    );
            }); 
            return builder;
        }
        public static WebApplicationBuilder AddRepositoryService<T>(this WebApplicationBuilder builder, string connectionName) 
            where T : IEntity
        {
            builder.Services.AddSingleton<IRepoBase<T>>(service=>{
                var database = service.GetService<IMongoDatabase>();
                return new RepoBase<T>(database, connectionName);
            }); 
            return builder;
        }
    }
    
}