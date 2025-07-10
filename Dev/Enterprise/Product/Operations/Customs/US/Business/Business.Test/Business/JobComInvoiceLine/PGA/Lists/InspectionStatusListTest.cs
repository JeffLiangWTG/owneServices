using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	class InspectionStatusListTest : TestCaseWithFactory
	{
		public void TestGetListForAPHIS()
		{
			var categoryTypes = new APHISCategoryTypeCodeList();
			var list1 = InspectionStatusList.GetListForAPHIS(Factory);
			var list2 = InspectionStatusList.GetListForAPHIS(Factory);
			AssertEquals("Cached", list1, list2);
			AssertEquals("Count", 2, list2.Count);
			AssertEquals(InspectionStatusList.Codes.PreviouslyPerformed, InspectionStatusList.Descriptions.PreviouslyPerformed, list2.GetDescriptionFromCode(InspectionStatusList.Codes.PreviouslyPerformed));
			AssertEquals(InspectionStatusList.Codes.BTAAnticipatedArrivalInformation, InspectionStatusList.BTAAnticipatedArrivalInformationDescriptionForAPHIS, list2.GetDescriptionFromCode(InspectionStatusList.Codes.BTAAnticipatedArrivalInformation));
		}
	}
}
