using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	sealed class WhsWorkOrderDataObjectWriterTest : WhsDocketDataObjectWriterTest<WhsWorkOrder, WhsWorkOrderDataObjectWriter>
	{
		#region TestTopLevelDataContextType

		public void TestTopLevelDataContextType()
		{
			var whsWorkOrderBO = Factory.New<WhsWorkOrder>();
			var writer = GetNewDataObjectWriter(whsWorkOrderBO);
			AssertEquals(DataContextType.WarehouseWorkOrder, ((ITopLevelDataObjectWriter)writer).TopLevelDataContextType);
		}

		#endregion

		#region TestBasicOrderLevelFieldMappings

		public void TestBasicOrderLevelFieldMappings()
		{
			var requiredByDate = new ZDateTimeOffset(2011, 1, 5, 23, 59, 0);

			var whsPick = Factory.New<WhsPick>();
			whsPick.WP_PickNo = "P0000001";

			var consignee = Factory.NewWithValidTestData<OrgAddress>();
			consignee.OA_RL_NKRelatedPortCode = "AUBNE";

			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "WHS";
			warehouse.WW_WarehouseName = "Ware this!";
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUMEL";

			var whsWorkOrderBO = Factory.New<WhsWorkOrder>();
			whsPick.Orders.Add(whsWorkOrderBO);
			whsWorkOrderBO.WD_WW_Whs = warehouse.PK;
			whsWorkOrderBO.ConsigneeAddressPK = consignee.PK;
			whsWorkOrderBO.WD_OH_Client = GetOrganizationBO_INTHEMSYD(Factory.BOFactory).PK;
			whsWorkOrderBO.WD_PickOption = "AUT";
			whsWorkOrderBO.WD_TotalWeightUnit = "LB";
			whsWorkOrderBO.WD_TotalCubicUnit = "CF";
			whsWorkOrderBO.WD_PackagesSent = 5;
			whsWorkOrderBO.WD_DocketID = "W10001011";
			whsWorkOrderBO.WD_DocketStatus = "PIC";
			whsWorkOrderBO.WD_DocketSubType = "ASS";
			whsWorkOrderBO.WD_ExternalReference = "ORDER123";
			whsWorkOrderBO.WD_ExternalReferenceSplit = 1;
			whsWorkOrderBO.WD_RequiredDate = requiredByDate;
			whsWorkOrderBO.WD_TotalCubic = 23.3m;
			whsWorkOrderBO.WD_TotalUnits = 7.1m;
			whsWorkOrderBO.WD_TotalWeight = 45.8m;

			var whsWorkOrderData = GetNewDataObjectWriter(whsWorkOrderBO).GetDataObject(whsWorkOrderBO);
			AssertNotNull(whsWorkOrderData);

			CombineAssertions(() =>
			{
				AssertEquals("whsWorkOrderData.OuterPacks", 5, whsWorkOrderData.OuterPacks);
				AssertEquals("whsWorkOrderData.OuterPacksPackageType.Code", "PCE", whsWorkOrderData.OuterPacksPackageType.Code);
				AssertEquals("whsWorkOrderData.OuterPacksPackageType.Description", "Piece", whsWorkOrderData.OuterPacksPackageType.Description);

				AssertEquals("whsWorkOrderData.TotalVolume", 23.3m, whsWorkOrderData.TotalVolume);
				AssertEquals("whsWorkOrderData.TotalVolumeUnit.Code", "CF", whsWorkOrderData.TotalVolumeUnit.Code);
				AssertEquals("whsWorkOrderData.TotalVolumeUnit.Description", "Cubic Feet", whsWorkOrderData.TotalVolumeUnit.Description);
				AssertEquals("whsWorkOrderData.TotalWeight", 45.8m, whsWorkOrderData.TotalWeight);
				AssertEquals("whsWorkOrderData.TotalWeightUnit.Code", "LB", whsWorkOrderData.TotalWeightUnit.Code);
				AssertEquals("whsWorkOrderData.TotalWeightUnit.Description", "Pounds", whsWorkOrderData.TotalWeightUnit.Description);

				var localProcessing = whsWorkOrderData.LocalProcessing;
				AssertEquals("localProcessing.DeliveryRequiredBy", requiredByDate.ToZDateTime(), localProcessing.DeliveryRequiredBy);

				var order = whsWorkOrderData.Order;
				AssertEquals("order.Warehouse.Code", "WHS", order.Warehouse.Code);
				AssertEquals("order.Warehouse.Name", "Ware this!", order.Warehouse.Name);
				AssertEquals("order.OrderNumber", "ORDER123", order.OrderNumber);
				AssertEquals("order.OrderNumberSplit", new ZByte(1), order.OrderNumberSplit);
				AssertEquals("order.Type.Code", "ASS", order.Type.Code);
				AssertEquals("order.Type.Description", "Assemble", order.Type.Description);
				AssertEquals("order.Status.Code", "PIC", order.Status.Code);
				AssertEquals("order.Status.Description", "Picking", order.Status.Description);
				AssertEquals("order.PickOption.Code", "AUT", order.PickOption.Code);
				AssertEquals("order.PickOption.Description", "Auto Pick", order.PickOption.Description);
				AssertEquals("order.TotalUnits", 7.1m, order.TotalUnits);
				AssertEquals("order.TotalLineVolume", 23.3m, order.TotalLineVolume);
				AssertEquals("order.TotalLineWeight", 45.8m, order.TotalLineWeight);
			});

			var clientAddress = whsWorkOrderData.OrganizationAddressCollection.Single(o => o.AddressType.Value == "ConsignorDocumentaryAddress");
			AssertOrganizationBO_INTHEMSYD("ConsignorDocumentaryAddress", clientAddress, "ConsignorDocumentaryAddress");
		}

		#endregion

		#region TestType

		public void TestType_Assembly()
		{
			var whsWorkOrderBO = Factory.NewWithValidTestData<WhsWorkOrder>();
			whsWorkOrderBO.WD_DocketSubType = "ASS";

			var whsWorkOrderData = GetNewDataObjectWriter(whsWorkOrderBO).GetDataObject(whsWorkOrderBO);
			AssertNotNull(whsWorkOrderData);
			var order = whsWorkOrderData.Order;
			AssertEquals("order.Type.Code", "ASS", order.Type.Code);
			AssertEquals("order.Type.Description", "Assemble", order.Type.Description);
		}

		public void TestType_Disassembly()
		{
			var whsWorkOrderBO = Factory.NewWithValidTestData<WhsWorkOrder>();
			whsWorkOrderBO.WD_DocketSubType = "DIS";

			var whsWorkOrderData = GetNewDataObjectWriter(whsWorkOrderBO).GetDataObject(whsWorkOrderBO);
			AssertNotNull(whsWorkOrderData);
			var order = whsWorkOrderData.Order;
			AssertEquals("order.Type.Code", "DIS", order.Type.Code);
			AssertEquals("order.Type.Description", "Disassemble", order.Type.Description);
		}

		#endregion

		#region TestOuterPacks

		public void TestOuterPacks_TotalPallets()
		{
			var whsWorkOrderBO = Factory.NewWithValidTestData<WhsWorkOrder>();
			whsWorkOrderBO.WD_TotalPallets = 7;
			whsWorkOrderBO.WD_PackagesSent = 42;
			whsWorkOrderBO.WD_TotalUnits = 3.14m;

			var whsWorkOrderData = GetNewDataObjectWriter(whsWorkOrderBO).GetDataObject(whsWorkOrderBO);
			AssertNotNull(whsWorkOrderData);

			AssertEquals("Should preference total pallets.", 7, whsWorkOrderData.OuterPacks);
		}

		public void TestOuterPacks_PackagesSent()
		{
			var whsWorkOrderBO = Factory.NewWithValidTestData<WhsWorkOrder>();
			whsWorkOrderBO.WD_TotalPallets = 0;
			whsWorkOrderBO.WD_PackagesSent = 42;
			whsWorkOrderBO.WD_TotalUnits = 3.14m;

			var whsWorkOrderData = GetNewDataObjectWriter(whsWorkOrderBO).GetDataObject(whsWorkOrderBO);
			AssertNotNull(whsWorkOrderData);

			AssertEquals("Should preference packages sent.", 42, whsWorkOrderData.OuterPacks);
		}

		public void TestOuterPacks_TotalUnits()
		{
			var whsWorkOrderBO = Factory.NewWithValidTestData<WhsWorkOrder>();
			whsWorkOrderBO.WD_TotalPallets = 0;
			whsWorkOrderBO.WD_PackagesSent = 0;
			whsWorkOrderBO.WD_TotalUnits = 3m;

			var whsWorkOrderData = GetNewDataObjectWriter(whsWorkOrderBO).GetDataObject(whsWorkOrderBO);
			AssertNotNull(whsWorkOrderData);

			AssertEquals("Should use total units.", 3, whsWorkOrderData.OuterPacks);
		}

		public void TestOuterPacks_TotalUnits_Rounding()
		{
			var whsWorkOrderBO = Factory.NewWithValidTestData<WhsWorkOrder>();
			whsWorkOrderBO.WD_TotalPallets = 0;
			whsWorkOrderBO.WD_PackagesSent = 0;
			whsWorkOrderBO.WD_TotalUnits = 3.14m;

			var whsWorkOrderData = GetNewDataObjectWriter(whsWorkOrderBO).GetDataObject(whsWorkOrderBO);
			AssertNotNull(whsWorkOrderData);

			AssertEquals("Should truncate total units, in line with orders/receives.", 3, whsWorkOrderData.OuterPacks);
		}

		public void TestOuterPacks_Null()
		{
			var whsWorkOrderBO = Factory.NewWithValidTestData<WhsWorkOrder>();
			whsWorkOrderBO.WD_TotalPallets = 0;
			whsWorkOrderBO.WD_PackagesSent = 0;
			whsWorkOrderBO.WD_TotalUnits = 0m;

			var whsWorkOrderData = GetNewDataObjectWriter(whsWorkOrderBO).GetDataObject(whsWorkOrderBO);
			AssertNotNull(whsWorkOrderData);

			AssertNull("Should not have set outer packs.", whsWorkOrderData.OuterPacks);
		}

		public void TestOuterPacks_TotalUnitsExceedsMaximumIntQuantity()
		{
			var whsDocketBO = GetNewDocket();
			whsDocketBO.WD_TotalUnits = int.MaxValue + 5000m;

			var notifications = new TestNotificationBuffer();
			var whsDocketData = GetNewDataObjectWriter(whsDocketBO, notifications).GetDataObject(whsDocketBO);

			AssertNotNull("whsDocketData", whsDocketData);
			AssertEquals("whsDocketData.OuterPacks", null, whsDocketData.OuterPacks);

			var notification = notifications.LastEvent;
			AssertEquals(NotificationType.Warning, notification.Type);
			AssertEquals($"Unable to convert WD_TotalUnits to an integer, as it has value {whsDocketBO.WD_TotalUnits}.", notification.Message);
		}

		#endregion

		#region TestPickPriority

		public void TestPickPriority()
		{
			var whsWorkOrderBO = Factory.NewWithValidTestData<WhsWorkOrder>();
			whsWorkOrderBO.WD_PickPriority = 3;

			var whsWorkOrderData = GetNewDataObjectWriter(whsWorkOrderBO).GetDataObject(whsWorkOrderBO);
			AssertNotNull(whsWorkOrderData);

			AssertEquals((byte)3, whsWorkOrderData.Order.PickPriority);
		}

		#endregion

		#region TestOuterPacksPackageType

		public void TestOuterPacksPackageType_PalletSentLessThanZero()
		{
			var whsWorkOrderBO = Factory.NewWithValidTestData<WhsWorkOrder>();
			whsWorkOrderBO.WD_TotalPallets = -1;

			var whsWorkOrderData = GetNewDataObjectWriter(whsWorkOrderBO).GetDataObject(whsWorkOrderBO);
			AssertNotNull(whsWorkOrderData);

			AssertEquals("Outer package type should be pieces.", Constants.PkgUnit.Piece, whsWorkOrderData.OuterPacksPackageType.Code);
			AssertEquals("Outer package type should be pieces.", Constants.PkgUnit.GetDescription(Constants.PkgUnit.Piece), whsWorkOrderData.OuterPacksPackageType.Description);
		}

		public void TestOuterPacksPackageType_PalletSentZero()
		{
			var whsWorkOrderBO = Factory.NewWithValidTestData<WhsWorkOrder>();
			whsWorkOrderBO.WD_TotalPallets = 0;

			var whsWorkOrderData = GetNewDataObjectWriter(whsWorkOrderBO).GetDataObject(whsWorkOrderBO);
			AssertNotNull(whsWorkOrderData);

			AssertEquals("Outer package type should be pieces.", Constants.PkgUnit.Piece, whsWorkOrderData.OuterPacksPackageType.Code);
			AssertEquals("Outer package type should be pieces.", Constants.PkgUnit.GetDescription(Constants.PkgUnit.Piece), whsWorkOrderData.OuterPacksPackageType.Description);
		}

		public void TestOuterPacksPackageType_PalletSentGreaterThanZero()
		{
			var whsWorkOrderBO = Factory.NewWithValidTestData<WhsWorkOrder>();
			whsWorkOrderBO.WD_TotalPallets = 1;

			var whsWorkOrderData = GetNewDataObjectWriter(whsWorkOrderBO).GetDataObject(whsWorkOrderBO);
			AssertNotNull(whsWorkOrderData);

			AssertEquals("Outer package type should be pallets.", Constants.PkgUnit.Pallet, whsWorkOrderData.OuterPacksPackageType.Code);
			AssertEquals("Outer package type should be pallets.", Constants.PkgUnit.GetDescription(Constants.PkgUnit.Pallet), whsWorkOrderData.OuterPacksPackageType.Description);
		}

		#endregion

		#region TestIsInwardProcessingJob

		public void TestIsInwardProcessingJob_False() => TestIsInwardProcessingJob(isInwardProcessing: false);
		public void TestIsInwardProcessingJob_True() => TestIsInwardProcessingJob(isInwardProcessing: true);

		void TestIsInwardProcessingJob(bool isInwardProcessing)
		{
			var whsWorkOrderBO = Factory.NewWithValidTestData<WhsWorkOrder>();
			whsWorkOrderBO.WD_IsInwardsProcessingJob = isInwardProcessing;

			var whsWorkOrderData = GetNewDataObjectWriter(whsWorkOrderBO).GetDataObject(whsWorkOrderBO);
			AssertEquals(isInwardProcessing, whsWorkOrderData.Order.IsInwardsProcessingJob);
		}

		#endregion

		#region TestAutoFinaliseBOMIntoInventory

		public void TestAutoFinaliseBOMIntoInventory_False() => TestAutoFinaliseBOMIntoInventory(autoFinaliseBOMIntoInventory: false);
		public void TestAutoFinaliseBOMIntoInventory_True() => TestAutoFinaliseBOMIntoInventory(autoFinaliseBOMIntoInventory: true);

		void TestAutoFinaliseBOMIntoInventory(bool autoFinaliseBOMIntoInventory)
		{
			var whsWorkOrderBO = Factory.NewWithValidTestData<WhsWorkOrder>();
			whsWorkOrderBO.WD_AutoFinaliseBOMIntoInventory = autoFinaliseBOMIntoInventory;

			var whsWorkOrderData = GetNewDataObjectWriter(whsWorkOrderBO).GetDataObject(whsWorkOrderBO);
			AssertEquals(autoFinaliseBOMIntoInventory, whsWorkOrderData.Order.AutoFinaliseBOMIntoInventory);
		}

		#endregion

		#region TestAdditionalReferences

		public void TestAdditionalReferences()
		{
			var whsDocketBO = GetNewDocket();

			var reference1 = whsDocketBO.References.AddNew();
			reference1.WX_RefType = "HSB";
			reference1.WX_Reference = "BILL";

			var reference2 = whsDocketBO.References.AddNew();
			reference2.WX_RefType = "MAR";
			reference2.WX_Reference = "100001133";

			var whsDocketData = GetNewDataObjectWriter(whsDocketBO).GetDataObject(whsDocketBO);

			AssertNotNull("whsDocketData", whsDocketData);
			AssertEquals("whsDocketData.AdditionalReferenceCollection.Count", 2, whsDocketData.AdditionalReferenceCollection.Count);

			CombineAssertions(delegate
			{
				var universalReference1 = whsDocketData.AdditionalReferenceCollection[0];
				AssertEquals("reference1.Type.Code", "HSB", universalReference1.Type.Code);
				AssertEquals("reference1.Type.Description", "House Bill", universalReference1.Type.Description);
				AssertEquals("reference1.ReferenceNumber", "BILL", universalReference1.ReferenceNumber);

				var universalReference2 = whsDocketData.AdditionalReferenceCollection[1];
				AssertEquals("reference2.Type.Code", "MAR", universalReference2.Type.Code);
				AssertEquals("reference2.Type.Description", "Marks and Numbers", universalReference2.Type.Description);
				AssertEquals("reference2.ReferenceNumber", "100001133", universalReference2.ReferenceNumber);
			});
		}

		#endregion

		#region TestOrganizationAddressCollection

		public void TestOrganizationAddressCollection()
		{
			var whsWorkOrderBO = Factory.New<WhsWorkOrder>();
			whsWorkOrderBO.WD_OH_Client = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).PK;
			whsWorkOrderBO.ConsigneeDocAddress.E2_OA_Address = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory).MainAddress.PK;
			whsWorkOrderBO.TransportCoDocAddress.E2_OA_Address = GetOrganizationBO_INTHEMSYD(Factory.BOFactory).MainAddress.PK;

			var whsWorkOrderData = GetNewDataObjectWriter(whsWorkOrderBO).GetDataObject(whsWorkOrderBO);
			AssertNotNull("whsWorkOrderData", whsWorkOrderData);
			AssertEquals("whsWorkOrderData.OrganizationAddressCollection.Count", 3, whsWorkOrderData.OrganizationAddressCollection.Count);

			var clientAddress = whsWorkOrderData.OrganizationAddressCollection.Single(o => o.AddressType.Value == "ConsignorDocumentaryAddress");
			AssertOrganizationBO_WUFSHIJNB("ConsignorDocumentaryAddress", clientAddress, "ConsignorDocumentaryAddress");

			var consigneeAddress = whsWorkOrderData.OrganizationAddressCollection.Single(o => o.AddressType.Value == "ConsigneeAddress");
			AssertOrganizationBO_CRAHOLSYD("ConsigneeAddress", consigneeAddress, "ConsigneeAddress", true);

			var transportCoAddress = whsWorkOrderData.OrganizationAddressCollection.Single(o => o.AddressType.Value == "TransportCompanyDocumentaryAddress");
			AssertOrganizationBO_INTHEMSYD("TransportCompanyDocumentaryAddress", transportCoAddress, "TransportCompanyDocumentaryAddress", true);
		}

		#endregion

		#region TestOrderLines

		public void TestOrderLines()
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsWorkOrderLine(workOrder, data.Part1, 10m);
			var line2 = Helper.CreateWhsWorkOrderLine(workOrder, data.Part2, 3.14m);

			var whsWorkOrderData = GetNewDataObjectWriter(workOrder).GetDataObject(workOrder);
			AssertNotNull(whsWorkOrderData);
			AssertEquals("whsWorkOrderData.Order.OrderLineCollection.Count", 2, whsWorkOrderData.Order.OrderLineCollection.Count);

			var lineData1 = whsWorkOrderData.Order.OrderLineCollection[0];
			AssertEquals("lineData.Product.Code", data.Part1.OP_PartNum, lineData1.Product.Code);
			AssertEquals("lineData.OrderedQty", 10m, lineData1.OrderedQty);

			var lineData2 = whsWorkOrderData.Order.OrderLineCollection[1];
			AssertEquals("lineData.Product.Code", data.Part2.OP_PartNum, lineData2.Product.Code);
			AssertEquals("lineData.OrderedQty", 3.14m, lineData2.OrderedQty);
		}

		public void TestOrderLines_ExcludesComponentLines()
		{
			var factory2 = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory2);
			var data = new TestDataSimpleEnvironment(factory2);
			var part3 = helper.CreateProduct("P3", data.Org1);
			var part4 = helper.CreateProduct("P4", data.Org1);
			helper.CreateProductBOM(data.Part2, data.Part1, 3m, "UNT");
			var bom2 = helper.CreateProductBOM(data.Part2, part3, 2m, "UNT");
			bom2.OE_ExcludeForVirtualWarehouse = true;
			helper.CreateProductBOM(data.Part2, part4, 1m, "UNT");
			factory2.Save();

			var location = data.Whs1.DefaultLocation;
			var componentReceive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = helper.CreateWhsReceiveLine(componentReceive, data.Part1, 15m, location, "", "");
			var receiveLine2 = helper.CreateWhsReceiveLine(componentReceive, part3, 10m, location, "", "");
			var receiveLine3 = helper.CreateWhsReceiveLine(componentReceive, part4, 5m, location, "", "");

			componentReceive.FinaliseDocket();
			AssertIsFinalisedPrecondition(componentReceive);
			factory2.Save();

			var workOrder = helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 5m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;

			helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);
			AssertEquals("Work order all lines correct.", 4, workOrder.AllLines.Count);

			var whsWorkOrderData = GetNewDataObjectWriter(workOrder).GetDataObject(workOrder);
			AssertNotNull(whsWorkOrderData);
			AssertEquals("whsWorkOrderData.Order.OrderLineCollection.Count", 1, whsWorkOrderData.Order.OrderLineCollection.Count);

			var lineData1 = whsWorkOrderData.Order.OrderLineCollection[0];
			AssertEquals("lineData.Product.Code", data.Part2.OP_PartNum, lineData1.Product.Code);
			AssertEquals("lineData.OrderedQty", 5m, lineData1.OrderedQty);
		}

		#endregion

		#region Implementation

		protected override WhsWorkOrder GetNewDocket() => Factory.NewWithValidTestData<WhsWorkOrder>();

		protected override WhsWorkOrderDataObjectWriter GetNewDataObjectWriter(BusinessObject topLevelBO, INotifications notifications)
			=> new WhsWorkOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, topLevelBO) { Notifications = notifications }));

		protected override TestDataForUniversal GetNewTestData() => new TestDataForUniversal(Factory, Logger, DataContextType.WarehouseWorkOrder);

		#endregion
	}
}
