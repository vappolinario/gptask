using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

const string GPT_MODEL = "text-davinci-001";

if (args.Length > 0)
{
    var content = new StringContent(
            JsonConvert.SerializeObject(new
            {
                temperature = 1,
                max_tokens = 100,
                model = GPT_MODEL,
                prompt = args[0]
            }),
    System.Text.Encoding.UTF8, "application/json");

    var result = GetCompletion(content).Result;

    try
    {
        var dyData = JsonConvert.DeserializeObject<dynamic>(result);
        string text = dyData!.choices[0].text;
        var guess = GuessCommand(text);
        Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine($"O Comando é {guess}");
        Console.ResetColor();

    }
    catch (System.Exception ex)
    {
        System.Console.WriteLine(ex.Message);
        return;
    }
}
else
{
    Console.WriteLine("usage: ask 'pergunta?'");
}

static async Task<string> GetCompletion(StringContent content)
{
    var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

    var secretProvider = config.Providers.First();
    secretProvider.TryGet("Openai:ServiceApiKey", out var apiKey);

    var client = new HttpClient();
    client.DefaultRequestHeaders.Add("authorization", $"Bearer {apiKey}");
    var response = await client.PostAsync("https://api.openai.com/v1/completions", content);
    var responseString = await response.Content.ReadAsStringAsync();
    return responseString;
}

static string GuessCommand(string raw)
{
    var guess = raw.Substring(raw.LastIndexOf('\n') + 1);
    TextCopy.ClipboardService.SetText(guess);
    return guess;
}

// namespace GptAsk
// {
//     public class GptContent
//     {
//         [JsonProperty("model")]
//         public string? Model { get; set; }
//         [JsonProperty("prompt")]
//         public string? Prompt { get; set; }
//         [JsonProperty("temperature")]
//         public int Temperature { get; set; }
//         [JsonProperty("max_tokens")]
//         public int MaxTokens { get; set; }
//     }
// }
