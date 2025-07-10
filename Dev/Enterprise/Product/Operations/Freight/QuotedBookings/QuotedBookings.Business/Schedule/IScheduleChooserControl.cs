using System.Diagnostics.CodeAnalysis;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Freight.QuotedBookings
{
	public interface IScheduleChooserControl
	{
		[SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly", Justification = "Legit readonly property")]
		bool ShowDirect { set; }

		[SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly", Justification = "Legit readonly property")]
		bool ShowIsNeutral { set; }

		[SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly", Justification = "Legit readonly property")]
		bool ShowServiceLevelFields { set; }

		[SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly", Justification = "Legit readonly property")]
		bool ShowMAWBFields { set; }

		[SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly", Justification = "Legit readonly property")]
		bool ShowMAWBSeaNumberTextBox { set; }

		[SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly", Justification = "Legit readonly property")]
		bool ShowHBLNumberTextBox { set; }

		[SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly", Justification = "Legit readonly property")]
		bool ShowCarrierDetails { set; }

		[SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly", Justification = "Legit readonly property")]
		bool ShowCreditorDetails { set; }

		[SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly", Justification = "Legit readonly property")]
		bool ShowScheduleTotalWeightAndVolume { set; }

		[SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly", Justification = "Legit readonly property")]
		bool ShowVesselBoundTextBox { set; }

		[SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly", Justification = "Legit readonly property")]
		bool ShowFCLCutOffReaoOnlyDateEdit { set; }

		[SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly", Justification = "Legit readonly property")]
		bool ShowImportOnlineSchedulesButton { set; }

		ZString TransportContainerMode { get; }
	}

	public static class ScheduleChooserControlExtensions
	{
		public static void SetControlVisibility(this IScheduleChooserControl control, QuotedBooking booking)
		{
			if (booking == null || booking.Booking == null)
			{
				return;
			}

			control.ShowDirect = ((ISailingChooserParent)booking).IsDirectEnabled;
			bool isDirect = ((ISailingChooserParent)booking).IsDirectEnabled && booking.Booking.JS_IsDirectBooking;

			control.ShowIsNeutral = booking.Booking.IsAir && isDirect;
			control.ShowServiceLevelFields = isDirect;
			control.ShowMAWBFields = booking.Booking.IsAir && isDirect;
			control.ShowMAWBSeaNumberTextBox = booking.Booking.IsSea && isDirect;
			control.ShowHBLNumberTextBox = !(booking.Booking.IsAir || booking.Booking.IsSea) || !isDirect;

			control.ShowCarrierDetails = false;
			control.ShowCreditorDetails = false;
			control.ShowScheduleTotalWeightAndVolume = false;

			var packingMode = RatingConstants.GetContainerModeFromMode(control.TransportContainerMode);
			switch (packingMode)
			{
				case Constants.ContainerModes.FCL:
				case Constants.ContainerModes.LCL:
					control.ShowCarrierDetails = true;
					control.ShowCreditorDetails = true;
					break;

				default:
					switch (control.TransportContainerMode)
					{
						case Constants.ContainerModes.RollOnRollOff:
						case Constants.ContainerModes.Liquid:
						case Constants.ContainerModes.Bulk:
						case Constants.ContainerModes.BreakBulk:
							break;

						default:
							control.ShowScheduleTotalWeightAndVolume = true;
							break;
					}
					break;
			}

			control.ShowVesselBoundTextBox = false;
			control.ShowFCLCutOffReaoOnlyDateEdit = false;
			control.ShowImportOnlineSchedulesButton = false;

			if (booking.Booking.IsSea)
			{
				control.ShowVesselBoundTextBox = true;
				control.ShowFCLCutOffReaoOnlyDateEdit = true;
				control.ShowImportOnlineSchedulesButton = true;
			}
			else if (booking.Booking.IsRail)
			{
				control.ShowVesselBoundTextBox = true;
				control.ShowFCLCutOffReaoOnlyDateEdit = true;
			}
			else if (booking.Booking.IsRoad)
			{
				control.ShowFCLCutOffReaoOnlyDateEdit = true;
			}
		}
	}
}
