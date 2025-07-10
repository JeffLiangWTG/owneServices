using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ForwardingShipmentWorkflowUserControlTest : TestCaseWithFactory
	{
		public void TestGetStmALogFilterStripBusinessObjectOverriddenCorrectly()
		{
			using (var workflowUserControl = new TestForwardingShipmentWorkflowUserControl())
			{
				AssertEquals(
					"The GetStmALogFilterStripBusinessObject of workflowUserControl should always return a type of ForwardingShipmentLogFilterBusinessObject."
					, workflowUserControl.GetStmALogFilterStripBusinessObjectExposed.Invoke(Factory.New<DummyEnterpriseBusinessObject>()).GetType().Name
					, nameof(ForwardingShipmentLogFilterBusinessObject)
					);
			}
		}
	}

	#region Implementation

	class TestForwardingShipmentWorkflowUserControl : ForwardingShipmentWorkflowUserControl
	{
		public GetStmALogFilterStripBusinessObject GetStmALogFilterStripBusinessObjectExposed => GetStmALogFilterStripBusinessObject;
	}

	#endregion
}
