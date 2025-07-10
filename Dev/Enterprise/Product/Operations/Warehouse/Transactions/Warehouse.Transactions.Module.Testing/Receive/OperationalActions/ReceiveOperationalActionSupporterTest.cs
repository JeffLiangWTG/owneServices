using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(ReceiveOperationalActionSupporter))]
	public class ReceiveOperationalActionSupporterTest : WhsOperationalActionSupporterTest<ReceiveOperationalActionSupporter>
	{
		#region Implementation

		protected override ModuleIdentifier ModuleID => ModuleIDs.WhsReceive;

		#endregion

		#region TestSingularElementNoun

		public void TestSingularElementNoun()
		{
			AssertEquals("Receive", Supporter.SingularElementNoun);
		}

		#endregion

		#region TestPluralElementNoun

		public void TestPluralElementNoun()
		{
			AssertEquals("Receives", Supporter.PluralElementNoun);
		}

		#endregion

		#region TestInvoicingCheckpoint

		public void TestInvoicingCheckpoint()
		{
			AssertEquals("InvoicingCheckpoint", Env.Security.WhsReceiveJobInvoicing, ((IInvoicingSecurityCheckpointProvider)Supporter).InvoicingCheckpoint);
		}

		#endregion

		#region TestActionMethodGroups

		public void TestActionMethodGroups()
		{
			AssertCollectionContains(ActionMethodProviderIDs.Accounting, Supporter.Methods.GetAllIds());
		}

		#endregion
	}
}
