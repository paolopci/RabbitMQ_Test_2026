# Repository Guidelines

## Struttura del progetto

Il repository contiene quattro esercizi con applicazioni console C#:
- `1-First-RabbitMQ-App/`: Producer e Consumer.
- `2-Competing-Consumers/`: Producer e Consumer.
- `3-Pub-Sub/`: Producer, FirstConsumer e SecondConsumer.
- `4-Routing/`: Producer, AnalyticsConsumer e PaymentsConsumer.

I dieci progetti usano `net10.0`, `RabbitMQ.Client` **7.2.2**, nullable reference types e implicit usings. Gli entry point sono nei rispettivi `Program.cs`. Non sono presenti solution o progetti di test.

## Comandi di sviluppo

Eseguire dalla radice con un SDK .NET 10 disponibile:

```powershell
dotnet build .\4-Routing\Producer\Producer.csproj
dotnet run --project .\4-Routing\Producer\Producer.csproj --no-build
```

Adattare il percorso al progetto interessato. La build esegue il ripristino implicito; `--no-build` richiede una build esistente. In `4-Routing`, con RabbitMQ su `localhost`, compilare e avviare prima entrambi i consumer, attendere la registrazione dei binding, poi avviare il producer.

## RabbitMQ e routing

- Usare le API v7 asincrone: `CreateChannelAsync`, `BasicPublishAsync` e `BasicConsumeAsync`; distinguere pubblicazione da registrazione del consumer.
- I gestori `ReceivedAsync` devono restituire un `Task`: usare `Task.CompletedTask` quando il lavoro e' sincrono.
- `QueueBindAsync` configura un collegamento exchange-coda; non pubblica messaggi.
- In `4-Routing`, l'exchange `routing` e' `direct`: Analytics accetta `analyticsonly` e `both`; Payments accetta `paymentsonly` e `both`. Le chiavi devono coincidere esattamente. `both` e' una chiave convenzionale, non speciale.
- `autoAck: true` conferma alla consegna, senza attendere l'elaborazione. `mandatory: false` consente lo scarto dei messaggi non instradabili, salvo alternate exchange configurato.

## Stile e nomi

Usare quattro spazi, `PascalCase` per tipi e membri pubblici, `camelCase` per parametri e variabili locali. Rispettare nullability e stile esistente; commentare i parametri accanto alle chiamate quando richiesto.

## Verifica e test

Compilare i progetti modificati. Distinguere compilazione da verifica di invio/ricezione sul broker; dichiarare le prove non eseguite. Non presentare `dotnet test` senza test rilevati come copertura. Per sole modifiche documentali, verificare contenuto e diff.

## Commit e pull request

Usare messaggi brevi, descrittivi e all'imperativo. Nelle PR riportare scopo, file coinvolti, verifiche, nuove dipendenze ed eventuali incompatibilita'.

## Regole operative e configurazione

Preservare modifiche preesistenti. Richiedere permesso prima di eliminare contenuti, installare dipendenze o operare fuori dal progetto. Per cambiamenti significativi, preparare un piano e attenderne l'approvazione. Non inserire credenziali nel codice; prima di configurazioni sensibili, spiegarne l'impatto.
