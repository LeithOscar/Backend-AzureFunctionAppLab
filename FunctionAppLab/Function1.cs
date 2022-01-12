namespace FunctionAppLab
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Microsoft.Azure.Cosmos.Table;


    public static class FunctionLab
    {
        //[FunctionName("SetVoteFunction")]
        //public static async Task<IActionResult> Run(
        //    [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        //    [Queue("outqueue"), StorageAccount("AzureWebJobsStorage")] ICollector<string> msg,
        //    ILogger log)
        //{
        //    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        //    dynamic data = JsonConvert.DeserializeObject(requestBody);

        //    string taskId = data?.taskId;
        //    string tenantId = data?.tenantId;

        //    msg.Add(taskId);
        //    msg.Add(tenantId);


        //    return new OkObjectResult($"set messeage with task:{taskId} and tenant:{tenantId}");
        //}


        [FunctionName("SetVoteFunction")]
        public static async Task<IActionResult> Run(
           [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
           ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            string taskId = data?.taskId;
            string tenantId = data?.tenantId;
            
            try
            {
                var connectionString = System.Environment.GetEnvironmentVariable("AzureWebJobsStorage");

                var storageAccount = CloudStorageAccount.Parse(connectionString);
                var tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
                var table = tableClient.GetTableReference("votetask");
                await table.CreateIfNotExistsAsync();


                // Create the InsertOrReplace table operation
                var entity = new ClientVote() {
                    TaskId= taskId,
                    TenandId= tenantId,
                    PartitionKey=taskId+tenantId,
                    RowKey= taskId+tenantId

                };

               TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(entity);
               TableResult result = await table.ExecuteAsync(insertOrMergeOperation);
          

            }
            catch (Exception)
            {
                throw;
            }


            return new OkObjectResult($"¡Set vote with task:{taskId} and tenant:{tenantId} succed!");
        }


        public class ClientVote : TableEntity
        {
          
            public string TaskId { get; set; }
            public string TenandId { get; set; }
        }
    }
}
