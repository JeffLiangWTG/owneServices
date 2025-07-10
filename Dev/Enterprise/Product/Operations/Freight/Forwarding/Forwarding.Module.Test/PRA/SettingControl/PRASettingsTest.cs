using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(PRASettings))]
	public class PRASettingsTest : OperationalActionMethodSettingsTest<PRASettings>
	{
		public void TestErrorBehaviour_List()
		{
			PRASettings settings = GetNewPRASettings();
			AssertEquals(2, settings.ErrorBehaviour_List.Count);
			AssertEquals("ABT", settings.ErrorBehaviour_List[0].Code);
			AssertEquals("SKP", settings.ErrorBehaviour_List[1].Code);
		}

		public void TestSerialisation()
		{
			const string expectedXml =
				"<Settings>" +
					"<ErrorBehaviour>SKP</ErrorBehaviour>" +
				"</Settings>";

			PRASettings settings1 = GetNewPRASettings();
			settings1.ErrorBehaviour = PRASettings.Codes.Skip;

			string actualXml;

			using (StringWriter stream = new StringWriter())
			using (XmlTextWriter writer = new XmlTextWriter(stream))
			{
				writer.WriteStartElement("Settings");
				((IXmlSerializable)settings1).WriteXml(writer);
				writer.WriteEndElement();
				writer.Flush();
				actualXml = stream.ToString();
			}

			AssertMultilineASCIIEquals("", expectedXml.Replace("><", ">\n<"), actualXml.Replace("><", ">\n<"));

			PRASettings settings2;

			using (StringReader stream = new StringReader(actualXml))
			using (XmlTextReader reader = new XmlTextReader(stream))
			{
				settings2 = GetNewPRASettings();
				((IXmlSerializable)settings2).ReadXml(reader);
			}

			AssertEquals(PRASettings.Codes.Skip, settings2.ErrorBehaviour);
		}

		PRASettings GetNewPRASettings()
		{
			var factory = new BusinessObjectFactory();
			return new PRASettings(factory);
		}
	}
}
