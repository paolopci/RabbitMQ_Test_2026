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

var factory = new ConnectionFactory() { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();
await channel.ExchangeDeclareAsync(
    exchange: "pubsub",
    type: ExchangeType.Fanout // Il fanout exchange inoltra ogni messaggio a 
                              // tutte le code collegate, ignorando la routing key.
);

var messageNumber = 1;

while (true)
{
    var message = $"Messaggio {messageNumber}: Hello I want to broadcast this message";
    var body = Encoding.UTF8.GetBytes(message);

    await channel.BasicPublishAsync(
        exchange: "pubsub", // Nome dell'exchange a cui inviare il messaggio.
        routingKey: "",    // Chiave di instradamento: vuota perché l'exchange Fanout la ignora.
        mandatory: false,  // Se nessuna coda può ricevere il messaggio, il broker lo scarta.
        body: body         // Contenuto del messaggio, già convertito in byte.
    );

    Console.WriteLine($"Send message: {message}");
    messageNumber++;

    await Task.Delay(2000); // Attende due secondi prima di inviare il prossimo messaggio.
}
