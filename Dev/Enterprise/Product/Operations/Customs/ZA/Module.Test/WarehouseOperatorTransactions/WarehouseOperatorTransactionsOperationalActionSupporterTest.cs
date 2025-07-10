using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(WarehouseOperatorTransactionsOperationalActionSupporter))]
	class WarehouseOperatorTransactionsOperationalActionSupporterTest : OperationalActionSupporterTest<WarehouseOperatorTransactionsOperationalActionSupporter>
	{
		public override bool ShouldSupportDocuments => false;

		protected override ModuleIdentifier ModuleID => ZAModuleIDs.WarehouseOperatorTransactions;
	}
}
