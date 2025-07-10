namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class SealProviderTest : Customs.Business.Testing.DataProviderTestCase<SealProvider>
{
	public void TestConstructor()
	{
		AssertNoExceptionThrown(() => new SealProvider(1, null));
	}

	public void TestSequenceNumber() => AssertEquals("99", Provider.SequenceNumber);

	public void TestIdentifier() => AssertEquals("ABC", Provider.Identifier);

	protected override SealProvider GetProvider() => new SealProvider(99, "ABC");
}
