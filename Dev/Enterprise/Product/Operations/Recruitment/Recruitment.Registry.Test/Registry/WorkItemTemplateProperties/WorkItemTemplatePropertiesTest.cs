using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.Recruitment.Registry;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruitment.Testing.Registry
{
	[TestedType(typeof(WorkItemTemplateProperties))]
	sealed class WorkItemTemplatePropertiesTest : RegistryBusinessObjectTemplateTestCase<WorkItemTemplateProperties>
	{
		protected override WorkItemTemplateProperties GetBusinessObjectToClone() => new WorkItemTemplateProperties();

		protected override WorkItemTemplateProperties GetBusinessObjectToSerialise() => GetBusinessObjectToClone();

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		public void TestWorkItemTemplateFriendlyName()
		{
			var wkiProperties = new WorkItemTemplateProperties();
			AssertEquals(ZString.Empty, wkiProperties.FriendlyName);

			wkiProperties.FriendlyName = "meh";
			AssertEquals("meh", wkiProperties.FriendlyName);
		}

		public void TestWorkItemTemplatePropertiesWKI_PK()
		{
			var wkiProperties = new WorkItemTemplateProperties();
			AssertEquals(ZString.Empty, wkiProperties.Area);

			_ = ZGuid.TryParse("3139C719-9E1C-470E-8E08-0D182C1E5B95", out ZGuid pk);
			wkiProperties.WKI_PK = pk;

			AssertNoErrors(wkiProperties.WKI_PKInfo);
		}

		const string xmlRegistryTextFormat = @"<root><FriendlyName>friendly_name</FriendlyName><WKI_PK>{0}</WKI_PK></root>";
		const string xmlRegistryText = @"<root><FriendlyName>friendly_name</FriendlyName><WKI_PK>3139C719-9E1C-470E-8E08-0D182C1E5B95</WKI_PK></root>";

		public void TestWorkItemTemplatePropertiesSerialise()
		{
			var collection = new WorkItemTemplatePropertiesCollection();
			var witp = collection.Add("friendly_name");

			var builder = new StringBuilder();
			var settings = new XmlWriterSettings
			{
				OmitXmlDeclaration = true
			};
			using (var writer = XmlWriter.Create(builder, settings))
			{
				writer.WriteStartElement("root");
				foreach (var v in collection)
				{
					((IXmlSerializable)v).WriteXml(writer);
				}
				writer.WriteEndElement();
			}

			var xml = builder.ToString();
			AssertEquals(string.Format(xmlRegistryTextFormat, witp.WKI_PK), xml);
		}

		public void TestWorkItemTemplatePropertiesDeserialise()
		{
			var witp = new WorkItemTemplateProperties();

			using (var sReader = new StringReader(xmlRegistryText))
			using (var reader = XmlReader.Create(sReader))
			{
				_ = reader.Read();
				((IXmlSerializable)witp).ReadXml(reader);
			}

			AssertNotNull(witp);

			AssertEquals("friendly_name", witp.FriendlyName);
			AssertEquals(new ZGuid("3139C719-9E1C-470E-8E08-0D182C1E5B95"), witp.WKI_PK);
		}
	}
}
