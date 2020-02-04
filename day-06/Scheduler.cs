using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace day_06
{
    public partial class Scheduler
    {
        [FunctionName(nameof(Scheduler))]
        public async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            var message = context.GetInput<SlackMessage>();

            var slackWebhookUrl = "https://hooks.slack.com/services/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";
            var slackContent = new
            {
                Text = $"TODO: remind you about '{message.Text}'"
            };

            log.LogInformation($"Timer of 5 minutes is starting...");
            DateTime inFiveMinutes = context.CurrentUtcDateTime.AddMinutes(5);
            await context.CreateTimer(inFiveMinutes, CancellationToken.None);
            log.LogInformation($"Timer of 5 minutes has completed.");

            log.LogInformation($"Timer of 1 hour is starting...");
            DateTime inOneHour = context.CurrentUtcDateTime.AddHours(1);
            await context.CreateTimer(inOneHour, CancellationToken.None);
            log.LogInformation($"Timer of 1 hour has completed.");

            var slackContentJson = JsonConvert.SerializeObject(slackContent, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            await context.CallHttpAsync(HttpMethod.Post, new Uri(slackWebhookUrl), slackContentJson);
        }

        [FunctionName(nameof(Scheduler) + "_" + nameof(SayHello))]
        public string SayHello([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        }

        [FunctionName(nameof(Scheduler) + "_" + nameof(HttpStart))]
        public async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "schedule")]HttpRequestMessage req,
            [DurableClient]IDurableOrchestrationClient client,
            ILogger log)
        {
            var message = await req.Content.ReadAsStringAsync();
            string instanceId = await client.StartNewAsync(nameof(Scheduler), new SlackMessage { Text = message});

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return client.CreateCheckStatusResponse(req, instanceId);
        }
    }
}