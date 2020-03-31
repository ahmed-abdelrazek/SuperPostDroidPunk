using Newtonsoft.Json;
using System.IO;
using System.Xml;

namespace SuperPostDroidPunk.Extensions
{
    public static partial class XmlExtensions
    {
        /// <summary>
        /// Takes a Json as string and turn it into XML
        /// </summary>
        /// <param name="json">Json content as string</param>
        /// <param name="rootName">XML root name</param>
        /// <param name="rootPropertyName">Xml root property Name</param>
        /// <returns>Xml Document</returns>
        public static XmlDocument DeserializeXmlNode(string json, string rootName, string rootPropertyName)
        {
            return DeserializeXmlNode(new StringReader(json), rootName, rootPropertyName);
        }

        public static XmlDocument DeserializeXmlNode(TextReader textReader, string rootName, string rootPropertyName)
        {
            var prefix = "{" + JsonConvert.SerializeObject(rootPropertyName) + ":";
            var postfix = "}";

            using var combinedReader = new StringReader(prefix).Concat(textReader).Concat(new StringReader(postfix));
            var settings = new JsonSerializerSettings
            {
                Converters = { new Newtonsoft.Json.Converters.XmlNodeConverter() { DeserializeRootElementName = rootName } },
                DateParseHandling = DateParseHandling.None,
            };
            using var jsonReader = new JsonTextReader(combinedReader) { CloseInput = false, DateParseHandling = DateParseHandling.None };
            return JsonSerializer.CreateDefault(settings).Deserialize<XmlDocument>(jsonReader);
        }

        public static string AsString(this XmlDocument xmlDoc)
        {
            using StringWriter sw = new StringWriter();
            using XmlTextWriter tx = new XmlTextWriter(sw);
            tx.Formatting = System.Xml.Formatting.Indented;
            xmlDoc.WriteTo(tx);
            string strXmlText = sw.ToString();
            return strXmlText;
        }
    }
}
