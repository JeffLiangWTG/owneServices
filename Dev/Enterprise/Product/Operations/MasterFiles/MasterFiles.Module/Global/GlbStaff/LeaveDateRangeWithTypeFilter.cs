using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class LeaveDateRangeWithTypeFilter : ModuleDateFilter
	{
		public LeaveDateRangeWithTypeFilter(ZString description) : base(description)
		{
		}

		#region Clear / IsEmpty

		protected override void ClearCore()
		{
			base.ClearCore();
			WorkHolidayType = ZString.Empty;
		}

		protected override bool IsEmptyCore => base.IsEmptyCore && WorkHolidayType.IsEmpty;

		#endregion

		#region Properties

		[MaxLength(AutoGlbStaffHoliday.Schema.GA_WorkHolidayTypeMaxLength)]
		[List("WorkHolidayTypes")]
		public ZString WorkHolidayType
		{
			get => workHolidayType;
			set
			{
				if (workHolidayType != value)
				{
					CheckMaximumLength(WorkHolidayTypeInfo, value);
					workHolidayType = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateWorkHolidayType();
					}

					WorkHolidayTypeInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		ZString workHolidayType;

		public ZPropertyInfo WorkHolidayTypeInfo => GetZPropertyInfo(nameof(WorkHolidayType));

		#endregion

		#region Query

		protected override ZQuery GetQuery()
		{
			if (IsEmpty)
			{
				return new ZQuery();
			}

			var staffQuery = new ZDBOnlyQuery(typeof(GlbStaff));
			var subQuery = new ZDBOnlySubQuery(typeof(GlbStaffHoliday), GlbStaffHolidaySchema.GA_GS);

			var overLapStartQuery = new ZQuery();
			var overLapEndQuery = new ZQuery();
			var overLapStartAndEndQuery = new ZQuery();
			var holidayTypeQuery = new ZQuery();

			if (!ToDate.IsEmpty && !FromDate.IsEmpty)
			{
				overLapStartQuery.AddToFilter(GlbStaffHolidaySchema.GA_StartTime, SQLComparisonOperator.LessThanOrEqualTo, ToDate);
				overLapStartQuery.AddToFilter(JoinCondition.And, GlbStaffHolidaySchema.GA_StartTime, SQLComparisonOperator.GreaterThanOrEqualTo, FromDate);

				overLapEndQuery.AddToFilter(GlbStaffHolidaySchema.GA_EndTime, SQLComparisonOperator.LessThanOrEqualTo, ToDate);
				overLapEndQuery.AddToFilter(JoinCondition.And, GlbStaffHolidaySchema.GA_EndTime, SQLComparisonOperator.GreaterThanOrEqualTo, FromDate);

				overLapStartAndEndQuery.AddToFilter(GlbStaffHolidaySchema.GA_StartTime, SQLComparisonOperator.LessThanOrEqualTo, FromDate);
				overLapStartAndEndQuery.AddToFilter(JoinCondition.And, GlbStaffHolidaySchema.GA_EndTime, SQLComparisonOperator.GreaterThanOrEqualTo, ToDate);
			}
			else if (!ToDate.IsEmpty)
			{
				overLapStartQuery.AddToFilter(GlbStaffHolidaySchema.GA_StartTime, SQLComparisonOperator.LessThanOrEqualTo, ToDate);
				overLapEndQuery.AddToFilter(GlbStaffHolidaySchema.GA_EndTime, SQLComparisonOperator.LessThanOrEqualTo, ToDate);
			}
			else if (!FromDate.IsEmpty)
			{
				overLapStartQuery.AddToFilter(GlbStaffHolidaySchema.GA_StartTime, SQLComparisonOperator.GreaterThanOrEqualTo, FromDate);
				overLapEndQuery.AddToFilter(GlbStaffHolidaySchema.GA_EndTime, SQLComparisonOperator.GreaterThanOrEqualTo, FromDate);
			}
			else if (IsPropertySearchUsingHasNoDateEntered)
			{
				staffQuery.AddSubQuery(new ZDBOnlySubQuery(typeof(GlbStaffHoliday), GlbStaffHolidaySchema.GA_GS, true), JoinCondition.And);
				return staffQuery;
			}

			if (!WorkHolidayType.IsEmpty)
			{
				holidayTypeQuery.AddToFilter(GlbStaffHolidaySchema.GA_WorkHolidayType, SQLComparisonOperator.Equal, workHolidayType);
			}

			subQuery.AddToFilter(overLapStartQuery, JoinCondition.Or);
			subQuery.AddToFilter(overLapEndQuery, JoinCondition.Or);
			subQuery.AddToFilter(overLapStartAndEndQuery, JoinCondition.Or);
			subQuery.AddToFilter(holidayTypeQuery, JoinCondition.And);
			staffQuery.AddSubQuery(subQuery, JoinCondition.And);

			return staffQuery;
		}

		#endregion

		#region XML Serialization

		protected override void DeserializePropertiesFromXml(System.Xml.XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);
			if (reader.Name == "WorkHolidayType")
			{
				WorkHolidayType = reader.ReadElementString("WorkHolidayType");
			}
		}

		protected override void SerializePropertiesToXml(System.Xml.XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);
			writer.WriteElementString("WorkHolidayType", WorkHolidayType);
		}

		#endregion

		#region Lookups

		public CodeDescriptionBoolCollection WorkHolidayTypes => SystemDataRegistry.Instance.StaffLeaveTypes.Value;

		#endregion

		#region Validation

		public new LeaveDateRangeWithTypeFilterValidation Validation => (LeaveDateRangeWithTypeFilterValidation)base.Validation;

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new LeaveDateRangeWithTypeFilterValidation(this);
		}

		#endregion
	}
}
