using NUnit.Framework;

namespace Enterprise.Customs.Module.OperationalActions.Testing
{
	[TestedType(typeof(BrokerageOperationalActionMethodProvider))]
	sealed class BrokerageOperationalActionMethodProviderTest : Services.OperationalActions.Support.Testing.OperationalActionMethodProviderTest
	{
		protected override Services.OperationalActions.Support.ActionMethodProviderID ID => Services.OperationalActions.Support.ActionMethodProviderIDs.Brokerage;
	}
}
