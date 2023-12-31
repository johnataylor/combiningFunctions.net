﻿using CombiningFunctions;
using System.Text.Json.Nodes;

var model = "gpt-3.5-turbo-0613";
var api_key = "";


var functionDescriptions = JsonNode.Parse(File.ReadAllText("descriptions.json")) ?? throw new Exception("unable to read descriptions");

//await Test.TestAsync("hi", model, api_key, functionDescriptions, null);
//await Test.TestAsync("what is the creation date for work order 00052?", model, api_key, functionDescriptions, null);
//await Test.TestAsync("I would like to book a flight from London to Paris", model, api_key, functionDescriptions, null);

var functionImplementations = new Dictionary<string, Func<JsonNode, JsonNode>>
{
    //{ "get_work_order_details", get_work_order_details },
    { "get_multiple_work_order_details", arguments => mapcar(arguments["work_order_ids"]?.AsArray(), get_work_order_details) },
    { "get_work_orders_by_account", get_work_orders_by_account },
};

var result = await Resolver.Run("What is the summary for work order 52?", model, api_key, functionDescriptions, functionImplementations);
//var result = await Resolver.Run("what are the 'in progress' work orders for account 01234?", model, api_key, functionDescriptions, functionImplementations);
Console.WriteLine(result);

// this illustrates that "functions" replace prompts that were designed to extract structure
//await Test.TestAsync($"Create a work order from the following email ```{CreateEmail()}```", model, api_key, functionDescriptions, null);

//await AzureRepro.Test();

JsonNode get_work_order_details(JsonNode? arguments)
{
    var work_order_id = arguments?["work_order_id"]?.GetValue<string>() ?? throw new InvalidDataException("expected work_order_id");

    work_order_id = work_order_id.PadLeft(5, '0');

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

JsonNode mapcar(JsonArray? array, Func<JsonNode?, JsonNode> func)
{
    array = array ?? throw new ArgumentNullException("array");
    return new JsonArray(array.Select((element, Index) => func(element)).ToArray());
}

JsonNode get_work_orders_by_account(JsonNode arguments)
{
    return new JsonArray { new JsonObject { { "work_order_id", "00052" } }, new JsonObject { { "work_order_id", "00042" } }, new JsonObject { { "work_order_id", "52341" } } };
}

string CreateEmail()
{
    return @"
Subject: Denver Hospital MRI Machine
From: Kaitlyn (Denver Hospital Adminstration)    
To: Bob (Woodgrove Medical Systems)    
  
Dear Bob,    
I would like to schedule the annual inspection of our MRI machine.
  
It's coming up to 12 month so there is some urgency as we can't afford any downtime.
  
Apologies for not getting this sorted out weeks ago but we've had some staff turn over.

Looking forward to hearing back from you.    
  
Best regards,    
Kaitlyn";
}
