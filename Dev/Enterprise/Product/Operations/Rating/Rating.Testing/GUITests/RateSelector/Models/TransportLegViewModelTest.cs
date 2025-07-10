using System;
using Enterprise.Freight.Integration;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI.RateSelector.Models;
using Moq;

namespace Enterprise.Rating.Testing.GUITests.RateSelector.Models
{
	sealed class TransportLegViewModelTest : RatingTestCase
	{
		public void TestPopulateFromBookingTransportLeg()
		{
			var transportLeg = new Mock<IBookingTransportLeg>(MockBehavior.Loose);
			transportLeg.SetupGet(t => t.TransportMode).Returns(Core.Constants.TransportModes.Air);
			transportLeg.SetupGet(t => t.LegOrder).Returns(1);
			transportLeg.SetupGet(t => t.VoyageNumber).Returns("QF123");
			transportLeg.SetupGet(t => t.VesselType).Returns("333");
			transportLeg.SetupGet(t => t.VesselName).Returns("Airbus");
			transportLeg.SetupGet(t => t.PortOfLoadingCode).Returns("SYD");
			transportLeg.SetupGet(t => t.PortOfDischargeCode).Returns("JFK");
			transportLeg.SetupGet(t => t.EstimatedDeparture).Returns(new DateTime(2020, 12, 24));
			transportLeg.SetupGet(t => t.EstimatedArrival).Returns(new DateTime(2020, 12, 24));

			var chargeViewModel = TransportLegViewModel.New(transportLeg.Object);

			AssertEquals("Transport mode should match", TransportMode.Air, chargeViewModel.TransportMode);
			AssertEquals("Leg order should match", 1, chargeViewModel.LegOrder);
			AssertEquals("Voyage number should match", "QF123", chargeViewModel.VoyageNumber);
			AssertEquals("Vessel type should match", "333", chargeViewModel.VesselType);
			AssertEquals("Vessel name should match", "Airbus", chargeViewModel.VesselName);
			AssertEquals("Port of loading code should match", "SYD", chargeViewModel.PortOfLoadingCode);
			AssertEquals("Port of discharge code should match", "JFK", chargeViewModel.PortOfDischargeCode);
			AssertEquals("Estimated departure should match", new DateTime(2020, 12, 24), chargeViewModel.EstimatedDeparture);
			AssertEquals("Estimated arrival should match", new DateTime(2020, 12, 24), chargeViewModel.EstimatedArrival);
		}

		public void TestMapTransportModeFromBookingTransportLeg()
		{
			AssertMappedTransportMode(Core.Constants.TransportModes.Sea, TransportMode.Sea);
			AssertMappedTransportMode(Core.Constants.TransportModes.Road, TransportMode.Truck);
			AssertMappedTransportMode(Core.Constants.TransportModes.Air, TransportMode.Air);
			AssertMappedTransportMode(Core.Constants.TransportModes.Rail, TransportMode.Air);

			Assert(true);
		}

		void AssertMappedTransportMode(string transportMode, TransportMode expectedTransportMode)
		{
			var transportLeg = new Mock<IBookingTransportLeg>(MockBehavior.Loose);
			transportLeg.SetupGet(t => t.TransportMode).Returns(transportMode);

			var chargeViewModel = TransportLegViewModel.New(transportLeg.Object);

			AssertEquals("The transport mode should match the expected transport mode.", expectedTransportMode, chargeViewModel.TransportMode);
		}
	}
}
