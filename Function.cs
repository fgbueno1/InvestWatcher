using Amazon.Lambda.Core;
using Newtonsoft.Json.Linq;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace InvestWatch;

public class Function
{
    private readonly Dictionary<string, List<(string, double)>> coins = new()
    {
        {"BTC", new List<(string, double)>{("USDT", 86000)}},
        {"USDT", new List <(string, double)>{("BRL", 6)}},
        {"EURC", new List<(string, double)>{("BRL", 6.15)}}
    };

    private readonly string telegramUsername = "@Cr0wn_T";

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
            var values = coin.Value[0];
            var price = await GetCryptoPriceAsync(coin.Key, values.Item1);
            _bot.message += $"{coin.Key} current price: {values.Item1} {price}\n\n";
            if (price <= values.Item2)
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
