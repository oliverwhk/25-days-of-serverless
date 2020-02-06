using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using day_06.Model;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace day_06
{
    public class Scheduler
    {
        private readonly SchedulerOptions _options;

        public Scheduler(IOptions<SchedulerOptions> options)
        {
            _options = options.Value;
        }

        [FunctionName(nameof(Scheduler))]
        public async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            var message = context.GetInput<SlackMessage>();

            var schedulingMessage = $"\"{message.Text}\" has been scheduled";
            await PostSlackMessage(context, schedulingMessage, log);

            //TODO: To schedule for some time
            // await context.CallActivityAsync(nameof(Scheduler) + "_" + nameof(PostSlackMessage), message);
        }

        public async Task PostSlackMessage(IDurableOrchestrationContext context, string message, ILogger log)
        {
            var slackContent = new { Text = message };
            var slackContentJson = JsonConvert.SerializeObject(slackContent, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            await context.CallHttpAsync(HttpMethod.Post, new Uri(_options.SlackWebhookUrl), slackContentJson);

            log.LogInformation($"Posted slack message: {message}.");
        }

        // [FunctionName(nameof(Scheduler) + "_" + nameof(PostSlackMessage))]
        // public string PostSlackMessage([ActivityTrigger] IDurableActivityContext context, ILogger log)
        // {
        //     var message = context.GetInput<SlackMessage>();
        //
        //     var slackWebhookUrl = "https://hooks.slack.com/services/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";
        //     var slackContent = new
        //     {
        //         Text = $"TODO: remind you about '{message.Text}'"
        //     };
        //     
        //     var slackContentJson = JsonConvert.SerializeObject(slackContent, new JsonSerializerSettings
        //     {
        //         ContractResolver = new CamelCasePropertyNamesContractResolver()
        //     });
        //
        //     await context.CallHttpAsync(HttpMethod.Post, new Uri(slackWebhookUrl), slackContentJson);
        //
        //
        //     log.LogInformation($"Post slack message: {message}.");
        //     return $"Hello {message}!";
        // }

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