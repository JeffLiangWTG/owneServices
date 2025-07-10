using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Customs.TW.Messaging.MessageBuilders
{
	public sealed class XmlHelper
	{
		XmlHelper() { }

		public static string Serializer(Type type, object obj, bool removeIndentAndLineBreak = false)
		{
			var result = string.Empty;
			var settings = new XmlWriterSettings();
			settings.Indent = !removeIndentAndLineBreak;
			settings.NewLineOnAttributes = !removeIndentAndLineBreak;
			settings.NewLineHandling = NewLineHandling.None;

			using (var stringWriter = new StringWriterWithUTF8Encoding())
			using (var xmlWritter = XmlWriter.Create(stringWriter, settings))
			{
				if (type.GetField("XsiSchemaLocation") != null)
				{
					xmlWritter.WriteStartDocument(true);
				}
				var xmlSerialiser = ZXmlSerializer.New(type);
				xmlSerialiser.Serialize(xmlWritter, obj);
				result = stringWriter.ToString();
			}
			return result;
		}
	}

	sealed class StringWriterWithUTF8Encoding : StringWriter
	{
		public StringWriterWithUTF8Encoding() : base(new StringBuilder(), CultureInfo.InvariantCulture) { }

		public override Encoding Encoding => Encoding.UTF8;
	}
}
