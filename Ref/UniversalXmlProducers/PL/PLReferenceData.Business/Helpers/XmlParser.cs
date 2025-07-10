using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Helpers
{
	public static class XmlParser
	{
		public static T DeserializeFromFile<T>(string filePath)
		{
			T result;

			var serializer = new XmlSerializer(typeof(T));

			using (var stream = File.OpenRead(filePath))
			using (var reader = XmlReader.Create(stream))
			{
				result = (T)serializer.Deserialize(reader);
			}

			return result;
		}

		public static T DeserializeFromString<T>(string xmlData)
		{
			T result;

			var serializer = new XmlSerializer(typeof(T));

			using (var stream = new StringReader(xmlData))
			using (var reader = XmlReader.Create(stream))
			{
				result = (T)serializer.Deserialize(reader);
			}

			return result;
		}

		public static T Deserialize<T>(XDocument doc)
		{
			T result;

			var xmlSerializer = new XmlSerializer(typeof(T));

			using (var reader = doc.Root.CreateReader())
			{
				result = (T)xmlSerializer.Deserialize(reader);
			}

			return result;
		}

		public static XDocument Serialize<T>(T value)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

			XDocument doc = new XDocument();

			using (var writer = doc.CreateWriter())
			{
				xmlSerializer.Serialize(writer, value);
			}

			return doc;
		}

		public static void SerializeToFile<T>(T value, string filePath)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
			using var writer = XmlWriter.Create(filePath, new XmlWriterSettings { Indent = true });
			xmlSerializer.Serialize(writer, value);
		}
	}
}
