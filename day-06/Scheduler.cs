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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SchedulerOptions _options;

        public Scheduler(IOptions<SchedulerOptions> options, IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
        }

        [FunctionName(nameof(Scheduler))]
        public async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            var message = context.GetInput<SlackMessage>();

            var schedulingMessage = $"\"{message.Text}\" has been scheduled";
             await context.CallActivityAsync(nameof(Scheduler) + "_" + nameof(PostSlackMessage), schedulingMessage);

            //TODO: To schedule for some time
        }

        [FunctionName(nameof(Scheduler) + "_" + nameof(PostSlackMessage))]
        public async void PostSlackMessage([ActivityTrigger] IDurableActivityContext context, ILogger log)
        {
            var message = context.GetInput<string>();

            var httpClient = _httpClientFactory.CreateClient();

            var slackContent = new { Text = message };
            var slackContentJson = JsonConvert.SerializeObject(slackContent, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            await httpClient.PostAsync(_options.SlackWebhookUrl, new StringContent(slackContentJson));

            log.LogInformation($"Posted slack message: \"{message}.\"");
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