using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace day_03
{
    public static class PngImageCommitFunction
    {
        [FunctionName("PngImageCommitFunction")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log)
        {
            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            name = name ?? data?.name;

            //TODO: Deserialise request into Git webhook event object
            log.LogInformation("Deserialise request into Git webhook event object");

            //TODO: Extract PNG urls from the event object
            log.LogInformation("Extract PNG urls from the event object");

            //TODO: Persist down to azure table storage
            log.LogInformation("Persist down to azure table storage");

            return name != null
                ? (ActionResult)new OkObjectResult($"Hello, {name}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }
}
