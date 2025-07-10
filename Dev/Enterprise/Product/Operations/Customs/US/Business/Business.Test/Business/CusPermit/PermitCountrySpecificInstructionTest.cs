using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class PermitCountrySpecificInstructionTest : TestCaseWithFactory
	{
		public void TestGetTypeList()
		{
			AssertType<PermitTypeList>(countrySpecificInstruction.GetTypeList());
		}

		public void TestGetTransactionTypeList()
		{
			AssertType<Customs.Business.PermitTransactionTypeList>(countrySpecificInstruction.GetTransactionTypeList("", ""));
			CombineAssertions("Transaction Types for FTZ Permit Type: ", () =>
			{
				var list = countrySpecificInstruction.GetTransactionTypeList(PermitTypeList.Codes.FTZ, "");
				Assert("FTZ", list.ContainsCode(PermitTransactionTypeList.Codes.FTZ));
				Assert("OBL", list.ContainsCode(PermitTransactionTypeList.Codes.OBL));
				Assert("ADJ", list.ContainsCode(PermitTransactionTypeList.Codes.ADJ));
				Assert("CUS", list.ContainsCode(PermitTransactionTypeList.Codes.CUS));
				Assert("TRA", list.ContainsCode(PermitTransactionTypeList.Codes.TRA));
			});
		}

		public void TestGetRuleCodeList()
		{
			AssertType<Customs.Business.PermitRuleCodeList>(countrySpecificInstruction.GetRuleCodeList("", ""));
			CombineAssertions("Rule Codes for FTZ Permit Type: ", () =>
			{
				var list = countrySpecificInstruction.GetRuleCodeList(PermitTypeList.Codes.FTZ, "");
				Assert("COO", list.ContainsCode(USPermitRuleCodeList.Codes.COO));
				Assert("PRD", list.ContainsCode(USPermitRuleCodeList.Codes.PRD));
				Assert("TAR", list.ContainsCode(USPermitRuleCodeList.Codes.TAR));
				Assert("ZST", list.ContainsCode(USPermitRuleCodeList.Codes.ZST));
				Assert("FRM", list.ContainsCode(USPermitRuleCodeList.Codes.FRM));
			});
		}

		public void TestAppliesToIndicator()
		{
			AssertEquals("Applies To = ForceEmpty for no type", Customs.Business.AppliesToIndicator.ForceEmpty, countrySpecificInstruction.GetAppliesToIndicator("", ""));
			AssertEquals("Applies To = Optional for FTZ", Customs.Business.AppliesToIndicator.Optional, countrySpecificInstruction.GetAppliesToIndicator(PermitTypeList.Codes.FTZ, ""));
		}

		public void TestGetMatchingType()
		{
			AssertEquals("Default to Range", Customs.Business.PermitMatchingType.Range, countrySpecificInstruction.GetMatchingType(""));
			AssertEquals("ZST = Single Value", Customs.Business.PermitMatchingType.SingleValue, countrySpecificInstruction.GetMatchingType(USPermitRuleCodeList.Codes.ZST));
			AssertEquals("FRM = Single Value", Customs.Business.PermitMatchingType.SingleValue, countrySpecificInstruction.GetMatchingType(USPermitRuleCodeList.Codes.FRM));
		}

		public void TestIsRuleExceptionApplicable()
		{
			Assert("Default to Applicable", countrySpecificInstruction.IsRuleExceptionApplicable(""));
			Assert("ZST = Rule Exception Not Applicable", !countrySpecificInstruction.IsRuleExceptionApplicable(USPermitRuleCodeList.Codes.ZST));
		}

		public void TestGetValueFromFieldType()
		{
			AssertEquals("Text", countrySpecificInstruction.GetValueFromFieldType(USPermitRuleCodeList.Codes.TAR));
			AssertEquals("TextDropEdit", countrySpecificInstruction.GetValueFromFieldType(USPermitRuleCodeList.Codes.ZST));
			AssertEquals("TextCodeFindBox", countrySpecificInstruction.GetValueFromFieldType(USPermitRuleCodeList.Codes.FRM));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var permitHeader = Factory.New<CusPermitHeader>();
			countrySpecificInstruction = (PermitCountrySpecificInstruction)permitHeader.CountrySpecificInstruction;
		}

		PermitCountrySpecificInstruction countrySpecificInstruction;
	}
}
