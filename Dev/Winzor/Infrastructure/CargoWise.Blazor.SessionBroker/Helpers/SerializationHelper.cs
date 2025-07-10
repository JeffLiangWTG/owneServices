using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace CargoWise.Blazor.SessionBroker.Helpers
{
	public static class SerializationHelper
	{
		public static T Deserialise<T>(byte[] value)
		{
			var result = default(T);
			if (value == null || value.Length == 0)
			{
				return result;
			}

			var serialiser = new XmlSerializer(typeof(T));
			using (var stream = new MemoryStream(value))
			using (var reader = XmlReader.Create(stream))
			{
				result = (T)serialiser.Deserialize(reader);
			}
			return result;
		}
	}
}
