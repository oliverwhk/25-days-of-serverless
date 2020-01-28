using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace day_06
{
    public class Scheduler
    {
        [FunctionName(nameof(Scheduler))]
        public async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var message = context.GetInput<string>();

            var outputs = new List<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            var sayHelloFunctionName = nameof(Scheduler) + "_" + nameof(SayHello);
            outputs.Add(await context.CallActivityAsync<string>(sayHelloFunctionName, "Tokyo"));
            outputs.Add(await context.CallActivityAsync<string>(sayHelloFunctionName, "Seattle"));
            outputs.Add(await context.CallActivityAsync<string>(sayHelloFunctionName, "London"));

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
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
            string instanceId = await client.StartNewAsync(nameof(Scheduler), message);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return client.CreateCheckStatusResponse(req, instanceId);
        }
    }
}