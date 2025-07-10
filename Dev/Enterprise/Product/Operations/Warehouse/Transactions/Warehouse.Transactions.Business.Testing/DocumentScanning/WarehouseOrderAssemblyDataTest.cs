using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WarehouseOrderAssemblyDataTest : WhsTestCaseWithFactory
	{
		#region TestBusinessObjectType

		public void TestBusinessObjectType()
		{
			AssertEquals(typeof(WhsOrder), AssemblyData.BusinessObjectType);
		}

		#endregion

		#region TestGetBusinessObjectCollection

		public void TestGetBusinessObjectCollection()
		{
			AssertNotNull(AssemblyData.GetBusinessObjectCollection(Factory));
			AssertEquals(typeof(WhsOrderCollection), AssemblyData.GetBusinessObjectCollection(Factory).GetType());
		}

		#endregion

		#region TestModuleID

		public void TestModuleID()
		{
			AssertEquals(ModuleIDs.WhsOrder, AssemblyData.ModuleID);
		}

		#endregion

		#region TestReferenceType

		public void TestReferenceType()
		{
			AssertEquals(Constants.ReferenceTypes.SupplyChainLogistics, AssemblyData.ReferenceType);
		}

		#endregion

		#region TestIsAllowedForUnallocatedeDocs

		public void TestIsAllowedForUnallocatedeDocs()
		{
			AssertEquals(true, AssemblyData.IsAllowedForUnallocatedeDocs);
		}

		#endregion

		#region TestGetQuery

		public void TestGetQuery()
		{
			var org1 = Helper.CreateClient("ORG1");
			var org2 = Helper.CreateClient("ORG2");
			var org3 = Helper.CreateClient("ORG3");
			var whs = Helper.CreateWarehouse("WHS");
			var part = Helper.CreateProduct(org1, "P1");
			Helper.CreateProductClientRelationShip(org2, part);
			Helper.CreateProductClientRelationShip(org3, part);
			Factory.Save();
			var oldDate = ZDateTimeOffset.Today.AddMonths(-1);
			var emptyDateOffset = ZDateTimeOffset.Empty;
			var order1 =
				CreateWhsOrderWithLineAndFinaliseAssertion(org1, org2, whs, "OR1", part, emptyDateOffset,
					false); // ORG1 - Client
			var order2 =
				CreateWhsOrderWithLineAndFinaliseAssertion(org2, org1, whs, "OR2", part, emptyDateOffset,
					false); // ORG1 - Consignee
			var order3 =
				CreateWhsOrderWithLineAndFinaliseAssertion(org1, org1, whs, "OR3", part, emptyDateOffset,
					false); // ORG1 - Both Client and Consignee
			var order4 =
				CreateWhsOrderWithLineAndFinaliseAssertion(org3, org3, whs, "OR4", part, emptyDateOffset,
					false); // ORG3 - Both Client and Consignee
			var order5 =
				CreateWhsOrderWithLineAndFinaliseAssertion(org3, org3, whs, "OR5", part, oldDate.AddDays(1),
					false); // Orders Finalised
			var order6 =
				CreateWhsOrderWithLineAndFinaliseAssertion(org3, org3, whs, "OR6", part, oldDate.AddDays(2), false);
			var order7 =
				CreateWhsOrderWithLineAndFinaliseAssertion(org3, org3, whs, "OR7", part, oldDate.AddDays(3),
					true); // Orders and Picks finalised
			var order8 =
				CreateWhsOrderWithLineAndFinaliseAssertion(org3, org3, whs, "OR8", part, oldDate.AddDays(4), true);
			Factory.Save();

			var emptyDate = ZDateTime.Empty;
			var query1 = AssemblyData.GetQuery(new AssemblyDataParams(true, true, emptyDateOffset.ToLocalZDateTime(), emptyDate, emptyDate,
				emptyDate, emptyDate, emptyDate, org1.PK));
			AssertCorrectOrderListLoaded(query1, new WhsOrder[] { order1, order2, order3 });
			// Is Consignee - should include Order Jobs where the Organisation is Consignee.
			var query2 = AssemblyData.GetQuery(new AssemblyDataParams(true, false, emptyDate, emptyDate, emptyDate,
				emptyDate, emptyDate, emptyDate, org1.PK));
			AssertCorrectOrderListLoaded(query2, new WhsOrder[] { order2, order3 });
			// Is Client/Consignor - should include Order Jobs where the Organisation is Client.
			var query3 = AssemblyData.GetQuery(new AssemblyDataParams(false, true, emptyDate, emptyDate, emptyDate,
				emptyDate, emptyDate, emptyDate, org1.PK));
			AssertCorrectOrderListLoaded(query3, new WhsOrder[] { order1, order3 });
			// Finalised Dates
			var query4 = AssemblyData.GetQuery(new AssemblyDataParams(true, true, emptyDate, emptyDate, emptyDate,
				emptyDate, oldDate.ToLocalZDateTime().AddDays(2), emptyDate, org3.PK)); // Job Closed From
			AssertCorrectOrderListLoaded(query4, new WhsOrder[] { order6, order7, order8 });
			var query5 = AssemblyData.GetQuery(new AssemblyDataParams(true, true, emptyDate, emptyDate, emptyDate,
				emptyDate, emptyDate, oldDate.ToLocalZDateTime().AddDays(3), org3.PK)); // Job Closed To
			AssertCorrectOrderListLoaded(query5, new WhsOrder[] { order5, order6, order7 });
			var query6 = AssemblyData.GetQuery(new AssemblyDataParams(true, true, emptyDate, emptyDate, emptyDate,
				emptyDate, oldDate.ToLocalZDateTime().AddDays(2), oldDate.ToLocalZDateTime().AddDays(3), org3.PK)); // Job Closed Range
			AssertCorrectOrderListLoaded(query6, new WhsOrder[] { order6, order7 });
		}

		WhsOrder CreateWhsOrderWithLineAndFinaliseAssertion(OrgHeader client, OrgHeader consignee, WhsWarehouse whs,
			ZString externalReference, OrgSupplierPart part, ZDateTimeOffset finaliseDate, bool finalisePick)
		{
			WhsOrder order = Helper.CreateWhsOrder(client.PK, whs.PK, consignee.PK, externalReference);
			Helper.CreateWhsOrderLine(order, part, 5m);
			WhsPick pick = Helper.CreatePickNew(order);
			if (!finaliseDate.IsEmpty)
			{
				order.FinaliseDocket();
				AssertEquals(true, order.IsFinalised);
				if (finalisePick)
				{
					pick.FinalisePick();
					AssertEquals(true, pick.IsFinalised);
				}

				order.WD_FinalisedDate = finaliseDate;
			}

			return order;
		}

		void AssertCorrectOrderListLoaded(ZQuery query, WhsOrder[] expectedOrderList)
		{
			var orderList = new WhsOrderCollection(Factory);
			orderList.AdditionalFilter = query;
			AssertEquals("Incorrect List of Orders was loaded", expectedOrderList.Length, orderList.Count);
			foreach (var order in expectedOrderList)
			{
				AssertEquals("Expected Order has been not loaded: " + order.WD_ExternalReference, true,
					orderList.Contains(order));
			}
		}

		#endregion

		#region Implementation

		WarehouseOrderAssemblyData AssemblyData
		{
			get
			{
				return assemblyData ?? (assemblyData = new WarehouseOrderAssemblyData());
			}
		}

		WarehouseOrderAssemblyData assemblyData;

		#endregion
	}
}
