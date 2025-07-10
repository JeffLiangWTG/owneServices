using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(LocalProcessorLocalAddressRequirement))]
	sealed class LocalProcessorLocalAddressRequirementTest : TestCaseWithFactory
	{
		public void TestCheckE2_CompanyNameMaxLength()
		{
			var warningMessage = "Only the first 70 characters will be sent to the customs.";
			var targetInfo = localProcessorLocalAddress.E2_CompanyNameInfo;
			localProcessorLocalAddress.E2_CompanyName = new ZString('A', 90);
			AssertHasWarning(targetInfo, warningMessage);

			localProcessorLocalAddress.E2_CompanyName = new ZString('A', 69);
			AssertNoWarning(targetInfo, warningMessage);
		}

		public void TestCheckE2_Address1Maxlength()
		{
			var warningMessage = "Chinese Address Only the first 100 characters will be sent to the customs.";
			var targetInfo = localProcessorLocalAddress.E2_Address1Info;
			localProcessorLocalAddress.Address2 = new string('A', 50);
			localProcessorLocalAddress.AdditionalAddressInformation = new string('A', 50);
			localProcessorLocalAddress.Address1 = new string('A', 50);
			AssertHasWarning(targetInfo, warningMessage);

			localProcessorLocalAddress.Address2 = ZString.Empty;
			localProcessorLocalAddress.Address1 = new string('A', 49);
			AssertNoWarning(targetInfo, warningMessage);
		}

		public void TestCheckE2_City()
		{
			localProcessorLocalAddress.E2_AddressOverride = true;
			localProcessorLocalAddress.E2_RN_NKCountryCode = "AU";
			localProcessorLocalAddress.Validation.ValidateE2_City();
			AssertNoErrors(localProcessorLocalAddress.E2_CityInfo);
		}

		public void TestCheckE2_Postcode()
		{
			localProcessorLocalAddress.E2_AddressOverride = true;
			localProcessorLocalAddress.Validation.ValidateE2_Postcode();
			AssertNoErrors(localProcessorLocalAddress.E2_PostcodeInfo);
		}

		public void TestCheckE2_State()
		{
			var targetInfo = localProcessorLocalAddress.E2_StateInfo;
			var warningMessage = "This state code needs to be followed by valid country code.";
			localProcessorLocalAddress.E2_AddressOverride = true;
			localProcessorLocalAddress.E2_RN_NKCountryCode = "78";
			localProcessorLocalAddress.E2_State = "EE";
			AssertHasWarning(targetInfo, warningMessage);
			localProcessorLocalAddress.E2_RN_NKCountryCode = "DE";
			localProcessorLocalAddress.Validation.ValidateE2_State();
			AssertNoWarning(targetInfo, warningMessage);
		}

		public void TestValidateE2_CompanyName_NX101()
		{
			const string expectedMessage = "You have not entered a Local Processor Local Company Name.";
			var targetInfo = localProcessorLocalAddress.E2_CompanyNameInfo;
			var validation = localProcessorLocalAddress.Validation;

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			localProcessorAddress.E2_CompanyName = "Company Name";
			validation.ValidateE2_CompanyName();
			AssertHasMessageError("Is Certificate15, Company Name is not empty, Local Company Name is empty", targetInfo, expectedMessage);

			cMHeader.TW1_CertificateType = ZString.Empty;
			localProcessorAddress.E2_CompanyName = ZString.Empty;
			validation.ValidateE2_CompanyName();
			AssertHasMessageError("Is not Certificate15, Company Name is empty, Local Company Name is empty", targetInfo, expectedMessage);

			cMHeader.TW1_CertificateType = ZString.Empty;
			localProcessorAddress.E2_CompanyName = "Company Name";
			validation.ValidateE2_CompanyName();
			AssertNoMessageError("Is not Certificate15, Company Name is not empty, Local Company Name is empty", targetInfo, expectedMessage);

			localProcessorAddress.E2_CompanyName = ZString.Empty;
			localProcessorLocalAddress.E2_CompanyName = "Local Company Name";
			validation.ValidateE2_CompanyName();
			AssertNoMessageError("Company Name is empty, Local Company Name is not empty", targetInfo, expectedMessage);
		}

		public void TestValidateE2_Address1_NX101()
		{
			const string expectedMessage = "You have not entered a Local Processor Local Address.";
			var targetInfo = localProcessorLocalAddress.Address1Info;
			var validation = localProcessorLocalAddress.Validation;

			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			cMHeader.TW1_CertificateType = CertificateTypeList.Codes.Code15;
			localProcessorAddress.E2_Address1 = "Address";
			validation.ValidateE2_Address1();
			AssertHasMessageError("Is Certificate15, Address is not empty, Local Address is empty", targetInfo, expectedMessage);

			cMHeader.TW1_CertificateType = ZString.Empty;
			localProcessorAddress.E2_Address1 = ZString.Empty;
			validation.ValidateE2_Address1();
			AssertHasMessageError("Is not Certificate15, Address is empty, Local Address is empty", targetInfo, expectedMessage);

			cMHeader.TW1_CertificateType = ZString.Empty;
			localProcessorAddress.E2_Address1 = "Address";
			validation.ValidateE2_Address1();
			AssertNoMessageError("Is not Certificate15, Address is not empty, Local Address is empty", targetInfo, expectedMessage);

			localProcessorAddress.E2_Address1 = ZString.Empty;
			localProcessorLocalAddress.E2_Address1 = "Local Address";
			validation.ValidateE2_Address1();
			AssertNoMessageError("Address is empty, Local Address is not empty", targetInfo, expectedMessage);
		}

		public void TestValidateE2_CompanyName_NX601()
		{
			const string expectedMessage = "You have not entered a Local Processor Local Company Name.";
			var targetInfo = localProcessorLocalAddress.E2_CompanyNameInfo;
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;

			localProcessorLocalAddress.E2_CompanyName = "Local Company Name";
			AssertNoMessageError("Local Company Name is not empty", targetInfo, expectedMessage);
			localProcessorLocalAddress.E2_CompanyName = ZString.Empty;
			AssertHasMessageError("Local Company Name is empty", targetInfo, expectedMessage);
		}

		public void TestValidateE2_Address1_NX601()
		{
			const string expectedMessage = "You have not entered a Local Processor Local Address.";
			var targetInfo = localProcessorLocalAddress.Address1Info;
			cMHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;

			localProcessorLocalAddress.E2_Address1 = "Local Address";
			AssertNoMessageError("Local Address is not empty", targetInfo, expectedMessage);

			localProcessorLocalAddress.E2_Address1 = ZString.Empty;
			AssertHasMessageError("Local Address is empty", targetInfo, expectedMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var instruction = declaration.CusEntryInstruction;
			cMHeader = instruction.ControllingMessageHeaders.AddNew();
			localProcessorAddress = cMHeader.LocalProcessorAddress;
			localProcessorAddress.E2_AddressOverride = true;
			localProcessorLocalAddress = localProcessorAddress.LocalAddress;
		}

		CusTWControllingMessageHeader cMHeader;
		JobDocAddress localProcessorLocalAddress;
		TWJobDocAddress localProcessorAddress;
	}
}
