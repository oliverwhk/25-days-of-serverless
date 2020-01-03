using System;
using System.Threading.Tasks;
using day_04.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace day_04
{
    public static class DishesFunction
    {
        [FunctionName("GetDishes")]
        public static async Task<IActionResult> GetDishes([HttpTrigger(AuthorizationLevel.Function, "get", Route = "dishes")] HttpRequest req, ILogger log)
        {
            var client = new MongoClient(System.Environment.GetEnvironmentVariable("MongoDbAtlasConnectionString"));
            var database = client.GetDatabase(System.Environment.GetEnvironmentVariable("MongoDbName"));
            var collection = database.GetCollection<Dish>("dishes");
            var dishes = await collection.Find(new BsonDocument()).ToListAsync();

            return new OkObjectResult(dishes);
        }
        
        [FunctionName("CreateDish")]
        public static async Task<IActionResult> CreateDishes([HttpTrigger(AuthorizationLevel.Function, "post", Route = "dishes")] HttpRequest req, ILogger log)
        {
            var client = new MongoClient(System.Environment.GetEnvironmentVariable("MongoDbAtlasConnectionString"));
            var database = client.GetDatabase(System.Environment.GetEnvironmentVariable("MongoDbName"));
            var collection = database.GetCollection<Dish>("dishes");

            var newDish = new Dish { Name = $"new dish {DateTime.Now}" };
            await collection.InsertOneAsync(newDish);

            return new OkObjectResult("");
        }
    }
}
