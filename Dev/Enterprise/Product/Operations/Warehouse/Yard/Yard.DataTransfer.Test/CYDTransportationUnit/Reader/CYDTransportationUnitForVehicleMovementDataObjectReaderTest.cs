using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal.Test
{
	[TestedType(typeof(CYDTransportationUnitDataObjectReader))]
	public class CYDTransportationUnitForVehicleMovementDataObjectReaderTest : TestCaseWithUniversalObjectFactory
	{
		public void TestReadFromDataObject()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsDataSourceGateVehicleMovement = true, IsIncoming = true });
			var logger = GetLogger(shipment);
			var now = new ZDateTime(2024, 7, 1, 00, 00, 00);

			var location = Factory.Load<WhsLocation>(new ZQuery()).FirstOrDefault();
			var gateInTime = new ZDateTimeOffset(shipment.DateCollection?.FirstOrDefault(date => date.Type == DateType.Start)?.Value ?? new UXmlDateTime(now));
			shipment.RelatedShipmentCollection.FirstOrDefault().WarehouseLocation = location.ToLocationString();

			new CYDTransportationUnitForVehicleMovementDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var transportationUnit = Factory.Load<CYDTransportationUnit>(new ZQuery()).Single();
			Assert("Transportation reference should be populated.", transportationUnit.YTU_TransportationReference.Equals("REG1"));
			Assert("Gate in time should be populated", transportationUnit.YTU_GateInTime.Equals(gateInTime));

			var jobDocAddress = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress)).Single();
			AssertEquals("WUFU SHIPPING LINE", jobDocAddress.CompanyName);

			var universalJobLink = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_SourceType, DataContextType.GateVehicleMovement)).Single();
			AssertEquals("GateVehicleMovement", universalJobLink.UCL_SourceType);
			AssertEquals("GV00001234", universalJobLink.UCL_SourceKey);
		}

		public void TestReadFromDataObject_VehicleEntryTimeIsZDateTime()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsDataSourceGateVehicleMovement = true, IsIncoming = true });
			var logger = GetLogger(shipment);
			var now = new ZDateTime(2024, 7, 1, 00, 00, 00);

			var location = Factory.Load<WhsLocation>(new ZQuery()).FirstOrDefault();
			shipment.RelatedShipmentCollection.FirstOrDefault().WarehouseLocation = location.ToLocationString();

			new CYDTransportationUnitForVehicleMovementDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var transportationUnit = Factory.Load<CYDTransportationUnit>(new ZQuery()).Single();
			Assert("Transportation reference should be populated.", transportationUnit.YTU_TransportationReference.Equals("REG1"));

			var yardRow = YardMatchingHelper.GetYard(shipment, Factory, logger);
			var yard = Factory.BOFactory.Load<WhsWarehouse>(yardRow.GetValue(WhsWarehouseSchema.PK));
			var gateInTime = yard.GetWarehouseBranchLocalDateTimeOffset(now);
			Assert("Gate in time should be populated", transportationUnit.YTU_GateInTime.Equals(gateInTime));

			var jobDocAddress = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress)).Single();
			AssertEquals("WUFU SHIPPING LINE", jobDocAddress.CompanyName);

			var universalJobLink = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_SourceType, DataContextType.GateVehicleMovement)).Single();
			AssertEquals("GateVehicleMovement", universalJobLink.UCL_SourceType);
			AssertEquals("GV00001234", universalJobLink.UCL_SourceKey);
		}

		public void TestReadFromDataObject_VehicleEntryTimeIsZDateTimeOffset()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsDataSourceGateVehicleMovement = true, IsIncoming = true });
			var logger = GetLogger(shipment);
			var now = new ZDateTimeOffset(2024, 7, 1, 00, 00, 00);

			var location = Factory.Load<WhsLocation>(new ZQuery()).FirstOrDefault();
			shipment.RelatedShipmentCollection.FirstOrDefault().WarehouseLocation = location.ToLocationString();

			new CYDTransportationUnitForVehicleMovementDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var transportationUnit = Factory.Load<CYDTransportationUnit>(new ZQuery()).Single();
			Assert("Transportation reference should be populated.", transportationUnit.YTU_TransportationReference.Equals("REG1"));

			var yardRow = YardMatchingHelper.GetYard(shipment, Factory, logger);
			var yard = Factory.BOFactory.Load<WhsWarehouse>(yardRow.GetValue(WhsWarehouseSchema.PK));
			var gateInTime = yard.GetWarehouseBranchDateTimeOffset(now.ToUtcDateTime());
			Assert("Gate in time should be populated", transportationUnit.YTU_GateInTime.Equals(gateInTime));

			var jobDocAddress = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress)).Single();
			AssertEquals("WUFU SHIPPING LINE", jobDocAddress.CompanyName);

			var universalJobLink = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_SourceType, DataContextType.GateVehicleMovement)).Single();
			AssertEquals("GateVehicleMovement", universalJobLink.UCL_SourceType);
			AssertEquals("GV00001234", universalJobLink.UCL_SourceKey);
		}

		public void TestReadFromDataObject_SaveGateInTimeWithoutSeconds()
		{
			var vehicleEntryTime = new ZDateTimeOffset(new ZDateTime(2024, 7, 1, 2, 3, 4), DateTimeKind.Local);
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsDataSourceGateVehicleMovement = true, IsIncoming = true, GateInTime = vehicleEntryTime });
			var logger = GetLogger(shipment);

			var location = Factory.Load<WhsLocation>(new ZQuery()).FirstOrDefault();
			shipment.RelatedShipmentCollection.FirstOrDefault().WarehouseLocation = location.ToLocationString();

			new CYDTransportationUnitForVehicleMovementDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var transportationUnit = Factory.Load<CYDTransportationUnit>(new ZQuery()).Single();
			Assert("Transportation reference should be populated.", transportationUnit.YTU_TransportationReference.Equals("REG1"));

			var yardRow = YardMatchingHelper.GetYard(shipment, Factory, logger);
			var yard = Factory.BOFactory.Load<WhsWarehouse>(yardRow.GetValue(WhsWarehouseSchema.PK));
			var gateInTime = yard.GetWarehouseBranchLocalDateTimeOffset(vehicleEntryTime.ToZDateTime()).AddSeconds(-4);
			AssertEquals("Gate in time should be populated", gateInTime, transportationUnit.YTU_GateInTime);

			var jobDocAddress = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress)).Single();
			AssertEquals("WUFU SHIPPING LINE", jobDocAddress.CompanyName);

			var universalJobLink = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_SourceType, DataContextType.GateVehicleMovement)).Single();
			AssertEquals("GateVehicleMovement", universalJobLink.UCL_SourceType);
			AssertEquals("GV00001234", universalJobLink.UCL_SourceKey);
		}

		public void TestReadFromDataObject_SaveGateInTimeWithCorrectOffset()
		{
			TestCase(6);
			TestCase(8);
			TestCase(10);

			void TestCase(int offset)
			{
				var vehicleEntryTime = new ZDateTimeOffset(new ZDateTime(2024, 7, 1, 2, 3, 0), new TimeSpan(offset, 0, 0));
				var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsDataSourceGateVehicleMovement = true, IsIncoming = true, GateInTime = vehicleEntryTime });
				var logger = GetLogger(shipment);

				var location = Factory.Load<WhsLocation>(new ZQuery()).FirstOrDefault();
				shipment.RelatedShipmentCollection.FirstOrDefault().WarehouseLocation = location.ToLocationString();

				new CYDTransportationUnitForVehicleMovementDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				var transportationUnit = Factory.Load<CYDTransportationUnit>(new ZQuery()).Single();
				Assert("Transportation reference should be populated.", transportationUnit.YTU_TransportationReference.Equals("REG1"));

				AssertEquals("Gate in time should have correct offset", vehicleEntryTime, transportationUnit.YTU_GateInTime);

				var jobDocAddress = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress)).Single();
				AssertEquals("WUFU SHIPPING LINE", jobDocAddress.CompanyName);

				var universalJobLink = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_SourceType, DataContextType.GateVehicleMovement)).Single();
				AssertEquals("GateVehicleMovement", universalJobLink.UCL_SourceType);
				AssertEquals("GV00001234", universalJobLink.UCL_SourceKey);
			}
		}

		public void TestReadFromDataObject_GetWaitingBayLocation()
		{
			var vehicleEntryTime = new ZDateTimeOffset(new DateTime(2024, 7, 1, 2, 3, 4), DateTimeKind.Local);
			var location = Factory.Load<WhsLocation>(new ZQuery(WhsLocationViewSchema.WLV_LocationString, "A")).Single();

			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsDataSourceGateVehicleMovement = true, IsIncoming = true, GateInTime = vehicleEntryTime, WarehouseLocation = location.ToLocationString() });
			var logger = GetLogger(shipment);
			var tpu = new CYDTransportationUnitForVehicleMovementDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			AssertEquals(location.PK, tpu.WaitingBayLocation.PK);

			var shipmentWithoutWarehouseLocation = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsDataSourceGateVehicleMovement = true, IsIncoming = true, GateInTime = vehicleEntryTime, WarehouseLocation = ZString.Empty });
			AssertExceptionThrown<DataObjectReadFailureException>("Error reading warehouse location", "Yard location is missing from UXML.", () => new CYDTransportationUnitForVehicleMovementDataObjectReader(shipmentWithoutWarehouseLocation, logger, Factory).ReadIntoBusinessObject());
		}

		public void TestReadFromDataObject_DeliverContainer_WithAcceptanceNumber()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsDataSourceGateVehicleMovement = true, IsIncoming = true, IsForPickup = false, BookingConfirmationReference = "BKR01" });
			var logger = GetLogger(shipment);
			var now = new ZDateTimeOffset(2024, 7, 1, 00, 00, 00);

			var location = Factory.Load<WhsLocation>(new ZQuery()).FirstOrDefault();
			shipment.RelatedShipmentCollection.FirstOrDefault().WarehouseLocation = location.ToLocationString();
			var gateInTime = new ZDateTimeOffset(shipment.DateCollection?.FirstOrDefault(date => date.Type == DateType.Start)?.Value ?? new UXmlDateTime(now));

			var reader = new CYDTransportationUnitForVehicleMovementDataObjectReader(shipment, logger, Factory);
			reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var transportationUnit = Factory.Load<CYDTransportationUnit>(new ZQuery()).Single();
			AssertEquals("Transportation reference should be populated.", "REG1", transportationUnit.YTU_TransportationReference);
			AssertEquals("Transportation Unit ID should be populated", "TPU00000001", transportationUnit.YTU_TransportationUnitID);
			AssertEquals("Gate in time should be populated", gateInTime, transportationUnit.YTU_GateInTime);

			var jobDocAddress = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress)).Single();
			AssertEquals("WUFU SHIPPING LINE", jobDocAddress.CompanyName);

			var universalJobLink = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_SourceType, DataContextType.GateVehicleMovement)).Single();
			AssertEquals("GateVehicleMovement", universalJobLink.UCL_SourceType);
			AssertEquals("GV00001234", universalJobLink.UCL_SourceKey);

			var deliveryHeader = Factory.Load<CYDDeliveryHeader>(new ZQuery()).Single();
			AssertEquals("Delivery Header Job Number should be populated.", "DI00000001", deliveryHeader.YDH_JobNumber);

			var delivery = Factory.Load<CYDDelivery>(new ZQuery()).Single();
			AssertEquals("Delivery Transport Reference should be populated", "TREF240419151008", delivery.YDL_TransportReference);

			reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			transportationUnit = Factory.Load<CYDTransportationUnit>(new ZQuery()).Single();
			AssertEquals("Transportation reference should be populated.", "REG1", transportationUnit.YTU_TransportationReference);
			AssertEquals("Transportation Unit ID should be populated", "TPU00000001", transportationUnit.YTU_TransportationUnitID);

			jobDocAddress = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress)).Single();
			AssertEquals("WUFU SHIPPING LINE", jobDocAddress.CompanyName);

			universalJobLink = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_SourceType, DataContextType.GateVehicleMovement)).Single();
			AssertEquals("GateVehicleMovement", universalJobLink.UCL_SourceType);
			AssertEquals("GV00001234", universalJobLink.UCL_SourceKey);

			deliveryHeader = Factory.Load<CYDDeliveryHeader>(new ZQuery()).Single();
			AssertEquals("Delivery Header Job Number should be populated.", "DI00000001", deliveryHeader.YDH_JobNumber);

			delivery = Factory.Load<CYDDelivery>(new ZQuery()).Single();
			AssertEquals("Delivery Transport Reference should be populated", "TREF240419151008", delivery.YDL_TransportReference);
		}

		public void TestReadFromDataObject_DeliverContainer_WithoutAcceptanceNumber()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsDataSourceGateVehicleMovement = true, IsIncoming = true, IsForPickup = false, BookingConfirmationReference = "" });
			var logger = GetLogger(shipment);

			var location = Factory.Load<WhsLocation>(new ZQuery()).FirstOrDefault();
			shipment.RelatedShipmentCollection.FirstOrDefault().WarehouseLocation = location.ToLocationString();

			AssertExceptionThrown<DataObjectReadFailureException>(
				"Missing booking reference in Gate Movement",
				"Booking Confirmation Reference is required in UXML for delivery.",
				() => new CYDTransportationUnitForVehicleMovementDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject());
		}

		public void TestReadFromDataObject_DeliverContainer_GateInForTPUWithoutPRA()
		{
			var yardUnit = Factory.LoadTop1<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, "GENL"));
			yardUnit.YUS_YRL_ReceiveLine = ZGuid.Empty;
			yardUnit.YUS_YEL_ReleaseLine = ZGuid.Empty;
			Factory.SaveForTesting();

			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsDataSourceGateVehicleMovement = true, IsIncoming = true, IsForPickup = false, BookingConfirmationReference = "BKR01" });
			var logger = GetLogger(shipment);

			var location = Factory.Load<WhsLocation>(new ZQuery()).FirstOrDefault();
			shipment.RelatedShipmentCollection.FirstOrDefault().WarehouseLocation = location.ToLocationString();

			AssertExceptionThrown<DataObjectReadFailureException>(
				"TPU Gate In when PRA is not available.",
				"TPU Gate In when PRA is not available.",
				() =>
				{
					var reader = new CYDTransportationUnitForVehicleMovementDataObjectReader(shipment, logger, Factory);
					reader.ReadIntoBusinessObject();
					Factory.SaveForTesting();
				});
		}

		public void TestReadFromDataObject_PickupContainer()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsForPickup = true , IsDataSourceGateVehicleMovement = true, IsIncoming = true });
			var logger = GetLogger(shipment);
			var now =  new ZDateTimeOffset(2024, 7, 1, 00, 00, 00);

			var location = Factory.Load<WhsLocation>(new ZQuery()).FirstOrDefault();
			shipment.RelatedShipmentCollection.FirstOrDefault().WarehouseLocation = location.ToLocationString();
			var gateInTime = new ZDateTimeOffset(shipment.DateCollection?.FirstOrDefault(date => date.Type == DateType.Start)?.Value ?? new UXmlDateTime(now));

			var reader = new CYDTransportationUnitForVehicleMovementDataObjectReader(shipment, logger, Factory);
			reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var transportationUnit = Factory.Load<CYDTransportationUnit>(new ZQuery()).Single();
			var transportationUnitPK = transportationUnit.PK; 	AssertEquals("Transportation reference should be populated.", "REG1", transportationUnit.YTU_TransportationReference);
			AssertEquals("Transportation Unit ID should be populated", "TPU00000001", transportationUnit.YTU_TransportationUnitID);
			Assert("Gate in time should be populated", transportationUnit.YTU_GateInTime.Equals(gateInTime));

			var jobDocAddress = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress)).Single();
			var jobDocAddressPK = jobDocAddress.PK;
			AssertEquals("WUFU SHIPPING LINE", jobDocAddress.CompanyName);

			var universalJobLink = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_SourceType, DataContextType.GateVehicleMovement)).Single();
			var universalJobLinkPK = universalJobLink.PK;
			AssertEquals("GateVehicleMovement", universalJobLink.UCL_SourceType);
			AssertEquals("GV00001234", universalJobLink.UCL_SourceKey);

			var pickupHeader = Factory.Load<CYDPickupHeader>(new ZQuery()).Single();
			var pickupHeaderPK = pickupHeader.PK;
			AssertEquals("Pickup Header Job Number should be populated.", "PI00000001", pickupHeader.YPH_JobNumber);

			var pickup = Factory.Load<CYDPickup>(new ZQuery()).Single();
			var pickupPK = pickup.PK;
			AssertEquals("Pickup Transport Reference should be populated", "TREF240419151008", pickup.YPL_TransportReference);
			AssertEquals("Pickup quantity should be 1", (short)1, pickup.UnitLineItem.YLI_Quantity);
			AssertEquals("Pickup type should be populated", "CNT", pickup.UnitLineItem.YLI_Type);

			var yardUnit = Factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_YPL_Pickup, pickupPK)).Single();
			AssertEquals("Pickup unit number should be matched", "GENL", yardUnit.YUS_UnitID);

			reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var transportationUnit2 = Factory.Load<CYDTransportationUnit>(new ZQuery()).Single();
			AssertEquals("TransportationUnit primary key should be unchanged", transportationUnitPK, transportationUnit2.PK);
			AssertEquals("Transportation reference should be unchanged.", "REG1", transportationUnit2.YTU_TransportationReference);
			AssertEquals("Transportation Unit ID should be unchanged", "TPU00000001", transportationUnit2.YTU_TransportationUnitID);

			var jobDocAddress2 = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress)).Single();
			AssertEquals("JobDocAddress primary key should be unchanged", jobDocAddressPK, jobDocAddress2.PK);
			AssertEquals("WUFU SHIPPING LINE", jobDocAddress2.CompanyName);

			var universalJobLink2 = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_SourceType, DataContextType.GateVehicleMovement)).Single();
			AssertEquals("UniversalJobLink primary key should be unchanged", universalJobLinkPK, universalJobLink2.PK);
			AssertEquals("GateVehicleMovement", universalJobLink.UCL_SourceType);
			AssertEquals("GV00001234", universalJobLink.UCL_SourceKey);

			var pickupHeader2 = Factory.Load<CYDPickupHeader>(new ZQuery()).Single();
			AssertEquals("PickupHeader primary key should be unchanged", pickupHeaderPK, pickupHeader2.PK);
			AssertEquals("Pickup Header Job Number should be unchanged.", "PI00000001", pickupHeader2.YPH_JobNumber);

			var pickup2 = Factory.Load<CYDPickup>(new ZQuery()).Single();
			AssertEquals("Pickup primary key should be unchanged", pickupPK, pickup2.PK);
			AssertEquals("Pickup Transport Reference should be unchanged", "TREF240419151008", pickup2.YPL_TransportReference);
			AssertEquals("Pickup quantity should be unchanged", (short)1, pickup2.UnitLineItem.YLI_Quantity);
			AssertEquals("Pickup type should be unchanged", "CNT", pickup2.UnitLineItem.YLI_Type);

			var yardUnit2 = Factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_YPL_Pickup, pickupPK)).Single();
			AssertEquals("Pickup unit number should be matched", "GENL", yardUnit2.YUS_UnitID);
		}

		public void TestReadFromDataObject_PickupContainer_WithoutContainerNumber()
		{
			var yardUnitStates = Factory.Load<CYDYardUnitState>(new ZQuery());
			yardUnitStates.DeleteAll();

			var releaseLines = Factory.Load<CYDReleaseAdviceLine>(new ZQuery());
			releaseLines.ToList().ForEach(releaseLine => releaseLine.UnitLineItem.YLI_IsPreAdvice = false);

			// Pickup 1 containers
			{
				var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsDataSourceGateVehicleMovement = true, IsIncoming = true, IsForPickup = true, ContainerNumbers = [null] });
				var logger = GetLogger(shipment);
				var now = new ZDateTimeOffset(2024, 7, 1, 00, 00, 00);

				var location = Factory.Load<WhsLocation>(new ZQuery()).First();
				shipment.RelatedShipmentCollection.First().WarehouseLocation = location.ToLocationString();
				var gateInTime = new ZDateTimeOffset(shipment.DateCollection?.FirstOrDefault(date => date.Type == DateType.Start)?.Value ?? new UXmlDateTime(now));

				var reader = new CYDTransportationUnitForVehicleMovementDataObjectReader(shipment, logger, Factory);
				reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();

				var transportationUnit = Factory.Load<CYDTransportationUnit>(new ZQuery()).Single();
				AssertEquals("Transportation reference should be populated.", "REG1", transportationUnit.YTU_TransportationReference);
				AssertEquals("Transportation Unit ID should be populated", "TPU00000001", transportationUnit.YTU_TransportationUnitID);
				Assert("Gate in time should be populated", transportationUnit.YTU_GateInTime.Equals(gateInTime));

				var jobDocAddress = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress)).Single();
				AssertEquals("WUFU SHIPPING LINE", jobDocAddress.CompanyName);

				var universalJobLink = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_SourceType, DataContextType.GateVehicleMovement)).Single();
				AssertEquals("GateVehicleMovement", universalJobLink.UCL_SourceType);
				AssertEquals("GV00001234", universalJobLink.UCL_SourceKey);

				var pickupHeader = Factory.Load<CYDPickupHeader>(new ZQuery()).Single();
				AssertEquals("Pickup Header Job Number should be populated.", "PI00000001", pickupHeader.YPH_JobNumber);

				var pickups = Factory.Load<CYDPickup>(new ZQuery());
				AssertEquals("1 Pickup should be populated.", 1, pickups.Length);
				AssertEquals("Pickup Transport Reference should be populated", "TREF240419151008", pickups[0].YPL_TransportReference);
				AssertEquals("Pickup quantity should be 1", (short)1, pickups[0].UnitLineItem.YLI_Quantity);
				AssertEquals("Pickup type should be populated", "CNT", pickups[0].UnitLineItem.YLI_Type);
				AssertEquals("20GP", Factory.Load<RefContainer>(pickups[0].UnitLineItem.YLI_RC_ContainerType).RC_Code);
			}

			// Pickup another container
			{
				var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsDataSourceGateVehicleMovement = true, IsIncoming = true, IsForPickup = true, ContainerNumbers = [string.Empty], DataSourceKeys = ["GBM00005678"] });
				var logger = GetLogger(shipment);

				var location = Factory.Load<WhsLocation>(new ZQuery()).First();
				shipment.RelatedShipmentCollection.First().WarehouseLocation = location.ToLocationString();

				var reader = new CYDTransportationUnitForVehicleMovementDataObjectReader(shipment, logger, Factory);
				reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();

				var transportationUnitAfterReImport = Factory.Load<CYDTransportationUnit>(new ZQuery()).Single();
				AssertEquals("Transportation reference should be unchanged.", "REG1", transportationUnitAfterReImport.YTU_TransportationReference);
				AssertEquals("Transportation Unit ID should be unchanged", "TPU00000001", transportationUnitAfterReImport.YTU_TransportationUnitID);

				var jobDocAddress2 = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress)).Single();
				AssertEquals("WUFU SHIPPING LINE", transportationUnitAfterReImport.TransportCompanyDocAddress.CompanyName);

				var universalJobLink2AfterReImport = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_SourceType, DataContextType.GateVehicleMovement)).Single();
				AssertEquals("GateVehicleMovement", universalJobLink2AfterReImport.UCL_SourceType);
				AssertEquals("GV00001234", universalJobLink2AfterReImport.UCL_SourceKey);

				var pickupHeadersAfterReImport = Factory.Load<CYDPickupHeader>(new ZQuery());
				AssertEquals("2 Pickup Header should be populated.", 2, pickupHeadersAfterReImport.Length);

				var pickupsAfterReImport = Factory.Load<CYDPickup>(new ZQuery());
				AssertEquals("2 Pickups should be populated.", 2, pickupsAfterReImport.Length);
				AssertEquals("Pickup Transport Reference should be unchanged", "TREF240419151008", pickupsAfterReImport[0].YPL_TransportReference);
				AssertEquals("Pickup Transport Reference should be unchanged", "TREF240419151008", pickupsAfterReImport[1].YPL_TransportReference);
				AssertEquals("Pickup quantity should be unchanged", (short)1, pickupsAfterReImport[0].UnitLineItem.YLI_Quantity);
				AssertEquals("Pickup quantity should be unchanged", (short)1, pickupsAfterReImport[1].UnitLineItem.YLI_Quantity);
				AssertEquals("Pickup type should be unchanged", "CNT", pickupsAfterReImport[0].UnitLineItem.YLI_Type);
				AssertEquals("Pickup type should be unchanged", "CNT", pickupsAfterReImport[1].UnitLineItem.YLI_Type);
				AssertEquals("20GP", Factory.Load<RefContainer>(pickupsAfterReImport[0].UnitLineItem.YLI_RC_ContainerType).RC_Code);
				AssertEquals("20GP", Factory.Load<RefContainer>(pickupsAfterReImport[1].UnitLineItem.YLI_RC_ContainerType).RC_Code);
			}
		}

		public void TestReadFromDataObject_YardIsNotFound()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsDataSourceGateVehicleMovement = true, IsIncoming = true, YardIsNotFound = true });
			var logger = GetLogger(shipment);

			AssertExceptionThrown<DataObjectReadFailureException>("Data Object Read Failure", "Yard is not found.", () =>
			{
				var reader = new CYDTransportationUnitForVehicleMovementDataObjectReader(shipment, logger, Factory);
				reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();
			});
		}

		public void TestReadFromDataObject_VehicleIsNotFound()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsDataSourceGateVehicleMovement = true, IsIncoming = true, IsNeedToBuildPreCarriage = false });
			var logger = GetLogger(shipment);

			AssertExceptionThrown<DataObjectReadFailureException>("Data Object Read Failure", "Vehicle Registration Number is not found.", () =>
			{
				var reader = new CYDTransportationUnitForVehicleMovementDataObjectReader(shipment, logger, Factory);
				reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();
			});
		}

		public void TestReadFromDataObject_GetVehicleFromVehicleRun()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsDataSourceGateVehicleMovement = true, IsIncoming = true, IsNeedToBuildVehicleRun = true });
			var logger = GetLogger(shipment);
			var location = Factory.Load<WhsLocation>(new ZQuery()).FirstOrDefault();
			shipment.RelatedShipmentCollection.FirstOrDefault().WarehouseLocation = location.ToLocationString();

			AssertNotNull(shipment.VehicleRun);

			AssertNoExceptionThrown(() =>
			{
				var reader = new CYDTransportationUnitForVehicleMovementDataObjectReader(shipment, logger, Factory);
				reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();
			});
		}

		public void TestReadFromDataObject_BookingPartyIsNotFound()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsDataSourceGateVehicleMovement = true, IsIncoming = true, BookingPartyIsNotFound = true });
			var logger = GetLogger(shipment);
			var location = Factory.Load<WhsLocation>(new ZQuery()).FirstOrDefault();
			shipment.RelatedShipmentCollection.FirstOrDefault().WarehouseLocation = location.ToLocationString();

			AssertExceptionThrown<DataObjectReadFailureException>("Data Object Read Failure", "Transport Company details are missing from UXML.", () =>
			{
				var reader = new CYDTransportationUnitForVehicleMovementDataObjectReader(shipment, logger, Factory);
				reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();
			});
		}

		public void TestGetExistingBusinessObjectUsingModuleSpecificBusinessRules_LoadFromJobLinks()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsDataSourceGateVehicleMovement = true, IsIncoming = true });
			var logger = GetLogger(shipment);
			var location = Factory.Load<WhsLocation>(new ZQuery()).FirstOrDefault();
			shipment.RelatedShipmentCollection.FirstOrDefault().WarehouseLocation = location.ToLocationString();

			new CYDTransportationUnitForVehicleMovementDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var factoryForLoad = NewUniversalObjectFactory();
			var transportationUnit = factoryForLoad.Load<CYDTransportationUnit>(new ZQuery()).Single();
			Assert("Transportation reference should be populated.", transportationUnit.YTU_TransportationReference.Equals("REG1"));

			var universalJobLink = factoryForLoad.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_SourceType, DataContextType.GateVehicleMovement)).Single();
			AssertEquals(transportationUnit.PK, universalJobLink.UCL_ParentID);

			var factoryForSave = NewUniversalObjectFactory();
			new CYDTransportationUnitForVehicleMovementDataObjectReader(shipment, logger, factoryForSave).ReadIntoBusinessObject();
			factoryForSave.SaveForTesting();

			var factoryForVerify = NewUniversalObjectFactory();
			var matchedTPU = factoryForVerify.Load<CYDTransportationUnit>(new ZQuery()).Single();
			AssertEquals(transportationUnit.PK, matchedTPU.PK);
			Assert("Transportation reference should load the data from job links.", matchedTPU.YTU_TransportationReference.Equals("REG1"));

			var universalLinkInNewFactory = factoryForVerify.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_SourceType, DataContextType.GateVehicleMovement)).Single();
			AssertEquals(universalJobLink.PK, universalLinkInNewFactory.PK);
		}

		public void TestReadFromDataObject_TransportReferenceIsNotFound()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsDataSourceGateVehicleMovement = true, IsIncoming = true,TransportReferenceIsNotFound = true });
			var logger = GetLogger(shipment);
			var location = Factory.Load<WhsLocation>(new ZQuery()).FirstOrDefault();
			shipment.RelatedShipmentCollection.FirstOrDefault().WarehouseLocation = location.ToLocationString();

			var reader = new CYDTransportationUnitForVehicleMovementDataObjectReader(shipment, logger, Factory);

			reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var delivery = Factory.Load<CYDDelivery>(new ZQuery()).Single();
			AssertEquals("Delivery Transport Reference should be empty", "", delivery.YDL_TransportReference);
		}

		public void TestReadFromDataObject_ReleaseHasExpired()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsDataSourceGateVehicleMovement = true, IsIncoming = true, IsForPickup = true });
			var logger = GetLogger(shipment);
			var location = Factory.Load<WhsLocation>(new ZQuery()).FirstOrDefault();
			shipment.RelatedShipmentCollection.FirstOrDefault().WarehouseLocation = location.ToLocationString();

			var reader = new CYDTransportationUnitForVehicleMovementDataObjectReader(shipment, logger, Factory);

			var releaseAdvice = Factory.Load<CYDReleaseAdvice>(new ZQuery(CYDReleaseAdviceSchema.YRE_ReleaseNumber, "BKR01")).Single();
			releaseAdvice.YRE_FromDate = new ZDate(2024, 1, 1);
			releaseAdvice.YRE_ToDate = new ZDate(2024, 1, 31);
			Factory.SaveForTesting();

			AssertExceptionThrown<YardUnitNotFoundException>("Data Object Read Failure", "Yard Unit (GENL) is not found or Instruction Advice (BKR01) has expired.", () =>
			{
				reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();
			});
		}

		public void TestGetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsDataSourceGateVehicleMovement = true, IsIncoming = true });
			var logger = GetLogger(shipment);

			var now = new ZDateTime(2024, 7, 1, 00, 00, 00);

			var area = Factory.NewWithValidTestData<WhsArea>();
			var location = Factory.Load<WhsLocation>(new ZQuery()).FirstOrDefault();
			location.WLV_WA_PickingArea = area.PK;
			location.WLV_WA_PutawayArea = area.PK;

			shipment.RelatedShipmentCollection.FirstOrDefault().WarehouseLocation = location.ToLocationString();

			var yard = YardMatchingHelper.GetYard(shipment, Factory, logger);
			var reader = new CYDTransportationUnitForVehicleMovementDataObjectReader(shipment, logger, Factory);
			AssertNull(reader.TryGetExistingBusinessObject());

			var gateInTime = new ZDateTimeOffset(shipment.DateCollection?.FirstOrDefault(date => date.Type == DateType.Start)?.Value ?? new UXmlDateTime(now));
			var tpu = CreateCYDTransportationUnit("REG1", new DateTime(2024, 3, 1), gateInTime, new DateTime(2024, 3, 1), location.PK, yard.GetValue(WhsWarehouseSchema.PK), shipment);
			AssertEquals(tpu, reader.TryGetExistingBusinessObject());

			var olderTPUWithMatchingVehicleRego = CreateCYDTransportationUnit("REG1", new DateTime(2024, 2, 1), gateInTime, new DateTime(2024, 2, 1), location.PK, yard.GetValue(WhsWarehouseSchema.PK), shipment);
			AssertEquals(olderTPUWithMatchingVehicleRego, reader.TryGetExistingBusinessObject());

			var newerTPUWithMatchingVehicleRego = CreateCYDTransportationUnit("REG1", new DateTime(2024, 4, 1), gateInTime, new DateTime(2024, 4, 1), location.PK, yard.GetValue(WhsWarehouseSchema.PK), shipment);
			AssertEquals(olderTPUWithMatchingVehicleRego, reader.TryGetExistingBusinessObject());

			var tpuWithDifferentRego = CreateCYDTransportationUnit("REG2", new DateTime(2024, 1, 1), gateInTime, new DateTime(2024, 1, 1), location.PK, yard.GetValue(WhsWarehouseSchema.PK), shipment);
			AssertEquals(olderTPUWithMatchingVehicleRego, reader.TryGetExistingBusinessObject());

			var gateInTPUWithMatchingRego = CreateCYDTransportationUnit("REG1", new DateTime(2024, 1, 1), gateInTime, new DateTime(2024, 1, 1), location.PK, yard.GetValue(WhsWarehouseSchema.PK), shipment);
			AssertEquals(gateInTPUWithMatchingRego, reader.TryGetExistingBusinessObject());
		}

		public void TestReadFromDataObject_GivenNoMatchingExistingTransportationUnit_ThenCreateTransportationUnit()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsDataSourceGateVehicleMovement = true, IsIncoming  = true });
			var logger = GetLogger(shipment);

			var location = Factory.Load<WhsLocation>(new ZQuery()).FirstOrDefault();

			shipment.RelatedShipmentCollection.FirstOrDefault().WarehouseLocation = location.ToLocationString();

			new CYDTransportationUnitForVehicleMovementDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var transportationUnit = Factory.Load<CYDTransportationUnit>(new ZQuery());

			AssertEquals("Expected a new transportation unit row to be created", 1, transportationUnit.Length);
		}

		public void TestReadFromDataObject_GivenMatchingExistingTransportationUnit_ThenUpdateTransportationUnit_PickUp()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsDataSourceGateVehicleMovement = true, IsIncoming = true, IsForPickup = true });
			var logger = GetLogger(shipment);
			var now = new ZDateTime(2024, 7, 1, 00, 00, 00);

			var location = Factory.Load<WhsLocation>(new ZQuery()).FirstOrDefault();

			shipment.RelatedShipmentCollection.FirstOrDefault().WarehouseLocation = location.ToLocationString();
			shipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new Container() { ContainerNumber = "CON1", ContainerType = new ContainerType() { Code = "LD-1" }, IsEmptyContainer = true }
			});
			shipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new Container() { ContainerNumber = "CON1", ContainerType = new ContainerType() { Code = "LD-2" }, IsEmptyContainer = true }
			});

			var yard = YardMatchingHelper.GetYard(shipment, Factory, logger);
			var gateInTime = new ZDateTimeOffset(shipment.DateCollection?.FirstOrDefault(date => date.Type == DateType.Start)?.Value ?? new UXmlDateTime(now));

			var tpu = CreateCYDTransportationUnit("REG1", new DateTime(2024, 7, 1), gateInTime, new DateTime(2024, 7, 1), location.PK, yard.GetValue(WhsWarehouseSchema.PK), shipment);

			var existingTransportationUnit = Factory.Load<CYDTransportationUnit>(new ZQuery());
			var reader = new CYDTransportationUnitForVehicleMovementDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			AssertEquals("Precondition: Expected there to be one transportation unit row", 1, existingTransportationUnit.Length);

			Factory.SaveForTesting();

			AssertEquals("Transportation unit exist", tpu.YTU_GateInTime, reader.YTU_GateInTime);

			var pickups = Factory.Load<CYDPickup>(new ZQuery());
			AssertEquals("1 Pickup should be populated.", 1, pickups.Length);
			AssertEquals("Expect only one Pickup row is created", 1, reader.Pickups.Count);
		}

		public void TestReadFromDataObject_GivenMatchingExistingTransportationUnit_ThenUpdateTransportationUnit_Delivery()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsDataSourceGateVehicleMovement = true, IsIncoming = true, IsForPickup = false });
			var logger = GetLogger(shipment);
			var now = new ZDateTime(2024, 7, 1, 00, 00, 00);

			var location = Factory.Load<WhsLocation>(new ZQuery()).FirstOrDefault();

			shipment.RelatedShipmentCollection.FirstOrDefault().WarehouseLocation = location.ToLocationString();
			shipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new Container() { ContainerNumber = "CON1", ContainerType = new ContainerType() { Code = "LD-1" }, IsEmptyContainer = true }
			});
			shipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new Container() { ContainerNumber = "CON1", ContainerType = new ContainerType() { Code = "LD-2" }, IsEmptyContainer = true }
			});

			var yard = YardMatchingHelper.GetYard(shipment, Factory, logger);
			var gateInTime = new ZDateTimeOffset(shipment.DateCollection?.FirstOrDefault(date => date.Type == DateType.Start)?.Value ?? new UXmlDateTime(now));

			var tpu = CreateCYDTransportationUnit("REG1", new DateTime(2024, 7, 1), gateInTime, new DateTime(2024, 7, 1), location.PK, yard.GetValue(WhsWarehouseSchema.PK), shipment);

			var existingTransportationUnit = Factory.Load<CYDTransportationUnit>(new ZQuery());
			var reader = new CYDTransportationUnitForVehicleMovementDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			AssertEquals("Precondition: Expected there to be one transportation unit row", 1, existingTransportationUnit.Length);

			Factory.SaveForTesting();

			AssertEquals("Transportation unit exist", tpu.YTU_GateInTime, reader.YTU_GateInTime);

			var deliveries = Factory.Load<CYDDelivery>(new ZQuery());
			AssertEquals("1 Delivery should be populated.", 1, deliveries.Length);
			AssertEquals("Expect only one Delivery row is created", 1, reader.Deliveries.Count);
		}

		public void TestReadFromDataObject_GivenMatchingMultipleExistingTransportationUnit_ThenUpdateEarliestTransportationUnit()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsDataSourceGateVehicleMovement = true, IsIncoming = true, IsForPickup = true });
			var logger = GetLogger(shipment);
			var now = new ZDateTime(2024, 7, 1, 00, 00, 00);

			var location = Factory.Load<WhsLocation>(new ZQuery()).FirstOrDefault();

			shipment.RelatedShipmentCollection.FirstOrDefault().WarehouseLocation = location.ToLocationString();

			var yard = YardMatchingHelper.GetYard(shipment, Factory, logger);
			var gateInTime = new ZDateTimeOffset(shipment.DateCollection?.FirstOrDefault(date => date.Type == DateType.Start)?.Value ?? new UXmlDateTime(now));

			var tpuLatest = CreateCYDTransportationUnit("REG1", new DateTime(2024, 7, 2), gateInTime, new DateTime(2024, 7, 2), location.PK, yard.GetValue(WhsWarehouseSchema.PK), shipment);
			var tpuEarliest = CreateCYDTransportationUnit("REG1", new DateTime(2024, 7, 1), gateInTime, new DateTime(2024, 7, 1), location.PK, yard.GetValue(WhsWarehouseSchema.PK), shipment);

			var existingTransportationUnit = Factory.Load<CYDTransportationUnit>(new ZQuery());
			var reader = new CYDTransportationUnitForVehicleMovementDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			AssertEquals("Precondition: Expected there to be two transportation unit rows", 2, existingTransportationUnit.Length);

			Factory.SaveForTesting();

			AssertEquals("Earliest transportation unit row should be updated", tpuEarliest.YTU_SystemCreateTimeUtc, reader.YTU_SystemCreateTimeUtc);
			AssertNotEquals("Latest transportation unit row should not be updated", tpuLatest.YTU_SystemCreateTimeUtc, reader.YTU_SystemCreateTimeUtc);
		}

		public void TestReadFromDataObject_GivenUXMLWithEmptyRelatedShipmentCollection_ThenVehicleMovementIsIgnoredAndNoExceptionThrown()
		{
			var location = Factory.Load<WhsLocation>(new ZQuery(WhsLocationViewSchema.WLV_LocationString, "A")).Single();
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsDataSourceGateVehicleMovement = true, IsIncoming = false, WarehouseLocation = location.ToLocationString() });
			var logger = GetLogger(shipment);

			AssertNull(shipment.RelatedShipmentCollection);
				
			AssertNoExceptionThrown(() =>
			{
				new CYDTransportationUnitForVehicleMovementDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			});
		}

		public void TestReadFromDataObject_GivenContainersInTwoExistingTransportationUnit_ThenMergeAndUpdateTransportationUnit_Delivery()
		{
			// Booking for container 1

			var shipmentForReg1 = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { ContainerNumbers = ["GENL"], VehicleNumbers = ["REG1"] });
			var logger = GetLogger(shipmentForReg1);
			var tpuReg1 = new CYDTransportationUnitForGateBookingDataObjectReader(shipmentForReg1, logger, Factory).ReadIntoBusinessObject();

			Factory.SaveForTesting();

			var etaForReg1 = shipmentForReg1.SubShipmentCollection.Single().DateCollection.First(x => x.Type == DateType.Start).Value.GetValueOrDefault().ToZDateTimeOffset();
			AssertEquals("REG1", tpuReg1.YTU_TransportationReference);
			AssertEquals(etaForReg1, tpuReg1.YTU_EstimatedGateInTime);
			Assert(tpuReg1.YTU_GateInTime.IsEmpty);

			var deliveryForReg1 = Factory.Load<CYDDelivery>(new ZQuery()).Single();
			AssertEquals(tpuReg1.PK, deliveryForReg1.YDL_YTU_DeliveryTransportationUnit);

			var yardUnit = Factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, "GENL")).Single();
			AssertEquals(deliveryForReg1.PK, yardUnit.YUS_YDL_Delivery);
			AssertEquals(tpuReg1.PK, yardUnit.YUS_YTU_ReceiveTransportationUnit);

			var jobDocAddress = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress)).Single();
			AssertEquals("WUFU SHIPPING LINE", jobDocAddress.CompanyName);

			var universalJobLink = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_SourceType, DataContextType.GateBooking)).Single();
			AssertEquals("GateBooking", universalJobLink.UCL_SourceType);
			AssertEquals("GB00001234", universalJobLink.UCL_SourceKey);

			// Booking for container 2

			var shipmentForTPUReg2 = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { ContainerNumbers = ["GENL2"], VehicleNumbers = ["REG2"], UseUniqueSourceKey = true, DataSourceKeys = ["GM00005678"] });
			logger = GetLogger(shipmentForTPUReg2);
			var tpuReg2 = new CYDTransportationUnitForGateBookingDataObjectReader(shipmentForTPUReg2, logger, Factory).ReadIntoBusinessObject();

			Factory.SaveForTesting();

			var etaForReg2 = shipmentForTPUReg2.SubShipmentCollection.Single().DateCollection.First(x => x.Type == DateType.Start).Value.GetValueOrDefault().ToZDateTimeOffset();
			AssertEquals("REG2", tpuReg2.YTU_TransportationReference);
			AssertEquals(etaForReg2, tpuReg2.YTU_EstimatedGateInTime);
			Assert(tpuReg2.YTU_GateInTime.IsEmpty);

			var deliveries = Factory.Load<CYDDelivery>(new ZQuery());
			AssertEquals(2, deliveries.Length);
			var deliveryForReg2 = deliveries.First(d => d.PK != deliveryForReg1.PK);
			AssertEquals(tpuReg2.PK, deliveryForReg2.YDL_YTU_DeliveryTransportationUnit);

			var yardUnitInReg2 = Factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, "GENL2")).Single();
			AssertEquals(deliveryForReg2.PK, yardUnitInReg2.YUS_YDL_Delivery);
			AssertEquals(tpuReg2.PK, yardUnitInReg2.YUS_YTU_ReceiveTransportationUnit);

			AssertEquals(2, Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress)).Length);
			AssertEquals(2, Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_SourceType, DataContextType.GateMovementBooking)).Length);
		}

		public void TestReadFromDataObject_GivenContainersInTwoExistingTransportationUnit_AndBothTPUsHaveGateInTime_ThenMergeAndUpdateTransportationUnit_Delivery()
		{
			// Booking for container 1

			var shipmentForReg1 = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { ContainerNumbers = ["GENL"], VehicleNumbers = ["REG1"] });
			var logger = GetLogger(shipmentForReg1);
			var tpuReg1 = new CYDTransportationUnitForGateBookingDataObjectReader(shipmentForReg1, logger, Factory).ReadIntoBusinessObject();

			Factory.SaveForTesting();

			var etaForReg1 = shipmentForReg1.SubShipmentCollection.Single().DateCollection.First(x => x.Type == DateType.Start).Value.GetValueOrDefault().ToZDateTimeOffset();
			AssertEquals("REG1", tpuReg1.YTU_TransportationReference);
			AssertEquals(etaForReg1, tpuReg1.YTU_EstimatedGateInTime);
			Assert(tpuReg1.YTU_GateInTime.IsEmpty);

			var deliveryForReg1 = Factory.Load<CYDDelivery>(new ZQuery()).Single();
			AssertEquals(tpuReg1.PK, deliveryForReg1.YDL_YTU_DeliveryTransportationUnit);

			var yardUnit = Factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, "GENL")).Single();
			AssertEquals(deliveryForReg1.PK, yardUnit.YUS_YDL_Delivery);
			AssertEquals(tpuReg1.PK, yardUnit.YUS_YTU_ReceiveTransportationUnit);

			var jobDocAddress = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress)).Single();
			AssertEquals("WUFU SHIPPING LINE", jobDocAddress.CompanyName);

			var universalJobLink = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_SourceType, DataContextType.GateBooking)).Single();
			AssertEquals("GateBooking", universalJobLink.UCL_SourceType);
			AssertEquals("GB00001234", universalJobLink.UCL_SourceKey);

			// Booking for container 2

			var shipmentForTPUReg2 = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { ContainerNumbers = ["GENL2"], VehicleNumbers = ["REG2"], UseUniqueSourceKey = true, DataSourceKeys = ["GB00005678"] });
			logger = GetLogger(shipmentForTPUReg2);
			var tpuReg2 = new CYDTransportationUnitForGateBookingDataObjectReader(shipmentForTPUReg2, logger, Factory).ReadIntoBusinessObject();

			Factory.SaveForTesting();

			var etaForReg2 = shipmentForTPUReg2.SubShipmentCollection.Single().DateCollection.First(x => x.Type == DateType.Start).Value.GetValueOrDefault().ToZDateTimeOffset();
			AssertEquals("REG2", tpuReg2.YTU_TransportationReference);
			AssertEquals(etaForReg2, tpuReg2.YTU_EstimatedGateInTime);
			Assert(tpuReg2.YTU_GateInTime.IsEmpty);

			var deliveries = Factory.Load<CYDDelivery>(new ZQuery());
			AssertEquals(2, deliveries.Length);
			var deliveryForReg2 = deliveries.First(d => d.PK != deliveryForReg1.PK);
			AssertEquals(tpuReg2.PK, deliveryForReg2.YDL_YTU_DeliveryTransportationUnit);

			var yardUnitInReg2 = Factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, "GENL2")).Single();
			AssertEquals(deliveryForReg2.PK, yardUnitInReg2.YUS_YDL_Delivery);
			AssertEquals(tpuReg2.PK, yardUnitInReg2.YUS_YTU_ReceiveTransportationUnit);

			AssertEquals(2, Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress)).Length);
			AssertEquals(2, Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_SourceType, DataContextType.GateMovementBooking)).Length);

			// GateIn both container 1 and container 2

			var shipmentForBothContainers = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { ContainerNumbers = ["GENL", "GENL2"], VehicleNumbers = ["REG1"], IsDataSourceGateVehicleMovement = true, IsIncoming = true });
			logger = GetLogger(shipmentForBothContainers);
			var location = Factory.Load<WhsLocation>(new ZQuery()).FirstOrDefault();
			shipmentForBothContainers.RelatedShipmentCollection.FirstOrDefault().WarehouseLocation = location.ToLocationString(); // TODO: move into shipment builder

			var gateInTime = new ZDateTimeOffset(shipmentForBothContainers.DateCollection?.FirstOrDefault(date => date.Type == DateType.Start)?.Value ?? new UXmlDateTime(ZDateTime.Now));
			tpuReg1.YTU_GateInTime = tpuReg2.YTU_GateInTime = gateInTime;
			tpuReg1.YTU_WL_WaitingBayLocation = tpuReg2.YTU_WL_WaitingBayLocation = location.PK;

			Factory.SaveForTesting();

			var mergedTPU = new CYDTransportationUnitForVehicleMovementDataObjectReader(shipmentForBothContainers, logger, Factory).ReadIntoBusinessObject();

			Factory.SaveForTesting();

			AssertEquals("REG1", mergedTPU.YTU_TransportationReference);
			AssertEquals(gateInTime, mergedTPU.YTU_GateInTime);

			AssertEquals(2, Factory.Load<CYDDelivery>(new ZQuery(CYDDeliverySchema.YDL_YTU_DeliveryTransportationUnit, mergedTPU.PK)).Length);

			var yardUnitForCont1 = Factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, "GENL")).Single();
			AssertEquals(mergedTPU.PK, yardUnitForCont1.Delivery.YDL_YTU_DeliveryTransportationUnit);
			AssertEquals(mergedTPU.PK, yardUnitForCont1.YUS_YTU_ReceiveTransportationUnit);

			var yardUnitForCont2 = Factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, "GENL2")).Single();
			AssertEquals(mergedTPU.PK, yardUnitForCont2.Delivery.YDL_YTU_DeliveryTransportationUnit);
			AssertEquals(mergedTPU.PK, yardUnitForCont2.YUS_YTU_ReceiveTransportationUnit);

			AssertEquals("WUFU SHIPPING LINE", mergedTPU.TransportCompanyDocAddress.CompanyName);

			var universalLinks = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_ParentID, mergedTPU.PK));
			var universalLinkForGateBooking = universalLinks.Single(l => l.UCL_SourceType.ToString() == "GateBooking");
			var universalLinkForGateMovement = universalLinks.Single(l => l.UCL_SourceType.ToString() == "GateVehicleMovement");
			AssertEquals("GB00001234", universalLinkForGateBooking.UCL_SourceKey);
			AssertEquals("GV00001234", universalLinkForGateMovement.UCL_SourceKey);
		}

		public void TestReadFromDataObject_GivenNotAllContainersBeingGateIn_ThenSplitAndUpdateTransportationUnit_Delivery()
		{
			// Booking for container 1

			var shipmentForGateBooking1 = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { ContainerNumbers = ["GENL"], VehicleNumbers = ["REG1"] });
			var logger1 = GetLogger(shipmentForGateBooking1);
			var originalTPU1 = new CYDTransportationUnitForGateBookingDataObjectReader(shipmentForGateBooking1, logger1, Factory).ReadIntoBusinessObject();

			var shipmentForGateBooking2 = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { ContainerNumbers = ["GENL2"], VehicleNumbers = ["REG1"], DataSourceKeys = ["GM00005678"] });
			var logger2 = GetLogger(shipmentForGateBooking2);
			var originalTPU2 = new CYDTransportationUnitForGateBookingDataObjectReader(shipmentForGateBooking2, logger2, Factory).ReadIntoBusinessObject();

			AssertEquals("originalTPU1 is equal originalTPU2", originalTPU1.PK, originalTPU2.PK);

			Factory.SaveForTesting();

			var estimatedGateInTime = shipmentForGateBooking1.SubShipmentCollection.Single().DateCollection.First(x => x.Type == DateType.Start).Value.GetValueOrDefault().ToZDateTimeOffset();
			AssertEquals("REG1", originalTPU1.YTU_TransportationReference);
			AssertEquals(estimatedGateInTime, originalTPU1.YTU_EstimatedGateInTime);
			Assert(originalTPU1.YTU_GateInTime.IsEmpty);

			var yardUnitForContainer1 = Factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, "GENL")).Single();
			AssertEquals(originalTPU1.PK, yardUnitForContainer1.YUS_YTU_ReceiveTransportationUnit);

			var yardUnitForContainer2 = Factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, "GENL2")).Single();
			AssertEquals(originalTPU1.PK, yardUnitForContainer2.YUS_YTU_ReceiveTransportationUnit);

			var deliveries = Factory.Load<CYDDelivery>(new ZQuery(CYDDeliverySchema.YDL_YTU_DeliveryTransportationUnit, originalTPU1.PK));
			AssertEquals(2, deliveries.Length);
			Assert(deliveries.All(d => d.YDL_YTU_DeliveryTransportationUnit == originalTPU1.PK));
			AssertContainsExactElementsInAnyOrder([yardUnitForContainer1.YUS_YDL_Delivery, yardUnitForContainer2.YUS_YDL_Delivery], deliveries.Select(d => d.PK));

			var jobDocAddress = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress)).Single();
			AssertEquals("WUFU SHIPPING LINE", jobDocAddress.CompanyName);

			var universalJobLink = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_SourceType, DataContextType.GateBooking)).Single();
			AssertEquals("GateBooking", universalJobLink.UCL_SourceType);
			AssertEquals("GB00001234", universalJobLink.UCL_SourceKey);

			// GateIn only container 1

			var shipmentForVehicleMovement = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { ContainerNumbers = ["GENL"], VehicleNumbers = ["REG1"], IsDataSourceGateVehicleMovement = true, IsIncoming = true });
			logger1 = GetLogger(shipmentForVehicleMovement);
			var location = Factory.Load<WhsLocation>(new ZQuery()).FirstOrDefault();
			shipmentForVehicleMovement.RelatedShipmentCollection.FirstOrDefault().WarehouseLocation = location.ToLocationString(); // TODO: move into shipment builder

			var gateInTime = new ZDateTimeOffset(shipmentForVehicleMovement.DateCollection?.FirstOrDefault(date => date.Type == DateType.Start)?.Value ?? new UXmlDateTime(ZDateTime.Now));
			originalTPU1.YTU_GateInTime = gateInTime;
			originalTPU1.YTU_WL_WaitingBayLocation = location.PK;

			Factory.SaveForTesting();

			var updatedTPU = new CYDTransportationUnitForVehicleMovementDataObjectReader(shipmentForVehicleMovement, logger1, Factory).ReadIntoBusinessObject();

			Factory.SaveForTesting();

			AssertEquals(originalTPU1.PK, updatedTPU.PK);
			AssertEquals("REG1", updatedTPU.YTU_TransportationReference);
			AssertEquals(gateInTime, updatedTPU.YTU_GateInTime);
			AssertEquals(1, updatedTPU.Deliveries.Count);
			AssertEquals(1, updatedTPU.ReceiveYardUnits.Count);

			var yardUnitForCont1 = Factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, "GENL")).Single();
			AssertEquals(updatedTPU.PK, yardUnitForCont1.Delivery.YDL_YTU_DeliveryTransportationUnit);
			AssertEquals(updatedTPU.PK, yardUnitForCont1.YUS_YTU_ReceiveTransportationUnit);

			var yardUnitForCont2 = Factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, "GENL2")).Single();
			AssertNotEquals(updatedTPU.PK, yardUnitForCont2.Delivery.YDL_YTU_DeliveryTransportationUnit);
			AssertNotEquals(updatedTPU.PK, yardUnitForCont2.YUS_YTU_ReceiveTransportationUnit);

			var clonedTPU = yardUnitForCont2.ReceiveTransportationUnit;
			AssertEquals(clonedTPU.PK, yardUnitForCont2.Delivery.YDL_YTU_DeliveryTransportationUnit);
			AssertEquals("REG1", clonedTPU.YTU_TransportationReference);
			AssertNotEquals(clonedTPU.YTU_TransportationUnitID, updatedTPU.YTU_TransportationUnitID);
			Assert(clonedTPU.YTU_GateInTime.IsEmpty);
			Assert(clonedTPU.YTU_WL_WaitingBayLocation.IsEmpty);

			AssertEquals("WUFU SHIPPING LINE", updatedTPU.TransportCompanyDocAddress.CompanyName);
			AssertEquals("WUFU SHIPPING LINE", clonedTPU.TransportCompanyDocAddress.CompanyName);

			var universalLinks = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_ParentID, updatedTPU.PK));
			var universalLinkForGateBooking = universalLinks.Single(l => l.UCL_SourceType.ToString() == "GateBooking");
			var universalLinkForVehicleMovement = universalLinks.Single(l => l.UCL_SourceType.ToString() == "GateVehicleMovement");
			AssertEquals("GB00001234", universalLinkForGateBooking.UCL_SourceKey);
			AssertEquals("GV00001234", universalLinkForVehicleMovement.UCL_SourceKey);
		}

		public void TestReadFromDataObject_GivenMultipleBookingOnSameVehicleOnDifferentTimeSlot_ThenUpdateDeliveriesAccordingly()
		{
			var today = ZDateTime.Today;

			// Booking for container 1
			var shipment1 = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { ContainerNumbers = ["GENL"], VehicleNumbers = ["REG1"], UseUniqueSourceKey = true, PopulateBookingSlotTime = false });
			shipment1.SubShipmentCollection[0].SetDateCollection(() =>
			[
				new Date { Type = DateType.Start, Value = today },
				new Date { Type = DateType.End, Value = today.AddHours(1) }
				]);
			var draftTpu1 = new CYDTransportationUnitForGateBookingDataObjectReader(shipment1, GetLogger(shipment1), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			// Booking for container 2 and container 3 for the same vehicle on different time slot
			var shipment2 = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { ContainerNumbers = ["GENL2", "GENL3"], VehicleNumbers = ["REG1"], UseUniqueSourceKey = true, PopulateBookingSlotTime = false });
			shipment2.SubShipmentCollection[0].SetDateCollection(() =>
			[
				new Date { Type = DateType.Start, Value = today.AddDays(1), },
				new Date { Type = DateType.End, Value = today.AddDays(1).AddHours(1), }
			]);
			var draftTpu2 = new CYDTransportationUnitForGateBookingDataObjectReader(shipment2, GetLogger(shipment2), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			// Only container 2 is coming in, all other containers are a no show.
			var gateInShipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { ContainerNumbers = ["GENL2"], VehicleNumbers = ["REG1"], IsDataSourceGateVehicleMovement = true, IsIncoming = true });
			var gateInTime = new ZDateTimeOffset(gateInShipment.DateCollection?.FirstOrDefault(date => date.Type == DateType.Start)?.Value ?? new UXmlDateTime(ZDateTime.Now));
			var location = Factory.Load<WhsLocation>(new ZQuery()).FirstOrDefault();
			gateInShipment.RelatedShipmentCollection.FirstOrDefault().WarehouseLocation = location.ToLocationString();
			var gatedInTpu = new CYDTransportationUnitForVehicleMovementDataObjectReader(gateInShipment, GetLogger(gateInShipment), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			// Container 2 should match with second draft TPU and deliveries should be split for container 3. First draft TPU should be untouched.
			AssertEquals(3, Factory.Load<CYDTransportationUnit>(new ZQuery(CYDTransportationUnitSchema.YTU_TransportationReference, "REG1")).Length);

			var yardUnitForCont1 = Factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, "GENL")).Single();
			AssertEquals(draftTpu1.PK, yardUnitForCont1.Delivery.YDL_YTU_DeliveryTransportationUnit);
			AssertEquals(draftTpu1.PK, yardUnitForCont1.YUS_YTU_ReceiveTransportationUnit);

			AssertEquals(draftTpu2.PK, gatedInTpu.PK);
			AssertEquals(gateInTime, gatedInTpu.YTU_GateInTime);
			var yardUnitForCont2 = Factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, "GENL2")).Single();
			AssertEquals(gatedInTpu.PK, yardUnitForCont2.Delivery.YDL_YTU_DeliveryTransportationUnit);
			AssertEquals(gatedInTpu.PK, yardUnitForCont2.YUS_YTU_ReceiveTransportationUnit);

			var yardUnitForCont3 = Factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, "GENL3")).Single();
			AssertNotNull(yardUnitForCont3.Delivery.YDL_YTU_DeliveryTransportationUnit);
			AssertNotNull(yardUnitForCont3.YUS_YTU_ReceiveTransportationUnit);
		}

		public void TestReadFromDataObject_GivenContainersInTwoExistingTransportationUnit_ThenMergeAndUpdateTransportationUnit_Pickup()
		{
			// Booking for container 1

			var shipmentForReg1 = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { ContainerNumbers = ["GENL"], VehicleNumbers = ["REG1"], IsForPickup = true });
			var logger = GetLogger(shipmentForReg1);
			var tpuReg1 = new CYDTransportationUnitForGateBookingDataObjectReader(shipmentForReg1, logger, Factory).ReadIntoBusinessObject();

			Factory.SaveForTesting();

			var etaForReg1 = shipmentForReg1.SubShipmentCollection.Single().DateCollection.First(x => x.Type == DateType.Start).Value.GetValueOrDefault().ToZDateTimeOffset();
			AssertEquals("REG1", tpuReg1.YTU_TransportationReference);
			AssertEquals(etaForReg1, tpuReg1.YTU_EstimatedGateInTime);
			Assert(tpuReg1.YTU_GateInTime.IsEmpty);

			var pickupForReg1 = Factory.Load<CYDPickup>(new ZQuery()).Single();
			AssertEquals(tpuReg1.PK, pickupForReg1.YPL_YTU_PickupTransportationUnit);

			var yardUnit = Factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, "GENL")).Single();
			AssertEquals(pickupForReg1.PK, yardUnit.YUS_YPL_Pickup);
			AssertEquals(tpuReg1.PK, yardUnit.YUS_YTU_DispatchTransportationUnit);

			var jobDocAddress = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress)).Single();
			AssertEquals("WUFU SHIPPING LINE", jobDocAddress.CompanyName);

			var universalJobLink = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_SourceType, DataContextType.GateBooking)).Single();
			AssertEquals("GateBooking", universalJobLink.UCL_SourceType);
			AssertEquals("GB00001234", universalJobLink.UCL_SourceKey);

			// Booking for container 2

			var shipmentForTPUReg2 = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { ContainerNumbers = ["GENL2"], VehicleNumbers = ["REG2"], UseUniqueSourceKey = true, IsForPickup = true, DataSourceKeys = ["GB00005678"] });
			logger = GetLogger(shipmentForTPUReg2);
			var tpuReg2 = new CYDTransportationUnitForGateBookingDataObjectReader(shipmentForTPUReg2, logger, Factory).ReadIntoBusinessObject();

			Factory.SaveForTesting();

			var etaForReg2 = shipmentForTPUReg2.SubShipmentCollection.Single().DateCollection.First(x => x.Type == DateType.Start).Value.GetValueOrDefault().ToZDateTimeOffset();
			AssertEquals("REG2", tpuReg2.YTU_TransportationReference);
			AssertEquals(etaForReg2, tpuReg2.YTU_EstimatedGateInTime);
			Assert(tpuReg2.YTU_GateInTime.IsEmpty);

			var pickups = Factory.Load<CYDPickup>(new ZQuery());
			AssertEquals(2, pickups.Length);
			var pickupForReg2 = pickups.First(p => p.PK != pickupForReg1.PK);
			AssertEquals(tpuReg2.PK, pickupForReg2.YPL_YTU_PickupTransportationUnit);

			var yardUnitInReg2 = Factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, "GENL2")).Single();
			AssertEquals(pickupForReg2.PK, yardUnitInReg2.YUS_YPL_Pickup);
			AssertEquals(tpuReg2.PK, yardUnitInReg2.YUS_YTU_DispatchTransportationUnit);

			AssertEquals(2, Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress)).Length);
			AssertEquals(2, Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_SourceType, DataContextType.GateMovementBooking)).Length);

			// GateIn both container 1 and container 2

			var shipmentForBothContainers = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { ContainerNumbers = ["GENL"], VehicleNumbers = ["REG1"], IsDataSourceGateVehicleMovement = true, IsIncoming = true, IsForPickup = true });
			shipmentForBothContainers.SubShipmentCollection.Add(UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions
			{
				ContainerNumbers = ["GENL2"],
				VehicleNumbers = ["REG1"],
				IsDataSourceGateVehicleMovement = true,
				IsIncoming = true,
				IsForPickup = true,
				DataSourceKeys = ["GM00005678"]
			}).SubShipmentCollection.FirstOrDefault());
			logger = GetLogger(shipmentForBothContainers);
			var location = Factory.Load<WhsLocation>(new ZQuery()).FirstOrDefault();
			shipmentForBothContainers.RelatedShipmentCollection.FirstOrDefault().WarehouseLocation = location.ToLocationString(); // TODO: move into shipment builder

			var gateInTime = new ZDateTimeOffset(shipmentForBothContainers.DateCollection?.FirstOrDefault(date => date.Type == DateType.Start)?.Value ?? new UXmlDateTime(ZDateTime.Now));
			tpuReg1.YTU_GateInTime = gateInTime;
			tpuReg1.YTU_WL_WaitingBayLocation = location.PK;

			Factory.SaveForTesting();

			var mergedTPU = new CYDTransportationUnitForVehicleMovementDataObjectReader(shipmentForBothContainers, logger, Factory).ReadIntoBusinessObject();

			Factory.SaveForTesting();

			AssertEquals("REG1", mergedTPU.YTU_TransportationReference);
			AssertEquals(gateInTime, mergedTPU.YTU_GateInTime);

			AssertEquals(2, Factory.Load<CYDPickup>(new ZQuery(CYDPickupSchema.YPL_YTU_PickupTransportationUnit, mergedTPU.PK)).Length);

			var yardUnitForCont1 = Factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, "GENL")).Single();
			AssertEquals(mergedTPU.PK, yardUnitForCont1.Pickup.YPL_YTU_PickupTransportationUnit);
			AssertEquals(mergedTPU.PK, yardUnitForCont1.YUS_YTU_DispatchTransportationUnit);

			var yardUnitForCont2 = Factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, "GENL2")).Single();
			AssertEquals(mergedTPU.PK, yardUnitForCont2.Pickup.YPL_YTU_PickupTransportationUnit);
			AssertEquals(mergedTPU.PK, yardUnitForCont2.YUS_YTU_DispatchTransportationUnit);
			AssertEquals("WUFU SHIPPING LINE", mergedTPU.TransportCompanyDocAddress.CompanyName);

			var universalLinks = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_ParentID, mergedTPU.PK));
			var universalLinkForGateBooking = universalLinks.Single(l => l.UCL_SourceType.ToString() == "GateBooking");
			var universalLinkForGateMovement = universalLinks.Single(l => l.UCL_SourceType.ToString() == "GateVehicleMovement");
			AssertEquals("GB00001234", universalLinkForGateBooking.UCL_SourceKey);
			AssertEquals("GV00001234", universalLinkForGateMovement.UCL_SourceKey);
		}

		public void TestReadFromDataObject_GivenContainersInTwoExistingTransportationUnit_AndBothTPUsHaveGateInTime_ThenMergeAndUpdateTransportationUnit_Pickup()
		{
			// Booking for container 1

			var shipmentForReg1 = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { ContainerNumbers = ["GENL"], VehicleNumbers = ["REG1"], IsForPickup = true });
			var logger = GetLogger(shipmentForReg1);
			var tpuReg1 = new CYDTransportationUnitForGateBookingDataObjectReader(shipmentForReg1, logger, Factory).ReadIntoBusinessObject();

			Factory.SaveForTesting();

			var etaForReg1 = shipmentForReg1.SubShipmentCollection.Single().DateCollection.First(x => x.Type == DateType.Start).Value.GetValueOrDefault().ToZDateTimeOffset();
			AssertEquals("REG1", tpuReg1.YTU_TransportationReference);
			AssertEquals(etaForReg1, tpuReg1.YTU_EstimatedGateInTime);
			Assert(tpuReg1.YTU_GateInTime.IsEmpty);

			var pickupForReg1 = Factory.Load<CYDPickup>(new ZQuery()).Single();
			AssertEquals(tpuReg1.PK, pickupForReg1.YPL_YTU_PickupTransportationUnit);

			var yardUnit = Factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, "GENL")).Single();
			AssertEquals(pickupForReg1.PK, yardUnit.YUS_YPL_Pickup);
			AssertEquals(tpuReg1.PK, yardUnit.YUS_YTU_DispatchTransportationUnit);

			var jobDocAddress = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress)).Single();
			AssertEquals("WUFU SHIPPING LINE", jobDocAddress.CompanyName);

			var universalJobLink = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_SourceType, DataContextType.GateBooking)).Single();
			AssertEquals("GateBooking", universalJobLink.UCL_SourceType);
			AssertEquals("GB00001234", universalJobLink.UCL_SourceKey);

			// Booking for container 2

			var shipmentForTPUReg2 = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { ContainerNumbers = ["GENL2"], VehicleNumbers = ["REG2"], UseUniqueSourceKey = true, IsForPickup = true, DataSourceKeys = ["GM00005678"] });
			logger = GetLogger(shipmentForTPUReg2);
			var tpuReg2 = new CYDTransportationUnitForGateBookingDataObjectReader(shipmentForTPUReg2, logger, Factory).ReadIntoBusinessObject();

			Factory.SaveForTesting();

			var etaForReg2 = shipmentForTPUReg2.SubShipmentCollection.Single().DateCollection.First(x => x.Type == DateType.Start).Value.GetValueOrDefault().ToZDateTimeOffset();
			AssertEquals("REG2", tpuReg2.YTU_TransportationReference);
			AssertEquals(etaForReg2, tpuReg2.YTU_EstimatedGateInTime);
			Assert(tpuReg2.YTU_GateInTime.IsEmpty);

			var pickups = Factory.Load<CYDPickup>(new ZQuery());
			AssertEquals(2, pickups.Length);
			var pickupForReg2 = pickups.First(p => p.PK != pickupForReg1.PK);
			AssertEquals(tpuReg2.PK, pickupForReg2.YPL_YTU_PickupTransportationUnit);

			var yardUnitInReg2 = Factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, "GENL2")).Single();
			AssertEquals(pickupForReg2.PK, yardUnitInReg2.YUS_YPL_Pickup);
			AssertEquals(tpuReg2.PK, yardUnitInReg2.YUS_YTU_DispatchTransportationUnit);

			AssertEquals(2, Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress)).Length);
			AssertEquals(2, Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_SourceType, DataContextType.GateMovementBooking)).Length);

			// GateIn both container 1 and container 2

			var shipmentForBothContainers = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { ContainerNumbers = ["GENL"], VehicleNumbers = ["REG1"], IsDataSourceGateVehicleMovement = true, IsIncoming = true, IsForPickup = true });
			shipmentForBothContainers.SubShipmentCollection.Add(UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions
			{
				ContainerNumbers = ["GENL2"],
				VehicleNumbers = ["REG1"],
				IsDataSourceGateVehicleMovement = true,
				IsIncoming = true,
				IsForPickup = true,
				DataSourceKeys = ["GMV00005678"]
			})
				.SubShipmentCollection.FirstOrDefault());
			logger = GetLogger(shipmentForBothContainers);

			var location = Factory.Load<WhsLocation>(new ZQuery()).FirstOrDefault();
			shipmentForBothContainers.RelatedShipmentCollection.FirstOrDefault().WarehouseLocation = location.ToLocationString(); // TODO: move into shipment builder

			var gateInTime = new ZDateTimeOffset(shipmentForBothContainers.DateCollection?.FirstOrDefault(date => date.Type == DateType.Start)?.Value ?? new UXmlDateTime(ZDateTime.Now));
			tpuReg1.YTU_GateInTime = tpuReg2.YTU_GateInTime = gateInTime;
			tpuReg1.YTU_WL_WaitingBayLocation = tpuReg2.YTU_WL_WaitingBayLocation = location.PK;

			Factory.SaveForTesting();

			var mergedTPU = new CYDTransportationUnitForVehicleMovementDataObjectReader(shipmentForBothContainers, logger, Factory).ReadIntoBusinessObject();

			Factory.SaveForTesting();

			AssertEquals("REG1", mergedTPU.YTU_TransportationReference);
			AssertEquals(gateInTime, mergedTPU.YTU_GateInTime);

			AssertEquals(2, Factory.Load<CYDPickup>(new ZQuery(CYDPickupSchema.YPL_YTU_PickupTransportationUnit, mergedTPU.PK)).Length);

			var yardUnitForCont1 = Factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, "GENL")).Single();
			AssertEquals(mergedTPU.PK, yardUnitForCont1.Pickup.YPL_YTU_PickupTransportationUnit);
			AssertEquals(mergedTPU.PK, yardUnitForCont1.YUS_YTU_DispatchTransportationUnit);

			var yardUnitForCont2 = Factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, "GENL2")).Single();
			AssertEquals(mergedTPU.PK, yardUnitForCont2.Pickup.YPL_YTU_PickupTransportationUnit);
			AssertEquals(mergedTPU.PK, yardUnitForCont2.YUS_YTU_DispatchTransportationUnit);
			AssertEquals("WUFU SHIPPING LINE", mergedTPU.TransportCompanyDocAddress.CompanyName);

			var universalLinks = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_ParentID, mergedTPU.PK));
			var universalLinkForGateBooking = universalLinks.Single(l => l.UCL_SourceType.ToString() == "GateBooking");
			var universalLinkForGateMovement = universalLinks.Single(l => l.UCL_SourceType.ToString() == "GateVehicleMovement");
			AssertEquals("GB00001234", universalLinkForGateBooking.UCL_SourceKey);
			AssertEquals("GV00001234", universalLinkForGateMovement.UCL_SourceKey);
		}

		public void TestReadFromDataObject_GivenNotAllContainersBeingGateIn_ThenSplitAndUpdateTransportationUnit_Pickup()
		{
			var shipmentForGateBooking = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { ContainerNumbers = ["GENL"], VehicleNumbers = ["REG1"], IsForPickup = true });
			shipmentForGateBooking.SubShipmentCollection.Add(UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions
			{
				ContainerNumbers = ["GENL2"],
				VehicleNumbers = ["REG1"],
				IsForPickup = true,
				DataSourceKeys = ["GBM00005678"]
			}).SubShipmentCollection.First());
			var logger = GetLogger(shipmentForGateBooking);
			var originalTPU = new CYDTransportationUnitForGateBookingDataObjectReader(shipmentForGateBooking, logger, Factory).ReadIntoBusinessObject();

			Factory.SaveForTesting();

			var estimatedGateInTime = shipmentForGateBooking.SubShipmentCollection.First().DateCollection.First(x => x.Type == DateType.Start).Value.GetValueOrDefault().ToZDateTimeOffset();
			AssertEquals("REG1", originalTPU.YTU_TransportationReference);
			AssertEquals(estimatedGateInTime, originalTPU.YTU_EstimatedGateInTime);
			Assert(originalTPU.YTU_GateInTime.IsEmpty);

			var yardUnitForContainer1 = Factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, "GENL")).Single();
			AssertEquals(originalTPU.PK, yardUnitForContainer1.YUS_YTU_DispatchTransportationUnit);

			var yardUnitForContainer2 = Factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, "GENL2")).Single();
			AssertEquals(originalTPU.PK, yardUnitForContainer2.YUS_YTU_DispatchTransportationUnit);

			var pickups = Factory.Load<CYDPickup>(new ZQuery(CYDPickupSchema.YPL_YTU_PickupTransportationUnit, originalTPU.PK));
			AssertEquals(2, pickups.Length);
			Assert(pickups.All(p => p.YPL_YTU_PickupTransportationUnit == originalTPU.PK));
			AssertContainsExactElementsInAnyOrder([yardUnitForContainer1.YUS_YPL_Pickup, yardUnitForContainer2.YUS_YPL_Pickup], pickups.Select(p => p.PK));

			var jobDocAddress = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress)).Single();
			AssertEquals("WUFU SHIPPING LINE", jobDocAddress.CompanyName);

			var universalJobLink = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_SourceType, DataContextType.GateBooking)).Single();
			AssertEquals("GateBooking", universalJobLink.UCL_SourceType);
			AssertEquals("GB00001234", universalJobLink.UCL_SourceKey);

			// GateIn only container 1

			var shipmentForVehicleMovement = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { ContainerNumbers = ["GENL"], VehicleNumbers = ["REG1"], IsDataSourceGateVehicleMovement = true, IsIncoming = true, IsForPickup = true });
			logger = GetLogger(shipmentForVehicleMovement);
			var location = Factory.Load<WhsLocation>(new ZQuery()).FirstOrDefault();
			shipmentForVehicleMovement.RelatedShipmentCollection.FirstOrDefault().WarehouseLocation = location.ToLocationString(); // TODO: move into shipment builder

			var gateInTime = new ZDateTimeOffset(shipmentForVehicleMovement.DateCollection?.FirstOrDefault(date => date.Type == DateType.Start)?.Value ?? new UXmlDateTime(ZDateTime.Now));
			originalTPU.YTU_GateInTime = gateInTime;
			originalTPU.YTU_WL_WaitingBayLocation = location.PK;

			Factory.SaveForTesting();

			var updatedTPU = new CYDTransportationUnitForVehicleMovementDataObjectReader(shipmentForVehicleMovement, logger, Factory).ReadIntoBusinessObject();

			Factory.SaveForTesting();

			AssertEquals(originalTPU.PK, updatedTPU.PK);
			AssertEquals("REG1", updatedTPU.YTU_TransportationReference);
			AssertEquals(gateInTime, updatedTPU.YTU_GateInTime);
			AssertEquals(1, updatedTPU.Pickups.Count);
			AssertEquals(1, updatedTPU.ReleaseYardUnits.Count);

			var yardUnitForCont1 = Factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, "GENL")).Single();
			AssertEquals(updatedTPU.PK, yardUnitForCont1.Pickup.YPL_YTU_PickupTransportationUnit);
			AssertEquals(updatedTPU.PK, yardUnitForCont1.YUS_YTU_DispatchTransportationUnit);

			var yardUnitForCont2 = Factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, "GENL2")).Single();
			AssertNotEquals(updatedTPU.PK, yardUnitForCont2.Pickup.YPL_YTU_PickupTransportationUnit);
			AssertNotEquals(updatedTPU.PK, yardUnitForCont2.YUS_YTU_DispatchTransportationUnit);

			var clonedTPU = yardUnitForCont2.DispatchTransportationUnit;
			AssertEquals(clonedTPU.PK, yardUnitForCont2.Pickup.YPL_YTU_PickupTransportationUnit);
			AssertEquals("REG1", clonedTPU.YTU_TransportationReference);
			AssertNotEquals(clonedTPU.YTU_TransportationUnitID, updatedTPU.YTU_TransportationUnitID);
			Assert(clonedTPU.YTU_GateInTime.IsEmpty);
			Assert(clonedTPU.YTU_WL_WaitingBayLocation.IsEmpty);

			AssertEquals("WUFU SHIPPING LINE", updatedTPU.TransportCompanyDocAddress.CompanyName);
			AssertEquals("WUFU SHIPPING LINE", clonedTPU.TransportCompanyDocAddress.CompanyName);

			var universalLinks = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_ParentID, updatedTPU.PK));
			var universalLinkForGateBooking = universalLinks.Single(l => l.UCL_SourceType.ToString() == "GateBooking");
			var universalLinkForVehicleMovement = universalLinks.Single(l => l.UCL_SourceType.ToString() == "GateVehicleMovement");
			AssertEquals("GB00001234", universalLinkForGateBooking.UCL_SourceKey);
			AssertEquals("GV00001234", universalLinkForVehicleMovement.UCL_SourceKey);
		}

		public void TestReadFromDataObject_GivenMultipleBookingOnSameVehicleOnDifferentTimeSlot_ThenUpdatePickupsAccordingly()
		{
			var today = ZDateTime.Today;

			// Booking for container 1
			var shipment1 = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { ContainerNumbers = ["GENL"], VehicleNumbers = ["REG1"], UseUniqueSourceKey = true, PopulateBookingSlotTime = false, IsForPickup = true });
			shipment1.SubShipmentCollection[0].SetDateCollection(() =>
			[
				new Date { Type = DateType.Start, Value = today },
				new Date { Type = DateType.End, Value = today.AddHours(1) }
				]);
			var draftTpu1 = new CYDTransportationUnitForGateBookingDataObjectReader(shipment1, GetLogger(shipment1), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			// Booking for container 2 for the same vehicle on different time slot
			var shipment2 = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { ContainerNumbers = ["GENL2", "GENL3"], VehicleNumbers = ["REG1"], UseUniqueSourceKey = true, PopulateBookingSlotTime = false, IsForPickup = true });
			shipment2.SubShipmentCollection[0].SetDateCollection(() =>
			[
				new Date { Type = DateType.Start, Value = today.AddDays(1), },
				new Date { Type = DateType.End, Value = today.AddDays(1).AddHours(1), }
			]);
			var draftTpu2 = new CYDTransportationUnitForGateBookingDataObjectReader(shipment2, GetLogger(shipment2), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			// GateIn for container 2 will match the first booking and it should update the draft TPU accordingly
			var gateInShipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { ContainerNumbers = ["GENL2"], VehicleNumbers = ["REG1"], IsDataSourceGateVehicleMovement = true, IsIncoming = true, IsForPickup = true });
			var gateInTime = new ZDateTimeOffset(gateInShipment.DateCollection?.FirstOrDefault(date => date.Type == DateType.Start)?.Value ?? new UXmlDateTime(ZDateTime.Now));
			var location = Factory.Load<WhsLocation>(new ZQuery()).FirstOrDefault();
			gateInShipment.RelatedShipmentCollection.FirstOrDefault().WarehouseLocation = location.ToLocationString();
			var gatedInTpu = new CYDTransportationUnitForVehicleMovementDataObjectReader(gateInShipment, GetLogger(gateInShipment), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			// Container 2 should match with second draft TPU and pickups should be split for container 3. First draft TPU should be untouched.
			AssertEquals(3, Factory.Load<CYDTransportationUnit>(new ZQuery(CYDTransportationUnitSchema.YTU_TransportationReference, "REG1")).Length);

			var yardUnitForCont1 = Factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, "GENL")).Single();
			AssertEquals(draftTpu1.PK, yardUnitForCont1.Pickup.YPL_YTU_PickupTransportationUnit);
			AssertEquals(draftTpu1.PK, yardUnitForCont1.YUS_YTU_DispatchTransportationUnit);

			AssertEquals(draftTpu2.PK, gatedInTpu.PK);
			AssertEquals(gateInTime, gatedInTpu.YTU_GateInTime);
			var yardUnitForCont2 = Factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, "GENL2")).Single();
			AssertEquals(gatedInTpu.PK, yardUnitForCont2.Pickup.YPL_YTU_PickupTransportationUnit);
			AssertEquals(gatedInTpu.PK, yardUnitForCont2.YUS_YTU_DispatchTransportationUnit);

			var yardUnitForCont3 = Factory.Load<CYDYardUnitState>(new ZQuery(CYDYardUnitStateSchema.YUS_UnitID, "GENL3")).Single();
			AssertNotNull(yardUnitForCont3.Pickup.YPL_YTU_PickupTransportationUnit);
			AssertNotNull(yardUnitForCont3.YUS_YTU_DispatchTransportationUnit);
		}

		CYDTransportationUnit CreateCYDTransportationUnit(string transportationReference, ZDateTimeOffset estimatedGateInTime, ZDateTimeOffset gateInTime, DateTime systemCreateTimeUtc, ZGuid waitingBayLocation, ZGuid yardPK, Shipment shipment)
		{
			var tpu = Factory.NewWithValidTestData<CYDTransportationUnit>();
			tpu.YTU_TransportationReference = transportationReference;
			tpu.YTU_GateInTime = gateInTime;
			tpu.YTU_SystemCreateTimeUtc = systemCreateTimeUtc;
			tpu.YTU_WL_WaitingBayLocation = waitingBayLocation;
			tpu.YTU_WW_Yard = yardPK;
			tpu.YTU_EstimatedGateInTime = estimatedGateInTime;

			var organizationAddress = shipment.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.TransportCompanyDocumentaryAddress));

			var header = Factory.LoadFromUniqueKey<IOrgHeader>(OrgHeaderSchema.OH_Code, (ZString)organizationAddress.OrganizationCode);
			var address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.OA_OH, header.PK));

			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_ParentID = tpu.PK;
			jobDocAddress.E2_ParentTableCode = CYDTransportationUnitSchema.Constants.Prefix;
			jobDocAddress.E2_OA_Address = address.PK;
			jobDocAddress.E2_AddressType = "TRA";

			Factory.SaveForTesting();
			return tpu;
		}

		UniversalTestData Data;

		protected override void SetUp()
		{
			base.SetUp();
			Data = new UniversalTestData(Factory, new TestErrorLogger());
			var today = ZDateTime.Today.Date;
			var tomorrow = ZDateTime.Today.AddDays(1).Date;
			Data.SetupDataForPickup("BKR01", "BKR01", today, tomorrow, "20GP", new string[] { "GENL", "GENL2", "GENL3" });
		}

		DummyLogger GetLogger(Shipment shipment)
		{
			var logger = new DummyLogger();
			logger.TopLevelDataObject = shipment;
			return logger;
		}
	}
}
