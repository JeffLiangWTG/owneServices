using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(AdHocServiceJobFilterBusinessObject))]
	public class AdHocServiceJobFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region TestClient

		public void TestClient()
		{
			var data = new TestDataForInventory(Factory, new TestNotificationBuffer());
			data.CreateMultiWarehouseClientProductInventory();

			var adhocServiceJob1 = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			var adhocServiceJob2 = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org2, ZDateTime.Today);

			Factory.Save();

			Asserter.AddToScope(adhocServiceJob1, adhocServiceJob2);

			var filter = (ModuleGuidFilter)FilterStrip.ModuleFilters[AdHocServiceJobFilterBusinessObject.FilterConstants.Client];
			filter.IsActive = true;
			AssertEquals("ClientPK: ", ZGuid.Empty, FilterStrip.ClientPK);
			AssertEquals(FilterCategories.Organisations, filter.Category);
			Asserter.AssertMatches("", filter, adhocServiceJob1, adhocServiceJob2);

			filter.Property = data.Org1.PK;
			Asserter.AssertMatches("", filter, adhocServiceJob1);
			AssertEquals("ClientPK: ", data.Org1.PK, FilterStrip.ClientPK);

			filter.Property = data.Org2.PK;
			Asserter.AssertMatches("", filter, adhocServiceJob2);
			AssertEquals("ClientPK: ", data.Org2.PK, FilterStrip.ClientPK);

			var otherClientGuid = ZGuid.NewZGuid();
			filter.Property = otherClientGuid;
			Asserter.AssertMatches("", filter);
			AssertEquals("ClientPK: ", otherClientGuid, FilterStrip.ClientPK);
		}

		public void TestFilterClientVisibility()
		{
			var filterBizObj = new AdHocServiceJobFilterBusinessObject();
			AssertEquals("Precondition: WhsAllowedWarehouses", true, Env.Security.WhsAllowedClients.IsAllowed);
			var clientFilter = (ModuleGuidFilter)filterBizObj["Client"];
			AssertEquals("Visibility", FilterVisibility.Visible, clientFilter.Visibility);

			Env.Security.WhsAllowedClients.IsAllowed = false;
			var filterBizObj2 = new AdHocServiceJobFilterBusinessObject();
			var clientfilter2 = (ModuleGuidFilter)filterBizObj2["Client"];
			AssertEquals("Visibility", FilterVisibility.AlwaysVisible, clientfilter2.Visibility);
		}

		#endregion

		#region TestWarehouse

		public void TestWarehouse()
		{
			var data = new TestDataForInventory(Factory, new TestNotificationBuffer());
			data.CreateMultiWarehouseClientProductInventory();

			var adhocServiceJob1 = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			var adhocServiceJob2 = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org2, ZDateTime.Today);
			var adhocServiceJob3 = Helper.CreateWhsAdHocServiceJob(data.Whs2, data.Org1, ZDateTime.Today);

			Factory.Save();

			Asserter.AddToScope(adhocServiceJob1, adhocServiceJob2, adhocServiceJob3);

			var filter = (ModuleGuidFilter)FilterStrip.ModuleFilters[AdHocServiceJobFilterBusinessObject.FilterConstants.Warehouse];
			filter.IsActive = true;
			AssertEquals("WarehousePK: ", ZGuid.Empty, FilterStrip.WarehousePK);
			AssertEquals(FilterCategories.Organisations, filter.Category);
			Asserter.AssertMatches("", filter, adhocServiceJob1, adhocServiceJob2, adhocServiceJob3);

			filter.Property = data.Whs1.PK;
			Asserter.AssertMatches("", filter, adhocServiceJob1, adhocServiceJob2);
			AssertEquals("WarehousePK: ", data.Whs1.PK, FilterStrip.WarehousePK);

			filter.Property = data.Whs2.PK;
			Asserter.AssertMatches("", filter, adhocServiceJob3);
			AssertEquals("WarehousePK: ", data.Whs2.PK, FilterStrip.WarehousePK);

			var otherWarehouseGuid = ZGuid.NewZGuid();
			filter.Property = otherWarehouseGuid;
			Asserter.AssertMatches("", filter);
			AssertEquals("WarehousePK: ", otherWarehouseGuid, FilterStrip.WarehousePK);
		}

		public void TestFilterWarehouseVisibility()
		{
			var filterBizObj = new AdHocServiceJobFilterBusinessObject();
			AssertEquals("Precondition: WhsAllowedWarehouses", true, Env.Security.WhsAllowedWarehouses.IsAllowed);
			var warehouseFilter = (ModuleGuidFilter)filterBizObj["Warehouse"];
			AssertEquals("Visibility", FilterVisibility.Visible, warehouseFilter.Visibility);

			Env.Security.WhsAllowedWarehouses.IsAllowed = false;
			var filterBizObj2 = new AdHocServiceJobFilterBusinessObject();
			var warehousefilter2 = (ModuleGuidFilter)filterBizObj2["Warehouse"];
			AssertEquals("Visibility", FilterVisibility.AlwaysVisible, warehousefilter2.Visibility);
		}

		#endregion

		#region TestWSJ_CustomerReference

		public void TestWSJ_CustomerReference()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var adhocServiceJob1 = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			var adhocServiceJob2 = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			var adhocServiceJob3 = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			adhocServiceJob1.WSJ_CustomerReference = "123";
			adhocServiceJob2.WSJ_CustomerReference = "456";
			adhocServiceJob3.WSJ_CustomerReference = "123xxx";
			Factory.Save();
			Asserter.AddToScope(adhocServiceJob1, adhocServiceJob2, adhocServiceJob3);

			var filter = (ModuleTextFilter)FilterStrip.ModuleFilters[AdHocServiceJobFilterBusinessObject.FilterConstants.WSJ_CustomerReference];
			AssertEquals(FilterCategories.NumbersAndReferences, filter.Category);

			filter.Property = "123";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			Asserter.AssertMatches("Should match correct Ad Hoc Service Job.", filter, adhocServiceJob1, adhocServiceJob3);

			filter.Property = "456";
			Asserter.AssertMatches("Should match correct Ad Hoc Service Job.", filter, adhocServiceJob2);

			filter.Property = "XXX";
			Asserter.AssertMatches("Should match no Ad Hoc Service Jobs.", filter);
		}

		#endregion

		#region TestBillingDate

		public void TestBillingDate()
		{
			var billingDateFilter = Array.Find(FilterStrip.ModuleFilters.ToSortedArrayWithIsExclusiveLast(),
				filter => filter.Code == "Billing Date" && filter.Category.Description == "Dates");
			AssertNotNull("Billing Date Filter Exists", billingDateFilter);
		}

		[TestDate(2016, 2, 3)]
		public void TestBillingDate_DateRange()
		{
			var data = new TestDataForInventory(Factory, new TestNotificationBuffer());
			data.CreateMultiWarehouseClientProductInventory();

			var adhocServiceJob1 = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			var adhocServiceJob2 = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org2, ZDateTime.Today.AddDays(7));
			var adhocServiceJob3 = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today.AddDays(-7));

			Factory.Save();

			Asserter.AddToScope(adhocServiceJob1, adhocServiceJob2, adhocServiceJob3);

			var filter = (ModuleDateFilter)FilterStrip.ModuleFilters[AdHocServiceJobFilterBusinessObject.FilterConstants.BillingDate];
			AssertEquals(FilterCategories.Dates, filter.Category);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			Asserter.AssertMatches("", filter, adhocServiceJob1, adhocServiceJob2, adhocServiceJob3);

			filter.Property1 = ZDateTime.Today.AddDays(-100);
			filter.Property2 = ZDateTime.Today.AddDays(100);
			Asserter.AssertMatches("", filter, adhocServiceJob1, adhocServiceJob2, adhocServiceJob3);

			filter.Property1 = ZDateTime.Today;
			filter.Property2 = ZDateTime.Today.AddDays(1);
			Asserter.AssertMatches("", filter, adhocServiceJob1);

			filter.Property1 = ZDateTime.Today.AddDays(-1);
			filter.Property2 = ZDateTime.Today;
			Asserter.AssertMatches("", filter, adhocServiceJob1);

			filter.Property1 = ZDateTime.Today;
			filter.Property2 = ZDateTime.Today.AddDays(10);
			Asserter.AssertMatches("", filter, adhocServiceJob1, adhocServiceJob2);

			filter.Property1 = ZDateTime.Today.AddDays(-10);
			filter.Property2 = ZDateTime.Today;
			Asserter.AssertMatches("", filter, adhocServiceJob1, adhocServiceJob3);
		}

		#endregion

		#region TestAdHocJobNumber

		public void TestAdHocJobNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var adhocServiceJob1 = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			var adhocServiceJob2 = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			var adhocServiceJob3 = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			adhocServiceJob1.WSJ_JobNumber = "WI0001";
			adhocServiceJob2.WSJ_JobNumber = "WI0002";
			adhocServiceJob3.WSJ_JobNumber = "WI1003";
			Factory.Save();
			Asserter.AddToScope(adhocServiceJob1, adhocServiceJob2, adhocServiceJob3);

			var filter = (ModuleFountainFilter)FilterStrip.ModuleFilters[AdHocServiceJobFilterBusinessObject.FilterConstants.AdHocJobNumber];
			AssertEquals(FilterCategories.NumbersAndReferences, filter.Category);

			filter.Property = "WI0";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			Asserter.AssertMatches("Should match correct Ad Hoc Service Job.", filter, adhocServiceJob1, adhocServiceJob2);

			filter.Property = "WI1";
			Asserter.AssertMatches("Should match correct Ad Hoc Service Job.", filter, adhocServiceJob3);

			filter.Property = "WI2";
			Asserter.AssertMatches("Should match no Ad Hoc Service Jobs.", filter);

			adhocServiceJob1.WSJ_JobNumber = "ABC001";
			filter.Property = "WI0";
			Asserter.AssertMatches("Should match correct Ad Hoc Service Job.", filter, adhocServiceJob2);

			filter.Property = "";
			Asserter.AssertMatches("Empty filter should return all jobs.", filter, adhocServiceJob1, adhocServiceJob2, adhocServiceJob3);
		}

		#endregion

		#region TestFilterMaxLength

		public void TestFilterMaxLength_ForJobHeaderDescription()
		{
			var filter = FilterStrip.ModuleFilters[AdHocServiceJobFilterBusinessObject.FilterConstants.WSJ_CustomerReference];
			AssertEquals("CustomerReference filter should contain MaxLength for JH_Description.", WhsAdHocServiceJobSchema.WSJ_CustomerReference.MaxLength, filter.MaxLength);
		}

		#endregion

		#region TestQueryJobWhenCreatedByAnotherCompany

		public void TestQueryJobWhenCreatedByAnotherCompany()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			Factory.Save();

			var filterObject1 = GetNewFilterStripBusinessObject();
			var adHocJobs1 = Factory.Load<WhsAdHocServiceJob>(filterObject1.Filter);
			AssertEquals("Should find an adhoc job when in current company.", 1, adHocJobs1.Length);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "NCP";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "NBR";
			branch.GB_GC = company.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var filterObject2 = GetNewFilterStripBusinessObject();
				var adHocJobs2 = Factory.Load<WhsAdHocServiceJob>(filterObject2.Filter);
				AssertEquals("Should not find any adhoc job when in different company.", 0, adHocJobs2.Length);
			}
		}

		#endregion

		#region TestServiceTypeDateBooked_WithNoFields

		public void TestServiceTypeDateBooked_WithNoFields()
		{
			var filterDate = new ZDate(2023, 12, 08);
			var assertionMessage = "All AdHocServiceJobs should be considered.";

			TestServiceTypeFilters(
				ServiceTypeDateFilter.ServiceTypeDateBooked,
				filterDate,
				assertionMessage,
				bookedFilter =>
				{
					bookedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
					bookedFilter.IsActive = true;
				},
				(job1, job2, job3) => new[] { job1, job2, job3 });
		}

		#endregion

		#region TestServiceTypeDateBooked_WithDateField

		public void TestServiceTypeDateBooked_WithDateField()
		{
			var filterDate = new ZDate(2023, 12, 08);
			var assertionMessage = "AdHocServiceJobs with matching date booked should only be considered.";

			TestServiceTypeFilters(
				ServiceTypeDateFilter.ServiceTypeDateBooked,
				filterDate,
				assertionMessage,
				bookedFilter =>
				{
					bookedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
					bookedFilter.Property2 = filterDate;
					bookedFilter.IsActive = true;
				},
				(job1, job2, job3) => new[] { job2 });
		}

		#endregion

		#region TestServiceTypeDateBooked_WithServiceTypeField

		public void TestServiceTypeDateBooked_WithServiceTypeField()
		{
			var filterDate = new ZDate(2023, 12, 08);
			var assertionMessage = "AdHocServiceJobs with matching service type should only be considered.";

			TestServiceTypeFilters(
				ServiceTypeDateFilter.ServiceTypeDateBooked,
				filterDate,
				assertionMessage,
				bookedFilter =>
				{
					bookedFilter.JobServiceType = "FUM";
					bookedFilter.IsActive = true;
				},
				(job1, job2, job3) => new[] { job2 });
		}

		#endregion

		#region TestServiceTypeDateBooked_WithBothDateAndServiceTypeFields

		public void TestServiceTypeDateBooked_WithBothDateAndServiceTypeFields()
		{
			var filterDate = new ZDate(2023, 12, 08);
			var assertionMessage = "AdHocServiceJobs with matching service type and booked date should only be considered.";

			TestServiceTypeFilters(
				ServiceTypeDateFilter.ServiceTypeDateBooked,
				filterDate,
				assertionMessage,
				bookedFilter =>
				{
					bookedFilter.JobServiceType = "FUM";
					bookedFilter.Property2 = filterDate;
					bookedFilter.IsActive = true;
				},
				(job1, job2, job3) => new[] { job2 });
		}

		#endregion

		#region TestServiceTypeDateCompleted_WithNoFields

		public void TestServiceTypeDateCompleted_WithNoFields()
		{
			var filterDate = new ZDate(2023, 12, 08);
			var assertionMessage = "All AdHocServiceJobs should be considered.";

			TestServiceTypeFilters(
				ServiceTypeDateFilter.ServiceTypeDateCompleted,
				filterDate,
				assertionMessage,
				completedFilter =>
				{
					completedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
					completedFilter.IsActive = true;
				},
				(job1, job2, job3) => new[] { job1, job2, job3 });
		}

		#endregion

		#region TestServiceTypeDateCompleted_WithDateField

		public void TestServiceTypeDateCompleted_WithDateField()
		{
			var filterDate = new ZDate(2023, 12, 08);
			var assertionMessage = "AdHocServiceJobs with matching completed date should only be considered.";

			TestServiceTypeFilters(
				ServiceTypeDateFilter.ServiceTypeDateCompleted,
				filterDate,
				assertionMessage,
				completedFilter =>
				{
					completedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
					completedFilter.Property2 = filterDate;
					completedFilter.IsActive = true;
				},
				(job1, job2, job3) => new[] { job2 });
		}

		#endregion

		#region TestServiceTypeDateCompleted_WithServiceTypeField

		public void TestServiceTypeDateCompleted_WithServiceTypeField()
		{
			var filterDate = new ZDate(2023, 12, 08);
			var assertionMessage = "AdHocServiceJobs with matching service type should only be considered.";

			TestServiceTypeFilters(
				ServiceTypeDateFilter.ServiceTypeDateCompleted,
				filterDate,
				assertionMessage,
				completedFilter =>
				{
					completedFilter.JobServiceType = "FUM";
					completedFilter.IsActive = true;
				},
				(job1, job2, job3) => new[] { job2 });
		}

		#endregion

		#region TestServiceTypeDateCompleted_WithBothDateAndServiceTypeFields

		public void TestServiceTypeDateCompleted_WithBothDateAndServiceTypeFields()
		{
			var filterDate = new ZDate(2023, 12, 08);
			var assertionMessage = "AdHocServiceJobs with matching service type and completed date should only be considered.";

			TestServiceTypeFilters(
				ServiceTypeDateFilter.ServiceTypeDateCompleted,
				filterDate,
				assertionMessage,
				completedFilter =>
				{
					completedFilter.JobServiceType = "FUM";
					completedFilter.Property2 = filterDate;
					completedFilter.IsActive = true;
				},
				(job1, job2, job3) => new[] { job2 });
		}

		#endregion

		#region Implementation

		void TestServiceTypeFilters(string filterName, ZDate filterDate, string assertionMessage, Action<ServiceTypeDateFilter> setupFilter, Func<WhsAdHocServiceJob, WhsAdHocServiceJob, WhsAdHocServiceJob, IEnumerable<WhsAdHocServiceJob>> getExpectedJobs)
		{
			var data = new TestDataForInventory(Factory, new TestNotificationBuffer());
			data.CreateMultiWarehouseClientProductInventory();

			var adHocServiceJob1 = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			var adHocServiceJob2 = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org2, ZDateTime.Today.AddDays(7));
			var adHocServiceJob3 = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today.AddDays(-7));
			
			var fumigationService = adHocServiceJob2.Services.AddNew();
			fumigationService.ES_ServiceCode = FreightServiceType.Codes.Fumigation;
			fumigationService.ES_ServiceCount = 30m;

			if (filterName.Equals(ServiceTypeDateFilter.ServiceTypeDateCompleted))
			{
				fumigationService.ES_Completed = filterDate;
			}
			else
			{
				fumigationService.ES_Booked = filterDate;
			}

			Factory.Save();
			Asserter.AddToScope(adHocServiceJob1, adHocServiceJob2, adHocServiceJob3);

			var filter = (ServiceTypeDateFilter)FilterStrip.ModuleFilters[filterName];
			setupFilter(filter);

			Asserter.AssertMatches(assertionMessage, filter, getExpectedJobs(adHocServiceJob1, adHocServiceJob2, adHocServiceJob3).ToArray());
		}

		FilterStripAsserter<WhsAdHocServiceJob> Asserter => asserter ?? (asserter = new FilterStripAsserter<WhsAdHocServiceJob>(Factory, s => s.WSJ_JobNumber));
		FilterStripAsserter<WhsAdHocServiceJob> asserter;

		AdHocServiceJobFilterBusinessObject FilterStrip => filterStrip ?? (filterStrip = GetNewFilterStrip());
		AdHocServiceJobFilterBusinessObject filterStrip;

		AdHocServiceJobFilterBusinessObject GetNewFilterStrip()
		{
			var filterStrip = new AdHocServiceJobFilterBusinessObject();
			filterStrip.AddActiveStatusFilters(typeof(WhsAdHocServiceJob));
			return filterStrip;
		}

		WhsTestHelperFunctions Helper => helper = helper ?? new WhsTestHelperFunctions(Factory);
		WhsTestHelperFunctions helper;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new AdHocServiceJobFilterBusinessObject();
		}

		#endregion
	}
}
