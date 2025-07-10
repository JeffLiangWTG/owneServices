using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class OrgCusCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOK_CustomsRegNo()
		{
			organisation.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Turkey;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.TerminalControlledPremisesID;
			cusCode.OK_CustomsRegNo = "";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "Please enter a Registration Number / Code.");
			cusCode.OK_CustomsRegNo = "Test";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "A Customs Bonded Terminal Code must be at least 5 characters long and at most 10 characters long");
			cusCode.OK_CustomsRegNo = "TestTestTest";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "A Customs Bonded Terminal Code must be at least 5 characters long and at most 10 characters long");
			cusCode.OK_CustomsRegNo = "Test123";
			AssertNoError(cusCode.OK_CustomsRegNoInfo, "A Customs Bonded Terminal Code must be at least 5 characters long and at most 10 characters long");

			cusCode.OK_CodeType = OrgCusCode.CodeTypes.WarehouseControlledPremisesID;
			cusCode.OK_CustomsRegNo = "";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "Please enter a Registration Number / Code.");
			cusCode.OK_CustomsRegNo = "Test";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "A Customs Bonded Warehouse Code must be at least 5 characters long and at most 10 characters long");
			cusCode.OK_CustomsRegNo = "TestTestTest";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "A Customs Bonded Warehouse Code must be at least 5 characters long and at most 10 characters long");
			cusCode.OK_CustomsRegNo = "Test123";
			AssertNoError(cusCode.OK_CustomsRegNoInfo, "A Customs Bonded Warehouse Code must be at least 5 characters long and at most 10 characters long");

			cusCode.OK_CodeType = OrgCusCode.CodeTypes.SupplierCode;
			cusCode.OK_CustomsRegNo = "";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "Please enter a Registration Number / Code.");
			cusCode.OK_CustomsRegNo = "TestTest10";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, "A Customs Bonded CSC type (Customs Supplier Code) must be 13 characters long.");
			cusCode.OK_CustomsRegNo = "TestTestTes13";
			AssertNoError(cusCode.OK_CustomsRegNoInfo, "A Customs Bonded CSC type (Customs Supplier Code) must be 13 characters long.");
		}

		OrgCusCode cusCode;
		OrgHeader organisation;

		protected override void SetUp()
		{
			base.SetUp();

			organisation = Factory.New<OrgHeader>();
			organisation.MainAddress.OA_RN_NKCountryCode = ZString.Empty;
			cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Turkey;
		}
	}
}

