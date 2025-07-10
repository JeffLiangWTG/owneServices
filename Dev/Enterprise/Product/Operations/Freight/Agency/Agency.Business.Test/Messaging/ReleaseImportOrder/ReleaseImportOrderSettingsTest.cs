using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(ReleaseImportOrderSettings))]
	internal class EIDOSettingsTest : OperationalActionMethodSettingsTest<ReleaseImportOrderSettings>
	{
		public void TestAssertDefaultValue()
		{
			ReleaseImportOrderSettings settings = new ReleaseImportOrderSettings();
			AssertEquals(OperationalActionErrorBehaviourList.Codes.Abort, settings.ErrorBehaviour);
		}

		public void TestSerialisation()
		{
			const string expectedXml =
				"<Settings>" +
					"<ErrorBehaviour>SKP</ErrorBehaviour>" +
				"</Settings>" +
				"";

			ReleaseImportOrderSettings settings1 = new ReleaseImportOrderSettings();
			settings1.ErrorBehaviour = OperationalActionErrorBehaviourList.Codes.Skip;

			string xml;

			using (StringWriter stream = new StringWriter())
			using (XmlTextWriter writer = new XmlTextWriter(stream))
			{
				writer.WriteStartElement("Settings");
				((IXmlSerializable)settings1).WriteXml(writer);
				writer.WriteEndElement();
				writer.Flush();
				xml = stream.ToString();
			}

			AssertMultilineASCIIEquals("", expectedXml.Replace("><", ">\n<"), xml.Replace("><", ">\n<"));

			ReleaseImportOrderSettings settings2;

			using (StringReader stream = new StringReader(xml))
			using (XmlTextReader reader = new XmlTextReader(stream))
			{
				settings2 = new ReleaseImportOrderSettings();
				((IXmlSerializable)settings2).ReadXml(reader);
			}

			AssertEquals(OperationalActionErrorBehaviourList.Codes.Skip, settings2.ErrorBehaviour);
		}
	}
}
