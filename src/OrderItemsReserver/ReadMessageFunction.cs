using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System.Threading.Tasks;


namespace ReadMessageFunction
{
    public static class ReadMessageFunction
    {
        [FunctionName("ReadMessageFromQueue")]
        public static async Task Run([ServiceBusTrigger("az-queue")] string myQueueItem, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
            CreateContainerIfNotExists(log, context);

            // Parse the connection string and return a reference to the storage account.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=orderitemsreserver202111;AccountKey=OO8daWMhH2Ct7hvbuXEtbUOZinVChF3ZCDqM/PXj6HydTOgU7DdWpif5t+A0gnwrnSbXTtAaYo5KSY8GBA3EPA==;EndpointSuffix=core.windows.net");
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("dummy-messages");


            string randomStr = Guid.NewGuid().ToString();
            CloudBlockBlob blob = container.GetBlockBlobReference(randomStr);

            var serializeJesonObject = JsonConvert.SerializeObject(new { ID = randomStr, Content = myQueueItem });
            blob.Properties.ContentType = "application/json";

            using (var ms = new MemoryStream())
            {
                LoadStreamWithJson(ms, serializeJesonObject);
                await blob.UploadFromStreamAsync(ms);
            }
            log.LogInformation($"Bolb {randomStr} is uploaded to container {container.Name}");
            await blob.SetPropertiesAsync();

            //return new OkObjectResult("UploadBlobHttpTrigger function executed successfully!!");
            //return (ActionResult)new OkObjectResult("UploadBlobHttpTrigger function executed successfully!!");

            //return new BadRequestObjectResult("Please provide a watch model in the query string");
        }


        private static void CreateContainerIfNotExists(ILogger logger, ExecutionContext executionContext)
        {
            CloudStorageAccount storageAccount = GetCloudStorageAccount(executionContext);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            string[] containers = new string[] { "dummy-messages" };
            foreach (var item in containers)
            {
                CloudBlobContainer blobContainer = blobClient.GetContainerReference(item);
                blobContainer.CreateIfNotExistsAsync();
            }
        }

        private static CloudStorageAccount GetCloudStorageAccount(ExecutionContext executionContext)
        {
            var config = new ConfigurationBuilder()
                            .SetBasePath(executionContext.FunctionAppDirectory)
                            .AddJsonFile("local.settings.json", true, true)
                            .AddEnvironmentVariables().Build();
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(config["CloudStorageAccount"]);
            return storageAccount;
        }
        private static void LoadStreamWithJson(Stream ms, object obj)
        {
            StreamWriter writer = new StreamWriter(ms);
            writer.Write(obj);
            writer.Flush();
            ms.Position = 0;
        }
    
    }
}

