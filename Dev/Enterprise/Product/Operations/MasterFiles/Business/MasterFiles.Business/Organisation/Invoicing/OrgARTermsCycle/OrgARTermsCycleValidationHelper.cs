using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class OrgARTermsCycleValidationHelper
	{
		public static bool IsValidCalendarDay(ZByte day)
		{
			return 1 <= day && day <= 31;
		}

		public static bool IsValidToDayForPaymentCycle(ZByte day)
		{
			return 1 <= day;
		}
	}
}
