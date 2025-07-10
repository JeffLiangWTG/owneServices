using NUnit.Framework;

namespace Enterprise.Customs.Module.OperationalActions.Testing
{
	[TestedType(typeof(DeclarationOperationalActionMethodProvider))]
	sealed class DeclarationOperationalActionMethodProviderTest : Services.OperationalActions.Support.Testing.OperationalActionMethodProviderTest
	{
		protected override Services.OperationalActions.Support.ActionMethodProviderID ID => Services.OperationalActions.Support.ActionMethodProviderIDs.JobDeclaration;
	}
}
