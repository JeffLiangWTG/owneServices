using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruitment.Copyback
{
	public class ColumnCopybackProcessor : IColumnCopybackProcessor
	{
		public void StaffWorkingBasis(BusinessObjectFactory factory, GlbStaff staff, IQueuedLog log)
			=> ColumnCopybackProcessorImpl.StaffWorkingBasis(factory, staff, log);

		public void EmploymentHistory(BusinessObjectFactory factory, GlbStaff staff, IQueuedLog log)
			=> ColumnCopybackProcessorImpl.EmploymentHistory(factory, staff, log);

		public void WorkPattern(BusinessObjectFactory factory, GlbStaff staff, IQueuedLog log)
			=> ColumnCopybackProcessorImpl.WorkPattern(factory, staff, log);

		public void StaffManager(BusinessObjectFactory factory, GlbStaff staff, IQueuedLog log)
			=> ColumnCopybackProcessorImpl.StaffManager(factory, staff, log);
	}

	public static class ColumnCopybackProcessorImpl
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "SQL strings")]
		public static void StaffWorkingBasis(BusinessObjectFactory factory, GlbStaff staff, IQueuedLog log)
		{
			using (var hrmConnection = Db.NewExtraUnrestrictedWriterConnection(
				Db.ServerName,
				Db.DatabaseName))
			{
				var result = hrmConnection.ExecuteScalar(@"
					SELECT GSW_WorkingBasis
					FROM hrm.GlbStaffWorkingBasis
					WHERE GSW_GS_Staff=@staffPk
						AND GSW_EffectiveDate < @now
						AND (GSW_AutoEffectiveEndDate IS NULL OR GSW_AutoEffectiveEndDate > @now)", AddParameters);

				void AddParameters(DbCommand cmd)
				{
					cmd.AddParameterBasedOnDbColumn("@staffPk", staff.PK.ToGuid(), GlbStaffWorkingBasisSchema.GSW_GS_Staff);
					cmd.AddParameterBasedOnDbColumn("@now", ZDateTimeOffset.UtcNow.ToDateTimeOffset(), GlbStaffWorkingBasisSchema.GSW_EffectiveDate);
				}

				if (result is string basis)
				{
					staff.GS_EmploymentBasis = basis;
				}
			}
		}

		public static void EmploymentHistory(BusinessObjectFactory factory, GlbStaff staff, IQueuedLog log)
		{
			var query = GenerateCopybackQuery(staff.PK);

			var employmentHistory = factory.Load<GlbEmploymentHistory>(query).SingleOrDefault();
			if (employmentHistory != null && !employmentHistory.GEH_JobTitle.IsEmpty)
			{
				staff.GS_Title = employmentHistory.GEH_JobTitle;
			}
		}

		static ZQuery GenerateCopybackQuery(ZGuid staffPK)
		{
			var utcNow = ZDateTime.UtcNow;

			var query = new ZDBOnlyQuery(typeof(GlbEmploymentHistory));
			query.AddToFilter(GlbEmploymentHistorySchema.GEH_GS_Staff, staffPK);
			query.AddToFilter(JoinCondition.And, GlbEmploymentHistorySchema.GEH_EffectiveDate, SQLComparisonOperator.LessThanOrEqualTo, utcNow);
			query.AddToFilter(JoinCondition.And, GlbEmploymentHistorySchema.GEH_IsApproved, SQLComparisonOperator.Equal, true);

			var autoEffectiveEndDateColumn = GlbEmploymentHistorySchema.GEH_AutoEffectiveEndDate;
			var sub = new ZQuery(autoEffectiveEndDateColumn, null);
			_ = sub.AddToFilter(JoinCondition.Or, autoEffectiveEndDateColumn, SQLComparisonOperator.GreaterThan, utcNow);

			query.AddToFilter(sub, JoinCondition.And);

			return query;
		}

		public static void WorkPattern(BusinessObjectFactory factory, GlbStaff staff, IQueuedLog log)
		{
			// load work pattern
			var query = new ZQuery(GlbWorkPatternSchema.GWP_GS_Staff, staff.PK);
			_ = query.AddToFilter(JoinCondition.And, GlbWorkPatternSchema.GWP_EffectiveDate, SQLComparisonOperator.LessThanOrEqualTo, ZDateTimeOffset.UtcNow);
			query.OrderBy = GlbWorkPatternSchema.GWP_EffectiveDate.Name + " DESC";

			var currentWorkPattern = factory.LoadTop1<GlbWorkPattern>(query);
			if (currentWorkPattern == null)
			{
				var staffCode = factory.Load<GlbStaff>(staff.PK).GS_Code;

				// effective date, create time, last edit time of next work pattern
				var nextWPQuery = new ZQuery(GlbWorkPatternSchema.GWP_GS_Staff, staff.PK);
				_ = nextWPQuery.AddToFilter(JoinCondition.And, GlbWorkPatternSchema.GWP_EffectiveDate, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTimeOffset.UtcNow);
				nextWPQuery.OrderBy = GlbWorkPatternSchema.GWP_EffectiveDate.Name + " ASC";
				var nextWP = factory.LoadTop1<GlbWorkPattern>(nextWPQuery);

				ErrorReporter.ReportOnce("CannotFindWorkPattern", $"An ECL log exists that references a work pattern that doesn't exist. Query={query.LiteralTextSqlFormatted} StaffCode={staffCode} EffectiveDate={nextWP?.GWP_EffectiveDate} CreateTime={nextWP?.GWP_SystemCreateTimeUtc} LastEditTime={nextWP?.GWP_SystemLastEditTimeUtc} EventTime={log?.EventTime} PostedTime={log?.PostedTimeUtc}");
				return;
			}

			// delete all work times for GS parent
			var deleteGSWorkTimesQuery = new ZQuery(GlbWorkTimeSchema.GW_ParentTableCode, GlbStaffSchema.Constants.Prefix);
			_ = deleteGSWorkTimesQuery.AddToFilter(JoinCondition.And, GlbWorkTimeSchema.GW_ParentID, staff.PK);
			factory.Load<GlbWorkTime>(deleteGSWorkTimesQuery).ForEach(wt => wt.Delete());

			// load all the work times for GWP parent
			var currentGWPWorkTimesQuery = new ZQuery(GlbWorkTimeSchema.GW_ParentTableCode, GlbWorkPatternSchema.Constants.Prefix);
			_ = currentGWPWorkTimesQuery.AddToFilter(JoinCondition.And, GlbWorkTimeSchema.GW_ParentID, currentWorkPattern.PK);
			var currentGWPWorkTimes = factory.Load<GlbWorkTime>(currentGWPWorkTimesQuery);

			// create new rows in work times for GS from GWP
			var newGSWorksTimes = currentGWPWorkTimes;
			foreach (var currentWorkTime in currentGWPWorkTimes)
			{
				var newWorkTime = factory.New<GlbWorkTime>();
				newWorkTime.GW_ParentID = staff.PK;
				newWorkTime.GW_ParentTableCode = GlbStaffSchema.Constants.Prefix;
				newWorkTime.GW_DayOfWeek = currentWorkTime.GW_DayOfWeek;
				newWorkTime.GW_StartTime = currentWorkTime.GW_StartTime;
				newWorkTime.GW_EndTime = currentWorkTime.GW_EndTime;
			}
		}

		public static void StaffManager(BusinessObjectFactory factory, GlbStaff staff, IQueuedLog log)
		{
			staff.GS_SystemLastEditTimeUtc = ZDateTime.UtcNow;
		}
	}
}
