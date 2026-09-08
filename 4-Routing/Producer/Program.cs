/*
  Sì, hai capito correttamente. Solo una correzione nell’ultima frase: con "both" ricevono AnalyticsConsumer 
  e PaymentsConsumer. Il Producer invia soltanto.
  
  Nel codice attuale il percorso è:
     Producer → exchange "routing" → coda → consumer

  RabbitMQ instrada quindi il messaggio verso le code, dalle quali i consumer lo ricevono.
    1. AnalyticsConsumer crea una propria coda e, con due QueueBindAsync, 
       la collega all’exchange per ricevere messaggi con chiave "analyticsonly" oppure "both".
    2. PaymentsConsumer crea un’altra coda e la collega allo stesso exchange per ricevere 
       messaggi con chiave "paymentsonly" oppure "both".
    3. Producer pubblica un messaggio sull’exchange "routing", di tipo direct. 
       L’exchange confronta la routing key del messaggio con quelle dei binding:

*/


using System.Text;
using RabbitMQ.Client;






var factory = new ConnectionFactory() { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.ExchangeDeclareAsync(
    exchange: "routing",
    type: ExchangeType.Direct
);

var message = "This message needs to be routed";
var body = Encoding.UTF8.GetBytes(message);

/*
 mandatory decide cosa succede quando il messaggio non può raggiungere nessuna coda:
- false: RabbitMQ lo scarta, salvo un alternate exchange configurato.
- true: RabbitMQ lo restituisce al producer, che deve gestire il ritorno tramite BasicReturnAsync.
*/
await channel.BasicPublishAsync(
    exchange: "routing",         // Nome dell'exchange a cui pubblicare il messaggio.
    routingKey: "analyticsonly",  // Con un exchange direct, deve coincidere con la chiave del binding della coda.
    mandatory: false,            // Se nessuna coda e' raggiungibile, scarta il messaggio (salvo alternate exchange configurato).
    body: body                   // Contenuto del messaggio in byte, qui ottenuti dalla stringa tramite UTF-8.
);

Console.WriteLine($"Send message: {message}");
