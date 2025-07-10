using System;

namespace Enterprise.MasterFiles.Integration
{
	public enum DatesToCalculate
	{
		Storage,
		Available,
		NextBusinessDay
	}

	public interface IDateCalculator
	{
		DateTime CalculateDate(DateTime fromDate, DatesToCalculate typeOfDate);
		DateTime CalculateDate(DateTime fromDate, DatesToCalculate typeOfDate, string transportMode, bool isDangerousGoods);
		DateTime CalculateDate(DateTime fromDate, DatesToCalculate typeOfDate, string transportMode, bool isDangerousGoods, int clientFreeDaysOverride);
	}
}