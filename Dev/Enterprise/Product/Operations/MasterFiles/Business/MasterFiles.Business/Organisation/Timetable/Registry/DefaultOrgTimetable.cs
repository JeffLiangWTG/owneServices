using System;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class DefaultOrgTimetable : AutoDefaultOrgTimetable
	{
		public DefaultOrgTimetable() : base()
		{
		}

		public DefaultOrgTimetable(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DefaultOrgTimetable(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DefaultOrgTimetable(fallbackLevel, factory);
		}

		DefaultOrgTimetableCollection Parent
		{
			get
			{
				return parentCollection;
			}
		}
		DefaultOrgTimetableCollection parentCollection;

		internal void SetParent(DefaultOrgTimetableCollection parent)
		{
			parentCollection = parent;
		}

		[List("Lookups.Types")]
		public override ZString Type
		{
			get { return base.Type; }
			set { base.Type = value; }
		}

		[List("Lookups.Days")]
		public override ZString Day
		{
			get { return base.Day; }
			set { base.Day = value; }
		}

		ZDateTime processingTimeInHours;

		[ZDateTimeDurationValue]
		public ZDateTime ProcessingTimeInHours
		{
			get
			{
				if (processingTimeInHours == ZDateTime.Empty && ProcessingTime > 0)
				{
					processingTimeInHours = TimeSpan.FromMinutes(ProcessingTime);
				}
				return processingTimeInHours;
			}
			set
			{
				var processingTimeInHoursValue = value.ConvertToDurationBasedDate(ProcessingTimeInHoursInfo);
				SetNonPersistentPropertyValue(ProcessingTimeInHoursInfo, ref processingTimeInHours, processingTimeInHoursValue);
				if (!IsValidationSuspended)
				{
					ValidateProcessingTimeInHours(ProcessingTimeInHoursInfo);
				}

				if (!processingTimeInHoursValue.IsValid)
				{
					ProcessingTime = 0;
				}
				else
				{
					ProcessingTime = (ZInt)processingTimeInHoursValue.ToTimeSpan().TotalMinutes;
				}
			}
		}

		public ZPropertyInfo ProcessingTimeInHoursInfo => GetZPropertyInfo(nameof(ProcessingTimeInHours));

		public DefaultOrgTimetableLookups Lookups
		{
			get { return lookups ?? (lookups = new DefaultOrgTimetableLookups(this)); }
		}
		DefaultOrgTimetableLookups lookups;

		#region Validation

		public override void ValidateDay()
		{
			base.ValidateDay();
			if (!DayOfWeekCodeList.Mappping.ContainsKey(Day))
			{
				DayInfo.AddError(Res.GetString("2B3CA662-4269-4699-8F84-1A58B0705E0F", "Must select a valid day from the list."));
			}
			ValidateTimeframeOverlap(DayInfo);
			ValidateCutOffTime(DayInfo);
			ValidateProcessingTime(DayInfo);
		}

		public override void ValidateType()
		{
			base.ValidateType();
			if (Type != OrgTimetableType.Codes.Deliver && Type != OrgTimetableType.Codes.Pickup)
			{
				TypeInfo.AddError(Res.GetString("FA03CB7F-D0A7-4CCF-A4E2-7A50057347B5", "Must select a valid type from the list."));
			}
			ValidateTimeframeOverlap(TypeInfo);
			ValidateCutOffTime(TypeInfo);
			ValidateProcessingTime(TypeInfo);
		}

		public override void ValidateTo()
		{
			ToInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(ToInfo, true, false);

			if (!To.IsValid)
			{
				ToInfo.AddError(Res.GetString("C8C52883-D5C4-4497-9800-B5668F03120B", "Must enter a valid time."));
			}
			else if (From.IsValid && From.TimeOfDay >= To.TimeOfDay)
			{
				ToInfo.AddError(Res.GetString("f60f7840-1fd2-44ac-9979-8e98772ae276", "'From Time' must be prior to 'To Time'."));
			}
		}

		public override void ValidateFrom()
		{
			FromInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(FromInfo, true, false);

			if (!From.IsValid)
			{
				FromInfo.AddError(Res.GetString("C8C52883-D5C4-4497-9800-B5668F03120B", "Must enter a valid time."));
			}
			else if (To.IsValid && From.TimeOfDay >= To.TimeOfDay)
			{
				FromInfo.AddError(Res.GetString("f60f7840-1fd2-44ac-9979-8e98772ae276",
					"'From Time' must be prior to 'To Time'."));
			}
		}

		void ValidateTimeframeOverlap(ZPropertyInfo info)
		{
			if (Parent != null)
			{
				foreach (DefaultOrgTimetable timetable in Parent)
				{
					if (timetable != this && OverlapTimeframe(timetable))
					{
						info.AddError(Res.GetString("8fc9ddeb-36a9-4812-ae1e-928bd9ea5844", "This is already part of {0} time from {1} to {2}.",
							timetable.Type, timetable.From.ToShortTimeString(), timetable.To.ToShortTimeString()));
					}
				}
			}
		}

		void ValidateCutOffTime(ZPropertyInfo info)
		{
			if (Parent != null)
			{
				foreach (DefaultOrgTimetable timetable in Parent)
				{
					if (timetable != this && IsCutOffTimeExisted(timetable))
					{
						info.AddError(Res.GetString("cec22d5f-ef73-4b7d-acb9-29f8bfff6555", "Cut-Off Time had already been set, as part of {0} on {1}. you are not allowed to add one here. Please remove your input.",
							timetable.Type,
							timetable.Day,
							timetable.CutOffTime.ToShortTimeString()));
					}
				}
			}
		}

		void ValidateProcessingTime(ZPropertyInfo info)
		{
			if (Parent != null)
			{
				foreach (DefaultOrgTimetable timetable in Parent)
				{
					if (timetable != this && IsProcessingTimeExisted(timetable))
					{
						info.AddError(Res.GetString("cec22d5f-ef73-4b7d-acb9-29f8bfff6588", "Processing Time(Hours) had already been set, as part of {0} on {1}. you are not allowed to add one here. Please remove your input.",
							timetable.Type,
							timetable.Day,
							timetable.ProcessingTimeInHours.ToShortTimeString()));
					}
				}
			}
		}

		void ValidateProcessingTimeInHours(ZPropertyInfo info)
		{
			info.ClearAllNotifications();
			ValidateProcessingTime(info);
			TypeValidation.CheckValidZDateTimeAndRange(info, Res.GetString("364efbe4-7a25-4b03-8c19-4460b4c227c0", "processing time"));
		}

		public override void ValidateCutOffTime()
		{
			base.ValidateCutOffTime();
			ValidateCutOffTime(CutOffTimeInfo);
		}

#if DEBUG
		public
#endif
		bool OverlapTimeframe(DefaultOrgTimetable item)
		{
			if (ShouldCheckOverlap(item))
			{
				if (From.IsValid && To.IsValid)
				{
					if (item.To.IsValid && item.From.IsValid && From.TimeOfDay < item.To.TimeOfDay && item.From.TimeOfDay < To.TimeOfDay)
					{
						return true;
					}
				}
			}
			return false;
		}

		bool ShouldCheckOverlap(DefaultOrgTimetable item)
		{
			if (Day == item.Day)
			{
				if ((Type == OrgTimetableType.Codes.Pickup && item.Type == OrgTimetableType.Codes.Pickup)
				|| (Type == OrgTimetableType.Codes.Deliver && item.Type == OrgTimetableType.Codes.Deliver))
				{
					return true;
				}
			}
			return false;
		}

#if DEBUG
		public
#endif
		bool IsCutOffTimeExisted(DefaultOrgTimetable item)
		{
			if (ShouldCheckOverlap(item))
			{
				if (item.CutOffTime.IsValid && CutOffTime.IsEmpty || item.CutOffTime.IsEmpty)
				{
					return false;
				}
				return true;
			}
			return false;
		}

#if DEBUG
		public
#endif
		bool IsProcessingTimeExisted(DefaultOrgTimetable item)
		{
			if (ShouldCheckOverlap(item))
			{
				if (item.ProcessingTimeInHours.IsValid && ProcessingTimeInHours.IsEmpty || item.ProcessingTimeInHours.IsEmpty)
				{
					return false;
				}
				return true;
			}
			return false;
		}

		#endregion
	}
}
