using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using FluentAssertions;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingConsolRatingAdaptersProviderTest : BaseFreightTest
	{
		public void TestUldConsol_ShipmentHasPackedAndUnpackedLines_CreateLseAndUldAdapters()
		{
			var shipment1 = CreateShipment(transportMode: "AIR", containerMode: "ULD");

			// This shipment has no pack lines. We add it to the test to make sure its weight is not considered by LSE adapter.
			// LSE adapter should only consider measures from unpacked pack lines.
			var shipment2 = CreateShipment(transportMode: "AIR", containerMode: "ULD");
			shipment2.JS_ActualWeight = 500;
			shipment2.OuterPackLines.RemoveAll();

			var consol1 = CreateConsol(transportMode: "AIR", containerMode: "ULD");
			consol1.Shipments.Add(shipment1);
			consol1.Shipments.Add(shipment2);

			var packLines = new[]
			{
				shipment1.AddPackLine(weight: 100),	// 0
				shipment1.AddPackLine(weight: 200),	// 1
				shipment1.AddPackLine(weight: 300),	// 2
				shipment1.AddPackLine(weight: 400),	// 3
				shipment1.AddPackLine(weight: 500), // 4
				shipment1.AddPackLine(weight: 600), // 5
			};

			// Packing some of shipment pack lines into consol 1 containers
			consol1.AddContainer(containerType: "LD-1", count: 2, packLines: new[] { packLines[0], packLines[1] });
			consol1.AddContainer(containerType: "LD-2", count: 1, packLines: new[] { packLines[4] });
			consol1.AddContainer(containerType: "LD-3", count: 1, packLines: new[] { packLines[5] });

			var adapters = GetAdapters(consol1);

			var uldAdapter = adapters.OfType<UldForwardingConsolRatingAdapter>().First();
			var uldMeasures = ((RateableMeasureSet)uldAdapter.RateableMeasures);
			uldMeasures.GetActual(MeasureType.Weight).Should().Be(1400m, "Should include total weight of packed lines into consol 1, i.e. 100 + 200 + 500 + 600");

			var lseAdapter = adapters.OfType<LseInUldForwardingConsolRatingAdapter>().First();
			var lseMeasures = ((RateableMeasureSet)lseAdapter.RateableMeasures);
			lseMeasures.GetActual(MeasureType.Weight).Should().Be(700m, "Should include total weight of unpacked lines, i.e. 300 + 400. 500 KG from shipment 2 must be ignored.");

			Assert(true);
		}

		public void TestUldConsol_AllShipmentLinesArePacked_CreateUldAdaptorOnly()
		{
			var shipment = CreateShipment(transportMode: "AIR", containerMode: "ULD");

			var consol1 = CreateConsol(transportMode: "AIR", containerMode: "ULD");
			consol1.Shipments.Add(shipment);

			var consol2 = CreateConsol(transportMode: "AIR", containerMode: "ULD");
			consol2.Shipments.Add(shipment);

			var packLines = new[]
			{
				shipment.AddPackLine(weight: 100),
				shipment.AddPackLine(weight: 200),
				shipment.AddPackLine(weight: 300),
				shipment.AddPackLine(weight: 400),
				shipment.AddPackLine(weight: 500),
				shipment.AddPackLine(weight: 600),
			};

			consol1.AddContainer(containerType: "LD-1", count: 2, packLines: new[] { packLines[0], packLines[1], packLines[2], packLines[3] });
			consol1.AddContainer(containerType: "LD-2", count: 1, packLines: new[] { packLines[4] });
			consol1.AddContainer(containerType: "LD-3", count: 1, packLines: new[] { packLines[5] });

			consol2.AddContainer(containerType: "LD-1", count: 4, packLines: new[] { packLines[1], packLines[2] });
			consol2.AddContainer(containerType: "LD-2", count: 5, packLines: new[] { packLines[3] });

			var adapters = GetAdapters(consol1);

			var uldAdapter = adapters.OfType<UldForwardingConsolRatingAdapter>().First();
			var uldMeasures = ((RateableMeasureSet)uldAdapter.RateableMeasures);
			uldMeasures.GetActual(MeasureType.Weight).Should().Be(2100m, "Should include total weight of packed lines");

			adapters.OfType<LseInUldForwardingConsolRatingAdapter>().Should().BeEmpty("There are no LSE measures, all shipment pack lines are packed in this consol");

			Assert(true);
		}

		public void TestUldConsol_ShipmentHasNoPackLines_CreateUldAdaptorOnly()
		{
			var shipment = CreateShipment(transportMode: "AIR", containerMode: "ULD");

			var consol1 = CreateConsol(transportMode: "AIR", containerMode: "ULD");
			consol1.Shipments.Add(shipment);

			var consol2 = CreateConsol(transportMode: "AIR", containerMode: "ULD");
			consol2.Shipments.Add(shipment);

			consol1.AddContainer(containerType: "LD-1", count: 2, packLines: Array.Empty<PackLine>());
			consol1.AddContainer(containerType: "LD-2", count: 1, packLines: Array.Empty<PackLine>());
			consol1.AddContainer(containerType: "LD-3", count: 1, packLines: Array.Empty<PackLine>());

			consol2.AddContainer(containerType: "LD-1", count: 4, packLines: Array.Empty<PackLine>());
			consol2.AddContainer(containerType: "LD-2", count: 5, packLines: Array.Empty<PackLine>());

			var adapters = GetAdapters(consol1);

			var uldAdapter = adapters.OfType<UldForwardingConsolRatingAdapter>().First();
			var uldMeasures = ((RateableMeasureSet)uldAdapter.RateableMeasures);
			uldMeasures.GetActual(MeasureType.ContainerCount).Should().Be(4m);

			adapters.OfType<LseInUldForwardingConsolRatingAdapter>().Should().BeEmpty("There are no LSE measures, all shipment pack lines are packed in this consol");

			Assert(true);
		}

		public void TestGetAdapters()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.AddTransport("AUMEL", "AUSYD", bookingReference: "Route1");
			consol.AddTransport("AUSYD", "AUBNE", bookingReference: "Route2");

			consol.Shipments.AddNew();
			consol.Shipments.AddNew();

			consol.IsGateway().Should().BeFalse();
			GetAdapters(consol).Single().Should().BeOfType<ForwardingConsolRatingAdapter>("No gateway, no multi-route then there should be only standard consol adapter");

			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "USMEM";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.First().PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var orgAppointedAgentPorts1 = Factory.New<OrgAppointedAgentPorts>();
			orgAppointedAgentPorts1.O5_PortOrCountry = "USMEM";
			orgAppointedAgentPorts1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPorts1.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(orgAppointedAgentPorts1);

			consol.IsGateway().Should().BeTrue("Gateway should have been setup");

			RatingDataRegistry.Instance.MultiModalRatingCost.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value: false);
			GetAdapters(consol)
				.Select(x => x.GetType())
				.Should().BeEquivalentTo(
					new[]
					{
						// standard consol adapter
						typeof(ForwardingConsolRatingAdapter),
						// +2 new gateway adapters for 2 shipments
						typeof(GatewayShipmentRatingAdapter),
						typeof(GatewayShipmentRatingAdapter)
					},
					options => options.WithStrictOrdering(),
					because: "Gateway no multi-route, there should be gateway shipment adapters");

			RatingDataRegistry.Instance.MultiModalRatingCost.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value: true);
			GetAdapters(consol)
				.Select(x => x.GetType())
				.Should().BeEquivalentTo(
					new[]
					{
						// 2 adapters for 2 routes
						typeof(ForwardingConsolRatingAdapter),
						typeof(ForwardingConsolRatingAdapter),
						// 1 specific adapter for ORG/DST service providers
						typeof(ForwardingConsolRatingAdapter),
						// 1 service adapter for consol
						typeof(ForwardingConsolJobServicesAdapter),
						typeof(GatewayShipmentRatingAdapter),
						typeof(GatewayShipmentRatingAdapter)
					},
					options => options.WithStrictOrdering(),
					because: "Gateway multi-route, there should be route adapters; standard consol adapter is replaced by a specific adapter for ORG/DST service providers");

			Assert("FluentAssertions", condition: true);
		}

		public void TestGetAdapters_MultiRoute_SupportsManualRateSelectionFlag()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUMEL";
			consol.JK_BookingReference = "mainroute";
			consol.Transports.DeleteAll();
			consol.AddTransport("CNSHA", "AUSYD", bookingReference: "Route1");
			consol.AddTransport("AUSYD", "AUMEL", bookingReference: "Route2");

			RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var adapters = GetAdapters(consol);
			AssertEquals("Adapters count mismatch", 4, adapters.Count());

			var firstLegAdapter = adapters.First(x => x.Origin.Code == "CNSHA" && x.Destination.Code == "AUSYD");
			AssertEquals("First leg adapter type mismatch", typeof(ForwardingConsolRatingAdapter), firstLegAdapter.GetType());
			Assert("First Transport Leg should support manual rate selection", ((IManualRateSelectionSupporter)firstLegAdapter).SupportsManualRateSelection);

			var secondLegAdapter = adapters.First(x => x.Origin.Code == "AUSYD" && x.Destination.Code == "AUMEL");
			AssertEquals("Second leg adapter type mismatch", typeof(ForwardingConsolRatingAdapter), secondLegAdapter.GetType());
			Assert("Second Transport Leg should support manual rate selection", ((IManualRateSelectionSupporter)secondLegAdapter).SupportsManualRateSelection);

			var firstLoadLastDischargeAdapter = adapters.First(x => x.GetType() == typeof(ForwardingConsolRatingAdapter) && x.Origin.Code == "CNSHA" && x.Destination.Code == "AUMEL");
			AssertNotNull("First Load & Last Discharge adapter is missing", firstLoadLastDischargeAdapter);
			Assert("First Load & Last Discharge should NOT support manual rate selection when multi-route is enabled",
				!((IManualRateSelectionSupporter)firstLoadLastDischargeAdapter).SupportsManualRateSelection);

			var servicesAdapter = adapters.First(x => x.GetType() == typeof(ForwardingConsolJobServicesAdapter) && x.Origin.Code == "CNSHA" && x.Destination.Code == "AUMEL");
			AssertNotNull("Services adapter is missing", servicesAdapter);
			Assert("Services should NEVER support manual rate selection",
				!((IManualRateSelectionSupporter)servicesAdapter).SupportsManualRateSelection);
		}

		#region Helpers

		IEnumerable<IAutoRating> GetAdapters(ForwardingConsol consol)
		{
			var adaptersProvider = new ForwardingConsolRatingAdaptersProvider(consol);
			var adapters = adaptersProvider.GetAdapters(Logger, new AutoRateOptions(autoRateCost: true, billingType: BillingType.Invoicing));

			return adapters;
		}

		RatingAdaptersProviderTest.TestUIInteractor Logger { get; } = new ();

		protected ForwardingShipment CreateShipment(
			string transportMode = "SEA",
			string containerMode = "FCL",
			string origin = "UAIEV",
			string destination = "AUSYD")
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			shipment.JS_PackingMode = containerMode;
			shipment.JS_TransportMode = transportMode;

			return shipment;
		}

		protected ForwardingConsol CreateConsol(
			string transportMode = "SEA",
			string containerMode = "FCL",
			string origin = "UAIEV",
			string destination = "AUSYD")
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = transportMode;
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;
			consol.Transports[0].JW_IsLinked = false;
			consol.JK_ConsolMode = containerMode;

			return consol;
		}

		#endregion
	}
}
