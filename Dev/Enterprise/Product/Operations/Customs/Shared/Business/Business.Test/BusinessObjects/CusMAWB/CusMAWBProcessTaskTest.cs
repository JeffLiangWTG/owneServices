using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusMAWBProcessTask))]
	sealed class CusMAWBProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParent()
		{
			var cusMAWB = Factory.New<CusMAWB>();
			var task = (CusMAWBProcessTask)((IWorkflowProvider)cusMAWB).WorkflowItems.AddNew();
			AssertEquals(cusMAWB, task.Parent);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var cusMAWB = Factory.New<CusMAWB>();
			cusMAWB.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			return ((IWorkflowProvider)cusMAWB).WorkflowItems.AddNew();
		}
	}
}
