using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace BizTalk.Utilities.Common
{
    public static class XmlHelper
    {
        public static T Deserialize<T>(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
            {
                using (XmlReader reader = XmlReader.Create(ms))
                {
                    return (T)serializer.Deserialize(reader);
                }
            }
        }
        public static string Serialize<T>(T target)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.Serialize(ms, target);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }
    }
}
