using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class SalesProductGridColumnDefinitionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestColumnName()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();
			var aaaColumn = salesProduct.FormLayout.TradeLaneCustomColumnDefinitionCollection.AddNew();
			aaaColumn.XC_Name = "AAA";
			var bbbColumn = salesProduct.FormLayout.TradeLaneCustomColumnDefinitionCollection.AddNew();
			bbbColumn.XC_Name = "BBB";

			var collection = new SalesProductTradeLaneGridColumnDefinitionCollection(salesProduct);
			var fieldDefinition1 = collection.AddNew();
			fieldDefinition1.GenCustomColumnDefinitionFk = aaaColumn.PK;
			var fieldDefinition2 = collection.AddNew();
			fieldDefinition2.GenCustomColumnDefinitionFk = bbbColumn.PK;

			AssertMandatoryValidationError(fieldDefinition2.GenCustomColumnDefinitionFkInfo, false);
			AssertPropertyIsUniqueInCollectionValidationError(fieldDefinition2.GenCustomColumnDefinitionFkInfo, false);

			fieldDefinition2.GenCustomColumnDefinitionFk = aaaColumn.PK;
			AssertMandatoryValidationError(fieldDefinition2.GenCustomColumnDefinitionFkInfo, false);
			AssertPropertyIsUniqueInCollectionValidationError(fieldDefinition2.GenCustomColumnDefinitionFkInfo, true);

			fieldDefinition2.GenCustomColumnDefinitionFk = ZGuid.Empty;
			AssertMandatoryValidationError(fieldDefinition2.GenCustomColumnDefinitionFkInfo, true);
			AssertPropertyIsUniqueInCollectionValidationError(fieldDefinition2.GenCustomColumnDefinitionFkInfo, false);
		}

		public void TestOrder()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();
			var collection = new SalesProductTradeLaneGridColumnDefinitionCollection(salesProduct);

			var fieldDefinition1 = collection.AddNew();
			fieldDefinition1.Order = 0;
			AssertMandatoryValidationError(fieldDefinition1.OrderInfo, true);

			fieldDefinition1.Order = 1;
			AssertMandatoryValidationError(fieldDefinition1.OrderInfo, false);

			var fieldDefinition2 = collection.AddNew();
			fieldDefinition2.Order = 2;
			AssertPropertyIsUniqueInCollectionValidationError(fieldDefinition2.OrderInfo, false);

			fieldDefinition2.Order = 1;
			AssertPropertyIsUniqueInCollectionValidationError(fieldDefinition2.OrderInfo, true);
		}
	}
}
