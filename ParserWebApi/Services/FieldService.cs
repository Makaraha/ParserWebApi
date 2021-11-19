using HtmlAgilityPack;
using ParserWebApi.Services.ParserServices;
using System.Net;
using System.Text;

namespace ParserWebApi.Services
{
    // Сервис для обработки полей и текста
    public class FieldService
    {
        public int GetPageCount(HtmlDocument doc)
        {
            var itemsString = GetTextFromXPath("/html/body/form/div[4]/div[5]/div[2]/div[2]/div[3]", doc);
            itemsString = itemsString.Substring(13);

            int itemsCount = int.Parse(itemsString);

            return itemsCount % 30 == 0 ? itemsCount / 30 : itemsCount / 30 + 1;
        }

        public string GetTextFromXPath(string xPath, HtmlDocument doc)
        {
            var node = doc.DocumentNode.SelectSingleNode(xPath);
            return node?.InnerText;
        }

        // Приводит строку к нормальному виду
        public string BringToStandart(string str)
        {
            str = WebUtility.HtmlDecode(str);
            var sb = new StringBuilder();

            str = str.Replace("\n", "").Replace("\t", " ").Replace("\r", " ");
            str = str.Replace("\t", " ");


            for (int i = 0; i < str.Length; i++)
            {
                if ((sb.Length == 0 && str[i] == ' ')
                    || (i < str.Length - 1 && str[i] == ' ' && str[i + 1] == ' ')
                    || (i == str.Length - 1 && str[i] == ' ')
                    || str[i] == 160)
                    continue;
                else
                    sb.Append(str[i]);
            }

            var result = sb.ToString();
            /*
            if (result.ToLower() == "да")
                return "1";
            if (result.ToLower() == "нет")
                return "0";*/
            return result;
        }

    }
}