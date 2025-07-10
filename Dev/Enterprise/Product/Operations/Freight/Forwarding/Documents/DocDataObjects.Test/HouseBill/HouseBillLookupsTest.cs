using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class HouseBillLookupsTest : TestCaseWithFactory
	{
		#region TestUnlocos

		public void TestUnlocos()
		{
			var lookups = new HouseBillLookups(Factory);

			var unlocos = lookups.Unlocos;

			AssertNotNull("lookup is not null", unlocos);
			AssertEquals("lookup is cached", unlocos, lookups.Unlocos);
		}

		#endregion

		#region TestCountries

		public void TestCountries()
		{
			var lookups = new HouseBillLookups(Factory);

			var countries = lookups.Countries;

			AssertNotNull("lookup is not null", countries);
			AssertEquals("lookup is cached", countries, lookups.Countries);
		}

		#endregion

		#region TestCurriencies

		public void TestCurriencies()
		{
			var lookups = new HouseBillLookups(Factory);

			var currencies = lookups.Currencies;

			AssertNotNull("lookup is not null", currencies);
			AssertEquals("lookup is cached", currencies, lookups.Currencies);
		}

		#endregion

		#region TestPaymentTerms

		public void TestPaymentTerms()
		{
			var lookups = new HouseBillLookups(Factory);

			var paymentTerms = lookups.PaymentTerms;

			AssertNotNull("lookup is not null", paymentTerms);
			AssertEquals("lookup is cached", paymentTerms, lookups.PaymentTerms);
		}

		#endregion

		#region TestReleaseTypes

		public void TestReleaseTypes()
		{
			var lookups = new HouseBillLookups(Factory);

			var releaseTypes = lookups.ReleaseTypes;

			AssertNotNull("lookup is not null", releaseTypes);
			AssertEquals("lookup is cached", releaseTypes, lookups.ReleaseTypes);
		}

		#endregion

		public void TestAsAgentOptions()
		{
			var lookups = new HouseBillLookups(Factory);

			var asAgentOptionList = lookups.AsAgentOptions;

			AssertNotNull("Lookup is not null", asAgentOptionList);
			AssertEquals("Lookup is cached", asAgentOptionList, lookups.AsAgentOptions);
		}
	}
}
