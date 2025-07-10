using System;
using CargoWise.CalendarArithmetic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class DateCalculator : IDateCalculator
	{
		readonly IWorkTimeArithmetic workingDays;
		readonly BusinessObjectFactory factory;
		readonly ZGuid departmentPK;

		public DateCalculator(BusinessObjectFactory factory, ZGuid departmentPK, ZGuid branchPK, ZGuid staffPK = default(ZGuid))
		{
			this.factory = factory;
			this.departmentPK = departmentPK;

			if (staffPK != default(ZGuid))
			{
				factory.AddFetchHint(GlbStaffHolidaySchema.GA_GS, staffPK);
			}

			workingDays = WorkingDays.GetInstance(factory, departmentPK, branchPK, staffPK);
		}

		public DateTime CalculateDate(DateTime fromDate, DatesToCalculate typeOfDate)
		{
			return CalculateDate(fromDate, typeOfDate, "", false);
		}

		public DateTime CalculateDate(DateTime fromDate, DatesToCalculate typeOfDate, string transportMode, bool isDangerousGoods)
		{
			return CalculateDate(fromDate, typeOfDate, transportMode, isDangerousGoods, 0);
		}

		public DateTime CalculateDate(DateTime fromDate, DatesToCalculate typeOfDate, string transportMode, bool isDangerousGoods, int clientFreeDaysOverride)
		{
			var result = DateTime.MinValue;

			if (typeOfDate == DatesToCalculate.NextBusinessDay)
			{
				result = GetWorkingDay(fromDate, 1);
			}
			else if (typeOfDate == DatesToCalculate.Storage)
			{
				int storageFreeDays = clientFreeDaysOverride;

				if (isDangerousGoods)
				{
					storageFreeDays = (transportMode == Constants.TransportModes.Air)
						? EnvProxy.Instance.Registry.CFSAirFreightDGLCLStorageFreeDays
						: EnvProxy.Instance.Registry.CFSSeaFreightDGLCLStorageFreeDays;
				}
				else if (clientFreeDaysOverride == 0
					|| (transportMode == Constants.TransportModes.Air && !CFSDataRegistry.Instance.CFSAirFreightUseClientFreeDays.Value)
					|| (transportMode == Constants.TransportModes.Sea && !CFSDataRegistry.Instance.CFSSeaFreightUseClientFreeDays.Value))
				{
					storageFreeDays = (transportMode == Constants.TransportModes.Air)
						? EnvProxy.Instance.Registry.CFSAirFreightLCLStorageFreeDays
						: EnvProxy.Instance.Registry.CFSSeaFreightLCLStorageFreeDays;
				}

				result = GetWorkingDay(fromDate, storageFreeDays);
			}
			else if (typeOfDate == DatesToCalculate.Available)
			{
				AvailableCommence registrySetting;
				if (transportMode == Constants.TransportModes.Air)
				{
					registrySetting = isDangerousGoods ? EnvProxy.Instance.Registry.CFSAirFreightDGLCLAvailableCommences : EnvProxy.Instance.Registry.CFSAirFreightLCLAvailableCommences;
				}
				else
				{
					registrySetting = isDangerousGoods ? EnvProxy.Instance.Registry.CFSSeaFreightDGLCLAvailableCommences : EnvProxy.Instance.Registry.CFSSeaFreightLCLAvailableCommences;
				}
				result = GetDateFromRegistrySetting(fromDate, registrySetting);
			}

			return result;
		}

		DateTime GetDateFromRegistrySetting(DateTime fromDate, AvailableCommence setting)
		{
			var result = GetWorkingDay(fromDate, 1);

			switch (setting)
			{
				case AvailableCommence.IME:
					result = fromDate;
					break;

				case AvailableCommence.NBD:
					result = GetWorkingDay(fromDate, 1);
					break;

				case AvailableCommence.NCD:
					result = fromDate.AddDays(1);
					break;
			}

			return result;
		}

		DateTime GetWorkingDay(DateTime fromDate, int noOfWorkingDaysFromToday)
		{
			var result = fromDate;

			for (int i = 0; i < noOfWorkingDaysFromToday; i++)
			{
				result = result.AddDays(1);
				int noOfHolidays = 0;

				while (workingDays.IsDateTimeAHoliday(result) || !IsDayANormalWorkingDay(result))
				{
					result = result.AddDays(1);

					//if there are no working days within 30 days of the date, then assume that every day is a working day
					noOfHolidays++;

					if (noOfHolidays > 30)
					{
						return fromDate.AddDays(noOfWorkingDaysFromToday);
					}
				}
			}

			return result;
		}

		bool IsDayANormalWorkingDay(DateTime dateToCheck)
		{
			if (!departmentPK.IsValid || dateToCheck > ZDateTime.MaxSmallDateTime)
			{
				return !workingDays.IsDateAWeekend(dateToCheck);
			}
			else
			{
				var query = new ZQuery(GlbWorkTimeSchema.GW_ParentID, departmentPK);
				query.AddToFilter(GlbWorkTimeSchema.GW_ParentTableCode, GlbDepartmentSchema.Constants.Prefix);
				query.AddToFilter(GlbWorkTimeSchema.GW_DayOfWeek, GetDayCode(dateToCheck.DayOfWeek));
				return factory.Exists(typeof(GlbWorkTime), query);
			}
		}

		public static string GetDayCode(DayOfWeek day)
		{
			switch (day)
			{
				case DayOfWeek.Monday:
					return AutoDayOfWeekCodeList.Codes.Monday;
				case DayOfWeek.Tuesday:
					return AutoDayOfWeekCodeList.Codes.Tuesday;
				case DayOfWeek.Wednesday:
					return AutoDayOfWeekCodeList.Codes.Wednesday;
				case DayOfWeek.Thursday:
					return AutoDayOfWeekCodeList.Codes.Thursday;
				case DayOfWeek.Friday:
					return AutoDayOfWeekCodeList.Codes.Friday;
				case DayOfWeek.Saturday:
					return AutoDayOfWeekCodeList.Codes.Saturday;
				case DayOfWeek.Sunday:
					return AutoDayOfWeekCodeList.Codes.Sunday;
				default:
					throw new ArgumentException("day must be MON-SUN");
			}
		}
	}
}
