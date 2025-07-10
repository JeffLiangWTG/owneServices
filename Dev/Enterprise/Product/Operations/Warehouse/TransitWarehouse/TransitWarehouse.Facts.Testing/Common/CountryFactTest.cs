using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transit.Facts.Testing
{
	public class CountryFactTest : TestCaseWithFactory
	{
		public void TestNullObject_ThrowsException()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CountryFact(null));
		}

		public void TestCode()
		{
			var refCountry = Factory.New<RefCountry>();
			var code = "AU";
			refCountry.RN_Code = code;

			var countryFact = new CountryFact(refCountry);
			AssertEquals(refCountry.Code, countryFact.Code);
			AssertEquals(code, countryFact.Code);
		}

		public void TestEconomicGrouping()
		{
			var refCountry = Factory.New<RefCountry>();

			var countryFact = new CountryFact(refCountry);
			AssertEquals(refCountry.RN_EconomicGrouping, countryFact.EconomicGrouping);
			AssertEquals(string.Empty, countryFact.EconomicGrouping);

			var economicGrouping = "EUN";
			refCountry.RN_EconomicGrouping = economicGrouping;

			countryFact = new CountryFact(refCountry);
			AssertEquals(refCountry.RN_EconomicGrouping, countryFact.EconomicGrouping);
			AssertEquals(economicGrouping, countryFact.EconomicGrouping);
		}
	}
}
