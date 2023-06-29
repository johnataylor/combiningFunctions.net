using System.Text.Json.Nodes;

namespace CombiningFunctions
{
    internal class Resolver
    {
        private const int MAX_ITERATIONS = 10;

        public static async Task<string> Run(string utterance, string model, string api_key, JsonNode functionDescriptions, IDictionary<string, Func<JsonNode, JsonNode>> functionImplementations)
        {
            var messages = new JsonArray
            {
                new JsonObject { { "role", "system" }, { "content", "Don't make assumptions about what values to plug into functions. Ask for clarification if a user request is ambiguous." } },
                new JsonObject { { "role", "user" }, { "content", utterance } }
            };

            for (int i=0; i < MAX_ITERATIONS; i++)
            {
                var result = await OpenAIClient.ChatCompletionRequestAsync(model, api_key, messages, functionDescriptions, null);

                var choice = result["choices"]?[0] ?? throw new InvalidDataException();

                messages.Add(JsonNode.Parse(choice["message"]?.ToJsonString() ?? throw new InvalidDataException()));

                var finishReason = choice["finish_reason"]?.GetValue<string>() ?? throw new InvalidDataException();

                // finish reason appears to indicate the model has determined a function should be called 
                if (finishReason == "function_call")
                {
                    var functionName = choice["message"]?["function_call"]?["name"]?.GetValue<string>() ?? throw new InvalidDataException();

                    var arguments = choice["message"]?["function_call"]?["arguments"]?.GetValue<string>();
                    if (arguments != null)
                    {
                        // arguments is a string of JSON embedded in a property of type string
                        var argumentsArguments = JsonNode.Parse(arguments) ?? throw new InvalidDataException("expected arguments to contain JSON");

                        if (functionImplementations.TryGetValue(functionName, out var func))
                        {
                            var functionResponse = func(argumentsArguments);

                            messages.Add(new JsonObject { { "role", "user" }, { "content", $"result from function:```{functionResponse}```" } });
                        }
                        else
                        {
                            throw new InvalidDataException($"unable to answer the question because function '{functionName}' doesn't exist");
                        }
                    }
                }
                else
                {
                    // TODO: check the finish reason is stop not length!

                    return choice["message"]?["content"]?.GetValue<string>() ?? throw new InvalidDataException();
                }
            }

            return "unable to answer the question";
        }
    }
}
