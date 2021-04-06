using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace SuperPostDroidPunk.Extensions
{
    public static class JsonExtensions
    {
        public static bool IsValidJson(this string rawString, out JToken jToken)
        {
            if (!string.IsNullOrWhiteSpace(rawString))
            {
                rawString = rawString.Trim();
                if ((rawString.StartsWith("{") && rawString.EndsWith("}")) || (rawString.StartsWith("[") && rawString.EndsWith("]")))
                {
                    try
                    {
                        jToken = JToken.Parse(rawString);
                        return true;
                    }
                    catch (JsonReaderException jex)
                    {
                        jToken = "";
                        //Exception in parsing json
                        Console.WriteLine(jex.Message);
                        return false;
                    }
                    catch (Exception ex) //some other exception
                    {
                        jToken = "";
                        Console.WriteLine(ex);
                        return false;
                    }
                }
            }
            jToken = "";
            return false;
        }
    }
}
