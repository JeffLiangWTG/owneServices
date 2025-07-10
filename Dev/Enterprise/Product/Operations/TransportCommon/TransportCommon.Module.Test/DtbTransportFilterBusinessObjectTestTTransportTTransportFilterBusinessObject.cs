using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Module;
using Enterprise.TransportCommon.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportCommon.Module.Testing
{
	public abstract class DtbTransportFilterBusinessObjectTest<TTransport, TTransportFilterBusinessObject> : FilterStripBusinessObjectTestCase
			where TTransport : DtbTransport
			where TTransportFilterBusinessObject : DtbTransportFilterBusinessObject<TTransport>, new()
	{
		#region TestWorkflowFilters

		#region TestGetModuleFiltersWhenCustomFilterNamesClashWithReservedNames

		public void TestGetModuleFiltersWhenCustomFilterNamesClashWithReservedNames()
		{
			var jobOpen = "Job Open";
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.DtbBookingWorkflowDescriptorCode;

			var columnDef = template.GenCustomColumnDefinitions.AddNew();
			columnDef.XC_Name = jobOpen;
			columnDef.XC_Type = Enterprise.MasterFiles.Business.CustomValues.AddOnColumnDataType.Codes.Datetime;

			template.Factory.Save();
			Assert("template should have been saved", template.IsInDatabase);

			Factory.Save();

			var collection = new TTransportFilterBusinessObject().ModuleFilters;

			AssertNotNull(collection[jobOpen]);
			AssertNotNull(collection[jobOpen + " " + WorkflowCustomFieldsFilter.WorkflowCustomFieldDescriptionDuplicateSuffix]);
		}
		#endregion

		#region TestWorkflowFiltersPresent

		public void TestWorkflowFiltersPresent()
		{
			var relatedFilters = FilterStrip.ModuleFilters.Where(f => f.Category.Description.ToString() == "Workflow Milestones").ToArray();
			AssertEquals(4, relatedFilters.Length);
			relatedFilters.Single(f => f.Description.ToString() == "Milestone Date");
			relatedFilters.Single(f => f.Description.ToString() == "Milestone Completed");
			relatedFilters.Single(f => f.Description.ToString() == "Next Milestone");
			relatedFilters.Single(f => f.Description.ToString() == "Last Completed Milestone");
		}

		#endregion

		#region TestMilestoneFilters

		public void TestMilestoneDateFilter()
		{
			var year = ZDateTime.Now.Year;

			var transport1 = GetNewTransport();
			var transport2 = GetNewTransport();
			Asserter.AddToScope(transport1);
			Asserter.AddToScope(transport2);

			var milestone1 = transport1.WorkflowItems.Milestones.AddNew();
			var milestone2 = transport2.WorkflowItems.Milestones.AddNew();

			milestone1.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(year, 1, 2)));
			milestone2.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(year, 7, 1)));

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (WorkflowModuleFilter)filterBizO.ModuleFilters["Milestone Date"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(year, 1, 1);
			filter.Property2 = new ZDateTime(year, 1, 3);
			filter.IsActive = true;

			Asserter.AssertMatches("", filter, transport1);
		}

		public void TestMilestoneCompletedFilter()
		{
			var year = ZDateTime.Now.Year;

			var transport1 = GetNewTransport();
			var transport2 = GetNewTransport();
			Asserter.AddToScope(transport1);
			Asserter.AddToScope(transport2);

			var milestone1 = transport1.WorkflowItems.Milestones.AddNew();
			var milestone2 = transport2.WorkflowItems.Milestones.AddNew();

			var trigger = transport1.WorkflowItems.Triggers.AddNew();
			var exception = transport2.WorkflowItems.Exceptions.AddNew();

			milestone1.SetMilestoneActualDateForTest(new ZDateTime(year, 1, 2));
			milestone2.SetMilestoneActualDateForTest(ZDateTime.Empty);
			exception.SetMilestoneActualDateForTest(new ZDateTimeOffset(new ZDateTime(year, 1, 2)));
			trigger.SetMilestoneActualDateForTest(ZDateTime.Empty);

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizO.ModuleFilters["Milestone Completed"];
			filter.Property = "Completed";
			filter.IsActive = true;

			Asserter.AssertMatches("", filter, transport1);

			filter.Property = "Not Completed";
			filter.IsActive = true;

			Asserter.AssertMatches("", filter, transport2);
		}

		public void TestMilestoneNextFilter()
		{
			var year = ZDateTime.Now.Year;

			var transport1 = GetNewTransport();
			var transport2 = GetNewTransport();
			Asserter.AddToScope(transport1);
			Asserter.AddToScope(transport2);

			var milestone1 = transport1.WorkflowItems.Milestones.AddNew();
			milestone1.TriggerConditions.TriggerEventCode = "AID";
			var milestone2 = transport2.WorkflowItems.Milestones.AddNew();
			milestone2.TriggerConditions.TriggerEventCode = "AID";
			milestone1.P9_Type = "MIL";
			milestone2.P9_Type = "MIL";

			milestone1.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(year, 1, 2)));
			milestone2.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(year, 7, 1)));

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (WorkflowModuleFilter)filterBizO.ModuleFilters["Next Milestone"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(year, 1, 1);
			filter.Property2 = new ZDateTime(year, 1, 3);
			filter.IsActive = true;

			Asserter.AssertMatches("", filter, transport1);
		}

		public void TestMilestoneLastCompletedFilter()
		{
			var year = ZDateTime.Now.Year;

			var transport1 = GetNewTransport();
			var transport2 = GetNewTransport();
			Asserter.AddToScope(transport1);
			Asserter.AddToScope(transport2);

			var milestone1 = transport1.WorkflowItems.Milestones.AddNew();
			var milestone2 = transport2.WorkflowItems.Milestones.AddNew();

			milestone1.P9_Type = "MIL";
			milestone2.P9_Type = "MIL";

			milestone1.SetMilestoneActualDateForTest(new ZDateTime(year, 1, 2));
			milestone2.SetMilestoneActualDateForTest(new ZDateTime(year, 7, 1));

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (WorkflowModuleFilter)filterBizO.ModuleFilters["Last Completed Milestone"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(year, 1, 1);
			filter.Property2 = new ZDateTime(year, 1, 3);
			filter.IsActive = true;

			Asserter.AssertMatches("", filter, transport1);
		}

		#endregion

		#endregion

		public void TestJobInvoicingStatusFilter()
		{
			var transport1 = GetNewTransport();
			Asserter.AddToScope(transport1);

			var filterBizO = GetNewFilterStripBusinessObject();
			AssertNotNull(filterBizO.ModuleFilters["Invoice Status"]);
			var jobstatusFilter = (ModuleTextFilter)filterBizO.ModuleFilters["Invoice Status"];

			var job = new JobHeader.Loader(transport1).TryLoadOrCreate();
			AssertEquals(JobHeaderStatus.Working.Code, job.JH_Status);

			Factory.Save();

			jobstatusFilter.Property = JobHeaderStatus.Working.Code;
			jobstatusFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			jobstatusFilter.IsActive = true;

			Asserter.AssertMatches("", jobstatusFilter, transport1);

			jobstatusFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;

			Asserter.AssertMatches("", jobstatusFilter);
		}

		public void TestInvoicedChargesFilter()
		{
			var transport1 = GetNewTransport();
			Asserter.AddToScope(transport1);

			var filterBizO = GetNewFilterStripBusinessObject();
			AssertNotNull(filterBizO.ModuleFilters["Invoiced / Charges / Billing"]);
			var invoicedChargesFilter = (ModuleFlagsFilter)filterBizO.ModuleFilters["Invoiced / Charges / Billing"];

			var job = new JobHeader.Loader(transport1).TryLoadOrCreate();
			AssertNotNull(job);

			Factory.Save();

			invoicedChargesFilter.IsActive = true;
			invoicedChargesFilter["No Charges"] = true;
			Asserter.AssertMatches("transport1 has no charges and should be find", invoicedChargesFilter, transport1);

			var code = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "CCLR"));
			var charge = Factory.New<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_AC = code.PK;
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge.JR_LocalSellAmt = 10m;
			Factory.Save();

			Asserter.AssertMatches("transport1 has one charge and should not be in the search results", invoicedChargesFilter);
		}

		#region TestOrganisationFilters

		public void TestBranch()
		{
			var branch1 = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK));
			var branch2 = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, new[] { GlbBranch.CurrentBranch.PK, branch1.PK }));

			var transport1 = GetNewTransport();
			var transport2 = GetNewTransport();
			Asserter.AddToScope(transport1);
			Asserter.AddToScope(transport2);

			transport1.KM_GB_Branch = branch1.PK;
			transport2.KM_GB_Branch = branch2.PK;

			Factory.Save();

			var filterBizO = GetNewFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)filterBizO.ModuleFilters["Branch"];
			filter.Property = branch1.PK;
			filter.IsActive = true;
			Asserter.AssertMatches("transport1 has branch1", filter, transport1);

			filter.Property = branch2.PK;
			Asserter.AssertMatches("transport2 has branch2", filter, transport2);
		}

		#endregion

		#region Implementation

		protected FilterStripAsserter<TTransport> Asserter
		{
			get { return asserter ?? (asserter = new FilterStripAsserter<TTransport>(Factory, b => b.KM_JobID)); }
		}

		TTransportFilterBusinessObject FilterStrip
		{
			get { return filterStrip ?? (filterStrip = new TTransportFilterBusinessObject()); }
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new TTransportFilterBusinessObject();
		}

		protected abstract TTransport GetNewTransport();

		FilterStripAsserter<TTransport> asserter;
		TTransportFilterBusinessObject filterStrip;

		#endregion
	}
}
