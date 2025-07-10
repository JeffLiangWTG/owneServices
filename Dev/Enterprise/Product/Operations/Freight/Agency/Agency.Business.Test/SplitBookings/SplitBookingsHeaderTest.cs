using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(SplitBookingsHeader))]
	internal class SplitBookingsHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestNew()
		{
			OrgHeader bookingParty = Factory.NewWithValidTestData<OrgHeader>();
			AgencyBooking booking = Factory.New<AgencyBooking>();
			booking.JS_UniqueConsignRef = "consign";
			booking.JS_CFSReference = "cfs";
			booking.JS_HouseBill = "bill";
			booking.JS_GoodsDescription = "description";
			booking.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			AgencyBookingContainer container = booking.BookedContainers.AddNew();
			AgencyBookingPackLine packline = booking.OuterPackLines.AddNew();
			Factory.Save();
			SplitBookingsHeader header = SplitBookingsHeader.New(booking);
			AssertEquals("OriginalShipment", booking, header.OriginalShipment);
			AssertEquals("OriginalShipment.HasChanges", false, booking.HasChanges);
			AssertNotNull("NewShipment", header.NewShipment);
			AssertEquals("", header.NewShipment.JS_UniqueConsignRef);
			AssertEquals("", header.NewShipment.JS_CFSReference);
			AssertEquals("", header.NewShipment.JS_HouseBill);
			AssertEquals("description", header.NewShipment.JS_GoodsDescription);
			AssertEquals("BookingParty", bookingParty.MainAddress.PK, header.NewShipment.BookingPartyDocumentaryAddress.E2_OA_Address);
			AssertEquals("Packlines", 0, header.NewShipment.OuterPackLines.Count);
			AssertEquals("BookedContainers", 0, header.NewShipment.BookedContainers.Count);
		}

		public void TestShowContainers()
		{
			SplitBookingsHeader header;
			AgencyBooking booking = Factory.New<AgencyBooking>();
			booking.JS_PackingMode = Constants.ContainerModes.BreakBulk;
			header = SplitBookingsHeader.New(booking);
			AssertEquals(false, header.ShowContainers);
			booking.JS_PackingMode = Constants.ContainerModes.FCL;
			header = SplitBookingsHeader.New(booking);
			AssertEquals(true, header.ShowContainers);
		}

		public void TestShowActualContainers()
		{
			SplitBookingsHeader header;
			AgencyBooking booking = Factory.New<AgencyBooking>();
			booking.JS_PackingMode = Constants.ContainerModes.FCL;
			header = SplitBookingsHeader.New(booking);
			AssertEquals(false, header.ShowActualContainers);
			booking.RealContainers.AddNew();
			header = SplitBookingsHeader.New(booking);
			AssertEquals(true, header.ShowActualContainers);
		}

		public void TestShowTopLevelPacksGetter_ComplexTest()
		{
			SplitBookingsHeader header;
			var booking = Factory.New<AgencyBooking>();
			booking.JS_PackingMode = Constants.ContainerModes.FCL;
			header = SplitBookingsHeader.New(booking);
			AssertEquals(false, header.ShowTopLevelPacks);
			booking.JS_PackingMode = Constants.ContainerModes.Liquid;
			header = SplitBookingsHeader.New(booking);
			AssertEquals(true, header.ShowTopLevelPacks);
		}

		public void TestShowVehiclesGetter_ComplexTest()
		{
			SplitBookingsHeader header;
			var booking = Factory.New<AgencyBooking>();
			booking.JS_PackingMode = Constants.ContainerModes.FCL;
			header = SplitBookingsHeader.New(booking);
			AssertEquals(false, header.ShowVehicles);
			booking.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;
			header = SplitBookingsHeader.New(booking);
			AssertEquals(true, header.ShowVehicles);
		}

		public void TestShowPackLinesGetter_ComplexTest()
		{
			SplitBookingsHeader header;
			var booking = Factory.New<AgencyBooking>();
			booking.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;
			header = SplitBookingsHeader.New(booking);
			AssertEquals(false, header.ShowPackLines);
			booking.JS_PackingMode = Constants.ContainerModes.FCL;
			header = SplitBookingsHeader.New(booking);
			AssertEquals(true, header.ShowPackLines);
		}

		public void TestMovePackline()
		{
			AgencyBooking booking = Factory.New<AgencyBooking>();
			AgencyBookingContainer container1 = booking.BookedContainers.AddNew();
			container1.JC_ContainerNum = "container1";
			AgencyBookingContainer container2 = booking.BookedContainers.AddNew();
			container1.JC_ContainerNum = "container2";
			AgencyBookingPackLine packline1 = booking.OuterPackLines.AddNew();
			packline1.JL_Description = "packline1";
			packline1.JL_JC = container1.PK;
			AgencyBookingPackLine packline2 = booking.OuterPackLines.AddNew();
			packline2.JL_Description = "packline2";
			packline2.JL_JC = container1.PK;
			AgencyBookingPackLine packline3 = booking.OuterPackLines.AddNew();
			packline3.JL_Description = "packline3";
			packline3.JL_JC = container2.PK;
			AgencyBookingPackLine packline4 = booking.OuterPackLines.AddNew();
			packline4.JL_Description = "packline4";
			SplitBookingsHeader header = SplitBookingsHeader.New(booking);
			CombineAssertions(delegate
			{
				AssertContainsExactElementsInAnyOrder("OriginalShipment: initial packlines", (p) => p.JL_Description, new AgencyBookingPackLine[] { packline1, packline2, packline3, packline4 }, header.OriginalShipment.OuterPackLines.ToArray<AgencyBookingPackLine>());
				AssertContainsExactElementsInAnyOrder("OriginalShipment: initial containers", (c) => c.JC_ContainerNum, new AgencyBookingContainer[] { container1, container2 }, header.OriginalShipment.BookedContainers.ToArray<AgencyBookingContainer>());
				AssertContainsExactElementsInAnyOrder("NewShipment: initial packlines", (p) => p.JL_Description, System.Array.Empty<AgencyBookingPackLine>(), header.NewShipment.OuterPackLines.ToArray<AgencyBookingPackLine>());
				AssertContainsExactElementsInAnyOrder("NewShipment: initial containers", (c) => c.JC_ContainerNum, System.Array.Empty<AgencyBookingContainer>(), header.NewShipment.BookedContainers.ToArray<AgencyBookingContainer>());
			});
			header.MovePackline(packline1, SplitBookingsHeader.MoveDirection.ToNew);
			CombineAssertions(delegate
			{
				AssertContainsExactElementsInAnyOrder("OriginalShipment: move packline1", (p) => p.JL_Description, new AgencyBookingPackLine[] { packline3, packline4 }, header.OriginalShipment.OuterPackLines.ToArray<AgencyBookingPackLine>());
				AssertContainsExactElementsInAnyOrder("OriginalShipment: move packline1", (c) => c.JC_ContainerNum, new AgencyBookingContainer[] { container2 }, header.OriginalShipment.BookedContainers.ToArray<AgencyBookingContainer>());
				AssertContainsExactElementsInAnyOrder("NewShipment: move packline1", (p) => p.JL_Description, new AgencyBookingPackLine[] { packline1, packline2 }, header.NewShipment.OuterPackLines.ToArray<AgencyBookingPackLine>());
				AssertContainsExactElementsInAnyOrder("NewShipment: move packline1", (c) => c.JC_ContainerNum, new AgencyBookingContainer[] { container1 }, header.NewShipment.BookedContainers.ToArray<AgencyBookingContainer>());
			});
			header.MovePackline(packline4, SplitBookingsHeader.MoveDirection.ToNew);
			CombineAssertions(delegate
			{
				AssertContainsExactElementsInAnyOrder("OriginalShipment: move packline4", (p) => p.JL_Description, new AgencyBookingPackLine[] { packline3 }, header.OriginalShipment.OuterPackLines.ToArray<AgencyBookingPackLine>());
				AssertContainsExactElementsInAnyOrder("OriginalShipment: move packline4", (c) => c.JC_ContainerNum, new AgencyBookingContainer[] { container2 }, header.OriginalShipment.BookedContainers.ToArray<AgencyBookingContainer>());
				AssertContainsExactElementsInAnyOrder("NewShipment: move packline4", (p) => p.JL_Description, new AgencyBookingPackLine[] { packline1, packline2, packline4 }, header.NewShipment.OuterPackLines.ToArray<AgencyBookingPackLine>());
				AssertContainsExactElementsInAnyOrder("NewShipment: move packline4", (c) => c.JC_ContainerNum, new AgencyBookingContainer[] { container1 }, header.NewShipment.BookedContainers.ToArray<AgencyBookingContainer>());
			});
			header.MovePackline(packline1, SplitBookingsHeader.MoveDirection.ToOriginal);
			CombineAssertions(delegate
			{
				AssertContainsExactElementsInAnyOrder("OriginalShipment: move packline1 back", (p) => p.JL_Description, new AgencyBookingPackLine[] { packline1, packline2, packline3 }, header.OriginalShipment.OuterPackLines.ToArray<AgencyBookingPackLine>());
				AssertContainsExactElementsInAnyOrder("OriginalShipment: move packline1 back", (c) => c.JC_ContainerNum, new AgencyBookingContainer[] { container1, container2 }, header.OriginalShipment.BookedContainers.ToArray<AgencyBookingContainer>());
				AssertContainsExactElementsInAnyOrder("NewShipment: move packline1 back", (p) => p.JL_Description, new AgencyBookingPackLine[] { packline4 }, header.NewShipment.OuterPackLines.ToArray<AgencyBookingPackLine>());
				AssertContainsExactElementsInAnyOrder("NewShipment: move packline1 back", (c) => c.JC_ContainerNum, System.Array.Empty<AgencyBookingContainer>(), header.NewShipment.BookedContainers.ToArray<AgencyBookingContainer>());
			});
		}

		public void TestMoveBookedContainer()
		{
			AgencyBooking booking = Factory.New<AgencyBooking>();
			AgencyBookingContainer container1 = booking.BookedContainers.AddNew();
			container1.JC_ContainerNum = "container1";
			AgencyBookingContainer container2 = booking.BookedContainers.AddNew();
			container1.JC_ContainerNum = "container2";
			AgencyBookingContainer container3 = booking.BookedContainers.AddNew();
			container3.JC_ContainerNum = "container3";
			AgencyBookingPackLine packline1 = booking.OuterPackLines.AddNew();
			packline1.JL_Description = "packline1";
			packline1.JL_JC = container1.PK;
			AgencyBookingPackLine packline2 = booking.OuterPackLines.AddNew();
			packline2.JL_Description = "packline2";
			packline2.JL_JC = container1.PK;
			AgencyBookingPackLine packline3 = booking.OuterPackLines.AddNew();
			packline3.JL_Description = "packline3";
			packline3.JL_JC = container2.PK;
			AgencyBookingPackLine packline4 = booking.OuterPackLines.AddNew();
			packline4.JL_Description = "packline4";
			SplitBookingsHeader header = SplitBookingsHeader.New(booking);
			CombineAssertions(delegate
			{
				AssertContainsExactElementsInAnyOrder("OriginalShipment: initial packlines", (p) => p.JL_Description, new AgencyBookingPackLine[] { packline1, packline2, packline3, packline4 }, header.OriginalShipment.OuterPackLines.ToArray<AgencyBookingPackLine>());
				AssertContainsExactElementsInAnyOrder("OriginalShipment: initial containers", (c) => c.JC_ContainerNum, new AgencyBookingContainer[] { container1, container2, container3 }, header.OriginalShipment.BookedContainers.ToArray<AgencyBookingContainer>());
				AssertContainsExactElementsInAnyOrder("NewShipment: initial packlines", (p) => p.JL_Description, System.Array.Empty<AgencyBookingPackLine>(), header.NewShipment.OuterPackLines.ToArray<AgencyBookingPackLine>());
				AssertContainsExactElementsInAnyOrder("NewShipment: initial containers", (c) => c.JC_ContainerNum, System.Array.Empty<AgencyBookingContainer>(), header.NewShipment.BookedContainers.ToArray<AgencyBookingContainer>());
			});
			header.MoveBookedContainer(container1, SplitBookingsHeader.MoveDirection.ToNew);
			CombineAssertions(delegate
			{
				AssertContainsExactElementsInAnyOrder("OriginalShipment: move container1", (p) => p.JL_Description, new AgencyBookingPackLine[] { packline3, packline4 }, header.OriginalShipment.OuterPackLines.ToArray<AgencyBookingPackLine>());
				AssertContainsExactElementsInAnyOrder("OriginalShipment: move container1", (c) => c.JC_ContainerNum, new AgencyBookingContainer[] { container2, container3 }, header.OriginalShipment.BookedContainers.ToArray<AgencyBookingContainer>());
				AssertContainsExactElementsInAnyOrder("NewShipment: move container1", (p) => p.JL_Description, new AgencyBookingPackLine[] { packline1, packline2 }, header.NewShipment.OuterPackLines.ToArray<AgencyBookingPackLine>());
				AssertContainsExactElementsInAnyOrder("NewShipment: move container1", (c) => c.JC_ContainerNum, new AgencyBookingContainer[] { container1 }, header.NewShipment.BookedContainers.ToArray<AgencyBookingContainer>());
			});
			header.MoveBookedContainer(container3, SplitBookingsHeader.MoveDirection.ToNew);
			CombineAssertions(delegate
			{
				AssertContainsExactElementsInAnyOrder("OriginalShipment: move container3", (p) => p.JL_Description, new AgencyBookingPackLine[] { packline3, packline4 }, header.OriginalShipment.OuterPackLines.ToArray<AgencyBookingPackLine>());
				AssertContainsExactElementsInAnyOrder("OriginalShipment: move container3", (c) => c.JC_ContainerNum, new AgencyBookingContainer[] { container2 }, header.OriginalShipment.BookedContainers.ToArray<AgencyBookingContainer>());
				AssertContainsExactElementsInAnyOrder("NewShipment: move container3", (p) => p.JL_Description, new AgencyBookingPackLine[] { packline1, packline2 }, header.NewShipment.OuterPackLines.ToArray<AgencyBookingPackLine>());
				AssertContainsExactElementsInAnyOrder("NewShipment: move container3", (c) => c.JC_ContainerNum, new AgencyBookingContainer[] { container1, container3 }, header.NewShipment.BookedContainers.ToArray<AgencyBookingContainer>());
			});
			header.MoveBookedContainer(container1, SplitBookingsHeader.MoveDirection.ToOriginal);
			CombineAssertions(delegate
			{
				AssertContainsExactElementsInAnyOrder("OriginalShipment: move container1 back", (p) => p.JL_Description, new AgencyBookingPackLine[] { packline1, packline2, packline3, packline4 }, header.OriginalShipment.OuterPackLines.ToArray<AgencyBookingPackLine>());
				AssertContainsExactElementsInAnyOrder("OriginalShipment: move container1 back", (c) => c.JC_ContainerNum, new AgencyBookingContainer[] { container1, container2 }, header.OriginalShipment.BookedContainers.ToArray<AgencyBookingContainer>());
				AssertContainsExactElementsInAnyOrder("NewShipment: move container1 back", (p) => p.JL_Description, System.Array.Empty<AgencyBookingPackLine>(), header.NewShipment.OuterPackLines.ToArray<AgencyBookingPackLine>());
				AssertContainsExactElementsInAnyOrder("NewShipment: move container1 back", (c) => c.JC_ContainerNum, new AgencyBookingContainer[] { container3 }, header.NewShipment.BookedContainers.ToArray<AgencyBookingContainer>());
			});
		}

		public void TestMoveVehicles_ComplexTest()
		{
			var booking = Factory.New<AgencyBooking>();
			booking.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;
			var item1 = booking.Vehicles.AddNew();
			var item2 = booking.Vehicles.AddNew();
			var item3 = booking.Vehicles.AddNew();
			var item4 = booking.Vehicles.AddNew();
			var header = SplitBookingsHeader.New(booking);
			header.MoveVehicle(item2, SplitBookingsHeader.MoveDirection.ToNew);
			header.MoveVehicle(item4, SplitBookingsHeader.MoveDirection.ToNew);
			AssertContainsExactElementsInAnyOrder(new[] { item1, item3 }, header.OriginalShipment.Vehicles.Cast<AgencyShipmentContainer>());
			AssertContainsExactElementsInAnyOrder(new[] { item2, item4 }, header.NewShipment.Vehicles.Cast<AgencyShipmentContainer>());
			header.MoveVehicle(item2, SplitBookingsHeader.MoveDirection.ToOriginal);
			AssertContainsExactElementsInAnyOrder(new[] { item1, item2, item3 }, header.OriginalShipment.Vehicles.Cast<AgencyShipmentContainer>());
			AssertContainsExactElementsInAnyOrder(new[] { item4 }, header.NewShipment.Vehicles.Cast<AgencyShipmentContainer>());
		}

		public void TestMoveTopLevelPacks_ComplexTest()
		{
			var booking = Factory.New<AgencyBooking>();
			booking.JS_PackingMode = Constants.ContainerModes.Liquid;
			var item1 = booking.TopLevelPacks.AddNew();
			var item2 = booking.TopLevelPacks.AddNew();
			var item3 = booking.TopLevelPacks.AddNew();
			var item4 = booking.TopLevelPacks.AddNew();
			var header = SplitBookingsHeader.New(booking);
			header.MoveTopLevelPack(item2, SplitBookingsHeader.MoveDirection.ToNew);
			header.MoveTopLevelPack(item4, SplitBookingsHeader.MoveDirection.ToNew);
			AssertContainsExactElementsInAnyOrder(new[] { item1, item3 }, header.OriginalShipment.TopLevelPacks.Cast<AgencyShipmentContainer>());
			AssertContainsExactElementsInAnyOrder(new[] { item2, item4 }, header.NewShipment.TopLevelPacks.Cast<AgencyShipmentContainer>());
			header.MoveTopLevelPack(item2, SplitBookingsHeader.MoveDirection.ToOriginal);
			AssertContainsExactElementsInAnyOrder(new[] { item1, item2, item3 }, header.OriginalShipment.TopLevelPacks.Cast<AgencyShipmentContainer>());
			AssertContainsExactElementsInAnyOrder(new[] { item4 }, header.NewShipment.TopLevelPacks.Cast<AgencyShipmentContainer>());
		}

		public void TestMoveActualContainer()
		{
			AgencyBooking booking = Factory.New<AgencyBooking>();
			AgencyBookingContainer container1 = booking.RealContainers.AddNew();
			container1.JC_ContainerNum = "container1";
			AgencyBookingContainer container2 = booking.RealContainers.AddNew();
			container1.JC_ContainerNum = "container2";
			AgencyBookingContainer container3 = booking.RealContainers.AddNew();
			container3.JC_ContainerNum = "container3";
			SplitBookingsHeader header = SplitBookingsHeader.New(booking);
			CombineAssertions(delegate
			{
				AssertContainsExactElementsInAnyOrder("OriginalShipment: initial containers", (c) => c.JC_ContainerNum, new AgencyBookingContainer[] { container1, container2, container3 }, header.OriginalShipment.RealContainers.ToArray<AgencyBookingContainer>());
				AssertContainsExactElementsInAnyOrder("NewShipment: initial containers", (c) => c.JC_ContainerNum, System.Array.Empty<AgencyBookingContainer>(), header.NewShipment.RealContainers.ToArray<AgencyBookingContainer>());
			});
			header.MoveActualContainer(container1, SplitBookingsHeader.MoveDirection.ToNew);
			CombineAssertions(delegate
			{
				AssertContainsExactElementsInAnyOrder("OriginalShipment: move container1", (c) => c.JC_ContainerNum, new AgencyBookingContainer[] { container2, container3 }, header.OriginalShipment.RealContainers.ToArray<AgencyBookingContainer>());
				AssertContainsExactElementsInAnyOrder("NewShipment: move container1", (c) => c.JC_ContainerNum, new AgencyBookingContainer[] { container1 }, header.NewShipment.RealContainers.ToArray<AgencyBookingContainer>());
			});
			header.MoveActualContainer(container3, SplitBookingsHeader.MoveDirection.ToNew);
			CombineAssertions(delegate
			{
				AssertContainsExactElementsInAnyOrder("OriginalShipment: move container3", (c) => c.JC_ContainerNum, new AgencyBookingContainer[] { container2 }, header.OriginalShipment.RealContainers.ToArray<AgencyBookingContainer>());
				AssertContainsExactElementsInAnyOrder("NewShipment: move container3", (c) => c.JC_ContainerNum, new AgencyBookingContainer[] { container1, container3 }, header.NewShipment.RealContainers.ToArray<AgencyBookingContainer>());
			});
			header.MoveActualContainer(container1, SplitBookingsHeader.MoveDirection.ToOriginal);
			CombineAssertions(delegate
			{
				AssertContainsExactElementsInAnyOrder("OriginalShipment: move container1 back", (c) => c.JC_ContainerNum, new AgencyBookingContainer[] { container1, container2 }, header.OriginalShipment.RealContainers.ToArray<AgencyBookingContainer>());
				AssertContainsExactElementsInAnyOrder("NewShipment: move container1 back", (c) => c.JC_ContainerNum, new AgencyBookingContainer[] { container3 }, header.NewShipment.RealContainers.ToArray<AgencyBookingContainer>());
			});
		}

		public void TestCopyingTransportsToNewShipment()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "MYBAG";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "CNCAN";
			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];
			var booking = Factory.New<AgencyBooking>();
			booking.JS_JX = sailing.PK;
			var transport1 = booking.Transports.AddNew();
			transport1.JW_TransportMode = Constants.TransportModes.Sea;
			transport1.JW_RL_NKLoadPort = "SGSIN";
			transport1.JW_RL_NKDiscPort = "AUMEL";
			transport1.JW_ETA = new ZDateTime(2013, 5, 5);
			transport1.JW_VoyageFlight = "QF789";
			transport1.JW_Vessel = "CONDOR";
			transport1.JW_IsLinked = true;
			var transport2 = booking.Transports.AddNew();
			transport2.JW_TransportMode = Constants.TransportModes.Road;
			transport2.JW_RL_NKLoadPort = "AUMEL";
			transport2.JW_RL_NKDiscPort = "AUSYD";
			transport2.JW_ETD = new ZDateTime(2013, 5, 15);
			transport2.JW_IsLinked = true;
			var transport3 = booking.Transports.AddNew();
			transport3.JW_TransportMode = Constants.TransportModes.Air;
			transport3.JW_RL_NKLoadPort = "AUSYD";
			transport3.JW_RL_NKDiscPort = "CNSHA";
			transport3.JW_IsLinked = false;
			Factory.Save();
			var header = SplitBookingsHeader.New(booking);
			AssertEquals("Number of transports on the original shipment should remain the same", booking.Transports.Count, header.OriginalShipment.Transports.Count);
			AssertEquals("Number of transports on the new shipment should remain the same as the original", booking.Transports.Count, header.NewShipment.Transports.Count);
			AssertTransportLegsMatch(booking, header.OriginalShipment);
			AssertTransportLegsMatch(booking, header.NewShipment);
		}

		void AssertTransportLegsMatch(AgencyShipment expectedShipment, AgencyShipment actualShipment)
		{
			var expectedTransports = expectedShipment.Transports;
			var actualTransports = actualShipment.Transports;
			AssertEquals("Expected and actual should have the same number of transports attached", expectedTransports.Count, actualTransports.Count);
			for (int i = 0; i < expectedTransports.Count; i++)
			{
				AssertEquals("Tranports should have the same load ports", expectedTransports[i].LoadPort, actualTransports[i].LoadPort);
				AssertEquals("Tranports should have the same discharge ports", expectedTransports[i].DiscPort, actualTransports[i].DiscPort);
				AssertEquals("Tranports should have the same transport type", expectedTransports[i].TransportMode, actualTransports[i].TransportMode);
				AssertEquals("Tranports should have the same voyage", expectedTransports[i].Voyage, actualTransports[i].Voyage);
				AssertEquals("Tranports should have the same voyage / flight number", expectedTransports[i].JW_VoyageFlight, actualTransports[i].JW_VoyageFlight);
				AssertEquals("Tranports should have the same vessel", expectedTransports[i].Vessel, actualTransports[i].Vessel);
				AssertEquals("Tranports should have the same ETD", expectedTransports[i].JW_ETD, actualTransports[i].JW_ETD);
				AssertEquals("Tranports should have the same ETA", expectedTransports[i].JW_ETA, actualTransports[i].JW_ETA);
				AssertEquals("Tranports should have the same ATD", expectedTransports[i].JW_ATD, actualTransports[i].JW_ATD);
				AssertEquals("Tranports should have the same ATA", expectedTransports[i].JW_ATA, actualTransports[i].JW_ATA);
				AssertEquals("Tranports should have the same linked status", expectedTransports[i].JW_IsLinked, actualTransports[i].JW_IsLinked);
				AssertEquals("Tranports should have correct parent attached", expectedShipment, expectedTransports[i].Parent);
				AssertEquals("Tranports should have correct parent attached", actualShipment, actualTransports[i].Parent);
			}
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SplitBookingsHeader(Factory);
		}
		#endregion
	}
}
