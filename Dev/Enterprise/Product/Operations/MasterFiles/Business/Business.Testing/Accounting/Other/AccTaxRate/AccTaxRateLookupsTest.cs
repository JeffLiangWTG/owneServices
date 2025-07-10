using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class AccTaxRateLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTaxMessagesIsUsingTheCorrectCountryCodeFilter()
		{
			var taxMsg = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg.A9_Code = "MSG";
			taxMsg.A9_EnglishMsg = "ENG msg";
			taxMsg.A9_LocalMsg = "Local msg";
			taxMsg.A9_RN_NKCountryCode = Constants.CountryCodes.UnitedStates;

			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = "COD";
			taxRate.AT_RN_NKCountry = Constants.CountryCodes.UnitedStates;
			Factory.Save();

			var lookup = new AccTaxRateLookups(taxRate);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France))
			{
				AssertEquals(true, lookup.DefaultVatClasses.Contains(taxMsg));
			}
		}

		public void TestTypes()
		{
			AssertNotNull(new AccTaxRateLookups(Factory.New<AccTaxRate>()).Types);
		}

		public void TestExtraTypes()
		{
			AssertNotNull(new AccTaxRateLookups(Factory.New<AccTaxRate>()).ExtraTypes);
		}
	}
}
