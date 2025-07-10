using NUnit.Framework;

namespace Enterprise.Customs.SG.MHUB.Mhx4Soap.Util.Testing
{
	sealed class UnixTimestampProviderTest : TestCase
	{
		[TestDate(1970, 1, 1, 0, 0, 0)]
		public void TestAtTheDawnOfUnixTime()
		{
			AssertEquals("0", UnixTimestampProvider.GetCurrentUnixTimestampMilliseconds());
			AssertEquals("0", UnixTimestampProvider.GetCurrentUnixTimestampMicroseconds());
		}

		[TestDate(1970, 1, 1, 0, 0, 1)]
		public void TestOneSecondLater()
		{
			AssertEquals("1000", UnixTimestampProvider.GetCurrentUnixTimestampMilliseconds());
			AssertEquals("1000000", UnixTimestampProvider.GetCurrentUnixTimestampMicroseconds());
		}

		[TestDate(2015, 8, 22, 14, 0, 0)]
		public void TestAGoodDate()
		{
			AssertEquals("1440252000000", UnixTimestampProvider.GetCurrentUnixTimestampMilliseconds());
			AssertEquals("1440252000000000", UnixTimestampProvider.GetCurrentUnixTimestampMicroseconds());
		}
	}
}
