using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.TW.Business.Testing.TWJobDocAddressTest;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(SupplierPicDlvAddressRequirement))]
	sealed class SupplierPicDlvAddressRequirementTest : TestCaseWithFactory
	{
		public void TestCheckE2_CompanyName()
		{
			supplierPickupAddress.E2_AddressOverride = true;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(supplierPickupAddress.E2_CompanyNameInfo);
		}

		public void TestCheckE2_City()
		{
			supplierPickupAddress.E2_AddressOverride = true;
			supplierPickupAddress.E2_RN_NKCountryCode = "AU";
			supplierPickupAddress.Validation.ValidateE2_City();
			AssertNoErrors(supplierPickupAddress.E2_CityInfo);
		}

		public void TestCheckE2_Postcode()
		{
			supplierPickupAddress.E2_AddressOverride = true;
			supplierPickupAddress.Validation.ValidateE2_Postcode();
			AssertNoErrors(supplierPickupAddress.E2_PostcodeInfo);
		}

		public void TestCheckE2_State()
		{
			var targetInfo = supplierPickupAddress.E2_StateInfo;
			var warningMessage = "This state code needs to be followed by valid country code.";
			supplierPickupAddress.E2_AddressOverride = true;
			supplierPickupAddress.E2_RN_NKCountryCode = "78";
			supplierPickupAddress.E2_State = "EE";
			AssertHasWarning(targetInfo, warningMessage);
			supplierPickupAddress.E2_RN_NKCountryCode = "DE";
			supplierPickupAddress.Validation.ValidateE2_State();
			AssertNoWarning(targetInfo, warningMessage);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclarationForJobDocAddressTest>();
			supplierPickupAddress = declaration.SupplierPickupAddress;
		}

		#endregion

		JobDeclarationForJobDocAddressTest declaration;
		JobDocAddress supplierPickupAddress;
	}
}
