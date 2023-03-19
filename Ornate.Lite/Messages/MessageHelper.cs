using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;

namespace Ornate.Lite.Messages
{
    public static class MessageHelper
    {
        //TODO: turn object into message base class
        public static bool TryGetRequest(Uri uri, string postData, out object message)
        {
            message = null;
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var attrib = type.GetCustomAttribute<RequestAttribute>();
                if (attrib == null)
                    continue;

                if (!attrib.Path.Equals(uri.LocalPath))
                    continue;

                // Found class
                // Create instance
                var parsed = Activator.CreateInstance(type);
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
            return false;
        }

        public static bool TryGetResponse(Uri uri, string data, out object message) 
        {
            message = null;
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var attrib = type.GetCustomAttribute<ResponseAttribute>();
                if (attrib == null)
                    continue;

                if (!attrib.Path.Equals(uri.LocalPath))
                    continue;

                // Found class
                var parsed = JsonSerializer.Deserialize(data, type);
                message = parsed;
                return true;
            }
            return false;
        }
    }
}
