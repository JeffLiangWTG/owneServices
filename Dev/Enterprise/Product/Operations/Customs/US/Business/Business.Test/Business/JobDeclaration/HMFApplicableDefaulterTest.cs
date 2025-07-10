using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	class HMFApplicableDefaulterTest : TestCaseWithFactory
	{
		public void TestGetCalculatedHMFApplicable()
		{
			HMFApplicableDefaulter hMF = new HMFApplicableDefaulter();
			AssertEquals(YesNoDefaultList.Codes.No, hMF.GetCalculatedHMFApplicable(TransportTypeList.Codes.Sea, EntryTypeList.Codes.InformalFreeDutiable, ""));
			AssertEquals(YesNoDefaultList.Codes.Yes, hMF.GetCalculatedHMFApplicable(TransportTypeList.Codes.Sea, EntryTypeList.Codes.ConsumptionFreeDutiable, ""));
			AssertEquals(YesNoDefaultList.Codes.Yes, hMF.GetCalculatedHMFApplicable(TransportTypeList.Codes.BorderWaterBorne, EntryTypeList.Codes.InformalFreeDutiable, ""));
			AssertEquals(YesNoDefaultList.Codes.No, hMF.GetCalculatedHMFApplicable(TransportTypeList.Codes.Mail, EntryTypeList.Codes.ConsumptionFreeDutiable, ""));

			AssertEquals(YesNoDefaultList.Codes.No, hMF.GetCalculatedHMFApplicable(TransportTypeList.Codes.Sea, EntryTypeList.Codes.ConsumptionFreeDutiable, "3302"));
			AssertEquals(YesNoDefaultList.Codes.Yes, hMF.GetCalculatedHMFApplicable(TransportTypeList.Codes.Sea, EntryTypeList.Codes.ConsumptionFreeDutiable, "3901"));
			AssertEquals(YesNoDefaultList.Codes.No, hMF.GetCalculatedHMFApplicable(TransportTypeList.Codes.Air, EntryTypeList.Codes.ConsumptionFreeDutiable, "3901"));

			AssertEquals(YesNoDefaultList.Codes.No, hMF.GetCalculatedHMFApplicable(TransportTypeList.Codes.Air, EntryTypeList.Codes.ReWarehouse, "3901"));
			AssertEquals(YesNoDefaultList.Codes.No, hMF.GetCalculatedHMFApplicable(TransportTypeList.Codes.Air, EntryTypeList.Codes.WarehouseFTZ, "3901"));
		}
	}
}
