using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(InvoicingOperationalActionsSupporter))]
	public class InvoicingOperationalActionsSupporterTest : WhsOperationalActionSupporterTest<InvoicingOperationalActionsSupporter>
	{
		#region TestCheckpoint

		public void TestCheckpoint()
		{
			AssertEquals("Checkpoint", Env.Security.WhsInvoicing, Supporter.BaseCheckpoint);
		}

		#endregion

		#region TestBusinessContext

		public void TestBusinessContext()
		{
			AssertEquals("BusinessContext", BusinessContext.WhsPeriodicBilling, Supporter.BusinessContext);
		}

		#endregion

		#region Implementation

		protected override ModuleIdentifier ModuleID => ModuleIDs.WhsInvoicing;

		#endregion
	}
}
