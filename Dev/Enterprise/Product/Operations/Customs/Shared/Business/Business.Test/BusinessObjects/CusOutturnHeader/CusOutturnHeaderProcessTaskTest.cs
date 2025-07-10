using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusOutturnHeaderProcessTask))]
	sealed class CusOutturnHeaderProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParent()
		{
			CusOutturnHeader cusOutturnHeader = Factory.New<CusOutturnHeader>();
			CusOutturnHeaderProcessTask task = (CusOutturnHeaderProcessTask)((IWorkflowProvider)cusOutturnHeader).WorkflowItems.AddNew();
			AssertEquals(cusOutturnHeader, task.Parent);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			CusOutturnHeader cusOutturnHeader = Factory.New<CusOutturnHeader>();
			return ((IWorkflowProvider)cusOutturnHeader).WorkflowItems.AddNew();
		}
	}
}
