using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(WorkOrderFilterBusinessObject))]
	public class WorkOrderFilterBusinessObjectTest : PickableDocketFilterBusinessObjectTest<WorkOrderFilterBusinessObject>
	{
		#region Overrides

		protected override bool SupportsContainerNoFilter => false;
		protected override bool SupportsCustomerReferenceFilter => false;
		protected override bool SupportsTransportReferenceFilter => false;
		protected override bool SupportsAccountingFilters => false;

		public override void TestFilterDocketReferenceMatchesExternalReferenceEvenWithNoDocketReferences()
		{
			Assert("incomplete test", true);
		}

		protected override bool SupportsFilterForEnteredStatus => false;

		public override void TestFilterEntryKey()
		{
			Assert("Non-empty bonded entry keys are not supported on work orders (Constraint_CustomsSpecificFields)", true);
		}

		protected override ZString ExternalReferenceFieldName => "Work Order No.";

		protected override CodeDescriptionPairList GetExpectedDocketStatus()
		{
			var status = base.GetExpectedDocketStatus();
			status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.Putaway));
			status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.Held));
			status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.Cancelled));
			return status;
		}

		#region TestFilterTransportCo

		protected override WhsPickableDocket GetDocketForTransportCoFilterTesting(TestDataSimpleEnvironment data, string docketReference, OrgSupplierPart product, OrgHeader transportCo = null)
		{
			var workOrder = transportCo != null
				? CreateWhsWorkOrderWithTransportCo(data.Org1, transportCo, data.Whs1, docketReference, data.Part1)
				: Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "WOR03");

			return workOrder;
		}

		protected override OrgSupplierPart SetupAndGetProductForTransportCoFilterTesting(TestDataSimpleEnvironment data)
		{
			Helper.CreateProductBOM(data.Part1, data.Part2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "INV1", data.Part2, 100m);
			Factory.Save();

			return data.Part1;
		}

		protected override bool SupportsTransportCoFiltersWithComparisonOperator => false;

		#endregion

		#region TestFilterByTransportCompanyName

		public void TestFilterByTransportCompanyName()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var mainProduct = data.Part1;
			var componentProduct = data.Part2;
			Helper.CreateProductBOM(mainProduct, componentProduct);

			var transportCo1 = Helper.CreateClient("TRC1");
			transportCo1.OH_FullName = "123";
			var transportCo2 = Helper.CreateClient("TRC2");
			transportCo2.OH_FullName = "xyz";

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", componentProduct, 10m);
			Factory.Save();

			var workOrder1 = CreateWhsWorkOrderWithTransportCo(data.Org1, transportCo1, data.Whs1, "OR1", mainProduct);
			var workOrder2 = CreateWhsWorkOrderWithTransportCo(data.Org1, transportCo2, data.Whs1, "OR2", mainProduct);
			var workOrder3 = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "WOR03");
			Helper.CreateWhsWorkOrderLine(workOrder3, mainProduct, 10m);
			Factory.Save();

			Helper.CreatePickNew(workOrder1);
			Helper.CreatePickNew(workOrder2);
			Helper.CreatePickNew(workOrder3);

			Factory.Save();

			GetDocketCollection();

			var transportNameFilter = (ModuleTextFilter)FilterStripBizO["Transport Company Name"];
			transportNameFilter.Property = "12";
			transportNameFilter.IsActive = true;
			transportNameFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			var workOrders = Factory.Load<WhsDocket>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { workOrder1 }, workOrders);

			transportNameFilter.Property = "xy";
			workOrders = Factory.Load<WhsDocket>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { workOrder2 }, workOrders);
		}

		#endregion

		#region TestFilterByConsignee

		public override void TestFilterByConsignee()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var mainProduct = data.Part1;
			var componentProduct = data.Part2;
			Helper.CreateProductBOM(mainProduct, componentProduct);

			var consignee1 = Helper.CreateClient("CNE1");
			var consignee2 = Helper.CreateClient("CNE2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", componentProduct, 10m);
			Factory.Save();

			var workOrder1 = CreateWhsWorkOrderWithConsignee(data.Org1, consignee1, data.Whs1, "OR1", mainProduct);
			var workOrder2 = CreateWhsWorkOrderWithConsignee(data.Org1, consignee2, data.Whs1, "OR2", mainProduct);
			var workOrder3 = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "WOR03");

			Factory.Save();

			Asserter.AddToScope(workOrder1, workOrder2, workOrder3);

			var filter = (ModuleGuidFilter)FilterStripBizO["Consignee"];

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = ZGuid.Empty;
			Asserter.AssertMatches("No Consignee specified should return all work orders.", filter, workOrder1, workOrder2, workOrder3);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = consignee1.PK;
			Asserter.AssertMatches("Filter by Consignee1, should return workOrder1.", filter, workOrder1);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = consignee2.PK;
			Asserter.AssertMatches("Filter by Consignee2, should return workOrder2.", filter, workOrder2);
		}

		WhsWorkOrder CreateWhsWorkOrderWithConsignee(OrgHeader client, OrgHeader consignee, WhsWarehouse whs, ZString externalReference, OrgSupplierPart part)
		{
			var order = Helper.CreateWhsOrderWithOrderLine(client, whs, externalReference, part, 1m);
			order.ConsigneePK = consignee.PK;
			order.BOM.AutoCreateWorkOrders(new NotificationBuffer());

			var workOrder = order.CurrentWorkOrders.Single();
			AssertEquals(workOrder.ConsigneeAddressPK, consignee.MainAddress.PK);

			return workOrder;
		}

		WhsWorkOrder CreateWhsWorkOrderWithTransportCo(OrgHeader client, OrgHeader transportCo, WhsWarehouse whs, ZString externalReference, OrgSupplierPart part)
		{
			var order = Helper.CreateWhsOrderWithOrderLine(client, whs, externalReference, part, 1m);
			order.TransportCoPK = transportCo.PK;
			order.BOM.AutoCreateWorkOrders(new NotificationBuffer());

			var workOrder = order.CurrentWorkOrders.Single();
			AssertEquals(workOrder.TransportCoDocAddress.E2_OA_Address, transportCo.MainAddress.PK);

			return workOrder;
		}

		public void TestFilterByConsignee_FilterConsigneeDocAddressType()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var mainProduct = data.Part1;
			var componentProduct = data.Part2;
			Helper.CreateProductBOM(mainProduct, componentProduct);

			var org1 = Helper.CreateClient("Org1");
			var org2 = Helper.CreateClient("Org2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", componentProduct, 10m);
			Factory.Save();

			var workOrder1 = CreateWhsWorkOrderWithConsignee(data.Org1, org1, data.Whs1, "OR1", mainProduct);
			var workOrder2 = CreateWhsWorkOrderWithConsignee(data.Org1, org2, data.Whs1, "OR2", mainProduct);
			var workOrder3 = CreateWhsWorkOrderWithTransportCo(data.Org1, org2, data.Whs1, "OR3", mainProduct);

			Factory.Save();

			Asserter.AddToScope(workOrder1, workOrder2, workOrder3);

			var filter1 = (ModuleGuidFilter)FilterStripBizO["Consignee"];
			filter1.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter1.Property = ZGuid.Empty;
			Asserter.AssertMatches("No Consignee specified should return all work orders.", filter1, new[] { workOrder1, workOrder2, workOrder3 });

			filter1.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter1.Property = org1.PK;
			Asserter.AssertMatches("Filter by org1, should return workOrder1.", filter1, new[] { workOrder1 });

			filter1.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter1.Property = org2.PK;
			Asserter.AssertMatches("Filter by org2, should return workOrder2.", filter1, new[] { workOrder2 });

			var filter2 = (ModuleGuidFilter)FilterStripBizO["Transport Co"];
			filter2.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter2.Property = org2.PK;
			Asserter.AssertMatches("Transport Co Filter by org2, should return workOrder3.", filter2, new[] { workOrder3 });
		}

		#endregion

		#endregion

		#region Status Filter - UnfinalisedFilterType

		public void TestStatusesFilter_UNF_DoNotIncludeCancelledDocket()
		{
			SetupTestData();

			Docket11.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			if (GetStatusFilterCaption() == "Order Status")
			{
				var pick = Helper.CreatePickNew();
				pick.WP_WW_Whs = Whs2.PK;
				Docket12.WD_WP = pick.PK;
			}
			Docket12.WD_DocketStatus = DocketStatus.Codes.Finalised;
			Docket12.WD_FinalisedDate = ZDateTimeOffset.Today;

			Factory.Save();

			var filter = new WorkOrderFilterBusinessObject();
			var statusFilter = (ModuleTextFilter)filter[GetStatusFilterCaption()];
			DocketCollection.AdditionalFilter = statusFilter.Query;
			AssertContainsExactElementsInAnyOrder("Pre-condition: no status filter property, should get every docket include finalised and cancelled docket.", new[] { Docket11, Docket12, Docket21, Docket22 }, DocketCollection);

			statusFilter.Property = (ZString)StatusFilterTypes.UnfinalisedFilterType;
			DocketCollection.AdditionalFilter = statusFilter.Query;
			AssertContainsExactElementsInAnyOrder("status filter property is Un-Finalised, should not include finalised and cancelled docket", new[] { Docket21, Docket22 }, DocketCollection);
		}

		#endregion

		#region TestParentOrderNoFilter

		public void TestParentOrderNoFilter()
		{
			var workOrder1 = Factory.NewWithValidTestData<WhsWorkOrder>();
			var workOrder2 = Factory.NewWithValidTestData<WhsWorkOrder>();
			var workOrder3 = Factory.NewWithValidTestData<WhsWorkOrder>();

			var order = Factory.NewWithValidTestData<WhsOrder>();
			order.WD_ExternalReference = "123";

			workOrder1.WD_WD_ParentDocket = order.PK;
			workOrder2.WD_WD_ParentDocket = order.PK;
			Factory.Save();

			Asserter.AddToScope(workOrder1, workOrder2, workOrder3);

			var filter = (ModuleTextFilter)FilterStripBizO["Parent Order No."];

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "777";
			Asserter.AssertMatches("There should be nothing returned", filter);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "123";
			Asserter.AssertMatches("Only the two work orders with the parent docket assigned should be returned", filter, workOrder1, workOrder2);
		}

		public void TestParentOrderFilter_MaxLength()
		{
			AssertEquals("MaxLength of Parent Order No. should be set correctly.", WhsDocketSchema.WD_ExternalReference.MaxLength, DocketFilter["Parent Order No."].MaxLength);
		}

		#endregion

		#region Implementation

		protected override CodeDescriptionPairList ValidOrderTypes => new WorkOrderType();

		protected override WhsDocket CreateDocket(OrgHeader org, WhsWarehouse whs, string @ref)
		{
			return Helper.CreateWhsWorkOrder(org, whs, @ref);
		}

		protected override WhsDocketLine CreateDocketLine(WhsDocket docket, OrgSupplierPart part, ZDecimal units)
		{
			return Helper.CreateWhsWorkOrderLine((WhsWorkOrder)docket, part, units);
		}

		protected override ActiveBusinessObjectCollection<WhsDocket> GetDocketCollection()
		{
			return new WhsWorkOrderCollection(Factory);
		}

		#endregion
	}
}
