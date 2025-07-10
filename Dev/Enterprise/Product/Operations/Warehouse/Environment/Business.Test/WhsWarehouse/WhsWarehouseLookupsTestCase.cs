using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration.CodeLists;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	class WhsWarehouseLookupsTestCase : WhsBusinessObjectLookupsTestCase
	{
		#region TestOrganisations

		public void TestOrganisations()
		{
			AssertEquals(typeof(OrganisationsFindBoxCollection), Whs.Lookups.Organisations.GetType());
		}

		#endregion

		#region TestPrinters

		public void TestPrinters()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			AssertEquals("Printer Lookups should have the correct Type.", ObjectFactory.GetType<IStmPrintQueueCollection>(), warehouse.Lookups.Printers.GetType());
		}

		#endregion

		#region TestRelatedCompanyBranches

		public void TestRelatedCompanyBranches()
		{
			var branchInCurrentCompany = Factory.New<GlbBranch>();
			var branchInCurrentCompany_Inactive = Factory.New<GlbBranch>();
			var branchInOtherCompany = Factory.New<GlbBranch>();

			branchInCurrentCompany.GB_GC = GlbCompany.CurrentCompany.PK;
			branchInCurrentCompany_Inactive.GB_GC = GlbCompany.CurrentCompany.PK;
			branchInOtherCompany.GB_GC = Factory.New<GlbCompany>().PK;

			branchInCurrentCompany_Inactive.GB_IsActive = false;

			// ensure the list only shows active branches for the current company
			var branches = Whs.Lookups.RelatedCompanyBranches;
			branches.Load();
			AssertCollectionContains(branchInCurrentCompany, branches);
			AssertCollectionNotContains(branchInCurrentCompany_Inactive, branches);
			AssertCollectionNotContains(branchInOtherCompany, branches);
		}

		#endregion

		#region TestWarehouseTypes

		public void TestWarehouseTypes()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			AssertContainsExactElementsInAnyOrder(new WarehouseTypes(), warehouse.Lookups.WarehouseTypes);
		}

		#endregion

		#region TestPhoneTypes

		public void TestPhoneTypes()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			AssertContainsExactElementsInAnyOrder(new PhoneTypeList(), warehouse.Lookups.PhoneTypes);
		}

		#endregion

		#region TestDockDoorLocations

		public void TestDockDoorLocations()
		{
			var whs = Helper.CreateWarehouse("wh");
			Helper.CreateRowAndGenerateLocations(whs, "A", 10, 1);
			Factory.Save();
			var filterDefault = whs.Lookups.DockDoorLocations.FilterBusinessObjectDefaults["Dock Door Locations:Property0"];
			AssertNotNull(filterDefault);
			Assert(!filterDefault.IsRemovable);
			AssertEquals(ZBool.True, filterDefault.Value);
		}

		#endregion

		#region TestLocationTypes

		public void TestLocationTypes()
		{
			var locationType = Helper.CreateLocationType("TST");
			Factory.Save();
			AssertNotNull(Whs.Lookups.LocationTypes.Single(lt => lt.WLT_Code == "TST"));
		}

		#endregion

		#region TestUNDGContactList

		public void TestUNDGContactList()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			AssertEquals(typeof(OrgContactDependentCollection), warehouse.Lookups.UNDGContacts.GetType());

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			var contact2 = org2.Contacts.AddNew();

			warehouse.WW_OA_WarehouseAddress = org1.MainAddress.PK;
			warehouse.Lookups.UNDGContacts.Load();
			AssertContainsExactElementsInAnyOrder(new[] { contact1 }, warehouse.Lookups.UNDGContacts);

			warehouse.WW_OA_WarehouseAddress = org2.MainAddress.PK;
			warehouse.Lookups.UNDGContacts.Load();
			AssertContainsExactElementsInAnyOrder(new[] { contact2 }, warehouse.Lookups.UNDGContacts);
		}

		#endregion

		#region TestDetailedTrackingMethods

		public void TestDetailedTrackingMethos()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			AssertContainsExactElementsInAnyOrder(new DetailedTrackingMethod(), warehouse.Lookups.DetailedTrackingMethods);
		}

		#endregion

		#region Implmentation

		protected WhsWarehouse Whs
		{
			get { return whs ?? (whs = Factory.New<WhsWarehouse>()); }
		}

		WhsWarehouse whs;

		#endregion
	}
}
