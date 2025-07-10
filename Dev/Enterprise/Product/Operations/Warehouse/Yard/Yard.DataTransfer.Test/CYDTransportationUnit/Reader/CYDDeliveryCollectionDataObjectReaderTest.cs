using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal.Test
{
	[TestedType(typeof(CYDDeliveryCollectionDataObjectReader))]
	public class CYDDeliveryCollectionDataObjectReaderTest : DataObjectCollectionReaderTest
	{
		public override void TestReadIntoCollection()
		{
			var today = ZDateTime.Today.Date;
			var tomorrow = ZDateTime.Today.AddDays(1).Date;
			new UniversalTestData(Factory, new TestErrorLogger()).SetupDataForDelivery("BKR01", today, tomorrow, "20GP", ["GENL"]);
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { ContainerNumbers = new string[] { "GENL" } });
			var subShipment = shipment.SubShipmentCollection.FirstOrDefault();
			var containers = subShipment.ContainerCollection;
			var transportationUnit = Factory.NewWithValidTestData<CYDTransportationUnit>();
			var yard = YardMatchingHelper.GetYard(shipment, Factory, new DummyLogger());
			transportationUnit.YTU_WW_Yard = yard.GetValue(WhsWarehouseSchema.PK);
			var logger = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			var reader = new CYDDeliveryCollectionDataObjectReader(containers, transportationUnit, subShipment, logger, Factory);
			reader.ReadIntoCollection();

			AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching Delivery found, creating new Delivery.
Information - Populating Delivery...
Information - Added CYDDelivery from UniversalShipment.
".Trim(), logger.LogOutput);

			var deliveryHeader = Factory.Load<CYDDeliveryHeader>(new ZQuery()).Single();
			AssertEquals("Delivery Header Job Number should be populated.", transportationUnit.YTU_WW_Yard, deliveryHeader.YDH_WW_Yard);

			var deliveries = Factory.Load<CYDDelivery>(new ZQuery());
			AssertEquals(1, deliveries.Length);
			AssertEquals("Delivery Transport Reference should be populated", "TREF240419151008", deliveries[0].YDL_TransportReference);
			AssertEquals("Delivery Transportation Unit should be populated", transportationUnit.PK, deliveries[0].LinkedYardUnit.YUS_YTU_ReceiveTransportationUnit);

			var yardUnit = Factory.Load<CYDYardUnitState>(new ZQuery()).Single();
			Assert("Yard Unit's line item should not be populated during booking", yardUnit.YUS_YLI_UnitLineItem.IsEmpty);
		}

		public void TestReadIntoCollection_ForVehicleMovement()
		{
			var today = ZDateTime.Today.Date;
			var tomorrow = ZDateTime.Today.AddDays(1).Date;
			new UniversalTestData(Factory, new TestErrorLogger()).SetupDataForDelivery("BKR01", today, tomorrow, "20GP", ["GENL"]);
			var shipment = UniversalTestShipmentBuilder.GetShipment(
				new TestShipmentOptions
				{
					IsDataSourceGateVehicleMovement = true,
					IsIncoming = true
				});
			var subShipment = shipment.SubShipmentCollection.FirstOrDefault();
			var containers = subShipment.ContainerCollection;
			var transportationUnit = Factory.NewWithValidTestData<CYDTransportationUnit>();
			var yard = YardMatchingHelper.GetYard(shipment, Factory, new DummyLogger());
			transportationUnit.YTU_WW_Yard = yard.GetValue(WhsWarehouseSchema.PK);
			var logger = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			var reader = new CYDDeliveryCollectionDataObjectReader(containers, transportationUnit, subShipment, logger, Factory);
			reader.ReadIntoCollection();

			AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching Delivery found, creating new Delivery.
Information - Populating Delivery...
Information - Added CYDDelivery from UniversalShipment.
".Trim(), logger.LogOutput);

			var deliveryHeader = Factory.Load<CYDDeliveryHeader>(new ZQuery()).Single();
			AssertEquals("Delivery Header Job Number should be populated.", transportationUnit.YTU_WW_Yard, deliveryHeader.YDH_WW_Yard);

			var deliveries = Factory.Load<CYDDelivery>(new ZQuery());
			AssertEquals(1, deliveries.Length);
			AssertEquals("Delivery Transport Reference should be populated", "TREF240419151008", deliveries[0].YDL_TransportReference);
			AssertEquals("Delivery Transportation Unit should be populated", transportationUnit.PK, deliveries[0].LinkedYardUnit.YUS_YTU_ReceiveTransportationUnit);

			var yardUnit = Factory.Load<CYDYardUnitState>(new ZQuery()).Single();
			Assert("Yard unit's line item should be populated during booking", yardUnit.YUS_YLI_UnitLineItem.IsValid);
		}

		public void TestReadIntoCollection_UpdateContainerType()
		{
			var today = ZDateTime.Today.Date;
			var tomorrow = ZDateTime.Today.AddDays(1).Date;
			new UniversalTestData(Factory, new TestErrorLogger()).SetupDataForDelivery("BKR01", today, tomorrow, "20FR", ["GENL"]);
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { ContainerNumbers = new string[] { "GENL" } });
			var subShipment = shipment.SubShipmentCollection.FirstOrDefault();
			var containers = subShipment.ContainerCollection;
			var transportationUnit = Factory.NewWithValidTestData<CYDTransportationUnit>();
			var yard = YardMatchingHelper.GetYard(shipment, Factory, new DummyLogger());
			transportationUnit.YTU_WW_Yard = yard.GetValue(WhsWarehouseSchema.PK);
			var logger = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			var reader = new CYDDeliveryCollectionDataObjectReader(containers, transportationUnit, subShipment, logger, Factory);
			reader.ReadIntoCollection();

			AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching Delivery found, creating new Delivery.
Information - Populating Delivery...
Information - Added CYDDelivery from UniversalShipment.
".Trim(), logger.LogOutput);

			var deliveryHeader = Factory.Load<CYDDeliveryHeader>(new ZQuery()).Single();
			AssertEquals("Delivery Header Job Number should be populated.", transportationUnit.YTU_WW_Yard, deliveryHeader.YDH_WW_Yard);

			var deliveries = Factory.Load<CYDDelivery>(new ZQuery());
			AssertEquals(1, deliveries.Length);
			AssertEquals("Delivery Transport Reference should be populated", "TREF240419151008", deliveries[0].YDL_TransportReference);
			AssertEquals("Delivery Container Type should be updated", "20GP", deliveries[0].UnitLineItem.ContainerType.RC_Code);
		}

		public void TestReadIntoCollection_WithUnknownContainerNumber()
		{
			var today = ZDateTime.Today.Date;
			var tomorrow = ZDateTime.Today.AddDays(1).Date;
			new UniversalTestData(Factory, new TestErrorLogger()).SetupDataForDelivery("BKR01", today, tomorrow, "20GP", []);
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { ContainerNumbers = new string[] { "UNKNOWN" } });
			var subShipment = shipment.SubShipmentCollection.FirstOrDefault();
			var containers = subShipment.ContainerCollection;
			var transportationUnit = Factory.NewWithValidTestData<CYDTransportationUnit>();
			var yard = YardMatchingHelper.GetYard(shipment, Factory, new DummyLogger());
			transportationUnit.YTU_WW_Yard = yard.GetValue(WhsWarehouseSchema.PK);
			var logger = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			var reader = new CYDDeliveryCollectionDataObjectReader(containers, transportationUnit, subShipment, logger, Factory);
			reader.ReadIntoCollection();

			AssertMultilineASCIIEquals("logger.Logs", @"
Information - The container 'UNKNOWN' of Acceptance Number 'BKR01' is not found, a new Yard Unit is created.
Information - No matching Delivery found, creating new Delivery.
Information - Populating Delivery...
Information - Added CYDDelivery from UniversalShipment.
".Trim(), logger.LogOutput);

			var deliveryHeader = Factory.Load<CYDDeliveryHeader>(new ZQuery()).Single();
			AssertEquals("Delivery Header Job Number should be populated.", transportationUnit.YTU_WW_Yard, deliveryHeader.YDH_WW_Yard);

			var deliveries = Factory.Load<CYDDelivery>(new ZQuery());
			AssertEquals(1, deliveries.Length);
			AssertEquals("Delivery Transport Reference should be populated", "TREF240419151008", deliveries[0].YDL_TransportReference);

			var yardUnits = Factory.Load<CYDYardUnitState>(new ZQuery());
			AssertNotNull("Unknow Yard Unit should be populated", yardUnits.FirstOrDefault(y => y.YUS_UnitID == "UNKNOWN"));
		}

		public void TestReadIntoCollection_WithNullContainerType()
		{
			var today = ZDateTime.Today.Date;
			var tomorrow = ZDateTime.Today.AddDays(1).Date;
			new UniversalTestData(Factory, new TestErrorLogger()).SetupDataForDelivery("BKR01", today, tomorrow, "20FR", ["GENL"]);
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { ContainerNumbers = ["GENL"], ContainerTypeCode = null });
			var subShipment = shipment.SubShipmentCollection.FirstOrDefault();
			var containers = subShipment.ContainerCollection;
			var transportationUnit = Factory.NewWithValidTestData<CYDTransportationUnit>();
			var yard = YardMatchingHelper.GetYard(shipment, Factory, new DummyLogger());
			transportationUnit.YTU_WW_Yard = yard.GetValue(WhsWarehouseSchema.PK);
			var logger = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			var reader = new CYDDeliveryCollectionDataObjectReader(containers, transportationUnit, subShipment, logger, Factory);
			reader.ReadIntoCollection();

			AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching Delivery found, creating new Delivery.
Information - Populating Delivery...
Information - Added CYDDelivery from UniversalShipment.
".Trim(), logger.LogOutput);

			var deliveryHeader = Factory.Load<CYDDeliveryHeader>(new ZQuery()).Single();
			AssertEquals("Delivery Header Job Number should be populated.", transportationUnit.YTU_WW_Yard, deliveryHeader.YDH_WW_Yard);

			var deliveries = Factory.Load<CYDDelivery>(new ZQuery());
			AssertEquals(1, deliveries.Length);
			AssertEquals("Delivery Transport Reference should be populated", "TREF240419151008", deliveries[0].YDL_TransportReference);
			AssertEquals("Delivery Container Type should be populated from PRA line", "20FR", deliveries[0].UnitLineItem.ContainerType.RC_Code);
		}

		public void TestReadIntoCollection_FirstMessageHasNoAcceptanceNumber()
		{
			var today = ZDateTime.Today.Date;
			var tomorrow = ZDateTime.Today.AddDays(1).Date;
			new UniversalTestData(Factory, new TestErrorLogger()).SetupDataForDelivery("BKR01", today, tomorrow, "20FR", ["GENL"]);
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { ContainerNumbers = new string[] { "GENL" }, BookingConfirmationReference = "" });
			var subShipment = shipment.SubShipmentCollection.FirstOrDefault();
			var containers = subShipment.ContainerCollection;
			var transportationUnit = Factory.NewWithValidTestData<CYDTransportationUnit>();
			var yard = YardMatchingHelper.GetYard(shipment, Factory, new DummyLogger());
			transportationUnit.YTU_WW_Yard = yard.GetValue(WhsWarehouseSchema.PK);
			var logger = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			var reader = new CYDDeliveryCollectionDataObjectReader(containers, transportationUnit, subShipment, logger, Factory);
			reader.ReadIntoCollection();

			AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching Delivery found, creating new Delivery.
Information - Populating Delivery...
Information - Added CYDDelivery from UniversalShipment.
".Trim(), logger.LogOutput);

			var deliveryHeader = Factory.Load<CYDDeliveryHeader>(new ZQuery()).Single();
			AssertEquals("Delivery Header Job Number should be populated.", transportationUnit.YTU_WW_Yard, deliveryHeader.YDH_WW_Yard);

			var deliveries = Factory.Load<CYDDelivery>(new ZQuery());
			AssertEquals(1, deliveries.Length);
			AssertEquals("Delivery Transport Reference should be populated", "TREF240419151008", deliveries[0].YDL_TransportReference);
			AssertEquals("New Yard Unit should be created", 1, deliveries.Count(d => d.LinkedYardUnit.YUS_UnitID == "GENL"));
			AssertEquals("Delivery Type should be populated", "CNT", deliveries[0].UnitLineItem.YLI_Type);
			AssertEquals("Delivery Container Type should be populated", "20GP", deliveries[0].UnitLineItem.ContainerType.RC_Code);
			AssertEquals("Delivery Quantity should be populated", (short)1, deliveries[0].UnitLineItem.YLI_Quantity);
			AssertEquals("Delivery IsEmpty should be populated", true, deliveries[0].UnitLineItem.YLI_IsEmpty);

			var yardUnits = Factory.Load<CYDYardUnitState>(new ZQuery());
			AssertEquals(1, yardUnits.Length);
			AssertEquals("Known Yard Unit should be populated", 1, yardUnits.Count(y => y.YUS_UnitID == "GENL"));

			// Second message
			Factory.SaveForTesting();
			var logger2 = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			subShipment.BookingConfirmationReference = "BKR01";
			var reader2 = new CYDDeliveryCollectionDataObjectReader(containers, transportationUnit, subShipment, logger2, Factory);
			reader2.ReadIntoCollection();

			AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching Delivery.
Information - Populating Delivery...
Information - Updated CYDDelivery from UniversalShipment.
".Trim(), logger2.LogOutput);

			deliveryHeader = Factory.Load<CYDDeliveryHeader>(new ZQuery()).Single();
			AssertEquals("Delivery Header Job Number should be populated.", transportationUnit.YTU_WW_Yard, deliveryHeader.YDH_WW_Yard);

			deliveries = Factory.Load<CYDDelivery>(new ZQuery());
			AssertEquals(1, deliveries.Length);
			AssertEquals("Delivery Transport Reference should be populated", "TREF240419151008", deliveries[0].YDL_TransportReference);
			AssertEquals("New Yard Unit should be created", 1, deliveries.Count(d => d.LinkedYardUnit.YUS_UnitID == "GENL"));
			AssertEquals("Delivery Type should be populated", "CNT", deliveries[0].UnitLineItem.YLI_Type);
			AssertEquals("Delivery Container Type should be populated", "20GP", deliveries[0].UnitLineItem.ContainerType.RC_Code);
			AssertEquals("Delivery Quantity should be populated", (short)1, deliveries[0].UnitLineItem.YLI_Quantity);
			AssertEquals("Delivery IsEmpty should be populated", true, deliveries[0].UnitLineItem.YLI_IsEmpty);
			AssertNotNull("Delivery Receive Line should be populated", deliveries[0].YDL_YRL_ReceiveAdviceLine);

			yardUnits = Factory.Load<CYDYardUnitState>(new ZQuery());
			AssertEquals(1, deliveries.Length);
			AssertEquals("Known Yard Unit should be populated", 1, yardUnits.Count(y => y.YUS_UnitID == "GENL"));
		}

		public void TestReadIntoCollection_SecondMessageHasNoAcceptanceNumber()
		{
			var today = ZDateTime.Today.Date;
			var tomorrow = ZDateTime.Today.AddDays(1).Date;
			new UniversalTestData(Factory, new TestErrorLogger()).SetupDataForDelivery("BKR01", today, tomorrow, "20FR", ["GENL"]);
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { ContainerNumbers = new string[] { "GENL" } });
			var subShipment = shipment.SubShipmentCollection.FirstOrDefault();
			var containers = subShipment.ContainerCollection;
			var transportationUnit = Factory.NewWithValidTestData<CYDTransportationUnit>();
			var yard = YardMatchingHelper.GetYard(shipment, Factory, new DummyLogger());
			transportationUnit.YTU_WW_Yard = yard.GetValue(WhsWarehouseSchema.PK);
			var logger = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			var reader = new CYDDeliveryCollectionDataObjectReader(containers, transportationUnit, subShipment, logger, Factory);
			reader.ReadIntoCollection();

			AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching Delivery found, creating new Delivery.
Information - Populating Delivery...
Information - Added CYDDelivery from UniversalShipment.
".Trim(), logger.LogOutput);

			var deliveryHeader = Factory.Load<CYDDeliveryHeader>(new ZQuery()).Single();
			AssertEquals("Delivery Header Job Number should be populated.", transportationUnit.YTU_WW_Yard, deliveryHeader.YDH_WW_Yard);

			var deliveries = Factory.Load<CYDDelivery>(new ZQuery());
			AssertEquals(1, deliveries.Length);
			AssertEquals("Delivery Transport Reference should be populated", "TREF240419151008", deliveries[0].YDL_TransportReference);
			AssertEquals("New Yard Unit should be created", 1, deliveries.Count(d => d.LinkedYardUnit.YUS_UnitID == "GENL"));
			AssertEquals("Delivery Type should be populated", "CNT", deliveries[0].UnitLineItem.YLI_Type);
			AssertEquals("Delivery Container Type should be populated", "20GP", deliveries[0].UnitLineItem.ContainerType.RC_Code);
			AssertEquals("Delivery Quantity should be populated", (short)1, deliveries[0].UnitLineItem.YLI_Quantity);
			AssertEquals("Delivery IsEmpty should be populated", true, deliveries[0].UnitLineItem.YLI_IsEmpty);

			var yardUnits = Factory.Load<CYDYardUnitState>(new ZQuery());
			AssertEquals(1, deliveries.Length);
			AssertEquals("Known Yard Unit should be populated", 1, yardUnits.Count(y => y.YUS_UnitID == "GENL"));

			// Second message
			Factory.SaveForTesting();
			var logger2 = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			subShipment.BookingConfirmationReference = "";
			var reader2 = new CYDDeliveryCollectionDataObjectReader(containers, transportationUnit, subShipment, logger2, Factory);
			reader2.ReadIntoCollection();

			AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching Delivery.
Information - Populating Delivery...
Information - Updated CYDDelivery from UniversalShipment.
".Trim(), logger2.LogOutput);

			deliveryHeader = Factory.Load<CYDDeliveryHeader>(new ZQuery()).Single();
			AssertEquals("Delivery Header Job Number should be populated.", transportationUnit.YTU_WW_Yard, deliveryHeader.YDH_WW_Yard);

			deliveries = Factory.Load<CYDDelivery>(new ZQuery());
			AssertEquals(1, deliveries.Length);
			AssertEquals("Delivery Transport Reference should be populated", "TREF240419151008", deliveries[0].YDL_TransportReference);
			AssertEquals("New Yard Unit should be created", 1, deliveries.Count(d => d.LinkedYardUnit.YUS_UnitID == "GENL"));
			AssertEquals("Delivery Type should be populated", "CNT", deliveries[0].UnitLineItem.YLI_Type);
			AssertEquals("Delivery Container Type should be populated", "20GP", deliveries[0].UnitLineItem.ContainerType.RC_Code);
			AssertEquals("Delivery Quantity should be populated", (short)1, deliveries[0].UnitLineItem.YLI_Quantity);
			AssertEquals("Delivery IsEmpty should be populated", true, deliveries[0].UnitLineItem.YLI_IsEmpty);
			AssertNotNull("Delivery Receive Line should be populated", deliveries[0].YDL_YRL_ReceiveAdviceLine);

			yardUnits = Factory.Load<CYDYardUnitState>(new ZQuery());
			AssertEquals(1, deliveries.Length);
			AssertEquals("Known Yard Unit should be populated", 1, yardUnits.Count(y => y.YUS_UnitID == "GENL"));
		}

		public void TestReadDeliveryDataObject_ForVehicleMovement_WhenGatedInAlready_ThenThrowDataObjectReadFailureException()
		{
			var today = ZDateTime.Today.Date;
			var tomorrow = ZDateTime.Today.AddDays(1).Date;
			new UniversalTestData(Factory, new TestErrorLogger()).SetupDataForDelivery("BKR01", today, tomorrow, "20GP", ["GENL", "GENL2", "GENL3"]);
			var shipment = UniversalTestShipmentBuilder.GetShipment(
				new TestShipmentOptions
				{
					ContainerNumbers = ["NEWCON"],
					VehicleNumbers = ["REG1"],
					IsDataSourceGateVehicleMovement = true,
					IsIncoming = true
				});
			var subShipment = shipment.SubShipmentCollection.FirstOrDefault();
			var containers = subShipment.ContainerCollection;
			var location = Factory.Load<WhsLocation>(new ZQuery()).FirstOrDefault();
			shipment.RelatedShipmentCollection.FirstOrDefault().WarehouseLocation = location.ToLocationString();

			// Simulate the behaviour on CYDTransportationUniDataObjectReader where TPU gate in time and location bay are set but hasn't been committed
			var transportationUnit = Factory.NewWithValidTestData<CYDTransportationUnit>();
			Factory.SaveForTesting();
			var gateInTime = new ZDateTimeOffset(shipment.DateCollection?.FirstOrDefault(date => date.Type == DateType.Start)?.Value ?? new UXmlDateTime(ZDateTime.Now));
			transportationUnit.YTU_GateInTime = gateInTime;
			transportationUnit.YTU_WL_WaitingBayLocation = location.PK;

			var logger = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			var reader = new CYDDeliveryCollectionDataObjectReader(containers, transportationUnit, subShipment, logger, Factory);

			AssertExceptionThrown(
				"TPU Gate In when PRA is not available.",
				typeof(DataObjectReadFailureException),
				"TPU Gate In when PRA is not available.",
				() => reader.ReadIntoCollection());

			// Use another factory to retrieve only data that has been committed to DB, we do not want to assert cached data.
			var factory = new UniversalObjectFactory();
			var tpu = factory.Load<CYDTransportationUnit>(new ZQuery(CYDTransportationUnitSchema.PK, transportationUnit.PK)).FirstOrDefault();
			AssertEquals("TPU Gate In should be blocked when PRA is not available.", ZDateTimeOffset.Empty, tpu.YTU_GateInTime);
			AssertEquals("TPU Gate In should be blocked when PRA is not available.", Guid.Empty, tpu.YTU_WL_WaitingBayLocation);
		}

		public void TestReadDeliveryDataObject_ForVehicleMovement_WhenBookingReferenceIsNotProvided_ShouldReturnError()
		{
			var today = ZDateTime.Today.Date;
			var tomorrow = ZDateTime.Today.AddDays(1).Date;
			new UniversalTestData(Factory, new TestErrorLogger()).SetupDataForDelivery("BKR01", today, tomorrow, "20GP", ["GENL", "GENL2", "GENL3"]);
			var shipment = UniversalTestShipmentBuilder.GetShipment(
				new TestShipmentOptions
				{
					ContainerNumbers = ["NEWCON"],
					VehicleNumbers = ["REG1"],
					BookingConfirmationReference = "",
					IsDataSourceGateVehicleMovement = true,
					IsIncoming = true
				});
			var subShipment = shipment.SubShipmentCollection.FirstOrDefault();
			var containers = subShipment.ContainerCollection;
			var location = Factory.Load<WhsLocation>(new ZQuery()).FirstOrDefault();
			shipment.RelatedShipmentCollection.FirstOrDefault().WarehouseLocation = location.ToLocationString();

			var transportationUnit = Factory.NewWithValidTestData<CYDTransportationUnit>();
			var logger = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			var reader = new CYDDeliveryCollectionDataObjectReader(containers, transportationUnit, subShipment, logger, Factory);

			AssertExceptionThrown(
				"Missing booking reference in Gate Movement",
				typeof(DataObjectReadFailureException),
				"Booking Confirmation Reference is required in UXML for delivery.",
				() => reader.ReadIntoCollection());
		}
	}
}
