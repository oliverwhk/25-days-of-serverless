using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using day_03.Model;
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
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
        {
            // Validate the GitHub Event Type
            if (!req.Headers.ContainsKey("X-GitHub-Event") || req.Headers["X-GitHub-Event"] != "push")
            {
                return new BadRequestObjectResult("Incorrect webhook trigger");
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var gitCommitEvents = JsonConvert.DeserializeObject<GitCommitEvent>(requestBody);

            foreach (var commit in gitCommitEvents.Commits)
            {
                foreach (var added in commit.Added)
                {
                    log.LogInformation($"Added file path: {gitCommitEvents.Repository.HtmlUrl}/{added}");
                    // if (added.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    // {
                    //     
                    // }
                }
            }

            //TODO: Extract PNG urls from the event object
            log.LogInformation("TODO: Extract PNG urls from the event object");

            //TODO: Persist down to azure table storage
            log.LogInformation("TODO: Persist down to azure table storage");

            return new OkObjectResult("");
        }
    }
}
