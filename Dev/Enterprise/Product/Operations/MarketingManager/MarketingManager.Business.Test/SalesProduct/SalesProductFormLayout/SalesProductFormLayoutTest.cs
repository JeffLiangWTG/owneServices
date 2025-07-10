using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(SalesProductFormLayout))]
	sealed class SalesProductFormLayoutTest : NonPersistentBusinessObjectTestCase
	{
		public void TestImport()
		{
			OrgSalesProductCustomColumnDefinition columnDefinition1;
			OrgSalesProductCustomColumnDefinition columnDefinition2;
			OrgSalesProductCustomColumnDefinition columnDefinition3;
			OrgSalesProductCustomColumnDefinition columnDefinition4;
			var anotherFactory = new BusinessObjectFactory();
			using (GetFactoryIsolater(anotherFactory))
			{
				columnDefinition1 = anotherFactory.NewWithValidTestData<OrgSalesProductCustomColumnDefinition>();
				columnDefinition2 = anotherFactory.NewWithValidTestData<OrgSalesProductCustomColumnDefinition>();
				columnDefinition3 = anotherFactory.NewWithValidTestData<OrgSalesProductCustomColumnDefinition>();
				columnDefinition4 = anotherFactory.NewWithValidTestData<OrgSalesProductCustomColumnDefinition>();
				anotherFactory.Save();
			}

			var data = new SalesProductFormLayoutData();
			data.TradeLaneCustomColumnDefinitionFks = new[] { columnDefinition1, columnDefinition2 }.Select(x => x.PK.ToString()).ToArray();
			data.TradeLaneGridColumnDefinitionData.AddNew().GenCustomColumnDefinitionFk = columnDefinition1.PK.ToString();
			data.TradeLaneGridColumnDefinitionData.AddNew().GenCustomColumnDefinitionFk = columnDefinition2.PK.ToString();

			data.TradeDetailCustomColumnDefinitionFks = new[] { columnDefinition3, columnDefinition4 }.Select(x => x.PK.ToString()).ToArray();
			data.TradeLaneFieldLayoutDefinitionData.AddNew().GenCustomColumnDefinitionFk = columnDefinition3.PK.ToString();
			data.TradeDetailGridColumnDefinitionData.AddNew().GenCustomColumnDefinitionFk = columnDefinition4.PK.ToString();

			Factory.ResetDatabaseLoadCount();
			var layout = GetNewFormLayout();
			layout.Import(data);
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgSalesProductCustomColumnDefinition>.PKOnlyComparer,
				new[] { columnDefinition1, columnDefinition2 },
				layout.TradeLaneCustomColumnDefinitionCollection.Cast<OrgSalesProductCustomColumnDefinition>());
			AssertEquals(2, layout.TradeLaneGridColumnDefinitions.Count);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgSalesProductCustomColumnDefinition>.PKOnlyComparer,
				new[] { columnDefinition3, columnDefinition4 },
				layout.TradeDetailCustomColumnDefinitionCollection.Cast<OrgSalesProductCustomColumnDefinition>());
			AssertEquals(1, layout.TradeDetailFieldLayoutDefinitions.Count);
			AssertEquals(1, layout.TradeDetailGridColumnDefinitions.Count);

			AssertEquals("HasChanges", false, layout.HasChanges);

			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(GenCustomColumnDefinitionSchema.Constants.TableName, 1);
			AssertDbHits(expectedDbHits, Factory);
		}

		public void TestExport()
		{
			var layout = GetNewFormLayout();
			layout.TradeLaneGridColumnDefinitions.AddNew();
			layout.TradeDetailFieldLayoutDefinitions.AddNew();
			layout.TradeDetailGridColumnDefinitions.AddNew();

			var data = layout.ToFormLayoutData();
			AssertEquals(1, data.TradeLaneGridColumnDefinitionData.Count);
			AssertEquals(1, data.TradeLaneFieldLayoutDefinitionData.Count);
			AssertEquals(1, data.TradeDetailGridColumnDefinitionData.Count);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewFormLayout();
		}

		SalesProductFormLayout GetNewFormLayout()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();
			return new SalesProductFormLayout(salesProduct);
		}
	}
}
