using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(ContainerMovementProcessTask))]
	internal class ContainerMovementProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestParentControllerID()
		{
			var task = Factory.New<ContainerMovementProcessTask>();
			AssertEquals(ControllerIDs.AgencyContainerManager, task.ParentControllerID);
		}

		public void TestParent()
		{
			var movement = Factory.New<ContainerMovement>();
			var task = (ContainerMovementProcessTask)((IWorkflowProvider)movement).WorkflowItems.AddNew();
			AssertEquals(movement, task.Parent);
		}

		#region Implementation
		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var movement = Factory.New<ContainerMovement>();
			return ((IWorkflowProvider)movement).WorkflowItems.AddNew();
		}
		#endregion
	}
}
