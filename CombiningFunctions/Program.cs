// See https://aka.ms/new-console-template for more information
using CombiningFunctions;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

var model = "gpt-3.5-turbo-0613";
var api_key = "";


var functionDescriptions = JsonNode.Parse(File.ReadAllText("descriptions.json")) ?? throw new Exception("unable to read descriptions");

//await Test.TestAsync("hi", model, api_key, functionDescriptions, null);
//await Test.TestAsync("what is the creation date for work order 00052?", model, api_key, functionDescriptions, null);
//await Test.TestAsync("I would like to book a flight from London to Paris", model, api_key, functionDescriptions, null);

var functionImplementations = new Dictionary<string, Func<JsonNode, JsonNode>>
{
    { "get_work_order_details", get_work_order_details },
    { "get_work_orders_by_account", get_work_orders_by_account },
};

//var result = await Resolver.Run("What is the summary for work order 00052?", model, api_key, functionDescriptions, functionImplementations);
var result = await Resolver.Run("what are the 'in progress' work orders for account 01234?", model, api_key, functionDescriptions, functionImplementations);
Console.WriteLine(result);

JsonNode get_work_order_details(JsonNode arguments)
{
    Console.WriteLine($"get_work_order_details('{arguments}')");

    var work_order_id = arguments["work_order_id"]?.GetValue<string>();

    switch (work_order_id)
    {
        case "00052":
            return new JsonObject { { "createdOn", "06/22/2023" }, { "work_order_type", "installation" }, { "status", "in progress" }, { "summary", "install car tires" } };

        case "00042":
            return new JsonObject { { "createdOn", "06/22/2023" }, { "work_order_type", "repair" }, { "status", "pending" }, { "summary", "fix car" } };

        case "52341":
            return new JsonObject { { "createdOn", "06/22/2023" }, { "work_order_type", "installation" }, { "status", "in progress" }, { "summary", "tow hitch" } };

        default:
            return new JsonObject();
    }
}

JsonNode get_work_orders_by_account(JsonNode arguments)
{
    Console.WriteLine($"get_work_orders_by_account('{arguments}')");

    return new JsonArray { new JsonObject { { "work_order_id", "00052" } }, new JsonObject { { "work_order_id", "00042" } }, new JsonObject { { "work_order_id", "52341" } } };
}