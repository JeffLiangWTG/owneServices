using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgSalesMainCompetitorModuleFilter))]
	sealed class OrgSalesMainCompetitorModuleFilterTest : ModuleFilterTestCase<OrgSalesMainCompetitorModuleFilter>
	{
		public void TestIsEmpty()
		{
			AssertFilterEmpty("TST", ZGuid.BrettsGuid, false);
			AssertFilterEmpty("TST", ZGuid.Empty, true);
			AssertFilterEmpty(ZString.Empty, ZGuid.BrettsGuid, true);
			AssertFilterEmpty(ZString.Empty, ZGuid.Empty, true);
		}

		void AssertFilterEmpty(ZString competitorType, ZGuid competitor, bool isEmpty)
		{
			var filter = (OrgSalesMainCompetitorModuleFilter)GetNewBusinessObject();
			filter.CompetitorType = competitorType;
			filter.Competitor = competitor;

			AssertEquals(isEmpty, filter.IsEmpty);
		}

		public void TestClear()
		{
			var competitorType = "2123";
			var competitor = ZGuid.BrettsGuid;

			var filter = (OrgSalesMainCompetitorModuleFilter)GetNewBusinessObject();
			filter.CompetitorType = competitorType;
			filter.Competitor = competitor;

			AssertEquals("Precondition", competitorType, filter.CompetitorType);
			AssertEquals("Precondition", competitor, filter.Competitor);

			filter.Clear();
			AssertEquals(ZString.Empty, filter.CompetitorType);
			AssertEquals(ZGuid.Empty, filter.Competitor);
		}

		public void TestCopyTransient()
		{
			var filter1 = GetNewModuleFilter();
			filter1.CompetitorType = "TST";
			filter1.Competitor = Guid.NewGuid();
			var filter2 = GetNewModuleFilter();

			filter2.CopyTransientProperties(filter1);
			AssertEquals("CompetitorType should not be changed", filter1.CompetitorType, filter2.CompetitorType);
			AssertEquals("HasMainCompetitor should not be changed", filter1.Competitor, filter2.Competitor);
		}

		readonly string filterXml = String.Format(@"<Filter>
  <CompetitorType>ABC123</CompetitorType>
  <Competitor>{0}</Competitor>
</Filter>", ZGuid.BrettsGuid);

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		public void TestSerialisation()
		{
			var filter = (OrgSalesMainCompetitorModuleFilter)GetNewBusinessObject();
			filter.CompetitorType = "ABC123";
			filter.Competitor = ZGuid.BrettsGuid;

			using (var writer = new StringWriter())
			using (XmlTextWriter xmlWriter = new XmlTextWriter(writer))
			{
				xmlWriter.Formatting = Formatting.Indented;
				xmlWriter.WriteStartElement("Filter");
				((IXmlSerializable)filter).WriteXml(xmlWriter);
				xmlWriter.WriteEndElement();

				AssertMultilineASCIIEquals(filterXml, writer.ToString());
			}
		}

		public void TestDeserilisation()
		{
			var filter = (OrgSalesMainCompetitorModuleFilter)GetNewBusinessObject();

			using (var reader = new StringReader(filterXml))
			using (var xmlReader = new XmlTextReader(reader))
			{
				xmlReader.WhitespaceHandling = WhitespaceHandling.None;
				xmlReader.MoveToContent();
				xmlReader.ReadStartElement("Filter");
				((IXmlSerializable)filter).ReadXml(xmlReader);
				xmlReader.ReadEndElement();
			}

			AssertEquals("ABC123", filter.CompetitorType);
			AssertEquals(ZGuid.BrettsGuid, filter.Competitor);
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override OrgSalesMainCompetitorModuleFilter GetNewModuleFilter()
		{
			return new OrgSalesMainCompetitorModuleFilter("description", (a, b) => new ZQuery());
		}

		protected override ZString ExpectedDescription => "description";
	}
}
