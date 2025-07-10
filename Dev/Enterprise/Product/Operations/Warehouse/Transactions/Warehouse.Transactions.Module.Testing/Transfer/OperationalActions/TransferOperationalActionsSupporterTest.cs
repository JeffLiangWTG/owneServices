using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(TransferOperationalActionsSupporter))]
	class TransferOperationalActionsSupporterTest : OperationalActionSupporterTest<TransferOperationalActionsSupporter>
	{
		protected override ModuleIdentifier ModuleID => ModuleIDs.WhsTransfer;
	}
}
