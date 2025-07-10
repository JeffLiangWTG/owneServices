using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgTimetable : AutoOrgTimetable
	{
		public OrgTimetable(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public bool AreAnyWeekendsDefinedForTheAddressCorrespondingToThisTimetable
		{
			get
			{
				if (RefCountry == null)
				{
					return false;
				}

				return RefCountry.AreAnyWeekendsDefined;
			}
		}

		RefCountry refCountry;

		RefCountry RefCountry
		{
			get
			{
				if (refCountry == null && Address != null)
				{
					refCountry = new RefCountry.Loader(Factory).LoadForCountry(Address.OA_RN_NKCountryCode);
				}

				return refCountry;
			}
		}

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool shouldBeReadOnly = false;

			if (Address != null && Address.Header != null)
			{
				shouldBeReadOnly = !Address.Header.SecurityProvider.HasModifyAddressAdditionalDetailsSecurity;
			}
			return shouldBeReadOnly || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		[List("Lookups.Types")]
		public override ZString OTT_Type
		{
			get { return base.OTT_Type; }
			set
			{
				base.OTT_Type = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateDayOfWeek();
				}
			}
		}

		public ZString TypeDescription
		{
			get { return Lookups.Types.GetDescriptionFromCode(OTT_Type); }
		}

		public virtual ZPropertyInfo TypeDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(TypeDescription)); }
		}

		public override ZDateTime OTT_TimeFrom
		{
			get
			{
				return base.OTT_TimeFrom;
			}
			set
			{
				ZDateTime timeFrom = value;
				if (value.IsValid)
				{
					timeFrom = new ZDateTime(1900, 1, 1, value.Hour, value.Minute, value.Second);
				}
				base.OTT_TimeFrom = timeFrom;
			}
		}

		public override ZDateTime OTT_TimeTo
		{
			get
			{
				return base.OTT_TimeTo;
			}
			set
			{
				ZDateTime timeTo = value;
				if (value.IsValid)
				{
					timeTo = new ZDateTime(1900, 1, 1, value.Hour, value.Minute, value.Second);
				}
				base.OTT_TimeTo = timeTo;
			}
		}

		public override ZDateTime OTT_CutOffTime
		{
			get
			{
				return base.OTT_CutOffTime;
			}
			set
			{
				ZDateTime cutOffTime = value;
				if (value.IsValid)
				{
					cutOffTime = new ZDateTime(1900, 1, 1, value.Hour, value.Minute, value.Second);
				}
				base.OTT_CutOffTime = cutOffTime;
			}
		}

		public ZString DayOfWeekLocalized
		{
			get
			{
				string dayofWeekLocalized;
				switch (DayOfWeek)
				{
					case AutoDayOfWeekCodeList.Codes.Monday:
						dayofWeekLocalized = DayOfWeekCodeList.LocalizedCodes.Monday;
						break;
					case AutoDayOfWeekCodeList.Codes.Tuesday:
						dayofWeekLocalized = DayOfWeekCodeList.LocalizedCodes.Tuesday;
						break;
					case AutoDayOfWeekCodeList.Codes.Wednesday:
						dayofWeekLocalized = DayOfWeekCodeList.LocalizedCodes.Wednesday;
						break;
					case AutoDayOfWeekCodeList.Codes.Thursday:
						dayofWeekLocalized = DayOfWeekCodeList.LocalizedCodes.Thursday;
						break;
					case AutoDayOfWeekCodeList.Codes.Friday:
						dayofWeekLocalized = DayOfWeekCodeList.LocalizedCodes.Friday;
						break;
					case AutoDayOfWeekCodeList.Codes.Saturday:
						dayofWeekLocalized = DayOfWeekCodeList.LocalizedCodes.Saturday;
						break;
					case AutoDayOfWeekCodeList.Codes.Sunday:
						dayofWeekLocalized = DayOfWeekCodeList.LocalizedCodes.Sunday;
						break;
					default:
						dayofWeekLocalized = string.Empty;
						break;
				}

				return dayofWeekLocalized;
			}
		}

		ZString dayOfWeek;

		[List("Lookups.Days")]
		public ZString DayOfWeek
		{
			get
			{
				if (OTT_IsForAllWeekDays)
				{
					return string.Empty;
				}
				if (OTT_Monday && OTT_Tuesday && OTT_Wednesday && OTT_Thursday && OTT_Friday)
				{
					return string.Empty;
				}
				else if (OTT_Monday)
				{
					return AutoDayOfWeekCodeList.Codes.Monday;
				}
				else if (OTT_Tuesday)
				{
					return AutoDayOfWeekCodeList.Codes.Tuesday;
				}
				else if (OTT_Wednesday)
				{
					return AutoDayOfWeekCodeList.Codes.Wednesday;
				}
				else if (OTT_Thursday)
				{
					return AutoDayOfWeekCodeList.Codes.Thursday;
				}
				else if (OTT_Friday)
				{
					return AutoDayOfWeekCodeList.Codes.Friday;
				}
				else if (OTT_Saturday)
				{
					return AutoDayOfWeekCodeList.Codes.Saturday;
				}
				else if (OTT_Sunday)
				{
					return AutoDayOfWeekCodeList.Codes.Sunday;
				}
				else
				{
					return dayOfWeek;
				}
			}
			set
			{
				OTT_Monday = false;
				OTT_Tuesday = false;
				OTT_Wednesday = false;
				OTT_Thursday = false;
				OTT_Friday = false;
				OTT_Saturday = false;
				OTT_Sunday = false;
				switch (value)
				{
					case AutoDayOfWeekCodeList.Codes.Monday:
						OTT_Monday = true;
						break;
					case AutoDayOfWeekCodeList.Codes.Tuesday:
						OTT_Tuesday = true;
						break;
					case AutoDayOfWeekCodeList.Codes.Wednesday:
						OTT_Wednesday = true;
						break;
					case AutoDayOfWeekCodeList.Codes.Thursday:
						OTT_Thursday = true;
						break;
					case AutoDayOfWeekCodeList.Codes.Friday:
						OTT_Friday = true;
						break;
					case AutoDayOfWeekCodeList.Codes.Saturday:
						OTT_Saturday = true;
						break;
					case AutoDayOfWeekCodeList.Codes.Sunday:
						OTT_Sunday = true;
						break;
					default:
						break;
				}
				SetNonPersistentPropertyValue(DayOfWeekInfo, ref dayOfWeek, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateDayOfWeek();
					Validation.ValidateOTT_Type();
				}
			}
		}

		public ZPropertyInfo DayOfWeekInfo
		{
			get { return GetZPropertyInfo(nameof(DayOfWeek)); }
		}

		public bool OverlapTimeframe(OrgTimetable item)
		{
			if (ShouldCheckOverlap(item))
			{
				if (OTT_TimeFrom.IsValid && OTT_TimeTo.IsValid)
				{
					if (item.OTT_TimeFrom.IsValid && item.OTT_TimeTo.IsValid && item.OTT_TimeFrom.TimeOfDay < OTT_TimeTo.TimeOfDay && item.OTT_TimeTo.TimeOfDay > OTT_TimeFrom.TimeOfDay)
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool IsCutOffTimeExisted(OrgTimetable item)
		{
			if (ShouldCheckOverlap(item))
			{
				if (item.OTT_CutOffTime.IsValid && OTT_CutOffTime.IsEmpty
					|| item.OTT_CutOffTime.IsEmpty)
				{
					return false;
				}
				return true;
			}
			return false;
		}

		public bool IsProcessingTimeExisted(OrgTimetable item)
		{
			if (ShouldCheckOverlap(item))
			{
				if (item.ProcessingTimeInHours.IsValid && ProcessingTimeInHours.IsEmpty
					|| item.ProcessingTimeInHours.IsEmpty)
				{
					return false;
				}
				return true;
			}
			return false;
		}

		bool ShouldCheckOverlap(OrgTimetable item)
		{
			if (DayOfWeek == item.DayOfWeek)
			{
				if ((OTT_Type == OrgTimetableType.Codes.Pickup && item.OTT_Type == OrgTimetableType.Codes.Pickup)
				|| (OTT_Type == OrgTimetableType.Codes.Deliver && item.OTT_Type == OrgTimetableType.Codes.Deliver))
				{
					return true;
				}
			}
			return false;
		}

		ZDateTime processingTimeInHours;

		[ZDateTimeDurationValue]
		public ZDateTime ProcessingTimeInHours
		{
			get
			{
				if (processingTimeInHours == ZDateTime.Empty && OTT_ProcessingTimeInMinutes > 0)
				{
					processingTimeInHours = TimeSpan.FromMinutes(OTT_ProcessingTimeInMinutes);
				}
				return processingTimeInHours;
			}
			set
			{
				var processingTimeInHoursValue = value.ConvertToDurationBasedDate(ProcessingTimeInHoursInfo);
				SetNonPersistentPropertyValue(ProcessingTimeInHoursInfo, ref processingTimeInHours, processingTimeInHoursValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateProcessingTimeInHours();
				}

				if (!processingTimeInHoursValue.IsValid)
				{
					OTT_ProcessingTimeInMinutes = 0;
				}
				else
				{
					OTT_ProcessingTimeInMinutes = (ZInt)processingTimeInHoursValue.ToTimeSpan().TotalMinutes;
				}
			}
		}

		public ZPropertyInfo ProcessingTimeInHoursInfo => GetZPropertyInfo(nameof(ProcessingTimeInHours));

		internal bool IsDefaultFromRegistry { set; get; }

		public override bool IsSavedByFactory
		{
			get
			{
				return base.IsSavedByFactory && !IsDefaultFromRegistry;
			}
		}

		public bool IsAppliedToDayOfWeek(DayOfWeek dayOfWeek)
		{
			if (OTT_IsForAllWeekDays)
			{
				return IsAppliedToDayOfWeekForWeekdayTimetable(dayOfWeek);
			}

			switch (dayOfWeek)
			{
				case System.DayOfWeek.Monday:
					return OTT_Monday;
				case System.DayOfWeek.Tuesday:
					return OTT_Tuesday;
				case System.DayOfWeek.Wednesday:
					return OTT_Wednesday;
				case System.DayOfWeek.Thursday:
					return OTT_Thursday;
				case System.DayOfWeek.Friday:
					return OTT_Friday;
				case System.DayOfWeek.Saturday:
					return OTT_Saturday;
				case System.DayOfWeek.Sunday:
					return OTT_Sunday;
				default:
					return false;
			}
		}

		bool IsAppliedToDayOfWeekForWeekdayTimetable(DayOfWeek dayOfWeek)
		{
			if (RefCountry == null || !AreAnyWeekendsDefinedForTheAddressCorrespondingToThisTimetable)
			{
				return dayOfWeek != System.DayOfWeek.Saturday && dayOfWeek != System.DayOfWeek.Sunday;
			}

			return RefCountry.IsWorkingDay(dayOfWeek);
		}
	}
}
