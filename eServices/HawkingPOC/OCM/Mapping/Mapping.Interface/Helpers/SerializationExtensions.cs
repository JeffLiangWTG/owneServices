using System.IO;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace OcmPoc.Mapping.Interface.Helpers
{
	public static class SerializationExtensions
    {
		public static T DeserializeFromXml<T>(this byte[] bytes)
		{
			using (var stream = new MemoryStream(bytes))
			{
				var serializer = new XmlSerializer(typeof(T));
				return (T)serializer.Deserialize(stream);
			}
		}

		public static byte[] SerializeToXml<T>(this T obj)
		{
			using (var stream = new MemoryStream())
			{
				var serializer = new XmlSerializer(typeof(T));
				serializer.Serialize(stream, obj);

				return stream.ToArray();
			}
		}

		public static byte[] SerializeToJson(this object obj)
		{
			return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj));
		}

		public static T DeserializeFromJson<T>(this byte[] bytes)
		{
			return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytes));
		}
	}
}
