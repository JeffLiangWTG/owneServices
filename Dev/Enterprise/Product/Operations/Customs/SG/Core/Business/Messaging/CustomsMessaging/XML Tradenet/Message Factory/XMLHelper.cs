using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet
{
	public static class XMLHelper
	{
		public static string MakeDataAlwaysInUppercase(string xml)
		{
			var doc = XDocument.Parse(xml);
			ToUpper(doc.Root);
			return ToStringWithDeclaration(doc);
		}

		static void ToUpper(XElement element)
		{
			foreach (var attribute in element.Attributes())
			{
				if (!attribute.IsNamespaceDeclaration)
				{
					var attributeValue = attribute.Value;
					if (NeedToUpper(attributeValue))
					{
						attribute.Value = attributeValue.ToUpperInvariant();
					}
				}
			}

			if (element.HasElements)
			{
				element.Elements().ToList().ForEach(ToUpper);
			}
			else
			{
				var elementValue = element.Value;
				if (NeedToUpper(elementValue))
				{
					element.Value = elementValue.ToUpperInvariant();
				}
			}
		}

		static bool NeedToUpper(string value) => !string.IsNullOrEmpty(value) && !IsBooleanValue(value);

		static bool IsBooleanValue(string value) => value == "true" || value == "false";

		static string ToStringWithDeclaration(XDocument doc)
		{
			using (var writer = new StringWriterWithUtf8Encoding())
			{
				doc.Save(writer);
				return writer.ToString();
			}
		}

		class StringWriterWithUtf8Encoding : StringWriter
		{
			public StringWriterWithUtf8Encoding() : base(new StringBuilder(), CultureInfo.InvariantCulture) { }

			public override Encoding Encoding => Encoding.UTF8;
		}
	}
}
