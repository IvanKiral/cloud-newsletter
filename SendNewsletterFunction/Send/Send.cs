using System;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Messaging.ServiceBus;
using SendNewsletterFunction.Models;

namespace SendNewsletterFunction.Send;

public static class Send
{
    [FunctionName("Send")]
    public static async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function,  "post", Route = null)] HttpRequest req, ILogger log)
    {
        var numOfMessages = 0;

        var tableName = Environment.GetEnvironmentVariable("tableStorageTableName");
        var tableClient = new TableClient(Environment.GetEnvironmentVariable("tableStorageConnectionString"), tableName);

         Pageable<EmailEntity> queryResultsFilter = tableClient.Query<EmailEntity>(e => e.Subscribed);
         
         Console.WriteLine($"The query returned {queryResultsFilter.Count()} entities.");

         var clientOptions = new ServiceBusClientOptions
         { 
             TransportType = ServiceBusTransportType.AmqpWebSockets
         };
         
         var client = new ServiceBusClient(Environment.GetEnvironmentVariable("connection"), clientOptions);
         var sender = client.CreateSender("emails");
         
         using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();
         
         foreach (EmailEntity qEntity in queryResultsFilter)
         {
             Console.WriteLine($"{qEntity.RowKey}: {qEntity.Subscribed}");
             
             var email = qEntity.RowKey;
             if (!messageBatch.TryAddMessage(new ServiceBusMessage(email)))
             {
                 throw new Exception($"The message {email} is too large to fit in the batch.");
             }
         
             numOfMessages += 1;
         }
         
         try
         {
             await sender.SendMessagesAsync(messageBatch);
             Console.WriteLine($"A batch of {numOfMessages} messages has been published to the queue.");
         }
         finally
         {
             await sender.DisposeAsync();
             await client.DisposeAsync();
         }

         return new OkObjectResult($"Completed");
    }
}