using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Module.Testing
{
	[TestedType(typeof(DtbBookingConsignmentOperationalActionSupporter))]
	internal class DtbBookingConsignmentOperationalActionSupporterTest : OperationalActionSupporterTest<DtbBookingConsignmentOperationalActionSupporter>
	{
		#region TestActionMethodGroups

		public void TestActionMethodGroups()
		{
			AssertContainsExactElementsInAnyOrder("These providers should be contributing action methods.",
				p => p.Name,
				new[] { ActionMethodProviderIDs.General, ActionMethodProviderIDs.Accounting },
				Supporter.Methods.GetAllIds());
		}

		#endregion

		#region TestSingularElementNoun

		public void TestSingularElementNoun()
		{
			AssertEquals("Consignment", Supporter.SingularElementNoun);
		}

		#endregion

		#region TestPluralElementNoun

		public void TestPluralElementNoun()
		{
			AssertEquals("Consignments", Supporter.PluralElementNoun);
		}

		#endregion

		#region IInvoicingSecurityCheckpointProvider Members

		public void TestIInvoicingSecurityCheckpointProvider()
		{
			AssertEquals("InvoicingCheckpoint", Env.Security.None, ((IInvoicingSecurityCheckpointProvider)Supporter).InvoicingCheckpoint);
		}

		#endregion

		#region Implementation

		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.DtbBookingConsignment; }
		}

		#endregion
	}
}
