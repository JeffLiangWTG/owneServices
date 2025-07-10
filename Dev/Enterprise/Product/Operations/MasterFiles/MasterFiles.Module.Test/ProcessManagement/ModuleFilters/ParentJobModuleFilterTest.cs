using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ParentJobModuleFilter))]
	sealed class ParentJobModuleFilterTest : ModuleFilterTestCase<ParentJobModuleFilter>
	{
		public void TestParentJobFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "MYORGSYD";
			org1.OH_FullName = "Hitech Software";
			var task1 = org1.WorkflowItems.Tasks.AddNew();

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "USORGSYD";
			org2.OH_FullName = "Lowtech Software";
			var task2 = org2.WorkflowItems.Tasks.AddNew();

			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var task3 = enquiry.WorkflowItems.Tasks.AddNew();

			Factory.Save();

			var filterBizo = new ProcessTaskFilterBusinessObject();
			var filter = (ParentJobModuleFilter)filterBizo["Parent Job"];

			filter.IsActive = true;
			filter.SelectedModule = "OrgHeader";
			filter.Property = org1.PK;

			var result = Factory.Load<OrgHeaderProcessTask>(filterBizo.Filter);

			AssertEquals(1, result.Length);
			AssertEquals(task1.PK, result.Single().PK);

			filter.SelectedModule = ModuleIDs.SalesEnquiry.Name;
			filter.Property = enquiry.PK;

			var result2 = Factory.Load<SalesEnquiryProcessTask>(filterBizo.Filter);

			AssertEquals(1, result2.Length);
			AssertEquals(task3.PK, result2.Single().PK);
		}

		public void TestParentJobModuleFilter_ProductivityWiseModeEnabled()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;

			var filter = new ParentJobModuleFilter("Moo. Mooooo.", ProcessTasksSchema.P9_ParentID, Factory);
			var list = filter.ModuleOptions.Cast<ICodeDescription>().Select(m => m.Code).ToArray();

			AssertContainsExactElementsInAnyOrder(new[]
			{
				ModuleIDs.AccCollectionBatch.Name,
				ModuleIDs.AccCollectionOrder.Name,
				ModuleIDs.AccPayableOrder.Name,
				ModuleIDs.APTransaction.Name,
				ModuleIDs.ARTransaction.Name,
				ModuleIDs.Communication.Name,
				ModuleIDs.CustomerServiceTicket.Name,
				ModuleIDs.GlbCompanyCampaign.Name,
				ModuleIDs.HRGlbCompanyCampaign.Name,
				ModuleIDs.GlbGroup.Name,
				ModuleIDs.GlbStaff.Name,
				ModuleIDs.GlbStaffChangeRequest.Name,
				ModuleIDs.GlbStaffHoliday.Name,
				ModuleIDs.HRHiringRequest.Name,
				ModuleIDs.HRJobApplication.Name,
				ModuleIDs.HRJobOpenings.Name,
				ModuleIDs.HROnBoarding.Name,
				ModuleIDs.GlbAccreditationAttempt.Name,
				ModuleIDs.Opportunity.Name,
				ModuleIDs.Organisation.Name,
				ModuleIDs.Project.Name,
				ModuleIDs.SalesEnquiry.Name,
				ModuleIDs.WorkItem.Name,
			}, list);
		}

		public void TestParentJobFilter_ShouldHaveWorkflowProviderModuleOptions()
		{
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			{
				var filter = new ParentJobModuleFilter("Parent Job", ProcessTasksSchema.P9_ParentID, Factory);

				AssertEquals(true, filter.ModuleOptions.Count > 30);

				AssertEquals(filter.ModuleOptions.GetAllCodes().Distinct().Count(), filter.ModuleOptions.Count);

				var allowedModules = ParentJobModuleFilter.GetWorkflowProviderModules(Factory).ToArray();

				foreach (var code in filter.ModuleOptions.GetAllCodes())
				{
					var id = ModuleIDs.AllIncludingClientModules.First(x => x.Name == code);

					AssertNotNull(id);
					AssertCollectionContains($"Each module option must be workflow enabled, and yet {code} is not.", id, allowedModules);
				}
			}
		}

		public void TestConstructor_ShouldNotLoadModuleList()
		{
			var factory = new BusinessObjectFactory();
			var filter = new ParentJobModuleFilter("Parent Job", ProcessTasksSchema.P9_ParentID, factory);
			var cachedValue = factory.GetCachedValue(ParentJobModuleFilter.WorkflowProviderModulesCacheKey, () => System.Array.Empty<ModuleIdentifier>().AsEnumerable());
			AssertEquals("The modules should not be cached yet, so the provided func should be executed instead, and yet...", 0, cachedValue.Count());

			factory = new BusinessObjectFactory();
			filter = new ParentJobModuleFilter("Parent Job", ProcessTasksSchema.P9_ParentID, factory);
			filter.SelectedModule = ModuleIDs.ProcessTasks.Name;
			cachedValue = factory.GetCachedValue(ParentJobModuleFilter.WorkflowProviderModulesCacheKey, () => System.Array.Empty<ModuleIdentifier>().AsEnumerable());
			AssertNotEquals("The modules should now be cached because setting the module would have validated against that list, and yet...", 0, cachedValue.Count());
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override ParentJobModuleFilter GetNewModuleFilter()
		{
			return new ParentJobModuleFilter("moo", ProcessTasksSchema.P9_ParentID, Factory);
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		protected override Dictionary<string, IZType> GetDummyValuesForCacheInvalidationTest(ParentJobModuleFilter filter)
		{
			var values = base.GetDummyValuesForCacheInvalidationTest(filter);
			values.Add(nameof(filter.SelectedModule), new ZString(ModuleIDs.SalesEnquiry.Name));

			return values;
		}
	}
}
