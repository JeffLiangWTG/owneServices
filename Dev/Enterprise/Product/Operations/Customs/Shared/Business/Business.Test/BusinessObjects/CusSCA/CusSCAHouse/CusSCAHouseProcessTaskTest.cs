using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusSCAHouseProcessTask))]
	sealed class CusSCAHouseProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParent()
		{
			var oceanBill = Factory.New<TestCusSCAOceanBill>();
			oceanBill.CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.BaseTesting;
			var cusSCAHouse = Factory.New<TestCusSCAHouse>();
			cusSCAHouse.CA_CB = oceanBill.PK;
			var task = (CusSCAHouseProcessTask)((IWorkflowProvider)cusSCAHouse).WorkflowItems.AddNew();
			AssertEquals(cusSCAHouse, task.Parent);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var cusSCAHouse = Factory.New<TestCusSCAHouse>();
			return ((IWorkflowProvider)cusSCAHouse).WorkflowItems.AddNew();
		}
	}
}
