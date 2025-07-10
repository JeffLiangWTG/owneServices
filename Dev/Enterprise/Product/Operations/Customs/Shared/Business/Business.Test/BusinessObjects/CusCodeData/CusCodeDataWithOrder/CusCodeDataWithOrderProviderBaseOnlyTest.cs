using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	class CusCodeDataWithOrderProviderBaseOnlyTest : TestCase
	{
		readonly CusCodeDataWithOrderProviderForTest cusCodeDataWithOrder = new CusCodeDataWithOrderProviderForTest("EUN");

		public void TestCountryCode()
		{
			AssertEquals("CountryCode", "EUN", cusCodeDataWithOrder.CountryCode);
		}

		public void TestCodesStartingOrder()
		{
			AssertEquals("CodesStartingOrder", new ZShort(3), cusCodeDataWithOrder.CodesStartingOrder);
		}

		public void TestNumberOfCodes()
		{
			AssertEquals("NumberOfCodes", new ZShort(8), cusCodeDataWithOrder.NumberOfCodes);
		}

		class CusCodeDataWithOrderProviderForTest : CusCodeDataWithOrderProvider
		{
			public CusCodeDataWithOrderProviderForTest(ZString countryCode) : base(countryCode)
			{
			}
		}
	}
}
