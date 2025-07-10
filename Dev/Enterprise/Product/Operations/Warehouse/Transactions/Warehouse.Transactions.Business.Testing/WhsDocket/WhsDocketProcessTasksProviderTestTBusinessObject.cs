using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsDocketProcessTasksProviderTest<TBusinessObject> : WorkflowProviderTest<TBusinessObject, ProcessTaskCollection>
	where TBusinessObject : WhsDocket
	{
		#region Overrides

		protected override ZString ExpectedWorkflowType => GetExpectedWorkflowType();

		protected override TBusinessObject GetNewBusinessObject(BusinessObjectFactory factory) => GetNewDocket();

		public override void TestEventDatePropertyAttribute_AppliedCorrectlyToProperties()
		{
			Assert("No milestones on Warehouse Jobs", true);
		}

		#endregion

		#region  TestGetTemplateFilterCriteria

		public void TestGetTemplateFilterCriteria_Client()
		{
			AssertGetTemplateFilterCriteria(Docket.WD_OH_ClientInfo, ProcessTaskTemplate.P0_OH_ClientInfo, Client.PK, Client2.PK, ZGuid.Empty);
		}

		public void TestGetTemplateFilterCriteria_Warehouse()
		{
			var warehouse2 = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse2.RelatedCompanyBranch.GB_GC = Env.CurrentCompanyPK;
			Docket.Factory.Save();

			AssertGetTemplateFilterCriteria(Docket.WD_WW_WhsInfo, ProcessTaskTemplate.P0_WWInfo, Warehouse.PK, warehouse2.PK, ZGuid.Empty);
		}

		public void TestGetTemplateFilterCriteria_WarehouseCompany()
		{
			var warehouse2 = Factory.NewWithValidTestData<WhsWarehouse>();
			Docket.Factory.Save();

			var setter = new SetBusinessObjectPropertyValueDelegate<ZGuid>(
				(ZGuid value) =>
				{
					Docket.Warehouse.RelatedCompanyBranch.GB_GCInfo.Value = value;
				}
			);
			AssertGetTemplateFilterCriteria(setter, ProcessTaskTemplate.P0_GCInfo, Warehouse.RelatedCompanyBranch.GB_GC, warehouse2.RelatedCompanyBranch.GB_GC, ZGuid.Empty);
		}

		public void TestUseWarehouseCompanyProcessTaskTemplatesInsteadOfCurrentCompanyTemplates()
		{
			var globalTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			globalTemplate.P0_Name = "GlobalTemplate";
			globalTemplate.P0_ProcessType = ExpectedWorkflowType;
			globalTemplate.P0_GC = ZGuid.Empty;

			var activeTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			activeTemplate.P0_Name = "ActiveTemplateInCurrentCompany";
			activeTemplate.P0_ProcessType = ExpectedWorkflowType;
			activeTemplate.P0_GC = Env.CurrentCompanyPK;

			var activeTemplateWithWhs = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			activeTemplateWithWhs.P0_Name = "ActiveTemplateWithWhsInCurrentCompany";
			activeTemplateWithWhs.P0_ProcessType = ExpectedWorkflowType;
			activeTemplateWithWhs.P0_WW = Docket.WD_WW_Whs;
			activeTemplateWithWhs.P0_GC = Env.CurrentCompanyPK;

			var company = Factory.NewWithValidTestData<GlbCompany>();
			Docket.Warehouse.RelatedCompanyBranch.GB_GC = company.PK;

			var activeTemplate2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			activeTemplate2.P0_Name = "ActiveTemplateInWhsCompany";
			activeTemplate2.P0_ProcessType = ExpectedWorkflowType;
			activeTemplate2.P0_GC = company.PK;

			var whsTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			whsTemplate.P0_Name = "WhsTemplate";
			whsTemplate.P0_ProcessType = ExpectedWorkflowType;
			whsTemplate.P0_WW = Docket.WD_WW_Whs;
			whsTemplate.P0_GC = company.PK;

			Factory.Save();

			AssertFindTemplates(Docket, whsTemplate, activeTemplate2, globalTemplate);
		}

		void AssertFindTemplates(IWorkflowProviderCore workflowProvider, params ProcessTaskTemplate[] expectedTemplates)
		{
			ProcessTaskTemplate[] templates = new ProcessTaskTemplate.Loader(Factory).FindMatches(workflowProvider);
			AssertEquals("Should be correct number of found templates", expectedTemplates.Length, templates.Length);
			for (int i = 0; i < expectedTemplates.Length; i++)
			{
				AssertEquals("Should be proper template in proper order", templates[i], expectedTemplates[i]);
			}
		}

		#endregion

		#region Implementation

		WhsDocket Docket => BusinessObject;

		protected WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		protected WhsWarehouse Warehouse => warehouse ?? (warehouse = GetNewWarehouse());
		WhsWarehouse warehouse;

		protected virtual WhsWarehouse GetNewWarehouse()
		{
			var warehouse = Helper.CreateWarehouse("TST");
			warehouse.RelatedCompanyBranch.GB_GC = Env.CurrentCompanyPK;
			return warehouse;
		}

		protected abstract ZString GetExpectedWorkflowType();
		protected abstract TBusinessObject GetNewDocket();

		protected ZShort NewRefNumber = 0;

		#endregion
	}
}
