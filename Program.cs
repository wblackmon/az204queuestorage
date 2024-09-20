using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, Azure Storage Queue!");

// Create and manage Azure Queue Storage and messages by using .NET

// Summary of Commands
// Create a resource group:
// az group create --name az204-storage-queue-rg --location eastus

// Create a storage account:
// az storage account create --name az204storageqacct01 --resource-group az204-storage-queue-rg --location eastus --sku Standard_LRS

// Create the storage queue:
// az storage queue create --account-name az204storageqacct01 --name az204storagequeue01

// Create the Queue service client

string connectionString = "STORAGE_CONNECTION_STRING";
string queueName = GenerateRandomQueueName("az204storageq01");

QueueClient queueClient = new QueueClient(connectionString, queueName);

await queueClient.CreateIfNotExistsAsync();

// Clear the queue before starting
await queueClient.ClearMessagesAsync();

// Verify the queue exists

if (queueClient.Exists())
{
    Console.WriteLine($"The queue '{queueClient.Name}' exists");
}
else
{
    Console.WriteLine($"The queue '{queueClient.Name}' does not exist");
}

// Add a message to the queue

string messageContent = $"Hello, Azure Storage Queue: {queueClient.Name}!";
await queueClient.SendMessageAsync(messageContent);

// Peek at the message in the queue

PeekedMessage[] messages = await queueClient.PeekMessagesAsync();

// Verify the peeked message

if (messages.Length > 0)
{
    Console.WriteLine($"The peeked message is: '{messages[0].MessageText}'");
}
else
{
    Console.WriteLine("No messages in the queue");
}

// Change the contents of a queued message
string? updatedMessageId = null;

if (await queueClient.ExistsAsync())
{
    QueueMessage[] retrievedMessage = await queueClient.ReceiveMessagesAsync(1, TimeSpan.FromSeconds(5));
    if (retrievedMessage.Length > 0)
    {
        await queueClient.UpdateMessageAsync(retrievedMessage[0].MessageId, retrievedMessage[0].PopReceipt, retrievedMessage[0].MessageText + " - Updated", TimeSpan.FromSeconds(5));
        updatedMessageId = retrievedMessage[0].MessageId;
        Console.WriteLine($"The updated message ID is: '{updatedMessageId}'");
        await Task.Delay(1000);
    }
}

// Verify the updated message

if (queueClient.Exists() && updatedMessageId != null)
{
    QueueMessage[] retrievedMessage = await queueClient.ReceiveMessagesAsync(1, TimeSpan.FromSeconds(5));
    if (retrievedMessage.Length > 0)
    {
        Console.WriteLine($"The updated message is: '{retrievedMessage[0].MessageText}'");
    }

    // Get the queue length
    int queueLength = await queueClient.GetPropertiesAsync().ContinueWith(task => task.Result.Value.ApproximateMessagesCount);
    Console.WriteLine($"The queue length is: {queueLength}");

    // Verify that the message exists, if it does, delete it from the queue

    if (retrievedMessage.Length > 0)
    {
        await queueClient.DeleteMessageAsync(retrievedMessage[0].MessageId, retrievedMessage[0].PopReceipt);
    }
}

// Delete the queue

await queueClient.DeleteIfExistsAsync();

// Verify the queue was deleted

if (await queueClient.ExistsAsync())
{
    Console.WriteLine($"The queue '{queueClient.Name}' exists");
}
else
{
    Console.WriteLine($"The queue '{queueClient.Name}' does not exist");
}

string GenerateRandomQueueName(string prefix)
{
    return $"{prefix}-{Guid.NewGuid().ToString().Substring(0, 8)}";
}
