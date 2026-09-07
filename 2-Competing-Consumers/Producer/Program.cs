/*

  Sì, nel tuo progetto funziona proprio così, grazie a queste impostazioni:
   - Tutte le istanze del consumer leggono dalla stessa coda letterbox.
   - prefetchCount: 1: ogni consumer riceve un solo messaggio alla volta, finché non lo conferma.
   - autoAck: false: il consumer manda l’ack dopo aver terminato l’elaborazione.
  Con un Producer e due istanze del Consumer, ad esempio:
   1. Il messaggio 1 arriva al Consumer A, che comincia a elaborarlo.
   2. Mentre A è occupato, il messaggio 2 può andare al Consumer B.
   3. Se entrambi sono occupati, il messaggio 3 rimane nella coda RabbitMQ.
   4. Appena uno termina e invia l’ack, può ricevere il messaggio 3.


Quindi non si accumulano ulteriori messaggi in attesa dentro ogni consumer: l’attesa avviene nella coda condivisa di RabbitMQ.
Non è necessariamente un’alternanza rigida A → B → A → B: il consumer più veloce può elaborare più messaggi. Ogni messaggio viene assegnato a un consumer alla volta; se questo si disconnette prima dell’ack, RabbitMQ può riconsegnarlo a un altro

*/


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
