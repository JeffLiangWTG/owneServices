using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseCusGoodsCatalog))]
	public class CusGoodsCatalogWorkflowProviderTest : WorkflowProviderTest<BaseCusGoodsCatalog, ProcessTaskCollection<CusGoodsCatalogProcessTask, BaseCusGoodsCatalog>>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.CusGoodsCatalogWorkflowDescriptorCode;

		public override void TestProcessTasksCreatedOnSave()
		{
			var cusGoodsCatalog = GetNewBusinessObject(Factory);
			if (cusGoodsCatalog.SupportsWorkflow)
			{
				base.TestProcessTasksCreatedOnSave();
			}
			else
			{
				Assert(true);
			}
		}

		public void TestWorkflows()
		{
			var cusGoodsCatalog = GetNewBusinessObject(Factory);
			AssertEquals("Enterprise.BufferManagement.Business.ProcessHeaderCollection", cusGoodsCatalog.Workflows.GetType().FullName);
		}

		public void TestWorkflowItems()
		{
			var cusGoodsCatalog = GetNewBusinessObject(Factory);
			AssertType<ProcessTaskCollection<CusGoodsCatalogProcessTask, BaseCusGoodsCatalog>>(cusGoodsCatalog.WorkflowItems);
			var workflowItems = cusGoodsCatalog.WorkflowItems.AddNew();
			cusGoodsCatalog.Delete();
			Assert("WorkflowItems should be deleted", workflowItems.IsDeleted);
		}

		public virtual void TestGetWorkflowInformationProvider()
		{
			var cusGoodsCatalog = GetNewBusinessObject(Factory);
			AssertNull(cusGoodsCatalog.GetWorkflowInformationProvider());
		}

		public virtual void TestGetTemplateSelectionCriteria()
		{
			var cusGoodsCatalog = GetNewBusinessObject(Factory);
			AssertType<ColumnValueRanker>(cusGoodsCatalog.GetTemplateSelectionCriteria());
		}
	}
}
