using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(ForwardingShipmentSupporter))]
	internal class ForwardingShipmentSupporterTest : OperationalActionSupporterTest<ForwardingShipmentSupporter>
	{
		#region TestRootType

		public void TestRootType()
		{
			AssertEquals("ForwardingShipment", Supporter.RootType.Name);
		}

		#endregion

		#region TestInvoicingCheckpoint

		public void TestInvoicingCheckpoint()
		{
			AssertEquals("InvoicingCheckpoint", Env.Security.MaintainShipmentJobInvoicing, ((IInvoicingSecurityCheckpointProvider)Supporter).InvoicingCheckpoint);
		}

		#endregion

		#region TestActionMethodGroups

		public void TestActionMethodGroups()
		{
			AssertContainsExactElementsInAnyOrder("These providers should be contributing action methods",
				p => p.Name,
				new[] { ActionMethodProviderIDs.General, ActionMethodProviderIDs.Shipment, ActionMethodProviderIDs.Accounting, ActionMethodProviderIDs.HVLV, ActionMethodProviderIDs.Brokerage, ActionMethodProviderIDs.SendAdvancedAirCargoReport, ActionMethodProviderIDs.DtbBookingParent },
				Supporter.Methods.GetAllIds());
		}

		#endregion

		#region Implementation

		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.JobShipment; }
		}

		#endregion
	}
}
