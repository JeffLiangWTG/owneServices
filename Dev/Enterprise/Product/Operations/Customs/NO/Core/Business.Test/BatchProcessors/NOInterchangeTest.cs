using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(NOInterchange))]
sealed class NOInterchangeTest : EDIInterchangeTest
{
	public void TestNOInterchange() => CombineAssertions(() =>
	{
		var interchange = Factory.New<NOInterchange>();
		AssertType<NOInterchange>(interchange);
		AssertEquals("NOC", interchange.EI_ApplicationCode);
	});
}
