using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace day_06
{
    public static class Scheduler
    {
        [FunctionName(nameof(Scheduler))]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
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
        public static string SayHello([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        }

        [FunctionName(nameof(Scheduler) + "_" + nameof(HttpStart))]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "schedule")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync(nameof(Scheduler), null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}