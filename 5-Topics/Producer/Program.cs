using System.Text;
using RabbitMQ.Client;






var factory = new ConnectionFactory() { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.ExchangeDeclareAsync(
    exchange: "topic",
    type: ExchangeType.Topic // instradamento tramite patter routing key
);

// producer invia 2 messaggi 
// messaggio 1
var userPaymentsMessage = "A european user paid for something";
var userPaymentsBody = Encoding.UTF8.GetBytes(userPaymentsMessage);
await channel.BasicPublishAsync(
    exchange: "topic",
    routingKey: "user.europe.payments",
    body: userPaymentsBody
);

Console.WriteLine($"Send message: {userPaymentsMessage}");


// producer invia 2 messaggi 
// messaggio 2
var businessOrderMessage = "A european business ordered goods";

var businessOrderBody = Encoding.UTF8.GetBytes(businessOrderMessage);

await channel.BasicPublishAsync(exchange: "topic", routingKey: "business.europe.order", businessOrderBody);

Console.WriteLine($"Send message: {businessOrderMessage}");