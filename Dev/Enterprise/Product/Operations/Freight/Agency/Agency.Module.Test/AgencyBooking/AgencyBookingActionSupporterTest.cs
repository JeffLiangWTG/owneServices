using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(AgencyBookingActionSupporter))]
	internal class AgencyBookingActionSupporterTest : AgencyShipmentActionSupporterTest<AgencyBooking, AgencyBookingActionSupporter>
	{
		#region TestInvoicingCheckpoint
		public void TestInvoicingCheckpoint()
		{
			AssertEquals("InvoicingCheckpoint", Env.Security.AgencyBookingJobInvoicing, ((IInvoicingSecurityCheckpointProvider)Supporter).InvoicingCheckpoint);
		}

		#endregion
		#region TestActionMethodGroups
		public void TestActionMethodGroups()
		{
			var expectedIDs = new[] { ActionMethodProviderIDs.General, ActionMethodProviderIDs.Accounting, ActionMethodProviderIDs.Shipping };
			AssertContainsExactElementsInAnyOrder("These providers should be contributing action methods", p => p.Name, expectedIDs, Supporter.Methods.GetAllIds());
		}

		#endregion
		#region Implementation
		protected override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.AgencyBooking;
			}
		}
		#endregion
	}
}
