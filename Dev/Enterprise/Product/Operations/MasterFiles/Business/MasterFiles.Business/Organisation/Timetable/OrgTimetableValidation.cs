using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgTimetableValidation : AutoOrgTimetableValidation
	{
		public OrgTimetableValidation(AutoOrgTimetable parent)
			: base(parent)
		{
		}

		public new OrgTimetable Parent
		{
			get { return base.Parent as OrgTimetable; }
		}

		protected override void CheckOTT_Type()
		{
			MandatoryValidation.CheckEntered(Parent.OTT_TypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.OTT_TypeInfo);
			ValidateTimeframeOverlap(Parent.OTT_TypeInfo);
			ValidateCutOffTime(Parent.OTT_TypeInfo);
			ValidateProcessingTime(Parent.OTT_TypeInfo);
		}

		protected override void CheckOTT_TimeFrom()
		{
			if (Parent.Address == null || !Parent.Address.Timetables.NotApplicable)
			{
				MandatoryValidation.CheckEntered(Parent.OTT_TimeFromInfo);
				CheckTimeRange(Parent.OTT_TimeFromInfo);
			}
		}

		protected override void CheckOTT_TimeTo()
		{
			if (Parent.Address == null || !Parent.Address.Timetables.NotApplicable)
			{
				MandatoryValidation.CheckEntered(Parent.OTT_TimeToInfo);
				CheckTimeRange(Parent.OTT_TimeToInfo);
			}
		}

		protected override void CheckOTT_TimeToIsValidZDateTimeRange()
		{
			TypeValidation.CheckValidZDateTimeRange(Parent.OTT_TimeToInfo, true, false);
		}

		protected override void CheckOTT_TimeFromIsValidZDateTimeRange()
		{
			TypeValidation.CheckValidZDateTimeRange(Parent.OTT_TimeFromInfo, true, false);
		}

		protected override void CheckOTT_CutOffTimeIsValidZDateTimeRange()
		{
			TypeValidation.CheckValidZDateTimeRange(Parent.OTT_CutOffTimeInfo, true, false);
			ValidateCutOffTime(Parent.OTT_CutOffTimeInfo);
		}

		protected override void CheckOTT_IsForAllWeekDays()
		{
			base.CheckOTT_IsForAllWeekDays();
			var areWeekendsDefined = Parent.AreAnyWeekendsDefinedForTheAddressCorrespondingToThisTimetable;
			var isForAllWeekdays = Parent.OTT_IsForAllWeekDays;
			if (isForAllWeekdays && !areWeekendsDefined)
			{
				Parent.OTT_IsForAllWeekDaysInfo.AddError(Res.GetString("581c0cb9-8c4e-48f9-9e76-ca3d4c09cddd", "To use a 'Weekday' option the weekends must be configured for Organization’s country within Maintain > Locations > Countries/Regions > Holidays module."));
			}
		}

		protected override void CheckOTT_ProcessingTimeInMinutes()
		{
			base.CheckOTT_ProcessingTimeInMinutes();
			if (Parent.OTT_ProcessingTimeInMinutes < 0)
			{
				Parent.OTT_ProcessingTimeInMinutesInfo.AddError(Res.GetString("6cb20774-57d7-4f97-a5b3-68323074cd51", "Processing time cannot be a negative number, only zero value is allowed."));
			}
		}

		void CheckTimeRange(ZPropertyInfo info)
		{
			if (Parent.OTT_TimeFrom.IsValid && Parent.OTT_TimeTo.IsValid && Parent.OTT_TimeFrom.TimeOfDay >= Parent.OTT_TimeTo.TimeOfDay)
			{
				info.AddError(Res.GetString("f60f7840-1fd2-44ac-9979-8e98772ae276", "'From Time' must be prior to 'To Time'."));
			}
			if (!Parent.HasRowErrors)
			{
				ValidateTimeframeOverlap(info);
			}
		}

		void ValidateTimeframeOverlap(ZPropertyInfo info)
		{
			if (Parent.Address != null)
			{
				var timetables = Parent.Address.Timetables;
				foreach (var timetable in timetables)
				{
					if (timetable.PK != Parent.PK && timetable.OverlapTimeframe(Parent))
					{
						info.AddError(Res.GetString("8fc9ddeb-36a9-4812-ae1e-928bd9ea583b", "This is already part of {0} time from {1} to {2}.",
							timetable.TypeDescription, timetable.OTT_TimeFrom.ToShortTimeString(), timetable.OTT_TimeTo.ToShortTimeString()));
					}
				}
			}
		}

		void ValidateCutOffTime(ZPropertyInfo info)
		{
			if (Parent.Address != null)
			{
				var timetables = Parent.Address.Timetables;
				foreach (var timetable in timetables)
				{
					if (timetable.PK != Parent.PK && timetable.IsCutOffTimeExisted(Parent))
					{
						info.AddError(Res.GetString("cec22d5f-ef73-4b7d-acb9-29f8bfff654c", "Cut-Off Time had already been set, as part of {0} on {1}, you are not allowed to add one here. Please remove your input.",
							timetable.TypeDescription,
							timetable.OTT_IsForAllWeekDays ? Res.GetString("425b953b-7630-48f7-8644-4e60f6bf3bab", "Weekday") : timetable.DayOfWeek,
							timetable.OTT_CutOffTime.ToShortTimeString()));
					}
				}
			}
		}

		void ValidateProcessingTime(ZPropertyInfo info)
		{
			if (Parent.Address != null)
			{
				var timetables = Parent.Address.Timetables;
				foreach (var timetable in timetables)
				{
					if (timetable.PK != Parent.PK && timetable.IsProcessingTimeExisted(Parent))
					{
						info.AddError(Res.GetString("ee3a4489-5917-4144-91c8-d160329debcd", "Processing Time(Hours) had already been set, as part of {0} on {1}, you are not allowed to add one here. Please remove your input.",
							timetable.TypeDescription,
							timetable.OTT_IsForAllWeekDays ? Res.GetString("42A4FC5C-27F2-413B-BD18-B59B3950D884", "Weekday") : timetable.DayOfWeek,
							timetable.ProcessingTimeInHours.ToShortTimeString()));
					}
				}
			}
		}

		public void ValidateProcessingTimeInHours()
		{
			ValidateCalculatedProperty(Parent.ProcessingTimeInHoursInfo);
		}

		protected void CheckProcessingTimeInHours()
		{
			TypeValidation.CheckValidZDateTimeAndRange(Parent.ProcessingTimeInHoursInfo, Res.GetString("bf7791a4-b713-439f-92e4-b12bd6add4cd", "processing time"));
			ValidateProcessingTime(Parent.ProcessingTimeInHoursInfo);
		}

		public override void ValidateAll()
		{
			if (Parent.Address == null || !Parent.Address.Timetables.NotApplicable)
			{
				base.ValidateAll();
				ValidateOTT_IsForAllWeekDays();
				ValidateDayOfWeek();
				ValidateProcessingTimeInHours();
			}
		}

		public void ValidateDayOfWeek()
		{
			ValidateCalculatedProperty(Parent.DayOfWeekInfo);
		}

		protected void CheckDayOfWeek()
		{
			if (Parent.Address != null && Parent.Address.Timetables.AdvancedRange)
			{
				MandatoryValidation.CheckEntered(Parent.DayOfWeekInfo);
				ListValidation.ErrorIfInvalidCode(Parent.DayOfWeekInfo);
				ValidateTimeframeOverlap(Parent.DayOfWeekInfo);
				ValidateCutOffTime(Parent.DayOfWeekInfo);
				ValidateProcessingTime(Parent.DayOfWeekInfo);
			}
		}

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
		{
			return false;
		}
	}
}
