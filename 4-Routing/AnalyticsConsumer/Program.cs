using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;






var factory = new ConnectionFactory() { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.ExchangeDeclareAsync(
    exchange: "routing",
    type: ExchangeType.Direct
);

var queueName = (await channel.QueueDeclareAsync()).QueueName;


// La coda Analytics accetta le chiavi analyticsonly e both.
await channel.QueueBindAsync(
    queue: queueName,
    exchange: "routing",
    routingKey: "analyticsonly"
);

/*
    Con "both", il producer pubblica una volta sola: 
    l’exchange mette una copia del messaggio in ciascuna coda, e ogni consumer stampa la propria copia.
*/
await channel.QueueBindAsync(
    queue: queueName,
    exchange: "routing",
    routingKey: "both"
);

var consumer = new AsyncEventingBasicConsumer(channel);

consumer.ReceivedAsync += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($"Analitics - Recieved new Message: {message}");
    return Task.CompletedTask;
};

await channel.BasicConsumeAsync(
    queue: queueName,   // Coda da cui ricevere i messaggi.
    autoAck: true,      // RabbitMQ considera il messaggio confermato alla consegna.
    consumer: consumer // Oggetto che riceve i messaggi tramite ReceivedAsync.
);

Console.WriteLine("Analytics Consuming");
Console.ReadKey();
