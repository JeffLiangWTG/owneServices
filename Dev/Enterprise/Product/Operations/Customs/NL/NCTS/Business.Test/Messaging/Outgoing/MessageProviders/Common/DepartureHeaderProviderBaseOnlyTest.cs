using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(DepartureHeaderProvider))]
sealed class DepartureHeaderProviderBaseOnlyTest : DepartureHeaderProviderAbstractTest<DepartureHeaderProvider>
{
	protected override string MessageType => string.Empty;
}
