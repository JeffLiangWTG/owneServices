using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Module.Testing
{
	[TestedType(typeof(CartageOperationalActionSupporter))]
	internal class CartageOperationalActionSupporterTest : OperationalActionSupporterTest<CartageOperationalActionSupporter>
	{
		public void TestInvoicingCheckpoint()
		{
			AssertEquals("InvoicingCheckpoint", Env.Security.TransportJobInvoicing, ((IInvoicingSecurityCheckpointProvider)Supporter).InvoicingCheckpoint);
		}

		public void TestActionMethodGroups()
		{
			AssertContainsExactElementsInAnyOrder("These providers should be contributing action methods", p => p.Name, new[] { ActionMethodProviderIDs.General, ActionMethodProviderIDs.Accounting }, Supporter.Methods.GetAllIds());
		}

		public void TestSingularElementNoun()
		{
			AssertEquals("Transport Job", Supporter.SingularElementNoun);
		}

		public void TestPluralElementNoun()
		{
			AssertEquals("Transport Jobs", Supporter.PluralElementNoun);
		}

		protected override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.Cartage;
			}
		}
	}
}
