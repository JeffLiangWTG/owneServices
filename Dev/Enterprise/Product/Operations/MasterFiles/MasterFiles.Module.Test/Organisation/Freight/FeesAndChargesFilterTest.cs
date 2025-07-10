using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(FeesAndChargesFilter))]
	sealed class FeesAndChargesFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIsEmpty()
		{
			AssertFilterEmpty("IWY", ZString.Empty, false);
			AssertFilterEmpty(ZString.Empty, "STD", false);
			AssertFilterEmpty(ZString.Empty, ZString.Empty, true);
		}

		void AssertFilterEmpty(ZString serviceType, ZString serviceLevel, bool isEmpty)
		{
			var filter = (FeesAndChargesFilter)GetNewBusinessObject();
			filter.ServiceType = serviceType;
			filter.ServiceLevel = serviceLevel;

			AssertEquals(isEmpty, filter.IsEmpty);
		}

		const string filterXml = @"<Filter>
  <ServiceType>IWY</ServiceType>
  <ServiceLevel>STD</ServiceLevel>
</Filter>";

		public void TestSerialisation()
		{
			var filter = (FeesAndChargesFilter)GetNewBusinessObject();
			filter.ServiceType = "IWY";
			filter.ServiceLevel = "STD";

			using (var writer = new StringWriter())
			using (XmlTextWriter xmlWriter = new XmlTextWriter(writer))
			{
				xmlWriter.Formatting = Formatting.Indented;
				xmlWriter.WriteStartElement("Filter");
				((IXmlSerializable)filter).WriteXml(xmlWriter);
				xmlWriter.WriteEndElement();

				AssertMultilineASCIIEquals("", filterXml, writer.ToString());
			}
		}

		public void TestDeserilisation()
		{
			var filter = (FeesAndChargesFilter)GetNewBusinessObject();

			using (var reader = new StringReader(filterXml))
			using (var xmlReader = new XmlTextReader(reader))
			{
				xmlReader.WhitespaceHandling = WhitespaceHandling.None;
				xmlReader.MoveToContent();
				xmlReader.ReadStartElement("Filter");
				((IXmlSerializable)filter).ReadXml(xmlReader);
				xmlReader.ReadEndElement();
			}

			AssertEquals("IWY", filter.ServiceType);
			AssertEquals("STD", filter.ServiceLevel);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new FeesAndChargesFilter("Desc", (a, b) => new ZQuery());
		}
	}
}
