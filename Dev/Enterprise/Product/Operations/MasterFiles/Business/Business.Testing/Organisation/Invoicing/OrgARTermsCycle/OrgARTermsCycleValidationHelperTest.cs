using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgARTermsCycleValidationHelperTest : TestCaseWithFactory
	{
		public void TestIsDayValidForTermCycle()
		{
			Assert(!OrgARTermsCycleValidationHelper.IsValidCalendarDay(0));
			Assert(OrgARTermsCycleValidationHelper.IsValidCalendarDay(1));
			Assert(OrgARTermsCycleValidationHelper.IsValidCalendarDay(15));
			Assert(OrgARTermsCycleValidationHelper.IsValidCalendarDay(31));
			Assert(!OrgARTermsCycleValidationHelper.IsValidCalendarDay(32));
		}

		public void TestIsDayValidForPaymentCycle()
		{
			Assert(!OrgARTermsCycleValidationHelper.IsValidToDayForPaymentCycle(0));
			Assert(OrgARTermsCycleValidationHelper.IsValidToDayForPaymentCycle(1));
			Assert(OrgARTermsCycleValidationHelper.IsValidToDayForPaymentCycle(15));
			Assert(OrgARTermsCycleValidationHelper.IsValidToDayForPaymentCycle(31));
			Assert(OrgARTermsCycleValidationHelper.IsValidToDayForPaymentCycle(50));
		}
	}
}
