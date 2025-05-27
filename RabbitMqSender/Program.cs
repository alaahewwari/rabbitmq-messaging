using RabbitMQ.Client;
using System.Text;
using Microsoft.Extensions.Configuration;
var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();
// Read RabbitMQ configuration values
var rabbitSection = config.GetSection("RabbitMQ");
string uri = rabbitSection["Uri"];
string exchangeName = rabbitSection["ExchangeName"];
string routingKey1 = rabbitSection["RoutingKey1"];
string routingKey2 = rabbitSection["RoutingKey2"];
string clientName = rabbitSection["ClientName"];
int messageCount = int.Parse(rabbitSection["MessageCount"]);
int delayMilliseconds = int.Parse(rabbitSection["DelayMilliseconds"]);

// Create a RabbitMQ connection factory
ConnectionFactory factory = new()
{
    Uri = new Uri(uri),
    ClientProvidedName = clientName
};

// Establish the connection and channel to RabbitMQ
using IConnection connection = factory.CreateConnection();
using IModel channel = connection.CreateModel();

// Declare a direct exchange (creates it if it doesn't exist)
channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);

// Declare the queue (creates it if it doesn't exist)
channel.QueueDeclare(
    queue: "Email Queue",
    durable: false,        // Queue won't survive a broker restart
    exclusive: false,      // Not exclusive to the current connection
    autoDelete: false,     // Don't delete when last consumer disconnects
    arguments: null        // No extra arguments
);
channel.QueueDeclare(
    queue: "SMS Queue",
    durable: false,
    exclusive: false,
    autoDelete: false,
    arguments: null
);

// Bind the queue to the exchange using the routing key
channel.QueueBind("Email Queue", exchangeName, routingKey1);
channel.QueueBind("SMS Queue", exchangeName, routingKey2);

// Send multiple messages with a delay between each
for (int i = 0; i < messageCount; i++)
{
    // Message for queue1 (e.g. Email processing)
    string emailMessage = $"[Email] Hello Alaa {i}";
    byte[] emailBytes = Encoding.UTF8.GetBytes(emailMessage);
    channel.BasicPublish(exchangeName, routingKey1, null, emailBytes);
    Console.WriteLine($"Sent to Email Queue: {emailMessage}");

    // Message for queue2 (e.g. SMS processing)
    string smsMessage = $"[SMS] Hello Alaa {i}";
    byte[] smsBytes = Encoding.UTF8.GetBytes(smsMessage);
    channel.BasicPublish(exchangeName, routingKey2, null, smsBytes);
    Console.WriteLine($"Sent to SMS Queue: {smsMessage}");

    Thread.Sleep(delayMilliseconds);
}

// The connection and channel are closed automatically because of 'using'
