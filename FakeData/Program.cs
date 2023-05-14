// See https://aka.ms/new-console-template for more information

using Azure;
using Azure.Data.Tables;
using Bogus;

var subscribedGenerator = new Faker<Emails>()
    .RuleFor(e => e.RowKey, f => ($"{f.Name.FirstName()}{f.Name.LastName()}@email.com").ToLower())
    .RuleFor(e => e.Subscribed, _ => true);


var unsubscribedGenerator = new Faker<Emails>()
    .RuleFor(e => e.RowKey, f => ($"{f.Name.FirstName()}{f.Name.LastName()}@email.com").ToLower())
    .RuleFor(e => e.Subscribed, _ => false);

var generatedEmails = subscribedGenerator.Generate(30);
generatedEmails.AddRange(unsubscribedGenerator.Generate(20));

foreach (var e in generatedEmails)
{
    e.PartitionKey = e.RowKey.First().ToString();
    Console.WriteLine($" {e.PartitionKey} {e.RowKey} {e.Subscribed}");
}

var tableName = "emails";
var tableClient = new TableClient("DefaultEndpointsProtocol=https;AccountName=484971hw04;AccountKey=WLITisuAgWMpxrelfGs/2CGDf44VWdaNBao9rXkqkUxCayEAgNJHwAqAZLwm0Um/Hrf8sajRVqR5+AStIVjYGw==;EndpointSuffix=core.windows.net", tableName);

Pageable<TableEntity> queryResultsFilter = tableClient.Query<TableEntity>();
         
Console.WriteLine($"The query returned {queryResultsFilter.Count()} entities.");

foreach (var e in generatedEmails)
{
    tableClient.AddEntity(e);
}


public sealed class Emails: ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public bool Subscribed { get; set; }
}

