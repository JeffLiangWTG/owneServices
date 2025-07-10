using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.TW.Business.Testing.TWJobDocAddressTest;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ImporterPicDlvAddressRequirement))]
	sealed class ImporterPicDlvAddressRequirementTest : TestCaseWithFactory
	{
		public void TestCheckE2_CompanyName()
		{
			importerDeliveryAddress.E2_AddressOverride = true;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(importerDeliveryAddress.E2_CompanyNameInfo);
		}

		public void TestCheckE2_City()
		{
			importerDeliveryAddress.E2_AddressOverride = true;
			importerDeliveryAddress.E2_RN_NKCountryCode = "AU";
			importerDeliveryAddress.Validation.ValidateE2_City();
			AssertNoErrors(importerDeliveryAddress.E2_CityInfo);
		}

		public void TestCheckE2_Postcode()
		{
			importerDeliveryAddress.E2_AddressOverride = true;
			importerDeliveryAddress.Validation.ValidateE2_Postcode();
			AssertNoErrors(importerDeliveryAddress.E2_PostcodeInfo);
		}

		public void TestCheckE2_State()
		{
			var targetInfo = importerDeliveryAddress.E2_StateInfo;
			var warningMessage = "This state code needs to be followed by valid country code.";
			importerDeliveryAddress.E2_AddressOverride = true;
			importerDeliveryAddress.E2_RN_NKCountryCode = "78";
			importerDeliveryAddress.E2_State = "EE";
			AssertHasWarning(targetInfo, warningMessage);
			importerDeliveryAddress.E2_RN_NKCountryCode = "DE";
			importerDeliveryAddress.Validation.ValidateE2_State();
			AssertNoWarning(targetInfo, warningMessage);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclarationForJobDocAddressTest>();
			importerDeliveryAddress = declaration.ImporterDeliveryAddress;
		}

		#endregion

		JobDeclarationForJobDocAddressTest declaration;
		JobDocAddress importerDeliveryAddress;
	}
}
