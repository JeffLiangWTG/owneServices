using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class UseDummyCustomsMessageStatusProviderAttributeTest : TestCase
	{
		[UseDummyCustomsMessageStatusProvider]
		public void TestUse()
		{
			AssertEquals(true, UseDummyCustomsMessageStatusProviderAttribute.UseDummy);
		}

		public void TestDontUse()
		{
			AssertEquals(false, UseDummyCustomsMessageStatusProviderAttribute.UseDummy);
		}
	}
}
