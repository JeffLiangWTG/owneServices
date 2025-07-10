using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.AIM.Messaging.Testing
{
	public class AIMDispositionCodesHelperTest : TestCaseWithFactory
	{
		public void TestFields()
		{
			AssertEquals("11", AIMDispositionCodesHelper._11);
			AssertEquals("12", AIMDispositionCodesHelper._12);
			AssertEquals("1G", AIMDispositionCodesHelper._1G);
			AssertEquals("83", AIMDispositionCodesHelper._83);
			AssertEquals("95", AIMDispositionCodesHelper._95);
			AssertEquals("1F", AIMDispositionCodesHelper.CBPLocalTransferAuthorized);
			AssertEquals("1D", AIMDispositionCodesHelper.InbondMovementAuthorized);
			AssertEquals("1E", AIMDispositionCodesHelper.InbondTransferNotAuthorized);
			AssertEquals("P3", AIMDispositionCodesHelper.P3);
		}

		public void TestGetCustomsStatusList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates);
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSAirDispositionCode, "AMSAD", dataGrouping.ZZZ_DataGrouping);
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "Z1", "Z1 DESC", startDate, endDate);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "Z2", "Z2 DESC", startDate, endDate);
			var code3 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "Z3", "Z3 DESC", startDate, endDate);
			Factory.Save();

			var list = AIMDispositionCodesHelper.GetCustomsStatusList(Factory);
			AssertEquals(3, list.Count);
			AssertEquals(code1.ZZD_Description, list.GetDescriptionFromCode(code1.ZZD_Code));
			AssertEquals(code2.ZZD_Description, list.GetDescriptionFromCode(code2.ZZD_Code));
			AssertEquals(code3.ZZD_Description, list.GetDescriptionFromCode(code3.ZZD_Code));
		}
	}
}
