using System;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class RefTransitTimeDetailValidation : AutoRefTransitTimeDetailValidation
	{
		public RefTransitTimeDetailValidation(AutoRefTransitTimeDetail parent) : base(parent)
		{
		}

		public new RefTransitTimeDetail Parent
		{
			get { return (RefTransitTimeDetail)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateTransitDays();
			ValidateTransitHours();
			ValidateDayOfWeek();
		}

		public void ValidateTransitDays()
		{
			ValidateCalculatedProperty(Parent.TransitDaysInfo);
		}

		protected virtual void CheckTransitDays()
		{
			MandatoryValidation.CheckNotNegative(Parent.TransitDaysInfo);

			if (Parent.TransitDays == 0 && Parent.TransitHours == 0)
			{
				Parent.TransitDaysInfo.AddError(TransitTimeGreaterThanZeroError);
			}
		}

		public void ValidateTransitHours()
		{
			ValidateCalculatedProperty(Parent.TransitHoursInfo);
		}

		protected virtual void CheckTransitHours()
		{
			MandatoryValidation.CheckNotNegative(Parent.TransitHoursInfo);

			if (Parent.TransitDays == 0 && Parent.TransitHours == 0)
			{
				Parent.TransitHoursInfo.AddError(TransitTimeGreaterThanZeroError);
			}

			ValidateArrivalTimeAndTransitHour(Parent.TransitHoursInfo);

			if (Parent.TransitHours >= 24)
			{
				Parent.TransitHoursInfo.AddError(Res.GetString("4132839a-5630-447e-b27b-7eec99d8f07c", "Hours must be less than 24. Please use the 'Days' component for longer times."));
			}
		}

		static string TransitTimeGreaterThanZeroError
		{
			get { return Res.GetString("4fa8c52a-06b6-410b-89bd-c34301864b92", "Please specify a Transit time greater than zero."); }
		}

		public void ValidateDayOfWeek()
		{
			ValidateCalculatedProperty(Parent.DayOfWeekInfo);
		}

		protected virtual void CheckDayOfWeek()
		{
			if (Parent.RTD_DayOfWeek < 1 || Parent.RTD_DayOfWeek > 7)
			{
				Parent.DayOfWeekInfo.AddError(Res.GetString("79ab1163-2522-42f6-91b1-df971d3c10c8", "The Correct Day Of week should be selected"));
				return;
			}

			ValidateOverlapTime(Parent.DayOfWeekInfo);
		}

		public void ValidateOverlapTime(ZPropertyInfo info)
		{
			var refTransitTimeDetailsWithOverlap = RefTransitTime?.RefTransitTimeDetails
				.Where(refTransitTimeDetail =>
				{
					var parentEffectiveDate = Parent.RTD_EffectiveDate.IsEmpty ? DateTimeOffset.MinValue : Parent.RTD_EffectiveDate;
					var parentEndDate = Parent.RTD_EndDate.IsEmpty ? DateTimeOffset.MaxValue : Parent.RTD_EndDate;
					var transitTimeEffectiveDate = refTransitTimeDetail.RTD_EffectiveDate.IsEmpty ? DateTimeOffset.MinValue : refTransitTimeDetail.RTD_EffectiveDate;
					var transitTimeEndDate = refTransitTimeDetail.RTD_EndDate.IsEmpty ? DateTimeOffset.MaxValue : refTransitTimeDetail.RTD_EndDate;

					var isDistinct =
						parentEndDate <= transitTimeEffectiveDate ||
						parentEffectiveDate >= transitTimeEndDate;

					return refTransitTimeDetail.RTD_DayOfWeek == Parent.RTD_DayOfWeek && !isDistinct && Parent.PK != refTransitTimeDetail.PK;
				})
				.ToList();

			if (refTransitTimeDetailsWithOverlap?.Count > 0)
			{
				info.AddError(Res.GetString("79ab1122-2522-42f6-91b1-df971d3c10c1", "The Effective/End Dates overlap on another record for the same Day of the Week , please select different dates."));
				return;
			}
		}

		protected override void CheckRTD_EffectiveDate()
		{
			base.ValidateRTD_EffectiveDate();
			ValidateEffectiveAndEndDate(Parent.RTD_EffectiveDateInfo);
		}

		protected override void CheckRTD_EndDate()
		{
			base.ValidateRTD_EndDate();
			ValidateEffectiveAndEndDate(Parent.RTD_EndDateInfo);
		}

		protected override void CheckRTD_EffectiveDateIsValidZDateTimeOffsetRange()
		{
		}

		protected override void CheckRTD_EndDateIsValidZDateTimeOffsetRange()
		{
		}

		protected override void CheckRTD_ArrivalTime()
		{
			base.ValidateRTD_ArrivalTime();
			ValidateArrivalTimeAndTransitHour(Parent.RTD_ArrivalTimeInfo);
		}

		public void ValidateArrivalTimeAndTransitHour(ZPropertyInfo info)
		{
			if (Parent.RTD_ArrivalTime.IsValid && Parent.TransitHours > 0)
			{
				info.AddError(Res.GetString("99ab1263-2112-42f6-91b1-df971d3c10c5", "When arrival time has a value, transit hours must be zero, and conversely."));
				return;
			}
		}

		public void ValidateEffectiveAndEndDate(ZPropertyInfo info)
		{
			if (Parent.RTD_EffectiveDate.IsValid && Parent.RTD_EndDate.IsValid &&
				Parent.RTD_EndDate < Parent.RTD_EffectiveDate
				)
			{
				info.AddError(Res.GetString("22ab1263-2522-42f6-91b1-df971d3c10c8", "End Date Should be greater than Effective Date"));
				return;
			}

			ValidateOverlapTime(info);
		}

		public RefTransitTime RefTransitTime
		{
			get { return Parent.Factory.Load<RefTransitTime>(Parent.RTD_RTT_Parent); }
		}

		protected override void CheckRTD_ArrivalTimeIsValidZDateTimeRange()
		{
		}
	}
}
