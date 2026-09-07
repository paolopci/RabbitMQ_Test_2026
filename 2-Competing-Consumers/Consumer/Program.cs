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
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory() { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();


// Dichiara la coda: la crea se non esiste; se esiste, le proprieta devono coincidere.
await channel.QueueDeclareAsync(
    // Nome della coda dalla quale i consumer ricevono i messaggi.
    queue: "letterbox",
    // false: la coda non sopravvive al riavvio del broker.
    durable: false,
    // false: la coda puo essere usata da piu connessioni, quindi da consumer concorrenti.
    exclusive: false,
    // false: la coda non viene eliminata automaticamente quando si disconnette l'ultimo consumer.
    autoDelete: false,
    // Nessun argomento aggiuntivo, come TTL dei messaggi o configurazione dead-letter.
    arguments: null
);

// Limita i messaggi consegnati e ancora non confermati, evitando di sovraccaricare un consumer.
await channel.BasicQosAsync(
    // 0: nessun limite sulla dimensione in byte; qui si usa il limite sul numero di messaggi.
    prefetchSize: 0,
    // Al massimo un messaggio non confermato per consumer: il successivo arriva dopo l'ack.
    prefetchCount: 1,
    // false: RabbitMQ applica il limite a ciascun nuovo consumer, non all'intero canale.
    global: false
);

var consumer = new AsyncEventingBasicConsumer(channel);
var random = new Random();

// Registra con += il gestore asincrono eseguito quando questo consumer riceve un messaggio.
// model: oggetto che ha generato l'evento (il consumer); qui non viene utilizzato.
// ea: dati della consegna, tra cui Body (contenuto) e DeliveryTag (identificativo per l'ack).
// async consente di usare await nel gestore, ad esempio per confermare la consegna.
consumer.ReceivedAsync += async (model, ea) =>
{
    var processingTime = random.Next(1, 6);
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);

    Console.WriteLine($"recieved: '{message}', will take {processingTime} to process");

    Task.Delay(TimeSpan.FromSeconds(processingTime)).Wait();

    // Conferma al broker che l'elaborazione e terminata e libera il posto previsto dal prefetch.
    await channel.BasicAckAsync(
        // Identifica la consegna da confermare sullo stesso canale che l'ha ricevuta.
        deliveryTag: ea.DeliveryTag,
        // false: conferma solo questa consegna, non tutte quelle precedenti.
        multiple: false
    );
};

// Avvia la sottoscrizione: RabbitMQ consegnera i messaggi al gestore ReceivedAsync.
await channel.BasicConsumeAsync(
    // Nome della coda da consumare; coincide con quella dichiarata sopra.
    queue: "letterbox",
    // false: conferma manuale tramite BasicAckAsync dopo l'elaborazione.
    // Se la connessione si chiude prima dell'ack, il messaggio puo essere riconsegnato.
    autoAck: false,
    // Istanza che riceve le consegne e richiama il gestore registrato sopra.
    consumer: consumer
);

Console.WriteLine("Consuming");
Console.ReadKey();
