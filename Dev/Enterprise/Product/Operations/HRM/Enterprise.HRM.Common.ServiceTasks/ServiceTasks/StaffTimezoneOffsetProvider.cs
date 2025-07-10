using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.HRM.Common
{
	public static class StaffTimezoneOffsetProvider
	{
		public static readonly TimeSpan InvalidOffset = new TimeSpan(13, 59, 0);

		public static TimeSpan GetOffset(BusinessObjectFactory factory, ZGuid staffPk, DateTimeOffset date)
			=> OffsetFromTimezoneHistory(factory, staffPk, date) ?? OffsetFromHomeBranch(factory, staffPk, date) ?? InvalidOffset;

		static TimeSpan? OffsetFromHomeBranch(BusinessObjectFactory factory, ZGuid staffPk, DateTimeOffset date)
		{
			var staff = factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffPk));

			var port = staff?.HomeBranch?.GB_RL_NKHomePort;
			var unloco = port.HasValue ? factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, port)) : null;

			return date.TimeSpan(unloco?.TimeZoneSet);
		}

		static TimeSpan? OffsetFromTimezoneHistory(BusinessObjectFactory factory, ZGuid staffPk, DateTimeOffset date)
		{
			var query = new ZQuery(GlbStaffTimezoneSchema.GSZ_GS_Staff, staffPk);
			query.AddToFilter(GlbStaffTimezoneSchema.GSZ_EffectiveDate, SQLComparisonOperator.LessThan, date);

			var endDateQuery = new ZQuery(GlbStaffTimezoneSchema.GSZ_AutoEffectiveEndDate, DBNull.Value);
			endDateQuery.AddToFilter(JoinCondition.Or, GlbStaffTimezoneSchema.GSZ_AutoEffectiveEndDate, SQLComparisonOperator.GreaterThan, date);

			query.AddToFilter(endDateQuery);

			var staffTimezone = factory.LoadTop1<GlbStaffTimezone>(query);
			return date.TimeSpan(staffTimezone?.TimeZoneSetName);
		}

		static TimeSpan? TimeSpan(this DateTimeOffset date, RefTimeZoneSet timeZoneSet)
		{
			RefTimeZone timezone = null;

			var unlocoTimeZone = timeZoneSet?.GetCalculationTimeZone();
			if (unlocoTimeZone != null)
			{
				timezone = unlocoTimeZone.IsDaylightSavingBasedOnUtc(date.UtcDateTime)
					? (timeZoneSet?.DaylightSavingZone)
					: timeZoneSet?.StandardZone;
			}

			return timezone?.Timespan();
		}

		static TimeSpan Timespan(this RefTimeZone timezone)
			=> System.TimeSpan.FromTicks(System.TimeSpan.FromMinutes(1).Ticks * timezone.R2_OffsetMinutesFromUTC);
	}
}
