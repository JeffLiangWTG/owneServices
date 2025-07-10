using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsWorkOrderDocAddressQueryHelperTest : WhsTestCaseWithFactory
	{
		public void TestGetWorkOrderAddressQuery()
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

			var workOrderConsigneeSubQuery = WhsWorkOrderDocAddressQueryHelper.GetWorkOrderAddressQuery(DocAddressTypes.Codes.ConsigneeAddress, consignee1.PK);
			var parentQuery = new ZDBOnlyQuery(typeof(WhsWorkOrder));
			parentQuery.AddSubQuery(workOrderConsigneeSubQuery, JoinCondition.And);
			var loadedWorkOrders = Factory.Load<WhsWorkOrder>(parentQuery);
			AssertContainsExactElementsInAnyOrder(new[] { workOrder1.PK }, loadedWorkOrders.Select(wo => wo.PK));

			var workOrderConsigneeSubQuery2 = WhsWorkOrderDocAddressQueryHelper.GetWorkOrderAddressQuery(DocAddressTypes.Codes.ConsigneeAddress, consignee2.PK);
			var parentQuery2 = new ZDBOnlyQuery(typeof(WhsWorkOrder));
			parentQuery2.AddSubQuery(workOrderConsigneeSubQuery2, JoinCondition.And);
			var loadedWorkOrders2 = Factory.Load<WhsWorkOrder>(parentQuery2);
			AssertContainsExactElementsInAnyOrder(new[] { workOrder2.PK }, loadedWorkOrders2.Select(wo => wo.PK));
		}

		public void TestGetWorkOrderConsigneeCompanyNameQuery()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var mainProduct = data.Part1;
			var componentProduct = data.Part2;
			Helper.CreateProductBOM(mainProduct, componentProduct);

			var consignee1 = Helper.CreateClient("CNE1");
			consignee1.OH_FullName = "123";
			var consignee2 = Helper.CreateClient("CNE2");
			consignee2.OH_FullName = "xyz";

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", componentProduct, 10m);
			Factory.Save();

			var workOrder1 = CreateWhsWorkOrderWithConsignee(data.Org1, consignee1, data.Whs1, "OR1", mainProduct);
			var workOrder2 = CreateWhsWorkOrderWithConsignee(data.Org1, consignee2, data.Whs1, "OR2", mainProduct);
			var workOrder3 = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "WOR03");

			// company name override
			var consignee3 = Helper.CreateClient("CNE3");
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "OR3", mainProduct, 1m);
			order.ConsigneePK = consignee3.PK;
			order.ConsigneeDocAddress.E2_AddressOverride = true;
			order.ConsigneeDocAddress.E2_CompanyName = "123 Override";

			order.BOM.AutoCreateWorkOrders(new NotificationBuffer());
			var workOrder4 = order.CurrentWorkOrders.Single();

			Factory.Save();

			var workOrderConsigneeSubQuery = WhsWorkOrderDocAddressQueryHelper.GetWorkOrderConsigneeCompanyNameQuery(SQLComparisonOperator.StartsWith, "12");
			var parentQuery = new ZDBOnlyQuery(typeof(WhsWorkOrder));
			parentQuery.AddSubQuery(workOrderConsigneeSubQuery, JoinCondition.And);
			var loadedWorkOrders = Factory.Load<WhsWorkOrder>(parentQuery);

			var expectedWorkOrderPKs = new ZGuid[] { workOrder1.PK, workOrder4.PK };
			AssertContainsExactElementsInAnyOrder(expectedWorkOrderPKs, loadedWorkOrders.Select(wo => wo.PK));

			workOrderConsigneeSubQuery = WhsWorkOrderDocAddressQueryHelper.GetWorkOrderConsigneeCompanyNameQuery(SQLComparisonOperator.StartsWith, "xy");
			parentQuery = new ZDBOnlyQuery(typeof(WhsWorkOrder));
			parentQuery.AddSubQuery(workOrderConsigneeSubQuery, JoinCondition.And);
			loadedWorkOrders = Factory.Load<WhsWorkOrder>(parentQuery);

			expectedWorkOrderPKs = new ZGuid[] { workOrder2.PK };
			AssertContainsExactElementsInAnyOrder(expectedWorkOrderPKs, loadedWorkOrders.Select(wo => wo.PK));
		}

		public void TestGetWorkOrderTransportCoCompanyNameQuery()
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

			// company name override
			var transportCo3 = Helper.CreateClient("TRC3");

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "OR3", mainProduct, 1m);
			order.TransportCoPK = transportCo3.PK;
			order.TransportCoDocAddress.E2_AddressOverride = true;
			order.TransportCoDocAddress.E2_CompanyName = "123 Override";

			order.BOM.AutoCreateWorkOrders(new NotificationBuffer());
			var workOrder4 = order.CurrentWorkOrders.Single();

			Factory.Save();

			var workOrderTransportCoSubQuery = WhsWorkOrderDocAddressQueryHelper.GetWorkOrderTransportCoCompanyNameQuery(SQLComparisonOperator.StartsWith, "12");
			var parentQuery = new ZDBOnlyQuery(typeof(WhsWorkOrder));
			parentQuery.AddSubQuery(workOrderTransportCoSubQuery, JoinCondition.And);
			var loadedWorkOrders = Factory.Load<WhsWorkOrder>(parentQuery);

			var expectedWorkOrderPKs = new ZGuid[] { workOrder1.PK, workOrder4.PK };
			AssertContainsExactElementsInAnyOrder(expectedWorkOrderPKs, loadedWorkOrders.Select(wo => wo.PK));

			workOrderTransportCoSubQuery = WhsWorkOrderDocAddressQueryHelper.GetWorkOrderTransportCoCompanyNameQuery(SQLComparisonOperator.StartsWith, "xy");
			parentQuery = new ZDBOnlyQuery(typeof(WhsWorkOrder));
			parentQuery.AddSubQuery(workOrderTransportCoSubQuery, JoinCondition.And);
			loadedWorkOrders = Factory.Load<WhsWorkOrder>(parentQuery);

			expectedWorkOrderPKs = new ZGuid[] { workOrder2.PK };
			AssertContainsExactElementsInAnyOrder(expectedWorkOrderPKs, loadedWorkOrders.Select(wo => wo.PK));
		}

		#region Implementation

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

		#endregion
	}
}
