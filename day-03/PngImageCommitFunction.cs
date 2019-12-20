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
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
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

            var pngImages = ExtractPngImageFilePaths(log, gitCommitEvents);
            await PersistPngImageFilePaths(log, pngImages);

            return new OkObjectResult("");
        }

        private static IEnumerable<PngImage> ExtractPngImageFilePaths(ILogger log, GitCommitEvent gitCommitEvents)
        {
            foreach (var commit in gitCommitEvents.Commits)
            {
                foreach (var added in commit.Added)
                {
                    if (added.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    {
                        var pngFilePath = $"{gitCommitEvents.Repository.Url}/raw/master/{added}";

                        log.LogInformation($"Extracted PNG file path: {pngFilePath}");

                        yield return new PngImage { Name = added, Url = pngFilePath };
                    }
                }
            }
        }

        private static async Task PersistPngImageFilePaths(ILogger log, IEnumerable<PngImage> pngImages)
        {
            if (!pngImages.Any())
            {
                log.LogInformation("No PNG images to persist.");
                return;
            }

            var storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("day032019");
            await table.CreateIfNotExistsAsync();

            var insertBatch = new TableBatchOperation();

            foreach (var pngImage in pngImages)
            {
                var pngImageFileName = pngImage.Name.Substring(pngImage.Name.LastIndexOf("/") + 1);
                var entity = new PngImageEntity(pngImageFileName)
                {
                    Url = pngImage.Url
                };
                insertBatch.Add(TableOperation.Insert(entity));
            }

            await table.ExecuteBatchAsync(insertBatch);

            log.LogInformation($"Persisted {pngImages.Count()} PNG images.");
        }
    }
}
