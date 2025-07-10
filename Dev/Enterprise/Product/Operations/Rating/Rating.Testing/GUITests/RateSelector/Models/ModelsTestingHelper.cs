using System.Collections.Generic;
using CargoWise.Integration;
using Enterprise.Freight.Integration;
using Enterprise.Rating.GUI.RateSelector.Models;
using Moq;

namespace Enterprise.Rating.Testing.GUITests.RateSelector.Models
{
	public static class ModelsTestingHelper
	{
		public static Mock<IBookingRate> GetMockedBookingRate(
			BookingEngineChargeViewModel charge,
			string carrierPrefix = null,
			string remarks = null,
			IReadOnlyCollection<IBookingTransportLeg> transportLegs = null,
			IReadOnlyCollection<ICodeDescription> additionalDetails = null,
			IReadOnlyCollection<IBookingCostBreakdownCharge> costBreakdownCharges = null)
		{
			return GetMockedBookingRate(
				charge.Amount,
				charge.Currency,
				charge.ChargeCode,
				charge.ChargeCodeDescription,
				carrierPrefix,
				remarks,
				transportLegs,
				additionalDetails,
				costBreakdownCharges);
		}

		public static Mock<IBookingRate> GetMockedBookingRate(
			decimal amount = 0,
			string currency = null,
			string chargeCode = null,
			string chargeCodeDescription = null,
			string carrierPrefix = null,
			string remarks = null,
			IReadOnlyCollection<IBookingTransportLeg> transportLegs = null,
			IReadOnlyCollection<ICodeDescription> additionalDetails = null,
			IReadOnlyCollection<IBookingCostBreakdownCharge> costBreakdownCharges = null
		)
		{
			var rate = new Mock<IBookingRate>();
			rate.SetupGet(r => r.Amount).Returns(amount);
			rate.SetupGet(r => r.Currency).Returns(currency);
			rate.SetupGet(r => r.ChargeCode).Returns(chargeCode);
			rate.SetupGet(r => r.ChargeCodeDescription).Returns(chargeCodeDescription);
			rate.SetupGet(r => r.CarrierPrefix).Returns(carrierPrefix);
			rate.SetupGet(r => r.Remarks).Returns(remarks);
			rate.SetupGet(r => r.TransportLegs).Returns(transportLegs);
			rate.SetupGet(r => r.AdditionalDetails).Returns(additionalDetails);
			rate.SetupGet(r => r.CostBreakdownCharges).Returns(costBreakdownCharges);

			return rate;
		}
	}
}
