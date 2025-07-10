using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class RefTransitTimeDetail : AutoRefTransitTimeDetail
	{
		public new class Schema : AutoRefTransitTimeDetail.Schema
		{
			public const string TransitDays = "TransitDays";
			public const string TransitHours = "TransitHours";
		}

		public RefTransitTimeDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			transitDays = RTD_TransitHours / 24;
			transitHours = RTD_TransitHours % 24;
		}

		public ZInt TransitDays
		{
			get { return transitDays; }
			set
			{
				if (transitDays != value)
				{
					SetNonPersistentPropertyValue(TransitDaysInfo, ref transitDays, value);
					CalculateTotalHours();

					if (!IsValidationSuspended)
					{
						Validation.ValidateTransitDays();
						Validation.ValidateTransitHours();
					}
				}
			}
		}

		ZInt transitDays;

		public ZPropertyInfo TransitDaysInfo
		{
			get { return GetZPropertyInfo(Schema.TransitDays); }
		}

		public ZInt TransitHours
		{
			get { return transitHours; }
			set
			{
				if (transitHours != value)
				{
					SetNonPersistentPropertyValue(TransitHoursInfo, ref transitHours, value);
					CalculateTotalHours();

					if (!IsValidationSuspended)
					{
						Validation.ValidateTransitHours();
						Validation.ValidateTransitDays();
					}
				}
			}
		}

		ZInt transitHours;

		public ZPropertyInfo TransitHoursInfo
		{
			get { return GetZPropertyInfo(Schema.TransitHours); }
		}

		void CalculateTotalHours()
		{
			RTD_TransitHours = (TransitDays * 24) + TransitHours;
		}

		public override ZDateTime RTD_ArrivalTime
		{
			get
			{
				return base.RTD_ArrivalTime;
			}
			set
			{
				var arrivalTime = value;
				if (value.IsValid)
				{
					arrivalTime = new ZDateTime(1900, 1, 1, value.Hour, value.Minute, 0);
				}
				base.RTD_ArrivalTime = arrivalTime;
			}
		}

		ZString dayOfWeek;

		[List("Lookups.Days")]
		public ZString DayOfWeek
		{
			get
			{
				switch (RTD_DayOfWeek)
				{
					case 1:
						return AutoDayOfWeekCodeList.Codes.Monday;
					case 2:
						return AutoDayOfWeekCodeList.Codes.Tuesday;
					case 3:
						return AutoDayOfWeekCodeList.Codes.Wednesday;
					case 4:
						return AutoDayOfWeekCodeList.Codes.Thursday;
					case 5:
						return AutoDayOfWeekCodeList.Codes.Friday;
					case 6:
						return AutoDayOfWeekCodeList.Codes.Saturday;
					case 7:
						return AutoDayOfWeekCodeList.Codes.Sunday;
					default:
						return dayOfWeek;
				}
			}
			set
			{
				switch (value)
				{
					case AutoDayOfWeekCodeList.Codes.Monday:
						RTD_DayOfWeek = 1;
						break;
					case AutoDayOfWeekCodeList.Codes.Tuesday:
						RTD_DayOfWeek = 2;
						break;
					case AutoDayOfWeekCodeList.Codes.Wednesday:
						RTD_DayOfWeek = 3;
						break;
					case AutoDayOfWeekCodeList.Codes.Thursday:
						RTD_DayOfWeek = 4;
						break;
					case AutoDayOfWeekCodeList.Codes.Friday:
						RTD_DayOfWeek = 5;
						break;
					case AutoDayOfWeekCodeList.Codes.Saturday:
						RTD_DayOfWeek = 6;
						break;
					case AutoDayOfWeekCodeList.Codes.Sunday:
						RTD_DayOfWeek = 7;
						break;
					default:
						RTD_DayOfWeek = 0;
						break;
				}
				if (dayOfWeek != value)
				{
					SetNonPersistentPropertyValue(DayOfWeekInfo, ref dayOfWeek, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateDayOfWeek();
					}
				}
			}
		}

		public ZPropertyInfo DayOfWeekInfo
		{
			get { return GetZPropertyInfo(nameof(DayOfWeek)); }
		}
	}
}
