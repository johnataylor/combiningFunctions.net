using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace CombiningFunctions
{
    internal class Test
    {
        public static async Task TestAsync(string utterance, string model, string api_key, JsonNode? functions, JsonNode? functionCall)
        {
            var messages = new JsonArray
            {
                new JsonObject { { "role", "system" }, { "content", "Don't make assumptions about what values to plug into functions. Ask for clarification if a user request is ambiguous." } },
                new JsonObject { { "role", "user" }, { "content", utterance } }
            };

            var result = await OpenAIClient.ChatCompletionRequestAsync(model, api_key, messages, functions, functionCall);

            var choice = result["choices"]?[0] ?? throw new InvalidDataException();

            var finishReason = choice["finish_reason"]?.GetValue<string>() ?? throw new InvalidDataException();

            // the stop reason seems to indicate that the model has found a function call that matches the description we gave it
            if (finishReason == "function_call")
            {
                var name = choice["message"]?["function_call"]?["name"]?.GetValue<string>() ?? throw new InvalidDataException();

                Console.WriteLine($"name: {name}");

                var arguments = choice["message"]?["function_call"]?["arguments"]?.GetValue<string>();

                // yes "arguments" is a string - it's actually a string of json - a little quirky perhaps

                if (arguments != null)
                {
                    var argumentsObject = JsonNode.Parse(arguments) ?? throw new InvalidDataException("expected arguments to contain JSON");

                    Console.WriteLine($"arguments: {argumentsObject}");
                }
            }

            // and sometimes we might ALSO get content
            var content = choice["message"]?["content"]?.GetValue<string>();
            if (content != null)
            {
                Console.WriteLine($"content: {content}");
            }
        }
    }
}
