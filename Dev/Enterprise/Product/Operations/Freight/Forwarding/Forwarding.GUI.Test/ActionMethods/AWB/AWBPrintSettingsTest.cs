using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(AWBPrintSettings))]
	public class AWBPrintSettingsTest : OperationalActionMethodSettingsTest<AWBPrintSettings>
	{
		public void TestOnErrorMessageError_List()
		{
			AWBPrintSettings settings = new AWBPrintSettings();
			AssertEquals(2, settings.OnErrorMessageError_List.Count);
			AssertEquals("ABT", settings.OnErrorMessageError_List[0].Code);
			AssertEquals("SKP", settings.OnErrorMessageError_List[1].Code);
		}

		public void TestSerialisation()
		{
			const string expectedXml =
				"<Settings>" +
					"<OnErrorMessageError>SKP</OnErrorMessageError>" +
				"</Settings>";

			AWBPrintSettings settings1 = new AWBPrintSettings();
			settings1.OnErrorMessageError = AWBPrintSettings.Codes.Skip;

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

			AWBPrintSettings settings2;

			using (StringReader stream = new StringReader(actualXml))
			using (XmlTextReader reader = new XmlTextReader(stream))
			{
				settings2 = new AWBPrintSettings();
				((IXmlSerializable)settings2).ReadXml(reader);
			}

			AssertEquals(AWBPrintSettings.Codes.Skip, settings2.OnErrorMessageError);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AWBPrintSettings();
		}
	}
}
