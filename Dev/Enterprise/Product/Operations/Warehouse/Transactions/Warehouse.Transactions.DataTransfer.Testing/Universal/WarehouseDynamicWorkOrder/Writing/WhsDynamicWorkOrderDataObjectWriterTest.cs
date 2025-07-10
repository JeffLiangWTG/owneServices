using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using NotificationType = CargoWise.EntityFramework.NotificationType;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	sealed class WhsDynamicWorkOrderDataObjectWriterTest : WhsDocketDataObjectWriterTest<WhsDynamicWorkOrder, WhsDynamicWorkOrderDataObjectWriter>
	{
		public void TestTopLevelDataContextType()
		{
			var whsDynamicWorkOrderBO = Factory.New<WhsDynamicWorkOrder>();
			var writer = GetNewDataObjectWriter(whsDynamicWorkOrderBO);
			AssertEquals(DataContextType.WarehouseDynamicWorkOrder, ((ITopLevelDataObjectWriter)writer).TopLevelDataContextType);
		}

		public void TestOrderLevelFieldMappings()
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

			var whsDynamicWorkOrderBO = Factory.New<WhsDynamicWorkOrder>();
			whsPick.Orders.Add(whsDynamicWorkOrderBO);
			whsDynamicWorkOrderBO.WD_WW_Whs = warehouse.PK;
			whsDynamicWorkOrderBO.ConsigneeAddressPK = consignee.PK;
			whsDynamicWorkOrderBO.WD_OH_Client = GetOrganizationBO_INTHEMSYD(Factory.BOFactory).PK;
			whsDynamicWorkOrderBO.WD_PickOption = "AUT";
			whsDynamicWorkOrderBO.WD_TotalWeightUnit = "LB";
			whsDynamicWorkOrderBO.WD_TotalCubicUnit = "CF";
			whsDynamicWorkOrderBO.WD_PackagesSent = 5;
			whsDynamicWorkOrderBO.WD_DocketID = "W10001011";
			whsDynamicWorkOrderBO.WD_DocketStatus = "PIC";
			whsDynamicWorkOrderBO.WD_DocketSubType = "ASS";
			whsDynamicWorkOrderBO.WD_ExternalReference = "ORDER123";
			whsDynamicWorkOrderBO.WD_ExternalReferenceSplit = 1;
			whsDynamicWorkOrderBO.WD_RequiredDate = requiredByDate;
			whsDynamicWorkOrderBO.WD_TotalCubic = 23.3m;
			whsDynamicWorkOrderBO.WD_TotalUnits = 7.1m;
			whsDynamicWorkOrderBO.WD_TotalWeight = 45.8m;

			var whsDynamicWorkOrderData = GetNewDataObjectWriter(whsDynamicWorkOrderBO).GetDataObject(whsDynamicWorkOrderBO);
			AssertNotNull(whsDynamicWorkOrderData);

			CombineAssertions(() =>
			{
				AssertEquals("whsWorkOrderData.OuterPacks", 7, whsDynamicWorkOrderData.OuterPacks);
				AssertEquals("whsWorkOrderData.OuterPacksPackageType.Code", "PCE", whsDynamicWorkOrderData.OuterPacksPackageType.Code);
				AssertEquals("whsWorkOrderData.OuterPacksPackageType.Description", "Piece", whsDynamicWorkOrderData.OuterPacksPackageType.Description);

				AssertEquals("whsWorkOrderData.TotalVolume", 23.3m, whsDynamicWorkOrderData.TotalVolume);
				AssertEquals("whsWorkOrderData.TotalVolumeUnit.Code", "CF", whsDynamicWorkOrderData.TotalVolumeUnit.Code);
				AssertEquals("whsWorkOrderData.TotalVolumeUnit.Description", "Cubic Feet", whsDynamicWorkOrderData.TotalVolumeUnit.Description);
				AssertEquals("whsWorkOrderData.TotalWeight", 45.8m, whsDynamicWorkOrderData.TotalWeight);
				AssertEquals("whsWorkOrderData.TotalWeightUnit.Code", "LB", whsDynamicWorkOrderData.TotalWeightUnit.Code);
				AssertEquals("whsWorkOrderData.TotalWeightUnit.Description", "Pounds", whsDynamicWorkOrderData.TotalWeightUnit.Description);

				var localProcessing = whsDynamicWorkOrderData.LocalProcessing;
				AssertEquals("localProcessing.DeliveryRequiredBy", requiredByDate.ToZDateTime(), localProcessing.DeliveryRequiredBy);

				var order = whsDynamicWorkOrderData.Order;
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

			var clientAddress = whsDynamicWorkOrderData.OrganizationAddressCollection.Single(o => o.AddressType.Value == "ConsignorDocumentaryAddress");
			AssertOrganizationBO_INTHEMSYD("ConsignorDocumentaryAddress", clientAddress, "ConsignorDocumentaryAddress");
		}

		public void TestIsInwardsProcessingJob_False() => TestIsInwardsProcessingJob(isInwardProcessing: false);
		public void TestIsInwardsProcessingJob_True() => TestIsInwardsProcessingJob(isInwardProcessing: true);

		void TestIsInwardsProcessingJob(bool isInwardProcessing)
		{
			var whsDynamicWorkOrderBO = Factory.NewWithValidTestData<WhsDynamicWorkOrder>();
			whsDynamicWorkOrderBO.WD_IsInwardsProcessingJob = isInwardProcessing;

			var whsDynamicWorkOrderData = GetNewDataObjectWriter(whsDynamicWorkOrderBO).GetDataObject(whsDynamicWorkOrderBO);
			AssertEquals(isInwardProcessing, whsDynamicWorkOrderData.Order.IsInwardsProcessingJob);
		}

		public void TestOrderLines()
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			var dynamicWorkOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1);
			Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, data.Part1, 10m);
			Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, data.Part2, 3.14m);

			var whsDynamicWorkOrderData = GetNewDataObjectWriter(dynamicWorkOrder).GetDataObject(dynamicWorkOrder);
			AssertNotNull(whsDynamicWorkOrderData);
			AssertEquals("whsWorkOrderData.Order.OrderLineCollection.Count", 2, whsDynamicWorkOrderData.Order.OrderLineCollection.Count);

			var lineData1 = whsDynamicWorkOrderData.Order.OrderLineCollection[0];
			AssertEquals("lineData.Product.Code", data.Part1.OP_PartNum, lineData1.Product.Code);
			AssertEquals("lineData.OrderedQty", 10m, lineData1.OrderedQty);

			var lineData2 = whsDynamicWorkOrderData.Order.OrderLineCollection[1];
			AssertEquals("lineData.Product.Code", data.Part2.OP_PartNum, lineData2.Product.Code);
			AssertEquals("lineData.OrderedQty", 3.14m, lineData2.OrderedQty);
		}

		public void TestOrderLines_CustomsData()
		{
			var data = new TestDataSimpleEnvironment(Factory.BOFactory, saveFactory_doNotUseForNewTests: false);
			var dynamicWorkOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1);
			var dynamicWorkOrderLine = Helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, data.Part1, 10m);
			WhsBondedWarehouseAttributeDataObjectWriterTest.GetCustomsData(dynamicWorkOrderLine);

			var whsDynamicWorkOrderData = GetNewDataObjectWriter(dynamicWorkOrder).GetDataObject(dynamicWorkOrder);
			AssertNotNull(whsDynamicWorkOrderData);
			WhsDynamicWorkOrderBondedWarehouseAttributeDataObjectWriterTest.AssertContents(whsDynamicWorkOrderData.Order.OrderLineCollection.Single().CustomsData, isComponentLine: false);
		}

		public void TestChildComponentLines()
		{
			var factory2 = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory2);
			var data = new TestDataSimpleEnvironment(factory2);
			helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var area = helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;

			var mainProduct = helper.CreateProduct("BIKE", data.Org1);
			var componentProduct1 = helper.CreateProduct("WHEEL", data.Org1);
			var componentProduct2 = helper.CreateProduct("ENGINE", data.Org1);
			factory2.Save();

			var dynamicWorkOrder = helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1, "ExtRef");
			dynamicWorkOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			dynamicWorkOrder.WD_IsInwardsProcessingJob = true;

			var workOrderLineMain = helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, mainProduct, 5m);
			workOrderLineMain.IsMainInwardProcessedItem = true;

			var workOrderLineComponent1 = helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, componentProduct1, 5m);
			workOrderLineComponent1.WE_WE_ParentDocketLine = workOrderLineMain.PK;

			var workOrderLineComponent2 = helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, componentProduct2, 10m);
			workOrderLineComponent2.WE_WE_ParentDocketLine = workOrderLineMain.PK;

			AssertContainsExactElementsInAnyOrder(new[] { workOrderLineComponent1, workOrderLineComponent2 }, workOrderLineMain.ChildComponentLinesCollection);

			var whsDynamicWorkOrderData = GetNewDataObjectWriter(dynamicWorkOrder).GetDataObject(dynamicWorkOrder);
			AssertNotNull(whsDynamicWorkOrderData);
			AssertEquals("whsWorkOrderData.Order.OrderLineCollection.Count", 1, whsDynamicWorkOrderData.Order.OrderLineCollection.Count);

			var mainLine = whsDynamicWorkOrderData.Order.OrderLineCollection.Single();
			AssertEquals("mainLine.OrderLineCollection.Count", 2, mainLine.OrderLineCollection.Count);
			var childLine1 = mainLine.OrderLineCollection.Single(line => line.Product.Code.Equals(componentProduct1.OP_PartNum));
			AssertEquals(5m, childLine1.OrderedQty);

			var childLine2 = mainLine.OrderLineCollection.Single(line => line.Product.Code.Equals(componentProduct2.OP_PartNum));
			AssertEquals(10m, childLine2.OrderedQty);
		}

		public void TestChildComponentLines_ExcludesComponentsOfChildLines()
		{
			var factory2 = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory2);
			var data = new TestDataSimpleEnvironment(factory2);
			helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var area = helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;

			var mainProduct = helper.CreateProduct("BIKE", data.Org1);
			var componentProduct = helper.CreateProduct("WHEEL", data.Org1);
			factory2.Save();

			var dynamicWorkOrder = helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1, "ExtRef");
			dynamicWorkOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			dynamicWorkOrder.WD_IsInwardsProcessingJob = true;

			var workOrderLineMain = helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, mainProduct, 5m);
			workOrderLineMain.IsMainInwardProcessedItem = true;

			var workOrderLineComponent = helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, componentProduct, 5m);
			workOrderLineComponent.WE_WE_ParentDocketLine = workOrderLineMain.PK;
			AssertContainsExactElementsInAnyOrder(new[] { workOrderLineComponent }, workOrderLineMain.ChildComponentLinesCollection);

			var childOfChildLine = workOrderLineComponent.ChildComponentLinesCollection.AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { childOfChildLine }, workOrderLineComponent.ChildComponentLinesCollection);

			var whsDynamicWorkOrderData = GetNewDataObjectWriter(dynamicWorkOrder).GetDataObject(dynamicWorkOrder);
			AssertNotNull(whsDynamicWorkOrderData);
			AssertEquals("whsWorkOrderData.Order.OrderLineCollection.Count", 1, whsDynamicWorkOrderData.Order.OrderLineCollection.Count);

			var mainLine = whsDynamicWorkOrderData.Order.OrderLineCollection.Single();
			AssertEquals("mainLine.OrderLineCollection.Count", 1, mainLine.OrderLineCollection.Count);

			var childLine = mainLine.OrderLineCollection.Single();
			AssertNull("childLine.OrderLineCollection.Count", childLine.OrderLineCollection);
		}

		public void TestChildComponentLines_ExcludesForDisassembly()
		{
			var factory2 = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory2);
			var data = new TestDataSimpleEnvironment(factory2);
			helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var area = helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;

			var mainProduct = helper.CreateProduct("BIKE", data.Org1);
			var componentProduct = helper.CreateProduct("WHEEL", data.Org1);
			factory2.Save();

			var dynamicWorkOrder = helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1, "ExtRef");
			dynamicWorkOrder.WD_DocketSubType = DynamicWorkOrderType.Codes.Disassemble;
			dynamicWorkOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			dynamicWorkOrder.WD_IsInwardsProcessingJob = true;

			var workOrderLineMain = helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, mainProduct, 5m);
			workOrderLineMain.IsMainInwardProcessedItem = true;

			// Disassembly should never manage to have component lines in production, but adding for sake of test
			var workOrderLineComponent = helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, componentProduct, 5m);
			workOrderLineComponent.WE_WE_ParentDocketLine = workOrderLineMain.PK;
			AssertContainsExactElementsInAnyOrder(new[] { workOrderLineComponent }, workOrderLineMain.ChildComponentLinesCollection);

			var childOfChildLine = workOrderLineComponent.ChildComponentLinesCollection.AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { childOfChildLine }, workOrderLineComponent.ChildComponentLinesCollection);

			var whsDynamicWorkOrderData = GetNewDataObjectWriter(dynamicWorkOrder).GetDataObject(dynamicWorkOrder);
			AssertNotNull(whsDynamicWorkOrderData);
			AssertEquals("whsWorkOrderData.Order.OrderLineCollection.Count", 1, whsDynamicWorkOrderData.Order.OrderLineCollection.Count);

			var mainLine = whsDynamicWorkOrderData.Order.OrderLineCollection.Single();
			AssertNull("mainLine.OrderLineCollection", mainLine.OrderLineCollection);
		}

		public void TestChildComponentLines_CustomsData()
		{
			var factory2 = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory2);
			var data = new TestDataSimpleEnvironment(factory2);
			helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.WW_IsVirtualWarehouse = true;

			var area = helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			var row = helper.CreateRowAndGenerateLocations(data.Whs1, "I");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			location.WLV_WA_PickingArea = area.PK;

			var mainProduct = helper.CreateProduct("BIKE", data.Org1);
			var componentProduct1 = helper.CreateProduct("WHEEL", data.Org1);
			factory2.Save();

			var dynamicWorkOrder = helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1, "ExtRef");
			dynamicWorkOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			dynamicWorkOrder.WD_IsInwardsProcessingJob = true;

			var workOrderLineMain = helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, mainProduct, 5m);
			workOrderLineMain.IsMainInwardProcessedItem = true;

			var workOrderLineComponent = helper.CreateWhsDynamicWorkOrderLine(dynamicWorkOrder, componentProduct1, 5m);
			workOrderLineComponent.WE_WE_ParentDocketLine = workOrderLineMain.PK;
			WhsBondedWarehouseAttributeDataObjectWriterTest.GetCustomsData(workOrderLineComponent);

			AssertContainsExactElementsInAnyOrder(new[] { workOrderLineComponent }, workOrderLineMain.ChildComponentLinesCollection);

			var whsDynamicWorkOrderData = GetNewDataObjectWriter(dynamicWorkOrder).GetDataObject(dynamicWorkOrder);
			AssertNotNull(whsDynamicWorkOrderData);
			AssertEquals("whsWorkOrderData.Order.OrderLineCollection.Count", 1, whsDynamicWorkOrderData.Order.OrderLineCollection.Count);

			var mainLine = whsDynamicWorkOrderData.Order.OrderLineCollection.Single();
			AssertEquals("mainLine.OrderLineCollection.Count", 1, mainLine.OrderLineCollection.Count);
			WhsDynamicWorkOrderBondedWarehouseAttributeDataObjectWriterTest.AssertContents(mainLine.OrderLineCollection.Single().CustomsData, isComponentLine: true);
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

		#region TestPickPriority

		public void TestPickPriority()
		{
			var whsDocketBO = GetNewDocket();
			whsDocketBO.WD_PickPriority = 3;

			var notifications = new TestNotificationBuffer();
			var whsDocketData = GetNewDataObjectWriter(whsDocketBO, notifications).GetDataObject(whsDocketBO);

			AssertNotNull("whsDocketData", whsDocketData);
			AssertEquals((byte)3, whsDocketData.Order.PickPriority);
		}

		#endregion

		protected override WhsDynamicWorkOrderDataObjectWriter GetNewDataObjectWriter(BusinessObject topLevelBO, INotifications notifications)
			=> new WhsDynamicWorkOrderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.WDO, topLevelBO) { Notifications = notifications }));

		protected override WhsDynamicWorkOrder GetNewDocket()
			=> Factory.NewWithValidTestData<WhsDynamicWorkOrder>();

		protected override TestDataForUniversal GetNewTestData()
			=> new TestDataForUniversal(Factory, Logger, DataContextType.WarehouseDynamicWorkOrder);

		protected override bool IsCustomFieldsSupported => false;
	}
}
