using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbCompanySupportWebAddressValidationControlTest : SupportWebAddressValidationTest<GlbCompany>
	{
		protected override void AssertCityValidationResultWhenAddressValidationIsEnabled(GlbCompany company)
		{
			company.GC_City = "Sydney";
			company.ValidationStatus = AddressValidationStatus.Verified;
			company.Validation.ValidateGC_City();
			AssertNoError(company.GC_CityInfo, "Please enter a City.");

			company.GC_City = ZString.Empty;
			company.ValidationStatus = AddressValidationStatus.Verified;
			company.Validation.ValidateGC_City();
			AssertNoError(company.GC_CityInfo, "Please enter a City.");

			company.GC_City = "Sydney";
			company.ValidationStatus = AddressValidationStatus.VerifiedToStreet;
			company.Validation.ValidateGC_City();
			AssertNoError(company.GC_CityInfo, "Please enter a City.");

			company.GC_City = ZString.Empty;
			company.ValidationStatus = AddressValidationStatus.VerifiedToStreet;
			company.Validation.ValidateGC_City();
			AssertNoError(company.GC_CityInfo, "Please enter a City.");

			company.GC_City = "Sydney";
			company.ValidationStatus = AddressValidationStatus.ManuallyVerified;
			company.Validation.ValidateGC_City();
			AssertNoError(company.GC_CityInfo, "Please enter a City.");

			company.GC_City = ZString.Empty;
			company.ValidationStatus = AddressValidationStatus.ManuallyVerified;
			company.Validation.ValidateGC_City();
			AssertHasError(company.GC_CityInfo, "Please enter a City.");

			company.GC_City = "Sydney";
			company.ValidationStatus = AddressValidationStatus.ToBeVerified;
			company.Validation.ValidateGC_City();
			AssertNoError(company.GC_CityInfo, "Please enter a City.");

			company.GC_City = ZString.Empty;
			company.ValidationStatus = AddressValidationStatus.ToBeVerified;
			company.Validation.ValidateGC_City();
			AssertHasError(company.GC_CityInfo, "Please enter a City.");
		}
	}
}
