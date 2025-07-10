using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusGoodsCatalogProcessTask))]
	sealed class CusGoodsCatalogProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParent()
		{
			var cusGoodsCatalog = Factory.New<BaseCusGoodsCatalog>();
			var task = (CusGoodsCatalogProcessTask)((IWorkflowProvider)cusGoodsCatalog).WorkflowItems.AddNew();
			AssertEquals(cusGoodsCatalog, task.Parent);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var cusGoodsCatalog = Factory.New<BaseCusGoodsCatalog>();
			return ((IWorkflowProvider)cusGoodsCatalog).WorkflowItems.AddNew();
		}
	}
}
