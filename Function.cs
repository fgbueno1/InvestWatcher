using Amazon.Lambda.Core;
using Newtonsoft.Json.Linq;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace InvestWatch;

public class Function
{
    private Dictionary<string, List<(string, double)>> coins = new Dictionary<string, List<(string, double)>>
    {
        {"BTC", new List<(string, double)>{("USDT", 86000)}},
        {"USDT", new List <(string, double)>{("BRL", 6)}},
        {"EURC", new List<(string, double)>{("BRL", 6.15)}}
    };

    private string telegramUsername = "@your_username";
    public async Task FunctionHandler(ILambdaContext context)
    {
        bool mention = false;
        var botToken = Environment.GetEnvironmentVariable("TelegramBotID"); ;
        var chatId = Environment.GetEnvironmentVariable("ChatID"); ;
        string message = "";

        foreach (var coin in coins)
        {
            var values = coin.Value[0];
            var price = await GetCryptoPriceAsync(coin.Key, values.Item1);
            message += $"{coin.Key} current price: {values.Item1} {price}\n\n";
            if (price <= values.Item2)
            {
                mention = true;
            }
        }
        if (mention)
        {
            message += telegramUsername;
        }
        #pragma warning disable CS8604
        await SendMessageToTelegramAsync(botToken, chatId, message);
        #pragma warning restore CS8604
    }

    public async Task<double> GetCryptoPriceAsync(string cryptoSymbol, string destCurrency)
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync($"https://api.coinbase.com/v2/exchange-rates?currency={cryptoSymbol}");     
        var jsonString = await response.Content.ReadAsStringAsync();
        var jsonObject = JObject.Parse(jsonString);
        #pragma warning disable CS8602 // Dereference of a possibly null reference.
        double price = Convert.ToDouble(jsonObject["data"]["rates"][destCurrency].ToString());
        #pragma warning restore CS8602 // Dereference of a possibly null reference.

        return Math.Round(price, 2);
    }

    public async Task SendMessageToTelegramAsync(string botToken, string chatId, string message)
    {
        using var httpClient = new HttpClient();
        var url = $"https://api.telegram.org/bot{botToken}/sendMessage?chat_id={chatId}&text={message}";
        await httpClient.GetAsync(url);
    }
}
