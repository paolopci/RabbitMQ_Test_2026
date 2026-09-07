/*
Qui ogni messaggio viene ricevuto da entrambi i consumer, perché ciascuno crea una propria coda e la collega all’exchange pubsub di tipo Fanout.
Producer → Exchange pubsub (Fanout)
                  ├── Coda A → FirstConsumer
                  └── Coda B → SecondConsumer
Il progetto funziona così:
  1. Il Producer invia un messaggio ogni due secondi.
  2. L’exchange inoltra una copia a ogni coda collegata, ignorando la routingKey.
  3. FirstConsumer legge dalla coda A; SecondConsumer legge dalla coda B.
  4. Ciascuno stampa il messaggio e incrementa il proprio contatore nella console.
 
 Quindi, se pubblichi 10 messaggi con entrambi già collegati, ciascun consumer riceve 10 messaggi. 
 Nel progetto Competing Consumers, invece, i consumer si dividono i messaggi di un’unica coda.
 Per lo smaltimento, qui hai autoAck: true: RabbitMQ considera ogni copia consegnata 
 appena la invia, senza aspettare che il consumer finisca di elaborarla. 
 Non hai il limite di un messaggio alla volta del progetto precedente: 
 se un consumer fosse lento, potrebbero accumularsi messaggi nel suo client.
 
 Le code create con QueueDeclareAsync() senza parametri sono temporanee e specifiche 
 di ogni istanza: quando la connessione si chiude vengono eliminate. 
 
 Per il test, avvia prima i consumer e poi il Producer: un consumer avviato 
 dopo non recupera i messaggi pubblicati prima che la sua coda fosse collegata.

*/

using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory() { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.ExchangeDeclareAsync(  // Asynchronously declare an exchange.
    exchange: "pubsub",
    type: ExchangeType.Fanout
);

var queueName = (await channel.QueueDeclareAsync()).QueueName;

await channel.QueueBindAsync(
    queue: queueName,
    exchange: "pubsub",
    routingKey: ""
);

var consumer = new AsyncEventingBasicConsumer(channel);
long processedMessages = 0; // Totale elaborato da questo consumer dall'avvio.

consumer.ReceivedAsync += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($"SecondConsumer - Recieved new message: {message}");
    var totalProcessed = Interlocked.Increment(ref processedMessages);
    Console.WriteLine($"SecondConsumer - Totale messaggi elaborati: {totalProcessed}");
    return Task.CompletedTask; // Segnala che la gestione del messaggio è terminata.
};

await channel.BasicConsumeAsync(
    queue: queueName,   // Nome della coda da cui ricevere i messaggi.
    autoAck: true,      // Conferma automatica alla consegna, senza attendere l'elaborazione.
    consumer: consumer  // Oggetto che gestisce i messaggi ricevuti.
);

Console.WriteLine("Consuming");

Console.ReadKey();



