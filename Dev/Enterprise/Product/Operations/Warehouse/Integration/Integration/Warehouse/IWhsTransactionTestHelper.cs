using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsTransactionTestHelper
	{
		BusinessObject CreateProductCategory(string categoryCode, string categoryDescription, ZGuid parentCategoryPK);

		BusinessObject CreateWarehouse(ZString name);
		BusinessObject CreateWarehouse(ZString name, string rowName);
		BusinessObject CreateWarehouse(ZString name, string rowName, short columns, short levels);
		BusinessObject CreateWarehouse(ZString code, IGlbBranch branch);
		BusinessObject CreateWarehouse(ZString name, IOrgAddress address, IGlbBranch branch);
		BusinessObject CreateWarehouse(ZString name, ZString code, string rowName);
		void SetUpBondedWarehouse(ZGuid whsPK, ZGuid addressPK);

		BusinessObject CreateTRWWarehouse();
		BusinessObject CreateTRWWarehouse(string warehouseCode, string rowName, short columns, short levels, string countrycode);

		IWhsRow CreateRow(IWhsWarehouse whs, string code);
		IWhsRow CreateRowAndGenerateLocations(IWhsWarehouse whs, string code, short cols = 1, short levels = 1);
		IWhsRow CreateRowAndGenerateLocations(IWhsWarehouse whs, string code, short cols, short levels, short trays, short rowSequence = 0);

		BusinessObject CreateWhsArea(ZGuid whsPK, ZString name);
		BusinessObject CreateWhsArea(ZGuid whsPK, ZString name, ZString type);

		IWhsLocation FindLocation(ZGuid whsPK, ZString locationString);
		void GenerateLocations(ZGuid whsPK);

		IWhsClientParameterByWarehouse CreateWhsClientParameterByWarehouse(ZGuid clientPK, ZGuid whsPK);

		BusinessObject CreateReceiveConsignment(string consignmentID, ZGuid warehousePK);

		ZGuid CreateClient(ZString clientCode);
		BusinessObject CreateClient(ZString clientCode, ZString clientName);
		BusinessObject CreateProduct(ZGuid orgPK, ZString productCode);
		BusinessObject CreateProductClientRelationShip(ZGuid ownerPK, ZGuid productPK);
		BusinessObject CreatePickface(ZGuid orgPK, ZGuid whsPK, ZGuid productPK, ZGuid locationPK);
		ZGuid CreatePickface(ZGuid orgPK, ZGuid whsPK, ZGuid productPK, ZString locnString);

		BusinessObject CreateProductParamsByWhsAndClient(ZGuid partPK, ZGuid clientPK, ZGuid warehousePK, ZDecimal minimum, ZDecimal economicQty, ZDecimal replenishmentMultiple, ZString receiveUQ, ZShort maximumShelfLife);

		ZGuid CreateWhsReceive(ZGuid orgPK, ZGuid whsPK, ZString reference, NotificationBuffer notify);
		ZGuid CreateWhsReceive(ZGuid orgPK, ZGuid whsPK, ZString reference, ZString docketSubType, NotificationBuffer notify);
		ZGuid CreateWhsReceiveInventoryLine(ZGuid docketPK, ZGuid productPK, ZDecimal quantity, ZGuid locationPK);
		ZGuid CreateWhsReceiveInventoryLine(ZGuid docketPK, ZGuid productPK, ZDecimal quantity, ZString locnString);
		ZGuid CreateWhsReceiveInventoryLine(ZGuid docketPK, ZGuid productPK, ZDecimal quantity, ZString locnString, ZString heldCode, string palletID = null);
		BusinessObject CreateWhsReceiveInventoryLine(ZGuid docketPK, ZGuid productPK, ZDecimal units, ZDate expiryDate, ZDate packingDate, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString entryKey);
		BusinessObject CreateWhsReceiveInventoryLine(ZGuid docketPK, ZGuid productPK, ZDecimal units, ZDate expiryDate, ZDate packingDate, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString entryKey, ZString locnString);
		IWhsInventoryView CreateWhsReceiveInventoryLine(ZGuid docketPK, ZGuid productPK, ZDecimal perPackageQty, ZDecimal units, ZDecimal currentQty, ZString packageGroupId, ZString packType, ZDateTimeOffset arrivalDate, ZString entryKey, ZString locnString);

		ZGuid CreateAsnLine(ZGuid docketPK, ZGuid productPK, ZDecimal units);

		ZGuid CreateWhsOrder(ZGuid orgPK, ZGuid whsPK, ZString reference, NotificationBuffer notify);
		BusinessObject CreateWhsOrder(ZGuid clientPK, ZGuid whsPK, ZGuid consigneePK, ZString reference);
		ZGuid CreateWhsOrderLine(ZGuid docketPK, ZGuid productPK, ZDecimal quantity);

		BusinessObject CreateWhsAdjustment(ZGuid orgPK, ZGuid whsPK, ZString reference, NotificationBuffer notify);
		ZGuid CreateWhsAdjustmentLine(ZGuid docketPK, ZGuid productPK, ZDecimal quantity, ZGuid locationPK);
		ZGuid CreateWhsAdjustmentLine(ZGuid docketPK, ZGuid productPK, ZDecimal quantity, ZString locnString);
		ZGuid CreateWhsAdjustmentLine(ZGuid docketPK, ZGuid productPK, ZDecimal quantity, ZString locnString, ZString pa1, ZString pa2, ZString pa3, string bondedEntryKey = "", decimal perPackageQty = 0m, string packageGroupID = "", string serialNumber = "");

		ZGuid CreateWhsTransfer(ZGuid orgPK, ZGuid whsPK, ZString reference, NotificationBuffer notify);
		ZGuid CreateWhsTransferLine(ZGuid docketPK, ZGuid productPK, ZDecimal quantity, ZGuid locationFromPK, ZGuid locationToPK);
		ZGuid CreateWhsTransferLine(ZGuid docketPK, ZGuid productPK, ZDecimal quantity, ZString locnStringFrom, ZString locnStringTo);
		ZGuid CreateWhsTransferLine(ZGuid docketPK, ZGuid productPK, ZDecimal quantity, ZString locnStringFrom, ZString locnStringTo, ZDateTimeOffset arrivalDate, ZDate expiryDate, ZDate packingDate, ZString pa1, ZString pa2, ZString pa3);

		IOrgAddress SetUpOrgAddress(string address1, string address2, string postCode, string city, string state, string portCode, IOrgHeader owner);

		BusinessObject CreateStock(ZGuid whsPK, ZGuid clientPK, ZGuid productPK, ZDecimal units);
		BusinessObject CreateStock(ZGuid whsPK, ZGuid clientPK, ZString reference, ZGuid productPK, ZDecimal units, string pA1 = "", string pA2 = "", string pA3 = "", string sn = "", string bEK = "");

		BusinessObject CreateWhsAdHocServiceJob(ZGuid warehousePK, ZGuid clientPK, ZDateTime billingDate, string customerReferenceNo = "");

		BusinessObject CreateWhsPickTrolleyJob(ZGuid trolleyPK, ZString jobStatus);

		BusinessObject CreateWhsPickTrolleySlot(ZGuid trolleyJobPK, ZGuid packagePK, ZShort slot);

		IWhsProductParamsByWhsAndClient CreateProductParamsByWhsAndClient(ZGuid partPK, ZGuid clientPK, ZGuid warehousePK, ZShort maximumShelfLife);

		IWhsProductParamsByWhsAndClient CreateProductParamsByWhsAndClient(ZGuid partPK, ZGuid clientPK, ZGuid warehousePK, ZGuid stagingAreaBOMPK, ZGuid putawayAreaPK);

		IWhsProductParamsByWhsAndClient GetProductParamsByWhsAndClient(ZGuid partPK, ZGuid warehousePK, ZGuid clientPK);

		ZGuid CreateWhsPick(ZGuid[] orderPKs);
		IEnumerable<IWhsPickLine> GetPickLines(ZGuid pickPK);

		void WhsReceiveAllocateLocationsMock(ZGuid docketPK);
		void WhsPickAllocationItems(ZGuid pickPK);
		void FinaliseDocket(ZGuid docketPK);
		void FinaliseDocketLine(ZGuid docketLinePK);
		void FinaliseDocketWithoutUserConfirmation(ZGuid docketPK);
		void FinalisePick(ZGuid pickPK);

		IDisposable MockOutboundDockDoorCreator();
		IWhsDocketLine PickAndMakeInTransitTransfer(IWhsPickLine pickLine, ZDateTime pickedTime, bool allowMultipleSteps = false);

		BusinessObject CreateWhsReceiveWithInventory(ZGuid clientPK, ZGuid whsWarehousePK, ZString reference, ZGuid partPK, ZDecimal units);
		void SetOrderType(ZGuid orderPK, ZString orderType, bool isImportingData = false);
		BusinessObject CreateWhsOrderLine(ZGuid docketPK, ZGuid productPK, ZDecimal quantity, ZString inwardsEntryKey, ZString outwardsEntryKey, ZString tariff, ZString countryOfOrigin, ZString primaryPreference, ZDecimal valueForDuty, ZDecimal customsQty, ZString customsUnitOfQty, ZDecimal customsSecondQuantity, ZString customsSecondUnitQty, ZString addInfo, ZDecimal bondedWhsQty, ZGuid manufacturerAddress, ZString zoneStatus);
		BusinessObject CreatePickNew(bool finaliseOrders, bool finalisePick, params ZGuid[] pickableDocketPKs);
		BusinessObject CreateWhsPickLine(IWhsDocketLine docketLine, IWhsInventoryView inventory, decimal qty);
		BusinessObject CreateFTZWarehouse(ZGuid addressPK);
		IDisposable UsePutawayEngineManagerMock();
		IDisposable UseAllocationEngineMock();
		BusinessObject CreateWhsSalesChannel(string code, string description);

		BusinessObject CreateInventoryHeldCode(ZString code, ZString description, bool isSystem = false);
		BusinessObject CreateInventoryHeldCode(ZString code, ZString description, ZGuid clientPK, bool isSystem = false);

		void EnableWarehouseForBond(IWhsWarehouse whs, bool enable);

		IWhsPickLine ReserveStockForOrderLineIfAbleTo(IWhsDocketLine orderLine, IWhsInventoryView inventory);

		BusinessObject CreateProductBOM(ZGuid partPK, ZGuid subPartGuid, ZDecimal componentQty);
		IWhsDocket CreateWhsWorkOrderWithLine(ZGuid clientPK, ZGuid warehousePK, ZGuid partPK, ZDecimal qty);

		BusinessObject CreateWhsReceiveTransportationUnit(ZString reference, ZGuid warehousePK, ZGuid locationPK, DateTimeOffset dateTimeOffset);
		BusinessObject CreateWhsItemPackageState(string status, IWhsLocation location, BusinessObject rtu, BusinessObject package, string unitType = "PKG");

		ZGuid CreateWhsLoad(ZGuid transportCompanyPK, ZGuid dockDoorPK, ZString taskPlanningStatus, string jobID = null);

		ZGuid CreateWhsCycleCountLocation(ZGuid locationPK, ZString granularity, ZString taskPlanningStatus);
	}
}
