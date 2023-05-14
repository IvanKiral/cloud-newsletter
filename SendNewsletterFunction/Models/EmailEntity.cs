using System;
using Azure;
using Microsoft.WindowsAzure.Storage.Table;
using ITableEntity = Azure.Data.Tables.ITableEntity;

namespace SendNewsletterFunction.Models;

public class EmailEntity: TableEntity, ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public bool Subscribed { get; set; }
}