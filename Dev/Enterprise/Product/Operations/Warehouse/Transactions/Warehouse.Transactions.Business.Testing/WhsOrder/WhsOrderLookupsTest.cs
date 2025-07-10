using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsOrderLookupsTest : WhsDocketLookupsTest<WhsOrder>
	{
		#region TestINCOTerms

		public override void TestINCOTerms()
		{
			base.TestINCOTerms();

			var order = Docket;
			CodeDescriptionPairList expectedInternationalList = new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms);

			OrgHeader org = Helper.CreateClient("XAG");
			order.ConsigneePK = org.PK;
			org.MainAddress.OA_RL_NKRelatedPortCode = "USCHI";
			WhsWarehouse warehouse = Helper.CreateWarehouse("X1X");
			order.WD_WW_Whs = warehouse.PK;

			AssertEquals(false, order.IsDomesticFreight);
			foreach (CodeDescriptionPair pair in Docket.Lookups.INCOTerms)
			{
				AssertEquals("Invalid CodeDesriptionPair found", true, expectedInternationalList.ContainsCode(pair));
			}

			foreach (CodeDescriptionPair pair in expectedInternationalList)
			{
				AssertEquals("Missing CodeDescriptionPair", true, Docket.Lookups.INCOTerms.ContainsCode(pair));
			}
		}

		#endregion

		#region TestSubTypes

		protected override void TestSubTypesCore()
		{
			AssertEquals(true, Lookups.SubTypes.ContainsCode(CodeLists.OrderType.Codes.BackOrder));
		}

		#endregion

		#region TestForwarders

		public void TestForwarders()
		{
			AssertEquals(typeof(OrgHeaderCollection), Lookups.Forwarders.GetType());
		}

		#endregion

		#region TestCrossDockLocations

		public void TestCrossDockLocations()
		{
			var ddlLocationType = Helper.CreateLocationType("XYZ", LocationClasses.Codes.DDL);
			var whs1 = Helper.CreateWarehouse("WH1", "A", 2, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "B", 2, 1);

			var dockDoorFilterKey = WhsLocationCollection.FilterSchema.DockDoorLocation + ":Property0";
			var warehouseFilterKey = WhsLocationCollection.FilterSchema.Warehouse + ":Property";

			AssertEquals("Dock Door location filter should be used.", true, Lookups.CrossDockLocations.FilterBusinessObjectDefaults[dockDoorFilterKey].Value);
			AssertEquals("Warehouse filter should not be used.", false, Lookups.CrossDockLocations.FilterBusinessObjectDefaults.ContainsDefaultFor(warehouseFilterKey));

			Docket.WD_WW_Whs = whs1.PK;
			AssertEquals("Dock Door location filter should be used.", true, Lookups.CrossDockLocations.FilterBusinessObjectDefaults[dockDoorFilterKey].Value);
			AssertEquals("Warehouse filter should be set for Whs1 and used.", whs1.PK, Lookups.CrossDockLocations.FilterBusinessObjectDefaults[warehouseFilterKey].Value);

			Docket.WD_WW_Whs = whs2.PK;
			AssertEquals("Dock Door location filter should be used.", true, Lookups.CrossDockLocations.FilterBusinessObjectDefaults[dockDoorFilterKey].Value);
			AssertEquals("Warehouse filter should be set for Whs2 and used.", whs2.PK, Lookups.CrossDockLocations.FilterBusinessObjectDefaults[warehouseFilterKey].Value);
		}

		#endregion

		#region TestSalesChannels

		public void TestSalesChannels()
		{
			AssertEquals("Correct collection type", typeof(WhsSalesChannelCollection), Lookups.SalesChannels.GetType());
		}

		#endregion

		#region Implementation

		protected new WhsOrderLookups Lookups
		{
			get { return (WhsOrderLookups)base.Lookups; }
		}

		#endregion
	}
}
