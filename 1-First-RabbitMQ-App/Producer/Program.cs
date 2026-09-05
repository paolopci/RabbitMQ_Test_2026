using System.Text;
using RabbitMQ.Client;






var factory = new ConnectionFactory() { HostName = "localhost" };
/*
Asynchronously create a connection to one of the endpoints provided 
by the IEndpointResolver returned by the EndpointResolverFactory. 
By default the configured hostname and port are used.

*/
using var connection = await factory.CreateConnectionAsync();
/*
 Asynchronously create and return a fresh channel, session, and channel.
*/
using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(
    queue: "letterbox",// nome della coda se lascio "" la genera il server
    durable: false, // se la cosa sopravvive a un restart 
    exclusive: false,
    autoDelete: false,// Should this queue be auto-deleted when its last consumer (if any) unsubscribes?
    arguments: null // Optional; additional queue arguments, e.g. "x-queue-type"
);

var message = "This is my first Message";
var body = Encoding.UTF8.GetBytes(message);
await channel.BasicPublishAsync(
    exchange: "",
    routingKey: "letterbox",
    body: body
    );

Console.WriteLine($"Send message: {message}");
