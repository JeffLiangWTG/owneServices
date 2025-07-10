using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusSCAOceanBillProcessTask))]
	sealed class CusSCAOceanBillProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParent()
		{
			var oceanBill = Factory.New<TestCusSCAOceanBill>();
			var task = (CusSCAOceanBillProcessTask)((IWorkflowProvider)oceanBill).WorkflowItems.AddNew();
			AssertEquals(oceanBill, task.Parent);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var oceanBill = Factory.New<TestCusSCAOceanBill>();
			return ((IWorkflowProvider)oceanBill).WorkflowItems.AddNew();
		}
	}
}
