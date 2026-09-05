using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// Configura l'accesso al broker sul computer locale, con porta AMQP predefinita 5672.
// La factory contiene le impostazioni; la connessione viene aperta nella chiamata seguente.
var factory = new ConnectionFactory() { HostName = "localhost" };

// Apre la connessione di rete in modo asincrono. await restituisce la connessione
// quando è pronta, senza bloccare il thread durante l'attesa.
// using var rilascia la risorsa quando il programma termina o esce per un'eccezione.
using var connection = await factory.CreateConnectionAsync();

// Apre un canale logico sulla stessa connessione per dichiarare code e ricevere messaggi.
// In RabbitMQ.Client 7 sostituisce CreateModel() della versione 6.
// Il canale viene rilasciato prima della connessione.
using var channel = await connection.CreateChannelAsync();

// Dichiara la coda anche qui, così il Consumer può partire prima del Producer.
// La chiamata è ASINCRONA: await attende la risposta prima di registrare il consumer.
// Se la coda esiste con proprietà equivalenti, non viene duplicata né svuotata.
// Le proprietà devono essere compatibili con quelle dichiarate dal Producer;
// una differenza, ad esempio su durable, causa PRECONDITION_FAILED sul canale.
await channel.QueueDeclareAsync(
    // Nome della coda da cui ricevere, uguale a quello usato dal Producer.
    // Con "" il server genera un nome, recuperabile dal risultato della dichiarazione.
    queue: "letterbox",
    // false: la coda non sopravvive al riavvio del broker.
    // true: rende durevole la coda, non automaticamente i suoi messaggi;
    // per conservarli al riavvio occorre anche pubblicarli come persistenti.
    durable: false,
    // false: consente l'accesso da connessioni diverse.
    // true: riserva la coda alla connessione che la dichiara e la elimina alla sua chiusura.
    exclusive: false,
    // false: la coda resta quando l'ultimo consumer si disconnette.
    // true: la elimina alla scomparsa dell'ultimo consumer, dopo averne avuto almeno uno.
    // Non significa "elimina quando la coda è vuota".
    autoDelete: false,
    // Dizionario di opzioni aggiuntive, ad esempio x-message-ttl (millisecondi),
    // x-max-length o x-queue-type. null non invia opzioni aggiuntive:
    // impostazioni predefinite e policy del broker restano applicabili.
    arguments: null
);

// Crea il gestore degli eventi di consegna associato al canale.
// Non avvia ancora la ricezione: la sottoscrizione avviene con BasicConsumeAsync.
var consumer = new AsyncEventingBasicConsumer(channel);

// Registra il codice da eseguire per ogni messaggio ricevuto.
// model è il mittente dell'evento; ea contiene corpo e metadati della consegna.
// Il gestore è dichiarato async, ma qui non contiene await: il compilatore
// può segnalare CS1998. Le istruzioni interne attuali sono sincrone.
consumer.ReceivedAsync += async (model, ea) =>
{
    // Copia i byte ricevuti in un array di proprietà dell'applicazione.
    // In versione 7 la memoria di ea.Body non va conservata oltre il gestore
    // senza copiarla; qui viene copiata e decodificata prima di uscire.
    var body = ea.Body.ToArray();
    // Usa la stessa codifica UTF-8 scelta dal Producer.
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($"Recieved new message: {message}");
};

// Registra il consumer sul broker: da questo momento riceve le consegne.
// await attende la registrazione, non la fine di tutti i messaggi.
await channel.BasicConsumeAsync(
    // Coda da cui il broker preleva i messaggi da consegnare.
    queue: "letterbox",
    // true: conferma automatica; il broker considera il messaggio consegnato
    // senza aspettare che questo gestore lo elabori correttamente.
    // Un errore durante l'elaborazione può quindi causare la perdita del messaggio.
    // Con false servirebbero conferme esplicite (BasicAckAsync) dopo il successo
    // e una gestione degli errori, ad esempio con BasicNackAsync.
    autoAck: true,
    // Oggetto che riceve le notifiche tramite ReceivedAsync.
    consumer: consumer
);

// Mantiene aperta la console e quindi attivi connessione, canale e ricezione.
// Premendo un tasto il programma termina e i using rilasciano le risorse.
Console.ReadKey();
