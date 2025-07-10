using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(BillOfLadingActionSupporter))]
	internal class BillOfLadingActionSupporterTest : AgencyShipmentActionSupporterTest<BillOfLading, BillOfLadingActionSupporter>
	{
		public void TestInvoicingCheckpoint()
		{
			AssertEquals("InvoicingCheckpoint", Env.Security.AgencyBillOfLadingJobInvoicing, ((IInvoicingSecurityCheckpointProvider)Supporter).InvoicingCheckpoint);
		}

		public void TestActionMethodGroups()
		{
			var expectedIDs = new[] { ActionMethodProviderIDs.General, ActionMethodProviderIDs.Accounting, ActionMethodProviderIDs.Shipping, };
			AssertContainsExactElementsInAnyOrder("These providers should be contributing action methods", p => p.Name, expectedIDs, Supporter.Methods.GetAllIds());
		}

		#region Implementation
		protected override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.AgencyBillOfLading;
			}
		}
		#endregion
	}
}
