using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.Business.Testing
{
	public class HVLVTestDataCreatorTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestCreateConsignment()
		{
			var factory = new BusinessObjectFactory();
			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			factory.Save();

			AssertEquals(0, shipment.HVLVConsignments.Count());

			AssertNoExceptionThrown(() => HVLVShipmentTestDataCreator.CreateConsignment(shipment.JS_UniqueConsignRef, 2, "Test"));

			var consignmentHeaders = factory.Load<HVLVConsignmentHeader>(new ZQuery());

			CombineAssertions("Created consignment header", () =>
			{
				AssertEquals("Should create 1 consignment header", 1, consignmentHeaders.Length);
				AssertEquals("Consignment header should link to shipment", shipment.PK, consignmentHeaders[0].HCH_JS_Shipment);
			});

			var consignments = factory.Load<HVLVConsignment>(new ZQuery());

			CombineAssertions("Created consignments", () =>
			{
				AssertEquals("Should created 2 consignments", 2, consignments.Length);
				Assert("Waybill number should start with Test", consignments.All(c => c.HVC_WaybillNumber.StartsWith("Test")));
				Assert("All created consignments should link to consignment header", consignments.All(c => c.HVC_HCH_Header == consignmentHeaders[0].PK));
			});
		}

		[UseSnapshotProtection]
		public void TestCreateConsignment_WithExistingConsignmentHeader()
		{
			var factory = new BusinessObjectFactory();
			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			factory.Save();

			AssertEquals(0, shipment.HVLVConsignments.Count());

			AssertNoExceptionThrown(() => HVLVShipmentTestDataCreator.CreateConsignment(shipment.JS_UniqueConsignRef, 2, "Test"));

			var consignments = factory.Load<HVLVConsignment>(new ZQuery());

			CombineAssertions("Created consignments", () =>
			{
				AssertEquals("Should created 2 consignments", 2, consignments.Length);
				Assert("Waybill number should start with Test", consignments.All(c => c.HVC_WaybillNumber.StartsWith("Test")));
				Assert("All created consignments should link to consignment header", consignments.All(c => c.HVC_HCH_Header == consignmentHeader.PK));
			});
		}

		[UseSnapshotProtection]
		public void TestCreateConsignment_WhenConsignmentHeaderNotExist_ShouldCreate()
		{
			var factory = new BusinessObjectFactory();
			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var address = factory.NewWithValidTestData<OrgAddress>();
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = address.PK;
			shipment.ConsignorDocumentaryAddress.E2_AddressType = MasterFiles.Integration.AutoDocAddressTypes.Codes.ConsignorDocumentaryAddress;

			factory.Save();

			AssertEquals(0, shipment.HVLVConsignments.Count());

			AssertNoExceptionThrown(() => HVLVShipmentTestDataCreator.CreateConsignment(shipment.JS_UniqueConsignRef, 2, "Test"));

			var consignmentHeaders = factory.Load<HVLVConsignmentHeader>(new ZQuery());

			CombineAssertions("created consignment header", () =>
			{
				AssertEquals("should created 1 consignment header", 1, consignmentHeaders.Length);
				AssertEquals("created consignment header should belong to shipment", shipment.PK, consignmentHeaders[0].HCH_JS_Shipment);
			});

			var consignments = factory.Load<HVLVConsignment>(new ZQuery());

			CombineAssertions("created consignments", () =>
			{
				AssertEquals("should created 2 consignments", 2, consignments.Length);
				Assert("waybill number should start with Test", consignments.All(c => c.HVC_WaybillNumber.StartsWith("Test")));
				Assert("all created consignments should be linked to consignment header", consignments.All(c => c.HVC_HCH_Header == consignmentHeaders[0].PK));
			});
		}

		[UseSnapshotProtection]
		public void TestCreateConsignment_SetShipperStateToAUK()
		{
			var factory = new BusinessObjectFactory();
			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			factory.Save();

			AssertEquals(0, shipment.HVLVConsignments.Count());

			AssertNoExceptionThrown(() => HVLVShipmentTestDataCreator.CreateConsignment(shipment.JS_UniqueConsignRef, 1, "Test"));

			var consignments = factory.Load<HVLVConsignment>(new ZQuery());
			var consignment = consignments.Single();

			AssertEquals("Shipper state is set to AUK", "AUK", consignment.HVC_ShipperState);
		}

		[UseSnapshotProtection]
		public void TestCreateConsignment_MultipleCreationsForSameShipment()
		{
			var factory = new BusinessObjectFactory();
			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			factory.Save();

			AssertEquals(0, shipment.HVLVConsignments.Count());

			for (var i = 0; i < 5; i++)
			{
				AssertNoExceptionThrown(() => HVLVShipmentTestDataCreator.CreateConsignment(shipment.JS_UniqueConsignRef, 2, "Test"));
			}
		}

		[UseSnapshotProtection]
		public void TestCreateConsignment_RequiredTestDataDetails()
		{
			var factory = new BusinessObjectFactory();
			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			factory.Save();

			HVLVShipmentTestDataCreator.CreateConsignment(shipment.JS_UniqueConsignRef, 1, "Test");
			var items = factory.Load<HVLVItem>(new ZQuery());

			CombineAssertions("Item details", () =>
			{
				AssertEquals("Item count is 1", 1, items.Length);
				AssertEquals("Item status is SHP", "SHP", items.First().HVI_Status);
				AssertEquals("IsScannedAtDestination is false ", false, items.First().HVI_IsScannedAtDestination);
			});
		}

		[UseSnapshotProtection]
		public void TestCreateConsignment_SystemCreateUserAndLastEditUserIsE()
		{
			var factory = new BusinessObjectFactory();
			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			factory.Save();

			HVLVShipmentTestDataCreator.CreateConsignment(shipment.JS_UniqueConsignRef, 1, "Test");

			var consignmentHeader = factory.LoadTop1<HVLVConsignmentHeader>(new ZQuery());
			var consignment = factory.LoadTop1<HVLVConsignment>(new ZQuery());
			var item = factory.LoadTop1<HVLVItem>(new ZQuery());
			var itemLine = factory.LoadTop1<HVLVItemLine>(new ZQuery());

			CombineAssertions("System create user and last edit user should be E for all created bizos", () =>
			{
				AssertEquals("E", consignmentHeader.HCH_SystemCreateUser);
				AssertEquals("E", consignmentHeader.HCH_SystemLastEditUser);
				AssertEquals("E", consignment.HVC_SystemCreateUser);
				AssertEquals("E", consignment.HVC_SystemLastEditUser);
				AssertEquals("E", item.HVI_SystemCreateUser);
				AssertEquals("E", item.HVI_SystemLastEditUser);
				AssertEquals("E", itemLine.HVS_SystemCreateUser);
				AssertEquals("E", itemLine.HVS_SystemLastEditUser);
			});
		}

		[UseSnapshotProtection]
		public void TestCreateConsignment_DeleteExistingConsignments()
		{
			var factory = new BusinessObjectFactory();
			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			factory.Save();

			HVLVShipmentTestDataCreator.CreateConsignment(shipment.JS_UniqueConsignRef, 1, "Test");
			var consignments = factory.Load<HVLVConsignment>(new ZQuery());
			AssertEquals("Insert 1 new line into table HVLVConsignment", 1, consignments.Length);
			HVLVShipmentTestDataCreator.DeleteExistingConsignments(shipment.JS_UniqueConsignRef);

			var factory2 = new BusinessObjectFactory();
			consignments = factory2.Load<HVLVConsignment>(new ZQuery());
			AssertEquals("All data in table HVLVConsignment has been deleted", 0, consignments.Length);
		}

		[UseSnapshotProtection]
		public void TestCreateConsignment_LastMileCarrierServiceLevel()
		{
			var factory = new BusinessObjectFactory();
			var shipment = factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			factory.Save();

			HVLVShipmentTestDataCreator.CreateConsignment(shipment.JS_UniqueConsignRef, 1, "Test");

			var consignment = factory.LoadTop1<HVLVConsignment>(new ZQuery());
			AssertEquals(string.Empty, consignment.HVC_PL_NKLastMileCarrierServiceLevel);
		}
	}
}
