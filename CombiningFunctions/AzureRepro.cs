using System.Text;
using System.Text.Json.Nodes;

namespace CombiningFunctions
{
    internal class AzureRepro
    {
        public static async Task Test()
        {
            /*
            // Azure OpenAI Implementation
            var api_key = "<YOUR AZURE KEY>";
            var baseUrl = "<YOUR AZURE ENDPOINT>";
            var deploymentName = "<YOUR DEPLOYMENT NAME>";
            var url = baseUrl + "/openai/deployments/" + deploymentName + "/chat/completions?api-version=2023-05-15";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("api-key", api_key);
            var model = "gpt-35-turbo";

            //  THIS CODE DOESN'T WORK - having tried lots of permutations of api-version and model

            //    NotFound

            //    {
            //      "error": {
            //        "message": "Unrecognized request argument supplied: functions",
            //        "type": "invalid_request_error",
            //        "param": null,
            //        "code": null
            //      }
            //    }


            */

            // OpenAI Implementation - THIS CODE WORKS FINE
            var api_key = "<YOUR OPENAI API-KEY>";
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            request.Headers.Add("Authorization", "Bearer " + api_key);
            var model = "gpt-3.5-turbo-0613";

            var utterance = "hi";

            var messages = new JsonArray
            {
                new JsonObject { { "role", "system" }, { "content", "Don't make assumptions about what values to plug into functions. Ask for clarification if a user request is ambiguous." } },
                new JsonObject { { "role", "user" }, { "content", utterance } }
            };

            var functions = JsonNode.Parse(File.ReadAllText("descriptions.json")) ?? throw new Exception("unable to read descriptions");

            var json = new JsonObject
            {
                { "model", model },
                { "messages", messages },
                { "functions", functions },
            };

            request.Content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");

            var client = new HttpClient();
            var response = await client.SendAsync(request);

            Console.WriteLine(response.StatusCode);
            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }
    }
}
