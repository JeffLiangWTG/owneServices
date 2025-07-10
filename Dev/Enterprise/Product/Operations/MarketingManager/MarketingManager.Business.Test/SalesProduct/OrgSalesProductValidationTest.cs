using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class OrgSalesProductValidationTest : BusinessObjectValidationTestCase
	{
		public void TestMP_Code()
		{
			var aaaSalesProduct = Factory.NewWithValidTestData<OrgSalesProduct>();
			aaaSalesProduct.MP_Code = "XXX";
			Factory.Save();

			var salesProduct = Factory.New<OrgSalesProduct>();
			salesProduct.MP_Code = "XXX";
			AssertPropertyIsUniqueInCollectionValidationError(salesProduct.MP_CodeInfo, true);

			salesProduct.MP_Code = "YYY";
			AssertPropertyIsUniqueInCollectionValidationError(salesProduct.MP_CodeInfo, false);
		}

		public void TestMP_Name()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();

			salesProduct.MP_Name = "Broadband";
			AssertMandatoryValidationError(salesProduct.MP_NameInfo, false);

			salesProduct.MP_Name = "";
			AssertMandatoryValidationError(salesProduct.MP_NameInfo, true);
		}

		public void TestMP_FormLayoutDataCanBeEmpty()
		{
			var salesProduct = Factory.New<OrgSalesProduct>();
			AssertEquals("Precondition", ZString.Empty, salesProduct.MP_FormLayoutData);

			salesProduct.Validation.ValidateMP_FormLayoutData();

			AssertNoErrors(salesProduct.MP_FormLayoutDataInfo);
		}
	}
}
