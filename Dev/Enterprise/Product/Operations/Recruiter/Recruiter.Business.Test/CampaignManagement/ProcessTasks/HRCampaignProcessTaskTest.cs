using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRCampaignProcessTasks))]
	sealed class HRCampaignProcessTaskTest : ProcessTaskTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var campaign = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			return ((IWorkflowProvider)campaign).WorkflowItems
				.AddNew();
		}

		public void TestGetTypeForLoad()
		{
			var campaign = (GlbCompanyCampaign)Factory.New<HRGlbCompanyCampaign>();
			var strategy = new GlbCompanyCampaignProcessTaskLoadStrategy();
			AssertEquals(typeof(HRCampaignProcessTasks), strategy.GetTypeForLoad(campaign.TablePrefix, campaign.PK, Factory));

			campaign = Factory.New<GlbCompanyCampaign>();
			AssertEquals(typeof(CRMCampaignProcessTasks), strategy.GetTypeForLoad(campaign.TablePrefix, campaign.PK, Factory));
		}

		public void TestGetTypeForLoadWhenParentNotInCache()
		{
			var hrCampaign = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();

			var crmCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			Factory.Save();

			var strategy = new GlbCompanyCampaignProcessTaskLoadStrategy();

			var factory2 = new BusinessObjectFactory();
			AssertEquals(typeof(HRCampaignProcessTasks), strategy.GetTypeForLoad(hrCampaign.TablePrefix, hrCampaign.PK, factory2));
			AssertEquals(typeof(CRMCampaignProcessTasks), strategy.GetTypeForLoad(crmCampaign.TablePrefix, crmCampaign.PK, factory2));
		}

		public void TestAddAdditionalParentFilters()
		{
			var crmCampaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var crmCampaignProcessTask = ((IWorkflowProvider)crmCampaign).WorkflowItems.AddNew();

			var hrCampaign = Factory.NewWithValidTestData<HRGlbCompanyCampaign>();
			var hrCampaignProcessTask = ((IWorkflowProvider)hrCampaign).WorkflowItems.AddNew();

			Factory.Save();

			var tasks = Factory.Load<ProcessTask>(GetNewQueryForTestAddAdditionalParentFilters(WorkflowDescriptors.CampaignWorkflowDescriptorCode));
			AssertEquals(1, tasks.Length);
			AssertCollectionContains(crmCampaignProcessTask, tasks);

			tasks = Factory.Load<ProcessTask>(GetNewQueryForTestAddAdditionalParentFilters(WorkflowDescriptors.HRCampaignWorkflowDescriptorCode));
			AssertEquals(1, tasks.Length);
			AssertCollectionContains(hrCampaignProcessTask, tasks);
		}

		ZDBOnlyQuery GetNewQueryForTestAddAdditionalParentFilters(string workflowTypeCode)
		{
			var descriptor = WorkflowDescriptors.Instance.TryGetValueSafe(workflowTypeCode);

			var query = new ZDBOnlyQuery(typeof(ProcessTask));
			var subQuery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaign), ProcessTasksSchema.P9_ParentID);
			var strategy = new GlbCompanyCampaignProcessTaskLoadStrategy();
			strategy.AddAdditionalParentFilters(descriptor, subQuery);
			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}
	}
}
