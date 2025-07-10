using System.Data;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class RefTransitTime : AutoRefTransitTime
	{
		public new class Schema : AutoRefTransitTime.Schema
		{
			public const string TransitDays = "TransitDays";
			public const string TransitHours = "TransitHours";
			public const string TransitTimeFormatted = "TransitTimeFormatted";
		}

		public RefTransitTime(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			transitDays = RTT_TransitHours / 24;
			transitHours = RTT_TransitHours % 24;
		}

		public ZoneSelection OriginZone
		{
			get
			{
				if (originZone == null)
				{
					originZone = new ZoneSelection(this, Schema.RTT_TZ_OriginDomesticZone, Schema.RTT_FZ_OriginInternationalZone);
					RegisterEditableChildObject(originZone);
				}

				return originZone;
			}
		}

		ZoneSelection originZone;

		public ZoneSelection DestinationZone
		{
			get
			{
				if (destinationZone == null)
				{
					destinationZone = new ZoneSelection(this, Schema.RTT_TZ_DestinationDomesticZone, Schema.RTT_FZ_DestinationInternationalZone);
					RegisterEditableChildObject(destinationZone);
				}

				return destinationZone;
			}
		}

		ZoneSelection destinationZone;

		protected RefTransitTimeDetailCollection fRefTransitTimeDetails;

		[ChildEditable()]
		public RefTransitTimeDetailCollection RefTransitTimeDetails
		{
			get
			{
				if (fRefTransitTimeDetails == null)
				{
					fRefTransitTimeDetails = new RefTransitTimeDetailCollection(this);
					fRefTransitTimeDetails.Load();
					RegisterEditableChildObject(fRefTransitTimeDetails);
				}

				return fRefTransitTimeDetails;
			}
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
			RTT_TransitHours = (TransitDays * 24) + TransitHours;
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var originZoneName = OriginZone.SelectedZone != null ? OriginZone.SelectedZone.ZoneCode : ZString.Empty;
				var destinationZoneName = DestinationZone.SelectedZone != null ? DestinationZone.SelectedZone.ZoneCode : ZString.Empty;
				return Res.GetString("c38efafc-e31f-423b-8129-e88f4ef88b17", "{0} Zone Transit: {1} - {2}", RTT_RS_NKServiceLevel, originZoneName, destinationZoneName);
			}
		}

		public ZString TransitTimeFormatted
		{
			get
			{
				var days = TransitDays == 1
					? Res.GetString("2d0c2e4f-0c6a-4c30-88f3-0b3004de8d96", "1 day")
					: Res.GetString("c2463817-0411-47eb-a007-369290283753", "{0} days", TransitDays);
				var hours = TransitHours == 1
					? Res.GetString("7fb82151-c734-4417-a6db-b2f1c6522987", "1 hour")
					: Res.GetString("60291a17-273f-421c-9523-a1dc561dcaac", "{0} hours", TransitHours);

				return string.Format(CultureInfo.InvariantCulture, "{0} {1}", days, hours);
			}
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		[List("Lookups.Modes")]
		public override ZString RTT_Mode
		{
			get => base.RTT_Mode;
			set
			{
				if (base.RTT_Mode != value)
				{
					base.RTT_Mode = value;
				}
			}
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			RTT_FZ_OriginInternationalZone = Factory.NewWithValidTestData<RefZoneHeader>().PK;
			RTT_FZ_DestinationInternationalZone = Factory.NewWithValidTestData<RefZoneHeader>().PK;
		}

#endif
	}
}
