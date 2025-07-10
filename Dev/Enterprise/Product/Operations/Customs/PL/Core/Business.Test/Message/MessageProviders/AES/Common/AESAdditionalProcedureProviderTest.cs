namespace Enterprise.Customs.PL.Business.Testing;

sealed class AESAdditionalProcedureProviderTest : Customs.Business.Testing.DataProviderTestCase<AESAdditionalProcedureProvider>
{
	public void TestSequenceNumber() => AssertEquals(999, GetProvider().SequenceNumber);

	public void TestAdditionalProcedure() => AssertEquals("asd", GetProvider().AdditionalProcedure);

	protected override AESAdditionalProcedureProvider GetProvider() => new AESAdditionalProcedureProvider("asd", 999);
}
