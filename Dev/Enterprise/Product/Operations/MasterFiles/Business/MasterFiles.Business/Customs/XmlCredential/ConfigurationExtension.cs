using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.MasterFiles.Business.Customs.XmlCredential
{
	public static class ConfigurationExtension
	{
		public static ZString ToXml(this Configuration configuration)
		{
			using (var writer = new StringWriter(CultureInfo.InvariantCulture))
			{
				var serializer = ObjectFactory.Get<IXmlValueObjectSerializer>("IXmlValueObjectSerializer", typeof(Configuration));
				serializer.Serialize(writer, configuration);
				writer.Flush();

				var xml = writer.GetStringBuilder().ToString();
				return AddMissedElementsWithDefaultValue(xml);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "XML body")]
		static string AddMissedElementsWithDefaultValue(string xml)
		{
			var document = XDocument.Parse(xml, LoadOptions.None);

			var elements = document
				.XPathSelectElements((NoResString)@"//*[local-name()='Group']/*[local-name()='FTP']")
				.Where(c => c.HasElements);

			foreach (var element in elements)
			{
				var portElement = element.XPathSelectElement((NoResString)@"./*[local-name()='Port']");
				if (portElement == null)
				{
					element.Add(new XElement(element.GetDefaultNamespace() + "Port", 21));
				}
			}

			return document.ToString();
		}

		public static Configuration DeserializeToConfiguration(this TextReader reader)
		{
			var serialiser = ZXmlSerializer.New(typeof(Configuration));
			return (Configuration)serialiser.Deserialize(reader);
		}
	}
}
