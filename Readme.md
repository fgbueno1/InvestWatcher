# InvestWatcher

Invest Watcher is a simple and small code to run on AWS Lambda functions. Invest Watcher get the current exchange rates for cryptocurrencies and send it to a Telegram group chat using the Telegram Bot API.

To run the application you need to configure first which cyptocurrencies you want to track as well the Telegram Bot configurations, such as Bot Token to acceess the API and group chat where the message is going to be send.

## Configurating Cryptocurrencies

To configurate which cryptocurrencies you want to track you just need to add the cryptocurrency in the coins dictionary like the example below.

```csharp
private readonly Dictionary<string, List<(string, double)>> coins = new()
    {
        {"BTC", new List<(string, double)>{("USDT", 86000)}},
        {"USDT", new List <(string, double)>{("BRL", 6)}},
        {"EURC", new List<(string, double)>{("BRL", 6.15)}}
    };
```

The Dictionary Key is the cryptocurrency you want to track the exchange rate;

The Dictionary Value is a list where the first value (string) is the currency pair to know the exchange rate. The second value (double) is the price you want to get notified when the exchange rate is below it (example: I want to get notified when BTC is below 86K USDT).

## Configurating Telegram Bot

Follow the steps below to configure your telegram bot.

  1. Create a new bot on telegram using @botfather;
  2. Add your new bot to a group chat;
  3. Make sure the bot has the right permission on the group chat to read and send messages;
  4. Get your bot token ID and the group chat ID;
  5. Add your bot token ID and group chat ID as environment variables in your AWS lambda function (the bot token ID variable must be "TelegramBotID" and the group chat ID "ChatID").

## Configurating Notifications

To get your bot to mention you when some currency is below your configured exchange rate price just add your telegram username in the telegramUsername variable.

```csharp
private readonly string telegramUsername = "@your_username";
```
