using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.DataTransfer.Universal.Testing;
using Enterprise.Freight.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	public class AgencyShipmentContainerDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		#region Allow/Disallow import
		public void TestDisallowImportUniversalIfDifferentContainerMode()
		{
			var sailing = UniversalTestHelper.CreateSailingWithVoyage(Factory.BOFactory, "AUSYD", "NZAKL", "GENERAL FRANCO", "001");
			var booking = Factory.New<BillOfLading>();
			booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			booking.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			booking.JS_HouseBill = "S0001";
			booking.JS_JX = sailing.PK;
			Factory.SaveForTesting();
			var dataObject = GetUniversalShipmentWithNoPackingLines(Core.Constants.ContainerModes.LCL);
			var reader = GetReader(dataObject);
			BusinessObject container = null;
			reader.ReadIntoBusinessObject(ref container);
			AssertNull("container hasn't been imported", container);
			AssertMultilineASCIIEquals("contains error message", @"Error - Cannot populate AgencyShipmentContainer because:
ContainerMode of data object is not compatible with the container mode of Parent Booking/Bill of Lading and would cause existing containers and packing lines to be removed", Logger.Logs);
		}

		public void TestAllowToImportUniversalWithExactlyOneContainer()
		{
			var sailing = UniversalTestHelper.CreateSailingWithVoyage(Factory.BOFactory, "AUSYD", "NZAKL", "GENERAL FRANCO", "001");
			var booking = Factory.New<BillOfLading>();
			booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			booking.JS_HouseBill = "S0001";
			booking.JS_JX = sailing.PK;
			Factory.SaveForTesting();
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.WayBillNumber = "S0001";
			dataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg> { new TransportLeg { PortOfLoading = new UNLOCO { Code = "AUSYD" }, PortOfDischarge = new UNLOCO { Code = "NZAKL" }, TransportMode = TransportMode.Sea, VesselName = "GENERAL FRANCO", VoyageFlightNo = "001" } });
			var reader = GetReader(dataObject);
			BusinessObject container = null;
			reader.ReadIntoBusinessObject(ref container);
			AssertNull("container hasn't been imported", container);
			AssertMultilineASCIIEquals("contains error message", @"Error - Cannot populate AgencyShipmentContainer because:
Data object does not contain any containers", Logger.Logs);
			Logger.ClearLogs();
			dataObject.SetContainerCollection(() => new DataObjectList<Container> { new Container { ContainerNumber = "AAA", ContainerType = new ContainerType { Code = "20GP", ISOCode = "22G0" } }, new Container { ContainerNumber = "BBB", ContainerType = new ContainerType { Code = "20GP", ISOCode = "22G0" } } });
			reader.ReadIntoBusinessObject(ref container);
			AssertNull("container hasn't been imported", container);
			AssertMultilineASCIIEquals("contains error message", @"Error - Cannot populate AgencyShipmentContainer because:
Data object contains more than one container", Logger.Logs);
		}

		public void TestDisallowImportUniversalIfShipmentDoesNotExist()
		{
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.WayBillNumber = "S0001";
			dataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg> { new TransportLeg { PortOfLoading = new UNLOCO { Code = "AUSYD" }, PortOfDischarge = new UNLOCO { Code = "NZAKL" }, TransportMode = TransportMode.Sea, VesselName = "GENERAL FRANCO", VoyageFlightNo = "001" } });
			dataObject.SetContainerCollection(() => new DataObjectList<Container> { new Container { ContainerNumber = "AAA", ContainerType = new ContainerType { Code = "20GP", ISOCode = "22G0" } } });
			var reader = GetReader(dataObject);
			BusinessObject container = null;
			reader.ReadIntoBusinessObject(ref container);
			AssertNull("container hasn't been imported", container);
			AssertMultilineASCIIEquals("contains error message", @"Error - Cannot populate AgencyShipmentContainer because:
Parent Booking/Bill of Lading could not be located", Logger.Logs);
		}

		public void TestDisallowImportBookingUniversalOnTopOfBillOfLading()
		{
			var sailing = UniversalTestHelper.CreateSailingWithVoyage(Factory.BOFactory, "AUSYD", "NZAKL", "GENERAL FRANCO", "001");
			var booking = Factory.New<BillOfLading>();
			booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			booking.JS_HouseBill = "S0001";
			booking.JS_JX = sailing.PK;
			Factory.SaveForTesting();
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.WayBillNumber = "S0001";
			dataObject.ShipmentStatus = new CodeDescriptionPair { Code = ShipmentStatusList.Codes.Booked };
			dataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg> { new TransportLeg { PortOfLoading = new UNLOCO { Code = "AUSYD" }, PortOfDischarge = new UNLOCO { Code = "NZAKL" }, TransportMode = TransportMode.Sea, VesselName = "GENERAL FRANCO", VoyageFlightNo = "001" } });
			dataObject.SetContainerCollection(() => new DataObjectList<Container> { new Container { ContainerNumber = "AAA", ContainerType = new ContainerType { Code = "20GP", ISOCode = "22G0" } } });
			var reader = GetReader(dataObject);
			BusinessObject container = null;
			reader.ReadIntoBusinessObject(ref container);
			AssertNull("container hasn't been imported", container);
			AssertMultilineASCIIEquals("contains error message", @"Error - Cannot populate AgencyShipmentContainer because:
Parent has been confirmed but the universal shipment contains Booking", Logger.Logs);
		}

		public void TestDisallowImportBillOfLadingUniversalOnTopOfBooking()
		{
			var sailing = UniversalTestHelper.CreateSailingWithVoyage(Factory.BOFactory, "AUSYD", "NZAKL", "GENERAL FRANCO", "001");
			var booking = Factory.New<AgencyBooking>();
			booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			booking.JS_HouseBill = "S0001";
			booking.JS_JX = sailing.PK;
			Factory.SaveForTesting();
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.WayBillNumber = "S0001";
			dataObject.ShipmentStatus = new CodeDescriptionPair { Code = ShipmentStatusList.Codes.Confirmed };
			dataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg> { new TransportLeg { PortOfLoading = new UNLOCO { Code = "AUSYD" }, PortOfDischarge = new UNLOCO { Code = "NZAKL" }, TransportMode = TransportMode.Sea, VesselName = "GENERAL FRANCO", VoyageFlightNo = "001" } });
			dataObject.SetContainerCollection(() => new DataObjectList<Container> { new Container { ContainerNumber = "AAA", ContainerType = new ContainerType { Code = "20GP", ISOCode = "22G0" } } });
			var reader = GetReader(dataObject);
			BusinessObject container = null;
			reader.ReadIntoBusinessObject(ref container);
			AssertNull("container hasn't been imported", container);
			AssertMultilineASCIIEquals("contains error message", @"Error - Cannot populate AgencyShipmentContainer because:
Parent has not yet been confirmed but the universal shipment contains Bill of Lading", Logger.Logs);
		}

		#endregion
		#region Import With VerifiedGrossContainerWeight
		public void TestImportAgencyBookingContainer_WithVGM()
		{
			var booking = Factory.New<AgencyBooking>();
			var dataObject = GetUniversalShipmentWithContainerAndBuildShipment(booking, true);
			var container = dataObject.ContainerCollection[0];
			var expectedRealContainers = new[] { "AAA|5555" };
			var expectedBookedContainers = new[] { "BBB|1111" };
			AssertContainers(dataObject, booking, expectedRealContainers, expectedBookedContainers);
			container.ContainerNumber = "BBB";
			container.Seal = "6666";
			expectedRealContainers = new[] { "AAA|5555" };
			expectedBookedContainers = new[] { "BBB|6666" };
			AssertContainers(dataObject, booking, expectedRealContainers, expectedBookedContainers);
			container.ContainerNumber = "CCC";
			container.Seal = "7777";
			expectedRealContainers = new[] { "AAA|5555", "CCC|7777" };
			AssertContainers(dataObject, booking, expectedRealContainers, expectedBookedContainers);
		}

		public void TestImportAgencyBookingContainer_WithOutVGM()
		{
			var booking = Factory.New<AgencyBooking>();
			var dataObject = GetUniversalShipmentWithContainerAndBuildShipment(booking, false);
			var container = dataObject.ContainerCollection[0];
			var expectedRealContainers = new[] { "AAA|0000" };
			var expectedBookedContainers = new[] { "BBB|1111", "AAA|5555" };
			AssertContainers(dataObject, booking, expectedRealContainers, expectedBookedContainers);
			container.ContainerNumber = "BBB";
			container.Seal = "6666";
			expectedBookedContainers = new[] { "BBB|6666", "AAA|5555" };
			AssertContainers(dataObject, booking, expectedRealContainers, expectedBookedContainers);
			container.ContainerNumber = "CCC";
			container.Seal = "7777";
			expectedBookedContainers = new[] { "BBB|6666", "AAA|5555", "CCC|7777" };
			AssertContainers(dataObject, booking, expectedRealContainers, expectedBookedContainers);
		}

		public void TestImportBillOfLadingContainer()
		{
			var billOfLading = Factory.New<BillOfLading>();
			var dataObject = GetUniversalShipmentWithContainerAndBuildShipment(billOfLading, true);
			var container = dataObject.ContainerCollection[0];
			var expectedRealContainers = new[] { "AAA|5555" };
			var expectedBookedContainers = new[] { "BBB|1111" };
			AssertContainers(dataObject, billOfLading, expectedRealContainers, expectedBookedContainers);
			container.ContainerNumber = "BBB";
			container.Seal = "6666";
			expectedRealContainers = new[] { "AAA|5555", "BBB|6666" };
			AssertContainers(dataObject, billOfLading, expectedRealContainers, expectedBookedContainers);
			container.ContainerNumber = "CCC";
			container.Seal = "7777";
			expectedRealContainers = new[] { "AAA|5555", "BBB|6666", "CCC|7777" };
			AssertContainers(dataObject, billOfLading, expectedRealContainers, expectedBookedContainers);
		}

		UniversalShipment GetUniversalShipmentWithContainerAndBuildShipment(AgencyShipment shipment, bool hasVgm)
		{
			var dataObject = GetUniversalShipmentWithNoPackingLines(Core.Constants.ContainerModes.FCL);
			var container = dataObject.ContainerCollection[0];
			container.Seal = "5555";
			if (hasVgm)
			{
				dataObject.DataContext = new DataContext { RecipientRoleCollection = new List<RecipientRole> { new RecipientRole { ServiceCode = ServiceCodeType.VGM } } };
			}

			dataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{ ContainerNumber = "VIN001", GoodsDescription = "TEST" } });
			dataObject.PackingLineCollection.Content = CollectionContent.Complete;
			var sailing = UniversalTestHelper.CreateSailingWithVoyage(Factory.BOFactory, "AUSYD", "NZAKL", "GENERAL FRANCO", "001");
			shipment.JS_HouseBill = "S0001";
			shipment.JS_JX = sailing.PK;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipment.JS_CFSReference = "BKGREF001";
			var container1 = shipment.RealContainers.AddNew();
			container1.JC_ContainerNum = "AAA";
			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container1.JC_SealNum = "0000";
			var container2 = shipment.BookedContainers.AddNew();
			container2.JC_ContainerNum = "BBB";
			container2.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container2.JC_SealNum = "1111";
			Factory.SaveForTesting();
			return dataObject;
		}

		void AssertContainers(UniversalShipment dataObject, AgencyShipment shipment, string[] expectedRealContainers, string[] expectedBookedContainers)
		{
			var reader = GetReader(dataObject);
			BusinessObject importedContainer = null;
			reader.ReadIntoBusinessObject(ref importedContainer);
			AssertContainsExactElementsInAnyOrder(expectedRealContainers, FormatContainers(shipment.RealContainers));
			AssertContainsExactElementsInAnyOrder(expectedBookedContainers, FormatContainers(shipment.BookedContainers));
		}

		string[] FormatContainers(AgencyShipmentContainerDependentCollection containers)
		{
			return containers.Cast<AgencyShipmentContainer>().Select(c => string.Format("{0}|{1}", c.JC_ContainerNum, c.JC_SealNum)).ToArray();
		}

		#endregion
		#region Import internally
		public void TestImportAgencyBookingContainerInternally_MatchedByJobNumber()
		{
			var booking = Factory.New<AgencyBooking>();
			var container = booking.ShippingContainers.AddNew();
			container.JC_ContainerJobID = "D00001000";
			Factory.SaveForTesting();
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.DataContext = DataContextFactory.New();
			dataObject.DataContext.CodesMappedToTarget = true;
			dataObject.DataContext.AddDataTarget(DataContextType.AgencyShipmentContainer, container.JC_ContainerJobID);
			dataObject.SetContainerCollection(() => new DataObjectList<Container> { new Container { ReleaseNum = "0101010" } });
			AssertEquals("prerequisite", ZString.Empty, container.JC_ReleaseNum);
			var reader = GetReader(dataObject);
			BusinessObject importedContainer = container;
			reader.ReadIntoBusinessObject(ref importedContainer);
			AssertEquals("updated matching container job", "0101010", container.JC_ReleaseNum);
		}

		public void TestImportBillOfLadingContainerInternally_MatchedByJobNumber()
		{
			var booking = Factory.New<BillOfLading>();
			var container = booking.ShippingContainers.AddNew();
			container.JC_ContainerJobID = "D00001000";
			Factory.SaveForTesting();
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.DataContext = DataContextFactory.New();
			dataObject.DataContext.CodesMappedToTarget = true;
			dataObject.DataContext.AddDataTarget(DataContextType.AgencyShipmentContainer, container.JC_ContainerJobID);
			dataObject.SetContainerCollection(() => new DataObjectList<Container> { new Container { ReleaseNum = "0101010" } });
			AssertEquals("prerequisite", ZString.Empty, container.JC_ReleaseNum);
			var reader = GetReader(dataObject);
			BusinessObject importedContainer = container;
			reader.ReadIntoBusinessObject(ref importedContainer);
			AssertEquals("updated matching container job", "0101010", container.JC_ReleaseNum);
		}

		#endregion
		#region Top Level Packs
		public void TestDisallowImportRoroShipmentWithNoPacklingLines()
		{
			AssertDisallowImportTLPShipment(Core.Constants.ContainerModes.RollOnRollOff, GetUniversalShipmentWithNoPackingLines(Core.Constants.ContainerModes.RollOnRollOff));
		}

		public void TestDisallowImportRoroShipmentWithTwoPacklingLines()
		{
			var dataObject = GetUniversalShipmentWithNoPackingLines(Core.Constants.ContainerModes.RollOnRollOff);
			dataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{ ContainerNumber = "VIN001", GoodsDescription = "lada samara" }, new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{ ContainerNumber = "VIN002", GoodsDescription = "polski fiat" } });
			dataObject.PackingLineCollection.Content = CollectionContent.Complete;
			AssertDisallowImportTLPShipment(Core.Constants.ContainerModes.RollOnRollOff, dataObject);
		}

		public void TestAllowImportRoroShipmentWithCorrectNumberOfPacklingLines()
		{
			var dataObject = GetUniversalShipmentWithNoPackingLines(Core.Constants.ContainerModes.RollOnRollOff);
			dataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{ ContainerNumber = "VIN001", GoodsDescription = "lada samara" } });
			dataObject.PackingLineCollection.Content = CollectionContent.Complete;
			AssertAllowImportTLPShipment(Core.Constants.ContainerModes.RollOnRollOff, dataObject);
		}

		public void TestDisallowImportBreakBulkShipmentWithNoPacklingLines()
		{
			AssertDisallowImportTLPShipment(Core.Constants.ContainerModes.BreakBulk, GetUniversalShipmentWithNoPackingLines(Core.Constants.ContainerModes.BreakBulk));
		}

		public void TestDisallowImportBreakBulkShipmentWithTwoPacklingLines()
		{
			var dataObject = GetUniversalShipmentWithNoPackingLines(Core.Constants.ContainerModes.BreakBulk);
			dataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{ ContainerNumber = "BATCH1", GoodsDescription = "some junk" }, new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{ ContainerNumber = "BATCH2", GoodsDescription = "another crap" } });
			dataObject.PackingLineCollection.Content = CollectionContent.Complete;
			AssertDisallowImportTLPShipment(Core.Constants.ContainerModes.BreakBulk, dataObject);
		}

		public void TestAllowImportBreakBulkShipmentWithCorrectNumberOfPacklingLines()
		{
			var dataObject = GetUniversalShipmentWithNoPackingLines(Core.Constants.ContainerModes.BreakBulk);
			dataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{ ContainerNumber = "BATCH1", GoodsDescription = "junk" } });
			dataObject.PackingLineCollection.Content = CollectionContent.Complete;
			AssertAllowImportTLPShipment(Core.Constants.ContainerModes.BreakBulk, dataObject);
		}

		public void TestReadIntoBusinessObject_CalculatedGrossVolumeIsToBig_ReplaceWithInvalidValue()
		{
			var containerDataObject = ContainerDataObjectTestHelper.SetupContainerWithVGM();
			containerDataObject.ContainerCount = 1;
			containerDataObject.FCL_LCL_AIR = new ContainerMode()
			{ Code = "ROR" };
			containerDataObject.TotalLength = 1000;
			containerDataObject.TotalWidth = 1000;
			containerDataObject.TotalHeight = 1000;
			containerDataObject.LengthUnit = new UnitOfLength()
			{ Code = "M" };
			var readerToTest = new ContainerDataObjectReader<AgencyShipmentContainer>(containerDataObject, new TestErrorLogger(), new UniversalObjectFactory(), (c) => null);
			var readContainer = readerToTest.ReadIntoBusinessObject();
			AssertNotNull(readContainer);
			AssertEquals("The value is fixed", 999999m, readContainer.JC_GrossVolume);
		}

		void AssertDisallowImportTLPShipment(string packingModeOnBizObject, UniversalShipment dataObject)
		{
			var sailing = UniversalTestHelper.CreateSailingWithVoyage(Factory.BOFactory, "AUSYD", "NZAKL", "GENERAL FRANCO", "001");
			var booking = Factory.New<AgencyBooking>();
			booking.JS_PackingMode = packingModeOnBizObject;
			booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			booking.JS_HouseBill = "S0001";
			booking.JS_JX = sailing.PK;
			Factory.SaveForTesting();
			var reader = GetReader(dataObject);
			BusinessObject container = null;
			reader.ReadIntoBusinessObject(ref container);
			AssertNull("container hasn't been imported", container);
			AssertMultilineASCIIEquals("contains error message", @"Error - Cannot populate AgencyShipmentContainer because:
Top Level Pack Bookings and Bill of Ladings must contain exactly one Packing Line", Logger.Logs);
		}

		void AssertAllowImportTLPShipment(string packingModeOnBizObject, UniversalShipment dataObject)
		{
			var sailing = UniversalTestHelper.CreateSailingWithVoyage(Factory.BOFactory, "AUSYD", "NZAKL", "GENERAL FRANCO", "001");
			var booking = Factory.New<AgencyBooking>();
			booking.JS_PackingMode = packingModeOnBizObject;
			booking.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			booking.JS_HouseBill = "S0001";
			booking.JS_JX = sailing.PK;
			Factory.SaveForTesting();
			var reader = GetReader(dataObject);
			BusinessObject container = null;
			reader.ReadIntoBusinessObject(ref container);
			AssertNotNull("container has been imported", container);
			AssertEquals("container should be not deleted", false, container.IsDeleted);
		}

		UniversalShipment GetUniversalShipmentWithNoPackingLines(string packingMode)
		{
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.WayBillNumber = "S0001";
			dataObject.BookingConfirmationReference = "BKGREF001";
			if (!string.IsNullOrWhiteSpace(packingMode))
			{
				dataObject.ContainerMode = new ContainerMode { Code = packingMode };
			}

			dataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg> { new TransportLeg { PortOfLoading = new UNLOCO { Code = "AUSYD" }, PortOfDischarge = new UNLOCO { Code = "NZAKL" }, TransportMode = TransportMode.Sea, VesselName = "GENERAL FRANCO", VoyageFlightNo = "001" } });
			dataObject.SetContainerCollection(() => new DataObjectList<Container> { new Container { ContainerNumber = "AAA", ContainerType = new ContainerType { Code = "20GP", ISOCode = "22G0" } } });
			return dataObject;
		}

		#endregion
		#region Implementation
		protected ITopLevelDataObjectReader GetReader(UniversalShipment dataObject)
		{
			return new AgencyShipmentContainerDataObjectReader(dataObject, Logger, Factory);
		}

		TestErrorLogger Logger
		{
			get
			{
				return logger ?? (logger = new TestErrorLogger());
			}
		}

		TestErrorLogger logger;
		#endregion
	}
}
