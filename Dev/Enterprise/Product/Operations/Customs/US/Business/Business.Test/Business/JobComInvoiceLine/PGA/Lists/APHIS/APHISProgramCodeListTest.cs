using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	class APHISProgramCodeListTest : TestCaseWithFactory
	{
		public void TestGetActiveList()
		{
			var list1 = APHISProgramCodeList.GetActiveList(Factory);
			var list2 = APHISProgramCodeList.GetActiveList(Factory);
			AssertEquals("Data should be cached", list1, list2);
			AssertEquals(4, list1.Count);
			AssertEquals(APHISProgramCodeList.Descriptions.AAC, list1.GetDescriptionFromCode(APHISProgramCodeList.Codes.AAC));
			AssertEquals(APHISProgramCodeList.Descriptions.ABS, list1.GetDescriptionFromCode(APHISProgramCodeList.Codes.ABS));
			AssertEquals(APHISProgramCodeList.Descriptions.APQ, list1.GetDescriptionFromCode(APHISProgramCodeList.Codes.APQ));
			AssertEquals(APHISProgramCodeList.Descriptions.AVS, list1.GetDescriptionFromCode(APHISProgramCodeList.Codes.AVS));
		}
	}
}
