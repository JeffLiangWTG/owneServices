using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(DocDataObjectSendingMessageSettings))]
	public class SendReportSettingsTest : OperationalActionMethodSettingsTest<DocDataObjectSendingMessageSettings>
	{
		public void TestOnErrorMessageError_List()
		{
			var settings = new DocDataObjectSendingMessageSettings();
			AssertEquals(2, settings.ErrorAction_List.Count);
			AssertEquals("ABT", settings.ErrorAction_List[0].Code);
			AssertEquals("SKP", settings.ErrorAction_List[1].Code);
		}

		public void TestSerialisation()
		{
			const string expectedXml =
				"<Settings>" +
					"<ErrorAction>SKP</ErrorAction>" +
				"</Settings>";

			var settings1 = new DocDataObjectSendingMessageSettings();
			settings1.ErrorAction = DocDataObjectSendingMessageSettings.Codes.Skip;

			string actualXml;

			using (var stream = new StringWriter())
			using (var writer = new XmlTextWriter(stream))
			{
				writer.WriteStartElement("Settings");
				((IXmlSerializable)settings1).WriteXml(writer);
				writer.WriteEndElement();
				writer.Flush();
				actualXml = stream.ToString();
			}

			AssertMultilineASCIIEquals("", expectedXml.Replace("><", ">\n<"), actualXml.Replace("><", ">\n<"));

			DocDataObjectSendingMessageSettings settings2;

			using (var stream = new StringReader(actualXml))
			using (var reader = new XmlTextReader(stream))
			{
				settings2 = new DocDataObjectSendingMessageSettings();
				((IXmlSerializable)settings2).ReadXml(reader);
			}

			AssertEquals(DocDataObjectSendingMessageSettings.Codes.Skip, settings2.ErrorAction);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocDataObjectSendingMessageSettings();
		}
	}
}
