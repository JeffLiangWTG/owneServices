using CargoWise.EntityFramework.Testing;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class OrgSalesProductCustomColumnDefinitionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckXC_Name()
		{
			var salesProduct1 = Factory.New<OrgSalesProduct>();
			var salesProduct2 = Factory.New<OrgSalesProduct>();

			var columnDefinition1A = salesProduct1.FormLayout.TradeLaneCustomColumnDefinitionCollection.AddNew();
			columnDefinition1A.XC_Name = "AAA";

			var columnDefinition2A = salesProduct2.FormLayout.TradeLaneCustomColumnDefinitionCollection.AddNew();
			columnDefinition2A.XC_Name = "AAA";
			var columnDefinition2B = salesProduct2.FormLayout.TradeLaneCustomColumnDefinitionCollection.AddNew();
			columnDefinition2B.XC_Name = "BBB";

			AssertPropertyIsUniqueInCollectionValidationError(columnDefinition2A.XC_NameInfo, false);
			AssertPropertyIsUniqueInCollectionValidationError(columnDefinition2B.XC_NameInfo, false);

			columnDefinition2B.XC_Name = "AAA";
			AssertPropertyIsUniqueInCollectionValidationError(columnDefinition2B.XC_NameInfo, true);

			columnDefinition2B.XC_Name = "aaa";
			AssertPropertyIsUniqueInCollectionValidationError(columnDefinition2B.XC_NameInfo, true);
		}
	}
}
