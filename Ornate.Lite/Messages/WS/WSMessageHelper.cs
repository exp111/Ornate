using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;

namespace Ornate.Lite.Messages.WS
{
    public static class WSMessageHelper
    {
        // Saves the ws request scopes and their corresponding request method
        public static Dictionary<string, string> WSRequests = new();
        //TODO: this only caches when a request is triggered and also dies on duplicate scopes with diff methods => parse all msgs on listboxRefresh

        // Tries to find and parse a websocket message that matches data
        public static bool TryGetMessage(Direction direction, string payloadData, out object message)
        {
            message = null;
            try
            {
                //TODO: can we merge this somehow into the JsonSerializer?
                if (direction == Direction.Sent)
                {
                    var parsed = JsonSerializer.Deserialize<BaseWSMessage>(payloadData);
                    message = parsed;
                    // input into dict
                    WSRequests[parsed.Scope] = parsed.Method;
                    // find data type with message.method //TODO: handle stuff that has no data
                    var types = Assembly.GetExecutingAssembly().GetTypes();
                    foreach (var type in types)
                    {
                        var attrib = type.GetCustomAttribute<WSMessageDataAttribute>();
                        if (attrib == null)
                            continue;

                        // Check if the scope matches
                        if (!attrib.Method.Equals(parsed.Method))
                            continue;

                        // Found class, json deserialize the data
                        var data = JsonSerializer.Deserialize((JsonElement)parsed.Data, type);
                        parsed.Data = data;
                    }
                }
                else
                {
                    var parsed = JsonSerializer.Deserialize<BaseWSResponse>(payloadData);
                    message = parsed;
                    if (WSRequests.TryGetValue(parsed.Scope, out var method))
                    {
                        // find data type with requestMethod
                        var types = Assembly.GetExecutingAssembly().GetTypes();
                        foreach (var type in types)
                        {
                            var attrib = type.GetCustomAttribute<WSResponseDataAttribute>();
                            if (attrib == null)
                                continue;

                            // Check if the method matches
                            if (!attrib.RequestMethod.Equals(method))
                                continue;

                            // Found class, json deserialize the data
                            var data = JsonSerializer.Deserialize((JsonElement)parsed.Result, type);
                            parsed.Result = data;
                        }
                    }
                }
                return true;
            }
            catch (Exception _)
            {

            }
            return false;
        }
    }
}
