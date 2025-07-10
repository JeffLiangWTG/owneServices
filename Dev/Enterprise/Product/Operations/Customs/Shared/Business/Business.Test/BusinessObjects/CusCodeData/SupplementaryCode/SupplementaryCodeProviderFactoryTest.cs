using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(SupplementaryCodeProviderFactory))]
	sealed class SupplementaryCodeProviderFactoryTest : TestCaseWithFactory
	{
		public void TestGetByCountryCodeOrDefault()
		{
			var supplementaryCodeProvider = SupplementaryCodeProviderFactory.GetByCountryCodeOrDefault(ZString.Empty,
				() => new BaseSupplementaryCodeProvider(Constants.CountryCodes.Germany));

			AssertType<BaseSupplementaryCodeProvider>("Default Type when country code is empty", supplementaryCodeProvider);

			supplementaryCodeProvider = SupplementaryCodeProviderFactory.GetByCountryCodeOrDefault(Constants.CountryCodes.Germany,
				() => new BaseSupplementaryCodeProvider(Constants.CountryCodes.Germany));

			AssertNotNull("Should be not null when country parameter is not empty", supplementaryCodeProvider);
			CombineAssertions("When Country Specific Provider is present", () =>
			{
				AssertEquals("Provider Type Name", "SupplementaryCodeProvider", supplementaryCodeProvider.GetType().Name);
				AssertEquals("Country Code", Constants.CountryCodes.Germany, supplementaryCodeProvider.CountryCode);
			});
		}
	}
}
