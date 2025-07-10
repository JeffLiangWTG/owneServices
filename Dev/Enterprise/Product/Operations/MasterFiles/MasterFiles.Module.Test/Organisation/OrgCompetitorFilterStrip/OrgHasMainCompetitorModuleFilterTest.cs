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
	[TestedType(typeof(OrgHasMainCompetitorModuleFilter))]
	sealed class OrgHasMainCompetitorModuleFilterTest : ModuleFilterTestCase<OrgHasMainCompetitorModuleFilter>
	{
		public void TestIsEmpty()
		{
			AssertFilterEmpty("TST", ZBool.False, false);
			AssertFilterEmpty("TST", ZBool.True, false);
			AssertFilterEmpty(ZString.Empty, ZBool.False, true);
			AssertFilterEmpty(ZString.Empty, ZBool.True, true);
		}

		void AssertFilterEmpty(ZString competitorType, ZBool hasMainCompetitor, bool isEmpty)
		{
			var filter = (OrgHasMainCompetitorModuleFilter)GetNewBusinessObject();
			filter.CompetitorType = competitorType;
			filter.HasMainCompetitor = hasMainCompetitor;

			AssertEquals(isEmpty, filter.IsEmpty);
		}

		public void TestClear()
		{
			var competitorType = "2123";
			var hasMainCompetitor = ZBool.True;

			var filter = (OrgHasMainCompetitorModuleFilter)GetNewBusinessObject();
			filter.CompetitorType = competitorType;
			filter.HasMainCompetitor = hasMainCompetitor;

			AssertEquals("Precondition", competitorType, filter.CompetitorType);
			AssertEquals("Precondition", hasMainCompetitor, filter.HasMainCompetitor);

			filter.Clear();
			AssertEquals(ZString.Empty, filter.CompetitorType);
			AssertEquals(ZBool.False, filter.HasMainCompetitor);
		}

		public void TestCopyTransient()
		{
			var filter1 = GetNewModuleFilter();
			filter1.CompetitorType = "TST";
			filter1.HasMainCompetitor = true;
			var filter2 = GetNewModuleFilter();

			filter2.CopyTransientProperties(filter1);
			AssertEquals("CompetitorType should not be changed", filter1.CompetitorType, filter2.CompetitorType);
			AssertEquals("HasMainCompetitor should not be changed", filter1.HasMainCompetitor, filter2.HasMainCompetitor);
		}

		const string filterXml = @"<Filter>
  <CompetitorType>ABC123</CompetitorType>
  <HasMainCompetitor>Y</HasMainCompetitor>
</Filter>";

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		public void TestSerialisation()
		{
			var filter = (OrgHasMainCompetitorModuleFilter)GetNewBusinessObject();
			filter.CompetitorType = "ABC123";
			filter.HasMainCompetitor = ZBool.True;

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
			var filter = (OrgHasMainCompetitorModuleFilter)GetNewBusinessObject();

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
			AssertEquals(ZBool.True, filter.HasMainCompetitor);
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override OrgHasMainCompetitorModuleFilter GetNewModuleFilter()
		{
			return new OrgHasMainCompetitorModuleFilter("description", (a, b) => new ZQuery());
		}

		protected override ZString ExpectedDescription => "description";
	}
}
