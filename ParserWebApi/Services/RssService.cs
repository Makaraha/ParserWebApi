using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ParserWebApi.Services
{
    public class RssService
    {
        private const string rssUrl = "https://zakupki.rosatom.ru/?node=currentorders&mode=order&action=rssfeed&ostate=&ptype=&cust=";
        
        //Считывает Rss росатома
        private async Task<string> ScanRssAsync()
        {
            WebRequest myWebRequest = WebRequest.Create(rssUrl);

            WebResponse myWebResponse = await myWebRequest.GetResponseAsync();
            Stream ReceiveStream = myWebResponse.GetResponseStream();

            Encoding encode = Encoding.GetEncoding("utf-8");
            TextReader reader = new StreamReader(ReceiveStream, encode);

            var text = await reader.ReadToEndAsync();
            text = WebUtility.HtmlDecode(text).Replace('&', ' ').Replace("<guid>http://zakupki.rosatom.ru/", "<guid>");
            reader.Close();

            //var text = File.ReadAllText(@"C:\Users\user\Desktop\Rosatom.txt").Replace('&', 'n')
                

            return text;
        }

        
        public async Task<RssModel.Procurement[]> ExtractProcurementsAsync()
        {
            var xml = await ScanRssAsync();
            var serializer = new XmlSerializer(typeof(RssModel.Rss));
            var rssModel = serializer.Deserialize(new StringReader(xml)) as RssModel.Rss;
            return rssModel.Channel.Procurements;
        }

    }
}