using Ornate.Lite.Messages.WS;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace Ornate.Lite.Messages
{
    public static class MessageHelper
    {
        // Tries to find and parse a request that matches the uri and data
        public static bool TryGetRequest(Uri uri, string postData, out object message)
        {
            message = null;
            try
            {
                // Iterate through all types and check the request attribute
                var types = Assembly.GetExecutingAssembly().GetTypes();
                foreach (var type in types)
                {
                    var attrib = type.GetCustomAttribute<RequestAttribute>();
                    if (attrib == null)
                        continue;

                    // Check if the path matches
                    if (!attrib.Path.Equals(uri.LocalPath))
                        continue;

                    // Found class
                    // Create instance
                    var parsed = (IRequest)Activator.CreateInstance(type);
                    // Parse queries
                    var query = HttpUtility.ParseQueryString(uri.Query);
                    NameValueCollection postQuery = null;
                    if (postData != null)
                        postQuery = HttpUtility.ParseQueryString(postData);

                    // Go through each property and get value from query
                    foreach (var member in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        var propNameAttrib = member.GetCustomAttribute<JsonPropertyNameAttribute>();
                        if (propNameAttrib == null)
                            continue;

                        var propName = propNameAttrib.Name;
                        // force read from url query
                        var forceParameter = member.GetCustomAttribute<ForceParameterAttribute>() != null;

                        // Get value from query or postdata
                        string value;
                        if (attrib.Method == RequestMethod.GET || forceParameter)
                            value = query[propName];
                        else
                            value = postQuery[propName];
                        // Cast into real type
                        var casted = Convert.ChangeType(value, member.PropertyType);
                        // Set value in member
                        member.SetValue(parsed, casted);
                    }
                    message = parsed;
                    return true;
                }
            }
            catch (Exception _)
            {

            }
            return false;
        }

        // Tries to find and parse a response that matches the uri and data
        public static bool TryGetResponse(Uri uri, string data, out object message) 
        {
            message = null;
            try
            {
                // Iterate through all types and check the response attribute
                var types = Assembly.GetExecutingAssembly().GetTypes();
                foreach (var type in types)
                {
                    var attrib = type.GetCustomAttribute<ResponseAttribute>();
                    if (attrib == null)
                        continue;

                    // Check if the path matches
                    if (!attrib.Path.Equals(uri.LocalPath))
                        continue;

                    // Found class, json deserialize the data
                    var parsed = (IResponse)JsonSerializer.Deserialize(data, type);
                    message = parsed;
                    return true;
                }
            }
            catch (Exception _)
            {

            }
            return false;
        }

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
