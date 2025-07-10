using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCusAccountProviderTest : TestCaseWithFactory
	{
		public void TestShouldDefaultTypeWhenAble()
		{
			var orgCusAccount = Factory.NewWithValidTestData<OrgCusAccount>();
			var provider = orgCusAccount.Provider;
			AssertEquals("Defaulting of Type is not enabled by default.", false, provider.ShouldDefaultTypeWhenAble);
		}

		public void TestOrgCusAccountProvider()
		{
			var authorisation = Factory.New<OrgCusAccount>();
			authorisation.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.Eritrea;
			var provider = authorisation.Provider;
			AssertType<OrgCusAccountProvider>(provider);
			AssertType<OrgCusAccountLookups>(provider.GetNewLookups(authorisation));
			AssertType<OrgCusAccountValidation>(provider.GetNewValidation(authorisation));
		}

		public void TestDifferentProviderTypesArePossible()
		{
			var countryCodes = typeof(Core.Constants.CountryCodes)
				.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
				.Where(fieldInfo => fieldInfo.FieldType == typeof(string))
				.Select(fieldInfo => (string)fieldInfo.GetValue(null))
				.ToArray();
			var providerTypes = countryCodes
				.Select(countryCode => OrgCusAccountProvider.GetByCountryCode(countryCode).GetType().FullName)
				.Distinct()
				.ToArray();
			AssertGreaterThan("Different provider types are possible", providerTypes.Length, 1);
		}
	}
}
