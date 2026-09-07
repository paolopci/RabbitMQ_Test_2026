using System.Text;
using RabbitMQ.Client;

var factory = new ConnectionFactory() { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(
    queue: "letterbox",
    durable: false,
    exclusive: false,
    autoDelete: false,
    arguments: null
);

var messageId = 1;
var random = new Random();

while (true)
{
    var message = $"Sending Message Id: {messageId}";
    var body = Encoding.UTF8.GetBytes(message);
    await channel.BasicPublishAsync("", "letterbox", body);
    Console.WriteLine($"Send message: {message}");

    var waitTime = random.Next(1, 4);
    Task.Delay(TimeSpan.FromSeconds(waitTime)).Wait();

    messageId++;
}
