using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Module.Testing
{
	[TestedType(typeof(DtbConsignmentOperationalActionSupporter))]
	internal class DtbConsignmentOperationalActionSupporterTest : OperationalActionSupporterTest<DtbConsignmentOperationalActionSupporter>
	{
		public void TestActionMethodGroups()
		{
			AssertContainsExactElementsInAnyOrder("These providers should be contributing action methods.",
				p => p.Name,
				new[] { ActionMethodProviderIDs.General, ActionMethodProviderIDs.Accounting },
				Supporter.Methods.GetAllIds());
		}

		public void TestSingularElementNoun()
		{
			AssertEquals("Consignment", Supporter.SingularElementNoun);
		}

		public void TestPluralElementNoun()
		{
			AssertEquals("Consignments", Supporter.PluralElementNoun);
		}

		public void TestIInvoicingSecurityCheckpointProvider()
		{
			AssertEquals("InvoicingCheckpoint", Env.Security.DtbConsignmentJobInvoicing, ((IInvoicingSecurityCheckpointProvider)Supporter).InvoicingCheckpoint);
		}

		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.DtbConsignment; }
		}
	}
}
