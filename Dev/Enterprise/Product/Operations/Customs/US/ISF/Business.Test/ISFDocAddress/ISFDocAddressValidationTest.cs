using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class ISFDocAddressValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateOrganisationPKHasToBeCallFirstInValidateAll()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "NAME*";
			var header = Factory.New<CusISFHeader>();
			header.MarkLightValidationAsValidForTesting();
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			header.BuyingParty.OrganisationPK = org.PK;
			header.RunPreSaveValidation();
			var message = @"Buying Party: Company Name : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'.";
			AssertHasWarningContaining(header.BuyingParty.OrganisationPKInfo, message);
		}

		public void TestCheckE2_SocialSecurityNumberDetails()
		{
			ISFDocAddress address = Factory.New<ISFDocAddress>();
			address.OverrideRequirement = new ISFDocAddressRequirement();
			string messageError1 = "MESSAGE ERROR 1";
			string messageError2 = "MESSAGE ERROR 2";
			address.ISFRequirement.ValidateSocialSecurityNumber = (JobDocAddressValidation validation) =>
			{
				ISFDocAddress parent = (ISFDocAddress)validation.Parent;
				parent.E2_SocialSecurityNumberInfo.AddMessageError(messageError1);
			};
			address.ISFRequirement.ValidateSocialSecurityNumberDateOfBirth = (JobDocAddressValidation validation) =>
			{
				ISFDocAddress parent = (ISFDocAddress)validation.Parent;
				parent.E2_SocialSecurityNumberDateOfBirthInfo.AddMessageError(messageError2);
			};
			address.E2_AddressOverride = true;
			address.Validation.ValidateE2_SocialSecurityNumberDetails();
			AssertNoMessageError(address.E2_SocialSecurityNumberDetailsInfo, messageError1);
			AssertNoMessageError(address.E2_SocialSecurityNumberDetailsInfo, messageError2);
			address.E2_GovRegNumType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			AssertHasMessageError(address.E2_SocialSecurityNumberDetailsInfo, messageError1);
			AssertHasMessageError(address.E2_SocialSecurityNumberDetailsInfo, messageError2);
			address.E2_GovRegNumType = OrgCusCode.CodeTypes.CarrierCode;
			AssertNoMessageError(address.E2_SocialSecurityNumberDetailsInfo, messageError1);
			AssertNoMessageError(address.E2_SocialSecurityNumberDetailsInfo, messageError2);
		}

		public void TestCheckE2_SocialSecurityNumber()
		{
			ISFDocAddress address = Factory.New<ISFDocAddress>();
			address.OverrideRequirement = new ISFDocAddressRequirement();
			string messageError1 = "MESSAGE ERROR 1";
			address.E2_AddressOverride = true;
			address.E2_GovRegNumType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			address.E2_SocialSecurityNumber = "Z";
			address.E2_SocialSecurityNumber = ZString.Empty;
			AssertNoMessageError(address.E2_SocialSecurityNumberInfo, messageError1);
			address.ISFRequirement.ValidateSocialSecurityNumber = (JobDocAddressValidation validation) =>
			{
				ISFDocAddress parent = (ISFDocAddress)validation.Parent;
				parent.E2_SocialSecurityNumberInfo.AddMessageError(messageError1);
			};
			address.E2_SocialSecurityNumber = "Z";
			address.E2_SocialSecurityNumber = ZString.Empty;
			AssertHasMessageError(address.E2_SocialSecurityNumberInfo, messageError1);
			address.E2_GovRegNumType = OrgCusCode.CodeTypes.CarrierCode;
			AssertNoMessageError(address.E2_SocialSecurityNumberInfo, messageError1);
		}

		public void TestCheckE2_SocialSecurityNumberDateOfBirth()
		{
			ISFDocAddress address = Factory.New<ISFDocAddress>();
			address.OverrideRequirement = new ISFDocAddressRequirement();
			string messageError1 = "MESSAGE ERROR 1";
			address.E2_AddressOverride = true;
			address.E2_GovRegNumType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			address.E2_SocialSecurityNumberDateOfBirth = ZDate.BrettsBirthday;
			address.E2_SocialSecurityNumberDateOfBirth = ZDate.Empty;
			AssertNoMessageError(address.E2_SocialSecurityNumberDateOfBirthInfo, messageError1);
			address.ISFRequirement.ValidateSocialSecurityNumberDateOfBirth = (JobDocAddressValidation validation) =>
			{
				ISFDocAddress parent = (ISFDocAddress)validation.Parent;
				parent.E2_SocialSecurityNumberDateOfBirthInfo.AddMessageError(messageError1);
			};
			address.E2_SocialSecurityNumberDateOfBirth = ZDate.BrettsBirthday;
			address.E2_SocialSecurityNumberDateOfBirth = ZDate.Empty;
			AssertHasMessageError(address.E2_SocialSecurityNumberDateOfBirthInfo, messageError1);
			address.E2_GovRegNumType = OrgCusCode.CodeTypes.CarrierCode;
			AssertNoMessageError(address.E2_SocialSecurityNumberDateOfBirthInfo, messageError1);
		}

		public void TestCheckE2_Contact()
		{
			var address = Factory.New<ISFDocAddress>();
			address.E2_AddressOverride = true;
			address.E2_Contact = new string('*', 35);
			AssertNoWarning(address.E2_ContactInfo, "Contact is too long. Only the first 35 characters will be sent.");
			address.E2_Contact = new string('*', 36);
			AssertHasWarning(address.E2_ContactInfo, "Contact is too long. Only the first 35 characters will be sent.");
		}

		public void TestCheckE2_CompanyName()
		{
			var address = Factory.New<ISFDocAddress>();
			address.E2_AddressOverride = true;
			address.E2_CompanyName = new string('*', 35);
			AssertNoWarning(address.E2_CompanyNameInfo, "Company Name is too long. Only the first 35 characters will be sent.");
			address.E2_CompanyName = new string('*', 36);
			AssertHasWarning(address.E2_CompanyNameInfo, "Company Name is too long. Only the first 35 characters will be sent.");
		}

		public void TestCheckE2_AddressSequence()
		{
			var header = Factory.New<CusISFHeader>();
			var manufacturerAddress = new CusISFHeaderManufacturerAddresses(header);
			for (int i = 0; i <= byte.MaxValue; i++)
			{
				var docAddress = Factory.NewWithValidTestData<ISFDocAddress>();
				manufacturerAddress.Add(docAddress);
				docAddress.E2_AddressSequence = new ZByte((byte)i);
			}

			Assert(!header.HasErrors);
			var finalDocAddress = Factory.NewWithValidTestData<ISFDocAddress>();
			manufacturerAddress.Add(finalDocAddress);
			header.RunPreSaveValidation();
			Assert(header.HasErrors);
			AssertNoNotifications(finalDocAddress.E2_AddressSequenceInfo);
		}

		public void TestShouldDowngradeToWarning()
		{
			var validation1 = GenerateISFDocAddressValidationForTesting(MessageStatusList.Codes.AwaitingISFAdd);
			var actual1 = validation1.ShouldSuppressErrorExposed();
			AssertEquals(true, actual1);
			var validation2 = GenerateISFDocAddressValidationForTesting(MessageStatusList.Codes.AwaitingISFDelete);
			var actual2 = validation2.ShouldSuppressErrorExposed();
			AssertEquals(true, actual2);
			var validation3 = GenerateISFDocAddressValidationForTesting(MessageStatusList.Codes.AwaitingISFReplace);
			var actual3 = validation3.ShouldSuppressErrorExposed();
			AssertEquals(true, actual3);
			var validation4 = GenerateISFDocAddressValidationForTesting(MessageStatusList.Codes.ClearISFAdd);
			var actual4 = validation4.ShouldSuppressErrorExposed();
			AssertEquals(true, actual4);
			var validation5 = GenerateISFDocAddressValidationForTesting(MessageStatusList.Codes.ClearISFDelete);
			var actual5 = validation5.ShouldSuppressErrorExposed();
			AssertEquals(true, actual5);
			var validation6 = GenerateISFDocAddressValidationForTesting(MessageStatusList.Codes.ClearISFReplace);
			var actual6 = validation6.ShouldSuppressErrorExposed();
			AssertEquals(true, actual6);
			var validation7 = GenerateISFDocAddressValidationForTesting(MessageStatusList.Codes.ClearWithWarningISFAdd);
			var actual7 = validation7.ShouldSuppressErrorExposed();
			AssertEquals(true, actual7);
			var validation8 = GenerateISFDocAddressValidationForTesting(MessageStatusList.Codes.ClearWithWarningISFDelete);
			var actual8 = validation8.ShouldSuppressErrorExposed();
			AssertEquals(true, actual8);
			var validation9 = GenerateISFDocAddressValidationForTesting(MessageStatusList.Codes.ClearWithWarningISFReplace);
			var actual9 = validation9.ShouldSuppressErrorExposed();
			AssertEquals(true, actual9);
		}

		public void TestShouldKeepError()
		{
			var validation1 = GenerateISFDocAddressValidationForTesting(MessageStatusList.Codes.ErrorISFAdd);
			var actual1 = validation1.ShouldSuppressErrorExposed();
			AssertEquals(false, actual1);
			var validation2 = GenerateISFDocAddressValidationForTesting(MessageStatusList.Codes.NotSentISF);
			var actual2 = validation2.ShouldSuppressErrorExposed();
			AssertEquals(false, actual2);
			var validation3 = GenerateISFDocAddressValidationForTesting(ZString.Empty);
			var actual3 = validation3.ShouldSuppressErrorExposed();
			AssertEquals(false, actual3);
		}

		ISFDocAddressValidationForTesting GenerateISFDocAddressValidationForTesting(string messageStatusCode)
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_CustomsStatus = messageStatusCode;
			var manufacturerAddress = new CusISFHeaderManufacturerAddresses(header);
			ISFDocAddress address = Factory.New<ISFDocAddress>();
			address.E2_AddressOverride = true;
			address.E2_ValidationStatus = AddressValidationStatus.Invalid;
			manufacturerAddress.Add(address);
			header.RunPreSaveValidation();
			ISFDocAddressValidationForTesting validation = new ISFDocAddressValidationForTesting(address);
			return validation;
		}

		sealed class ISFDocAddressValidationForTesting : ISFDocAddressValidation
		{
			public ISFDocAddressValidationForTesting(ISFDocAddress parent)
				: base(parent)
			{
			}

			public bool ShouldSuppressErrorExposed() => ShouldSuppressError();
		}
	}
}
