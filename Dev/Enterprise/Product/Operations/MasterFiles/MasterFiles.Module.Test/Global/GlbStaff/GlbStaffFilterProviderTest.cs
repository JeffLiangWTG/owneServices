using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GlbStaffFilterProvider))]
	internal class GlbStaffFilterProviderTest : FilterStripBusinessObjectTestCase
	{
		#region Filters

		#region TestSalesRepFilter

		public void TestSalesRepFilter()
		{
			var salesRepStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			var salesRepStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			var otherStaff = Factory.NewWithValidTestData<GlbStaff>();

			salesRepStaff1.GS_IsSalesRep = true;
			salesRepStaff2.GS_IsSalesRep = true;
			otherStaff.GS_IsSalesRep = false;

			Factory.Save();

			var staffCollection = new GlbStaffCollection(Factory, StaffFilterProvider.GetSalesRepFilter(OrgConstants.FilterControl.SalesRepStatus.Code.SalesRep));

			Assert("Filtered for Sales Rep staff, Collection should contain SalesRepStaff1", staffCollection.Contains(salesRepStaff1));
			Assert("Filtered for Sales Rep staff, Collection should contain SalesRepStaff2", staffCollection.Contains(salesRepStaff2));
			Assert("Filtered for Sales Rep staff, Collection should NOT contain OtherStaff", !staffCollection.Contains(otherStaff));

			staffCollection = new GlbStaffCollection(Factory, StaffFilterProvider.GetSalesRepFilter(OrgConstants.FilterControl.SalesRepStatus.Code.NonSalesRep));

			Assert("Filtered for NOT Sales Rep staff, Collection should NOT contain SalesRepStaff1", !staffCollection.Contains(salesRepStaff1));
			Assert("Filtered for NOT Sales Rep staff, Collection should NOT contain SalesRepStaff2", !staffCollection.Contains(salesRepStaff2));
			Assert("Filtered for NOT Sales Rep staff, Collection should contain OtherStaff", staffCollection.Contains(otherStaff));

			staffCollection = new GlbStaffCollection(Factory, StaffFilterProvider.GetSalesRepFilter(OrgConstants.FilterControl.SalesRepStatus.Code.AllStaff));

			Assert("Filtered for ALL staff, Collection should contain SalesRepStaff1", staffCollection.Contains(salesRepStaff1));
			Assert("Filtered for ALL staff, Collection should contain SalesRepStaff2", staffCollection.Contains(salesRepStaff2));
			Assert("Filtered for ALL staff, Collection should contain OtherStaff", staffCollection.Contains(otherStaff));

			staffCollection = new GlbStaffCollection(Factory, StaffFilterProvider.GetSalesRepFilter(""));

			Assert("Empty filter, Collection should contain SalesRepStaff1", staffCollection.Contains(salesRepStaff1));
			Assert("Empty filter, Collection should contain SalesRepStaff2", staffCollection.Contains(salesRepStaff2));
			Assert("Empty filter, Collection should contain OtherStaff", staffCollection.Contains(otherStaff));
		}

		#endregion

		#region TestDriverFilter

		public void TestDriverFilter()
		{
			var driverStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			var driverStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			var otherStaff = Factory.NewWithValidTestData<GlbStaff>();

			driverStaff1.GS_IsDriver = true;
			driverStaff2.GS_IsDriver = true;
			otherStaff.GS_IsDriver = false;

			Factory.Save();

			var staffCollection = new GlbStaffCollection(Factory, StaffFilterProvider.GetDriverFilter(OrgConstants.FilterControl.DriverStatus.Code.Driver));

			Assert("Filtered for Driver staff, Collection should contain driverStaff1", staffCollection.Contains(driverStaff1));
			Assert("Filtered for Driver staff, Collection should contain driverStaff2", staffCollection.Contains(driverStaff2));
			Assert("Filtered for Driver staff, Collection should NOT contain otherStaff", !staffCollection.Contains(otherStaff));

			staffCollection = new GlbStaffCollection(Factory, StaffFilterProvider.GetDriverFilter(OrgConstants.FilterControl.DriverStatus.Code.NonDriver));

			Assert("Filtered for NOT Driver staff, Collection should NOT contain driverStaff1", !staffCollection.Contains(driverStaff1));
			Assert("Filtered for NOT Driver staff, Collection should NOT contain driverStaff2", !staffCollection.Contains(driverStaff2));
			Assert("Filtered for NOT Driver staff, Collection should contain OtherStaff", staffCollection.Contains(otherStaff));

			staffCollection = new GlbStaffCollection(Factory, StaffFilterProvider.GetDriverFilter(OrgConstants.FilterControl.DriverStatus.Code.AllStaff));

			Assert("Filtered for ALL staff, Collection should contain driverStaff1", staffCollection.Contains(driverStaff1));
			Assert("Filtered for ALL staff, Collection should contain driverStaff2", staffCollection.Contains(driverStaff2));
			Assert("Filtered for ALL staff, Collection should contain OtherStaff", staffCollection.Contains(otherStaff));

			staffCollection = new GlbStaffCollection(Factory, StaffFilterProvider.GetDriverFilter(""));

			Assert("Empty filter, Collection should contain driverStaff1", staffCollection.Contains(driverStaff1));
			Assert("Empty filter, Collection should contain driverStaff2", staffCollection.Contains(driverStaff2));
			Assert("Empty filter, Collection should contain OtherStaff", staffCollection.Contains(otherStaff));
		}

		#endregion

		#endregion

		#region Implementation

		GlbStaffFilterProvider StaffFilterProvider;

		protected override void SetUp()
		{
			StaffFilterProvider = GetFilterProvider();
		}

		protected virtual GlbStaffFilterProvider GetFilterProvider()
		{
			return new GlbStaffFilterProvider();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GlbStaffFilterProvider();
		}

		#endregion
	}
}
