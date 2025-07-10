using CargoWise.CalendarArithmetic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.TransportCommon.Business
{
	public static class TransportWorkingDays
	{
		public static IWorkTimeArithmetic GetWorkTimeArithmetic(BusinessObjectFactory factory, ZGuid departmentPK)
		{
			return WorkingDays.GetInstance(factory, departmentPK, branchPK);
		}

		public static DateCalculator GetDateCalculator(BusinessObjectFactory factory, ZGuid departmentPK)
		{
			return new DateCalculator(factory, departmentPK, branchPK);
		}

		readonly static ZGuid branchPK = GlbBranch.CurrentBranch.PK;

		public static ZDate GetWorkingDate(BusinessObjectFactory factory, ZDateTime time, ZGuid departmentPK)
		{
			var result = ZDate.Empty;
			var workTimeArithmetic = GetWorkTimeArithmetic(factory, departmentPK);
			if (workTimeArithmetic.IsWorkDay(time.ToDateTime()))
			{
				result = time.Date;
			}
			else
			{
				var dateCalculator = new DateCalculator(factory, departmentPK, GlbBranch.CurrentBranch.PK);
				var date = dateCalculator.CalculateDate(time.ToDateTime(), DatesToCalculate.NextBusinessDay).Date;
				result = new ZDateTime(date).Date;
			}

			return result;
		}
	}
}
