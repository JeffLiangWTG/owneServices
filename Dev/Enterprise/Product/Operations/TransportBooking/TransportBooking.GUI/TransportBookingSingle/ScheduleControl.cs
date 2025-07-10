using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportBookings.GUI
{
	public partial class ScheduleControl : ZUserControl
	{
		public ScheduleControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			SetupSchedule();
		}

		void SetupSchedule()
		{
			var booking = Booking;
			if (booking != null)
			{
				var transportMode = booking.Schedule != null ? booking.Schedule.VL_TransportMode : ZString.Empty;
				switch (transportMode)
				{
					case Core.Constants.TransportModes.Air:
						SetupSailingForAir();
						break;
					case Core.Constants.TransportModes.Road:
						SetupSailingForRoad();
						break;
					case Core.Constants.TransportModes.Rail:
						SetupSailingForRail();
						break;
					default:
						SetupSailingForSea();
						break;
				}

				SetupSailingDates();
			}
		}

		public void SetupSailingForAir()
		{
			SetupScheduleControl(false, Res.GetString("TransportBooking|ScheduleControl|VoyageTextBox|Flight", "Flight"),
				Res.GetString("TransportBooking|ScheduleControl|FlightDetails", "Connecting Flight Schedule"), ZDateTimePickerFormat.Long);
		}

		public void SetupSailingForSea()
		{
			SetupScheduleControl(true, Res.GetString("TransportBooking|ScheduleControl|VoyageTextBox|Voyage", "Voyage"),
				 Res.GetString("TransportBooking|ScheduleControl|SailingDetails", "Connecting Sailing Schedule"), ZDateTimePickerFormat.Short);
		}

		public void SetupSailingForRail()
		{
			SetupScheduleControl(false, Res.GetString("TransportBooking|ScheduleControl|VoyageTextBox|Rail", "Journey"),
				Res.GetString("TransportBooking|ScheduleControl|RailDetails", "Connecting Rail Schedule"), ZDateTimePickerFormat.Long);
		}

		public void SetupSailingForRoad()
		{
			SetupScheduleControl(true, Res.GetString("TransportBooking|ScheduleControl|VoyageTextBox|Road", "Journey"),
				Res.GetString("TransportBooking|ScheduleControl|RoadDetails", "Connecting Road Schedule"), ZDateTimePickerFormat.Long);
		}

		void SetupScheduleControl(bool vesselVisible, ZString voyageCaption, ZString groupBoxCaption, ZDateTimePickerFormat dateFormat)
		{
			JJ_JV_NKVesselFindBox.Visible = vesselVisible;
			JX_VoyageTextBox.GetExtension<ILabelCaptionRenderer>().Caption = voyageCaption;
			SailingDetailsGroupBox.GetExtension<ILabelCaptionRenderer>().Caption = groupBoxCaption;
			JJ_JA_E_DEPDateEdit.DateTimeFormat = dateFormat;
			JJ_JB_E_ARVDateEdit.DateTimeFormat = dateFormat;
		}

		void SetupSailingDates()
		{
			var isPickup = Booking.IsPickupDirection;
			ExportDatesPanel.Visible = isPickup;
			ImportDatesPanel.Visible = !isPickup;
		}

		DtbBooking Booking
		{
			get { return (DtbBooking)CurrentDataItem; }
		}
	}
}
