using System;
using System.Collections.ObjectModel;
using CargoWise.Common;
using Enterprise.Freight.Integration;
using Enterprise.Rating.GUI.RateSelection;

namespace Enterprise.Rating.GUI.RateSelector.Models
{
	public enum TransportMode
	{
		Air,
		Sea,
		Truck
	}

	public sealed class TransportLegViewModel : ViewModelWithNotificationBase
	{
		public static TransportLegViewModel New(IBookingTransportLeg transportLeg)
		{
			Argument.NotNull(transportLeg, nameof(transportLeg));

			return new TransportLegViewModel
			{
				TransportMode = CreateTransportMode(transportLeg.TransportMode),
				LegOrder = transportLeg.LegOrder,
				VoyageNumber = transportLeg.VoyageNumber,
				VesselType = transportLeg.VesselType,
				VesselName = transportLeg.VesselName,
				PortOfLoadingCode = transportLeg.PortOfLoadingCode,
				PortOfDischargeCode = transportLeg.PortOfDischargeCode,
				EstimatedDeparture = transportLeg.EstimatedDeparture,
				EstimatedArrival = transportLeg.EstimatedArrival
			};
		}

		static TransportMode CreateTransportMode(string transportMode)
		{
			switch (transportMode)
			{
				case Core.Constants.TransportModes.Sea:
					return TransportMode.Sea;

				case Core.Constants.TransportModes.Road:
					return TransportMode.Truck;

				default:
					return TransportMode.Air;
			}
		}

		public int LegOrder { get; set; }

		public string VoyageNumber { get; set; }
		public DateTime EstimatedDeparture { get; set; }
		public DateTime EstimatedArrival { get; set; }

		public string PortOfLoadingCode { get; set; }
		public string PortOfLoadingName { get; set; }

		public string PortOfDischargeCode { get; set; }
		public string PortOfDischargeName { get; set; }

		public string VesselType { get; set; }
		public string VesselName { get; set; }

		public TimeSpan? TransitTime { get; set; }
		public TransportMode TransportMode { get; set; }

		public string TradeLane { get; set; }
		public string FlagCode { get; set; }

		public bool ShowDateInfos
		{
			get
			{
				return showDateInfos;
			}

			set
			{
				showDateInfos = value;
				OnPropertyChanged(nameof(ShowDateInfos));
			}
		}
		bool showDateInfos;

		public ObservableCollection<TransportLegDateInfoViewModel> DateInfos { get; set; }
	}
}
