using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.ContractManagement.Module.Testing
{
	[TestedType(typeof(AllocationRouteCoveringLocationFilter))]
	sealed class AllocationRouteCoveringLocationFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestClear()
		{
			var loadPort = "NZAKL";
			var dischargePort = "AUSYD";
			var showRelatedUNLOCOs = false;

			var filter = (AllocationRouteCoveringLocationFilter)GetNewBusinessObject();
			filter.Property1 = loadPort;
			filter.Property2 = dischargePort;
			filter.ShowRelatedUNLOCOs = showRelatedUNLOCOs;

			AssertEquals("Ensuring LoadLocation is not empty", filter.Property1, loadPort);
			AssertEquals("Ensuring Property2 is not empty", filter.Property2, dischargePort);
			AssertEquals("Ensuring ShowRelatedUNLOCOsProperty is not empty", filter.ShowRelatedUNLOCOs, showRelatedUNLOCOs);

			filter.Clear();
			AssertEquals(filter.Property1, ZString.Empty);
			AssertEquals(filter.Property2, ZString.Empty);
			AssertEquals(filter.ShowRelatedUNLOCOs, true);
		}

		const string sampleFilterXmlString = @$"<Filter>
  <Property1>NZAKL</Property1>
  <Property2>AUSYD</Property2>
  <ShowRelatedUNLOCOs>False</ShowRelatedUNLOCOs>
</Filter>";

		public void TestSerialisation()
		{
			var filter = (AllocationRouteCoveringLocationFilter)GetNewBusinessObject();
			filter.Property1 = "NZAKL";
			filter.Property2 = "AUSYD";
			filter.ShowRelatedUNLOCOs = false;

			using (var writer = new StringWriter())
			using (var xmlWriter = new XmlTextWriter(writer))
			{
				xmlWriter.Formatting = Formatting.Indented;
				xmlWriter.WriteStartElement("Filter");
				((IXmlSerializable)filter).WriteXml(xmlWriter);
				xmlWriter.WriteEndElement();

				AssertMultilineASCIIEquals("AllocationRouteFilter Serialisation", sampleFilterXmlString, writer.ToString());
			}
		}

		public void TestDeserialisation()
		{
			var filter = (AllocationRouteCoveringLocationFilter)GetNewBusinessObject();

			using (var reader = new StringReader(sampleFilterXmlString))
			using (var xmlReader = new XmlTextReader(reader))
			{
				xmlReader.WhitespaceHandling = WhitespaceHandling.None;
				xmlReader.MoveToContent();
				xmlReader.ReadStartElement("Filter");
				((IXmlSerializable)filter).ReadXml(xmlReader);
				xmlReader.ReadEndElement();

				AssertEquals("NZAKL", filter.Property1);
				AssertEquals("AUSYD", filter.Property2);
				AssertEquals(false, filter.ShowRelatedUNLOCOs);
			}
		}

		public void TestCanDeserialiseWithoutShowRelatedUNLOCOs()
		{
			var sampleFilterXmlString = @$"<Filter>
  <Property1>NZAKL</Property1>
  <Property2>AUSYD</Property2>
  </Filter>";

			var filter = (AllocationRouteCoveringLocationFilter)GetNewBusinessObject();

			using (var reader = new StringReader(sampleFilterXmlString))
			using (var xmlReader = new XmlTextReader(reader))
			{
				xmlReader.WhitespaceHandling = WhitespaceHandling.None;
				xmlReader.MoveToContent();
				xmlReader.ReadStartElement("Filter");
				((IXmlSerializable)filter).ReadXml(xmlReader);
				xmlReader.ReadEndElement();

				AssertEquals("NZAKL", filter.Property1);
				AssertEquals("AUSYD", filter.Property2);
				AssertEquals(true, filter.ShowRelatedUNLOCOs);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AllocationRouteCoveringLocationFilter(AllocationRouteFilterConstants.LoadDischargePort, new LocationCollection(Factory));
		}
	}
}
