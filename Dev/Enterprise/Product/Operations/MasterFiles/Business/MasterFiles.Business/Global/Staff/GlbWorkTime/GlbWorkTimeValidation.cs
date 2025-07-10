using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbWorkTimeValidation : AutoGlbWorkTimeValidation
	{
		public GlbWorkTimeValidation(AutoGlbWorkTime parent) : base(parent)
		{
		}

		//hide start time auto validation for year 1900
		protected override void CheckGW_StartTimeIsValidZDateTimeRange() { }
		//hide end time auto validation for year 1900
		protected override void CheckGW_EndTimeIsValidZDateTimeRange() { }

		protected override void CheckGW_StartTime()
		{
			base.CheckGW_StartTime();

			if (HasOverlappingTimeRange())
			{
				Parent.GW_StartTimeInfo.AddError(Res.GetString("BE7EA9CF-0FF3-49EC-874E-776C8D55860B", "Working times should not overlap."));
			}
		}

		protected override void CheckGW_EndTime()
		{
			base.CheckGW_EndTime();

			if (HasOverlappingTimeRange())
			{
				Parent.GW_EndTimeInfo.AddError(Res.GetString("26844C20-6628-404A-B3A2-23CFC3BA46A2", "Working times should not overlap."));
			}
		}

		bool HasOverlappingTimeRange()
		{
			if (Parent.GW_StartTime.IsEmpty || Parent.GW_EndTime.IsEmpty)
			{
				return false;
			}

			var timeRangeFilter = new ZQuery(GlbWorkTimeSchema.GW_StartTime, SQLComparisonOperator.LessThan, Parent.GW_EndTime)
				.AddToFilter(new ZQuery(GlbWorkTimeSchema.GW_EndTime, SQLComparisonOperator.GreaterThan, Parent.GW_StartTime), JoinCondition.And);

			var queryToFindConflicts = new ZQuery(GlbWorkTimeSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			queryToFindConflicts.AddToFilter(GlbWorkTimeSchema.GW_ParentID, Parent.GW_ParentID);
			queryToFindConflicts.AddToFilter(GlbWorkTimeSchema.GW_ParentTableCode, Parent.GW_ParentTableCode);
			queryToFindConflicts.AddToFilter(GlbWorkTimeSchema.GW_DayOfWeek, Parent.GW_DayOfWeek);
			queryToFindConflicts.AddToFilter(timeRangeFilter, JoinCondition.And);

			var conflictingWorktimes = Parent.Factory.Load<GlbWorkTime>(queryToFindConflicts);
			return conflictingWorktimes.Length > 0;
		}
	}
}
