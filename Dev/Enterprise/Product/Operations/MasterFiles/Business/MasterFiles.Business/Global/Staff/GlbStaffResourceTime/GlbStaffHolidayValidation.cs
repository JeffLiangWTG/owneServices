using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffHolidayValidation : AutoGlbStaffHolidayValidation
	{
		public GlbStaffHolidayValidation(AutoGlbStaffHoliday parent)
			: base(parent)
		{
		}

		#region GA_EndTime

		protected override void CheckGA_EndTime()
		{
			base.CheckGA_EndTime();
			MandatoryValidation.CheckEntered(Parent.GA_EndTimeInfo);
			if (Parent.GA_StartTime > Parent.GA_EndTime)
			{
				Parent.GA_EndTimeInfo.AddError(Res.GetString("585cc178-2d40-4ff0-a6c1-43a56224561c", "End time must be after the Start time."));
			}
			CheckOverLappedEndTime();
		}

		void CheckOverLappedEndTime()
		{
			if (Parent.GA_EndTime.IsValid && Parent.GA_ApprovalStatus != GlbStaffHolidayLookupsReal.Declined)
			{
				var endError = Res.GetString("19EB7C6C-8C2A-47D2-8EDD-F12D0B284719", "End time makes overlapped time range with others.");
				var query = GetOverlappedQuery();

				query.AddToFilter(GlbStaffHolidaySchema.GA_StartTime, SQLComparisonOperator.LessThan, Parent.GA_EndTime);
				query.AddToFilter(GlbStaffHolidaySchema.GA_EndTime, SQLComparisonOperator.GreaterThanOrEqualTo, Parent.GA_EndTime);

				if (Parent.Factory.Exists(typeof(GlbStaffHoliday), query))
				{
					if (!Parent.IsInDatabase || Parent.GA_EndTimeInfo.HasChanges)
					{
						Parent.GA_EndTimeInfo.AddError(endError);
					}
					else
					{
						Parent.GA_EndTimeInfo.AddWarning(endError);
					}
				}

				if (Parent.GA_StartTime.IsValid && Parent.GA_EndTimeInfo.HasChanges && !Parent.GA_EndTimeInfo.HasError(endError))
				{
					var queryContain = GetOverlappedQuery();
					queryContain.AddToFilter(GetSubQuery());
					if (Parent.Factory.Exists(typeof(GlbStaffHoliday), queryContain))
					{
						Parent.GA_StartTimeInfo.AddError(endError);
					}
				}
			}
		}

		#endregion

		#region GA_WorkHolidayType

		protected override void CheckGA_WorkHolidayType()
		{
			base.CheckGA_WorkHolidayType();
			MandatoryValidation.CheckEntered(Parent.GA_WorkHolidayTypeInfo);

			if (!Parent.IsInDatabase || Parent.GA_WorkHolidayTypeInfo.HasChanges)
			{
				ListValidation.ErrorIfInvalidCode(Parent.GA_WorkHolidayTypeInfo);
			}
			else
			{
				ListValidation.WarnIfInvalidCode(Parent.GA_WorkHolidayTypeInfo);
			}
		}

		#endregion

		#region GA_StartTime

		protected override void CheckGA_StartTime()
		{
			base.CheckGA_StartTime();
			MandatoryValidation.CheckEntered(Parent.GA_StartTimeInfo);
			if (Parent.GA_StartTime > Parent.GA_EndTime)
			{
				Parent.GA_StartTimeInfo.AddError(Res.GetString("C38331BE-5C35-4159-BC63-AB5884E34397", "Start time must be before the End time."));
			}
			CheckOverLappedStartTime();
		}

		void CheckOverLappedStartTime()
		{
			if (Parent.GA_StartTime.IsValid && Parent.GA_ApprovalStatus != GlbStaffHolidayLookupsReal.Declined)
			{
				var startError = Res.GetString("CF1F735F-600C-4EA7-8559-DC725BB572F6", "Start time makes overlapped time range with others.");
				var query = GetOverlappedQuery();

				query.AddToFilter(GlbStaffHolidaySchema.GA_StartTime, SQLComparisonOperator.LessThanOrEqualTo, Parent.GA_StartTime);
				query.AddToFilter(GlbStaffHolidaySchema.GA_EndTime, SQLComparisonOperator.GreaterThan, Parent.GA_StartTime);

				if (Parent.Factory.Exists(typeof(GlbStaffHoliday), query))
				{
					if (!Parent.IsInDatabase || Parent.GA_StartTimeInfo.HasChanges)
					{
						Parent.GA_StartTimeInfo.AddError(startError);
					}
					else
					{
						Parent.GA_StartTimeInfo.AddWarning(startError);
					}
				}

				if (Parent.GA_EndTime.IsValid && Parent.GA_StartTimeInfo.HasChanges && !Parent.GA_StartTimeInfo.HasError(startError))
				{
					var queryContain = GetOverlappedQuery();
					queryContain.AddToFilter(GetSubQuery());
					if (Parent.Factory.Exists(typeof(GlbStaffHoliday), queryContain))
					{
						Parent.GA_StartTimeInfo.AddError(startError);
					}
				}
			}
		}

		#endregion

		ZQuery GetOverlappedQuery()
		{
			var query = new ZQuery();
			query.AddToFilter(GlbStaffHolidaySchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			query.AddToFilter(GlbStaffHolidaySchema.GA_GS, Parent.GA_GS);
			query.AddToFilter(GlbStaffHolidaySchema.GA_ApprovalStatus, SQLComparisonOperator.NotEqual, GlbStaffHolidayLookupsReal.Declined);
			query.AddToFilter(GlbStaffHolidaySchema.GA_RecordType, SQLComparisonOperator.Equal, GlbStaffHolidayLookups.RecordTypes.Leave);
			return query;
		}

		ZQuery GetSubQuery()
		{
			var subQuery = new ZQuery();
			subQuery.AddToFilter(GlbStaffHolidaySchema.GA_StartTime, SQLComparisonOperator.GreaterThanOrEqualTo, Parent.GA_StartTime);
			subQuery.AddToFilter(GlbStaffHolidaySchema.GA_StartTime, SQLComparisonOperator.LessThan, Parent.GA_EndTime);
			subQuery.AddToFilter(GlbStaffHolidaySchema.GA_EndTime, SQLComparisonOperator.GreaterThan, Parent.GA_StartTime);
			subQuery.AddToFilter(GlbStaffHolidaySchema.GA_EndTime, SQLComparisonOperator.LessThanOrEqualTo, Parent.GA_EndTime);
			subQuery.AddToFilter(GlbStaffHolidaySchema.GA_RecordType, SQLComparisonOperator.Equal, GlbStaffHolidayLookups.RecordTypes.Leave);

			return subQuery;
		}
	}
}
