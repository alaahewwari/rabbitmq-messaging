using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Microsoft.Extensions.Configuration;

var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

Console.WriteLine($"[ENV] Loaded environment: {environment}");
Console.WriteLine($"[CONFIG] RabbitMQ Uri: {config["RabbitMQ:Uri"]}");
// Read RabbitMQ config values
var rabbitSection = config.GetSection("RabbitMQ");
string? uri = rabbitSection["Uri"];
string? exchangeName = rabbitSection["ExchangeName"];
string? routingKey = rabbitSection["RoutingKey"];
string? clientName = rabbitSection["ClientName"];
int processingDelaySeconds = int.TryParse(rabbitSection["DelayMilliseconds"], out var delay) ? delay : 1;

// Create a RabbitMQ connection factory
ConnectionFactory factory = new()
{
    Uri = new Uri(uri!),
    ClientProvidedName = clientName
};

// Establish connection and channel
using IConnection connection = factory.CreateConnection();
using IModel channel = connection.CreateModel();

// Declare the exchange and queue if not already present
channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
channel.QueueDeclare("Email Queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
channel.QueueBind("Email Queue", exchangeName, routingKey);

// Limit to one unacknowledged message at a time (QoS)
channel.BasicQos(0, 1, false);

// Create a consumer and define what happens when a message is received
var consumer = new EventingBasicConsumer(channel);
consumer.Received += (sender, args) =>
{
    // Simulate processing delay
    Task.Delay(TimeSpan.FromMicroseconds(processingDelaySeconds)).Wait();

    // Decode and display the message
    var body = args.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($"[x] Received message: {message}");

    // Acknowledge message as successfully processed
    channel.BasicAck(args.DeliveryTag, multiple: false);
};

// Start consuming messages from the queue
string consumerTag = channel.BasicConsume("Email Queue", autoAck: false, consumer);

// Keep the app running to continue receiving messages
Console.WriteLine(" [*] Waiting for messages. Press Enter to exit.");
Console.ReadLine();

// Clean up on exit
channel.BasicCancel(consumerTag);
channel.Close();
connection.Close();
