using System;
using System.Collections.Generic;
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
                log.LogInformation("Skipped because request is not a GitHub push event.");
                return new BadRequestObjectResult("Incorrect webhook trigger");
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var gitCommitEvents = JsonConvert.DeserializeObject<GitCommitEvent>(requestBody);

            var pngImageFilePaths = GetPngImageFilePaths(log, gitCommitEvents);
            PersistPngImageFilePaths(log, pngImageFilePaths);

            return new OkObjectResult("");
        }

        private static IEnumerable<string> GetPngImageFilePaths(ILogger log, GitCommitEvent gitCommitEvents)
        {
            foreach (var commit in gitCommitEvents.Commits)
            {
                foreach (var added in commit.Added)
                {
                    if (added.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    {
                        var pngFilePath = $"{gitCommitEvents.Repository.Url}/raw/master/{added}";

                        log.LogInformation($"Extracted PNG file path: {pngFilePath}");

                        yield return pngFilePath;
                    }
                }
            }
        }

        private static void PersistPngImageFilePaths(ILogger log, IEnumerable<string> pngImageFilePaths)
        {
            if (!pngImageFilePaths.Any())
            {
                log.LogInformation("No PNG image file paths to processed.");
                return;
            }

            log.LogInformation("TODO: Persist down to azure table storage");
        }
    }
}
