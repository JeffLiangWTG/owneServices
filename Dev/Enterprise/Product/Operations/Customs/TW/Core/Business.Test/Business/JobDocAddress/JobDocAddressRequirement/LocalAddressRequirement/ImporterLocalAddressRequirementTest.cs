using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ImporterLocalAddressRequirement))]
	sealed class ImporterLocalAddressRequirementTest : TestCaseWithFactory
	{
		public void TestCheckE2_CompanyNameMaxLength()
		{
			var warningMessage = "Only the first 70 characters will be sent to the customs.";
			var targetInfo = importerLocalAddress.E2_CompanyNameInfo;
			importerLocalAddress.E2_CompanyName = new ZString('A', 90);
			AssertHasWarning(targetInfo, warningMessage);

			importerLocalAddress.E2_CompanyName = new ZString('A', 69);
			AssertNoWarning(targetInfo, warningMessage);
		}

		public void TestCheckE2_Address1Maxlength()
		{
			var warningMessage = "Chinese Address Only the first 100 characters will be sent to the customs.";
			var targetInfo = importerLocalAddress.E2_Address1Info;
			importerLocalAddress.Address2 = new string('A', 50);
			importerLocalAddress.AdditionalAddressInformation = new string('A', 50);
			importerLocalAddress.Address1 = new string('A', 50);
			AssertHasWarning(targetInfo, warningMessage);

			importerLocalAddress.Address2 = ZString.Empty;
			importerLocalAddress.Address1 = new string('A', 49);
			AssertNoWarning(targetInfo, warningMessage);
		}

		public void TestCheckE2_City()
		{
			importerLocalAddress.E2_AddressOverride = true;
			importerLocalAddress.E2_RN_NKCountryCode = "AU";
			importerLocalAddress.Validation.ValidateE2_City();
			AssertNoErrors(importerLocalAddress.E2_CityInfo);
		}

		public void TestCheckE2_Postcode()
		{
			importerLocalAddress.E2_AddressOverride = true;
			importerLocalAddress.Validation.ValidateE2_Postcode();
			AssertNoErrors(importerLocalAddress.E2_PostcodeInfo);
		}

		public void TestCheckE2_State()
		{
			var targetInfo = importerLocalAddress.E2_StateInfo;
			var warningMessage = "This state code needs to be followed by valid country code.";
			importerLocalAddress.E2_AddressOverride = true;
			importerLocalAddress.E2_RN_NKCountryCode = "78";
			importerLocalAddress.E2_State = "EE";
			AssertHasWarning(targetInfo, warningMessage);
			importerLocalAddress.E2_RN_NKCountryCode = "DE";
			importerLocalAddress.Validation.ValidateE2_State();
			AssertNoWarning(targetInfo, warningMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
			importerDocumentaryAddress.E2_AddressOverride = true;
			importerLocalAddress = importerDocumentaryAddress.LocalAddress;
		}

		JobDocAddress importerLocalAddress;
	}
}
