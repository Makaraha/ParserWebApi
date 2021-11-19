using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ParserWebApi
{
    public class RssModel
    {
        [XmlRoot("rss")]
        public class Rss
        {
            [XmlElement("channel")]
            public Channel Channel { get; set; }
        }

        public class Channel
        {
            [XmlElement("item")]
            public Procurement[] Procurements { get; set; }
        }

        public class Procurement
        {
            [XmlElement("guid")]
            public string Guid { get; set; }
            [XmlElement("pubDate")]
            public DateTime PubDate { get; set; }
        }

    }
}