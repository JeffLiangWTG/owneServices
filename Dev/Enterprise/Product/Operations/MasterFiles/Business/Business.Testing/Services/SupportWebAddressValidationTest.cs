using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class SupportWebAddressValidationTest<T> : TestCaseWithFactory where T : BusinessObject, ISupportWebAddressValidation
	{
		protected virtual T GetBOToTest()
		{
			return Factory.NewWithValidTestData<T>();
		}

		#region Implementation

		protected virtual void AssertCityValidationResultWhenAddressValidationIsEnabled(T testBO)
		{
			testBO.City = "test";
			AssertEquals(false, testBO.CityInfo.HasNotifications());

			testBO.City = "";
			AssertEquals(false, testBO.CityInfo.HasNotifications());
		}

		protected virtual void AssertCityValidationResultWhenCityIsEmpty(T testBO)
		{
			Assert(testBO.CityInfo.HasNotifications());
		}

		#endregion

		#region Test

		public void TestCityWillNotThrowValidationErrors()
		{
			var testBO = GetBOToTest();

			Env.Registry.EnableAddressValidationWebService = false;

			testBO.City = "test";
			Assert(!testBO.CityInfo.HasNotifications());

			testBO.City = "";
			AssertCityValidationResultWhenCityIsEmpty(testBO);

			Env.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			AssertCityValidationResultWhenAddressValidationIsEnabled(testBO);
		}

		#endregion
	}
}
