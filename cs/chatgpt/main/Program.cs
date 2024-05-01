using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class HttpHandler {
        public static async Task<object> SendRequest(string apiKey, string URL, object content) {
            var client = new HttpClient();
            var jsonContent = JsonConvert.SerializeObject(content);
            var jsonStringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            try {
                var response = await client.PostAsync(URL, jsonStringContent);

                if (response.IsSuccessStatusCode) {
                    var data = await response.Content.ReadAsStringAsync();
                    var transformedData = JsonConvert.DeserializeObject(data);

                    return transformedData ?? new { error = true };
                }

                Console.WriteLine($"Something went wrong while sending the request. Status code: {response}");

                return new { error = true };

            } catch (Exception ex) {
                Console.WriteLine($"An error occurred: {ex.Message}");

                return new { error = true };
            }
    }
}

class Program {
    static async Task Main() {
       Env.SetEnvironmentVariables();

       var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY", EnvironmentVariableTarget.User);
       var URL = Environment.GetEnvironmentVariable("OPENAI_URL", EnvironmentVariableTarget.User);

       Console.WriteLine("Feel free to start the conversation! To end, enter 'stop' or hit CTRL+C \n");

       string? request = null;
       List<object> messages = [];

       while (true) {
            Console.Write("You: ");
            request = Console.ReadLine();
            Console.WriteLine();

            if (string.IsNullOrWhiteSpace(request)) {
                Console.WriteLine("Please enter some input");
                continue;
            }

            if (request.ToLower() == "stop") {
                break;
            }

            messages.Add(new { role = "user", content = request });

            var content = new { model = "gpt-3.5-turbo", messages };
            dynamic response = await HttpHandler.SendRequest(apiKey!, URL!, content);

            if(response.error == true ) {
                break;
            }

            var data = response.choices[0].message;

            messages.Add(data);

            Console.WriteLine($"OpenAi: {data.content}");
            Console.WriteLine();
       }
    }
}