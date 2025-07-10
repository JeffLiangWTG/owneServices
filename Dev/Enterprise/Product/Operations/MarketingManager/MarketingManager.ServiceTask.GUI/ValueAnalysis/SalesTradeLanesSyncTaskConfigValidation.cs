namespace Enterprise.MarketingManager.ServiceTask.GUI
{
	public class SalesTradeLanesSyncTaskConfigValidation : AutoSalesTradeLanesSyncTaskConfigValidation
	{
		public SalesTradeLanesSyncTaskConfigValidation(AutoSalesTradeLanesSyncTaskConfig parent) : base(parent)
		{
		}

		protected override void CheckSyncMonthsOnMainSchedule()
		{
			base.CheckSyncMonthsOnMainSchedule();
			if (Parent.SyncMonthsOnMainSchedule < 1 || Parent.SyncMonthsOnMainSchedule > 24)
			{
				Parent.SyncMonthsOnMainScheduleInfo.AddError(Res.GetString("C4012FC8-4B84-46A4-B9AC-AAF1C86FD84A", "The value should be between 1 and 24 months"));
			}
		}

		protected override void CheckSyncYearlyEveryMonths()
		{
			base.CheckSyncYearlyEveryMonths();
			if (Parent.SyncYearly && Parent.SyncYearlyEveryMonths < 1 || Parent.SyncYearlyEveryMonths > 12)
			{
				Parent.SyncYearlyEveryMonthsInfo.AddError(Res.GetString("D31B9E95-0698-4754-91E9-B458981E5719", "The value should be between 1 and 12 months"));
			}
		}
	}
}
