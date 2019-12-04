using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace serverless.day01
{
    public static class DreidelHttpTrigger
    {
        [FunctionName("Spin-Dreidel")]
        public static async Task<IActionResult> SpinDreidel(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "dreidel/spin")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Spin Dreidel.");

            var values = Enum.GetValues(typeof(DreidelSymbol));
            var random = new Random();
            var face = (DreidelSymbol)values.GetValue(random.Next(values.Length));

            return (ActionResult)new OkObjectResult($"Spin a dreidel and the result is {face.ToString()}.");
        }
    }
}
