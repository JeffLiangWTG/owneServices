using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WarehouseWorkOrderAssemblyDataTest : WhsTestCaseWithFactory
	{
		#region TestBusinessObjectType

		public void TestBusinessObjectType()
		{
			var assemblyData = new WarehouseWorkOrderAssemblyData();
			AssertEquals(typeof(WhsWorkOrder), assemblyData.BusinessObjectType);
		}

		#endregion

		#region TestGetBusinessObjectCollection

		public void TestGetBusinessObjectCollection()
		{
			var assemblyData = new WarehouseWorkOrderAssemblyData();
			AssertNotNull(assemblyData.GetBusinessObjectCollection(Factory));
			AssertEquals(typeof(WhsWorkOrderCollection), assemblyData.GetBusinessObjectCollection(Factory).GetType());
		}

		#endregion

		#region TestModuleID

		public void TestModuleID()
		{
			var assemblyData = new WarehouseWorkOrderAssemblyData();
			AssertEquals(ModuleIDs.WhsWorkOrder, assemblyData.ModuleID);
		}

		#endregion

		#region TestReferenceType

		public void TestReferenceType()
		{
			var assemblyData = new WarehouseWorkOrderAssemblyData();
			AssertEquals(Constants.ReferenceTypes.SupplyChainLogistics, assemblyData.ReferenceType);
		}

		#endregion

		#region TestIsAllowedForUnallocatedeDocs

		public void TestIsAllowedForUnallocatedeDocs()
		{
			var assemblyData = new WarehouseWorkOrderAssemblyData();
			AssertEquals(true, assemblyData.IsAllowedForUnallocatedeDocs);
		}

		#endregion

		#region TestGetQuery

		public void TestGetQuery_Client()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var mainProduct = data.Part1;
			var componentProduct = data.Part2;
			Helper.CreateProductBOM(mainProduct, componentProduct);
			var org2 = Helper.CreateClient("Org2");
			Helper.CreateProductClientRelationShip(org2, mainProduct);
			Helper.CreateProductClientRelationShip(org2, componentProduct);
			var workOrder1 = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "WO1", mainProduct, 1m);
			var workOrder2 = Helper.CreateWhsWorkOrderWithLine(org2, data.Whs1, "WO2", mainProduct, 1m);
			Factory.Save();
			var assemblyData = new WarehouseWorkOrderAssemblyData();
			var query1 = assemblyData.GetQuery(new AssemblyDataParams()
			{
				IncludeConsignor = true,
				Organisation = data.Org1.PK
			});
			AssertCorrectOrderListLoaded(query1, new WhsWorkOrder[] { workOrder1 });
			var query2 = assemblyData.GetQuery(new AssemblyDataParams()
			{
				IncludeConsignor = true,
				Organisation = org2.PK
			});
			AssertCorrectOrderListLoaded(query2, new WhsWorkOrder[] { workOrder2 });
		}

		public void TestGetQuery_Consignee()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var mainProduct = data.Part1;
			var componentProduct = data.Part2;
			Helper.CreateProductBOM(mainProduct, componentProduct);
			var org2 = Helper.CreateClient("ORG2");
			var org3 = Helper.CreateClient("ORG3");
			Helper.CreateProductClientRelationShip(org2, mainProduct);
			Helper.CreateProductClientRelationShip(org2, componentProduct);
			Helper.CreateProductClientRelationShip(org3, mainProduct);
			Helper.CreateProductClientRelationShip(org3, componentProduct);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", componentProduct, 10m);
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R2", componentProduct, 10m);
			Helper.CreateWhsReceiveWithInventory(org3, data.Whs1, "R3", componentProduct, 10m);
			Factory.Save();
			var workOrder1 =
				CreateWhsWorkOrderWithLineAndFinaliseAssertion(data.Org1, org2, data.Whs1, "OR1",
					mainProduct); // Client: Org1 - Consignee: Org2
			var workOrder2 =
				CreateWhsWorkOrderWithLineAndFinaliseAssertion(org2, data.Org1, data.Whs1, "OR2",
					mainProduct); // Client: Org2 - Consignee: Org1
			var workOrder3 =
				CreateWhsWorkOrderWithLineAndFinaliseAssertion(data.Org1, data.Org1, data.Whs1, "OR3",
					mainProduct); // Client: Org1 - Consignee: Org1
			var workOrder4 =
				CreateWhsWorkOrderWithLineAndFinaliseAssertion(org3, org3, data.Whs1, "OR4",
					mainProduct); // Client: Org3 - Consignee: Org3
			Factory.Save();
			var assemblyData = new WarehouseWorkOrderAssemblyData();
			var queryConsigneeOnlyOrg1 = assemblyData.GetQuery(new AssemblyDataParams()
			{
				IncludeConsignee = true,
				Organisation = data.Org1.PK
			});
			AssertCorrectOrderListLoaded(queryConsigneeOnlyOrg1, new WhsWorkOrder[] { workOrder2, workOrder3 });
			var queryConsigneeOnlyOrg2 = assemblyData.GetQuery(new AssemblyDataParams()
			{
				IncludeConsignee = true,
				Organisation = org2.PK
			});
			AssertCorrectOrderListLoaded(queryConsigneeOnlyOrg2, new WhsWorkOrder[] { workOrder1 });
			var queryBothClientAndConsigneeOrg1 = assemblyData.GetQuery(new AssemblyDataParams()
			{
				IncludeConsignee = true,
				IncludeConsignor = true,
				Organisation = data.Org1.PK
			});
			AssertCorrectOrderListLoaded(queryBothClientAndConsigneeOrg1,
				new WhsWorkOrder[] { workOrder1, workOrder2, workOrder3 });
			var queryBothClientAndConsigneeOrg2 = assemblyData.GetQuery(new AssemblyDataParams()
			{
				IncludeConsignee = true,
				IncludeConsignor = true,
				Organisation = org2.PK
			});
			AssertCorrectOrderListLoaded(queryBothClientAndConsigneeOrg2,
				new WhsWorkOrder[] { workOrder1, workOrder2 });
		}

		public void TestGetQuery_FinalisedDates()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var mainProduct = data.Part1;
			var componentProduct = data.Part2;
			Helper.CreateProductBOM(mainProduct, componentProduct);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", componentProduct, 10m);
			Factory.Save();
			var oldDateOffset = ZDateTimeOffset.Today.AddMonths(-1);
			var oldDate = oldDateOffset.Date;
			var workOrder1 = CreateWhsWorkOrderWithLineAndFinaliseAssertion(data.Org1, data.Org1, data.Whs1, "OR1",
				mainProduct, oldDateOffset.AddDays(1));
			var workOrder2 = CreateWhsWorkOrderWithLineAndFinaliseAssertion(data.Org1, data.Org1, data.Whs1, "OR2",
				mainProduct, oldDateOffset.AddDays(2));
			var workOrder3 = CreateWhsWorkOrderWithLineAndFinaliseAssertion(data.Org1, data.Org1, data.Whs1, "OR3",
				mainProduct, oldDateOffset.AddDays(3));
			var workOrder4 = CreateWhsWorkOrderWithLineAndFinaliseAssertion(data.Org1, data.Org1, data.Whs1, "OR4",
				mainProduct, oldDateOffset.AddDays(4));
			Factory.Save();
			// Finalised Dates
			var assemblyData = new WarehouseWorkOrderAssemblyData();
			var queryJobClosedFrom = assemblyData.GetQuery(new AssemblyDataParams()
			{
				JobClosedFrom = oldDate.AddDays(2)
			});
			AssertCorrectOrderListLoaded(queryJobClosedFrom, new WhsWorkOrder[] { workOrder2, workOrder3, workOrder4 });
			var queryJobClosedTo = assemblyData.GetQuery(new AssemblyDataParams() { JobClosedTo = oldDate.AddDays(3) });
			AssertCorrectOrderListLoaded(queryJobClosedTo, new WhsWorkOrder[] { workOrder1, workOrder2, workOrder3 });
			var queryDateRange = assemblyData.GetQuery(new AssemblyDataParams()
			{
				JobClosedFrom = oldDate.AddDays(2),
				JobClosedTo = oldDate.AddDays(3)
			});
			AssertCorrectOrderListLoaded(queryDateRange, new WhsWorkOrder[] { workOrder2, workOrder3 });
		}

		WhsWorkOrder CreateWhsWorkOrderWithLineAndFinaliseAssertion(OrgHeader client, OrgHeader consignee,
			WhsWarehouse whs, ZString externalReference, OrgSupplierPart part, ZDateTimeOffset finaliseDate = new ZDateTimeOffset())
		{
			var order = Helper.CreateWhsOrderWithOrderLine(client, whs, externalReference, part, 1m);
			order.ConsigneePK = consignee.PK;
			order.BOM.AutoCreateWorkOrders(Notify);
			var workOrder = order.CurrentWorkOrders.Single();
			AssertEquals(workOrder.ConsigneeAddressPK, consignee.MainAddress.PK);
			if (!finaliseDate.IsEmpty)
			{
				var pick = Helper.CreatePickNew(workOrder);
				workOrder.FinaliseDocketAlwaysFinalisingPick();
				AssertIsFinalisedPrecondition(workOrder);
				AssertIsFinalisedPrecondition(pick);
				workOrder.WD_FinalisedDate = finaliseDate;
			}

			return workOrder;
		}

		void AssertCorrectOrderListLoaded(ZQuery query, WhsWorkOrder[] expectedOrderList)
		{
			var workOrderList = Factory.Load<WhsWorkOrder>(query);
			AssertContainsExactElementsInAnyOrder("Incorrect List of Work Orders was loaded", expectedOrderList,
				workOrderList);
		}

		#endregion
	}
}
