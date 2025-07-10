using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(VoyageVesselModuleFilter))]
	sealed class VoyageVesselModuleFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIsEmpty()
		{
			AssertFilterEmpty("QF123", ZString.Empty, false);
			AssertFilterEmpty(ZString.Empty, "TITANIC", false);
			AssertFilterEmpty(ZString.Empty, ZString.Empty, true);
		}

		void AssertFilterEmpty(ZString voyageFlightNo, ZString vessel, bool isEmpty)
		{
			var filter = (VoyageVesselModuleFilter)GetNewBusinessObject();
			filter.VoyageFlightNo = voyageFlightNo;
			filter.Vessel = vessel;

			AssertEquals(isEmpty, filter.IsEmpty);
		}

		public void TestIsBlankOrNotBlankMakesVoyageNoAndVesselReadOnly()
		{
			var filter = (VoyageVesselModuleFilter)GetNewBusinessObject();
			filter.VoyageFlightNo = "Voyage Number";
			filter.Vessel = "FlyDutchman";

			filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			AssertComparisonOperator(filter, ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact, SQLComparisonOperator.Equal, "Voyage Number", "FlyDutchman", false);

			filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank;
			AssertComparisonOperator(filter, ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank, SpecialComparisonOperator.IsBlank, "", "", true);

			filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains;
			AssertComparisonOperator(filter, ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains, SQLComparisonOperator.Contains, "Voyage Number", "FlyDutchman", false);

			filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank;
			AssertComparisonOperator(filter, ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank, SpecialComparisonOperator.IsNotBlank, "", "", true);
		}

		void AssertComparisonOperator(VoyageVesselModuleFilter filter, string comparisonOperator, SQLComparisonOperator sqlOperator, string property, string nk, bool readOnly)
		{
			AssertEquals(comparisonOperator, filter.ComparisonOperator);
			AssertEquals(sqlOperator.GetType(), filter.SqlComparisonOperator.GetType());
			AssertEquals(property, filter.VoyageFlightNo);
			AssertEquals(readOnly, filter.VoyageFlightNoInfo.ReadOnly);
			AssertEquals(nk, filter.Vessel);
			AssertEquals(readOnly, filter.VesselInfo.ReadOnly);
		}

		public void TestClear()
		{
			var voyageFlightNo = "2123";
			var vessel = "ANRO ASIA";

			var filter = (VoyageVesselModuleFilter)GetNewBusinessObject();
			filter.VoyageFlightNo = voyageFlightNo;
			filter.Vessel = vessel;

			AssertEquals("Precondition", voyageFlightNo, filter.VoyageFlightNo);
			AssertEquals("Precondition", vessel, filter.Vessel);

			filter.Clear();
			AssertEquals(ZString.Empty, filter.VoyageFlightNo);
			AssertEquals(ZString.Empty, filter.Vessel);
		}

		const string filterXml = @"<Filter>
  <Comparer>starts with</Comparer>
  <Property>ABC123</Property>
  <Vessel>TITANIC</Vessel>
  <IncludeArchived>Y</IncludeArchived>
</Filter>";

		public void TestSerialisation()
		{
			var filter = (VoyageVesselModuleFilter)GetNewBusinessObject();
			filter.VoyageFlightNo = "ABC123";
			filter.Vessel = "TITANIC";
			filter.IncludeArchived = true;

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
			var filter = (VoyageVesselModuleFilter)GetNewBusinessObject();

			using (var reader = new StringReader(filterXml))
			using (var xmlReader = new XmlTextReader(reader))
			{
				xmlReader.WhitespaceHandling = WhitespaceHandling.None;
				xmlReader.MoveToContent();
				xmlReader.ReadStartElement("Filter");
				((IXmlSerializable)filter).ReadXml(xmlReader);
				xmlReader.ReadEndElement();
			}

			AssertEquals("ABC123", filter.VoyageFlightNo);
			AssertEquals("TITANIC", filter.Vessel);
			AssertEquals(true, filter.IncludeArchived);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new VoyageVesselModuleFilter("description", (a, b, c, d) => new ZQuery(), new RefVesselCollection(Factory));
		}
	}
}
