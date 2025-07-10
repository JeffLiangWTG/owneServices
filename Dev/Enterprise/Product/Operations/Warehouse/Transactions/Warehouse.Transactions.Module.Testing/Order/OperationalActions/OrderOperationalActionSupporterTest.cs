using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(OrderOperationalActionSupporter))]
	public class OrderOperationalActionSupporterTest : WhsOperationalActionSupporterTest<OrderOperationalActionSupporter>
	{
		#region Implementation

		protected override ModuleIdentifier ModuleID => ModuleIDs.WhsOrder;

		#endregion

		#region TestSingularElementNoun

		public void TestSingularElementNoun()
		{
			AssertEquals("Order", Supporter.SingularElementNoun);
		}

		#endregion

		#region TestPluralElementNoun

		public void TestPluralElementNoun()
		{
			AssertEquals("Orders", Supporter.PluralElementNoun);
		}

		#endregion

		#region TestInvoicingCheckpoint

		public void TestInvoicingCheckpoint()
		{
			AssertEquals("InvoicingCheckpoint", Env.Security.WhsOrderJobInvoicing, ((IInvoicingSecurityCheckpointProvider)Supporter).InvoicingCheckpoint);
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
