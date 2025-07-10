using System;
using System.Collections.ObjectModel;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Rating.GUI.RateSelector.Models
{
	[CodeAlive("Used in xaml designer")]
	public class BookingInfoViewModelSample : BookingInfoViewModel
	{
		#region SuppressResourceStringsCheckRegion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1122:DoNotUseDateTimeParse", Justification = "WPF Design Time Data")]
		public BookingInfoViewModelSample()
		{
			var baseDate = new DateTime(2020, 2, 2, 20, 20, 0);

			BookingTerms = new ObservableCollection<BookingTermViewModel>()
			{
				new BookingTermViewModel()
				{
					Name = "Amendment Fee",
					Fee = "50.0",
					Currency = "USD",
				},
				new BookingTermViewModel()
				{
					Name = "Cancellation Fee",
					Fee = "75.0",
					Currency = "USD",
				},
				new BookingTermViewModel()
				{
					Name = "No Show Fee",
					Fee = "125.0",
					Currency = "USD",
				},
				new BookingTermViewModel()
				{
					Name = "Compensation Fee",
					Fee = "-75.0",
					Currency = "USD",
				},
			};

			Penalties = new ObservableCollection<PenaltyViewModel>()
			{
				new PenaltyViewModel()
				{
					Direction = "Import",
					Type = "Demurrage",
					Name = "Import Demurrage",
					FreeTime = 5,
					PerUnitRate = 75 ,
					Currency = "EUR"
				},

				new PenaltyViewModel()
				{
					Direction = "Import",
					Type = "Demurrage",
					Name = "Import Demurrage",
					FreeTime = 10,
					PerUnitRate = 120 ,
					Currency = "EUR"
				},

				new PenaltyViewModel()
				{
					Direction = "Import",
					Type = "Demurrage",
					Name = "Import Demurrage",
					FreeTime = 17,
					PerUnitRate = 190 ,
					Currency = "EUR"
				},

				new PenaltyViewModel()
				{
					Direction = "Import",
					Type = "Detention",
					Name = "Import Detention",
					FreeTime = 8,
					PerUnitRate = 100 ,
					Currency = "EUR"
				},
			};

			TransportLegs = new ObservableCollection<TransportLegViewModel>()
			{
				new TransportLegViewModel()
				{
					LegOrder = 1,
					TransportMode = TransportMode.Sea,
					VesselType = "74F",
					VesselName = "EVELYN MAERSK",
					VoyageNumber = "SQ008",
					PortOfLoadingCode = "SIN",
					PortOfLoadingName = "Singapore",
					PortOfDischargeCode = "LHR",
					PortOfDischargeName = "London Heathrow xxxx",
					EstimatedDeparture = baseDate.AddHours(12),
					EstimatedArrival = baseDate.AddHours(18).AddMinutes(15),
					TransitTime = new TimeSpan(32, 16, 0, 0),
					TradeLane = "FEA/EUR",
					FlagCode = "DK",
					ShowDateInfos = true,
					DateInfos = new ObservableCollection<TransportLegDateInfoViewModel>()
					{
						new TransportLegDateInfoViewModel()
						{
							Code = "CY",
							Name = "Commercial Cargo Cutoff",
							Type = "Documentation",
							Date = DateTime.Parse("2020-12-11 00:00:00"), // WPF Design Time Data
						},
						new TransportLegDateInfoViewModel()
						{
							Code = "SINONAMS",
							Name = "Shipping Instructions Deadline",
							Type = "Documentation",
							Date = DateTime.Parse("2020-12-11 00:00:00"), // WPF Design Time Data
						},
						new TransportLegDateInfoViewModel()
						{
							Code = "LCD",
							Name = "Loadlist Closure Deadline",
							Type = "Marine Services",
							Date = DateTime.Parse("2020-12-10 09:00:00"), // WPF Design Time Data
						},
						new TransportLegDateInfoViewModel()
						{
							Code = "CSPD",
							Name = "Coprar to Stowage Planners Deadline",
							Type = "Marine Services",
							Date = DateTime.Parse("2020-12-10 07:00:00"), // WPF Design Time Data
						}
					}
				},
				new TransportLegViewModel()
				{
					LegOrder = 2,
					TransportMode = TransportMode.Air,
					VesselType = "388",
					VesselName = "Airbus A380-800",
					VoyageNumber = "QF123",
					PortOfLoadingCode = "SYD",
					PortOfDischargeCode = "SIN",
					PortOfDischargeName = "Signapore",
					EstimatedDeparture = baseDate,
					EstimatedArrival = baseDate.AddHours(8).AddMinutes(30),
					ShowDateInfos = true,
					DateInfos = new ObservableCollection<TransportLegDateInfoViewModel>()
					{
						new TransportLegDateInfoViewModel()
						{
							Code = "CY",
							Name = "Commercial Cargo Cutoff",
							Type = "Documentation",
							Date = DateTime.Parse("2020-12-11 00:00:00"), // WPF Design Time Data
						}
					}
				},
				new TransportLegViewModel()
				{
					LegOrder = 3,
					TransportMode = TransportMode.Air,
					VesselType = "74F",
					VesselName = "Boeing 747-400 Freighter",
					VoyageNumber = "AA001",
					PortOfLoadingCode = "LHR",
					PortOfDischargeCode = "JFK",
					PortOfDischargeName = "New York",
					EstimatedDeparture = baseDate.AddHours(12),
					EstimatedArrival = baseDate.AddHours(18).AddMinutes(15),
				}
			};

			Origin = "USLAX";
			Destination = "CNSGH";
			VesselName = "Vessel Name";
			VoyageNumber = "VN 123456";

			ArrivalTime = new DateTime(2020, 04, 25, 09, 0, 0);
			DepartureTime = ArrivalTime.AddDays(-35).AddHours(-16);
		}

		#endregion
	}
}
