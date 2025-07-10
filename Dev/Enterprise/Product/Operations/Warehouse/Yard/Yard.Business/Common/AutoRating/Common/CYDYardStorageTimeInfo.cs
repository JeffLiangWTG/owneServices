using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDYardStorageTimeInfo(IJobDatesProvider jobDatesProvider, CYDYardUnitState yardUnit, IPeriodicInvoicing invoicing)
	{
		public TimeInfo TimeInfo => timeInfo ??= GetStorageTimeInfo();

		TimeInfo timeInfo;

		public ZDateTime StartDate => startDate ??= GetStartDate();

		ZDateTime? startDate;

		public ZDateTime EndDate => endDate ??= GetEndDate();

		ZDateTime? endDate;

		public int FreeStorageDays => freeStorageDays ??= GetFreeStorageDays();

		int? freeStorageDays;

		TimeInfo GetStorageTimeInfo()
		{
			var timeInfo = new TimeInfo(0, 0, 0);
			if (StartDate.IsValid && EndDate.IsValid)
			{
				timeInfo = new TimeInfo(StartDate, EndDate, FreeStorageDays);
			}
			return timeInfo;
		}

		ZDateTime GetStartDate()
		{
			var yardInDate = GetDateOnly(jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.YardInDate));
			var calculationMethod = yardUnit.Factory.Load<OrgHeader>(invoicing.ET_OH_Client).CompanyData.OB_ARYardStorageCalcMethod;
			return calculationMethod == OrgCompanyDataLookups.YardStorageMethodYardOut
				? yardInDate
				: Max(yardInDate, invoicing.ET_StorageFromDate);
		}

		ZDateTime GetEndDate()
		{
			var yardOutDate = jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.YardOutDate);
			return yardOutDate.IsValid
				? Min(GetDateOnly(yardOutDate), invoicing.ET_StorageToDate)
				: invoicing.ET_StorageToDate;
		}

		int GetFreeStorageDays()
		{
			var minFreeDays = yardUnit.StorageLines
				.Where(sl => sl.PeriodicInvoicing.ET_StorageToDate < StartDate)
				.Select(sl => (int?)sl.YSL_FreeDaysCloseBalance)
				.Min();

			return minFreeDays ?? yardUnit.FreeStorageDays;
		}

		ZDateTime GetDateOnly(ZDateTime dateTime)
		{
			return new ZDateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0);
		}

		ZDateTime Max(ZDateTime dateTime1, ZDateTime dateTime2)
		{
			return dateTime1 > dateTime2 ? dateTime1 : dateTime2;
		}

		ZDateTime Min(ZDateTime dateTime1, ZDateTime dateTime2)
		{
			return dateTime1 < dateTime2 ? dateTime1 : dateTime2;
		}
	}
}
