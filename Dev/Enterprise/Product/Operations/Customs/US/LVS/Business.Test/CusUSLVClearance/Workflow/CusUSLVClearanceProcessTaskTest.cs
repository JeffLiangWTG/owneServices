using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	[TestedType(typeof(CusUSLVClearanceProcessTask))]
	public class CusUSLVClearanceProcessTaskTest : ProcessTaskTest
	{
		public void TestParentControllerID()
		{
			var processTask = Factory.New<CusUSLVClearanceProcessTask>();
			AssertEquals(ControllerIDs.JobShipment, processTask.ParentControllerID);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var shipment = Factory.New<CusUSLVClearance>() as IWorkflowProvider;
			return shipment.WorkflowItems.AddNew();
		}

		#endregion
	}
}
