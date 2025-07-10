using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class RoutingSupportWorkflowTriggerValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidatePortPair()
		{
			RoutingSupportProcessTask trigger = (RoutingSupportProcessTask)WorkflowProvider.WorkflowItems.Triggers.AddNew();
			Consol.JK_RL_NKLoadPort = "AUSYD";
			Consol.JK_RL_NKDischargePort = "AUSYD";
			trigger.PortPair = "AUSYD->MYPKG";
			AssertNoErrors("No errors for a valid port pair that has a corresponding transport", trigger.ReferenceCodeInfo);

			trigger.PortPair = "UNKNO->NPAIR";
			AssertEquals("UNKNO->NPAIR", trigger.PortPair);
			trigger.Validation.ValidateReferenceCode();
			AssertHasErrors("Error on a port pair that doesn't have a corresponding transport", trigger.ReferenceCodeInfo);
		}

		#region Implementation

		IWorkflowProvider WorkflowProvider
		{
			get { return (IWorkflowProvider)Consol; }
		}

		CommonConsol Consol
		{
			get { return consol ?? (consol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>()); }
		}
		CommonConsol consol;

		#endregion
	}
}
