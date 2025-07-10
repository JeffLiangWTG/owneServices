using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class TariffViewAsCodeDescriptionTest : TestCase
	{
		public void TestEquals()
		{
			CombineAssertions(() =>
			{
				Assert(new TariffViewAsCodeDescription("99038809", "test a", true).Equals(new TariffViewAsCodeDescription("99038809", "test b", true)));
				Assert(!new TariffViewAsCodeDescription("99038808", "test desc", true).Equals(new TariffViewAsCodeDescription("99038809", "test desc", true)));
				Assert(!new TariffViewAsCodeDescription("99038808", "test desc", true).Equals(null));
			});
		}
		public void TestGetHashCode()
		{
			AssertEquals(new TariffFormatter().DisplayFormat("99038809").GetHashCode(), new TariffViewAsCodeDescription("99038809", "test desc", true).GetHashCode());
		}
	}
}
