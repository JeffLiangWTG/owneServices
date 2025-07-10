using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(SupplierLocalAddressRequirement))]
	sealed class SupplierLocalAddressRequirementTest : TestCaseWithFactory
	{
		public void TestCheckE2_CompanyNameMaxLength()
		{
			var warningMessage = "Only the first 70 characters will be sent to the customs.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			var supplierLocalAddress = supplierDocumentaryAddress.LocalAddress;
			var targetInfo = supplierLocalAddress.E2_CompanyNameInfo;
			supplierLocalAddress.E2_CompanyName = new ZString('A', 90);
			AssertHasWarning(targetInfo, warningMessage);

			supplierLocalAddress.E2_CompanyName = new ZString('A', 69);
			AssertNoWarning(targetInfo, warningMessage);
		}

		public void TestCheckE2_Address1Maxlength()
		{
			var warningMessage = "Chinese Address Only the first 100 characters will be sent to the customs.";
			var targetInfo = supplierLocalAddress.E2_Address1Info;
			supplierLocalAddress.Address2 = new string('A', 50);
			supplierLocalAddress.AdditionalAddressInformation = new string('A', 50);
			supplierLocalAddress.Address1 = new string('A', 50);
			AssertHasWarning(targetInfo, warningMessage);

			supplierLocalAddress.Address2 = ZString.Empty;
			supplierLocalAddress.Address1 = new string('A', 49);
			AssertNoWarning(targetInfo, warningMessage);
		}

		public void TestCheckE2_City()
		{
			supplierLocalAddress.E2_AddressOverride = true;
			supplierLocalAddress.E2_RN_NKCountryCode = "AU";
			supplierLocalAddress.Validation.ValidateE2_City();
			AssertNoErrors(supplierLocalAddress.E2_CityInfo);
		}

		public void TestCheckE2_Postcode()
		{
			supplierLocalAddress.E2_AddressOverride = true;
			supplierLocalAddress.Validation.ValidateE2_Postcode();
			AssertNoErrors(supplierLocalAddress.E2_PostcodeInfo);
		}

		public void TestCheckE2_State()
		{
			var targetInfo = supplierLocalAddress.E2_StateInfo;
			var warningMessage = "This state code needs to be followed by valid country code.";
			supplierLocalAddress.E2_AddressOverride = true;
			supplierLocalAddress.E2_RN_NKCountryCode = "78";
			supplierLocalAddress.E2_State = "EE";
			AssertHasWarningContaining(targetInfo, warningMessage);
			supplierLocalAddress.E2_RN_NKCountryCode = "DE";
			supplierLocalAddress.Validation.ValidateE2_State();
			AssertNoWarningContaining(targetInfo, warningMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierLocalAddress = supplierDocumentaryAddress.LocalAddress;
		}

		JobDocAddress supplierLocalAddress;
	}
}
