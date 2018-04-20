using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AudioVideoMaker
{
    [XmlType(AnonymousType = true)]
    [XmlRoot("SlackMessage", Namespace = "http://www.example.com/schemas/TestNamespace/Interface6/Schema.xsd", IsNullable = false)]
    [Serializable]
    public class SlackMessage
    {
        [XmlElement]
        public string text { get; set; }
        [XmlElement]
        public string filePath { get; set; }
        [XmlElement]
        public bool isFile { get; set; }
    }
}
