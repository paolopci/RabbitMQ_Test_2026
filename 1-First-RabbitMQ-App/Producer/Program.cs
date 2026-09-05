using System.Text;
using RabbitMQ.Client;

// Configura il client: localhost indica il computer su cui gira questo programma.
// Il broker RabbitMQ deve essere raggiungibile; con le impostazioni predefinite
// si usa la porta AMQP 5672. La factory configura le connessioni, non le apre.
var factory = new ConnectionFactory() { HostName = "localhost" };

// Apre in modo asincrono la connessione di rete al broker.
// await attende il risultato senza bloccare il thread durante l'attesa:
// connection contiene la connessione ottenuta, non il Task dell'operazione.
// using var rilascia la risorsa a fine programma, anche in caso di eccezione.
using var connection = await factory.CreateConnectionAsync();

// Crea un canale logico sulla connessione: serve per dichiarare code e inviare
// messaggi. Una connessione può ospitare più canali senza aprire altre connessioni TCP.
// In RabbitMQ.Client 7 sostituisce il CreateModel() del corso basato sulla versione 6.
// Il canale viene rilasciato prima della connessione, in ordine inverso di dichiarazione.
using var channel = await connection.CreateChannelAsync();

// Una coda conserva i messaggi in attesa di consegnarli ai consumer.
// Questa dichiarazione è ASINCRONA: await attende la risposta del broker
// prima di passare alla pubblicazione; non trasforma la chiamata in sincrona.
// Crea la coda se manca; se esiste con proprietà equivalenti, la riutilizza.
// Producer e Consumer devono dichiararla con proprietà compatibili:
// ad esempio, cambiare durable su una coda esistente causa PRECONDITION_FAILED.
await channel.QueueDeclareAsync(
    // Nome della coda. Con "" il broker genera un nome, recuperabile dal risultato.
    queue: "letterbox",
    // false: la coda non sopravvive al riavvio del broker.
    // true: la coda è durevole, ma ciò non rende automaticamente persistenti
    // i messaggi: anche il produttore deve pubblicarli come persistenti.
    durable: false,
    // false: la coda è accessibile da connessioni diverse, come Producer e Consumer.
    // true: è riservata alla connessione che la dichiara ed eliminata alla sua chiusura.
    exclusive: false,
    // false: la scomparsa dell'ultimo consumer non elimina automaticamente la coda.
    // true: dopo aver avuto almeno un consumer, la coda viene eliminata
    // quando si disconnette o si cancella l'ultimo consumer; non quando si svuota.
    autoDelete: false,
    // Opzioni aggiuntive in un dizionario: ad esempio x-message-ttl (durata
    // dei messaggi in millisecondi), x-max-length o x-queue-type.
    // null: il client non specifica opzioni aggiuntive; restano applicabili
    // le impostazioni predefinite e le policy configurate sul broker.
    arguments: null
);

var message = "This is my first Message";
// RabbitMQ trasporta byte: codifica il testo in UTF-8, da decodificare nel Consumer.
var body = Encoding.UTF8.GetBytes(message);

// Pubblica in modo asincrono tramite un exchange, che instrada i messaggi.
// Il completamento non significa che il Consumer abbia già elaborato il messaggio.
await channel.BasicPublishAsync(
    // "" seleziona l'exchange predefinito, che instrada in base al nome della coda.
    exchange: "",
    // Con l'exchange predefinito, indica la coda di destinazione.
    routingKey: "letterbox",
    // Contenuto binario del messaggio; qui non si impostano proprietà di persistenza.
    body: body
    );

Console.WriteLine($"Send message: {message}");
