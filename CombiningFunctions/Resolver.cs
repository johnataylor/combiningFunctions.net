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

                // add to the transcript
                //messages.Add(JsonNode.Parse(choice["message"]?.ToJsonString() ?? throw new InvalidDataException()));

                var finishReason = choice["finish_reason"]?.GetValue<string>() ?? throw new InvalidDataException();

                // finish reason appears to indicate the model has determined a function should be called 
                if (finishReason == "function_call")
                {
                    var functionName = choice["message"]?["function_call"]?["name"]?.GetValue<string>() ?? throw new InvalidDataException();

                    var arguments = choice["message"]?["function_call"]?["arguments"]?.GetValue<string>();

                    // add to the transcript
                    //messages.Add(JsonNode.Parse(choice["message"]?.ToJsonString() ?? throw new InvalidDataException()));

                    var function_call = new JsonObject { { "name", functionName }, { "arguments", arguments } };
                    messages.Add(new JsonObject { { "role", "assistant" }, { "function_call", function_call }, { "content", "" } });
                    //messages.Add(new JsonObject { { "role", "assistant" }, { "function_call", function_call } });

                    if (arguments != null)
                    {
                        // arguments is a string of JSON embedded in a property of type string
                        var argumentsArguments = JsonNode.Parse(arguments) ?? throw new InvalidDataException("expected arguments to contain JSON");

                        if (functionImplementations.TryGetValue(functionName, out var func))
                        {
                            Trace($"function call:\n{functionName}('{argumentsArguments}')");

                            // call the function
                            var functionResponse = func(argumentsArguments);

                            Trace($"response:\n'{functionResponse}'");

                            // add the function result in the the messages array - note the content should be a string
                            messages.Add(new JsonObject { { "role", "function" }, { "name", functionName }, { "content", functionResponse.ToJsonString() } });
                        }
                        else
                        {
                            throw new InvalidDataException($"unable to answer the question because function '{functionName}' doesn't exist");
                        }
                    }
                }
                else
                {
                    // finish reason is not function_call so we must be done!

                    if (finishReason == "length")
                    {
                        throw new InvalidDataException("$unexpected finish reason length");
                    }

                    // add to the transcript
                    messages.Add(JsonNode.Parse(choice["message"]?.ToJsonString() ?? throw new InvalidDataException()));

                    var response = choice["message"]?["content"]?.GetValue<string>() ?? throw new InvalidDataException();

                    return response;
                }
            }

            Trace("reaching max iterations");

            return "unable to answer the question";
        }

        private static void Trace(string message)
        {
            var forgroundColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(message);
            Console.ForegroundColor = forgroundColor;
        }
    }
}
