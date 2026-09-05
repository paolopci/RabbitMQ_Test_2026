# Repository Guidelines

## Struttura del progetto

Il repository contiene un esercizio C# iniziale in `1-First-RabbitMQ-App/Producer/`:
- `Producer.csproj`: applicazione console destinata a `net10.0`, con nullable reference types e implicit usings abilitati.
- `Program.cs`: entry point con istruzioni top-level; attualmente stampa `Hello, World!`.
- `.gitignore`: esclude output di compilazione, file IDE e `.env`.

Non sono presenti solution, progetti di test, asset frontend o configurazioni Docker. Nonostante il nome del repository, il progetto non contiene ancora dipendenze o integrazioni RabbitMQ.

## Comandi di sviluppo

Eseguire dalla radice con un SDK .NET 10 disponibile:

```powershell
dotnet build .\1-First-RabbitMQ-App\Producer\Producer.csproj
dotnet run --project .\1-First-RabbitMQ-App\Producer\Producer.csproj --no-build
```

Il primo comando compila ed esegue il ripristino implicito; il secondo avvia la build esistente. Concordare prima eventuali installazioni di SDK o nuove dipendenze. Non eseguire comandi sulla sola radice aspettandosi una solution.

## Stile e nomi

Per il nuovo codice C#, usare quattro spazi, `PascalCase` per tipi e membri pubblici, `camelCase` per parametri e variabili locali. Mantenere semplice l'entry point e usare file con nomi corrispondenti ai tipi introdotti. Rispettare la nullability abilitata; usare il suffisso `Async` per metodi asincroni. Il progetto non configura formatter, linter o `.editorconfig` dedicati.

## Verifica e test

Non esistono framework di test o soglie di copertura configurati. Per modifiche al codice, compilare ed eseguire la console, verificando l'output atteso. Non presentare `dotnet test` senza test rilevati come prova di copertura. Quando autorizzati, preferire xUnit e nomi `Metodo_Scenario_RisultatoAtteso`; documentare il comando del nuovo progetto di test. Indicare sempre verifiche eseguite e limiti.

## Commit e pull request

La cronologia contiene un solo commit, `Inizializza repository con .gitignore per progetto .NET RabbitMQ`: non dimostra uno standard consolidato. Usare messaggi brevi, descrittivi e all'imperativo, coerenti con questo esempio. Nelle PR riportare scopo, file coinvolti, verifiche e issue collegate, se disponibili. Segnalare dipendenze aggiunte e cambiamenti incompatibili.

## Regole operative e configurazione

Preservare modifiche preesistenti. Richiedere permesso prima di eliminare contenuti, installare dipendenze o operare fuori dal progetto. Per cambiamenti significativi, preparare un piano e attenderne l'approvazione. Non inserire credenziali nel codice; prima di configurazioni sensibili, spiegarne l'impatto.
