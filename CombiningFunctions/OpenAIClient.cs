using System;
using System.Text;
using System.Text.Json.Nodes;
using static System.Net.WebRequestMethods;

namespace CombiningFunctions
{
    internal class OpenAIClient
    {
        public static async Task<JsonObject> ChatCompletionRequestAsync(string model, string api_key, JsonArray messages, JsonNode? functions = null, JsonNode? functionCall = null)
        {
            // OpenAI

            //var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            //request.Headers.Add("Authorization", "Bearer " + api_key);

            // Azure OpenAI

            var baseUrl = "https://frontlinegpt.openai.azure.com/";
            var deploymentName = "testfunctions";
            var url = baseUrl + "/openai/deployments/" + deploymentName + "/chat/completions?api-version=2023-07-01-preview";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("api-key", api_key);

            var json = new JsonObject
            {
                //{ "model", model }, // not needed on Azure (because its governed by the deployment)
                { "messages", JsonNode.Parse(messages.ToJsonString()) },
            };

            
            if (functions != null)
            {
                json.Add("functions", JsonNode.Parse(functions.ToJsonString()));
            }
            

            /*
            if (functionCall != null)
            {
                json.Add("function_call", JsonNode.Parse(functionCall.ToJsonString()));
            }
            */
            
            request.Content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");

            var client = new HttpClient();
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(errorResponse);
            }

            var result = await response.Content.ReadAsStringAsync() ?? throw new InvalidDataException();

            return (JsonObject)(JsonNode.Parse(result) ?? throw new InvalidDataException());
        }         
    }
}
