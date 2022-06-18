# chat-app

The Chat-App is a live chatting app built with Web Sockets with support for multiple users and bots commands in the chat.

## Stocks Bot
A bot built to get stocks quotations in real time using the following command in the chat:
```
/stocks={stock_code} //ex:APPL.US
```

## Stack
What you need to run this project:
- .Net 5.0 
- PostgreSQL
- RabbitMQ

## How to run
To run the project locally, you can use the docker compose to set up the environment:
``` 
docker-compose up
```
Once you have the database and the RabbitMQ Server running, run the following commands from the project root folder:
``` 
cd StocksBot
dotnet run
```

``` 
cd ChatWSServer
dotnet run
```

After the backend is setup, you can open the ``ChatFrontend/index.html`` in your browser and start using the application.

## Tests

Run tests with the following command in the **/ChatBackend** folder:
``` 
dotnet test
```

## Multiple Bots Support
The Web Sockets Server was built to support multiple bots, so in the if a new bot was created, it would only be needed to add a new input in **BotList** the **BotManager** class.

Example:
```c#
new Bot(name: "weather-bot", prefix: "/weather=", queue: "weather-queue")
```