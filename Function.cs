using Amazon.Lambda.Core;
using Newtonsoft.Json.Linq;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace InvestWatch;

public class Function
{
    public class CoinsSetup
    {
        public required string coinPair;
        public required double coinPrice;
    }

    private readonly Dictionary<string, CoinsSetup> coins = new()
    {
        {"BTC", new CoinsSetup{coinPair="USDT", coinPrice=86000 }},
        {"USDT", new CoinsSetup{coinPair="BRL", coinPrice=5.90}},
        {"EURC", new CoinsSetup{coinPair="BRL", coinPrice=6.15 }}
    };

    private readonly string telegramUsername = "@your_username";

    public class TelegramBot
    {
        public string? botToken;
        public string? chatId;
        public string? message;
    }

    public async Task FunctionHandler()
    {
        bool mention = false;
        TelegramBot _bot = new()
        {
            botToken = Environment.GetEnvironmentVariable("TelegramBotID"),
            chatId = Environment.GetEnvironmentVariable("ChatID")
        };

        foreach (var coin in coins)
        {
            var price = await GetCryptoPriceAsync(coin.Key, coin.Value.coinPair);
            _bot.message += $"{coin.Key} current price: {coin.Value.coinPair} {price}\n\n";
            if (price <= coin.Value.coinPrice)
            {
                mention = true;
            }
        }
        if (mention)
        {
            _bot.message += telegramUsername;
        }
        await SendMessageToTelegramAsync(_bot);
    }

    private static async Task<double> GetCryptoPriceAsync(string cryptoSymbol, string destCurrency)
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync($"https://api.coinbase.com/v2/exchange-rates?currency={cryptoSymbol}");     
        var jsonString = await response.Content.ReadAsStringAsync();
        var jsonObject = JObject.Parse(jsonString);
        #pragma warning disable CS8602
        double price = Convert.ToDouble(jsonObject["data"]["rates"][destCurrency].ToString());
        #pragma warning restore CS8602

        return Math.Round(price, 2);
    }

    private static async Task SendMessageToTelegramAsync(TelegramBot _bot)
    {
        using var httpClient = new HttpClient();
        var url = $"https://api.telegram.org/bot{_bot.botToken}/sendMessage?chat_id={_bot.chatId}&text={_bot.message}";
        await httpClient.GetAsync(url);
    }
}
