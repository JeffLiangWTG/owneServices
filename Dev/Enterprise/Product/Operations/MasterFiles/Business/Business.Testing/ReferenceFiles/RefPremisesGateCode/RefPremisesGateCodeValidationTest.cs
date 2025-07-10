using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefPremisesGateCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestRegCodeType()
		{
			PremisesGateCode.R5_OrgRegCodeType = "1st";
			AssertNoErrors("R5_OrgRegCodeType should not be empty", PremisesGateCode.R5_OrgRegCodeTypeInfo);
			PremisesGateCode.R5_OrgRegCodeType = "";
			AssertEquals("'Should be errors", true, PremisesGateCode.R5_OrgRegCodeTypeInfo.HasErrors());
		}

		public void TestDataProvide()
		{
			PremisesGateCode.R5_DataProvider = "1st";
			AssertNoErrors("Data Provider should not be empty", PremisesGateCode.R5_DataProviderInfo);
			PremisesGateCode.R5_DataProvider = "";
			AssertEquals("'Should be errors", true, PremisesGateCode.R5_DataProviderInfo.HasErrors());
		}

		public void TestPremisesCode()
		{
			PremisesGateCode.R5_PremisesGateCode = "1st";
			AssertNoErrors("Gate Code should not be empty", PremisesGateCode.R5_PremisesGateCodeInfo);
			PremisesGateCode.R5_PremisesGateCode = "";
			AssertEquals("'Should be errors", true, PremisesGateCode.R5_PremisesGateCodeInfo.HasErrors());
		}

		public void TestPremisesCodeDescription()
		{
			PremisesGateCode.R5_PremisesGateDescription = "1st";
			AssertNoErrors("Gate Code Description should not be empty", PremisesGateCode.R5_PremisesGateDescriptionInfo);
			PremisesGateCode.R5_PremisesGateDescription = "";
			AssertEquals("'Should be errors", true, PremisesGateCode.R5_PremisesGateDescriptionInfo.HasErrors());
		}

		public void TestR5_OrgRegCodeType_ListValidation()
		{
			var bizObj = Factory.New<RefPremisesGateCode>();
			bizObj.R5_OrgRegCodeType = "AAA";

			AssertHasError("R5_OrgRegCodeType", bizObj.R5_OrgRegCodeTypeInfo, "Enter a valid Org. Reg. Code Type.");

			bizObj.R5_OrgRegCodeType = PremiseGateCodeDataProviderList.Codes.OneStop;
			AssertNoError("R5_OrgRegCodeType", bizObj.R5_OrgRegCodeTypeInfo, "Enter a valid Org. Reg. Code Type.");

			bizObj.R5_OrgRegCodeType = "";
			AssertNoError("R5_OrgRegCodeType", bizObj.R5_OrgRegCodeTypeInfo, "Enter a valid Org. Reg. Code Type.");
		}

		public void TestR5_DataProvider_ListValidation()
		{
			var bizObj = Factory.New<RefPremisesGateCode>();
			bizObj.R5_DataProvider = "AAA";

			AssertHasError("R5_DataProvider", bizObj.R5_DataProviderInfo, "Enter a valid Data Provider.");

			bizObj.R5_DataProvider = PremiseGateCodeDataProviderList.Codes.OneStop;
			AssertNoError("R5_DataProvider", bizObj.R5_DataProviderInfo, "Enter a valid Data Provider.");

			bizObj.R5_DataProvider = "";
			AssertNoError("R5_DataProvider", bizObj.R5_DataProviderInfo, "Enter a valid Data Provider.");
		}

		RefPremisesGateCode PremisesGateCode
		{
			get
			{
				RefPremisesGateCode resultPremisesGateCode = fPremisesGateCode;
				if (resultPremisesGateCode == null)
				{
					fPremisesGateCode = Factory.New<RefPremisesGateCode>();
					resultPremisesGateCode = fPremisesGateCode;
				}
				return resultPremisesGateCode;
			}
		}

		RefPremisesGateCode fPremisesGateCode;
	}
}
