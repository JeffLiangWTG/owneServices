using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefComplianceListProcessTask))]
	public class RefComplianceListProcessTaskTest : ProcessTaskTest
	{
		public void TestProcessTask()
		{
			var complianceList = Factory.NewWithValidTestData<RefComplianceList>();
			var processTask = ((RefComplianceListProcessTaskCollection)complianceList.WorkflowItems).AddNew();
			AssertEquals(complianceList, processTask.Parent);
			AssertEquals(ControllerIDs.RefComplianceList, processTask.ParentControllerID);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var loadList = Factory.New<RefComplianceList>();
			return loadList.WorkflowItems.AddNew();
		}
	}
}
