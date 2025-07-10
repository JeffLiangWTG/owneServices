using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(SealsProvider))]
class SealsProviderTest : Customs.Business.Testing.DataProviderTestCase<SealsProvider>
{
	public void TestSequenceNumeric()
	{
		AssertEquals(1, Provider.SequenceNumeric);
	}

	public void TestId()
	{
		AssertEquals("sealnr", Provider.Id);
	}

	protected override SealsProvider GetProvider() => new SealsProvider("sealnr", 1);
}
