using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory() { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

var replyQueue = await channel.QueueDeclareAsync(
    queue: "",  // RabbitMQ genera un nome univoco per la coda.
    exclusive: true, // Solo questa connessione può usarla; alla sua chiusura viene eliminata.
    autoDelete: true // Eliminata quando si disconnette l'ultimo consumer, dopo averne avuto almeno uno.
);

await channel.QueueDeclareAsync(
    queue: "request-queue",
    exclusive: false
);

var consumer = new AsyncEventingBasicConsumer(channel);

consumer.ReceivedAsync += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($"Reply Recieved: {message}");

    return Task.CompletedTask;
};

await channel.BasicConsumeAsync(
    queue: replyQueue.QueueName,
    autoAck: true,
    consumer: consumer
);

// Invia una nuova richiesta ogni secondo; interrompi il client con Ctrl+C.
while (true)
{
    var properties = new BasicProperties
    {
        ReplyTo = replyQueue.QueueName,             // Coda alla quale il server deve inviare la risposta.
        CorrelationId = Guid.NewGuid().ToString()   // Identificativo per associare la risposta alla richiesta.
    };

    var message = "Can I request a reply";
    var body = Encoding.UTF8.GetBytes(message);

    Console.WriteLine($"Sending Request: {properties.CorrelationId}");

    await channel.BasicPublishAsync(
        exchange: "",                 // Exchange predefinito.
        routingKey: "request-queue",   // Coda delle richieste al server.
        mandatory: false,             // Se la coda non esiste, il messaggio viene scartato.
        basicProperties: properties,  // Metadati della richiesta: ReplyTo e CorrelationId.
        body: body                    // Contenuto della richiesta in byte.
    );

    await Task.Delay(1000); // Pausa per osservare richieste e risposte nella console.
}
