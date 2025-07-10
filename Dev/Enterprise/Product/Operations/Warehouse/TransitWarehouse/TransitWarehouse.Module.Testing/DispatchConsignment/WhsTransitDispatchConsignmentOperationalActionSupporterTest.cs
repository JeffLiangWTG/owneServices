using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Module.Testing
{
	[TestedType(typeof(WhsTransitDispatchConsignmentOperationalActionSupporter))]
	public class WhsTransitDispatchConsignmentOperationalActionSupporterTest : OperationalActionSupporterTest<WhsTransitDispatchConsignmentOperationalActionSupporter>
	{
		public void TestMethods()
		{
			AssertContainsExactElementsInAnyOrder("Action Methods",
				(i) => i.Name,
				new ActionMethodProviderID[] { ActionMethodProviderIDs.General, ActionMethodProviderIDs.Accounting },
				Supporter.Methods.GetAllIds());
		}

		public void TestSingularElementNoun()
		{
			AssertEquals("Dispatch Consignment", Supporter.SingularElementNoun);
		}

		public void TestPluralElementNoun()
		{
			AssertEquals("Dispatch Consignments", Supporter.PluralElementNoun);
		}

		public void TestInvoicingCheckpoint()
		{
			AssertEquals("InvoicingCheckpoint", Env.Security.WhsItemDispatchConsignmentJobInvoicing, ((IInvoicingSecurityCheckpointProvider)Supporter).InvoicingCheckpoint);
		}

		#region Implementation

		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.WhsTransitDispatchConsignment; }
		}

		#endregion
	}
}
