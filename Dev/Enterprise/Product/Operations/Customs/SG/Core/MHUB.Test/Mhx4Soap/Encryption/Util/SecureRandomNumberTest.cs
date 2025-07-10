using NUnit.Framework;

namespace Enterprise.Customs.SG.MHUB.Mhx4Soap.Encryption.Util.Testing
{
	sealed class SecureRandomNumberTest : TestCase
	{
		public void TestBetween()
		{
			var random = SecureRandomNumber.Between(0, 100);
			AssertEquals(true, random >= 0);
			AssertEquals(true, random <= 100);
		}
	}
}
