using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Module.Testing
{
	sealed class PeriodFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPeriodYear()
		{
			Filter.PeriodYear = 9999;
			AssertHasErrorContaining(Filter.PeriodYearInfo, "Year should bigger than");
			Filter.PeriodYear = 1;
			AssertHasErrorContaining(Filter.PeriodYearInfo, "Year should bigger than");
			Filter.PeriodYear = 2015;
			AssertNoNotifications(Filter.PeriodYearInfo);
		}

		public void TestCheckPeriodMonth()
		{
			Filter.PeriodMonth = 13;
			AssertHasErrorContaining(Filter.PeriodMonthInfo, "Month should bigger than");
			Filter.PeriodMonth = -1;
			AssertHasErrorContaining(Filter.PeriodMonthInfo, "Month should bigger than");
			Filter.PeriodMonth = 3;
			AssertNoNotifications(Filter.PeriodMonthInfo);
		}

		PeriodFilter Filter
		{
			get
			{
				return filter ?? (filter = new PeriodFilter("Period", delegate
				{
					return new ZQuery();
				}));
			}
		}
		PeriodFilter filter;
	}
}
