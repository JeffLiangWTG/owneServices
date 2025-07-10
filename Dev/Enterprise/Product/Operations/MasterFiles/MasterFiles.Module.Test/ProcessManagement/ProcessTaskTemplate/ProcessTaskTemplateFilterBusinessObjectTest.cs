using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ProcessTaskTemplateFilterBusinessObject))]
	sealed class ProcessTaskTemplateFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region FlagsFilterTests

		public void TestIsPartialFilter()
		{
			var partialTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var nonPartialTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			partialTemplate.P0_IsPartialTemplate = true;

			Factory.Save();

			var bizo = new ProcessTaskTemplateFilterBusinessObject();
			var filter = (ModuleTextFilter)bizo["Is Partial"];
			filter.Property = PartialTypeList.Codes.Partial;
			filter.IsActive = true;

			var templates = new ProcessTaskTemplateCollection(Factory);
			templates.Load(bizo.Filter);

			AssertCollectionContains(partialTemplate, templates);
			AssertCollectionNotContains(nonPartialTemplate, templates);

			filter.Property = PartialTypeList.Codes.NonPartial;

			templates = new ProcessTaskTemplateCollection(Factory);
			templates.Load(bizo.Filter);

			AssertCollectionContains(nonPartialTemplate, templates);
			AssertCollectionNotContains(partialTemplate, templates);

			filter.Property = PartialTypeList.Codes.All;

			templates = new ProcessTaskTemplateCollection(Factory);
			templates.Load(bizo.Filter);

			AssertCollectionContains(partialTemplate, templates);
			AssertCollectionContains(nonPartialTemplate, templates);
		}

		public void TestIsUniversalFilter()
		{
			var universalTemplate = MasterFilesTestHelper.CreateUniversalTemplate(Factory, "DUM");
			var nonUniversalTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			Factory.Save();

			var bizo = new ProcessTaskTemplateFilterBusinessObject();
			var filter = (ModuleTextFilter)bizo["Is Universal"];

			filter.Property = UniversalTemplateTypeList.Codes.Universal;
			filter.IsActive = true;

			var templates = new ProcessTaskTemplateCollection(Factory);
			templates.Load(bizo.Filter);

			AssertCollectionContains(universalTemplate, templates);
			AssertCollectionNotContains(nonUniversalTemplate, templates);

			filter.Property = UniversalTemplateTypeList.Codes.NonUniversal;

			templates = new ProcessTaskTemplateCollection(Factory);
			templates.Load(bizo.Filter);

			AssertCollectionContains(nonUniversalTemplate, templates);
			AssertCollectionNotContains(universalTemplate, templates);

			filter.Property = UniversalTemplateTypeList.Codes.All;

			templates = new ProcessTaskTemplateCollection(Factory);
			templates.Load(bizo.Filter);

			AssertCollectionContains(universalTemplate, templates);
			AssertCollectionContains(nonUniversalTemplate, templates);
		}

		#endregion

		#region TextFiltersTests

		public void TestWorkflowTypeFilter()
		{
			AssertEquals("Precondition", false, DataRegistry.Instance.ProductivityWiseModeEnabled);

			ProcessTaskTemplate workflowType1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			ProcessTaskTemplate workflowType2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			workflowType1.P0_ProcessType = "OPP";
			workflowType2.P0_ProcessType = "CON";

			Factory.Save();

			ProcessTaskTemplateFilterBusinessObject filter = new ProcessTaskTemplateFilterBusinessObject();
			var workflowTypeFilter = (ModuleTextFilter)filter["Workflow Type"];
			workflowTypeFilter.Property = "OPP";
			workflowTypeFilter.IsActive = true;

			ProcessTaskTemplateCollection processTTs = new ProcessTaskTemplateCollection(Factory);
			processTTs.Load(filter.Filter);

			AssertCollectionContains(workflowType1, processTTs);
			AssertCollectionNotContains(workflowType2, processTTs);

			foreach (CodeDescriptionPair providerCodePair in workflowTypeFilter.List)
			{
				WorkflowDescriptor provider;
				AssertEquals(true, WorkflowDescriptors.Instance.TryGetValue(providerCodePair.Code, out provider));
				AssertNotNull(provider);
				AssertEquals(
					"Should only include types corresponding to workflow descriptors that support workflow templates or support universal templates, to be consistent with the 'Process Type' field in the form for workflow templates",
					true,
					provider.SupportsWorkflowTemplates || provider.SupportsUniversalTemplates
				);
			}
			AssertCollectionNotContains(
				"Should not have code TBC, as it is not available in the 'Process Type' field in the form for workflow templates, due to the workflow descriptor DtbBookingConfirmationWorkflowDescriptor not supporting workflow templates or universal templates",
				WorkflowDescriptors.DtbBookingConfirmationWorkflowDescriptorCode,
				((CodeDescriptionPairList)workflowTypeFilter.List).GetAllCodes()
			);
		}

		public void TestWorkflowTypeFilter_ProductivityWiseModeEnabled()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;

			var filterBizo = new ProcessTaskTemplateFilterBusinessObject();
			var workflowTypeFilter = (ModuleTextFilter)filterBizo["Workflow Type"];
			workflowTypeFilter.IsActive = true;

			AssertContainsExactElementsInAnyOrder(new[]
			{
				WorkflowDescriptors.AccPayableOrderHeaderCode,
				WorkflowDescriptors.APInvoiceCode,
				WorkflowDescriptors.ARInvoiceCode,
				WorkflowDescriptors.CampaignWorkflowDescriptorCode,
				WorkflowDescriptors.CollectionBatchCode,
				WorkflowDescriptors.CollectionOrderCode,
				WorkflowDescriptors.CommunicationWorkflowDescriptorCode,
				WorkflowDescriptors.CustomerServiceTicketWorkflowDescriptorCode,
				WorkflowDescriptors.GlbAccreditationAttemptWorkflowDescriptorCode,
				WorkflowDescriptors.GlbGroupWorkflowDescriptorCode,
				WorkflowDescriptors.GlbStaffChangeRequestWorkflowDescriptorCode,
				WorkflowDescriptors.GlbStaffDescriptorCode,
				WorkflowDescriptors.GlbStaffHolidayDescriptorCode,
				WorkflowDescriptors.HRCampaignWorkflowDescriptorCode,
				WorkflowDescriptors.HRHiringRequestDescriptorCode,
				WorkflowDescriptors.HRJobApplicationWorkflowDescriptorCode,
				WorkflowDescriptors.HROnBoardingWorkflowDescriptorCode,
				WorkflowDescriptors.HRRecruitmentJobCampaignWorkflowDescriptorCode,
				WorkflowDescriptors.OpportunityWorkflowDescriptorCode,
				WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode,
				WorkflowDescriptors.ProjectWorkflowDescriptorCode,
				WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode,
				WorkflowDescriptors.WorkItemWorkflowDescriptorCode,
				// Should not contain WorkflowDescriptors.StandAloneTaskWorkflowDescriptor, as this is not based on WorkflowDescriptorListWithStandaloneTaskType
			}, ((CodeDescriptionPairList)(workflowTypeFilter.List)).GetAllCodes());
		}

		public void TestTaskFallbackMethodFilter()
		{
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_TaskFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
			template2.P0_TaskFallbackMethod = FallbackTypeList.Codes.EmptyFallback;

			Factory.Save();

			var filter = new ProcessTaskTemplateFilterBusinessObject();
			((ModuleTextFilter)filter["Task Fallback Method"]).Property = FallbackTypeList.Codes.AlwaysFallback;
			((ModuleTextFilter)filter["Task Fallback Method"]).IsActive = true;

			var collection = new ProcessTaskTemplateCollection(Factory);
			collection.Load(filter.Filter);

			AssertCollectionContains(template1, collection);
			AssertCollectionNotContains(template2, collection);
		}

		public void TestMilestoneFallbackMethodFilter()
		{
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_MilestoneFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
			template2.P0_MilestoneFallbackMethod = FallbackTypeList.Codes.EmptyFallback;

			Factory.Save();

			var filter = new ProcessTaskTemplateFilterBusinessObject();
			((ModuleTextFilter)filter["Milestone Fallback Method"]).Property = FallbackTypeList.Codes.AlwaysFallback;
			((ModuleTextFilter)filter["Milestone Fallback Method"]).IsActive = true;

			var collection = new ProcessTaskTemplateCollection(Factory);
			collection.Load(filter.Filter);

			AssertCollectionContains(template1, collection);
			AssertCollectionNotContains(template2, collection);
		}

		public void TestTriggerFallbackMethodFilter()
		{
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_TriggerFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
			template2.P0_TriggerFallbackMethod = FallbackTypeList.Codes.EmptyFallback;

			Factory.Save();

			var filter = new ProcessTaskTemplateFilterBusinessObject();
			((ModuleTextFilter)filter["Trigger Fallback Method"]).Property = FallbackTypeList.Codes.AlwaysFallback;
			((ModuleTextFilter)filter["Trigger Fallback Method"]).IsActive = true;

			var collection = new ProcessTaskTemplateCollection(Factory);
			collection.Load(filter.Filter);

			AssertCollectionContains(template1, collection);
			AssertCollectionNotContains(template2, collection);
		}

		#endregion

		#region Effective Date

		public void TestEffectiveStartDate()
		{
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_EffectiveStartDateUtc = ZDateTime.UtcNow.AddHours(1);
			template2.P0_EffectiveStartDateUtc = ZDateTime.UtcNow.AddDays(27);

			var filters = new ProcessTaskTemplateFilterBusinessObject();
			var filter = (ModuleDateFilter)filters["Effective Start"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.UtcNow;
			filter.Property2 = ZDateTime.UtcNow.AddDays(1);
			filter.IsActive = true;

			var collection = new ProcessTaskTemplateCollection(Factory);
			collection.Load(filters.Filter);

			AssertCollectionContains(template1, collection);
			AssertCollectionNotContains(template2, collection);
		}

		public void TestEffectiveEndDate()
		{
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_EffectiveEndDateUtc = ZDateTime.UtcNow.AddHours(1);
			template2.P0_EffectiveEndDateUtc = ZDateTime.UtcNow.AddDays(27);

			var filters = new ProcessTaskTemplateFilterBusinessObject();
			var filter = (ModuleDateFilter)filters["Effective End"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.UtcNow;
			filter.Property2 = ZDateTime.UtcNow.AddDays(1);
			filter.IsActive = true;

			var collection = new ProcessTaskTemplateCollection(Factory);
			collection.Load(filters.Filter);

			AssertCollectionContains(template1, collection);
			AssertCollectionNotContains(template2, collection);
		}

		public void TestDateFilterVisibility()
		{
			WorkflowDataRegistry.Instance.EnableDateLimitsOnWorkflowTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var filters = new ProcessTaskTemplateFilterBusinessObject();
			Assert(!filters.ModuleFilters.Any(m => m.Description == "Effective End"));
			Assert(!filters.ModuleFilters.Any(m => m.Description == "Effective Start"));

			WorkflowDataRegistry.Instance.EnableDateLimitsOnWorkflowTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			filters = new ProcessTaskTemplateFilterBusinessObject();
			Assert(filters.ModuleFilters.Any(m => m.Description == "Effective End"));
			Assert(filters.ModuleFilters.Any(m => m.Description == "Effective Start"));
		}

		#endregion

		#region RelatedItemFiltersTests

		public void TestLoadPortFilter()
		{
			ProcessTaskTemplate port1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			ProcessTaskTemplate port2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			port1.P0_LoadPortCountry = "AUSYD";
			port2.P0_LoadPortCountry = "UAIEV";

			Factory.Save();

			ProcessTaskTemplateFilterBusinessObject filter = new ProcessTaskTemplateFilterBusinessObject();
			((ModuleNkFilter)filter["Load Port"]).Property = "AUSYD";
			((ModuleNkFilter)filter["Load Port"]).IsActive = true;

			ProcessTaskTemplateCollection processTTs = new ProcessTaskTemplateCollection(Factory);
			processTTs.Load(filter.Filter);

			AssertCollectionContains(port1, processTTs);
			AssertCollectionNotContains(port2, processTTs);
		}

		public void TestDischargePortFilter()
		{
			ProcessTaskTemplate port1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			ProcessTaskTemplate port2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			port1.P0_DischargePortCountry = "AUSYD";
			port2.P0_DischargePortCountry = "UAIEV";

			Factory.Save();

			ProcessTaskTemplateFilterBusinessObject filter = new ProcessTaskTemplateFilterBusinessObject();
			((ModuleNkFilter)filter["Discharge Port"]).Property = "AUSYD";
			((ModuleNkFilter)filter["Discharge Port"]).IsActive = true;

			ProcessTaskTemplateCollection processTTs = new ProcessTaskTemplateCollection(Factory);
			processTTs.Load(filter.Filter);

			AssertCollectionContains(port1, processTTs);
			AssertCollectionNotContains(port2, processTTs);
		}

		#region TestWarehouseFilter

		public void TestWarehouseFilter()
		{
			BusinessObject warehouse1 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IWhsWarehouse)));
			BusinessObject warehouse2 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IWhsWarehouse)));
			BusinessObject warehouse3 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IWhsWarehouse)));

			var warehouse1Template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			warehouse1Template.P0_WW = warehouse1.PK;

			ProcessTaskTemplate warehouse2Template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			warehouse2Template.P0_WW = warehouse2.PK;

			ProcessTaskTemplate warehouse3Template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			warehouse3Template.P0_WW = warehouse3.PK;

			Factory.Save();

			var filterBizO1 = new ProcessTaskTemplateFilterBusinessObject();
			var warehouseFilter1 = (ModuleGuidFilter)filterBizO1["Warehouse"];
			warehouseFilter1.Property = warehouse1.PK;
			warehouseFilter1.IsActive = true;

			var templates1 = new ProcessTaskTemplateCollection(Factory);
			templates1.Load(filterBizO1.Filter);

			AssertCollectionContains(warehouse1Template, templates1);
			AssertCollectionNotContains(warehouse2Template, templates1);
			AssertCollectionNotContains(warehouse3Template, templates1);

			var filterBizO2 = new ProcessTaskTemplateFilterBusinessObject();
			var warehouseFilter2 = (ModuleGuidFilter)filterBizO2["Warehouse"];
			warehouseFilter2.Property = warehouse2.PK;
			warehouseFilter2.IsActive = true;

			var templates2 = new ProcessTaskTemplateCollection(Factory);
			templates2.Load(filterBizO2.Filter);

			AssertCollectionContains(warehouse2Template, templates2);
			AssertCollectionNotContains(warehouse1Template, templates2);
			AssertCollectionNotContains(warehouse3Template, templates2);
		}

		#endregion

		public void TestClientFilter()
		{
			OrgHeader orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();

			ProcessTaskTemplate client0 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			client0.P0_OH_Client = orgHeader1.PK;

			ProcessTaskTemplate client1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			client1.P0_OH_Client = orgHeader2.PK;

			ProcessTaskTemplate client2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			client2.P0_OH_Client = orgHeader2.PK;

			Factory.Save();

			ProcessTaskTemplateFilterBusinessObject orgHeader1Filter = new ProcessTaskTemplateFilterBusinessObject();
			((ModuleGuidFilter)orgHeader1Filter["Client"]).Property = orgHeader1.PK;
			((ModuleGuidFilter)orgHeader1Filter["Client"]).IsActive = true;

			ProcessTaskTemplateCollection processTTs = new ProcessTaskTemplateCollection(Factory);
			processTTs.Load(orgHeader1Filter.Filter);

			AssertCollectionContains(client0, processTTs);
			AssertCollectionNotContains(client1, processTTs);
			AssertCollectionNotContains(client2, processTTs);

			ProcessTaskTemplateFilterBusinessObject orgHeader2Filter = new ProcessTaskTemplateFilterBusinessObject();
			((ModuleGuidFilter)orgHeader2Filter["Client"]).Property = orgHeader2.PK;
			((ModuleGuidFilter)orgHeader2Filter["Client"]).IsActive = true;

			processTTs = new ProcessTaskTemplateCollection(Factory);
			processTTs.Load(orgHeader2Filter.Filter);

			AssertCollectionContains(client1, processTTs);
			AssertCollectionContains(client2, processTTs);
			AssertCollectionNotContains(client0, processTTs);

			ProcessTaskTemplateFilterBusinessObject emptyOrgHeaderFilter = new ProcessTaskTemplateFilterBusinessObject();
			((ModuleGuidFilter)emptyOrgHeaderFilter["Client"]).Property = ZGuid.Empty;
			((ModuleGuidFilter)emptyOrgHeaderFilter["Client"]).IsActive = false;

			processTTs = new ProcessTaskTemplateCollection(Factory);
			processTTs.Load(emptyOrgHeaderFilter.Filter);

			AssertCollectionContains(client0, processTTs);
			AssertCollectionContains(client1, processTTs);
			AssertCollectionContains(client2, processTTs);
		}

		#region Buffer Management System Filter

		public void TestBufferManagementSystemFilter()
		{
			var bms1 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(BufferManagement.Integration.IBMSystem)));
			var bms2 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(BufferManagement.Integration.IBMSystem)));

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_FS_BufferManagementSystem = bms1.PK;

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_FS_BufferManagementSystem = bms1.PK;

			var template3 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template3.P0_FS_BufferManagementSystem = bms2.PK;

			var template4 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template4.P0_FS_BufferManagementSystem = ZGuid.Empty;

			Factory.Save();

			var bms1Filter = new ProcessTaskTemplateFilterBusinessObject();
			((ModuleGuidFilter)bms1Filter["Buffer Management System"]).Property = bms1.PK;
			((ModuleGuidFilter)bms1Filter["Buffer Management System"]).IsActive = true;

			var processTTs = new ProcessTaskTemplateCollection(Factory);
			processTTs.Load(bms1Filter.Filter);

			AssertCollectionContains(template1, processTTs);
			AssertCollectionContains(template2, processTTs);
			AssertCollectionNotContains(template3, processTTs);
			AssertCollectionNotContains(template4, processTTs);

			var bms2Filter = new ProcessTaskTemplateFilterBusinessObject();
			((ModuleGuidFilter)bms2Filter["Buffer Management System"]).Property = bms2.PK;
			((ModuleGuidFilter)bms2Filter["Buffer Management System"]).IsActive = true;

			processTTs = new ProcessTaskTemplateCollection(Factory);
			processTTs.Load(bms2Filter.Filter);

			AssertCollectionNotContains(template1, processTTs);
			AssertCollectionNotContains(template2, processTTs);
			AssertCollectionContains(template3, processTTs);
			AssertCollectionNotContains(template4, processTTs);

			var emptyFilter = new ProcessTaskTemplateFilterBusinessObject();
			((ModuleGuidFilter)emptyFilter["Buffer Management System"]).Property = ZGuid.Empty;
			((ModuleGuidFilter)emptyFilter["Buffer Management System"]).IsActive = false;

			processTTs = new ProcessTaskTemplateCollection(Factory);
			processTTs.Load(emptyFilter.Filter);

			AssertCollectionContains(template1, processTTs);
			AssertCollectionContains(template2, processTTs);
			AssertCollectionContains(template3, processTTs);
			AssertCollectionContains(template4, processTTs);
		}

		#endregion

		#endregion

		#region Criteria Filters

		public void TestCriteriaFilters()
		{
			var template1 = Factory.New<ProcessTaskTemplate>();
			template1.P0_SubType1 = "BOO";
			template1.P0_SubType2 = "BAR";
			template1.P0_SubType3 = "FOO";
			template1.P0_SubType4 = "FAR";
			template1.P0_SubType5 = "BAZ";

			var template2 = Factory.New<ProcessTaskTemplate>();
			template2.P0_SubType1 = "ZOO";
			template2.P0_SubType2 = "ZAR";
			template2.P0_SubType3 = "FOO";
			template2.P0_SubType4 = "FAR";
			template2.P0_SubType5 = "FAZ";

			var filterBizo = new ProcessTaskTemplateFilterBusinessObject();

			var subType1Filter = (ModuleTextFilter)filterBizo["Criteria 1 Code"];
			var subType2Filter = (ModuleTextFilter)filterBizo["Criteria 2 Code"];
			var subType3Filter = (ModuleTextFilter)filterBizo["Criteria 3 Code"];
			var subType4Filter = (ModuleTextFilter)filterBizo["Criteria 4 Code"];
			var subType5Filter = (ModuleTextFilter)filterBizo["Criteria 5 Code"];

			subType1Filter.IsActive = true;
			subType1Filter.Property = "BOO";

			subType2Filter.IsActive = true;
			subType2Filter.Property = "BAR";

			var results = Factory.Load<ProcessTaskTemplate>(filterBizo.Filter);
			AssertEquals(1, results.Length);
			AssertEquals(template1, results[0]);

			subType1Filter.Property = "ZOO";

			results = Factory.Load<ProcessTaskTemplate>(filterBizo.Filter);
			AssertEquals(0, results.Length);

			subType2Filter.Property = "ZAR";

			results = Factory.Load<ProcessTaskTemplate>(filterBizo.Filter);
			AssertEquals(1, results.Length);
			AssertEquals(template2, results[0]);

			subType1Filter.IsActive = subType2Filter.IsActive = false;

			subType3Filter.IsActive = true;
			subType3Filter.Property = "FOO";

			subType4Filter.IsActive = true;
			subType4Filter.Property = "FAR";

			results = Factory.Load<ProcessTaskTemplate>(filterBizo.Filter);
			AssertEquals(2, results.Length);
			AssertCollectionContains(template1, results);
			AssertCollectionContains(template2, results);

			subType5Filter.IsActive = true;
			subType5Filter.Property = "BAZ";

			results = Factory.Load<ProcessTaskTemplate>(filterBizo.Filter);
			AssertEquals(1, results.Length);
			AssertCollectionContains(template1, results);

			subType5Filter.Property = "FAZ";

			results = Factory.Load<ProcessTaskTemplate>(filterBizo.Filter);
			AssertEquals(1, results.Length);
			AssertCollectionContains(template2, results);
		}

		#endregion

		#region WorkflowTasksFiltersTests

		public void TestTasksFilter()
		{
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			var task1 = template1.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "Test1";
			var task2 = template2.WorkflowItems.Tasks.AddNew();
			task2.P9_Description = "Test2";

			Factory.Save();

			var filterBizo = new ProcessTaskTemplateFilterBusinessObject();
			var filter = (TasksModuleFilter)filterBizo["Tasks"];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			filter.SelectedFilters.AddTextFilterStrip("Description", "Test1");
			filter.IsActive = true;

			var results = Factory.Load<ProcessTaskTemplate>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("Using this query:" + filter.Query.LiteralTextSqlFormatted + "has returned these results", results, template1);
		}

		public void TestMilestonesFilter()
		{
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			var task1 = template1.WorkflowItems.Milestones.AddNew();
			task1.P9_Description = "Test1";
			var task2 = template2.WorkflowItems.Milestones.AddNew();
			task2.P9_Description = "Test2";

			Factory.Save();

			var filterBizo = new ProcessTaskTemplateFilterBusinessObject();
			var filter = (MilestonesModuleFilter)filterBizo["Milestones"];
			filter.IsActive = true;

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			filter.SelectedFilters.AddTextFilterStrip("Description", "Test1");

			var results = Factory.Load<ProcessTaskTemplate>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("Using this query:" + filter.Query.LiteralTextSqlFormatted + "has returned these results", results, template1);
		}

		public void TestTriggersFilter()
		{
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			var task1 = template1.WorkflowItems.Triggers.AddNew();
			task1.P9_Description = "Test1";
			var task2 = template2.WorkflowItems.Triggers.AddNew();
			task2.P9_Description = "Test2";

			Factory.Save();

			var filterBizo = new ProcessTaskTemplateFilterBusinessObject();
			var filter = (TriggersModuleFilter)filterBizo["Triggers"];

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			filter.SelectedFilters.AddTextFilterStrip("Description", "Test1");
			filter.IsActive = true;

			var results = Factory.Load<ProcessTaskTemplate>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("Using this query:" + filter.Query.LiteralTextSqlFormatted + "has returned these results", results, template1);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ProcessTaskTemplateFilterBusinessObject();
		}

		#endregion
	}
}
