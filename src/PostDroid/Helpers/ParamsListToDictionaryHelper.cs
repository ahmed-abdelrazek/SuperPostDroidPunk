using SuperPostDroidPunk.Models;
using System.Collections.Generic;

namespace SuperPostDroidPunk.Helpers
{
    public class ParamsListToDictionaryHelper
    {
        /// <summary>
        /// Takes a list of type Param and add it to chosen Dictionary<string, string>
        /// </summary>
        /// <param name="list">List<Param></param>
        /// <param name="dictionary">Dictionary you want to add the params to</param>
        public static void ListToDictionary(IEnumerable<Param> list, Dictionary<string, string> dictionary)
        {
            foreach (var item in list)
            {
                if (item == null)
                {
                    continue;
                }
                if (string.IsNullOrEmpty(item.Key))
                {
                    //todo add message to warn user
                    continue;
                }
                if (string.IsNullOrEmpty(item.Value))
                {
                    item.Value = string.Empty;
                }
                if (item.IsSelected)
                {
                    dictionary.Add(item.Key, item.Value);
                }
            }
        }

        /// <summary>
        /// Takes a list of type Param and add it to chosen Dictionary<string, string>
        /// </summary>
        /// <param name="list">List<Param></param>
        /// <param name="dictionary">Dictionary you want to add the params to</param>
        public static void DictionaryToList(Dictionary<string, string> dictionary, IEnumerable<Param> list)
        {
            foreach (var item in list)
            {
                if (!item.IsSelected)
                {
                    continue;
                }

                dictionary.Add(item.Key, item.Value);
            }
        }
    }
}
