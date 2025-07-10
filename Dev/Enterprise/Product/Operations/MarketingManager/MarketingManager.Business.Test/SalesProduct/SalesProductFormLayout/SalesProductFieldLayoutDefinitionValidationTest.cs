using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class SalesProductFieldLayoutDefinitionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestGenCustomColumnDefinitionFk()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();
			var fieldDefintion1 = salesProduct.FormLayout.TradeLaneCustomColumnDefinitionCollection.AddNew();
			var fieldDefintion2 = salesProduct.FormLayout.TradeLaneCustomColumnDefinitionCollection.AddNew();

			var collection = new SalesProductTradeDetailFieldLayoutDefinitionCollection(salesProduct);
			var fieldLayoutDefinition1 = collection.AddNew();
			fieldLayoutDefinition1.GenCustomColumnDefinitionFk = fieldDefintion1.PK;
			var fieldLayoutDefinition2 = collection.AddNew();
			fieldLayoutDefinition2.GenCustomColumnDefinitionFk = fieldDefintion2.PK;

			AssertMandatoryValidationError(fieldLayoutDefinition2.GenCustomColumnDefinitionFkInfo, false);
			AssertPropertyIsUniqueInCollectionValidationError(fieldLayoutDefinition2.GenCustomColumnDefinitionFkInfo, false);

			fieldLayoutDefinition2.GenCustomColumnDefinitionFk = fieldDefintion1.PK;
			AssertMandatoryValidationError(fieldLayoutDefinition2.GenCustomColumnDefinitionFkInfo, false);
			AssertPropertyIsUniqueInCollectionValidationError(fieldLayoutDefinition2.GenCustomColumnDefinitionFkInfo, true);

			fieldLayoutDefinition2.GenCustomColumnDefinitionFk = ZGuid.Empty;
			AssertMandatoryValidationError(fieldLayoutDefinition2.GenCustomColumnDefinitionFkInfo, true);
			AssertPropertyIsUniqueInCollectionValidationError(fieldLayoutDefinition2.GenCustomColumnDefinitionFkInfo, false);
		}

		public void TestOrder()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();
			var collection = new SalesProductTradeDetailFieldLayoutDefinitionCollection(salesProduct);

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
