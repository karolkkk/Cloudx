using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.IO;

namespace FunctionApp2
{
    public static class OrderItemsReserver
    {
        [FunctionName("OrderItemsReserver")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            // Retrieve the model id from the query string
            string model = req.Query["order"];

            // If the user specified a model id, find the details of the model of watch
            if (model != null)
            {
                var content = await new StreamReader(req.Body).ReadToEndAsync();

                MyClass request = JsonConvert.DeserializeObject<MyClass>(content);
                Order order = JsonConvert.DeserializeObject<Order>(request.order);
                //log.LogInformation($"C# Http trigger function executed at: {DateTime.Now}");
                var str = Environment.GetEnvironmentVariable("webdb_connection");
                using (SqlConnection conn = new SqlConnection(str))
                {
                    conn.Open();
                    var insertIntoOrders = ($"INSERT INTO [dbo].[Orders] " +
                                       "([BuyerId]" +
                                       ",[OrderDate]" +
                                       ",[ShipToAddress_Street]" +
                                       ",[ShipToAddress_City]" +
                                       ",[ShipToAddress_State]" +
                                       ",[ShipToAddress_Country]" +
                                       ",[ShipToAddress_ZipCode])" +
                                 "VALUES" +
                                      "('" + order.BuyerId + "'," +
                                       "" + order.OrderDate + "," +
                                       "" + order.ShipToAddress + "," +
                                       "" + order.ShipToAddress.City + "," +
                                       "" + order.ShipToAddress.State + "," +
                                       "" + order.ShipToAddress.Country + "," +
                                       "" + order.ShipToAddress.ZipCode + ")");  
         
                    using (SqlCommand cmd = new SqlCommand(insertIntoOrders, conn))
                    {
                        // Execute the command and log the # rows affected.
                        var rows = await cmd.ExecuteNonQueryAsync();
                        log.LogInformation($"{rows} rows were updated");
                    }
                    var selectOrderId = ($"SELECT [Id] FROM [dbo].[Orders] WHERE " +
                                       "[BuyerId] = '" + order.BuyerId + "' AND" +
                                       "[OrderDate] = '" + order.OrderDate + "' AND" +
                                       "[ShipToAddress_Street] = '" + order.ShipToAddress.Street + "' AND" +
                                       "[ShipToAddress_City] = '" + order.ShipToAddress.City + "' AND" +
                                       "[ShipToAddress_State] = '" + order.ShipToAddress.State + "' AND" +
                                       "[ShipToAddress_Country] = '" + order.ShipToAddress.Country + "' AND" +
                                       "[ShipToAddress_ZipCode] = '" + order.ShipToAddress.ZipCode + "'");  
         
                    using (SqlCommand cmd = new SqlCommand(selectOrderId, conn))
                    {
                        // Execute the command and log the # rows affected.
                        var rows = await cmd.ExecuteNonQueryAsync();
                        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            string orderIdToAdd = rdr["CourseName"].ToString();
                            foreach (var item in order.OrderItems)
                            {
                                var insertIntoOrderItems = ($"INSERT INTO [dbo].[OrderItems] " +
                                               "(ItemOrdered_CatalogItemId" +
                                               ",[ItemOrdered_ProductName]" +
                                               ",[ItemOrdered_PictureUri]" +
                                               ",[UnitPrice]" +
                                               ",[Units]" +
                                               ",[OrderId])" +
                                         "VALUES" +
                                              "('" + item.ItemOrdered.CatalogItemId + "'," +
                                               "" + item.ItemOrdered.ProductName + "," +
                                               "" + item.ItemOrdered.PictureUri + "," +
                                               "" + item.UnitPrice + "," +
                                               "" + item.Units + "," +
                                               "" + orderIdToAdd + ")");
                                using (SqlCommand cmd1 = new SqlCommand(insertIntoOrders, conn))
                                {
                                    // Execute the command and log the # rows affected.
                                    var rows1 = await cmd1.ExecuteNonQueryAsync();
                                    log.LogInformation($"{rows1} rows were updated");
                                }
                            }
                                
                            }
                        log.LogInformation($"{rows} rows were updated");
                        
                    }
                    }
                    
                }
                return (ActionResult)new OkObjectResult($"Watch Details: {model}");
            /*              CreateContainerIfNotExists(log, context);

                          CloudStorageAccount storageAccount = GetCloudStorageAccount(context);
                          CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                          CloudBlobContainer container = blobClient.GetContainerReference("dummy-messages");


                              string randomStr = Guid.NewGuid().ToString();
                              CloudBlockBlob blob = container.GetBlockBlobReference(randomStr);

                              var serializeJesonObject = JsonConvert.SerializeObject(new { ID = randomStr, Content = model });
                              blob.Properties.ContentType = "application/json";

                              using (var ms = new MemoryStream())
                              {
                                  LoadStreamWithJson(ms, serializeJesonObject);
                                  await blob.UploadFromStreamAsync(ms);
                              }
                              log.LogInformation($"Bolb {randomStr} is uploaded to container {container.Name}");
                              await blob.SetPropertiesAsync();

                          return new OkObjectResult("UploadBlobHttpTrigger function executed successfully!!");
                          //return (ActionResult)new OkObjectResult($"Watch Details: {model}");
                      }
                      return new BadRequestObjectResult("Please provide a watch model in the query string");

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
                  }*/
            //return new BadRequestObjectResult("Please provide a watch model in the query string");
        }
            
        }
    }

