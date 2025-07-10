using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(StatementOperationalActionSupporter))]
	sealed class StatementOperationalActionSupporterTest : OperationalActionSupporterTest<StatementOperationalActionSupporter>
	{
		public void TestPopulateMethods()
		{
			var supporter = new StatementOperationalActionSupporter();
			var allIds = supporter.Methods.GetAllIds();
			AssertCollectionContains(ActionMethodProviderIDs.USStatement, allIds);
		}

		protected override ModuleIdentifier ModuleID => ModuleIDs.Customs.US.USCustomsStatement;
	}
}
