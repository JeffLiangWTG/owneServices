using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Customs.TR.Business
{
	public static class StampDutyLedgerNumberFountainHelper
	{
		public static ZString GetNextStampDutyLedgerNumber(BusinessObjectFactory factory, ZString companyCode)
		{
			var registry = TRCustomsDataRegistry.Instance.StampDutyLedgerNumberCustomization.Value;
			var year = GetFiscalYear(registry.StartDate, registry.EndDate);

			var minValue = registry.ExpiredYear < year ? 1 : (long)registry.StartNumber;
			return Env.NumberFountains.GetTRStampDutyLedgerNumber(companyCode, year.ToString(), minValue).GetNextFormatted(factory);
		}

		public static int GetFiscalYear(ZDateTime startDate, ZDateTime endDate)
		{
			var today = ZDateTime.Today;
			var currentYear = today.Year;

			if (startDate.IsEmpty)
			{
				if (endDate.IsEmpty)
				{
					startDate = new ZDateTime(currentYear, 01, 01);
					endDate = new ZDateTime(currentYear, 12, 31);
				}
				else
				{
					startDate = endDate.AddYears(-1).AddDays(1);
				}
			}
			else
			{
				if (endDate.IsEmpty)
				{
					endDate = startDate.AddYears(1).AddDays(-1);
				}
			}

			var year = currentYear;

			if (currentYear == endDate.Year)
			{
				if (today > endDate)
				{
					year = currentYear + 1;
				}
			}
			else if (currentYear > endDate.Year)
			{
				if (today.Month > endDate.Month)
				{
					year = currentYear + 1;
				}
			}
			else
			{
				if (today < startDate)
				{
					if (today.Month >= startDate.Month)
					{
						year = currentYear + 1;
					}
				}
				else
				{
					year = endDate.Year;
				}
			}

			return year;
		}
	}
}
