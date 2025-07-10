using System;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AESSealProviderTest : DataProviderTestCase<AESSealProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null Identifier", "Value cannot be null.\r\nParameter name: identifier",
			() => new AESSealProvider(1, null));
	}

	public void TestSequenceNumber()
	{
		AssertEquals(1, Provider.SequenceNumber);
	}

	public void TestIdentifier()
	{
		AssertEquals("ABCD", Provider.Identifier);
	}

	protected override AESSealProvider GetProvider() => new AESSealProvider(1, "ABCD");
}
