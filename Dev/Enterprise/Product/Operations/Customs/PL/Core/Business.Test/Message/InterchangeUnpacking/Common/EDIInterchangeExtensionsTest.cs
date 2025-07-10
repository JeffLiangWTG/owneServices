using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class EDIInterchangeExtensionsTest : TestCaseWithFactory
{
	public void TestGetReadContext() => AssertType<InterchangeReadContext>(Factory.New<EDIInterchange>().GetReadContext());
}
