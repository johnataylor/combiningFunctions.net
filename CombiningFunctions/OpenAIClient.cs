using System.Text;
using System.Text.Json.Nodes;

namespace CombiningFunctions
{
    internal class OpenAIClient
    {
        public static async Task<JsonObject> ChatCompletionRequestAsync(string model, string api_key, JsonArray messages, JsonNode? functions = null, JsonNode? functionCall = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            request.Headers.Add("Authorization", "Bearer " + api_key);

            var json = new JsonObject
            {
                { "model", model },
                { "messages", JsonNode.Parse(messages.ToJsonString()) },
            };

            if (functions != null)
            {
                json.Add("functions", JsonNode.Parse(functions.ToJsonString()));
            }
            if (functionCall != null)
            {
                json.Add("function_call", JsonNode.Parse(functionCall.ToJsonString()));
            }

            request.Content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");

            var client = new HttpClient();
            var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync() ?? throw new InvalidDataException();

            return (JsonObject)(JsonNode.Parse(result) ?? throw new InvalidDataException());
        }         
    }
}
