using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.GUI.Consol;
using Enterprise.Freight.Forwarding.GUI.Consol.UserControls;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ForwardingConsolWorkflowUserControlTest : TestCaseWithFactory
	{
		public void TestGetStmALogFilterStripBusinessObjectOverriddenCorrectly()
		{
			using (EnvProxy.Instance.SetTemporaryUserContext(User.UnKnownUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (var workflowUserControl = new TestForwardingConsolWorkflowUserControl())
			{
				AssertEquals(
					"The GetStmALogFilterStripBusinessObject of workflowUserControl should return a type of ForwardingConsolLogFilterBusinessObject  when user is not CWSupport."
					, workflowUserControl.GetStmALogFilterStripBusinessObjectExposed.Invoke(Factory.New<DummyEnterpriseBusinessObject>()).GetType().Name
					, nameof(ForwardingConsolLogFilterBusinessObject)
					);
			}

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (var workflowUserControl = new TestForwardingConsolWorkflowUserControl())
			{
				AssertEquals(
					"The GetStmALogFilterStripBusinessObject of workflowUserControl should return a type of ZStmALogFilterBusinessObject  when user is CWSupport."
					, workflowUserControl.GetStmALogFilterStripBusinessObjectExposed.Invoke(Factory.New<DummyEnterpriseBusinessObject>()).GetType().Name
					, nameof(ZStmALogFilterBusinessObject)
					);
			}
		}
	}

	#region Implementation

	class TestForwardingConsolWorkflowUserControl : ForwardingConsolWorkflowUserControl
	{
		public GetStmALogFilterStripBusinessObject GetStmALogFilterStripBusinessObjectExposed => GetStmALogFilterStripBusinessObject;
	}

	#endregion
}
