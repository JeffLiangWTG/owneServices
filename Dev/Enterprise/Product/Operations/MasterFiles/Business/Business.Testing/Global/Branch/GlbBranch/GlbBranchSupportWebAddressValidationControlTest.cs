using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbBranchSupportWebAddressValidationControlTest : SupportWebAddressValidationTest<GlbBranch>
	{
		protected override void AssertCityValidationResultWhenAddressValidationIsEnabled(GlbBranch branch)
		{
			branch.GB_City = "Sydney";
			branch.ValidationStatus = AddressValidationStatus.Verified;
			branch.Validation.ValidateGB_City();
			AssertNoError(branch.GB_CityInfo, "Please enter a City.");

			branch.GB_City = ZString.Empty;
			branch.ValidationStatus = AddressValidationStatus.Verified;
			branch.Validation.ValidateGB_City();
			AssertNoError(branch.GB_CityInfo, "Please enter a City.");

			branch.GB_City = "Sydney";
			branch.ValidationStatus = AddressValidationStatus.VerifiedToStreet;
			branch.Validation.ValidateGB_City();
			AssertNoError(branch.GB_CityInfo, "Please enter a City.");

			branch.GB_City = ZString.Empty;
			branch.ValidationStatus = AddressValidationStatus.VerifiedToStreet;
			branch.Validation.ValidateGB_City();
			AssertNoError(branch.GB_CityInfo, "Please enter a City.");

			branch.GB_City = "Sydney";
			branch.ValidationStatus = AddressValidationStatus.ManuallyVerified;
			branch.Validation.ValidateGB_City();
			AssertNoError(branch.GB_CityInfo, "Please enter a City.");

			branch.GB_City = ZString.Empty;
			branch.ValidationStatus = AddressValidationStatus.ManuallyVerified;
			branch.Validation.ValidateGB_City();
			AssertHasError(branch.GB_CityInfo, "Please enter a City.");

			branch.GB_City = "Sydney";
			branch.ValidationStatus = AddressValidationStatus.ToBeVerified;
			branch.Validation.ValidateGB_City();
			AssertNoError(branch.GB_CityInfo, "Please enter a City.");

			branch.GB_City = ZString.Empty;
			branch.ValidationStatus = AddressValidationStatus.ToBeVerified;
			branch.Validation.ValidateGB_City();
			AssertHasError(branch.GB_CityInfo, "Please enter a City.");
		}
	}
}
