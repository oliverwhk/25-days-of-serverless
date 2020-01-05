using System.Threading.Tasks;
using day_04.Model;
using Kevsoft.Azure.WebJobs;
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
        public static async Task<IActionResult> GetDishes(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "dishes")] HttpRequest req, 
            ILogger log,
            [MongoDb("day-04", "dishes", ConnectionStringSetting = "MongoDbUrl")] IMongoCollection<Dish> dishCollection)
        {
            var dishes = await dishCollection.Find(new BsonDocument()).ToListAsync();
            return new OkObjectResult(dishes);
        }
        
        [FunctionName("GetDishById")]
        public static async Task<IActionResult> GetDishById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "dishes/{id}")] HttpRequest req, 
            ILogger log,
            [MongoDb("day-04", "dishes", "{id}", ConnectionStringSetting = "MongoDbUrl")] Dish dish)
        {
            return new OkObjectResult(dish);
        }
        
        [FunctionName("CreateDish")]
        public static async Task<IActionResult> CreateDishes(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "dishes")] Dish newDish, 
            ILogger log,
            [MongoDb("day-04", "dishes", ConnectionStringSetting = "MongoDbUrl")] IAsyncCollector<Dish> dishCollection)
        {
            await dishCollection.AddAsync(newDish);

            return new OkObjectResult(newDish);
        }
    }
}
