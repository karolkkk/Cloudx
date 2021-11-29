using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs.Host;


namespace DeliveryOrderProccesor
{
    public static class Function1
    {

        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [CosmosDB(databaseName: "cloudx-final-db", collectionName: "cloudx-final-container", CreateIfNotExists = true , PartitionKey = "/id",
             ConnectionStringSetting = "AccountEndpoint" )] IAsyncCollector<dynamic> documentsOut, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string order = req.Query["order"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
           
                // Add a JSON document to the output container.
                await documentsOut.AddAsync(new
                {
                    // create a random ID
                    id = System.Guid.NewGuid().ToString(),
                    order = data
                });
                log.LogInformation($"Hello, {order}. This HTTP triggered function executed successfully.");
            
            string responseMessage = string.IsNullOrEmpty(order)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {order}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);


            /* log.LogInformation("C# HTTP trigger function processed a request.");

             string content = req.Query["order"];
             List<OrderItem> orderItems = new List<OrderItem>();



             var requestBody = new StreamReader(req.Body).ReadToEnd();
             //string requestBody = await streamReader.ReadToEndAsync();
             dynamic data = JsonConvert.DeserializeObject(requestBody);

             MyClass request = JsonConvert.DeserializeObject<MyClass>(requestBody);


             //log.LogInformation($"C# Http trigger function executed at: {DateTime.Now}");
             //var str = Environment.GetEnvironmentVariable("webdb_connection");
             Order order = JsonConvert.DeserializeObject<Order>(request.order);
             decimal total = order.Total();
             Address shippingAdress;
             order = order ?? data?.order;
             orderItems = orderItems ?? data?.order.OrderItems;
             shippingAdress = order ?? data?.order.ShipToAddress;
             document = new { Id = "Identifier1", Title = "Some Title" };

             if (!string.IsNullOrEmpty(content))
             {
                 // Add a JSON document to the output container.
                 *//*  documentsOut.Add(new
                   {
                       // create a random ID
                       id = System.Guid.NewGuid().ToString(),
                       order = order,
                       orderItems = orderItems,
                       total = total,
                       shippingAdress = shippingAdress
                   });*//*
                 document = new { Id = "Identifier1", Title = "Some Title" };
             }

             string responseMessage = string.IsNullOrEmpty(content)
                 ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                 : $"Hello, {order}. This HTTP triggered function executed successfully.";

             //return new OkObjectResult(responseMessage);*/
        }
    }
}
