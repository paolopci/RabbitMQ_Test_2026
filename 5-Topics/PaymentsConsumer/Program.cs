using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory() { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.ExchangeDeclareAsync(
    exchange: "topic",
    type: ExchangeType.Topic
);

var queueName = (await channel.QueueDeclareAsync()).QueueName;
await channel.QueueBindAsync(
    queue: queueName,
    exchange: "topic",
    routingKey: "#.payments"
);

var consumer = new AsyncEventingBasicConsumer(channel);

consumer.ReceivedAsync += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($"Payments - Recieved new message: {message}");

    return Task.CompletedTask;
};

await channel.BasicConsumeAsync(
    queue: queueName,
    autoAck: true,
    consumer: consumer
);

Console.WriteLine("Payments Consuming");

Console.ReadKey();
