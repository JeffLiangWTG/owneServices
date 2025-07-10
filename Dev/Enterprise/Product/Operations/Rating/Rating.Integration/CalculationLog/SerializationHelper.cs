using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Types;

namespace Enterprise.Rating.Integration
{
	public static class SerializationHelper
	{
		public static ZString SerializeToString(this IXmlSerializable serializable)
		{
			ZString result = ZString.Empty;

			using (StringWriter writer = new StringWriter())
			using (XmlTextWriter xmlWriter = new XmlTextWriter(writer))
			{
				serializable.WriteXml(xmlWriter);
				result = writer.ToString();
			}

			return result;
		}

		public static void DeserializeFromString(this IXmlSerializable serializable, ZString serializedValue)
		{
			using (StringReader reader = new StringReader(serializedValue))
			using (XmlTextReader xmlReader = new XmlTextReader(reader))
			{
				xmlReader.WhitespaceHandling = WhitespaceHandling.None;
				serializable.ReadXml(xmlReader);
			}
		}

		public static string DecimalToInvariantString(ZDecimal number)
		{
			return ((decimal)number).ToString(CultureInfo.InvariantCulture.NumberFormat);
		}

		public static ZDecimal InvariantStringToDecimal(string numberString)
		{
			return decimal.Parse(numberString, CultureInfo.InvariantCulture.NumberFormat);
		}

		public static void SkipDeprecated(XmlReader reader, string deprecatedString)
		{
			if (reader.Name == deprecatedString)
			{
				_ = reader.ReadElementString(deprecatedString);
			}
		}
	}
}
