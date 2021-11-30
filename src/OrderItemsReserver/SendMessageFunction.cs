using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;

namespace MessageFunctions
{
    public static class SendMessageFunction
    {
        [FunctionName("SendMessage")]
        [return: ServiceBus("az-queue", ServiceBusEntityType.Queue)]
        public static async Task<string> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
           /* log.LogInformation("SendMessage function requested");
            string body = req.Query["order"];
            using (var reader = new StreamReader(req.Body, Encoding.UTF8))
            {
                body = await reader.ReadToEndAsync();
                log.LogInformation($"Message body : {body}");
            }
            log.LogInformation($"SendMessage processed.");*/
            
            string model = req.Query["order"];

            // If the user specified a model id, find the details of the model of watch
            
                var content = await new StreamReader(req.Body).ReadToEndAsync();

                return content;
            
        }
    }
}
