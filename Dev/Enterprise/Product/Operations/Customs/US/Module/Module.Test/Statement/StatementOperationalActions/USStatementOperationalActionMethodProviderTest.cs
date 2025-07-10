using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USStatementOperationalActionMethodProvider))]
	sealed class USStatementOperationalActionMethodProviderTest : Services.OperationalActions.Support.Testing.OperationalActionMethodProviderTest
	{
		protected override Services.OperationalActions.Support.ActionMethodProviderID ID => Services.OperationalActions.Support.ActionMethodProviderIDs.USStatement;
	}
}
