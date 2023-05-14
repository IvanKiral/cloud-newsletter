using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendNewsletterFunction.Models;

namespace SendNewsletterFunction.Subscribe;

public static class Subscribe
{
    [FunctionName("Subscribe")]
    public static async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ILogger log)
    {
        var tableName = Environment.GetEnvironmentVariable("tableStorageTableName");
        var tableClient = new TableClient(Environment.GetEnvironmentVariable("tableStorageConnectionString"), tableName);
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        if (string.IsNullOrEmpty(requestBody))
        {
            return new BadRequestObjectResult("The request body is empty");
        }

        var body = JsonConvert.DeserializeObject<EmailRequest>(requestBody);

        if (body.email == null)
        {
            return new BadRequestObjectResult("The request body does not contain email body");
        }

        Console.WriteLine(body.email);
        
        Pageable<EmailEntity> queryResultsFilter = tableClient.Query<EmailEntity>(e => e.RowKey == body.email);

        if (queryResultsFilter.Any())
        {
            var entity = queryResultsFilter.First();
            
            if (entity.Subscribed)
            {
                return new BadRequestObjectResult("User already subscribed");
            }
            
            entity.Subscribed = true;

            try
            {
                await tableClient.UpdateEntityAsync(entity, entity.ETag, TableUpdateMode.Replace);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new BadRequestObjectResult(e.ToString());
            }
        }
        else
        {

            var emailEntity = new EmailEntity
            {
                PartitionKey = body.email.First().ToString(),
                RowKey = body.email,
                Subscribed = true
            };

            try
            {
                tableClient.AddEntity(emailEntity);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new BadRequestObjectResult(e.ToString());
            }
        }

        return new OkObjectResult("Subscribed");
    }
}