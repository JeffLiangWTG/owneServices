#region SuppressResourceStringsCheckRegion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Rating.GUI.RateSelector.Models
{
	[CodeAlive("Used in xaml designer")]
	public class BookingEngineRateViewModelSample : BookingEngineRateViewModel
	{
		public BookingEngineRateViewModelSample()
		{
			var baseDate = new DateTime(2020, 2, 2, 20, 20, 0);
			TransportLegs = new ObservableCollection<TransportLegViewModel>()
			{
				new TransportLegViewModel()
				{
					LegOrder = 1,
					TransportMode = TransportMode.Air,
					VesselType = "388",
					VesselName = "Airbus A380-800",
					VoyageNumber = "QF123",
					PortOfLoadingCode = "SYD",
					PortOfDischargeCode = "SIN",
					EstimatedDeparture = baseDate,
					EstimatedArrival = baseDate.AddHours(8).AddMinutes(30)
				},
				new TransportLegViewModel()
				{
					LegOrder = 2,
					TransportMode = TransportMode.Sea,
					VesselType = "74F",
					VesselName = "Boeing 747-400 Freighter",
					VoyageNumber = "SQ008",
					PortOfLoadingCode = "SIN",
					PortOfLoadingName = "Singapore",
					PortOfDischargeCode = "LHR",
					PortOfDischargeName = "London Heathrow xxxx",
					EstimatedDeparture = baseDate.AddHours(12),
					EstimatedArrival = baseDate.AddHours(18).AddMinutes(15),
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

			AdditionalDetails = new ObservableCollection<AdditionalDetailsViewModel>()
			{
				new AdditionalDetailsViewModel("CO2 Emission", "XX Value"),
				new AdditionalDetailsViewModel("CO2 Emission", "XX Value"),
				new AdditionalDetailsViewModel("CO2 Emission", "XX Value")
			};

			CostBreakdownCharges = new ObservableCollection<CostBreakdownChargeViewModel>
			{
				new CostBreakdownChargeViewModel
				{
					ChargeCode = "FRC",
					ChargeCodeDescription = "Total Freight Charge",
					Amount = 100m,
					Currency = "USD",
					LocalAmountError = "no local"
				},
				new CostBreakdownChargeViewModel
				{
					ChargeCode = "VAL",
					ChargeCodeDescription = "Valuation",
					Amount = 0m,
					Currency = "USD",
					LocalAmountError = "no local"
				},
				new CostBreakdownChargeViewModel
				{
					ChargeCode = "XX",
					ChargeCodeDescription = "Other (XX)",
					Amount = 0.40m,
					Currency = "USD",
					LocalAmountError = "no local"
				},
			};

			Origin = "SYD";
			Destination = "LHR";
			CarrierCode = "AF";
			CarrierName = "Air France";
			Remarks = "Online booking - Confirm immediately";
			FreightCharges.Add(new BookingEngineChargeViewModel()
			{
				Amount = 10.25m,
				Currency = "USD",
				ChargeCode = "STANDARD",
				ChargeCodeDescription = "Standard Cargo",
				LocalAmountError = "Some error!!"
			});

			IsExpanded = true;
			Currency = "USD";
			CommodityGroups = new List<string>() { "Personal effects" };
			DisplayPrice = true;
			TotalPriceString = "100.40 USD";
			DepartureTime = TransportLegs.First().EstimatedDeparture;
			ArrivalTime = TransportLegs.Last().EstimatedArrival;
			MainCharge = new BookingEngineChargeViewModel()
			{
				Amount = 10.25m,
				Currency = "USD",
				ChargeCode = "STANDARD",
				ChargeCodeDescription = "Standard Cargo",
				LocalAmountError = "Some error!!"
			};
		}

		public new bool DisplayPrice { get; set; }
		public new string TotalPriceString { get; set; }
	}
}

#endregion
