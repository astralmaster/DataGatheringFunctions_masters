using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HandlersAndModels.Handlers
{
    public static class GeneralHandlers
    {

        public static int ExtractIdFrom(string text, string pattern)
        {

            int count = 0;
            int i = 0;
            while ((i = text.IndexOf(pattern, i, StringComparison.InvariantCulture)) != -1)
            {
                i += pattern.Length;
                if (i > text.Length) break;
                count++;
            }
            return count;
        }

        public static int CountStringOccurrences(string text, string pattern)
        {

            int count = 0;
            int i = 0;
            while ((i = text.IndexOf(pattern, i, StringComparison.InvariantCulture)) != -1)
            {
                i += pattern.Length;
                if (i > text.Length) break;
                count++;
            }
            return count;
        }

        public static bool IsValidJson(string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput)) { return false;}
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    Console.WriteLine(jex.Message);
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

    }
}
