using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(SalesProductTradeDetailFieldLayoutDefinitionCollection))]
	sealed class SalesProductFieldLayoutDefinitionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SalesProductTradeDetailFieldLayoutDefinitionCollection>
	{
		public void TestDefaultValues()
		{
			var collection = new SalesProductTradeDetailFieldLayoutDefinitionCollection(SalesProduct);
			AssertEquals(1, collection.AddNew().Order);
			AssertEquals(2, collection.AddNew().Order);
			AssertEquals(3, collection.AddNew().Order);
		}

		public void TestImport()
		{
			var salesProduct = Factory.NewWithValidTestData<OrgSalesProduct>();
			var aaaColumn = Factory.New<OrgSalesProductCustomColumnDefinition>();
			aaaColumn.XC_ParentID = salesProduct.PK;
			aaaColumn.XC_ParentTableCode = salesProduct.TablePrefix;
			aaaColumn.XC_Name = "AAA";
			var bbbColumn = Factory.New<OrgSalesProductCustomColumnDefinition>();
			bbbColumn.XC_ParentID = salesProduct.PK;
			bbbColumn.XC_ParentTableCode = salesProduct.TablePrefix;
			bbbColumn.XC_Name = "BBB";

			Factory.Save();

			var data = new SalesProductFormLayoutData();

			var aaaDefintionData = data.TradeLaneFieldLayoutDefinitionData.AddNew();
			aaaDefintionData.GenCustomColumnDefinitionFk = aaaColumn.PK.ToString();
			aaaDefintionData.Order = 1;
			var bbbDefintionData = data.TradeLaneFieldLayoutDefinitionData.AddNew();
			bbbDefintionData.GenCustomColumnDefinitionFk = bbbColumn.PK.ToString();
			bbbDefintionData.Order = 2;

			salesProduct.FormLayout.TradeDetailFieldLayoutDefinitions.Import(data);

			var aaaDefinition = salesProduct.FormLayout.TradeDetailFieldLayoutDefinitions.Cast<SalesProductFieldLayoutDefinition>().Single(x => x.GenCustomColumnDefinitionFk == aaaColumn.PK);
			AssertEquals(1, aaaDefinition.Order);
			var bbbDefinition = salesProduct.FormLayout.TradeDetailFieldLayoutDefinitions.Cast<SalesProductFieldLayoutDefinition>().Single(x => x.GenCustomColumnDefinitionFk == bbbColumn.PK);
			AssertEquals(2, bbbDefinition.Order);

			AssertEquals("Should not flag SalesProduct as changed", false, salesProduct.HasChanges);
		}

		public void TestExport()
		{
			var salesProduct = Factory.NewWithValidTestData<OrgSalesProduct>();
			var aaaColumn = salesProduct.FormLayout.TradeLaneCustomColumnDefinitionCollection.AddNew();
			aaaColumn.XC_Name = "AAA";
			var bbbColumn = salesProduct.FormLayout.TradeLaneCustomColumnDefinitionCollection.AddNew();
			bbbColumn.XC_Name = "BBB";

			var aaaDefinition = salesProduct.FormLayout.TradeDetailFieldLayoutDefinitions.AddNew();
			aaaDefinition.GenCustomColumnDefinitionFk = aaaColumn.PK;
			aaaDefinition.Order = 1;
			var bbbDefinition = salesProduct.FormLayout.TradeDetailFieldLayoutDefinitions.AddNew();
			bbbDefinition.GenCustomColumnDefinitionFk = bbbColumn.PK;
			bbbDefinition.Order = 2;

			Factory.Save();

			var data = new SalesProductFormLayoutData();
			salesProduct.FormLayout.TradeDetailFieldLayoutDefinitions.Export(data);

			var aaaDefintionData = data.TradeLaneFieldLayoutDefinitionData.Cast<SalesProductFieldLayoutDefinitionData>().Single(x => x.GenCustomColumnDefinitionFk == aaaColumn.PK.ToString());
			AssertEquals(1, aaaDefintionData.Order);
			var bbbDefintionData = data.TradeLaneFieldLayoutDefinitionData.Cast<SalesProductFieldLayoutDefinitionData>().Single(x => x.GenCustomColumnDefinitionFk == bbbColumn.PK.ToString());
			AssertEquals(2, bbbDefintionData.Order);

			AssertEquals("Should not flag SalesProduct as changed", false, salesProduct.HasChanges);
		}

		protected override SalesProductTradeDetailFieldLayoutDefinitionCollection GetCollectionToTest()
		{
			return new SalesProductTradeDetailFieldLayoutDefinitionCollection(SalesProduct);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SalesProductTradeDetailFieldLayoutDefinition(SalesProduct);
		}

		OrgSalesProduct SalesProduct
		{
			get { return salesProduct ?? (salesProduct = Factory.New<OrgSalesProduct>()); }
		}
		OrgSalesProduct salesProduct;
	}
}
