using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory() { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(
    queue: "request-queue", // Nome della coda che riceve le richieste dei client.
    durable: false,         // La coda non sopravvive al riavvio del broker RabbitMQ.
    exclusive: false       // La coda puo' essere usata da altre connessioni e non viene eliminata alla chiusura di questa connessione.
);

var consumer = new AsyncEventingBasicConsumer(channel);

consumer.ReceivedAsync += async (model, ea) =>
{
    var replyTo = ea.BasicProperties.ReplyTo;
    if (string.IsNullOrWhiteSpace(replyTo))
    {
        Console.WriteLine("Richiesta senza ReplyTo: impossibile inviare la risposta.");
        return;
    }

    Console.WriteLine($"Received Request: {ea.BasicProperties.CorrelationId}");
    var replayMessage = $"This is your reply: {ea.BasicProperties.CorrelationId}";
    var body = Encoding.UTF8.GetBytes(replayMessage);

    await channel.BasicPublishAsync(
        exchange: "",         // Exchange predefinito: instrada usando il nome della coda.
        routingKey: replyTo,  // Nome della coda di risposta indicato dal client in ReplyTo.
        mandatory: false,     // Se la coda di risposta non esiste, il messaggio viene scartato.
        body: body            // Contenuto della risposta convertito in byte UTF-8.
    );
    // Con async/await il Task viene restituito automaticamente, senza return Task.CompletedTask.
};

await channel.BasicConsumeAsync(queue: "request-queue", autoAck: true, consumer: consumer);

Console.ReadKey();
