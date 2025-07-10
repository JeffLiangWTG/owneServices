using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.Warehouse.Yard.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	[TestedType(typeof(CYDAdHocServiceOrderFilterBusinessObject))]
	public class CYDAdHocServiceOrderFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CYDAdHocServiceOrderFilterBusinessObject();
		}

		#region JobNumber
		public void TestJobNumber()
		{
			var whsWharehouse = YardHelper.CreateContainerYardInCurrentBranch();
			Factory.Save();
			var adHocServiceOrder = YardHelper.CreateAdHocServiceOrder(whsWharehouse);
			Factory.Save();

			var adHocServiceOrder2 = YardHelper.CreateAdHocServiceOrder(whsWharehouse);
			Factory.Save();

			var filters = GetNewFilterStripBusinessObject();
			Factory.Save();
			Asserter.AddToScope(adHocServiceOrder, adHocServiceOrder2);

			var filter = filters["JobNumber"] as ModuleTextFilter;
			filter.IsActive = true;
			AssertEquals(FilterCategories.NumbersAndReferences, filter.Category);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = adHocServiceOrder.YAO_JobNumber;
			Factory.Save();
			Asserter.AssertMatches("Must return the AdHocServiceOrder with specified Job number", filters.Filter, adHocServiceOrder);
		}
		#endregion

		#region ServiceOrderStatus
		public void TestPendingStatus()
		{
			var whsWharehouse = YardHelper.CreateContainerYardInCurrentBranch();
			Factory.Save();
			var adHocServiceOrder1 = YardHelper.CreateAdHocServiceOrder(whsWharehouse);
			var yardUnitState1 = Factory.NewWithValidTestData<CYDYardUnitState>();
			yardUnitState1.YUS_WW_CurrentYard = whsWharehouse.PK;
			Factory.Save();
			YardHelper.CreateAdHocServiceWithJobService(adHocServiceOrder1, yardUnitState1, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty);

			var adHocServiceOrder2 = YardHelper.CreateAdHocServiceOrder(whsWharehouse);
			var yardUnitState2 = Factory.NewWithValidTestData<CYDYardUnitState>();
			yardUnitState2.YUS_WW_CurrentYard = whsWharehouse.PK;
			Factory.Save();
			YardHelper.CreateAdHocServiceWithJobService(adHocServiceOrder2, yardUnitState2, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty);
			YardHelper.CreateAdHocServiceWithJobService(adHocServiceOrder2, yardUnitState2, ZDateTimeOffset.Now, ZDateTimeOffset.Empty);

			var filters = GetNewFilterStripBusinessObject();
			Factory.Save();

			Asserter.AddToScope(adHocServiceOrder1, adHocServiceOrder2);

			var filter = filters["Service Order Status"] as ModuleTextFilter;
			filter.IsActive = true;
			AssertEquals(FilterCategories.TextSearch, filter.Category);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "PEN";
			Factory.Save();

			Asserter.AssertMatches("Must return AdHocServiceOrders that have all linked AdHocServices as not yet booked", filters.Filter, adHocServiceOrder1);
		}

		public void TestInProgressStatus()
		{
			var whsWharehouse = YardHelper.CreateContainerYardInCurrentBranch();
			Factory.Save();
			var adHocServiceOrder1 = YardHelper.CreateAdHocServiceOrder(whsWharehouse);
			var yardUnitState1 = Factory.NewWithValidTestData<CYDYardUnitState>();
			yardUnitState1.YUS_WW_CurrentYard = whsWharehouse.PK;
			Factory.Save();
			YardHelper.CreateAdHocServiceWithJobService(adHocServiceOrder1, yardUnitState1, ZDateTimeOffset.Now, ZDateTimeOffset.Empty);
			YardHelper.CreateAdHocServiceWithJobService(adHocServiceOrder1, yardUnitState1, ZDateTimeOffset.Now, ZDateTimeOffset.Now);

			var adHocServiceOrder2 = YardHelper.CreateAdHocServiceOrder(whsWharehouse);
			var yardUnitState2 = Factory.NewWithValidTestData<CYDYardUnitState>();
			yardUnitState2.YUS_WW_CurrentYard = whsWharehouse.PK;
			Factory.Save();
			YardHelper.CreateAdHocServiceWithJobService(adHocServiceOrder2, yardUnitState2, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty);
			YardHelper.CreateAdHocServiceWithJobService(adHocServiceOrder2, yardUnitState2, ZDateTimeOffset.Now, ZDateTimeOffset.Empty);

			var filters = GetNewFilterStripBusinessObject();
			Factory.Save();
			Asserter.AddToScope(adHocServiceOrder1, adHocServiceOrder2);

			var filter = filters["Service Order Status"] as ModuleTextFilter;
			filter.IsActive = true;
			AssertEquals(FilterCategories.TextSearch, filter.Category);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "PRO";
			Factory.Save();

			Asserter.AssertMatches("Must return AdHocServiceOrders that have all linked AdHocServices as not yet booked", filters.Filter, [adHocServiceOrder1, adHocServiceOrder2]);
		}

		public void TestCompletedStatus()
		{
			var whsWharehouse = YardHelper.CreateContainerYardInCurrentBranch();
			Factory.Save();
			var adHocServiceOrder1 = YardHelper.CreateAdHocServiceOrder(whsWharehouse);
			var yardUnitState1 = Factory.NewWithValidTestData<CYDYardUnitState>();
			yardUnitState1.YUS_WW_CurrentYard = whsWharehouse.PK;
			Factory.Save();
			YardHelper.CreateAdHocServiceWithJobService(adHocServiceOrder1, yardUnitState1, ZDateTimeOffset.Now, ZDateTimeOffset.Now);
			YardHelper.CreateAdHocServiceWithJobService(adHocServiceOrder1, yardUnitState1, ZDateTimeOffset.Now, ZDateTimeOffset.Now);

			var adHocServiceOrder2 = YardHelper.CreateAdHocServiceOrder(whsWharehouse);
			var yardUnitState2 = Factory.NewWithValidTestData<CYDYardUnitState>();
			yardUnitState2.YUS_WW_CurrentYard = whsWharehouse.PK;
			Factory.Save();
			YardHelper.CreateAdHocServiceWithJobService(adHocServiceOrder2, yardUnitState2, ZDateTimeOffset.Now, ZDateTimeOffset.Empty);
			YardHelper.CreateAdHocServiceWithJobService(adHocServiceOrder2, yardUnitState2, ZDateTimeOffset.Now, ZDateTimeOffset.Now);

			var filters = GetNewFilterStripBusinessObject();
			Factory.Save();
			Asserter.AddToScope(adHocServiceOrder1, adHocServiceOrder2);

			var filter = filters["Service Order Status"] as ModuleTextFilter;
			filter.IsActive = true;
			AssertEquals(FilterCategories.TextSearch, filter.Category);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "COM";
			Factory.Save();

			Asserter.AssertMatches("Must return AdHocServiceOrders that have all linked AdHocServices as not yet booked", filters.Filter, adHocServiceOrder1);
		}

		#endregion

		#region Client
		public void TestClient()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var whsWharehouse = YardHelper.CreateContainerYardInCurrentBranch(address: address);
			Factory.Save();

			var adHocServiceOrder1 = YardHelper.CreateAdHocServiceOrder(whsWharehouse);
			var adHocServiceOrder2 = YardHelper.CreateAdHocServiceOrder(whsWharehouse);
			Factory.Save();

			var client = adHocServiceOrder2.Client;
			client.E2_OA_Address = address.PK;
			Factory.Save();

			var filters = GetNewFilterStripBusinessObject();
			Factory.Save();
			Asserter.AddToScope(adHocServiceOrder1, adHocServiceOrder2);

			var filter = filters["Client"] as ModuleTextFilter;
			filter.IsActive = true;
			AssertEquals(FilterCategories.TextSearch, filter.Category);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "Header";
			Factory.Save();
			Asserter.AssertMatches("Must return the AdHocServiceOrder with specified Client", filters.Filter, adHocServiceOrder2);
		}
		#endregion

		#region ClientReference
		public void TestClientReference()
		{
			var whsWharehouse = YardHelper.CreateContainerYardInCurrentBranch();
			Factory.Save();
			var adHocServiceOrder1 = YardHelper.CreateAdHocServiceOrder(whsWharehouse);
			var adHocServiceOrder2 = YardHelper.CreateAdHocServiceOrder(whsWharehouse);
			adHocServiceOrder2.YAO_ClientReference = "Mr WiseTechGlobal Reference";
			Factory.Save();

			var filters = GetNewFilterStripBusinessObject();
			Factory.Save();
			Asserter.AddToScope(adHocServiceOrder1, adHocServiceOrder2);

			var filter = filters["Client Reference"] as ModuleTextFilter;
			filter.IsActive = true;
			AssertEquals(FilterCategories.TextSearch, filter.Category);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "Mr WiseTechGlobal Reference";
			Factory.Save();
			Asserter.AssertMatches("Must return the AdHocServiceOrder with specified Client Reference", filters.Filter, adHocServiceOrder2);
		}
		#endregion

		#region BillingDate
		public void TestBillingDate()
		{
			var whsWharehouse = YardHelper.CreateContainerYardInCurrentBranch();
			Factory.Save();
			var adHocServiceOrder1 = YardHelper.CreateAdHocServiceOrder(whsWharehouse);
			adHocServiceOrder1.YAO_BillingDate = ZDate.BrettsBirthday;
			var adHocServiceOrder2 = YardHelper.CreateAdHocServiceOrder(whsWharehouse);
			adHocServiceOrder2.YAO_BillingDate = ZDate.Today;
			Factory.Save();

			var filters = GetNewFilterStripBusinessObject();
			Factory.Save();
			Asserter.AddToScope(adHocServiceOrder1, adHocServiceOrder2);

			var filter = filters["Billing Date"] as ModuleDateFilter;
			filter.IsActive = true;
			AssertEquals(FilterCategories.Dates, filter.Category);
			filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today;
			Factory.Save();
			Asserter.AssertMatches("Must return the AdHocServiceOrder with specified Billing Date", filters.Filter, adHocServiceOrder2);
		}
		#endregion

		protected FilterStripAsserter<CYDAdHocServiceOrder> Asserter
		{
			get { return asserter ?? (asserter = new FilterStripAsserter<CYDAdHocServiceOrder>(Factory, x => x.YAO_JobNumber)); }
		}
		FilterStripAsserter<CYDAdHocServiceOrder> asserter;

		CYDYardTestHelper YardHelper => new CYDYardTestHelper(Factory);
	}
}
