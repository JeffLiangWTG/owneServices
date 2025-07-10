using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using UShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	static class BookingRateBuilder
	{
		public static IBookingRate Build(string carrierPrefix, UShipment shipment)
		{
			if (shipment == null
				|| shipment.TransportLegCollection?.Count == 0
				|| shipment.ConsolCosts == null
				|| shipment.ConsolCosts.ConsolCostLineCollection?.Count == 0
				|| shipment.AddInfoCollection?.Count == 0)
			{
				return null;
			}

			var consolCostLine = shipment.ConsolCosts.ConsolCostLineCollection[0];

			var rate = new BookingRate
			{
				ChargeCode = consolCostLine.ChargeCode?.Code,
				ChargeCodeDescription = consolCostLine.ChargeCode?.Description,
				Amount = consolCostLine.CostOSAmount.GetValueOrDefault(),
				Currency = consolCostLine.CostOSCurrency?.Code,
				CarrierPrefix = carrierPrefix
			};

			const string remarksKey = "RateDescription";

			rate.Remarks = shipment
				.AddInfoCollection
				.FirstOrDefault(i => i.Key.HasValue && i.Key.Value.EqualsIgnoringCase(remarksKey))
				?.Value;

			rate.TransportLegs = shipment
				.TransportLegCollection
				.Select(CreateTransportLeg)
				.ToArray();

			CreateAdditionalDetails(shipment, rate);
			CreateCostBreakdownCharges(shipment, rate);
			return rate;
		}

		static void CreateAdditionalDetails(UShipment shipment, BookingRate rate)
		{
			if (shipment.CustomizedFieldCollection != null)
			{
				rate.AdditionalDetails =
					shipment.CustomizedFieldCollection
					.Where(customizedField => customizedField != null && customizedField.Key.HasValue && customizedField.Value.HasValue)
					.Select(customizedField => new CustomizedFieldCodeDescription(customizedField.Key.Value, customizedField.Value.Value))
					.ToArray();
			}
		}

		static void CreateCostBreakdownCharges(UShipment shipment, BookingRate rate)
		{
			var chargeLinesData = shipment.JobCosting?.ChargeLineCollection;
			if (chargeLinesData != null)
			{
				var costBreadownCharges = new List<IBookingCostBreakdownCharge>();
				foreach (var chargeLine in chargeLinesData)
				{
					costBreadownCharges.Add(new AirBookingCostBreakdownCharge
					{
						Amount = chargeLine.CostOSAmount ?? 0.0m,
						Currency = chargeLine.CostOSCurrency?.Code ?? ZString.Empty,
						ChargeCode = chargeLine.ChargeCode?.Code ?? ZString.Empty,
						ChargeCodeDescription = chargeLine.ChargeCode?.Description ?? ZString.Empty
					});
				}
				rate.CostBreakdownCharges = costBreadownCharges.AsReadOnly();
			}
		}

		static IBookingTransportLeg CreateTransportLeg(TransportLeg transportLeg)
		{
			var transportMode = transportLeg.TransportMode.HasValue
				? Convert.ToString(transportLeg.TransportMode.Value)
				: Core.Constants.TransportModes.Air;

			return new BookingTransportLeg
			{
				TransportMode = transportMode,
				LegOrder = transportLeg.LegOrder.GetValueOrDefault(),
				VoyageNumber = transportLeg.VoyageFlightNo.GetValueOrDefault(),
				VesselName = transportLeg.AircraftType?.Description,
				VesselType = transportLeg.AircraftType?.Code,
				EstimatedDeparture = transportLeg.EstimatedDeparture.GetValueOrDefault().ToDateTime(),
				EstimatedArrival = transportLeg.EstimatedArrival.GetValueOrDefault().ToDateTime(),
				PortOfLoadingCode = transportLeg.PortOfLoading?.Code,
				PortOfLoadingName = transportLeg.PortOfLoading?.Name,
				PortOfDischargeCode = transportLeg.PortOfDischarge?.Code,
				PortOfDischargeName = transportLeg.PortOfDischarge?.Name
			};
		}
	}
}
