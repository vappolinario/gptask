using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

if (args.Length > 0)
{
    try
    {
        var payload = CreatePayload(args[0]);
        var result = GetCompletion(payload).Result;

        var dyData = JsonConvert.DeserializeObject<dynamic>(result);
        string text = dyData!.choices[0].text;
        var guess = GuessCommand(text);

        Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine($"{guess}");
        Console.ResetColor();
    }
    catch (System.Exception ex)
    {
        System.Console.WriteLine(ex.Message);
    }
}
else
{
    Console.WriteLine("usage: ask 'pergunta?'");
}

static StringContent CreatePayload(string prompt)
{
    const string GPT_MODEL = "text-davinci-001";
    var content = new StringContent(
            JsonConvert.SerializeObject(new
            {
                temperature = 1,
                max_tokens = 100,
                model = GPT_MODEL,
                prompt = prompt
            }),
    System.Text.Encoding.UTF8, "application/json");
    return content;
}

static async Task<string> GetCompletion(StringContent content)
{
    var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
    var secretProvider = config.Providers.First();
    secretProvider.TryGet("Openai:ServiceApiKey", out var apiKey);

    using (var client = new HttpClient())
    {
        client.DefaultRequestHeaders.Add("authorization", $"Bearer {apiKey}");
        var response = await client.PostAsync("https://api.openai.com/v1/completions", content);
        var responseString = await response.Content.ReadAsStringAsync();
        return responseString;
    }
}

static string GuessCommand(string raw)
{
    var guess = raw.Substring(raw.LastIndexOf('\n') + 1);
    TextCopy.ClipboardService.SetText(guess);
    return guess;
}

