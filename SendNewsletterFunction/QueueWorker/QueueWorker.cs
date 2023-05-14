using System;
using System.Threading.Tasks;
using Azure;
using Azure.Communication.Email;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace SendNewsletterFunction.QueueWorker;

public static class QueueWorker
{
    [FunctionName("QueueWorker")]
    public static async Task RunAsync([ServiceBusTrigger("emails", Connection = "connection")] string myQueueItem, ILogger log)
    {
        log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
        
        var connectionString = Environment.GetEnvironmentVariable("communicationServiceConnectionString");; // Find your Communication Services resource in the Azure portal
        EmailClient emailClient = new EmailClient(connectionString);
        
        try
        {
            var emailSendOperation = emailClient.Send(
                wait: WaitUntil.Completed,
                senderAddress: "DoNotReply@02d0705d-d6da-4686-8665-d717e90e463b.azurecomm.net",
            recipientAddress: myQueueItem,
            subject: "This is the subject",
            htmlContent: "<html><body>This is the html body</body></html>");
            
            Console.WriteLine($"Email Sent. Status = {emailSendOperation.Value.Status}");

            /// Get the OperationId so that it can be used for tracking the message for troubleshooting
            string operationId = emailSendOperation.Id;
            Console.WriteLine($"Email operation id = {operationId}");
        }
        catch ( RequestFailedException ex )
        {
            /// OperationID is contained in the exception message and can be used for troubleshooting purposes
            Console.WriteLine($"Email send operation failed with error code: {ex.ErrorCode}, message: {ex.Message}");
        }
        
    }
}