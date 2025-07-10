using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	#region WhsBondedChangeOfInventoryDataObjectReader_BCR

	abstract class WhsBondedChangeOfInventoryDataObjectReader_BCR : WhsBondedChangeOfInventoryDataObjectReaderTest
	{
		protected override RecipientRoleType RecipientRoleForTest => RecipientRoleType.BCR;

		#region TestPopulateBusinessObject_Failed_CustomsLineDetailsForChangeOfRegimeNotRegistered

		public void TestPopulateBusinessObject_Failed_CustomsLineDetailsForChangeOfRegimeNotRegistered()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			Factory.SaveForTesting();

			var emptyChangeOfRegimeDetails = new Hashtable();
			using (ObjectFactory.Substitute("WarehouseCustomsDetailsChangeOfRegimes", emptyChangeOfRegimeDetails))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);

				AssertExceptionThrown("Must provide details for import.", typeof(DataObjectReadFailureException), @"
	Customs Details for Change of Regime import could not be found for Country Code: AU.
	".Trim(), () => reader.ReadIntoBusinessObject());
			}
		}

		#endregion

		#region TestPopulateBusinessObject_BondedWarehouse

		public void TestPopulateBusinessObject_ChangeOfRegime_PopulatesLineLocation_NoChangeOfRegimeType_BondedWarehouse()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			Factory.SaveForTesting();

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: false, intoRegimeType: CustomsRegime.BondedWarehouse, outOfRegimeType: CustomsRegime.BondedWarehouse))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				var changeOfInventoryBO = reader.ReadIntoBusinessObject();
				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", changeOfInventoryBO);

				var order = changeOfInventoryBO.Order;
				AssertNotNull("Should have read in a Order.", order);
				AssertEquals("Should set Docket IsInwardsProcessingJob appropriately", false, order.WD_IsInwardsProcessingJob);

				var receive = changeOfInventoryBO.Receive;
				AssertNotNull("Should have read in a Receive.", receive);
				AssertEquals("Should set Docket IsInwardsProcessingJob appropriately", false, receive.WD_IsInwardsProcessingJob);

				var receiveLine = receive.Lines.SingleOrDefault();
				AssertNotNull("Should have read in a Receive Line.", receiveLine);

				AssertEquals("P1", receiveLine.ProductCode);
				AssertEquals("RED", receiveLine.WE_PartAttrib1);
				AssertEquals("Medium", receiveLine.WE_PartAttrib2);
				AssertEquals("S1234", receiveLine.WE_PartAttrib3);
				AssertEquals("DUMMYOUTWARD-1-102", receiveLine.WE_BondedEntryKey);

				AssertEquals(IsDocketFinalisedOnImport ? "BOND" : "A-1-1-1", receiveLine.LocationString);
			}
		}

		#endregion

		#region TestChangeOfRegime_ChangeOfWarehouse

		public void TestChangeOfRegime_ChangeOfWarehouse_ToPhysicalWarehouse()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventoryAsInwardProcessing: false);
			Factory.SaveForTesting();

			var newWarehouseAddress = Data.Orgs.CRAHOLSYD.MainAddress;
			var newWarehouse = Helper.CreateWarehouse("WH2", "A", 1, 1);
			newWarehouse.WW_OA_WarehouseAddress = newWarehouseAddress.PK;
			newWarehouse.WW_IsVirtualWarehouse = false;
			newWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			Factory.SaveForTesting();

			var bondArea = Helper.CreateArea(newWarehouse, "BOND", AreaTypes.Codes.Bonded);
			var location1 = newWarehouse.FindLocation("A");
			location1.WLV_LocationStatus = LocationStatus.Codes.Normal;
			location1.WLV_WA_PutawayArea = bondArea.PK;

			Factory.SaveForTesting();

			var newWarehouseOrganisationAddress = GetNewAddressData_CRAHOLSYD(DocAddressType.CustomsWarehouseAddress);

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: false, newWarehouse: newWarehouseOrganisationAddress))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				var changeOfInventoryBO = reader.ReadIntoBusinessObject();
				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", changeOfInventoryBO);

				var order = changeOfInventoryBO.Order;
				AssertNotNull("Should have read in a Order.", order);
				AssertEquals("Should set Docket IsInwardsProcessingJob appropriately",
					false,
					order.WD_IsInwardsProcessingJob);

				var receive = changeOfInventoryBO.Receive;
				AssertNotNull("Should have read in a Receive.", receive);
				AssertEquals("Should set Docket IsInwardsProcessingJob appropriately",
					false,
					receive.WD_IsInwardsProcessingJob);

				AssertEquals("Should have read receive into new warehouse.", newWarehouse.PK, receive.WD_WW_Whs);

				var receiveLine = receive.Lines.SingleOrDefault();
				AssertNotNull("Should have read in a Receive Line.", receiveLine);

				AssertEquals("P1", receiveLine.ProductCode);
				AssertEquals("RED", receiveLine.WE_PartAttrib1);
				AssertEquals("Medium", receiveLine.WE_PartAttrib2);
				AssertEquals("S1234", receiveLine.WE_PartAttrib3);
				AssertEquals("DUMMYOUTWARD-1-102", receiveLine.WE_BondedEntryKey);

				AssertEquals("Order is finalised only for virtual warehouses.", IsSourceWarehouseVirtual, order.IsFinalised);
				AssertEquals("Order is held for customs accordingly.", !IsSourceWarehouseVirtual, order.IsDocketHeldByCustoms);
				Assert("Receive is not finalised for physical warehouse.", !receive.IsFinalised);
				Assert("Receive is held for customs.", receive.IsDocketHeldByCustoms);
			}
		}

		public void TestChangeOfRegime_ChangeOfWarehouse_ToVirtualWarehouse()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventoryAsInwardProcessing: false);
			Factory.SaveForTesting();

			var newWarehouseAddress = Data.Orgs.CRAHOLSYD.MainAddress;
			var newWarehouse = Helper.CreateWarehouse("WH2", "A", 1, 1);
			newWarehouse.WW_OA_WarehouseAddress = newWarehouseAddress.PK;
			newWarehouse.WW_IsVirtualWarehouse = true;
			newWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			Factory.SaveForTesting();

			var bondArea = Helper.CreateArea(newWarehouse, "BOND", AreaTypes.Codes.Bonded);
			var location1 = newWarehouse.FindLocation("A");
			location1.WLV_LocationStatus = LocationStatus.Codes.Normal;
			location1.WLV_WA_PutawayArea = bondArea.PK;

			Factory.SaveForTesting();

			var newWarehouseOrganisationAddress = GetNewAddressData_CRAHOLSYD(DocAddressType.CustomsWarehouseAddress);

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: false, newWarehouse: newWarehouseOrganisationAddress))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				var changeOfInventoryBO = reader.ReadIntoBusinessObject();
				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", changeOfInventoryBO);

				var order = changeOfInventoryBO.Order;
				AssertNotNull("Should have read in a Order.", order);
				AssertEquals("Should set Docket IsInwardsProcessingJob appropriately",
					false,
					order.WD_IsInwardsProcessingJob);

				var receive = changeOfInventoryBO.Receive;
				AssertNotNull("Should have read in a Receive.", receive);
				AssertEquals("Should set Docket IsInwardsProcessingJob appropriately",
					false,
					receive.WD_IsInwardsProcessingJob);

				AssertEquals("Should have read receive into new warehouse.", newWarehouse.PK, receive.WD_WW_Whs);

				var receiveLine = receive.Lines.SingleOrDefault();
				AssertNotNull("Should have read in a Receive Line.", receiveLine);

				AssertEquals("P1", receiveLine.ProductCode);
				AssertEquals("RED", receiveLine.WE_PartAttrib1);
				AssertEquals("Medium", receiveLine.WE_PartAttrib2);
				AssertEquals("S1234", receiveLine.WE_PartAttrib3);
				AssertEquals("DUMMYOUTWARD-1-102", receiveLine.WE_BondedEntryKey);

				AssertEquals("Order is finalised only for virtual warehouses.", IsSourceWarehouseVirtual, order.IsFinalised);
				AssertEquals("Order is held for customs accordingly.", !IsSourceWarehouseVirtual, order.IsDocketHeldByCustoms);
				Assert("Receive is finalised for virtual warehouse.", receive.IsFinalised);
				Assert("Receive is not held for customs.", !receive.IsDocketHeldByCustoms);
			}
		}

		#endregion
	}

	class WhsBondedChangeOfInventoryDataObjectReader_RealWarehouse_BCR_Test : WhsBondedChangeOfInventoryDataObjectReader_BCR
	{
		protected override bool IsSourceWarehouseVirtual => false;

		// Dockets for ChangeOfRegime in Physical Warehouse should be held for Customs, not finalised
		protected override bool IsDocketFinalisedOnImport => false;

		#region TestChangeOfRegime_PhysicalWarehouse

		public void TestChangeOfRegime_PhysicalWarehouse_ExistingReceiveIsFinalised()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventory: false);
			Factory.SaveForTesting();

			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, Warehouse, "R1");
			receive.WD_CustomsParentReference = "B123-EDIDATEDI";
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 100m, LocationA1, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.SaveForTesting();

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: false, intoRegimeType: CustomsRegime.InwardProcessing))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);

				AssertExceptionThrown("Should fail due to matching Receive which is finalised.", typeof(DataObjectReadFailureException), @"
Cannot amend warehouse job for stock movement if original warehouse job is finalized in Physical Warehouse.
".Trim(), () => reader.ReadIntoBusinessObject());
			}
		}

		public void TestChangeOfRegime_PhysicalWarehouse_ExistingReceiveIsStartedReceiving()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventory: false);
			Factory.SaveForTesting();

			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, Warehouse, "R1");
			receive.WD_CustomsParentReference = "B123-EDIDATEDI";
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 100m, LocationA1, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			Factory.SaveForTesting();

			receive.PopulateASNLines();
			Factory.SaveForTesting();

			Assert("Precondition: Receive has started receiving.", receive.StartedReceiving);

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: false, intoRegimeType: CustomsRegime.InwardProcessing))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);

				AssertExceptionThrown("Should fail due to matching Receive which has started receiving.", typeof(DataObjectReadFailureException), @"
Cannot amend receive for Change of Regime if original receive has started receiving in Physical Warehouse.
".Trim(), () => reader.ReadIntoBusinessObject());
			}
		}

		[GuiTest]
		public void TestChangeOfRegime_PhysicalWarehouse_ExistingOrderIsFinalised()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventory: false);
			Factory.SaveForTesting();

			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, Warehouse, "R1");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 100m, LocationA1, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.SaveForTesting();

			var order = Helper.CreateWhsOrder(Data.Orgs.CRAHOLSYD, Warehouse, "O1");
			order.WD_CustomsParentReference = "B123-EDIDATEDI";
			var orderLine = Helper.CreateWhsOrderLine(order, Data.ProductCRAHOLSYD, 100m);
			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);
			pick.AutoAllocateItems();
			pick.FinaliseAllOrders();
			Factory.SaveForTesting();

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: false, intoRegimeType: CustomsRegime.InwardProcessing))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);

				AssertExceptionThrown("Should fail due to matching Order which is finalised.", typeof(DataObjectReadFailureException), @"
Cannot amend warehouse job for stock movement if original warehouse job is finalized in Physical Warehouse.
".Trim(), () => reader.ReadIntoBusinessObject());
			}
		}

		public void TestChangeOfRegime_PhysicalWarehouse_ExistingOrderIsCancelled()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventory: false);
			Factory.SaveForTesting();

			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, Warehouse, "R1");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 100m, LocationA1, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.SaveForTesting();

			var order = Helper.CreateWhsOrder(Data.Orgs.CRAHOLSYD, Warehouse, "O1");
			order.WD_CustomsParentReference = "B123-EDIDATEDI";
			order.CancelReactivateDocket();
			Factory.SaveForTesting();

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: false, intoRegimeType: CustomsRegime.InwardProcessing))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);

				AssertExceptionThrown("Should fail due to matching Order which is cancelled.", typeof(DataObjectReadFailureException), @"
Cannot amend warehouse job for stock movement if original warehouse job is canceled in Physical Warehouse.
".Trim(), () => reader.ReadIntoBusinessObject());
			}
		}

		[GuiTest]
		public void TestChangeOfRegime_PhysicalWarehouse_CancelsOriginalPick()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventory: false);
			Factory.SaveForTesting();

			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, Warehouse, "R1");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 50m, LocationA1, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 50m, LocationA1, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 50m, LocationA1, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.SaveForTesting();

			var order = Helper.CreateWhsOrder(Data.Orgs.CRAHOLSYD, Warehouse, "O1");
			order.WD_CustomsParentReference = "B123-EDIDATEDI";
			var orderLine = Helper.CreateWhsOrderLine(order, Data.ProductCRAHOLSYD, 100m);
			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);
			pick.AutoAllocateItems();
			Factory.SaveForTesting();

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: false, intoRegimeType: CustomsRegime.InwardProcessing))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				var changeOfInventoryBO = reader.ReadIntoBusinessObject();
				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", changeOfInventoryBO);

				var newOrder = changeOfInventoryBO.Order;
				AssertNotNull("Should have read in a Order.", order);

				var newReceive = changeOfInventoryBO.Receive;
				AssertNotNull("Should have read in a Receive.", receive);

				Assert("Order is not finalised for physical warehouse.", !newOrder.IsFinalised);
				Assert("Order is held for customs.", newOrder.IsDocketHeldByCustoms);
				Assert("Receive is not finalised for physical warehouse.", !newReceive.IsFinalised);
				Assert("Receive is held for customs.", newReceive.IsDocketHeldByCustoms);

				Assert("Original Pick is Cancelled.", pick.IsCancelled);
				AssertNotEquals("New Pick is assigned to order.", pick, newOrder.Pick);
			}
		}

		[GuiTest]
		public void TestChangeOfRegime_PhysicalWarehouse_PickCannotBeCancelled()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventory: false);
			Factory.SaveForTesting();

			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, Warehouse, "R1");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 50m, LocationA1, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 50m, LocationA1, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 50m, LocationA1, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.SaveForTesting();

			var order = Helper.CreateWhsOrder(Data.Orgs.CRAHOLSYD, Warehouse, "O1");
			order.WD_CustomsParentReference = "B123-EDIDATEDI";
			var orderLine = Helper.CreateWhsOrderLine(order, Data.ProductCRAHOLSYD, 100m);
			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);
			pick.AutoAllocateItems();
			Factory.SaveForTesting();

			var pickLine = orderLine.PickLines[0];
			pickLine.WZ_PickedDateTime = DateTime.Now;
			Factory.SaveForTesting();

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: true))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);

				AssertExceptionThrown("Should fail due to matching Order which is finalised.", typeof(DataObjectReadFailureException), @"
Cannot Import Order
Warehouse Order could not be amended for Customs Job B123 because of the following error(s) when canceling original pick:
Cannot perform this operation because the Order is partially or fully picked.
".Trim(), () => reader.ReadIntoBusinessObject());
			}
		}

		public void TestChangeOfRegime_PhysicalWarehouse_OrderIsInShortfall()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventory: false);
			Factory.SaveForTesting();

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: false, intoRegimeType: CustomsRegime.InwardProcessing))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);

				AssertExceptionThrown("Should fail due to matching Order which is in shortfall.", typeof(DataObjectReadFailureException), @"
Cannot Import Order
Order could not be created for Customs Job B123 because there are errors:
You do not have enough stock to fulfill shortfalls on this order
ENTRYNUMBER123-2 Product P1/P1 can not be ordered due to lack of stock. 5 was ordered, but 0 is available
".Trim(), () => reader.ReadIntoBusinessObject());
			}
		}

		public void TestChangeOfRegime_PhysicalWarehouse_BondedWarehouseToInwardsProcessing()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventoryAsInwardProcessing: false);
			Factory.SaveForTesting();

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: false, outOfRegimeType: CustomsRegime.BondedWarehouse, intoRegimeType: CustomsRegime.InwardProcessing))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				var changeOfInventoryBO = reader.ReadIntoBusinessObject();
				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", changeOfInventoryBO);

				var order = changeOfInventoryBO.Order;
				AssertNotNull("Should have read in a Order.", order);
				AssertEquals("Should set Docket IsInwardsProcessingJob appropriately",
					false,
					order.WD_IsInwardsProcessingJob);

				var receive = changeOfInventoryBO.Receive;
				AssertNotNull("Should have read in a Receive.", receive);
				AssertEquals("Should set Docket IsInwardsProcessingJob appropriately",
					true,
					receive.WD_IsInwardsProcessingJob);

				var receiveLine = receive.Lines.SingleOrDefault();
				AssertNotNull("Should have read in a Receive Line.", receiveLine);

				AssertEquals("P1", receiveLine.ProductCode);
				AssertEquals("RED", receiveLine.WE_PartAttrib1);
				AssertEquals("Medium", receiveLine.WE_PartAttrib2);
				AssertEquals("S1234", receiveLine.WE_PartAttrib3);
				AssertEquals("DUMMYOUTWARD-1-102", receiveLine.WE_BondedEntryKey);

				Assert("Order is not finalised for physical warehouse.", !order.IsFinalised);
				Assert("Order is held for customs.", order.IsDocketHeldByCustoms);
				Assert("Receive is not finalised for physical warehouse.", !receive.IsFinalised);
				Assert("Receive is held for customs.", receive.IsDocketHeldByCustoms);
			}
		}

		#endregion
	}

	class WhsBondedChangeOfInventoryDataObjectReader_VirtualWarehouse_BCR_Test : WhsBondedChangeOfInventoryDataObjectReader_BCR
	{
		protected override bool IsSourceWarehouseVirtual => true;

		#region TestPopulateBusinessObject_InwardsProcessing

		public void TestPopulateBusinessObject_InwardsProcessing_IntoRegimeType()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			Factory.SaveForTesting();

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: false, intoRegimeType: CustomsRegime.InwardProcessing))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);

				var changeOfInventoryBO = reader.ReadIntoBusinessObject();
				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", changeOfInventoryBO);
				AssertNotNull("Should have read in a Order.", changeOfInventoryBO.Order);
				AssertNotNull("Should have read in a Receive.", changeOfInventoryBO.Receive);
			}
		}

		public void TestPopulateBusinessObject_InwardsProcessing_OutOfRegimeType()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventoryAsInwardProcessing: true);
			Factory.SaveForTesting();

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: false, outOfRegimeType: CustomsRegime.InwardProcessing))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				var changeOfInventoryBO = reader.ReadIntoBusinessObject();
				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", changeOfInventoryBO);
				AssertNotNull("Should have read in a Order for Warehouse 1.", changeOfInventoryBO.Order);
				AssertNotNull("Should have read in a Receive for Warehouse 2.", changeOfInventoryBO.Receive);
			}
		}

		#endregion

		#region TestPopulateBusinessObject_ChangeOfRegime_PopulatesLineLocation

		public void TestPopulateBusinessObject_ChangeOfRegime_PopulatesLineLocation_NoChangeOfRegimeType_InwardProcessing()
		{
			TestPopulateBusinessObject_ChangeOfRegime_PopulatesLineLocation_ChangeOfRegimeTypeCore(CustomsRegime.InwardProcessing, CustomsRegime.InwardProcessing);
		}

		public void TestPopulateBusinessObject_ChangeOfRegime_PopulatesLineLocation_BondedWarehouseToInwardProcessing()
		{
			TestPopulateBusinessObject_ChangeOfRegime_PopulatesLineLocation_ChangeOfRegimeTypeCore(CustomsRegime.BondedWarehouse, CustomsRegime.InwardProcessing);
		}

		public void TestPopulateBusinessObject_ChangeOfRegime_PopulatesLineLocation_InwardProcessingToBondedWarehouse()
		{
			TestPopulateBusinessObject_ChangeOfRegime_PopulatesLineLocation_ChangeOfRegimeTypeCore(CustomsRegime.InwardProcessing, CustomsRegime.BondedWarehouse);
		}

		void TestPopulateBusinessObject_ChangeOfRegime_PopulatesLineLocation_ChangeOfRegimeTypeCore(CustomsRegime intoRegimeType, CustomsRegime outOfRegimeType)
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventoryAsInwardProcessing: outOfRegimeType == CustomsRegime.InwardProcessing);
			Factory.SaveForTesting();

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: false, intoRegimeType: intoRegimeType, outOfRegimeType: outOfRegimeType))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				var changeOfInventoryBO = reader.ReadIntoBusinessObject();
				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", changeOfInventoryBO);

				var order = changeOfInventoryBO.Order;
				AssertNotNull("Should have read in a Order.", order);
				AssertEquals("Should set Docket IsInwardsProcessingJob appropriately",
					outOfRegimeType == CustomsRegime.InwardProcessing,
					order.WD_IsInwardsProcessingJob);

				var receive = changeOfInventoryBO.Receive;
				AssertNotNull("Should have read in a Receive.", receive);
				AssertEquals("Should set Docket IsInwardProcessingJob appropriately",
					intoRegimeType == CustomsRegime.InwardProcessing,
					receive.WD_IsInwardsProcessingJob);

				var receiveLine = receive.Lines.SingleOrDefault();
				AssertNotNull("Should have read in a Receive Line.", receiveLine);

				AssertEquals("P1", receiveLine.ProductCode);
				AssertEquals("RED", receiveLine.WE_PartAttrib1);
				AssertEquals("Medium", receiveLine.WE_PartAttrib2);
				AssertEquals("S1234", receiveLine.WE_PartAttrib3);
				AssertEquals("DUMMYOUTWARD-1-102", receiveLine.WE_BondedEntryKey);

				var expectedLocation = receive.WD_IsInwardsProcessingJob ? "INW" : "BOND";
				AssertEquals(expectedLocation, receiveLine.LocationString);
			}
		}

		#endregion
	}

	#endregion

	#region WhsBondedChangeOfInventoryDataObjectReader_BCO

	abstract class WhsBondedChangeOfInventoryDataObjectReader_BCO : WhsBondedChangeOfInventoryDataObjectReaderTest
	{
		protected override RecipientRoleType RecipientRoleForTest => RecipientRoleType.BCO;

		#region TestPopulateBusinessObject_Failed_InvalidNewOwner

		public void TestPopulateBusinessObject_Failed_InvalidNewOwner()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(addImporterToXML: false);
			Factory.SaveForTesting();

			var invalidNewOwner = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress), CompanyName = "INVALID OWNER" };
			var customsDetails = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsDetailsChangeOfOwnershipMock(newOwner: invalidNewOwner);
			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(customDetails: customsDetails, isChangeOfOwnership: true))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				AssertExceptionThrown("New owner Receive cannot be found.", typeof(DataObjectReadFailureException), @"
Cannot Import Receipt
Unable to match New Owner Address, please make sure the supplied New Owner Address is valid. Details were:
CompanyName: INVALID OWNER
".Trim(), () => reader.ReadIntoBusinessObject());
			}
		}

		#endregion

		#region TestPopulateBusinessObject_Failed_InvalidNewOwnerProducts

		public void TestPopulateBusinessObject_Failed_InvalidNewOwnerProducts()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			Factory.SaveForTesting();

			var line = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine();
			var lineDetails = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(invoiceLine: line, newProductCode: "X1");
			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(customsLineDetails: new[] { lineDetails.Object }, isChangeOfOwnership: true))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				AssertExceptionThrown("New owner products cannot be found.", typeof(DataObjectReadFailureException), @"
Cannot Import Receipt Line 1
Unable to match Product: X1 for New Owner WUFSHIJNB.
".Trim(), () => reader.ReadIntoBusinessObject());
			}
		}

		#endregion

		#region TestPopulateBusinessObject_Failed_InvalidOldOwnerProducts

		public void TestPopulateBusinessObject_Failed_InvalidOldOwnerProducts()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			Factory.SaveForTesting();

			var line = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine("X1");
			var lineDetails = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(Data.ProductWUFSHIJNB.OP_PartNum, invoiceLine: line);
			using (new WarehouseCustomsDetailsProvidersMocks(customsLineDetails: new[] { lineDetails.Object }, isChangeOfOwnership: true))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				AssertExceptionThrown("New owner products cannot be found.", typeof(DataObjectReadFailureException), @"
Cannot Import Order Line 1
Unable to match Product: X1 for Old Owner CRAHOLSYD.
".Trim(), () => reader.ReadIntoBusinessObject());
			}
		}

		#endregion

		#region TestPopulateBusinessObject_Failed_InvalidOldOwner

		public void TestPopulateBusinessObject_Failed_InvalidOldOwner()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(addImporterToXML: false);
			Factory.SaveForTesting();

			var invalidOldOwner = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress), CompanyName = "INVALID OWNER" };
			var customsDetails = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsDetailsChangeOfOwnershipMock(oldOwner: invalidOldOwner);
			using (new WarehouseCustomsDetailsProvidersMocks(customDetails: customsDetails, isChangeOfOwnership: true))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				AssertExceptionThrown("Old owner for Order cannot be found.", typeof(DataObjectReadFailureException), @"
Cannot Import Order
Unable to match Old Owner Address, please make sure the supplied Old Owner Address is valid. Details were:
CompanyName: INVALID OWNER
".Trim(), () => reader.ReadIntoBusinessObject());
			}
		}

		#endregion

		#region TestPopulateBusinessObject_OnHold_ValidateHeldReceiveForNewOwnerProducts

		public void TestPopulateBusinessObject_OnHold_ValidateHeldReceiveForNewOwnerProducts()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(addImporterToXML: true, isWarehouseCreatedAsVirtual: IsSourceWarehouseVirtual, createInventory: true, recipientRoles: new[] { new RecipientRoleDetail { Type = RecipientRoleForTest, ServiceCode = ServiceCodeType.HLD } }, isSerialNumberTest: true);
			Factory.SaveForTesting();
			AssertEquals("Precondition - hold service code is set for recipient role ", ServiceCodeType.HLD, Logger.TopLevelDataObject.DataContext.RecipientRoleCollection.First(r => r.Code == RecipientRoleForTest).ServiceCode);

			// make new owner attrib 1 mandatory 
			Helper.SetClientAttributeType(Data.Orgs.WUFSHIJNB, AttributeNumber.One, mandatoryAttributeType: true, attributeName: "Height");
			Factory.SaveForTesting();

			// import new product that does not specify attrib1
			var line = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine(orderedQty: 1, previousSerialNum: "SNN");
			var lineDetails = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock("W1", invoiceLine: line, newAttrib1: "");

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(customsLineDetails: new[] { lineDetails.Object }, isChangeOfOwnership: true))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);

				AssertExceptionThrown("New owner product requires a mandatory attrib 1.", typeof(DataObjectReadFailureException), @"
Cannot Import Receipt
Receipt could not be finalized into the Warehouse for Customs Job B123 because of the following error(s):
Error: Finalise
Error - WE_PartAttrib1: Please enter a Height.
".Trim(), () => reader.ReadIntoBusinessObject());
			}
		}

		#endregion

		#region TestPopulateBusinessObject_CreateFinalisedOrderAndReceive_ChangeOfOwnership_DoesNotUseWarehouseClient

		public void TestPopulateBusinessObject_CreateFinalisedOrderAndReceive_ChangeOfOwnership_DoesNotUseWarehouseClient()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			Factory.SaveForTesting();

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: true))
			{
				var warehouseClientDO = GetNewAddressData_INTHEMSYD(AddressTypes.WarehouseClient);
				Data.ShipmentDataObject.OrganizationAddressCollection.Add(warehouseClientDO);

				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				var changeOfInventoryBO = reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", changeOfInventoryBO);
				AssertWhsDocket(changeOfInventoryBO.Order, Data.Orgs.CRAHOLSYD, "B123", "B123-EDIDATEDI");
				AssertWhsDocket(changeOfInventoryBO.Receive, Data.Orgs.WUFSHIJNB, "B123", "B123-EDIDATEDI");
			}
		}

		#endregion

		#region TestPopulateBusinessObject_MultipleProducts_NoNewAttributes

		public void TestPopulateBusinessObject_MultipleProducts_NoNewAttributes()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			Factory.SaveForTesting();

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: true))
			{
				var warehouseClientDO = GetNewAddressData_INTHEMSYD(AddressTypes.WarehouseClient);
				Data.ShipmentDataObject.OrganizationAddressCollection.Add(warehouseClientDO);

				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				var changeOfInventoryBO = reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", changeOfInventoryBO);
				AssertWhsDocket(changeOfInventoryBO.Order, Data.Orgs.CRAHOLSYD, "B123", "B123-EDIDATEDI");
				AssertWhsDocket(changeOfInventoryBO.Receive, Data.Orgs.WUFSHIJNB, "B123", "B123-EDIDATEDI");
			}
		}

		#endregion

		#region TestPopulateBusinessObject_RegimeType

		public void TestPopulateBusinessObject_RegimeType_NotSpecified()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			Factory.SaveForTesting();

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: true, registerRegimeTypes: false))
			using (ObjectFactory.Substitute("WarehouseRegimeTypeProviders", new Hashtable()))
			{
				var warehouseClientDO = GetNewAddressData_INTHEMSYD(AddressTypes.WarehouseClient);
				Data.ShipmentDataObject.OrganizationAddressCollection.Add(warehouseClientDO);

				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				var changeOfInventoryBO = reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", changeOfInventoryBO);
				AssertWhsDocket(changeOfInventoryBO.Order, Data.Orgs.CRAHOLSYD, "B123", "B123-EDIDATEDI");
				AssertEquals("Should default to Bonded Warehouse Job.", false, changeOfInventoryBO.Order.WD_IsInwardsProcessingJob);
				AssertWhsDocket(changeOfInventoryBO.Receive, Data.Orgs.WUFSHIJNB, "B123", "B123-EDIDATEDI");
				AssertEquals("Should default to Bonded Warehouse Job.", false, changeOfInventoryBO.Receive.WD_IsInwardsProcessingJob);
			}
		}

		public void TestPopulateBusinessObject_RegimeType_BondedWarehouse()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			Factory.SaveForTesting();

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: true, outOfRegimeType: CustomsRegime.BondedWarehouse))
			{
				var warehouseClientDO = GetNewAddressData_INTHEMSYD(AddressTypes.WarehouseClient);
				Data.ShipmentDataObject.OrganizationAddressCollection.Add(warehouseClientDO);

				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				var changeOfInventoryBO = reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", changeOfInventoryBO);
				AssertWhsDocket(changeOfInventoryBO.Order, Data.Orgs.CRAHOLSYD, "B123", "B123-EDIDATEDI");
				AssertEquals("Should set Docket to Bonded Warehouse Job.", false, changeOfInventoryBO.Order.WD_IsInwardsProcessingJob);
				AssertWhsDocket(changeOfInventoryBO.Receive, Data.Orgs.WUFSHIJNB, "B123", "B123-EDIDATEDI");
				AssertEquals("Should set Docket to Bonded Warehouse Job.", false, changeOfInventoryBO.Receive.WD_IsInwardsProcessingJob);
			}
		}

		#endregion
	}

	class WhsBondedChangeOfInventoryDataObjectReader_VirtualWarehouse_BCO_Test : WhsBondedChangeOfInventoryDataObjectReader_BCO
	{
		protected override bool IsSourceWarehouseVirtual => true;

		#region TestPopulateBusinessObject_RegimeType

		public void TestPopulateBusinessObject_RegimeType_InwardsProcessing()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventoryAsInwardProcessing: true);
			Factory.SaveForTesting();

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: true, outOfRegimeType: CustomsRegime.InwardProcessing))
			{
				var warehouseClientDO = GetNewAddressData_INTHEMSYD(AddressTypes.WarehouseClient);
				Data.ShipmentDataObject.OrganizationAddressCollection.Add(warehouseClientDO);

				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				var changeOfInventoryBO = reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", changeOfInventoryBO);
				AssertWhsDocket(changeOfInventoryBO.Order, Data.Orgs.CRAHOLSYD, "B123", "B123-EDIDATEDI");
				AssertEquals("Should set Docket to Inwards Processing Job.", true, changeOfInventoryBO.Order.WD_IsInwardsProcessingJob);
				AssertWhsDocket(changeOfInventoryBO.Receive, Data.Orgs.WUFSHIJNB, "B123", "B123-EDIDATEDI");
				AssertEquals("Should set Docket to Inwards Processing Job.", true, changeOfInventoryBO.Receive.WD_IsInwardsProcessingJob);
			}
		}

		#endregion
	}

	class WhsBondedChangeOfInventoryDataObjectReader_RealWarehouse_BCO_Test : WhsBondedChangeOfInventoryDataObjectReader_BCO
	{
		protected override bool IsSourceWarehouseVirtual => false;
	}

	#endregion

	#region WhsBondedChangeOfInventoryDataObjectReaderTest

	abstract class WhsBondedChangeOfInventoryDataObjectReaderTest : WhsUniversalTestCase
	{
		#region TestPopulateBusinessObject_CreatesChangeOfInventory

		protected IPutawayEngineManagerForReceive SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()
		{
			var putawayEngineMock = PutawayEngineManagerForReceiveMockHandler.GetMockPutawayEngineManagerForReceive();
			return putawayEngineMock.Object;
		}

		public void TestPopulateBusinessObject_CreatesChangeOfInventory()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			Factory.SaveForTesting();

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: IsChangeOfOwnership))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				var changeOfInventoryBO = reader.ReadIntoBusinessObject();
				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", changeOfInventoryBO);
			}
		}

		#endregion

		#region TestPopulateBusinessObject_CreateOrderAndReceive

		public void TestPopulateBusinessObject_CreateOrderAndReceive()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			Factory.SaveForTesting();

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: IsChangeOfOwnership))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				var changeOfInventoryBO = reader.ReadIntoBusinessObject();
				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", changeOfInventoryBO);
				AssertNotNull("Should have read in a Receive.", changeOfInventoryBO.Receive);
				AssertNotNull("Should have read in a Order.", changeOfInventoryBO.Order);
			}
		}

		#endregion

		#region TestPopulateBusinessObject_CreateFinalisedOrderAndReceive

		public void TestPopulateBusinessObject_CreateFinalisedOrderAndReceive()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			Factory.SaveForTesting();

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: IsChangeOfOwnership))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				var changeOfInventoryBO = reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", changeOfInventoryBO);
				AssertWhsDocket(changeOfInventoryBO.Order, Data.Orgs.CRAHOLSYD, "B123", "B123-EDIDATEDI");
				AssertWhsDocket(changeOfInventoryBO.Receive, NewOwnerForTest, "B123", "B123-EDIDATEDI");
			}
		}

		#endregion

		#region TestPopulateBusinessObject_CreateFinalisedOrderAndReceive_NoFinaliseSecurityRight

		public void TestPopulateBusinessObject_CreateFinalisedOrderAndReceive_NoFinaliseSecurityRight()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(addImporterToXML: true, createInventory: true);
			Env.Security.WhsReleaseFinalise.IsAllowed = false;
			Factory.SaveForTesting();

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: IsChangeOfOwnership))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				var changeOfInventoryBO = reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", changeOfInventoryBO);
				AssertWhsDocket(changeOfInventoryBO.Order, Data.Orgs.CRAHOLSYD, "B123", "B123-EDIDATEDI");
				AssertWhsDocket(changeOfInventoryBO.Receive, NewOwnerForTest, "B123", "B123-EDIDATEDI");
			}
		}

		#endregion

		#region TestPopulateBusinessObject_PopulateNewOwnerDetails

		public void TestPopulateBusinessObject_PopulateNewOwnerDetails()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventory: false);
			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, Warehouse, "R1");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 100m, LocationA1, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.SaveForTesting();

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: IsChangeOfOwnership))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				var changeOfInventoryBO = reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", changeOfInventoryBO);
				AssertWhsDocket(changeOfInventoryBO.Order, Data.Orgs.CRAHOLSYD, "B123", "B123-EDIDATEDI");
				AssertWhsDocket(changeOfInventoryBO.Receive, NewOwnerForTest, "B123", "B123-EDIDATEDI");

				var originalReceiveLocation = receive.Lines.Single().Location;
				AssertLineBondedWarehouseAttributeMatchWithInventory(changeOfInventoryBO.Order.Lines.Single(), "P1", 5m, originalReceiveLocation, "ENTRYNUMBER123-2", "DummyOutward-1", 102, "RED", "Medium", "S1234");

				if (IsChangeOfOwnership)
				{
					AssertLine(changeOfInventoryBO.Receive.Lines.Single(), NewProductCodeForTest, 5m, originalReceiveLocation, "DUMMYOUTWARD-1-102", "DummyOutward-1", 102, "12", "34", "56");
				}
				else
				{
					AssertLine(changeOfInventoryBO.Receive.Lines.Single(), NewProductCodeForTest, 5m, originalReceiveLocation, "DUMMYOUTWARD-1-102", "DummyOutward-1", 102, "RED", "Medium", "S1234");
				}
			}
		}

		#endregion

		#region TestPopulateBusinessObject_MultipleProducts

		public void TestPopulateBusinessObject_MultipleProducts()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventory: false);

			// setup additional product for CRAHOLSYD
			var originalProduct2 = Helper.CreateProduct(Data.Orgs.CRAHOLSYD, "P2");
			Helper.SetProductAllAttributeUse(Data.Orgs.CRAHOLSYD, originalProduct2, use: true, setReleaseCaptured: false, useSerialNumber: false);

			// receive in those 2 products 
			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, Warehouse, "R1");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 100m, LocationA1, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, originalProduct2, 40m, LocationA2, ZDate.Empty, ZDate.Empty, "Blue", "Large", "S5678", "EntryNumber456-3");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(receive);

			// setup additional product for WUFSHIJNB
			var newProduct2 = Helper.CreateProduct(NewOwnerForTest, "W2");
			Helper.SetProductAllAttributeUse(NewOwnerForTest, newProduct2, use: true, setReleaseCaptured: false, useSerialNumber: false);
			Factory.SaveForTesting();

			var invoiceLine1 = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine();
			var invoiceLine2 = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine("P2", 40m, "BLUE", "Large", "S5678");

			var lineDetails1 = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(NewProductCodeForTest, invoiceLine1, newSerialNum: "");
			var lineDetails2 = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock("W2", invoiceLine2, "1", "2", "3", "EntryNumber456", 3, newSerialNum: "");

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(new[] { lineDetails1.Object, lineDetails2.Object }, isChangeOfOwnership: IsChangeOfOwnership))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				var changeOfInventoryBO = reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", changeOfInventoryBO);
				AssertWhsDocket(changeOfInventoryBO.Order, Data.Orgs.CRAHOLSYD, "B123", "B123-EDIDATEDI");
				AssertWhsDocket(changeOfInventoryBO.Receive, NewOwnerForTest, "B123", "B123-EDIDATEDI");

				AssertLineBondedWarehouseAttributeMatchWithInventory(changeOfInventoryBO.Order.Lines.First(l => l.ProductCode == "P1"), "P1", 5m, LocationA1, "ENTRYNUMBER123-2", "DummyOutward-1", 102, "RED", "Medium", "S1234");
				AssertLineBondedWarehouseAttributeMatchWithInventory(changeOfInventoryBO.Order.Lines.First(l => l.ProductCode == "P2"), "P2", 40m, LocationA2, "ENTRYNUMBER456-3", "DummyOutward-1", 102, "BLUE", "Large", "S5678");

				if (IsChangeOfOwnership)
				{
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.ProductCode == NewProductCodeForTest), NewProductCodeForTest, 5m, LocationA1, "DUMMYOUTWARD-1-102", "DummyOutward-1", 102, "12", "34", "56");
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.ProductCode == "W2"), "W2", 40m, LocationA2, "DUMMYOUTWARD-1-102", "DummyOutward-1", 102, "1", "2", "3");
				}
				else
				{
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.ProductCode == NewProductCodeForTest), NewProductCodeForTest, 5m, LocationA1, "DUMMYOUTWARD-1-102", "DummyOutward-1", 102, "RED", "Medium", "S1234");
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.ProductCode == "P2"), "P2", 40m, LocationA2, "DUMMYOUTWARD-1-102", "DummyOutward-1", 102, "BLUE", "Large", "S5678");
				}
			}
		}

		#endregion

		#region TestPopulateBusinessObject_MatchByEntryKey

		public void TestPopulateBusinessObject_MatchByEntryKey()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventory: false);

			// receive in those 4 lines 
			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, Warehouse, "R1");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 100m, LocationA1, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber111-1");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 100m, LocationA2, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber222-2");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 100m, LocationA3, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber222-3");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 100m, LocationA4, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber333-4");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(receive);

			var invoiceLine1 = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine(orderedQty: 5);
			var invoiceLine2 = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine(orderedQty: 10);
			var invoiceLine3 = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine(orderedQty: 15);
			var invoiceLine4 = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine(orderedQty: 20);

			var lineDetails1 = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(NewProductCodeForTest, invoiceLine1, previousEntryNo: "EntryNumber111", previousEntryLineNo: 1, entryNumber: "OUT", entryLineNo: 5, newSerialNum: "");
			var lineDetails2 = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(NewProductCodeForTest, invoiceLine2, previousEntryNo: "EntryNumber222", previousEntryLineNo: 2, entryNumber: "OUT", entryLineNo: 6, newSerialNum: "");
			var lineDetails3 = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(NewProductCodeForTest, invoiceLine3, previousEntryNo: "EntryNumber222", previousEntryLineNo: 3, entryNumber: "OUT", entryLineNo: 7, newSerialNum: "");
			var lineDetails4 = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(NewProductCodeForTest, invoiceLine4, previousEntryNo: "EntryNumber333", previousEntryLineNo: 4, entryNumber: "OUT", entryLineNo: 8, newSerialNum: "");

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(new[] { lineDetails1.Object, lineDetails2.Object, lineDetails3.Object, lineDetails4.Object }, isChangeOfOwnership: IsChangeOfOwnership))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				var changeOfInventoryBO = reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", changeOfInventoryBO);
				AssertWhsDocket(changeOfInventoryBO.Order, Data.Orgs.CRAHOLSYD, "B123", "B123-EDIDATEDI");
				AssertWhsDocket(changeOfInventoryBO.Receive, NewOwnerForTest, "B123", "B123-EDIDATEDI");

				AssertLineBondedWarehouseAttributeMatchWithInventory(changeOfInventoryBO.Order.Lines.First(l => l.WE_BondedEntryKey == "ENTRYNUMBER111-1"), "P1", 5m, LocationA1, "ENTRYNUMBER111-1", "OUT", 5, "RED", "Medium", "S1234");
				AssertLineBondedWarehouseAttributeMatchWithInventory(changeOfInventoryBO.Order.Lines.First(l => l.WE_BondedEntryKey == "ENTRYNUMBER222-2"), "P1", 10m, LocationA2, "ENTRYNUMBER222-2", "OUT", 6, "RED", "Medium", "S1234");
				AssertLineBondedWarehouseAttributeMatchWithInventory(changeOfInventoryBO.Order.Lines.First(l => l.WE_BondedEntryKey == "ENTRYNUMBER222-3"), "P1", 15m, LocationA3, "ENTRYNUMBER222-3", "OUT", 7, "RED", "Medium", "S1234");
				AssertLineBondedWarehouseAttributeMatchWithInventory(changeOfInventoryBO.Order.Lines.First(l => l.WE_BondedEntryKey == "ENTRYNUMBER333-4"), "P1", 20m, LocationA4, "ENTRYNUMBER333-4", "OUT", 8, "RED", "Medium", "S1234");

				if (IsChangeOfOwnership)
				{
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_BondedEntryKey == "OUT-5"), NewProductCodeForTest, 5m, LocationA1, "OUT-5", "OUT", 5, "12", "34", "56");
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_BondedEntryKey == "OUT-6"), NewProductCodeForTest, 10m, LocationA2, "OUT-6", "OUT", 6, "12", "34", "56");
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_BondedEntryKey == "OUT-7"), NewProductCodeForTest, 15m, LocationA3, "OUT-7", "OUT", 7, "12", "34", "56");
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_BondedEntryKey == "OUT-8"), NewProductCodeForTest, 20m, LocationA4, "OUT-8", "OUT", 8, "12", "34", "56");
				}
				else
				{
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_TransactionQuantity == 5m), NewProductCodeForTest, 5m, LocationA1, "OUT-5", "OUT", 5, "RED", "Medium", "S1234");
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_TransactionQuantity == 10m), NewProductCodeForTest, 10m, LocationA2, "OUT-6", "OUT", 6, "RED", "Medium", "S1234");
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_TransactionQuantity == 15m), NewProductCodeForTest, 15m, LocationA3, "OUT-7", "OUT", 7, "RED", "Medium", "S1234");
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_TransactionQuantity == 20m), NewProductCodeForTest, 20m, LocationA4, "OUT-8", "OUT", 8, "RED", "Medium", "S1234");
				}
			}
		}

		#endregion

		#region TestPopulateBusinessObject_MultipleLocations

		public void TestPopulateBusinessObject_MultipleLocations()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventory: false);

			// receive in those 3 lines 
			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, Warehouse, "R1");
			var receiveLine1 = CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 5m, LocationA1, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber111-1");
			var receiveLine2 = CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 10m, LocationA2, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber111-1");
			var receiveLine3 = CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 100m, LocationA3, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber111-1");
			receive.FinaliseDocketWithoutUserConfirmation();
			// make sure FIFO allocation works
			receiveLine1.WI_ArrivalDate = receive.WD_ArrivalDate.AddMinutes(1);
			receiveLine2.WI_ArrivalDate = receive.WD_ArrivalDate.AddMinutes(2);
			receiveLine3.WI_ArrivalDate = receive.WD_ArrivalDate.AddMinutes(3);
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(receive);

			var invoiceLine = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine(orderedQty: 35);
			var lineDetails = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(NewProductCodeForTest, invoiceLine, previousEntryNo: "EntryNumber111", previousEntryLineNo: 1, entryNumber: "OUT", entryLineNo: 5, newSerialNum: "");

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(new[] { lineDetails.Object }, isChangeOfOwnership: IsChangeOfOwnership))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				var changeOfInventoryBO = reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", changeOfInventoryBO);
				AssertWhsDocket(changeOfInventoryBO.Order, Data.Orgs.CRAHOLSYD, "B123", "B123-EDIDATEDI");
				AssertWhsDocket(changeOfInventoryBO.Receive, NewOwnerForTest, "B123", "B123-EDIDATEDI");

				if (IsSourceWarehouseVirtual)
				{
					AssertLineBondedWarehouseAttributeMatchWithInventory(changeOfInventoryBO.Order.Lines.Single(l => l.WE_TransactionQuantity == 5m), "P1", 5m, LocationA1, "ENTRYNUMBER111-1", "OUT", 5, "RED", "Medium", "S1234", (l) => l.PickLines.Single(p => p.WZ_Units == 5m).Inventory.Location, lineNo: 1);
					AssertLineBondedWarehouseAttributeMatchWithInventory(changeOfInventoryBO.Order.Lines.Single(l => l.WE_TransactionQuantity == 10m), "P1", 10m, LocationA2, "ENTRYNUMBER111-1", "OUT", 5, "RED", "Medium", "S1234", (l) => l.PickLines.Single(p => p.WZ_Units == 10m).Inventory.Location, lineNo: 2);
					AssertLineBondedWarehouseAttributeMatchWithInventory(changeOfInventoryBO.Order.Lines.Single(l => l.WE_TransactionQuantity == 20m), "P1", 20m, LocationA3, "ENTRYNUMBER111-1", "OUT", 5, "RED", "Medium", "S1234", (l) => l.PickLines.Single(p => p.WZ_Units == 20m).Inventory.Location, lineNo: 3);
				}
				else
				{
					AssertLineBondedWarehouseAttributeMatchWithInventory(changeOfInventoryBO.Order.Lines.Single(), "P1", 35m, LocationA1, "ENTRYNUMBER111-1", "OUT", 5, "RED", "Medium", "S1234", (l) => l.PickLines.Single(p => p.WZ_Units == 5m).Inventory.Location);
					AssertLineBondedWarehouseAttributeMatchWithInventory(changeOfInventoryBO.Order.Lines.Single(), "P1", 35m, LocationA2, "ENTRYNUMBER111-1", "OUT", 5, "RED", "Medium", "S1234", (l) => l.PickLines.Single(p => p.WZ_Units == 10m).Inventory.Location);
					AssertLineBondedWarehouseAttributeMatchWithInventory(changeOfInventoryBO.Order.Lines.Single(), "P1", 35m, LocationA3, "ENTRYNUMBER111-1", "OUT", 5, "RED", "Medium", "S1234", (l) => l.PickLines.Single(p => p.WZ_Units == 20m).Inventory.Location);
				}

				if (IsChangeOfOwnership)
				{
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_TransactionQuantity == 5m), NewProductCodeForTest, 5m, LocationA1, "OUT-5", "OUT", 5, "12", "34", "56");
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_TransactionQuantity == 10m), NewProductCodeForTest, 10m, LocationA2, "OUT-5", "OUT", 5, "12", "34", "56");
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_TransactionQuantity == 20m), NewProductCodeForTest, 20m, LocationA3, "OUT-5", "OUT", 5, "12", "34", "56");
				}
				else
				{
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_TransactionQuantity == 5m), NewProductCodeForTest, 5m, LocationA1, "OUT-5", "OUT", 5, "RED", "Medium", "S1234");
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_TransactionQuantity == 10m), NewProductCodeForTest, 10m, LocationA2, "OUT-5", "OUT", 5, "RED", "Medium", "S1234");
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_TransactionQuantity == 20m), NewProductCodeForTest, 20m, LocationA3, "OUT-5", "OUT", 5, "RED", "Medium", "S1234");
				}
			}
		}

		#endregion

		#region TestPopulateBusinessObject_OnHold_OnlySaveOrderIfHeld

		public void TestPopulateBusinessObject_OnHold_OnlySaveOrderIfHeld()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(recipientRoles: new[] { new RecipientRoleDetail { Type = RecipientRoleForTest, ServiceCode = ServiceCodeType.HLD } });
			Factory.SaveForTesting();
			AssertEquals("Precondition - hold service code is set for recipient role ", ServiceCodeType.HLD, Logger.TopLevelDataObject.DataContext.RecipientRoleCollection.First(r => r.Code == RecipientRoleForTest).ServiceCode);

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: IsChangeOfOwnership))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				var changeOfInventoryBO = reader.ReadIntoBusinessObject();

				Factory.SaveForTesting();

				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", changeOfInventoryBO);
				AssertWhsDocket(changeOfInventoryBO.Order, Data.Orgs.CRAHOLSYD, "B123", "B123-EDIDATEDI");
				AssertWhsDocket(changeOfInventoryBO.Receive, NewOwnerForTest, "B123", "B123-EDIDATEDI", isSaved: false);
			}
		}

		#endregion

		#region TestPopulateBusinessObject_Failed_NotEnoughInventory

		public void TestPopulateBusinessObject_Failed_NotEnoughInventory()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventory: false);

			// create inventory 
			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, Warehouse, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, Data.ProductCRAHOLSYD, 100m, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(receive);

			// request to change owner on 1 extra product
			var invoiceLine1 = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine(orderedQty: 101);
			var lineDetails1 = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(NewProductCodeForTest, invoiceLine1, "1", "2", "3");

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(new[] { lineDetails1.Object }, isChangeOfOwnership: IsChangeOfOwnership))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				AssertExceptionThrown("Not enough stock.", typeof(DataObjectReadFailureException), @"
Cannot Import Order
Order could not be created for Customs Job B123 because there are errors:
You do not have enough stock to fulfill shortfalls on this order
ENTRYNUMBER123-2 Product P1/P1 can not be ordered due to lack of stock. 101 was ordered, but 100 is available
".Trim(), () => reader.ReadIntoBusinessObject());
			}
		}

		#endregion

		#region TestPopulateBusinessObject_Failed_US

		public void TestPopulateBusinessObject_Failed_US()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB();

				using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: IsChangeOfOwnership))
				{
					var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
					AssertExceptionThrown("We do not support US yet. Pack Detail complexities.", typeof(DataObjectReadFailureException), @"
Change of Inventory for US Customs is not supported.
".Trim(), () => reader.ReadIntoBusinessObject());
				}
			}
		}

		#endregion

		#region TestPopulateBusinessObject_MatchByAttrib1

		public void TestPopulateBusinessObject_MatchByAttrib1()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventory: false);

			// receive in those 4 lines 
			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, Warehouse, "R1");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 100m, LocationA1, ZDate.Empty, ZDate.Empty, "RED", "Medium", "S1234", "EntryNumber111-1");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 100m, LocationA2, ZDate.Empty, ZDate.Empty, "BLUE", "Medium", "S1234", "EntryNumber111-1");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 100m, LocationA3, ZDate.Empty, ZDate.Empty, "GREEN", "Medium", "S1234", "EntryNumber111-1");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 100m, LocationA4, ZDate.Empty, ZDate.Empty, "YELLOW", "Medium", "S1234", "EntryNumber111-1");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(receive);

			var invoiceLine1 = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine(orderedQty: 5, previousAttrib1: "RED");
			var invoiceLine2 = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine(orderedQty: 10, previousAttrib1: "BLUE");
			var invoiceLine3 = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine(orderedQty: 15, previousAttrib1: "GREEN");
			var invoiceLine4 = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine(orderedQty: 20, previousAttrib1: "YELLOW");

			var lineDetails1 = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(NewProductCodeForTest, invoiceLine1, previousEntryNo: "EntryNumber111", previousEntryLineNo: 1, entryNumber: "OUT", entryLineNo: 5, newSerialNum: "");
			var lineDetails2 = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(NewProductCodeForTest, invoiceLine2, previousEntryNo: "EntryNumber111", previousEntryLineNo: 1, entryNumber: "OUT", entryLineNo: 6, newSerialNum: "");
			var lineDetails3 = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(NewProductCodeForTest, invoiceLine3, previousEntryNo: "EntryNumber111", previousEntryLineNo: 1, entryNumber: "OUT", entryLineNo: 7, newSerialNum: "");
			var lineDetails4 = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(NewProductCodeForTest, invoiceLine4, previousEntryNo: "EntryNumber111", previousEntryLineNo: 1, entryNumber: "OUT", entryLineNo: 8, newSerialNum: "");

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (new WarehouseCustomsDetailsProvidersMocks(new[] { lineDetails1.Object, lineDetails2.Object, lineDetails3.Object, lineDetails4.Object }, isChangeOfOwnership: IsChangeOfOwnership))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				var changeOfInventoryBO = reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", changeOfInventoryBO);
				AssertWhsDocket(changeOfInventoryBO.Order, Data.Orgs.CRAHOLSYD, "B123", "B123-EDIDATEDI");
				AssertWhsDocket(changeOfInventoryBO.Receive, NewOwnerForTest, "B123", "B123-EDIDATEDI");

				AssertLineBondedWarehouseAttributeMatchWithInventory(changeOfInventoryBO.Order.Lines.First(l => l.WE_PartAttrib1 == "RED"), "P1", 5m, LocationA1, "ENTRYNUMBER111-1", "OUT", 5, "RED", "Medium", "S1234");
				AssertLineBondedWarehouseAttributeMatchWithInventory(changeOfInventoryBO.Order.Lines.First(l => l.WE_PartAttrib1 == "BLUE"), "P1", 10m, LocationA2, "ENTRYNUMBER111-1", "OUT", 6, "BLUE", "Medium", "S1234");
				AssertLineBondedWarehouseAttributeMatchWithInventory(changeOfInventoryBO.Order.Lines.First(l => l.WE_PartAttrib1 == "GREEN"), "P1", 15m, LocationA3, "ENTRYNUMBER111-1", "OUT", 7, "GREEN", "Medium", "S1234");
				AssertLineBondedWarehouseAttributeMatchWithInventory(changeOfInventoryBO.Order.Lines.First(l => l.WE_PartAttrib1 == "YELLOW"), "P1", 20m, LocationA4, "ENTRYNUMBER111-1", "OUT", 8, "YELLOW", "Medium", "S1234");

				if (IsChangeOfOwnership)
				{
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_BondedEntryKey == "OUT-5"), NewProductCodeForTest, 5m, LocationA1, "OUT-5", "OUT", 5, "12", "34", "56");
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_BondedEntryKey == "OUT-6"), NewProductCodeForTest, 10m, LocationA2, "OUT-6", "OUT", 6, "12", "34", "56");
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_BondedEntryKey == "OUT-7"), NewProductCodeForTest, 15m, LocationA3, "OUT-7", "OUT", 7, "12", "34", "56");
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_BondedEntryKey == "OUT-8"), NewProductCodeForTest, 20m, LocationA4, "OUT-8", "OUT", 8, "12", "34", "56");
				}
				else
				{
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_TransactionQuantity == 5m), NewProductCodeForTest, 5m, LocationA1, "OUT-5", "OUT", 5, "RED", "Medium", "S1234");
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_TransactionQuantity == 10m), NewProductCodeForTest, 10m, LocationA2, "OUT-6", "OUT", 6, "BLUE", "Medium", "S1234");
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_TransactionQuantity == 15m), NewProductCodeForTest, 15m, LocationA3, "OUT-7", "OUT", 7, "GREEN", "Medium", "S1234");
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_TransactionQuantity == 20m), NewProductCodeForTest, 20m, LocationA4, "OUT-8", "OUT", 8, "YELLOW", "Medium", "S1234");
				}
			}
		}

		#endregion

		#region TestPopulateBusinessObject_MatchByAttrib2

		public void TestPopulateBusinessObject_MatchByAttrib2()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventory: false);

			// receive in those 4 lines 
			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, Warehouse, "R1");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 100m, LocationA1, ZDate.Empty, ZDate.Empty, "RED", "Small", "S1234", "EntryNumber111-1");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 100m, LocationA2, ZDate.Empty, ZDate.Empty, "RED", "Medium", "S1234", "EntryNumber111-1");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 100m, LocationA3, ZDate.Empty, ZDate.Empty, "RED", "Large", "S1234", "EntryNumber111-1");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 100m, LocationA4, ZDate.Empty, ZDate.Empty, "RED", "XLarge", "S1234", "EntryNumber111-1");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(receive);

			var invoiceLine1 = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine(orderedQty: 5, previousAttrib2: "Small");
			var invoiceLine2 = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine(orderedQty: 10, previousAttrib2: "Medium");
			var invoiceLine3 = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine(orderedQty: 15, previousAttrib2: "Large");
			var invoiceLine4 = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine(orderedQty: 20, previousAttrib2: "XLarge");

			var lineDetails1 = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(NewProductCodeForTest, invoiceLine1, previousEntryNo: "EntryNumber111", previousEntryLineNo: 1, entryNumber: "OUT", entryLineNo: 5, newSerialNum: "");
			var lineDetails2 = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(NewProductCodeForTest, invoiceLine2, previousEntryNo: "EntryNumber111", previousEntryLineNo: 1, entryNumber: "OUT", entryLineNo: 6, newSerialNum: "");
			var lineDetails3 = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(NewProductCodeForTest, invoiceLine3, previousEntryNo: "EntryNumber111", previousEntryLineNo: 1, entryNumber: "OUT", entryLineNo: 7, newSerialNum: "");
			var lineDetails4 = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(NewProductCodeForTest, invoiceLine4, previousEntryNo: "EntryNumber111", previousEntryLineNo: 1, entryNumber: "OUT", entryLineNo: 8, newSerialNum: "");

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (new WarehouseCustomsDetailsProvidersMocks(new[] { lineDetails1.Object, lineDetails2.Object, lineDetails3.Object, lineDetails4.Object }, isChangeOfOwnership: IsChangeOfOwnership))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				var changeOfInventoryBO = reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", changeOfInventoryBO);
				AssertWhsDocket(changeOfInventoryBO.Order, Data.Orgs.CRAHOLSYD, "B123", "B123-EDIDATEDI");
				AssertWhsDocket(changeOfInventoryBO.Receive, NewOwnerForTest, "B123", "B123-EDIDATEDI");

				AssertLineBondedWarehouseAttributeMatchWithInventory(changeOfInventoryBO.Order.Lines.First(l => l.WE_PartAttrib2 == "Small"), "P1", 5m, LocationA1, "ENTRYNUMBER111-1", "OUT", 5, "RED", "Small", "S1234");
				AssertLineBondedWarehouseAttributeMatchWithInventory(changeOfInventoryBO.Order.Lines.First(l => l.WE_PartAttrib2 == "Medium"), "P1", 10m, LocationA2, "ENTRYNUMBER111-1", "OUT", 6, "RED", "Medium", "S1234");
				AssertLineBondedWarehouseAttributeMatchWithInventory(changeOfInventoryBO.Order.Lines.First(l => l.WE_PartAttrib2 == "Large"), "P1", 15m, LocationA3, "ENTRYNUMBER111-1", "OUT", 7, "RED", "Large", "S1234");
				AssertLineBondedWarehouseAttributeMatchWithInventory(changeOfInventoryBO.Order.Lines.First(l => l.WE_PartAttrib2 == "XLarge"), "P1", 20m, LocationA4, "ENTRYNUMBER111-1", "OUT", 8, "RED", "XLarge", "S1234");

				if (IsChangeOfOwnership)
				{
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_BondedEntryKey == "OUT-5"), NewProductCodeForTest, 5m, LocationA1, "OUT-5", "OUT", 5, "12", "34", "56");
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_BondedEntryKey == "OUT-6"), NewProductCodeForTest, 10m, LocationA2, "OUT-6", "OUT", 6, "12", "34", "56");
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_BondedEntryKey == "OUT-7"), NewProductCodeForTest, 15m, LocationA3, "OUT-7", "OUT", 7, "12", "34", "56");
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_BondedEntryKey == "OUT-8"), NewProductCodeForTest, 20m, LocationA4, "OUT-8", "OUT", 8, "12", "34", "56");
				}
				else
				{
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_TransactionQuantity == 5m), NewProductCodeForTest, 5m, LocationA1, "OUT-5", "OUT", 5, "RED", "Small", "S1234");
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_TransactionQuantity == 10m), NewProductCodeForTest, 10m, LocationA2, "OUT-6", "OUT", 6, "RED", "Medium", "S1234");
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_TransactionQuantity == 15m), NewProductCodeForTest, 15m, LocationA3, "OUT-7", "OUT", 7, "RED", "Large", "S1234");
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_TransactionQuantity == 20m), NewProductCodeForTest, 20m, LocationA4, "OUT-8", "OUT", 8, "RED", "XLarge", "S1234");
				}
			}
		}

		#endregion

		#region TestPopulateBusinessObject_MatchByAttrib3

		public void TestPopulateBusinessObject_MatchByAttrib3()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventory: false);

			// receive in those 4 lines 
			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, Warehouse, "R1");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 100m, LocationA1, ZDate.Empty, ZDate.Empty, "RED", "Medium", "S1", "EntryNumber111-1");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 100m, LocationA2, ZDate.Empty, ZDate.Empty, "RED", "Medium", "S2", "EntryNumber111-1");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 100m, LocationA3, ZDate.Empty, ZDate.Empty, "RED", "Medium", "S3", "EntryNumber111-1");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 100m, LocationA4, ZDate.Empty, ZDate.Empty, "RED", "Medium", "S4", "EntryNumber111-1");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.SaveForTesting();
			AssertIsFinalisedPrecondition(receive);

			var invoiceLine1 = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine(orderedQty: 5, previousAttrib3: "S1");
			var invoiceLine2 = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine(orderedQty: 10, previousAttrib3: "S2");
			var invoiceLine3 = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine(orderedQty: 15, previousAttrib3: "S3");
			var invoiceLine4 = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine(orderedQty: 20, previousAttrib3: "S4");

			var lineDetails1 = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(NewProductCodeForTest, invoiceLine1, previousEntryNo: "EntryNumber111", previousEntryLineNo: 1, entryNumber: "OUT", entryLineNo: 5, newSerialNum: "");
			var lineDetails2 = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(NewProductCodeForTest, invoiceLine2, previousEntryNo: "EntryNumber111", previousEntryLineNo: 1, entryNumber: "OUT", entryLineNo: 6, newSerialNum: "");
			var lineDetails3 = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(NewProductCodeForTest, invoiceLine3, previousEntryNo: "EntryNumber111", previousEntryLineNo: 1, entryNumber: "OUT", entryLineNo: 7, newSerialNum: "");
			var lineDetails4 = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(NewProductCodeForTest, invoiceLine4, previousEntryNo: "EntryNumber111", previousEntryLineNo: 1, entryNumber: "OUT", entryLineNo: 8, newSerialNum: "");

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (new WarehouseCustomsDetailsProvidersMocks(new[] { lineDetails1.Object, lineDetails2.Object, lineDetails3.Object, lineDetails4.Object }, isChangeOfOwnership: IsChangeOfOwnership))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				var changeOfInventoryBO = reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", changeOfInventoryBO);
				AssertWhsDocket(changeOfInventoryBO.Order, Data.Orgs.CRAHOLSYD, "B123", "B123-EDIDATEDI");
				AssertWhsDocket(changeOfInventoryBO.Receive, NewOwnerForTest, "B123", "B123-EDIDATEDI");

				AssertLineBondedWarehouseAttributeMatchWithInventory(changeOfInventoryBO.Order.Lines.First(l => l.WE_PartAttrib3 == "S1"), "P1", 5m, LocationA1, "ENTRYNUMBER111-1", "OUT", 5, "RED", "Medium", "S1");
				AssertLineBondedWarehouseAttributeMatchWithInventory(changeOfInventoryBO.Order.Lines.First(l => l.WE_PartAttrib3 == "S2"), "P1", 10m, LocationA2, "ENTRYNUMBER111-1", "OUT", 6, "RED", "Medium", "S2");
				AssertLineBondedWarehouseAttributeMatchWithInventory(changeOfInventoryBO.Order.Lines.First(l => l.WE_PartAttrib3 == "S3"), "P1", 15m, LocationA3, "ENTRYNUMBER111-1", "OUT", 7, "RED", "Medium", "S3");
				AssertLineBondedWarehouseAttributeMatchWithInventory(changeOfInventoryBO.Order.Lines.First(l => l.WE_PartAttrib3 == "S4"), "P1", 20m, LocationA4, "ENTRYNUMBER111-1", "OUT", 8, "RED", "Medium", "S4");

				if (IsChangeOfOwnership)
				{
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_BondedEntryKey == "OUT-5"), NewProductCodeForTest, 5m, LocationA1, "OUT-5", "OUT", 5, "12", "34", "56");
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_BondedEntryKey == "OUT-6"), NewProductCodeForTest, 10m, LocationA2, "OUT-6", "OUT", 6, "12", "34", "56");
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_BondedEntryKey == "OUT-7"), NewProductCodeForTest, 15m, LocationA3, "OUT-7", "OUT", 7, "12", "34", "56");
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_BondedEntryKey == "OUT-8"), NewProductCodeForTest, 20m, LocationA4, "OUT-8", "OUT", 8, "12", "34", "56");
				}
				else
				{
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_TransactionQuantity == 5m), NewProductCodeForTest, 5m, LocationA1, "OUT-5", "OUT", 5, "RED", "Medium", "S1");
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_TransactionQuantity == 10m), NewProductCodeForTest, 10m, LocationA2, "OUT-6", "OUT", 6, "RED", "Medium", "S2");
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_TransactionQuantity == 15m), NewProductCodeForTest, 15m, LocationA3, "OUT-7", "OUT", 7, "RED", "Medium", "S3");
					AssertLine(changeOfInventoryBO.Receive.Lines.First(l => l.WE_TransactionQuantity == 20m), NewProductCodeForTest, 20m, LocationA4, "OUT-8", "OUT", 8, "RED", "Medium", "S4");
				}
			}
		}

		#endregion

		#region TestPopulateBusinessObject_Amend_FinalisedOrderAndReceive

		[GuiTest]
		public void TestPopulateBusinessObject_Amend_FinalisedOrderAndReceive()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventory: false);
			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, Warehouse, "R1");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 100m, LocationA1, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.SaveForTesting();

			WhsBondedChangeOfInventory firstChangeOfInventory;

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: IsChangeOfOwnership))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				firstChangeOfInventory = reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", firstChangeOfInventory);
				AssertWhsDocket(firstChangeOfInventory.Order, Data.Orgs.CRAHOLSYD, "B123", "B123-EDIDATEDI");
				AssertWhsDocket(firstChangeOfInventory.Receive, NewOwnerForTest, "B123", "B123-EDIDATEDI");

				var originalReceiveLocation = receive.Lines.Single().Location;
				AssertLineBondedWarehouseAttributeMatchWithInventory(firstChangeOfInventory.Order.Lines.Single(), "P1", 5m, originalReceiveLocation, "ENTRYNUMBER123-2", "DummyOutward-1", 102, "RED", "Medium", "S1234");
				if (IsChangeOfOwnership)
				{
					AssertLine(firstChangeOfInventory.Receive.Lines.Single(), NewProductCodeForTest, 5m, originalReceiveLocation, "DUMMYOUTWARD-1-102", "DummyOutward-1", 102, "12", "34", "56");
				}
				else
				{
					AssertLine(firstChangeOfInventory.Receive.Lines.Single(), NewProductCodeForTest, 5m, originalReceiveLocation, "DUMMYOUTWARD-1-102", "DummyOutward-1", 102, "RED", "Medium", "S1234");
				}
			}

			FinaliseDocketsFromImport(firstChangeOfInventory);

			// ammend qty
			var line = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine(orderedQty: 6);
			var lineDetails = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(NewProductCodeForTest, invoiceLine: line);
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(customsLineDetails: new[] { lineDetails.Object }, isChangeOfOwnership: IsChangeOfOwnership, intoRegimeType: CustomsRegime.InwardProcessing))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);

				if (IsSourceWarehouseVirtual || IsChangeOfOwnership)
				{
					var amendedChangeOfInventoryBO = reader.ReadIntoBusinessObject();
					Factory.SaveForTesting();

					AssertNotEquals("Should have cancelled the previous Order and created a new one.", firstChangeOfInventory.Order.PK, amendedChangeOfInventoryBO.Order.PK);
					AssertEquals("Should have amended the same Receive.", firstChangeOfInventory.Receive.PK, amendedChangeOfInventoryBO.Receive.PK);

					var orderInOtherFactory = new BusinessObjectFactory().Load<WhsDocket>(firstChangeOfInventory.Order.PK);
					AssertNotNull(orderInOtherFactory.Logs.Find(l => l.SL_SE_NKEvent == Events.CancelledCode).SingleOrDefault());
					AssertEquals("Amendment should have cancelled Order.", true, orderInOtherFactory.IsCancelled);
					AssertNull("Amendment should have cancelled Pick.", ((WhsOrder)orderInOtherFactory).Pick);

					AssertWhsDocket(amendedChangeOfInventoryBO.Order, Data.Orgs.CRAHOLSYD, "B123", "B123-EDIDATEDI", splitNumber: 1);
					AssertWhsDocket(amendedChangeOfInventoryBO.Receive, NewOwnerForTest, "B123", "B123-EDIDATEDI");

					var originalReceiveLocation = receive.Lines.Single().Location;
					AssertLineBondedWarehouseAttributeMatchWithInventory(amendedChangeOfInventoryBO.Order.Lines.Single(), "P1", 6m, originalReceiveLocation, "ENTRYNUMBER123-2", "DummyOutward-1", 102, "RED", "Medium", "S1234");
					if (IsChangeOfOwnership)
					{
						AssertLine(amendedChangeOfInventoryBO.Receive.Lines.Single(), NewProductCodeForTest, 6m, originalReceiveLocation, "DUMMYOUTWARD-1-102", "DummyOutward-1", 102, "12", "34", "56");
					}
					else
					{
						AssertLine(amendedChangeOfInventoryBO.Receive.Lines.Single(), NewProductCodeForTest, 6m, originalReceiveLocation, "DUMMYOUTWARD-1-102", "DummyOutward-1", 102, "RED", "Medium", "S1234");
					}
				}
				else
				{
					AssertExceptionThrown("Already finalised, can't make changes to qty.", typeof(DataObjectReadFailureException), @"
Cannot amend warehouse job for stock movement if original warehouse job is finalized in Physical Warehouse.
	".Trim(), () => reader.ReadIntoBusinessObject());
				}
			}
		}

		#endregion

		#region TestPopulateBusinessObject_Amend_FinalisedOrder

		[GuiTest]
		public void TestPopulateBusinessObject_Amend_FinalisedOrder()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventory: false, recipientRoles: new[] { new RecipientRoleDetail { Type = RecipientRoleForTest, ServiceCode = ServiceCodeType.HLD } });
			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, Warehouse, "R1");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 100m, LocationA1, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.SaveForTesting();
			AssertEquals("Precondition - hold service code is set for recipient role ", ServiceCodeType.HLD, Logger.TopLevelDataObject.DataContext.RecipientRoleCollection.First(r => r.Code == RecipientRoleForTest).ServiceCode);

			WhsBondedChangeOfInventory firstChangeOfInventory;

			var putawayEngineMock = SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK();
			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(putawayEngineMock))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: IsChangeOfOwnership))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				firstChangeOfInventory = reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", firstChangeOfInventory);
				AssertWhsDocket(firstChangeOfInventory.Order, Data.Orgs.CRAHOLSYD, "B123", "B123-EDIDATEDI");
				AssertWhsDocket(firstChangeOfInventory.Receive, NewOwnerForTest, "B123", "B123-EDIDATEDI", isSaved: false);

				var originalReceiveLocation = receive.Lines.Single().Location;
				AssertLineBondedWarehouseAttributeMatchWithInventory(firstChangeOfInventory.Order.Lines.Single(), "P1", 5m, originalReceiveLocation, "ENTRYNUMBER123-2", "DummyOutward-1", 102, "RED", "Medium", "S1234");

				if (IsChangeOfOwnership)
				{
					AssertLine(firstChangeOfInventory.Receive.Lines.Single(), NewProductCodeForTest, 5m, originalReceiveLocation, "DUMMYOUTWARD-1-102", "DummyOutward-1", 102, "12", "34", "56");
				}
				else
				{
					AssertLine(firstChangeOfInventory.Receive.Lines.Single(), NewProductCodeForTest, 5m, originalReceiveLocation, "DUMMYOUTWARD-1-102", "DummyOutward-1", 102, "RED", "Medium", "S1234");
				}
			}

			FinaliseDocketsFromImport(firstChangeOfInventory);

			// send without HELD code
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventory: false);
			Factory.SaveForTesting();

			// ammend qty
			var line = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine(orderedQty: 6);
			var lineDetails = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(invoiceLine: line, newProductCode: NewProductCodeForTest, newSerialNum: "");

			using (ObjectFactory.Substitute(putawayEngineMock))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(customsLineDetails: new[] { lineDetails.Object }, isChangeOfOwnership: IsChangeOfOwnership, intoRegimeType: CustomsRegime.InwardProcessing))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				if (IsSourceWarehouseVirtual || IsChangeOfOwnership)
				{
					var amendedChangeOfInventoryBO = reader.ReadIntoBusinessObject();
					Factory.SaveForTesting();

					AssertNotEquals("Should have cancelled the previous Order and created a new one.", firstChangeOfInventory.Order.PK, amendedChangeOfInventoryBO.Order.PK);
					AssertNotEquals("Should have amended another Receive as the first one wasn't saved.", firstChangeOfInventory.Receive.PK, amendedChangeOfInventoryBO.Receive.PK);

					var orderInOtherFactory = new BusinessObjectFactory().Load<WhsDocket>(firstChangeOfInventory.Order.PK);
					AssertNotNull(orderInOtherFactory.Logs.Find(l => l.SL_SE_NKEvent == Events.CancelledCode).SingleOrDefault());
					AssertEquals("Amendment should have cancelled Order.", true, orderInOtherFactory.IsCancelled);
					AssertNull("Amendment should have cancelled Pick.", ((WhsOrder)orderInOtherFactory).Pick);

					AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", amendedChangeOfInventoryBO);
					AssertWhsDocket(amendedChangeOfInventoryBO.Order, Data.Orgs.CRAHOLSYD, "B123", "B123-EDIDATEDI", splitNumber: 1);
					AssertWhsDocket(amendedChangeOfInventoryBO.Receive, NewOwnerForTest, "B123", "B123-EDIDATEDI");

					var originalReceiveLocation = receive.Lines.Single().Location;
					AssertLineBondedWarehouseAttributeMatchWithInventory(amendedChangeOfInventoryBO.Order.Lines.Single(), "P1", 6m, originalReceiveLocation, "ENTRYNUMBER123-2", "DummyOutward-1", 102, "RED", "Medium", "S1234");
					if (IsChangeOfOwnership)
					{
						AssertLine(amendedChangeOfInventoryBO.Receive.Lines.Single(), NewProductCodeForTest, 6m, originalReceiveLocation, "DUMMYOUTWARD-1-102", "DummyOutward-1", 102, "12", "34", "56");
					}
					else
					{
						AssertLine(amendedChangeOfInventoryBO.Receive.Lines.Single(), NewProductCodeForTest, 6m, Warehouse.DefaultLocationInInwardProcessingArea, "DUMMYOUTWARD-1-102", "DummyOutward-1", 102, "RED", "Medium", "S1234");
					}
				}
				else
				{
					AssertExceptionThrown("Already finalised, can't make changes to qty.", typeof(DataObjectReadFailureException), @"
Cannot amend warehouse job for stock movement if original warehouse job is finalized in Physical Warehouse.
	".Trim(), () => reader.ReadIntoBusinessObject());
				}
			}
		}

		#endregion

		#region TestPopulateBusinessObject_AmendAsHeld_FinalisedOrderAndReceive

		[GuiTest]
		public void TestPopulateBusinessObject_AmendAsHeld_FinalisedOrderAndReceive()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventory: false);
			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, Warehouse, "R1");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 100m, LocationA1, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.SaveForTesting();

			WhsBondedChangeOfInventory firstChangeOfInventory;

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: IsChangeOfOwnership))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				firstChangeOfInventory = reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", firstChangeOfInventory);
				AssertWhsDocket(firstChangeOfInventory.Order, Data.Orgs.CRAHOLSYD, "B123", "B123-EDIDATEDI");
				AssertWhsDocket(firstChangeOfInventory.Receive, NewOwnerForTest, "B123", "B123-EDIDATEDI");

				var originalReceiveLocation = receive.Lines.Single().Location;
				AssertLineBondedWarehouseAttributeMatchWithInventory(firstChangeOfInventory.Order.Lines.Single(), "P1", 5m, originalReceiveLocation, "ENTRYNUMBER123-2", "DummyOutward-1", 102, "RED", "Medium", "S1234");
				if (IsChangeOfOwnership)
				{
					AssertLine(firstChangeOfInventory.Receive.Lines.Single(), NewProductCodeForTest, 5m, originalReceiveLocation, "DUMMYOUTWARD-1-102", "DummyOutward-1", 102, "12", "34", "56");
				}
				else
				{
					AssertLine(firstChangeOfInventory.Receive.Lines.Single(), NewProductCodeForTest, 5m, originalReceiveLocation, "DUMMYOUTWARD-1-102", "DummyOutward-1", 102, "RED", "Medium", "S1234");
				}
			}

			FinaliseDocketsFromImport(firstChangeOfInventory);

			// ammend qty - send with HELD this time
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventory: false, recipientRoles: new[] { new RecipientRoleDetail { Type = RecipientRoleForTest, ServiceCode = ServiceCodeType.HLD } });
			Factory.SaveForTesting();
			AssertEquals("Precondition - hold service code is set for recipient role ", ServiceCodeType.HLD, Logger.TopLevelDataObject.DataContext.RecipientRoleCollection.First(r => r.Code == RecipientRoleForTest).ServiceCode);

			var line = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine(orderedQty: 6);
			var lineDetails = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(NewProductCodeForTest, invoiceLine: line);
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(customsLineDetails: new[] { lineDetails.Object }, isChangeOfOwnership: IsChangeOfOwnership, intoRegimeType: CustomsRegime.InwardProcessing))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);

				if (IsSourceWarehouseVirtual || IsChangeOfOwnership)
				{
					var amendedChangeOfInventoryBO = reader.ReadIntoBusinessObject();
					Factory.SaveForTesting();

					AssertNotEquals("Should have cancelled the previous Order and created a new one.", firstChangeOfInventory.Order.PK, amendedChangeOfInventoryBO.Order.PK);
					AssertEquals("Should have amended the same same Receive.", firstChangeOfInventory.Receive.PK, amendedChangeOfInventoryBO.Receive.PK);

					var orderInOtherFactory = new BusinessObjectFactory().Load<WhsDocket>(firstChangeOfInventory.Order.PK);
					AssertNotNull(orderInOtherFactory.Logs.Find(l => l.SL_SE_NKEvent == Events.CancelledCode).SingleOrDefault());
					AssertEquals("Amendment should have cancelled Order.", true, orderInOtherFactory.IsCancelled);
					AssertNull("Amendment should have cancelled Pick.", ((WhsOrder)orderInOtherFactory).Pick);

					AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", amendedChangeOfInventoryBO);
					AssertWhsDocket(amendedChangeOfInventoryBO.Order, Data.Orgs.CRAHOLSYD, "B123", "B123-EDIDATEDI", splitNumber: 1);
					AssertWhsDocket(amendedChangeOfInventoryBO.Receive, NewOwnerForTest, "B123", "B123-EDIDATEDI", splitNumber: 0);

					var originalReceiveLocation = receive.Lines.Single().Location;
					AssertLineBondedWarehouseAttributeMatchWithInventory(amendedChangeOfInventoryBO.Order.Lines.Single(), "P1", 6m, originalReceiveLocation, "ENTRYNUMBER123-2", "DummyOutward-1", 102, "RED", "Medium", "S1234");

					// Receive should not actually be ammended as this is a HELD message. The accept message would be sent later to update the receive.
					AssertEquals("All stock must become Held.", InventoryHoldCodes.Codes.Held, amendedChangeOfInventoryBO.Receive.Lines.Single().WE_WHC_NKCurrentInventoryHeldCode);
					if (IsChangeOfOwnership)
					{
						AssertLine(amendedChangeOfInventoryBO.Receive.Lines.Single(), NewProductCodeForTest, 5m, originalReceiveLocation, "DUMMYOUTWARD-1-102", "DummyOutward-1", 102, "12", "34", "56");
					}
					else
					{
						AssertLine(amendedChangeOfInventoryBO.Receive.Lines.Single(), NewProductCodeForTest, 5m, originalReceiveLocation, "DUMMYOUTWARD-1-102", "DummyOutward-1", 102, "RED", "Medium", "S1234");
					}
				}
				else
				{
					AssertExceptionThrown("Already finalised, can't make changes to qty.", typeof(DataObjectReadFailureException), @"
Cannot amend warehouse job for stock movement if original warehouse job is finalized in Physical Warehouse.
	".Trim(), () => reader.ReadIntoBusinessObject());
				}
			}
		}

		#endregion

		#region TestPopulateBusinessObject_AmendAsHeld_FinalisedOrder

		[GuiTest]
		public void TestPopulateBusinessObject_AmendAsHeld_FinalisedOrder()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventory: false, recipientRoles: new[] { new RecipientRoleDetail { Type = RecipientRoleForTest, ServiceCode = ServiceCodeType.HLD } });
			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, Warehouse, "R1");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 100m, LocationA1, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.SaveForTesting();
			AssertEquals("Precondition - hold service code is set for recipient role ", ServiceCodeType.HLD, Logger.TopLevelDataObject.DataContext.RecipientRoleCollection.First(r => r.Code == RecipientRoleForTest).ServiceCode);

			WhsBondedChangeOfInventory firstChangeOfInventory;

			var putawayEngineMock = SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK();

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (ObjectFactory.Substitute(putawayEngineMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: IsChangeOfOwnership))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				firstChangeOfInventory = reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", firstChangeOfInventory);
				AssertWhsDocket(firstChangeOfInventory.Order, Data.Orgs.CRAHOLSYD, "B123", "B123-EDIDATEDI");
				AssertWhsDocket(firstChangeOfInventory.Receive, NewOwnerForTest, "B123", "B123-EDIDATEDI", isSaved: false);

				var originalReceiveLocation = receive.Lines.Single().Location;
				AssertLineBondedWarehouseAttributeMatchWithInventory(firstChangeOfInventory.Order.Lines.Single(), "P1", 5m, originalReceiveLocation, "ENTRYNUMBER123-2", "DummyOutward-1", 102, "RED", "Medium", "S1234");
				if (IsChangeOfOwnership)
				{
					AssertLine(firstChangeOfInventory.Receive.Lines.Single(), NewProductCodeForTest, 5m, originalReceiveLocation, "DUMMYOUTWARD-1-102", "DummyOutward-1", 102, "12", "34", "56");
				}
				else
				{
					AssertLine(firstChangeOfInventory.Receive.Lines.Single(), NewProductCodeForTest, 5m, originalReceiveLocation, "DUMMYOUTWARD-1-102", "DummyOutward-1", 102, "RED", "Medium", "S1234");
				}
			}

			FinaliseDocketsFromImport(firstChangeOfInventory);

			// ammend qty - send with HELD again
			var line = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine(orderedQty: 6);
			var lineDetails = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(NewProductCodeForTest, invoiceLine: line, newSerialNum: "");
			using (ObjectFactory.Substitute(putawayEngineMock))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(customsLineDetails: new[] { lineDetails.Object }, isChangeOfOwnership: IsChangeOfOwnership, intoRegimeType: CustomsRegime.InwardProcessing))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);

				if (IsSourceWarehouseVirtual || IsChangeOfOwnership)
				{
					var amendedChangeOfInventoryBO = reader.ReadIntoBusinessObject();
					Factory.SaveForTesting();

					AssertNotEquals("Should have cancelled the previous Order and created a new one.", firstChangeOfInventory.Order.PK, amendedChangeOfInventoryBO.Order.PK);
					AssertNotEquals("Should have amended another Receive as the first one wasn't saved.", firstChangeOfInventory.Receive.PK, amendedChangeOfInventoryBO.Receive.PK);

					var orderInOtherFactory = new BusinessObjectFactory().Load<WhsDocket>(firstChangeOfInventory.Order.PK);
					AssertNotNull(orderInOtherFactory.Logs.Find(l => l.SL_SE_NKEvent == Events.CancelledCode).SingleOrDefault());
					AssertEquals("Amendment should have cancelled Order.", true, orderInOtherFactory.IsCancelled);
					AssertNull("Amendment should have cancelled Pick.", ((WhsOrder)orderInOtherFactory).Pick);

					AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", amendedChangeOfInventoryBO);
					AssertWhsDocket(amendedChangeOfInventoryBO.Order, Data.Orgs.CRAHOLSYD, "B123", "B123-EDIDATEDI", splitNumber: 1);
					AssertWhsDocket(amendedChangeOfInventoryBO.Receive, NewOwnerForTest, "B123", "B123-EDIDATEDI", isSaved: false);

					var originalReceiveLocation = receive.Lines.Single().Location;
					AssertLineBondedWarehouseAttributeMatchWithInventory(amendedChangeOfInventoryBO.Order.Lines.Single(), "P1", 6m, originalReceiveLocation, "ENTRYNUMBER123-2", "DummyOutward-1", 102, "RED", "Medium", "S1234");
				}
				else
				{
					AssertExceptionThrown("Already finalised, can't make changes to qty.", typeof(DataObjectReadFailureException), @"
Cannot amend warehouse job for stock movement if original warehouse job is finalized in Physical Warehouse.
	".Trim(), () => reader.ReadIntoBusinessObject());
				}
			}
		}

		#endregion

		#region TestPopulateBusinessObject_Amend_FinalisedOrderAndReceive_AlreadyWithdrawn_Failed

		[GuiTest]
		public void TestPopulateBusinessObject_Amend_FinalisedOrderAndReceive_AlreadyWithdrawn_Failed()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventory: false);
			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, Warehouse, "R1");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 100m, LocationA1, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.SaveForTesting();

			WhsBondedChangeOfInventory firstChangeOfInventory;
			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: IsChangeOfOwnership))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				firstChangeOfInventory = reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", firstChangeOfInventory);
				AssertWhsDocket(firstChangeOfInventory.Order, Data.Orgs.CRAHOLSYD, "B123", "B123-EDIDATEDI");
				AssertWhsDocket(firstChangeOfInventory.Receive, NewOwnerForTest, "B123", "B123-EDIDATEDI");

				var originalReceiveLocation = receive.Lines.Single().Location;
				AssertLineBondedWarehouseAttributeMatchWithInventory(firstChangeOfInventory.Order.Lines.Single(), "P1", 5m, originalReceiveLocation, "ENTRYNUMBER123-2", "DummyOutward-1", 102, "RED", "Medium", "S1234");
				if (IsChangeOfOwnership)
				{
					AssertLine(firstChangeOfInventory.Receive.Lines.Single(), NewProductCodeForTest, 5m, originalReceiveLocation, "DUMMYOUTWARD-1-102", "DummyOutward-1", 102, "12", "34", "56");
				}
				else
				{
					AssertLine(firstChangeOfInventory.Receive.Lines.Single(), NewProductCodeForTest, 5m, originalReceiveLocation, "DUMMYOUTWARD-1-102", "DummyOutward-1", 102, "RED", "Medium", "S1234");
				}

				FinaliseDocketsFromImport(firstChangeOfInventory);
				WithdrawStockFromNewOwner();
			}

			// ammend qty
			var line = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine(orderedQty: 6);
			var lineDetails = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(NewProductCodeForTest, invoiceLine: line);
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(customsLineDetails: new[] { lineDetails.Object }, isChangeOfOwnership: IsChangeOfOwnership, intoRegimeType: CustomsRegime.InwardProcessing))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);

				if (IsSourceWarehouseVirtual || IsChangeOfOwnership)
				{
					AssertExceptionThrown("Already withdrawn, can't make changes to qty.", typeof(DataObjectReadFailureException), @"
	Cannot do an amendment. Stock related properties affected.
	".Trim(), () => reader.ReadIntoBusinessObject());
				}
				else
				{
					AssertExceptionThrown("Already finalised, can't make changes to qty.", typeof(DataObjectReadFailureException), @"
Cannot amend warehouse job for stock movement if original warehouse job is finalized in Physical Warehouse.
	".Trim(), () => reader.ReadIntoBusinessObject());
				}
			}
		}

		#endregion

		#region TestPopulateBusinessObject_Amend_FinalisedOrderAndReceive_AlreadyWithdrawn_CustomsDataChange_OK

		[GuiTest]
		public void TestPopulateBusinessObject_Amend_FinalisedOrderAndReceive_AlreadyWithdrawn_CustomsDataChange_OK()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventory: false);
			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, Warehouse, "R1");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 100m, LocationA1, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.SaveForTesting();

			WhsBondedChangeOfInventory firstChangeOfInventory;
			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: IsChangeOfOwnership))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				firstChangeOfInventory = reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", firstChangeOfInventory);
				AssertWhsDocket(firstChangeOfInventory.Order, Data.Orgs.CRAHOLSYD, "B123", "B123-EDIDATEDI");
				AssertWhsDocket(firstChangeOfInventory.Receive, NewOwnerForTest, "B123", "B123-EDIDATEDI");

				var originalReceiveLocation = receive.Lines.Single().Location;
				AssertLineBondedWarehouseAttributeMatchWithInventory(firstChangeOfInventory.Order.Lines.Single(), "P1", 5m, originalReceiveLocation, "ENTRYNUMBER123-2", "DummyOutward-1", 102, "RED", "Medium", "S1234");
				if (IsChangeOfOwnership)
				{
					AssertLine(firstChangeOfInventory.Receive.Lines.Single(), NewProductCodeForTest, 5m, originalReceiveLocation, "DUMMYOUTWARD-1-102", "DummyOutward-1", 102, "12", "34", "56");
				}
				else
				{
					AssertLine(firstChangeOfInventory.Receive.Lines.Single(), NewProductCodeForTest, 5m, originalReceiveLocation, "DUMMYOUTWARD-1-102", "DummyOutward-1", 102, "RED", "Medium", "S1234");
				}

				FinaliseDocketsFromImport(firstChangeOfInventory);
				WithdrawStockFromNewOwner();
			}

			// ammend bonded attributes - leave entry keys (they wont change)
			var line = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine();
			var lineDetails = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(NewProductCodeForTest, invoiceLine: line, newSerialNum: "");

			lineDetails.Setup(s => s.AddInfos).Returns("This was information that wasn't useful");
			lineDetails.Setup(s => s.CountryOfOrigin).Returns(new Country() { Code = "AU", Name = "Australia" });
			lineDetails.Setup(s => s.CustomsQuantity).Returns(13.3m);
			lineDetails.Setup(s => s.CustomsQuantityUnit).Returns(new CodeDescriptionPair6Char() { Code = "KG", Description = "Kilograms" });
			lineDetails.Setup(s => s.CustomsSecondQuantity).Returns(64.8m);
			lineDetails.Setup(s => s.CustomsSecondQuantityUnit).Returns(new CodeDescriptionPair6Char() { Code = "GRM", Description = "Gram" });
			lineDetails.Setup(s => s.ExtraClassificationDetails).Returns(new ZString[] { "ExtraClassificationDetails" });
			lineDetails.Setup(s => s.PrimaryPreference).Returns("STANDARD");
			lineDetails.Setup(s => s.Tariff).Returns("TRF");
			lineDetails.Setup(s => s.TILV).Returns(32.9m);
			lineDetails.Setup(s => s.ValueForDuty).Returns(24.5m);
			lineDetails.Setup(s => s.CustomsThirdQuantity).Returns(10.1m);
			lineDetails.Setup(s => s.CustomsThirdQuantityUnit).Returns(new CodeDescriptionPair6Char() { Code = "GRM", Description = "Gram" });
			// ManufacturerAddress need to be tested after customs team implement it

			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(customsLineDetails: new[] { lineDetails.Object }, isChangeOfOwnership: IsChangeOfOwnership, intoRegimeType: CustomsRegime.InwardProcessing))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				if (IsSourceWarehouseVirtual || IsChangeOfOwnership)
				{
					var amendedChangeOfInventoryBO = reader.ReadIntoBusinessObject();
					Factory.SaveForTesting();

					AssertEquals("Should NOT have cancelled the previous Order and created a new one.", firstChangeOfInventory.Order.PK, amendedChangeOfInventoryBO.Order.PK);
					AssertEquals("Should have amended the same same Receive.", firstChangeOfInventory.Receive.PK, amendedChangeOfInventoryBO.Receive.PK);

					AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", amendedChangeOfInventoryBO);
					AssertWhsDocket(amendedChangeOfInventoryBO.Order, Data.Orgs.CRAHOLSYD, "B123", "B123-EDIDATEDI");
					AssertWhsDocket(amendedChangeOfInventoryBO.Receive, NewOwnerForTest, "B123", "B123-EDIDATEDI");

					var originalReceiveLocation = receive.Lines.Single().Location;
					AssertLineBondedWarehouseAttributeMatchWithInventory(amendedChangeOfInventoryBO.Order.Lines.Single(), "P1", 5m, originalReceiveLocation, "ENTRYNUMBER123-2", "DummyOutward-1", 102, "RED", "Medium", "S1234");
					if (IsChangeOfOwnership)
					{
						AssertLineWithNewCustomsData(amendedChangeOfInventoryBO.Receive.Lines.Single(), NewProductCodeForTest, 5m, originalReceiveLocation, "DUMMYOUTWARD-1-102", "DummyOutward-1", 102, "12", "34", "56");
					}
					else
					{
						AssertLineWithNewCustomsData(amendedChangeOfInventoryBO.Receive.Lines.Single(), NewProductCodeForTest, 5m, originalReceiveLocation, "DUMMYOUTWARD-1-102", "DummyOutward-1", 102, "RED", "Medium", "S1234");
					}
				}
				else
				{
					AssertExceptionThrown("Already finalised, can't make changes to qty.", typeof(DataObjectReadFailureException), @"
Cannot amend warehouse job for stock movement if original warehouse job is finalized in Physical Warehouse.
	".Trim(), () => reader.ReadIntoBusinessObject());
				}
			}
		}

		void AssertLineWithNewCustomsData(WhsDocketLine line, ZString partNo, decimal qty, WhsLocation location, ZString docketLineEntryKey, ZString customsEntryNo, ZShort wbEntryLineNo, ZString attrib1, ZString attrib2, ZString attrib3, Func<WhsDocketLine, WhsLocation> getLineLocation = null)
		{
			AssertEquals("Product Code", partNo, line.SupplierPart.OP_PartNum);
			AssertEquals("Bonded Qty", qty, line.WE_TransactionQuantity);
			AssertEquals("Bonded Qty UQ", "BOX", line.WE_F3_NKPackType);

			var actualLocation = getLineLocation != null ? getLineLocation(line) : line.Docket.WD_DocketType == DocketType.Codes.Order ? line.PickLines.Single().Inventory.Location : line.Location;
			AssertEquals("Location", location.PK, actualLocation.PK);

			var customsData = line.CustomsData;
			AssertEquals("WB_AddInfo", "This was information that wasn't useful", customsData.WB_AddInfo);
			AssertEquals("WB_CustomsQty", 13.3m, customsData.WB_CustomsQty);
			AssertEquals("WB_CustomsUnitOfQty", "KG", customsData.WB_CustomsUnitOfQty);
			AssertEquals("WB_DeclarationReference", "B123", customsData.WB_DeclarationReference);
			AssertEquals("ustoms Entry Key", customsEntryNo, customsData.WB_EntryKey);
			AssertEquals("Customs Entry Line No.", wbEntryLineNo, customsData.WB_EntryLineNo);
			AssertEquals("WB_RN_NKCountryOfOrigin", "AU", customsData.WB_RN_NKCountryOfOrigin);
			AssertEquals("WB_TILV", 32.9m, customsData.WB_TILV);
			AssertEquals("WB_ValueForDuty", 24.5m, customsData.WB_ValueForDuty);
			AssertEquals("WB_CustomsSecondQuantity", 64.8m, customsData.WB_CustomsSecondQuantity);
			AssertEquals("WB_CustomsSecondUnitQty", "GRM", customsData.WB_CustomsSecondUnitQty);
			AssertEquals("WB_Tariff", "TRF", customsData.WB_Tariff);
			AssertEquals("WB_PrimaryPreference", "STANDARD", customsData.WB_PrimaryPreference);

			AssertEquals("Order/Comm Inv Line No", new ZShort(3), line.WE_LineNo);
			AssertEquals("Attrib1", attrib1, line.WE_PartAttrib1);
			AssertEquals("Attrib2", attrib2, line.WE_PartAttrib2);
			AssertEquals("Attrib3", attrib3, line.WE_PartAttrib3);
			AssertEquals("Bonded Entry Key", docketLineEntryKey, line.WE_BondedEntryKey);
			AssertEquals("WB_CustomsThirdQuantity", 10.1m, customsData.WB_CustomsThirdQuantity);
			AssertEquals("WB_CustomsThirdUnitQty", "GRM", customsData.WB_CustomsThirdUnitQty);
			// ManufacturerAddress need to be tested after customs team implement it
		}

		#endregion

		#region TestPopulateBusinessObject_ResendAsHeld_FinalisedOrderAndReceive_AlreadyWithdrawn_Failed

		[GuiTest]
		public void TestPopulateBusinessObject_ResendAsHeld_FinalisedOrderAndReceive_AlreadyWithdrawn_Failed()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventory: false);
			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, Warehouse, "R1");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 100m, LocationA1, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.SaveForTesting();

			WhsBondedChangeOfInventory firstChangeOfInventory;
			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: IsChangeOfOwnership))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				firstChangeOfInventory = reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", firstChangeOfInventory);
				AssertWhsDocket(firstChangeOfInventory.Order, Data.Orgs.CRAHOLSYD, "B123", "B123-EDIDATEDI");
				AssertWhsDocket(firstChangeOfInventory.Receive, NewOwnerForTest, "B123", "B123-EDIDATEDI");

				var originalReceiveLocation = receive.Lines.Single().Location;
				AssertLineBondedWarehouseAttributeMatchWithInventory(firstChangeOfInventory.Order.Lines.Single(), "P1", 5m, originalReceiveLocation, "ENTRYNUMBER123-2", "DummyOutward-1", 102, "RED", "Medium", "S1234");
				if (IsChangeOfOwnership)
				{
					AssertLine(firstChangeOfInventory.Receive.Lines.Single(), NewProductCodeForTest, 5m, originalReceiveLocation, "DUMMYOUTWARD-1-102", "DummyOutward-1", 102, "12", "34", "56");
				}
				else
				{
					AssertLine(firstChangeOfInventory.Receive.Lines.Single(), NewProductCodeForTest, 5m, originalReceiveLocation, "DUMMYOUTWARD-1-102", "DummyOutward-1", 102, "RED", "Medium", "S1234");
				}
			}

			if (IsDocketFinalisedOnImport)
			{
				WithdrawStockFromNewOwner();
				// send with HELD this time - make no other changes
				SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventory: false, recipientRoles: new[] { new RecipientRoleDetail { Type = RecipientRoleForTest, ServiceCode = ServiceCodeType.HLD } });
				Factory.SaveForTesting();
				AssertEquals("Precondition - hold service code is set for recipient role ", ServiceCodeType.HLD, Logger.TopLevelDataObject.DataContext.RecipientRoleCollection.First(r => r.Code == RecipientRoleForTest).ServiceCode);

				using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
				using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: IsChangeOfOwnership))
				{
					var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
					AssertExceptionThrown("Already withdrawn, can't hold stock.", typeof(DataObjectReadFailureException), @"
Cannot hold stock. Stock related properties affected.
".Trim(), () => reader.ReadIntoBusinessObject());
				}
			}
		}

		#endregion

		#region TestPopulateBusinessObject_Cancel_FinalisedOrderAndReceive

		[GuiTest]
		public void TestPopulateBusinessObject_Cancel_FinalisedOrderAndReceive()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(addImporterToXML: true, isWarehouseCreatedAsVirtual: IsSourceWarehouseVirtual, createInventory: true, recipientRoles: new[] { new RecipientRoleDetail { Type = RecipientRoleForTest } }, isSerialNumberTest: true);
			Factory.SaveForTesting();

			var line = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine(orderedQty: 1, previousSerialNum: "SNN");
			var lineDetails = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(NewProductCodeForTest, invoiceLine: line);
			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(customsLineDetails: new[] { lineDetails.Object }, isChangeOfOwnership: IsChangeOfOwnership))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				var changeOfInventoryBO = reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", changeOfInventoryBO);
				AssertWhsDocket(changeOfInventoryBO.Order, Data.Orgs.CRAHOLSYD, "B123", "B123-EDIDATEDI");
				AssertWhsDocket(changeOfInventoryBO.Receive, NewOwnerForTest, "B123", "B123-EDIDATEDI");

				FinaliseDocketsFromImport(changeOfInventoryBO);

				var eventDataObject = Data.GetEventDataObject(Events.CancelTheWarehouseJob, new[] { new RecipientRoleDetail { Type = RecipientRoleForTest } });
				ImportEventViaDataContextManager(eventDataObject);
				Factory.SaveForTesting();

				// cancelling the job as a result of a customs cancel event occurs in a separate factory, so load from db.
				var orderInOtherFactory = new BusinessObjectFactory().Load<WhsDocket>(changeOfInventoryBO.Order.PK);
				AssertNotNull(orderInOtherFactory.Logs.Find(l => l.SL_SE_NKEvent == Events.CancelledCode).SingleOrDefault());
				AssertEquals("Cancel Event should have cancelled Order.", true, orderInOtherFactory.IsCancelled);
				AssertNull("Cancelling Order should have cancelled Pick.", ((WhsOrder)orderInOtherFactory).Pick);

				var receiveInOtherFactory = new BusinessObjectFactory().Load<WhsDocket>(changeOfInventoryBO.Receive.PK);
				AssertNotNull(receiveInOtherFactory.Logs.Find(l => l.SL_SE_NKEvent == Events.CancelledCode).SingleOrDefault());
				AssertEquals("Cancel Event should have cancelled Receive.", true, receiveInOtherFactory.IsCancelled);
				var inventory = ((WhsReceive)receiveInOtherFactory).Inventory[0];
				AssertEquals("Cancelling Receive should NOT have cleared all locations. Adjustment took stock, but we keep the location for history.", false, inventory.WI_WL.IsEmpty);
				AssertEquals("Cancelling Receive should have reduced all stock to Zero. Adjustment took the stock.", 0m, inventory.WI_TotalUnits);
			}
		}

		#endregion

		#region TestPopulateBusinessObject_Cancel_FinalisedOrder

		[GuiTest]
		public void TestPopulateBusinessObject_Cancel_FinalisedOrder()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(recipientRoles: new[] { new RecipientRoleDetail { Type = RecipientRoleForTest, ServiceCode = ServiceCodeType.HLD } });
			Factory.SaveForTesting();
			AssertEquals("Precondition - hold service code is set for recipient role ", ServiceCodeType.HLD, Logger.TopLevelDataObject.DataContext.RecipientRoleCollection.First(r => r.Code == RecipientRoleForTest).ServiceCode);

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: IsChangeOfOwnership))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				var changeOfInventoryBO = reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", changeOfInventoryBO);
				AssertWhsDocket(changeOfInventoryBO.Order, Data.Orgs.CRAHOLSYD, "B123", "B123-EDIDATEDI");
				AssertWhsDocket(changeOfInventoryBO.Receive, NewOwnerForTest, "B123", "B123-EDIDATEDI", isSaved: false);

				FinaliseDocketsFromImport(changeOfInventoryBO);

				var eventDataObject = Data.GetEventDataObject(Events.CancelTheWarehouseJob, new[] { new RecipientRoleDetail { Type = RecipientRoleForTest } });
				ImportEventViaDataContextManager(eventDataObject);
				Factory.SaveForTesting();

				// cancelling the job as a result of a customs cancel event occurs in a separate factory, so load from db.
				var orderInOtherFactory = new BusinessObjectFactory().Load<WhsDocket>(changeOfInventoryBO.Order.PK);
				AssertNotNull(orderInOtherFactory.Logs.Find(l => l.SL_SE_NKEvent == Events.CancelledCode).SingleOrDefault());
				AssertEquals("Cancel Event should have cancelled Order.", true, orderInOtherFactory.IsCancelled);
				AssertNull("Cancelling Order should have cancelled Pick.", ((WhsOrder)orderInOtherFactory).Pick);

				var receiveInOtherFactory = new BusinessObjectFactory().Load<WhsDocket>(changeOfInventoryBO.Receive.PK);
				AssertNull(receiveInOtherFactory);
			}
		}

		#endregion

		#region TestPopulateBusinessObject_Cancel_FinalisedOrderAndReceive_AlreadyWithdrawn_Failed

		[GuiTest]
		public void TestPopulateBusinessObject_Cancel_FinalisedOrderAndReceive_AlreadyWithdrawn_Failed()
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(addImporterToXML: true, isWarehouseCreatedAsVirtual: IsSourceWarehouseVirtual, createInventory: true, recipientRoles: new[] { new RecipientRoleDetail { Type = RecipientRoleForTest } }, isSerialNumberTest: true);
			Factory.SaveForTesting();

			var line = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine(orderedQty: 1, previousSerialNum: "SNN");
			var lineDetails = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(NewProductCodeForTest, invoiceLine: line);
			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(customsLineDetails: new[] { lineDetails.Object }, isChangeOfOwnership: IsChangeOfOwnership))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				var changeOfInventoryBO = reader.ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", changeOfInventoryBO);
				AssertWhsDocket(changeOfInventoryBO.Order, Data.Orgs.CRAHOLSYD, "B123", "B123-EDIDATEDI");
				AssertWhsDocket(changeOfInventoryBO.Receive, NewOwnerForTest, "B123", "B123-EDIDATEDI");

				FinaliseDocketsFromImport(changeOfInventoryBO);
				WithdrawStockFromNewOwner(isSerialTest: true);

				var eventDataObject = Data.GetEventDataObject(Events.CancelTheWarehouseJob, new[] { new RecipientRoleDetail { Type = RecipientRoleForTest } });
				ImportEventViaDataContextManager(eventDataObject);
				Factory.SaveForTesting();

				// cancelling the job as a result of a customs cancel event occurs in a separate factory, so load from db.
				var orderInOtherFactory = new BusinessObjectFactory().Load<WhsDocket>(changeOfInventoryBO.Order.PK);
				AssertNull(orderInOtherFactory.Logs.Find(l => l.SL_SE_NKEvent == Events.CancelledCode).SingleOrDefault());
				AssertEquals("Cancel Event should have cancelled Order.", false, orderInOtherFactory.IsCancelled);
				AssertNotNull("Cancelling Order should have cancelled Pick.", ((WhsOrder)orderInOtherFactory).Pick);

				var receiveInOtherFactory = new BusinessObjectFactory().Load<WhsDocket>(changeOfInventoryBO.Receive.PK);
				AssertNull(receiveInOtherFactory.Logs.Find(l => l.SL_SE_NKEvent == Events.CancelledCode).SingleOrDefault());
				AssertEquals("Cancel Event should have cancelled Receive.", false, receiveInOtherFactory.IsCancelled);
				var inventory = ((WhsReceive)receiveInOtherFactory).Inventory[0];
				AssertEquals("Receive was not cancelled, should have location.", false, inventory.WI_WL.IsEmpty);
				AssertEquals("Receive was not cancelled, stock should stay the same.", 0m, inventory.WI_TotalUnits);
			}
		}

		#endregion

		#region TestPopulateBusinessObject_OutwardType

		public void TestPopulateBusinessObject_OutwardType()
		{
			var whs = Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: IsSourceWarehouseVirtual, isBondedWarehouse: true, isFTZWarehouse: true);
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(isWarehouseCreatedAsVirtual: IsSourceWarehouseVirtual, createInventory: false, isFTZWarehouse: true);
			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, whs, "R1");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 100m, whs.DefaultLocation, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			receive.Lines[0].CustomsData.WB_OutwardType = "TOF";
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.SaveForTesting();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var invoiceLine = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine();
				var lineDetailsUS = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMockUS(invoiceLine, zoneStatusCode: "", outwardType: OutwardType.Exports);

				var allocationMock = new AllocateFIFOLegacyMock();
				using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
				using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
				using (new WarehouseCustomsDetailsProvidersMocks(new[] { lineDetailsUS }, isChangeOfOwnership: IsChangeOfOwnership))
				{
					var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
					var changeOfInventoryBO = reader.ReadIntoBusinessObject();
					Factory.SaveForTesting();

					AssertEquals("OutwardType should read from xml not from inventory.", "EXS", changeOfInventoryBO.Order.Lines.Single().CustomsData.WB_OutwardType);
				}
			}
		}

		#endregion

		#region TestPopulateBusinessObject_ChangeOfWarehouse

		public void TestPopulateBusinessObject_ChangeOfWarehouse()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			Factory.SaveForTesting();

			var newWarehouseAddress = Data.Orgs.CRAHOLSYD.MainAddress;
			var newWarehouse = Helper.CreateWarehouse("WH2", "A", 1, 1);
			newWarehouse.WW_OA_WarehouseAddress = newWarehouseAddress.PK;
			newWarehouse.WW_IsVirtualWarehouse = true;
			newWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			Factory.SaveForTesting();

			var bondArea = Helper.CreateArea(newWarehouse, "BOND", AreaTypes.Codes.Bonded);
			var location1 = newWarehouse.FindLocation("A");
			location1.WLV_LocationStatus = LocationStatus.Codes.Normal;
			location1.WLV_WA_PutawayArea = bondArea.PK;

			Factory.SaveForTesting();

			var newWarehouseOrganisationAddress = GetNewAddressData_CRAHOLSYD(DocAddressType.CustomsWarehouseAddress);

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: IsChangeOfOwnership, newWarehouse: newWarehouseOrganisationAddress))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);

				var changeOfInventoryBO = reader.ReadIntoBusinessObject();
				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", changeOfInventoryBO);
				AssertNotNull("Should have read in a Order for Warehouse 1.", changeOfInventoryBO.Order);
				AssertEquals(Warehouse.PK, changeOfInventoryBO.Order.WD_WW_Whs);

				AssertNotNull("Should have read in a Receive for Warehouse 2.", changeOfInventoryBO.Receive);
				AssertEquals(newWarehouse.PK, changeOfInventoryBO.Receive.WD_WW_Whs);
			}
		}

		public void TestPopulateBusinessObject_ChangeOfWarehouse_ToPhysicalWarehouse()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			Factory.SaveForTesting();

			var newWarehouseAddress = Data.Orgs.CRAHOLSYD.MainAddress;
			var newWarehouse = Helper.CreateWarehouse("WH2", "A", 1, 1);
			newWarehouse.WW_OA_WarehouseAddress = newWarehouseAddress.PK;
			newWarehouse.WW_IsVirtualWarehouse = false;
			newWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			Factory.SaveForTesting();

			var bondArea = Helper.CreateArea(newWarehouse, "BOND", AreaTypes.Codes.Bonded);
			var location1 = newWarehouse.FindLocation("A");
			location1.WLV_LocationStatus = LocationStatus.Codes.Normal;
			location1.WLV_WA_PutawayArea = bondArea.PK;

			Factory.SaveForTesting();

			var newWarehouseOrganisationAddress = GetNewAddressData_CRAHOLSYD(DocAddressType.CustomsWarehouseAddress);

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: IsChangeOfOwnership, newWarehouse: newWarehouseOrganisationAddress))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);

				var changeOfInventoryBO = reader.ReadIntoBusinessObject();
				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", changeOfInventoryBO);
				AssertNotNull("Should have read in a Order for Warehouse 1.", changeOfInventoryBO.Order);
				AssertEquals(Warehouse.PK, changeOfInventoryBO.Order.WD_WW_Whs);

				AssertNotNull("Should have read in a Receive for Warehouse 2.", changeOfInventoryBO.Receive);
				AssertEquals(newWarehouse.PK, changeOfInventoryBO.Receive.WD_WW_Whs);

				AssertEquals("Order is finalised only for virtual warehouses.", IsSourceWarehouseVirtual, changeOfInventoryBO.Order.IsFinalised);
				AssertEquals("Order is held for customs accordingly.", !IsSourceWarehouseVirtual, changeOfInventoryBO.Order.IsDocketHeldByCustoms);
				Assert("Receive is not finalised for physical warehouse.", !changeOfInventoryBO.Receive.IsFinalised);
				Assert("Receive is held for customs.", changeOfInventoryBO.Receive.IsDocketHeldByCustoms);
			}
		}

		[GuiTest]
		public void TestPopulateBusinessObject_ChangeOfWarehouse_ToPhysicalWarehouse_ExistingOrderFinalised()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventory: false);
			Factory.SaveForTesting();

			var newWarehouseAddress = Data.Orgs.CRAHOLSYD.MainAddress;
			var newWarehouse = Helper.CreateWarehouse("WH2", "A", 1, 1);
			newWarehouse.WW_OA_WarehouseAddress = newWarehouseAddress.PK;
			newWarehouse.WW_IsVirtualWarehouse = false;
			newWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			Factory.SaveForTesting();

			var bondArea = Helper.CreateArea(newWarehouse, "BOND", AreaTypes.Codes.Bonded);
			var location1 = newWarehouse.FindLocation("A");
			location1.WLV_LocationStatus = LocationStatus.Codes.Normal;
			location1.WLV_WA_PutawayArea = bondArea.PK;

			Factory.SaveForTesting();

			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventory: false);
			Factory.SaveForTesting();

			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, Warehouse, "R1");
			CreateWhsReceiveInventoryLineWithCustomsAttributeData(receive, Data.ProductCRAHOLSYD, 100m, LocationA1, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.SaveForTesting();

			var order = Helper.CreateWhsOrder(Data.Orgs.CRAHOLSYD, Warehouse, "O1");
			order.WD_CustomsParentReference = "B123-EDIDATEDI";
			var orderLine = Helper.CreateWhsOrderLine(order, Data.ProductCRAHOLSYD, 100m);
			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);
			pick.AutoAllocateItems();
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.SaveForTesting();

			var newWarehouseOrganisationAddress = GetNewAddressData_CRAHOLSYD(DocAddressType.CustomsWarehouseAddress);

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: IsChangeOfOwnership, newWarehouse: newWarehouseOrganisationAddress))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);

				if (IsSourceWarehouseVirtual)
				{
					AssertNoExceptionThrown("Should not fail if virtual Warehouse.", () => reader.ReadIntoBusinessObject());
				}
				else
				{
					AssertExceptionThrown("Should fail due to matching Receive which is finalised.", typeof(DataObjectReadFailureException), @"
Cannot amend warehouse job for stock movement if original warehouse job is finalized in Physical Warehouse.
".Trim(), () => reader.ReadIntoBusinessObject());
				}
			}
		}

		public void TestPopulateBusinessObject_ChangeOfWarehouse_InvalidWarehouse_WrongWarehouseType()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			Factory.SaveForTesting();

			var newWarehouseAddress = Data.Orgs.CRAHOLSYD.MainAddress;
			var newWarehouse = Helper.CreateWarehouse("WH2", "A", 1, 1);
			newWarehouse.WW_OA_WarehouseAddress = newWarehouseAddress.PK;
			newWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			Factory.SaveForTesting();

			var bondArea = Helper.CreateArea(newWarehouse, "BOND", AreaTypes.Codes.Bonded);
			var location1 = newWarehouse.FindLocation("A");
			location1.WLV_LocationStatus = LocationStatus.Codes.Normal;
			location1.WLV_WA_PutawayArea = bondArea.PK;

			Factory.SaveForTesting();

			var newWarehouseOrganisationAddress = GetNewAddressData_CRAHOLSYD(DocAddressType.CustomsWarehouseAddress);

			var expectedError = string.Format("Cannot Import Receipt\r\nUnable to match Warehouse for Organization");

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: IsChangeOfOwnership, newWarehouse: newWarehouseOrganisationAddress))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				AssertExceptionThrown(typeof(DataObjectReadFailureException), expectedError, () => reader.ReadIntoBusinessObject(), assertStartsWith: true);
			}
		}

		public void TestPopulateBusinessObject_ChangeOfWarehouse_InvalidWarehouse_NoMatchFound()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			Factory.SaveForTesting();

			var invalidNewWarehouseAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.CustomsWarehouseAddress), CompanyName = "INVALID OWNER" };

			var expectedError = @"
Cannot Import Receipt
Unable to match Warehouse Address, please make sure the supplied Warehouse Address is valid. Details were:
CompanyName: INVALID OWNER
".Trim();

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: IsChangeOfOwnership, newWarehouse: invalidNewWarehouseAddress))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				AssertExceptionThrown(typeof(DataObjectReadFailureException), expectedError, () => reader.ReadIntoBusinessObject(), assertStartsWith: true);
			}
		}

		#endregion

		#region TestPopulateBusinessObject_CopiesMainAndSecondaryInwardsProcessedItem

		public void TestPopulateBusinessObject_ChangeOfRegime_OutOfInwardsProcessing_CopiesMainInwardsProcessedItemField()
		{
			TestPopulateBusinessObject_CopiesMainAndSecondaryInwardsProcessedItemCore(true, false);
		}

		public void TestPopulateBusinessObject_CopiesSecondaryInwardsProcessedItemField()
		{
			TestPopulateBusinessObject_CopiesMainAndSecondaryInwardsProcessedItemCore(false, true);
		}

		void TestPopulateBusinessObject_CopiesMainAndSecondaryInwardsProcessedItemCore(bool isMainInwardsProcessedItem, bool isSecondaryInwardsProcessedItem)
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventory: false);
			Factory.SaveForTesting();

			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, Warehouse, "R1");
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, Data.Product, 100m, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			receiveLine.CustomsData.WB_IsMainInwardsProcessedItem = isMainInwardsProcessedItem;
			receiveLine.CustomsData.WB_IsSecondaryInwardsProcessedItem = isSecondaryInwardsProcessedItem;

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.SaveForTesting();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(isChangeOfOwnership: IsChangeOfOwnership))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				var changeOfInventoryBO = reader.ReadIntoBusinessObject();
				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", changeOfInventoryBO);

				AssertNotNull("Should have read in a Order.", changeOfInventoryBO.Order);

				var newReceive = changeOfInventoryBO.Receive;
				AssertNotNull("Should have read in a Receive.", newReceive);
				var newReceiveLine = newReceive.Lines.SingleOrDefault();
				AssertNotNull("Should have read in a Receive Line.", newReceiveLine);

				AssertEquals("Should copy WB_IsMainInwardsProcessedItem for OutOfInwardsProcessing", isMainInwardsProcessedItem, newReceiveLine.CustomsData.WB_IsMainInwardsProcessedItem);
				AssertEquals("Should copy WB_IsSecondaryInwardsProcessedItem for OutOfInwardsProcessing", isSecondaryInwardsProcessedItem, newReceiveLine.CustomsData.WB_IsSecondaryInwardsProcessedItem);
			}
		}

		#endregion

		#region TestPopulateBusinessObject_AllocationInfos

		public void TestPopulateBusinessObject_AllocationInfos()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventory: false);

			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, Warehouse, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, Data.ProductCRAHOLSYD, 2m, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			receiveLine1.WI_AllocationKey = "AllocationKey-1";
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, Data.ProductCRAHOLSYD, 3m, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			receiveLine2.WI_AllocationKey = "AllocationKey-2";

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.SaveForTesting();

			var allocationInfo1 = new Mock<IWarehouseCustomsLineAllocationInfo>();
			allocationInfo1.Setup(a => a.AllocationKey).Returns("AllocationKey-1");
			allocationInfo1.Setup(a => a.Quantity).Returns(2m);

			var allocationInfo2 = new Mock<IWarehouseCustomsLineAllocationInfo>();
			allocationInfo2.Setup(a => a.AllocationKey).Returns("AllocationKey-2");
			allocationInfo2.Setup(a => a.Quantity).Returns(3m);

			var line = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine();
			line.BondedWarehouseQuantity = null;

			var customsLineDetails = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(
				newProductCode: IsChangeOfOwnership ? "W1" : "P1",
				line,
				newSerialNum: "",
				allocationInfos: new[] { allocationInfo1.Object, allocationInfo2.Object });

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(customsLineDetails: new[] { customsLineDetails.Object }, isChangeOfOwnership: IsChangeOfOwnership))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				var changeOfInventoryBO = reader.ReadIntoBusinessObject();
				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", changeOfInventoryBO);

				var order = changeOfInventoryBO.Order;
				AssertNotNull("Should have read in a Order.", order);

				var orderLines = order.Lines;
				AssertEquals("Should have read in two order lines.", 2, orderLines.Count);
				var orderLine1 = orderLines.First(l => l.WE_AllocationKey == "AllocationKey-1");
				AssertEquals("Should have read in Allocation Key Info correctly.", 2m, orderLine1.WE_TransactionQuantity);
				var orderLine2 = orderLines.First(l => l.WE_AllocationKey == "AllocationKey-2");
				AssertEquals("Should have read in Allocation Key Info correctly.", 3m, orderLine2.WE_TransactionQuantity);

				AssertNotNull("Should have read in a Receive.", changeOfInventoryBO.Receive);
			}
		}

		public void TestPopulateBusinessObject_AllocationInfos_AllocatesByKey()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventory: false);

			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, Warehouse, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, Data.ProductCRAHOLSYD, 3m, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			receiveLine1.WI_AllocationKey = "AllocationKey-1";
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, Data.ProductCRAHOLSYD, 3m, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			receiveLine2.WI_AllocationKey = "AllocationKey-2";
			var receiveLine3 = Helper.CreateWhsReceiveInventoryLine(receive, Data.ProductCRAHOLSYD, 3m, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			receiveLine3.WI_AllocationKey = "AllocationKey-3";

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.SaveForTesting();

			Data.ShipmentDataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].BondedWarehouseQuantity = 6m;

			var allocationInfo1 = new Mock<IWarehouseCustomsLineAllocationInfo>();
			allocationInfo1.Setup(a => a.AllocationKey).Returns("AllocationKey-1");
			allocationInfo1.Setup(a => a.Quantity).Returns(3m);

			var allocationInfo2 = new Mock<IWarehouseCustomsLineAllocationInfo>();
			allocationInfo2.Setup(a => a.AllocationKey).Returns("AllocationKey-3");
			allocationInfo2.Setup(a => a.Quantity).Returns(3m);

			var invoiceLine = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine();
			invoiceLine.BondedWarehouseQuantity = null;

			var customsLineDetails = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(
				newProductCode: IsChangeOfOwnership ? "W1" : "P1",
				newSerialNum: "",
				invoiceLine: invoiceLine,
				allocationInfos: new[] { allocationInfo1.Object, allocationInfo2.Object });

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(customsLineDetails: new[] { customsLineDetails.Object }, isChangeOfOwnership: IsChangeOfOwnership))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				var changeOfInventoryBO = reader.ReadIntoBusinessObject();
				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", changeOfInventoryBO);

				var order = changeOfInventoryBO.Order;
				AssertNotNull("Should have read in a Order.", order);

				var orderLines = order.Lines;
				AssertEquals("Should have read in two order lines.", 2, orderLines.Count);

				var orderLine1 = orderLines.First(l => l.WE_AllocationKey == "AllocationKey-1");
				AssertEquals("Should have read in Allocation Key Info correctly.", 3m, orderLine1.WE_TransactionQuantity);
				AssertEquals("Should have allocated correct inventory.", receiveLine1.PK, orderLine1.PickLines.Single().WZ_WE_InventoryLine);

				var orderLine2 = orderLines.First(l => l.WE_AllocationKey == "AllocationKey-3");
				AssertEquals("Should have read in Allocation Key Info correctly.", 3m, orderLine2.WE_TransactionQuantity);
				AssertEquals("Should have allocated correct inventory.", receiveLine3.PK, orderLine2.PickLines.Single().WZ_WE_InventoryLine);

				AssertNotNull("Should have read in a Receive.", changeOfInventoryBO.Receive);
			}
		}

		public void TestPopulateBusinessObject_AllocationInfos_AllocatesByKey_NoInventoryWithMatchingKey()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB(createInventory: false);

			var receive = Helper.CreateWhsReceive(Data.Orgs.CRAHOLSYD, Warehouse, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, Data.ProductCRAHOLSYD, 5m, ZDate.Empty, ZDate.Empty, "Red", "Medium", "S1234", "EntryNumber123-2");
			receiveLine1.WI_AllocationKey = "AllocationKey-1";

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.SaveForTesting();

			var allocationInfo1 = new Mock<IWarehouseCustomsLineAllocationInfo>();
			allocationInfo1.Setup(a => a.AllocationKey).Returns("AllocationKey-2");
			allocationInfo1.Setup(a => a.Quantity).Returns(5m);

			var line = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine();
			line.BondedWarehouseQuantity = null;

			var customsLineDetails = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(
				newProductCode: IsChangeOfOwnership ? "W1" : "P1",
				line,
				newSerialNum: "",
				allocationInfos: new[] { allocationInfo1.Object });

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(customsLineDetails: new[] { customsLineDetails.Object }, isChangeOfOwnership: IsChangeOfOwnership))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				AssertExceptionThrown("Not enough stock.", typeof(DataObjectReadFailureException), @"
Cannot Import Order
Order could not be created for Customs Job B123 because there are errors:
You do not have enough stock to fulfill shortfalls on this order
ENTRYNUMBER123-2 Product P1/P1 can not be ordered due to lack of stock. 5 was ordered, but 0 is available
".Trim(), () => reader.ReadIntoBusinessObject());
			}
		}

		public void TestPopulateBusinessObject_AllocationInfos_BondedWarehouseQtyProvided()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			Factory.SaveForTesting();

			var allocationInfo1 = new Mock<IWarehouseCustomsLineAllocationInfo>();
			allocationInfo1.Setup(a => a.AllocationKey).Returns("AllocationKey-1");
			allocationInfo1.Setup(a => a.Quantity).Returns(5m);

			var allocationInfo2 = new Mock<IWarehouseCustomsLineAllocationInfo>();
			allocationInfo2.Setup(a => a.AllocationKey).Returns("AllocationKey-2");
			allocationInfo2.Setup(a => a.Quantity).Returns(5m);

			var line = WarehouseCustomsDetailsProvidersMocks.CreateInvoiceLine();
			line.BondedWarehouseQuantity = 5m;

			var customsLineDetails = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(
				newProductCode: IsChangeOfOwnership ? "W1" : "P1",
				line,
				newSerialNum: "",
				allocationInfos: new[] { allocationInfo1.Object, allocationInfo2.Object });

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(customsLineDetails: new[] { customsLineDetails.Object }, isChangeOfOwnership: IsChangeOfOwnership))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				AssertExceptionThrown(typeof(DataObjectReadFailureException), "Cannot Import Customs Job B123 as Order Line Bonded Warehouse Quantity and Allocation Key Infos were both provided.", () => reader.ReadIntoBusinessObject());
			}
		}

		public void TestPopulateBusinessObject_AllocationInfos_EmptyCollection()
		{
			SetupProductsAndShipmentDataObjectForCustomsImportInDB();
			Factory.SaveForTesting();

			var customsLineDetails = WarehouseCustomsDetailsProvidersMocks.CreateWarehouseCustomsLineDetailsMock(
				newProductCode: IsChangeOfOwnership ? "W1" : "P1",
				newSerialNum: "",
				allocationInfos: Array.Empty<IWarehouseCustomsLineAllocationInfo>());

			var allocationMock = new AllocateFIFOLegacyMock();
			using (ObjectFactory.Substitute(SetupPutawayEngineMock_WE_WLEqualsWarehouseDefaultLocationPK()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(allocationMock))
			using (new WarehouseCustomsDetailsProvidersMocks(customsLineDetails: new[] { customsLineDetails.Object }, isChangeOfOwnership: IsChangeOfOwnership))
			{
				var reader = new WhsBondedChangeOfInventoryDataObjectReader(Data.ShipmentDataObject, Logger, Factory);
				var changeOfInventoryBO = reader.ReadIntoBusinessObject();
				AssertNotNull("Should have read in a new WhsBondedChangeOfInventory", changeOfInventoryBO);

				var order = changeOfInventoryBO.Order;
				AssertNotNull("Should have read in a Order.", order);

				var orderLines = order.Lines;
				AssertEquals("Should have read in a single order line.", 1, orderLines.Count);
				AssertEquals("Should have read in line correctly.", 5m, orderLines[0].WE_TransactionQuantity);

				AssertNotNull("Should have read in a Receive.", changeOfInventoryBO.Receive);
			}
		}

		#endregion

		#region Implementation

		protected void SetupProductsAndShipmentDataObjectForCustomsImportInDB(bool createInventory = true, bool createInventoryAsInwardProcessing = false, bool addImporterToXML = true, IEnumerable<RecipientRoleDetail> recipientRoles = null)
		{
			Data.SetupProductsAndShipmentDataObjectForCustomsImportInDB(addImporterToXML: addImporterToXML, isWarehouseCreatedAsVirtual: IsSourceWarehouseVirtual, createInventory: createInventory, createInventoryAsInwardProcessing: createInventoryAsInwardProcessing, recipientRoles: recipientRoles ?? DefaultRecipientRoles);
		}

		protected void ImportEventViaDataContextManager(UniversalEvent eventDataObject)
		{
			var message = GetQueuedUniversalEventMessage(eventDataObject);
			var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
			manager.Process(message);
		}

		protected abstract bool IsSourceWarehouseVirtual { get; }

		protected virtual bool IsDocketFinalisedOnImport => true;

		protected IEnumerable<RecipientRoleDetail> DefaultRecipientRoles => new[] { new RecipientRoleDetail { Type = RecipientRoleForTest } };

		protected abstract RecipientRoleType RecipientRoleForTest { get; }

		bool IsChangeOfOwnership => DefaultRecipientRoles.Any(r => r.Type == RecipientRoleType.BCO);

		OrgHeader NewOwnerForTest => IsChangeOfOwnership ? Data.Orgs.WUFSHIJNB : Data.Orgs.CRAHOLSYD;

		string NewProductCodeForTest => IsChangeOfOwnership ? Data.ProductWUFSHIJNB.OP_PartNum : Data.ProductCRAHOLSYD.OP_PartNum;

		protected override TestDataForUniversal GetNewTestData()
		{
			return new TestDataForUniversal(Factory, Logger, DataContextType.WarehouseBondedChangeOfInventory);
		}

		void FinaliseDocketsFromImport(WhsBondedChangeOfInventory changeOfInventoryBO)
		{
			if (!IsDocketFinalisedOnImport)
			{
				changeOfInventoryBO.Order.Logs.AddNew(Events.WarehouseJobCanNowBeFinalised);
				changeOfInventoryBO.Receive.Logs.AddNew(Events.WarehouseJobCanNowBeFinalised);
				Factory.SaveForTesting();

				changeOfInventoryBO.Order.FinaliseDocketWithoutUserConfirmation();
				changeOfInventoryBO.Order.Pick.FinalisePick();
				changeOfInventoryBO.Receive.FinaliseDocketWithoutUserConfirmation();
				Factory.SaveForTesting();
			}

			Assert(changeOfInventoryBO.Order.IsFinalised);
			Assert(changeOfInventoryBO.Receive.IsFinalised);
		}

		void WithdrawStockFromNewOwner(bool isSerialTest = false)
		{
			var order = Helper.CreateWhsOrder(NewOwnerForTest, Warehouse);
			var product = IsChangeOfOwnership ? Data.ProductWUFSHIJNB : Data.ProductCRAHOLSYD;
			var orderLine = Helper.CreateWhsOrderLine(order, product, isSerialTest ? 1m : 3m, IsChangeOfOwnership ? "DUMMYOUTWARD-1-102" : "DUMMYOUTWARD-1-102", "NewEK-1", "");
			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);
			pick.AutoAllocateItems();
			pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order);

			// pick stock to reduce total units
			orderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("Total Units should have been reduced.", isSerialTest ? 0m : 2m, order.Lines.Single().PickLines.Single().Inventory.WI_TotalUnits);
			Factory.SaveForTesting();
		}

		protected void AssertWhsDocket(WhsDocket docket, OrgHeader expectedClient, ZString expectedExternalReference, ZString expectedCustomsParentReference, bool isSaved = true, byte splitNumber = 0)
		{
			AssertNotNull("Should have read in a docket.", docket);
			AssertEquals("Docket Owner should be set.", expectedClient.PK, docket.WD_OH_Client);
			AssertEquals("Docket External Reference should be set.", expectedExternalReference, docket.WD_ExternalReference);
			AssertEquals("Docket Customs Parent Reference should be set.", expectedCustomsParentReference, docket.WD_CustomsParentReference);
			AssertEquals("Docket should be saved.", isSaved, docket.IsInDatabase);
			AssertEquals("Docket should have Split number.", splitNumber, docket.WD_ExternalReferenceSplit);

			if (!IsDocketFinalisedOnImport && !IsChangeOfOwnership)
			{
				AssertEquals("Docket should be held for Customs for Change of Regime or Warehouse.", true, docket.IsDocketHeldByCustoms);
			}
			else
			{
				AssertEquals("Docket should be finalised.", true, docket.IsFinalised);
			}
		}

		#region SetWhsBondedWarehouseAttribute

		static void SetWhsBondedWarehouseAttribute(WhsBondedWarehouseAttribute customsData)
		{
			customsData.WB_AddInfo = "From inventory";
			customsData.WB_CustomsQty = 10m;
			customsData.WB_CustomsUnitOfQty = "BAG";
			customsData.WB_RN_NKCountryOfOrigin = "UX";
			customsData.WB_TILV = 20m;
			customsData.WB_ValueForDuty = 30m;
			customsData.WB_DeclarationReference = "A000";
			customsData.WB_CustomsSecondQuantity = 40m;
			customsData.WB_CustomsSecondUnitQty = "CM";
			customsData.WB_Tariff = "T0";
			customsData.WB_PrimaryPreference = "P0";
			customsData.WB_CustomsThirdQuantity = 50;
			customsData.WB_CustomsThirdUnitQty = "KG";
		}

		#endregion

		protected override void TearDown()
		{
			Factory.Load<JobHeader>(new ZQuery { FetchOnlyFromLocalCache = true }).ForEach(j => j.Dispose());
			base.TearDown();
		}

		#endregion

		#region Warehouse

		protected WhsWarehouse Warehouse
		{
			get { return Data.GetOrCreateWarehouseInDB(isWarehouseCreatedAsVirtual: IsSourceWarehouseVirtual); }
		}

		#endregion

		#region Locations

		protected WhsLocation LocationA1
		{
			get { return Warehouse.FindLocation(IsSourceWarehouseVirtual ? "BOND" : "A-1-1-1"); }
		}

		protected WhsLocation LocationA2
		{
			get { return Warehouse.FindLocation(IsSourceWarehouseVirtual ? "BOND" : "A-2-1-1"); }
		}

		WhsLocation LocationA3
		{
			get { return Warehouse.FindLocation(IsSourceWarehouseVirtual ? "BOND" : "A-3-1-1"); }
		}

		WhsLocation LocationA4
		{
			get { return Warehouse.FindLocation(IsSourceWarehouseVirtual ? "BOND" : "A-4-1-1"); }
		}

		#endregion

		#region AssertLine

		protected static void AssertLine(WhsDocketLine line, ZString partNo, decimal qty, WhsLocation location, ZString docketLineEntryKey, ZString customsEntryNo, ZShort wbEntryLineNo, ZString attrib1, ZString attrib2, ZString attrib3, Func<WhsDocketLine, WhsLocation> getLineLocation = null, short lineNo = 3)
		{
			AsserLineValues(line, partNo, qty, location, docketLineEntryKey, customsEntryNo, wbEntryLineNo, attrib1, attrib2, attrib3, getLineLocation, lineNo);
			AssertBondedWarehouseAttribute(line);
		}

		protected static void AssertLineBondedWarehouseAttributeMatchWithInventory(WhsDocketLine line, ZString partNo, decimal qty, WhsLocation location, ZString docketLineEntryKey, ZString customsEntryNo, ZShort wbEntryLineNo, ZString attrib1, ZString attrib2, ZString attrib3, Func<WhsDocketLine, WhsLocation> getLineLocation = null, short lineNo = 3)
		{
			AsserLineValues(line, partNo, qty, location, docketLineEntryKey, customsEntryNo, wbEntryLineNo, attrib1, attrib2, attrib3, getLineLocation, lineNo);
			AssertBondedWarehouseAttributeMatchWithInventory(line);
		}

		static void AsserLineValues(WhsDocketLine line, ZString partNo, decimal qty, WhsLocation location, ZString docketLineEntryKey, ZString customsEntryNo, ZShort wbEntryLineNo, ZString attrib1, ZString attrib2, ZString attrib3, Func<WhsDocketLine, WhsLocation> getLineLocation, short lineNo = 3)
		{
			AssertEquals("Product Code", partNo, line.SupplierPart.OP_PartNum);
			AssertEquals("Bonded Qty", qty, line.WE_TransactionQuantity);
			AssertEquals("Bonded Qty UQ", "BOX", line.WE_F3_NKPackType);

			var actualLocation = getLineLocation != null ? getLineLocation(line) : line.Docket.WD_DocketType == DocketType.Codes.Order ? line.PickLines.Single().Inventory.Location : line.Location;
			AssertEquals("Location", location.PK, actualLocation.PK);

			AssertEquals("Order/Comm Inv Line No", lineNo, line.WE_LineNo);
			AssertEquals("Attrib1", attrib1, line.WE_PartAttrib1);
			AssertEquals("Attrib2", attrib2, line.WE_PartAttrib2);
			AssertEquals("Attrib3", attrib3, line.WE_PartAttrib3);
			AssertEquals("Bonded Entry Key", docketLineEntryKey, line.WE_BondedEntryKey);

			AssertEquals("Customs Entry Key", customsEntryNo, line.CustomsData.WB_EntryKey);
			AssertEquals("Customs Entry Line No.", wbEntryLineNo, line.CustomsData.WB_EntryLineNo);
		}

		static void AssertBondedWarehouseAttribute(WhsDocketLine line)
		{
			AssertEquals("Customs AddInfo", "Add info?", line.CustomsData.WB_AddInfo);
			AssertEquals("Customs Qty", 6m, line.CustomsData.WB_CustomsQty);
			AssertEquals("Customs Qty UQ", "PCE", line.CustomsData.WB_CustomsUnitOfQty);
			AssertEquals("Customs Country of Origin", "IT", line.CustomsData.WB_RN_NKCountryOfOrigin);
			AssertEquals("Customs TILV", 777m, line.CustomsData.WB_TILV);
			AssertEquals("Customs Value for Duty", 888m, line.CustomsData.WB_ValueForDuty);
			AssertEquals("Declaration Reference", "B123", line.CustomsData.WB_DeclarationReference);
			AssertEquals("Customs Second Qty", 12m, line.CustomsData.WB_CustomsSecondQuantity);
			AssertEquals("Customs Second Qty UQ", "BOX", line.CustomsData.WB_CustomsSecondUnitQty);
			AssertEquals("Customs Tariff", "T2", line.CustomsData.WB_Tariff);
			AssertEquals("Customs Primary Preference", "PP", line.CustomsData.WB_PrimaryPreference);
			AssertEquals("Customs Third Qty", 5m, line.CustomsData.WB_CustomsThirdQuantity);
			AssertEquals("Customs Third Qty UQ", "BOX", line.CustomsData.WB_CustomsThirdUnitQty);
			// ManufacturerAddress need to be tested after customs team implement it
		}

		static void AssertBondedWarehouseAttributeMatchWithInventory(WhsDocketLine line)
		{
			AssertEquals("Customs AddInfo", "Add info?*From inventory", line.CustomsData.WB_AddInfo);
			AssertEquals("Customs Qty", 10m, line.CustomsData.WB_CustomsQty);
			AssertEquals("Customs Qty UQ", "BAG", line.CustomsData.WB_CustomsUnitOfQty);
			AssertEquals("Customs Country of Origin", "UX", line.CustomsData.WB_RN_NKCountryOfOrigin);
			AssertEquals("Customs TILV", 20m, line.CustomsData.WB_TILV);
			AssertEquals("Customs Value for Duty", 30m, line.CustomsData.WB_ValueForDuty);
			AssertEquals("Declaration Reference", "B123", line.CustomsData.WB_DeclarationReference);
			AssertEquals("Customs Second Qty", 40m, line.CustomsData.WB_CustomsSecondQuantity);
			AssertEquals("Customs Second Qty UQ", "CM", line.CustomsData.WB_CustomsSecondUnitQty);
			AssertEquals("Customs Tariff", "T0", line.CustomsData.WB_Tariff);
			AssertEquals("Customs Primary Preference", "P0", line.CustomsData.WB_PrimaryPreference);
			AssertEquals("Customs Third Qty", 50m, line.CustomsData.WB_CustomsThirdQuantity);
			AssertEquals("Customs Third Qty UQ", "KG", line.CustomsData.WB_CustomsThirdUnitQty);
		}

		#endregion

		#region CreateWhsReceiveInventoryLineWithCustomsAttributeData

		protected WhsInventoryView CreateWhsReceiveInventoryLineWithCustomsAttributeData(WhsReceive receive, OrgSupplierPart part, ZDecimal units, WhsLocation location, ZDate ed, ZDate pd, string pa1, string pa2, string pa3, string bek)
		{
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, part, units, location, "", ed, pd, pa1, pa2, pa3, bek);
			SetWhsBondedWarehouseAttribute(inventory.CustomsData);
			return inventory;
		}

		#endregion

		#region WarehouseCustomsLineDetailsProvidersMock

		public class WarehouseCustomsDetailsProvidersMocks : IDisposable
		{
			public WarehouseCustomsDetailsProvidersMocks(IEnumerable<IWarehouseCustomsLineDetails> customsLineDetails = null, IWarehouseCustomsDetailsChangeOfOwnership customDetails = null, CustomsRegime intoRegimeType = CustomsRegime.BondedWarehouse, CustomsRegime outOfRegimeType = CustomsRegime.BondedWarehouse, bool isChangeOfOwnership = true, bool useSerial = false, OrganizationAddress newWarehouse = null, bool registerRegimeTypes = true)
			{
				var customsLineDetailProvider = new Mock<IWarehouseCustomsLineDetailsProvider>();
				customsLineDetailProvider.Setup(s => s.GetLineDetails()).Returns(customsLineDetails ?? new[] { CreateWarehouseCustomsLineDetailsMock(newProductCode: isChangeOfOwnership ? "W1" : "P1", newSerialNum: useSerial ? "SN3" : "").Object });
				var customsLineDetailProviders = new Hashtable { { "Shared", new TestObjectHandle(customsLineDetailProvider.Object) } };
				customsLineDetailProvidersSubstitution = ObjectFactory.Substitute("WarehouseCustomsLineDetailsProviders", customsLineDetailProviders);
				customsNctsLineDetailProvidersSubstitution = ObjectFactory.Substitute("WarehouseNctsCustomsLineDetailsProviders", customsLineDetailProviders);

				var defaultNewOwner = isChangeOfOwnership ? GetNewAddressData_WUFSHIJNB(DocAddressType.ImporterDocumentaryAddress) : GetNewAddressData_CRAHOLSYD(DocAddressType.ConsignorDocumentaryAddress);
				var customsDetailsProvider = customDetails ?? CreateWarehouseCustomsDetailsChangeOfOwnershipMock(newOwner: defaultNewOwner, newWarehouse: newWarehouse);
				var customsChangeOfOwnershipDetailProviders = new Hashtable { { "Shared", new TestObjectHandle(customsDetailsProvider) } };
				customsChangeOfOwnershipDetailProviderSubstitution = ObjectFactory.Substitute("WarehouseCustomsDetailsChangeOfOwnerships", customsChangeOfOwnershipDetailProviders);

				var customsChangeOfRegimeMock = CreateWarehouseCustomsDetailsChangeOfRegimeMock(intoRegimeType, outOfRegimeType, newWarehouse);
				var customsChangeOfRegimeDetails = new Hashtable { { "Shared", new TestObjectHandle(customsChangeOfRegimeMock) } };
				customsChangeOfRegimeDetailProviderSubstitution = ObjectFactory.Substitute("WarehouseCustomsDetailsChangeOfRegimes", customsChangeOfRegimeDetails);

				if (registerRegimeTypes)
				{
					var warehouseRegimeTypeProviderMock = CreateWarehouseRegimeTypeProviderMock(outOfRegimeType);
					var warehouseRegimeTypeProvider = new Hashtable { { "Shared", new TestObjectHandle(warehouseRegimeTypeProviderMock) } };
					customsWarehouseRegimeTypeProviderSubstitution = ObjectFactory.Substitute("WarehouseRegimeTypeProviders", warehouseRegimeTypeProvider);
				}
			}

			readonly IDisposable customsLineDetailProvidersSubstitution;
			readonly IDisposable customsNctsLineDetailProvidersSubstitution;
			readonly IDisposable customsChangeOfOwnershipDetailProviderSubstitution;
			readonly IDisposable customsChangeOfRegimeDetailProviderSubstitution;
			readonly IDisposable customsWarehouseRegimeTypeProviderSubstitution;

			public static IWarehouseCustomsDetailsChangeOfOwnership CreateWarehouseCustomsDetailsChangeOfOwnershipMock(OrganizationAddress oldOwner = null, OrganizationAddress newOwner = null, OrganizationAddress newWarehouse = null)
			{
				var result = new Mock<IWarehouseCustomsDetailsChangeOfOwnership>();
				result.Setup(s => s.OldOwner).Returns(oldOwner ?? OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(DocAddressType.ConsignorDocumentaryAddress));
				result.Setup(s => s.NewOwner).Returns(newOwner ?? OrganizationAddressTestHelper.GetNewAddressData_WUFSHIJNB(DocAddressType.ImporterDocumentaryAddress));
				result.Setup(s => s.NewWarehouse).Returns(newWarehouse);

				return result.Object;
			}

			public static IWarehouseCustomsDetailsChangeOfRegime CreateWarehouseCustomsDetailsChangeOfRegimeMock(CustomsRegime intoRegimeType = CustomsRegime.BondedWarehouse, CustomsRegime outOfRegimeType = CustomsRegime.BondedWarehouse, OrganizationAddress newWarehouse = null)
			{
				var result = new Mock<IWarehouseCustomsDetailsChangeOfRegime>();
				result.Setup(s => s.IntoRegimeType).Returns(intoRegimeType);
				result.Setup(s => s.OutOfRegimeType).Returns(outOfRegimeType);
				result.Setup(s => s.NewWarehouse).Returns(newWarehouse);

				return result.Object;
			}

			public static IWarehouseRegimeTypeProvider CreateWarehouseRegimeTypeProviderMock(CustomsRegime regimeType = CustomsRegime.BondedWarehouse)
			{
				var result = new Mock<IWarehouseRegimeTypeProvider>();
				result.Setup(s => s.GetCustomsRegime()).Returns(regimeType);
				return result.Object;
			}

			#region CreateWarehouseCustomsLineDetailsMock

			public static CommercialInvoiceLine CreateInvoiceLine(string previousProductCode = "P1", decimal orderedQty = 5m, string previousAttrib1 = "RED", string previousAttrib2 = "Medium", string previousAttrib3 = "S1234", string previousSerialNum = "")
			{
				var result = new CommercialInvoiceLine();
				result.Commodity = new Commodity { Code = "CMM", Description = "Comm" };
				result.BondedWarehouseQuantity = orderedQty;
				result.BondedWarehouseQuantityUnit = new CodeDescriptionPair { Code = "BOX", Description = "Box" };
				result.LineNo = 3;
				result.LinePrice = 10.3;
				result.PartNo = previousProductCode;
				result.CustomizedFieldCollection = new List<CustomizedField>()
				{
					CustomizedField.New("Colour", new ZString(previousAttrib1)),
					CustomizedField.New("Size", new ZString(previousAttrib2)),
					CustomizedField.New("Serial", new ZString(previousAttrib3)),
					CustomizedField.New("Serial Number", new ZString(previousSerialNum))
				};

				return result;
			}

			public static Mock<IWarehouseCustomsLineDetails> CreateWarehouseCustomsLineDetailsMock(string newProductCode, CommercialInvoiceLine invoiceLine = null, string newAttrib1 = "12", string newAttrib2 = "34", string newAttrib3 = "56", string previousEntryNo = "EntryNumber123", short previousEntryLineNo = 2, string entryNumber = "DummyOutward-1", short entryLineNo = 102, string orderNumber = "", string newSerialNum = "SN3", IEnumerable<IWarehouseCustomsLineAllocationInfo> allocationInfos = null)
			{
				var result = new Mock<IWarehouseCustomsLineDetails>();

				// IWarehouseCustomsLineDetails
				result.Setup(s => s.AddInfos).Returns("Add info?");
				result.Setup(s => s.CountryOfOrigin).Returns(new Country() { Code = "IT", Name = "Italy" });
				result.Setup(s => s.CustomsQuantity).Returns(6m);
				result.Setup(s => s.CustomsQuantityUnit).Returns(new CodeDescriptionPair6Char() { Code = "PCE", Description = "Piece" });
				result.Setup(s => s.CustomsSecondQuantity).Returns(12m);
				result.Setup(s => s.CustomsSecondQuantityUnit).Returns(new CodeDescriptionPair6Char() { Code = "BOX", Description = "BOX" });
				result.Setup(s => s.EntryLineNumber).Returns(entryLineNo);
				result.Setup(s => s.EntryNumber).Returns(entryNumber);
				result.Setup(s => s.ExtraClassificationDetails).Returns(new ZString[] { "ExtraClassificationDetails" });
				result.Setup(s => s.InvoiceLine).Returns(invoiceLine ?? CreateInvoiceLine());
				result.Setup(s => s.PackDetails).Returns<IEnumerable<IWarehouseCustomsLinePackDetails>>(null);// new[] { CreateWarehouseCustomsLinePackDetailsMock() }); for US
				result.Setup(s => s.PreviousEntryLineNumber).Returns(previousEntryLineNo);
				result.Setup(s => s.PreviousEntryNumber).Returns(previousEntryNo);
				result.Setup(s => s.OrderNumber).Returns(orderNumber);
				result.Setup(s => s.OrderLineNo).Returns(3);
				result.Setup(s => s.PrimaryPreference).Returns("PP");
				result.Setup(s => s.SupplierAddress).Returns<OrganizationAddress>(null);
				result.Setup(s => s.Tariff).Returns("T2");
				result.Setup(s => s.TILV).Returns(777m);
				result.Setup(s => s.ValueForDuty).Returns(888m);
				result.Setup(s => s.CustomsThirdQuantity).Returns(5m);
				result.Setup(s => s.CustomsThirdQuantityUnit).Returns(new CodeDescriptionPair6Char() { Code = "BOX", Description = "BOX" });

				result.Setup(s => s.AllocationInfos).Returns(allocationInfos ?? Array.Empty<IWarehouseCustomsLineAllocationInfo>());
				// ManufacturerAddress need to be tested after customs team implement it
				// IWarehouseCustomsLineDetailsChangeOfOwnership
				result.Setup(s => s.NewOwnerProductCode).Returns(newProductCode);
				result.Setup(s => s.NewOwnerPartAttribute1).Returns(newAttrib1);
				result.Setup(s => s.NewOwnerPartAttribute2).Returns(newAttrib2);
				result.Setup(s => s.NewOwnerPartAttribute3).Returns(newAttrib3);
				result.Setup(s => s.NewOwnerSerialNumber).Returns(newSerialNum);

				return result;
			}

			public static IUSWarehouseCustomsLineDetails CreateWarehouseCustomsLineDetailsMockUS(CommercialInvoiceLine invoiceLine = null, ZString? zoneStatusCode = null, ZBool? isFromOtherFTZ = null, OutwardType? outwardType = null)
			{
				var result = new Mock<IUSWarehouseCustomsLineDetails>();

				result.Setup(s => s.EntryNumber).Returns("DummyOutward-1");
				result.Setup(s => s.InvoiceLine).Returns(invoiceLine ?? CreateInvoiceLine());
				result.Setup(s => s.ZoneStatus).Returns(zoneStatusCode);
				result.Setup(s => s.FromOtherFTZ).Returns(isFromOtherFTZ);
				result.Setup(s => s.OutwardType).Returns(outwardType);

				return result.Object;
			}

			#endregion

			#region Dispose

			void IDisposable.Dispose()
			{
				customsLineDetailProvidersSubstitution.Dispose();
				customsNctsLineDetailProvidersSubstitution.Dispose();
				customsChangeOfOwnershipDetailProviderSubstitution.Dispose();
				customsChangeOfRegimeDetailProviderSubstitution.Dispose();
				customsWarehouseRegimeTypeProviderSubstitution?.Dispose();
			}

			#endregion
		}

		#endregion
	}

	#endregion
}
