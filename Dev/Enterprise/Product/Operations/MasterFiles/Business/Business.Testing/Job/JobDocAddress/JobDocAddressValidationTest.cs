using System;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobDocAddressValidation))]
	public class JobDocAddressValidationTest : BusinessObjectValidationTestCase
	{
		const string InvalidAddressMessage =
			"There is an invalid address recorded on this job. " +
			"Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. " +
			"The suggestion box is accessed by clicking the envelope icon next to the address.";

		public void TestValidateAll_WhenDisablingSuppressErrorOnRegistryAndRecord_ShouldRaiseError()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.IsErrorSuppressionEnabled = false;

			var docAddress = Factory.CreateValidOverriddenJobDocAddress();
			docAddress.E2_SuppressAddressValidationError = false;
			docAddress.E2_ValidationStatus = AddressValidationStatus.Invalid;

			var validation = new JobDocAddressValidation(docAddress);

			validation.ValidateAll();

			AssertHasError(
				"Should treat invalid address message as error.",
				docAddress.E2_ValidationStatusInfo,
				InvalidAddressMessage);

			AssertNoWarning(
				"Should not treat invalid address message as warning.",
				docAddress.E2_ValidationStatusInfo,
				InvalidAddressMessage);
		}

		public void TestValidateAll_WhenDisablingSuppressErrorOnRegistryButNotOnRecord_ShouldRaiseError()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.IsErrorSuppressionEnabled = false;

			var docAddress = Factory.CreateValidOverriddenJobDocAddress();
			docAddress.E2_SuppressAddressValidationError = true;
			docAddress.E2_ValidationStatus = AddressValidationStatus.Invalid;

			var validation = new JobDocAddressValidation(docAddress);

			validation.ValidateAll();

			AssertHasError(
				"Should treat invalid address message as error.",
				docAddress.E2_ValidationStatusInfo,
				InvalidAddressMessage);

			AssertNoWarning(
				"Should not treat invalid address message as warning.",
				docAddress.E2_ValidationStatusInfo,
				InvalidAddressMessage);
		}

		public void TestValidateAll_WhenDisablingSuppressErrorOnRecordButNotOnRegistry_ShouldRaiseError()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.IsErrorSuppressionEnabled = true;

			var docAddress = Factory.CreateValidOverriddenJobDocAddress();
			docAddress.E2_SuppressAddressValidationError = false;
			docAddress.E2_ValidationStatus = AddressValidationStatus.Invalid;

			var validation = new JobDocAddressValidation(docAddress);

			validation.ValidateAll();

			AssertHasError(
				"Should treat invalid address message as error.",
				docAddress.E2_ValidationStatusInfo,
				InvalidAddressMessage);

			AssertNoWarning(
				"Should not treat invalid address message as warning.",
				docAddress.E2_ValidationStatusInfo,
				InvalidAddressMessage);
		}

		public void TestValidateE2_AdditionalAddressInformation_WhenEnabledRegistry_OrgHeaderOverrideAdditionalAddressInformationIsEnabled()
		{
			using (OrganisationRegistry.Instance.AllowOverrideAddressAdditionalInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_OverrideAdditionalAddressInformation = true;

				var additional = orgHeader.MainAddress.AdditionalInfos.AddNew();
				additional.OAI_AdditionalInfo = "Dummy Additional Info";

				var docAddress = Factory.New<JobDocAddress>();
				docAddress.E2_AddressOverride = false;
				docAddress.E2_OA_Address = orgHeader.MainAddress.PK;
				docAddress.UnrestrictedAdditionalAddressInformation = "Overrride Additional Info";

				AssertEquals("Overrride Additional Info", docAddress.E2_AdditionalAddressInformation);
				AssertEquals(false, docAddress.E2_AdditionalAddressInformationInfo.HasErrors());

				additional.Delete();
				docAddress.UnrestrictedAdditionalAddressInformation = "New Overrride Additional Info";
				AssertEquals(false, docAddress.E2_AdditionalAddressInformationInfo.HasErrors());

				docAddress.UnrestrictedAdditionalAddressInformation = string.Empty;
				AssertEquals(false, docAddress.E2_AdditionalAddressInformationInfo.HasErrors());
			}
		}

		public void TestValidateE2_AdditionalAddressInformation_WhenEnabledRegistry_OrgHeaderOverrideAdditionalAddressInformationIsDisabled()
		{
			using (OrganisationRegistry.Instance.AllowOverrideAddressAdditionalInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_OverrideAdditionalAddressInformation = false;

				var additional = orgHeader.MainAddress.AdditionalInfos.AddNew();
				additional.OAI_AdditionalInfo = "Dummy Additional Info";

				var docAddress = Factory.New<JobDocAddress>();
				docAddress.E2_AddressOverride = false;
				docAddress.E2_OA_Address = orgHeader.MainAddress.PK;
				docAddress.UnrestrictedAdditionalAddressInformation = "Overrride Additional Info";

				AssertEquals("Overrride Additional Info", docAddress.E2_AdditionalAddressInformation);
				AssertEquals(true, docAddress.E2_AdditionalAddressInformationInfo.HasErrors());

				var errors = docAddress.E2_AdditionalAddressInformationInfo.GetErrors().ToArray();
				AssertEquals(1, errors.Length);
				AssertEquals("The organization does not allow additional information override", errors[0].Message);

				docAddress.UnrestrictedAdditionalAddressInformation = string.Empty;
				AssertEquals(false, docAddress.E2_AdditionalAddressInformationInfo.HasErrors());

				docAddress.UnrestrictedAdditionalAddressInformation = "Dummy Additional Info";
				AssertEquals(false, docAddress.E2_AdditionalAddressInformationInfo.HasErrors());

				docAddress.UnrestrictedAdditionalAddressInformation = "DummY Additional Info";
				AssertEquals("Ignore Case", false, docAddress.E2_AdditionalAddressInformationInfo.HasErrors());
			}
		}

		public void TestValidateE2_AdditionalAddressInformation_WhenEnabledRegistry_OrgAddressAdditionalInfoCollectionIsEmpty()
		{
			using (OrganisationRegistry.Instance.AllowOverrideAddressAdditionalInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_OverrideAdditionalAddressInformation = false;

				var docAddress = Factory.New<JobDocAddress>();
				docAddress.E2_AddressOverride = false;
				docAddress.E2_OA_Address = orgHeader.MainAddress.PK;
				docAddress.UnrestrictedAdditionalAddressInformation = "Overrride Additional Info";

				AssertEquals("Overrride Additional Info", docAddress.E2_AdditionalAddressInformation);
				AssertEquals(true, docAddress.E2_AdditionalAddressInformationInfo.HasErrors());

				var errors = docAddress.E2_AdditionalAddressInformationInfo.GetErrors().ToArray();
				AssertEquals(1, errors.Length);
				AssertEquals("The organization does not allow additional information override", errors[0].Message);
			}
		}

		public void TestValidateE2_AdditionalAddressInformation_WhenDisabledRegistry()
		{
			using (OrganisationRegistry.Instance.AllowOverrideAddressAdditionalInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_OverrideAdditionalAddressInformation = true;

				var docAddress = Factory.New<JobDocAddress>();
				docAddress.E2_AddressOverride = false;
				docAddress.E2_OA_Address = orgHeader.MainAddress.PK;
				docAddress.UnrestrictedAdditionalAddressInformation = "Overrride Additional Info";

				AssertEquals(string.Empty, docAddress.E2_AdditionalAddressInformation);
				AssertEquals(false, docAddress.E2_AdditionalAddressInformationInfo.HasErrors());
			}
		}

		public void TestValidateAll_WhenEnablingSuppressErrorOnRegistryAndRecord_ShouldRaiseError()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.IsErrorSuppressionEnabled = true;

			var docAddress = Factory.CreateValidOverriddenJobDocAddress();
			docAddress.E2_SuppressAddressValidationError = true;
			docAddress.E2_ValidationStatus = AddressValidationStatus.Invalid;

			var validation = new JobDocAddressValidation(docAddress);

			validation.ValidateAll();

			AssertNoError(
				"Should not treat invalid address message as error.",
				docAddress.E2_ValidationStatusInfo,
				InvalidAddressMessage);

			AssertHasWarning(
				"Should treat invalid address message as warning.",
				docAddress.E2_ValidationStatusInfo,
				InvalidAddressMessage);
		}

		public void TestCheckE2_Address1AndE2_Address2()
		{
			var msg = MandatoryValidation.YouHaveNotEnteredMessage("Address");

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_AddressOverride = true;

			var info = docAddress.E2_Address1AndE2_Address2Info;
			docAddress.E2_Address1AndE2_Address2 = "Address 123";

			CombineAssertions(() =>
			{
				AssertNoMessageError(info, msg);

				docAddress.E2_Address1AndE2_Address2 = string.Empty;

				AssertHasMessageError(info, msg);
			});
		}

		public void TestCheckE2_PhoneFormatted()
		{
			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_AddressOverride = true;

			docAddress.OverrideRequirement = new JobDocAddressRequirement();

			CombineAssertions(() =>
			{
				AssertNoMessageError("Phone validation not set on requirement", docAddress.E2_Phone_FormattedInfo, "Phone is required.");

				docAddress.Requirement.ValidatePhoneFormatted = validation =>
				{
					var parent = validation.Parent;
					if (parent.E2_Phone_Formatted.IsEmpty)
					{
						parent.E2_Phone_FormattedInfo.AddMessageError("Phone is required.");
					}
				};

				docAddress.E2_Phone_Formatted = "012345";

				AssertNoMessageError("E2_Phone_Formatted has value", docAddress.E2_Phone_FormattedInfo, "Phone is required.");

				docAddress.E2_Phone_Formatted = string.Empty;

				AssertHasMessageError("E2_Phone empty, requirement validation is set", docAddress.E2_Phone_FormattedInfo, "Phone is required.");
			});
		}

		public void TestE2_RN_NKCountryCodeRequirementValidation()
		{
			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_AddressOverride = true;

			docAddress.OverrideRequirement = new JobDocAddressRequirement();
			var info = docAddress.E2_RN_NKCountryCodeInfo;

			CombineAssertions(() =>
			{
				docAddress.E2_RN_NKCountryCode = "DE";
				AssertNoError("Valid Country, no error", info, "Enter a valid Unspecified: Country/Region Code.");
				docAddress.E2_RN_NKCountryCode = "XY";
				AssertHasError("Invalid Country, default Error", info, "Enter a valid Unspecified: Country/Region Code.");

				docAddress.Requirement.ValidateCountry += validation =>
				{
					ListValidation.MessageErrorIfInvalidCode(validation.Parent.E2_RN_NKCountryCodeInfo);
				};

				docAddress.E2_RN_NKCountryCode = "DE";

				AssertNoMessageError("Valid Country, no Message error", info, "The code you have selected is not in the list.");

				docAddress.E2_RN_NKCountryCode = "XY";
				AssertHasMessageError("Invalid Country + Requirement.ValidateCountry is set, Message Error", info, "The code you have selected is not in the list.");
				AssertNoError("Requirement Message Error overrides default Error", info, "Enter a valid Unspecified: Country/Region Code.");
			});
		}

		public void TestCheckE2_RN_NKCountryCode()
		{
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			JobDocAddress address = Factory.New<JobDocAddress>();
			address.OverrideRequirement = new JobDocAddressRequirement();
			address.Requirement.ValidateCountry =
				delegate(JobDocAddressValidation validation)
				{
					if (validation.Parent.E2_RN_NKCountryCode.IsEmpty)
					{
						validation.Parent.E2_RN_NKCountryCodeInfo.AddMessageError("Country is required.");
					}
				};

			address.E2_AddressOverride = true;
			address.E2_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			AssertEquals(false, address.E2_RN_NKCountryCodeInfo.HasMessageErrors());

			address.E2_RN_NKCountryCode = "";
			AssertEquals(true, address.E2_RN_NKCountryCodeInfo.HasMessageErrors());
		}

		public void TestCheckE2_Contact()
		{
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			JobDocAddress address = Factory.New<JobDocAddress>();
			address.OverrideRequirement = new JobDocAddressRequirement();
			address.Requirement.ValidateContact =
				delegate(JobDocAddressValidation validation)
				{
					if (validation.Parent.E2_Contact.IsEmpty)
					{
						validation.Parent.E2_ContactInfo.AddMessageError("Contact is required.");
					}
				};

			address.E2_AddressOverride = true;
			address.E2_Contact = "BOB THE BUILDER";
			AssertNoMessageError(address.E2_ContactInfo, "Contact is required.");

			address.E2_Contact = "";
			AssertHasMessageError(address.E2_ContactInfo, "Contact is required.");

			address.E2_AddressOverride = false;
			address.E2_OA_Address = organisation.MainAddress.PK;
			address.E2_Contact = "BOB THE BUILDER";
			AssertNoMessageError(address.E2_ContactInfo, "Contact is required.");

			address.E2_Contact = "";
			AssertHasMessageError(address.E2_ContactInfo, "Contact is required.");

			address.E2_Contact = "BOB THE BUILDER";
			var inactiveContact = organisation.Contacts.AddNew();
			inactiveContact.OC_IsActive = false;
			inactiveContact.OC_ContactName = "BOB THE BUILDER";
			address.OrganisationPK = organisation.PK;
			address.ContactPK = inactiveContact.PK;

			address.Validation.ValidateE2_Contact();

			AssertHasWarning(address.E2_ContactInfo, "This Contact is inactive.");

			var activeContact = organisation.ContactsActive.AddNew();
			activeContact.OC_IsActive = false;
			activeContact.OC_ContactName = "BOB THE BUILDER";
			address.OrganisationPK = organisation.PK;
			address.ContactPK = activeContact.PK;

			address.Validation.ValidateE2_Contact();

			AssertNoWarning(address.E2_ContactInfo, "This Contact is inactive.");
		}

		public void TestCheckE2_PassportDetails()
		{
			JobDocAddress address = Factory.New<JobDocAddress>();
			address.OverrideRequirement = new JobDocAddressRequirement();
			string messageError1 = "MESSAGE ERROR 1";
			string messageError2 = "MESSAGE ERROR 2";
			string messageError3 = "MESSAGE ERROR 3";
			address.Requirement.ValidatePassportID =
				delegate(JobDocAddressValidation validation)
				{
					validation.Parent.E2_PassportIDInfo.AddMessageError(messageError1);
				};
			address.Requirement.ValidatePassportDateOfBirth =
				delegate(JobDocAddressValidation validation)
				{
					validation.Parent.E2_PassportDateOfBirthInfo.AddMessageError(messageError2);
				};
			address.Requirement.ValidatePassportCountryOfIssue =
				delegate(JobDocAddressValidation validation)
				{
					validation.Parent.E2_PassportCountryOfIssueInfo.AddMessageError(messageError3);
				};

			address.E2_AddressOverride = true;
			address.Validation.ValidateE2_PassportDetails();
			AssertNoMessageError(address.E2_PassportDetailsInfo, messageError1);
			AssertNoMessageError(address.E2_PassportDetailsInfo, messageError2);
			AssertNoMessageError(address.E2_PassportDetailsInfo, messageError3);

			address.E2_GovRegNumType = OrgCusCode.CodeTypes.PassportID;
			AssertHasMessageError(address.E2_PassportDetailsInfo, messageError1);
			AssertHasMessageError(address.E2_PassportDetailsInfo, messageError2);
			AssertHasMessageError(address.E2_PassportDetailsInfo, messageError3);

			address.E2_GovRegNumType = OrgCusCode.CodeTypes.CarrierCode;
			AssertNoMessageError(address.E2_PassportDetailsInfo, messageError1);
			AssertNoMessageError(address.E2_PassportDetailsInfo, messageError2);
			AssertNoMessageError(address.E2_PassportDetailsInfo, messageError3);
		}

		public void TestCheckE2_PassportID()
		{
			JobDocAddress address = Factory.New<JobDocAddress>();
			address.OverrideRequirement = new JobDocAddressRequirement();
			string messageError = "MESSAGE ERROR 1";
			address.E2_AddressOverride = true;
			address.E2_GovRegNumType = OrgCusCode.CodeTypes.PassportID;
			address.E2_PassportID = "Z";
			address.E2_PassportID = ZString.Empty;
			AssertNoMessageError(address.E2_PassportIDInfo, messageError);

			address.Requirement.ValidatePassportID =
				delegate(JobDocAddressValidation validation)
				{
					validation.Parent.E2_PassportIDInfo.AddMessageError(messageError);
				};
			address.E2_PassportID = "Z";
			address.E2_PassportID = ZString.Empty;
			AssertHasMessageError(address.E2_PassportIDInfo, messageError);

			address.E2_AddressOverride = false;
			AssertNoMessageError(address.E2_PassportIDInfo, messageError);

			address.E2_AddressOverride = true;
			address.E2_PassportID = ZString.Empty;
			AssertHasMessageError(address.E2_PassportIDInfo, messageError);

			address.E2_GovRegNumType = OrgCusCode.CodeTypes.CarrierCode;
			AssertNoMessageError(address.E2_PassportIDInfo, messageError);
		}

		public void TestCheckE2_PassportCountryOfIssue()
		{
			JobDocAddress address = Factory.New<JobDocAddress>();
			address.E2_AddressOverride = true;
			address.OverrideRequirement = new JobDocAddressRequirement();
			string messageError1 = "MESSAGE ERROR 1";
			address.E2_AddressOverride = true;
			address.E2_GovRegNumType = OrgCusCode.CodeTypes.PassportID;
			address.E2_PassportCountryOfIssue = Core.Constants.CountryCodes.Australia;
			address.E2_PassportCountryOfIssue = ZString.Empty;
			AssertNoMessageError(address.E2_PassportCountryOfIssueInfo, messageError1);

			address.Requirement.ValidatePassportCountryOfIssue =
				delegate(JobDocAddressValidation validation)
				{
					validation.Parent.E2_PassportCountryOfIssueInfo.AddMessageError(messageError1);
				};
			address.E2_PassportCountryOfIssue = Core.Constants.CountryCodes.Australia;
			address.E2_PassportCountryOfIssue = ZString.Empty;
			AssertHasMessageError(address.E2_PassportCountryOfIssueInfo, messageError1);

			address.E2_GovRegNumType = OrgCusCode.CodeTypes.CarrierCode;
			AssertNoMessageError(address.E2_PassportCountryOfIssueInfo, messageError1);
		}

		public void TestCheckE2_PassportDateOfBirth()
		{
			JobDocAddress address = Factory.New<JobDocAddress>();
			address.OverrideRequirement = new JobDocAddressRequirement();
			string messageError1 = "MESSAGE ERROR 1";
			address.E2_AddressOverride = true;
			address.E2_GovRegNumType = OrgCusCode.CodeTypes.PassportID;
			address.E2_PassportDateOfBirth = ZDate.BrettsBirthday;
			address.E2_PassportDateOfBirth = ZDate.Empty;
			AssertNoMessageError(address.E2_PassportDateOfBirthInfo, messageError1);

			address.Requirement.ValidatePassportDateOfBirth =
				delegate(JobDocAddressValidation validation)
				{
					validation.Parent.E2_PassportDateOfBirthInfo.AddMessageError(messageError1);
				};
			address.E2_PassportDateOfBirth = ZDate.BrettsBirthday;
			address.E2_PassportDateOfBirth = ZDate.Empty;
			AssertHasMessageError(address.E2_PassportDateOfBirthInfo, messageError1);

			address.E2_GovRegNumType = OrgCusCode.CodeTypes.CarrierCode;
			AssertNoMessageError(address.E2_PassportDateOfBirthInfo, messageError1);
		}

		public void TestCheckE2_GovRegNumType()
		{
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			JobDocAddress address = Factory.New<JobDocAddress>();
			address.OverrideRequirement = new JobDocAddressRequirement();
			address.Requirement.ValidateGovRegNumType =
				delegate(JobDocAddressValidation validation)
				{
					if (validation.Parent.E2_GovRegNumType.IsEmpty)
					{
						validation.Parent.E2_GovRegNumTypeInfo.AddMessageError("GovRegNumType is required.");
					}
				};

			address.E2_AddressOverride = true;
			address.E2_GovRegNumType = "TET";
			AssertEquals(false, address.E2_GovRegNumTypeInfo.HasMessageErrors());

			address.E2_GovRegNumType = "";
			AssertEquals(true, address.E2_GovRegNumTypeInfo.HasMessageErrors());
		}

		public void TestValidateE2_GovRegNumType_CachesRegistrationNumber() => TestValidation_CachesRegistrationNumber(v => v.ValidateE2_GovRegNumType(), 4, hookOntoRegNum: true, hookOntoRegNumInfo: false);

		public void TestCheckE2_GovRegNum()
		{
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			JobDocAddress address = Factory.New<JobDocAddress>();
			address.OverrideRequirement = new JobDocAddressRequirement();
			address.Requirement.ValidateGovRegNo =
				delegate(JobDocAddressValidation validation)
				{
					if (validation.Parent.E2_GovRegNum.IsEmpty)
					{
						validation.Parent.E2_GovRegNumInfo.AddMessageError("GovRegNo is required.");
					}
				};

			address.E2_AddressOverride = true;
			address.E2_GovRegNum = "TEST";
			AssertEquals(false, address.E2_GovRegNumInfo.HasMessageErrors());

			address.E2_GovRegNum = "";
			AssertEquals(true, address.E2_GovRegNumInfo.HasMessageErrors());
		}

		public void TestValidateE2_GovRegNum_CachesRegistrationNumber() => TestValidation_CachesRegistrationNumber(v => v.ValidateE2_GovRegNum(), 4, hookOntoRegNum: false, hookOntoRegNumInfo: true);

		public void TestValidateAll_CachesRegistrationNumber() => TestValidation_CachesRegistrationNumber(v => v.ValidateAll(), 21);

		void TestValidation_CachesRegistrationNumber(Action<JobDocAddressValidation> validationMethod, int expectedValidationMethodsHit, bool hookOntoRegNum = true, bool hookOntoRegNumInfo = true)
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var orgaddress = organisation.Addresses.AddNew();
			orgaddress.OA_Address1 = "Add1-1";

			var cusCode = organisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, "123");
			var address = JobDocAddress.New((BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>(), DocAddressType.SupplierPickupDeliveryAddress);
			address.E2_OA_Address = orgaddress.PK;
			address.OverrideRequirement = new JobDocAddressRequirement();
			address.Requirement.GetRegistrationNumberResult =
				(jda) => new RegistrationNumberResult(Factory, true, delegate { return new RegistrationNumber { Number = cusCode.OK_CustomsRegNo, NumberType = cusCode.OK_CodeType }; });

			var validationMethodsHit = 0;
			address.E2_PassportIDInfo.AdditionalValidation += incrementAndAssert;
			address.E2_PassportCountryOfIssueInfo.AdditionalValidation += incrementAndAssert;
			address.E2_PassportDateOfBirthInfo.AdditionalValidation += incrementAndAssert;

			if (hookOntoRegNum)
			{
				address.E2_GovRegNumInfo.AdditionalValidation += incrementAndAssert;
			}

			if (hookOntoRegNumInfo)
			{
				address.E2_GovRegNumTypeInfo.AdditionalValidation += incrementAndAssert;
			}

			AssertEquals("Precondition.", OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, address.E2_GovRegNumType);
			AssertEquals("Precondition.", "123", address.E2_GovRegNum);

			validationMethod(address.Validation);
			AssertEquals("Should have hit validation methods.", expectedValidationMethodsHit, validationMethodsHit);

			AssertEquals("Should *not* have cached RegistrationNumberFromRequirement outside of the validation method.", OrgCusCode.CodeTypes.BuyerCode, address.E2_GovRegNumType);
			AssertEquals("Should *not* have cached RegistrationNumberFromRequirement outside of the validation method.", "356", address.E2_GovRegNum);

			void incrementAndAssert()
			{
				AssertEquals("Should have cached RegistrationNumberFromRequirement.", OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, address.E2_GovRegNumType);
				AssertEquals("Should have cached RegistrationNumberFromRequirement.", "123", address.E2_GovRegNum);

				address.Requirement.GetRegistrationNumberResult =
					_ => new RegistrationNumberResult(Factory, true, delegate { return new RegistrationNumber { Number = "356", NumberType = OrgCusCode.CodeTypes.BuyerCode }; });

				AssertEquals("Should have cached RegistrationNumberFromRequirement.", OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, address.E2_GovRegNumType);
				AssertEquals("Should have cached RegistrationNumberFromRequirement.", "123", address.E2_GovRegNum);

				validationMethodsHit++;
			}
		}

		public void TestValidateE2_PassportIDNoDeveloperExceptions()
		{
			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.PassportID;
			jobDocAddress.E2_AddressOverride = true;
			Factory.Save();

			var validation = new JobDocAddressValidation(jobDocAddress);

			jobDocAddress.Delete();
			validation.ValidateE2_PassportID();

			AssertEquals("There should be no developer exceptions", 0, CargoWise.Common.ErrorReporter.TotalErrorCount);
		}

		public void TestCheckOrganisationPK()
		{
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			JobDocAddress address = Factory.New<JobDocAddress>();
			address.OverrideRequirement = new JobDocAddressRequirement();
			address.Requirement.ValidateOrganisationPK =
				delegate(JobDocAddressValidation validation)
				{
					if (validation.Parent.OrganisationPK.IsEmpty)
					{
						validation.Parent.OrganisationPKInfo.AddMessageError("Organisation is required.");
					}
				};

			address.E2_AddressOverride = false;
			address.OrganisationPK = Factory.New<OrgHeader>().PK;
			AssertEquals(false, address.OrganisationPKInfo.HasMessageErrors());

			address.OrganisationPK = ZGuid.Empty;
			AssertEquals(true, address.OrganisationPKInfo.HasMessageErrors());

			address.OverrideRequirement = new JobDocAddressRequirement();
			address.Requirement.IsMandatory = true;
			string error = "Please specify a " + address.AddressCaption + " for this job.";
			address.RunPreSaveValidation();
			AssertHasError(address.OrganisationPKInfo, error);
			AssertNoMessageError(address.OrganisationPKInfo, "Only message error");
			address.Requirement.ValidateOrganisationPKUponMandatoryRequirement
				+= delegate(JobDocAddressValidation validation)
				{
					validation.Parent.OrganisationPKInfo.AddMessageError("Only message error");
				};

			address.RunPreSaveValidation();
			AssertNoError(address.OrganisationPKInfo, error);
			AssertHasMessageError(address.OrganisationPKInfo, "Only message error");
		}

		public void TestCheckOrganisationPK_ActiveOrganisationValidation()
		{
			string errorMessage = (NoResString)"This Organization is not active.";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = Factory.New<JobDocAddress>();

			address.OrganisationPK = orgHeader.PK;
			address.Validation.ValidateOrganisationPK();
			AssertNoError("Expected no validation errors with Active Organisation.", address.OrganisationPKInfo, errorMessage);

			orgHeader.OH_IsActive = false;
			address.Validation.ValidateOrganisationPK();
			AssertHasError("Expected validation error with Inactive Organisation.", address.OrganisationPKInfo, errorMessage);
		}

		public void TestCheckOrganizationPKOnDeletedJobDocAddress()
		{
			const string expectedError = "This field has been previously cleared and an underlying record has been marked for deletion. Please reopen this form to enter new value.";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = Factory.NewWithValidTestData<JobDocAddress>();

			address.OrganisationPK = orgHeader.PK;
			address.Validation.ValidateOrganisationPK();
			Factory.Save();

			AssertNoError("Expected no validation on new object.", address.OrganisationPKInfo, expectedError);

			address.Delete();
			using (((IBusinessObjectInternals)address).SuppressReportRowDeletedError())
			{
				address.Validation.ValidateOrganisationPK();
				AssertHasError("Expected error message about deleted record still used.", address.OrganisationPKInfo, expectedError);
			}
		}

		public void TestOrganisationNameOrPKValidation()
		{
			string error1 = "Enter a valid selection.";
			string error2 = "Please enter a Company Name, or remove the override for this Address.";

			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			JobDocAddress address = Factory.NewWithValidTestData<JobDocAddress>();
			address.E2_AddressOverride = false;

			address.OrganisationPK = ZGuid.Invalid;
			address.Validation.ValidateOrganisationNameOrPK();
			AssertHasError(address.OrganisationNameOrPKInfo, error1);

			address.OrganisationPK = organisation.PK;
			address.Validation.ValidateOrganisationNameOrPK();
			AssertNoError(address.OrganisationNameOrPKInfo, error1);

			address.E2_AddressOverride = true;

			address.E2_CompanyName = "Blah";
			address.Validation.ValidateOrganisationNameOrPK();
			AssertNoError(address.OrganisationNameOrPKInfo, error2);

			address.E2_CompanyName = "";
			address.Validation.ValidateOrganisationNameOrPK();
			AssertHasError(address.OrganisationNameOrPKInfo, error2);
		}

		public void TestValidationOnOverriddenFields_ClearsAutomatically()
		{
			JobDocAddress docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_AddressOverride = true;

			docAddress.E2_CompanyName = "X";
			docAddress.E2_Address1 = "X";
			docAddress.E2_City = "X";

			docAddress.E2_CompanyName = "";
			docAddress.E2_Address1 = "";
			docAddress.E2_City = "";

			AssertHasErrors(docAddress.E2_CompanyNameInfo);
			AssertHasErrors(docAddress.E2_Address1Info);
			AssertHasErrors(docAddress.E2_CityInfo);

			docAddress.E2_AddressOverride = false;
			AssertNoErrors(docAddress.E2_CompanyNameInfo);
			AssertNoErrors(docAddress.E2_Address1Info);
			AssertNoErrors(docAddress.E2_CityInfo);
		}

		public void TestValidationOnOverriddenFields()
		{
			JobDocAddress docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_AddressOverride = true;
			Assert(!string.IsNullOrEmpty(docAddress.E2_RN_NKCountryCode));
			docAddress.E2_RN_NKCountryCode = "";
			docAddress.E2_AdditionalAddressInformation = "ABC";
			docAddress.E2_Address1 = "ABC";
			docAddress.E2_City = "ABC";
			docAddress.E2_State = "ABC";
			docAddress.E2_CompanyName = "ABC";
			docAddress.E2_Email = "Richard.White@edi.com.au";
			docAddress.E2_Mobile = "+1(111)1111111";
			docAddress.E2_Fax = "+2(222)2222222";
			docAddress.E2_Phone = "+3(333)3333333";

			JobDocAddressValidation docAddressValidation = new JobDocAddressValidation(docAddress);
			AssertEquals("There should be no errors or warnings.", false, docAddress.E2_AdditionalAddressInformationInfo.HasErrors() || docAddress.E2_Address1Info.HasWarnings());
			AssertEquals("There should be no errors or warnings.", false, docAddress.E2_Address1Info.HasErrors() || docAddress.E2_Address1Info.HasWarnings());
			AssertEquals("There should be no errors or warnings.", false, docAddress.E2_CityInfo.HasErrors() || docAddress.E2_CityInfo.HasWarnings());
			Assert("Should have warnings.", !docAddress.E2_StateInfo.HasErrors() && docAddress.E2_StateInfo.HasWarnings());
			AssertEquals("There should be no errors or warnings.", false, docAddress.E2_CompanyNameInfo.HasErrors() || docAddress.E2_CompanyNameInfo.HasWarnings());
			AssertEquals("There should be no errors or warnings.", false, docAddress.E2_EmailInfo.HasErrors() || docAddress.E2_EmailInfo.HasWarnings());
			AssertEquals("There should be no errors or warnings.", false, docAddress.E2_MobileInfo.HasErrors() || docAddress.E2_MobileInfo.HasWarnings());
			AssertEquals("There should be no errors or warnings.", false, docAddress.E2_FaxInfo.HasErrors() || docAddress.E2_FaxInfo.HasWarnings());
			AssertEquals("There should be no errors or warnings.", false, docAddress.E2_PhoneInfo.HasErrors() || docAddress.E2_PhoneInfo.HasWarnings());

			docAddress.E2_AdditionalAddressInformation = "";
			docAddress.E2_Address1 = "";
			docAddress.E2_City = "";
			docAddress.E2_State = "";
			docAddress.E2_CompanyName = "";
			docAddress.E2_Email = "RichardWhitegonebad";

			byte[] arabicBytes = new byte[10];
			for (byte b = 0; b < arabicBytes.Length; b++)
			{
				arabicBytes[b] = (byte)(b + 128);
			}

			char[] arabicChars = Encoding.GetEncoding(1256).GetChars(arabicBytes);

			docAddress.E2_Mobile = new String(arabicChars);
			docAddress.E2_Fax = new String(arabicChars);
			docAddress.E2_Phone = new String(arabicChars);

			AssertEquals("There should not be an error.", false, docAddress.E2_AdditionalAddressInformationInfo.HasErrors());
			AssertEquals("There should be an error.", true, docAddress.E2_Address1Info.HasErrors());
			AssertEquals("There should be an error.", true, docAddress.E2_CityInfo.HasErrors());
			AssertEquals("There should not be an error.", false, docAddress.E2_StateInfo.HasErrors());
			AssertEquals("There should be an error.", true, docAddress.E2_CompanyNameInfo.HasErrors());
			AssertEquals("There should be an error.", true, docAddress.E2_EmailInfo.HasErrors());
			AssertEquals("There should be an error.", true, docAddress.E2_MobileInfo.HasErrors());
			AssertEquals("There should be an error.", true, docAddress.E2_FaxInfo.HasErrors());
			AssertEquals("There should be an error.", true, docAddress.E2_PhoneInfo.HasErrors());

			docAddress.E2_State = "ABCdefghi!!";
			AssertEquals("There should be a warning.", true, docAddress.E2_StateInfo.HasWarnings());

			RefCountry au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			docAddress.E2_RN_NKCountryCode = "AU";

			docAddress.E2_AddressOverride = false;
			docAddress.E2_Mobile = new String(arabicChars);
			docAddress.E2_Fax = new String(arabicChars);
			docAddress.E2_Phone = new String(arabicChars);

			AssertEquals("There should be an error", true, docAddress.E2_MobileInfo.HasErrors());
			AssertEquals("There should be an error", true, docAddress.E2_FaxInfo.HasErrors());
			AssertEquals("There should be an error", true, docAddress.E2_PhoneInfo.HasErrors());
		}

		public void TestE2_CompanyName()
		{
			JobDocAddress docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_AddressOverride = true;
			docAddress.E2_CompanyName = "ABC";
			AssertEquals("There should be no errors.", false, docAddress.E2_CompanyNameInfo.HasErrors());
			AssertNoMessageError(docAddress.E2_CompanyNameInfo, "CompanyName is not set");

			docAddress.E2_CompanyName = "";
			AssertEquals("There should be errors.", true, docAddress.E2_CompanyNameInfo.HasErrors());

			docAddress.OverrideRequirement = new JobDocAddressRequirement();
			docAddress.Requirement.ValidateCompanyName
				+= delegate(JobDocAddressValidation validation)
				{
					if (validation.Parent.E2_CompanyName.IsEmpty)
					{
						validation.Parent.E2_CompanyNameInfo.AddMessageError("CompanyName is not set");
					}
				};

			docAddress.E2_CompanyName = "ABC";
			docAddress.E2_CompanyName = "";
			AssertEquals("There should be no errors.", false, docAddress.E2_CompanyNameInfo.HasErrors());
			AssertHasMessageError(docAddress.E2_CompanyNameInfo, "CompanyName is not set");
		}

		public void TestCheckE2_AdditionalAddressInformation()
		{
			JobDocAddress docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_AddressOverride = true;

			docAddress.UnrestrictedAdditionalAddressInformation = string.Empty;
			AssertEquals("VALID: Empty value", false, docAddress.E2_AdditionalAddressInformationInfo.HasErrors());

			docAddress.UnrestrictedAdditionalAddressInformation = new string('0', docAddress.E2_AdditionalAddressInformationInfo.MaxLength - 1);
			AssertEquals("VALID: Value length less than max length", false, docAddress.E2_AdditionalAddressInformationInfo.HasErrors());

			docAddress.UnrestrictedAdditionalAddressInformation = new string('0', docAddress.E2_AdditionalAddressInformationInfo.MaxLength);
			AssertEquals("VALID: Value length equal to max length", false, docAddress.E2_AdditionalAddressInformationInfo.HasErrors());

			var messageError = $"This field contains text which is longer than allowed. Please shorten the text to {docAddress.E2_AdditionalAddressInformationInfo.MaxLength} letters or less before trying to save the address again.";
			docAddress.UnrestrictedAdditionalAddressInformation = new string('0', docAddress.E2_AdditionalAddressInformationInfo.MaxLength + 1);
			AssertEquals("INVALID: Value length more than max length - has an error", true, docAddress.E2_AdditionalAddressInformationInfo.HasErrors());
			AssertHasError("INVALID: Value length more than max length - message is correct", docAddress.E2_AdditionalAddressInformationInfo, messageError);
		}

		public void TestE2_Address1()
		{
			JobDocAddress docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_AddressOverride = true;
			docAddress.E2_Address1 = "ABC";
			AssertEquals("There should be no errors.", false, docAddress.E2_Address1Info.HasErrors());

			docAddress.E2_Address1 = "";
			AssertEquals("There should be errors.", true, docAddress.E2_Address1Info.HasErrors());
			AssertNoMessageError(docAddress.E2_Address1Info, "Address1 is not set");

			docAddress.OverrideRequirement = new JobDocAddressRequirement();
			docAddress.Requirement.ValidateAddress1
				+= delegate(JobDocAddressValidation validation)
				{
					if (validation.Parent.E2_Address1.IsEmpty)
					{
						validation.Parent.E2_Address1Info.AddMessageError("Address1 is not set");
					}
				};

			docAddress.E2_Address1 = "ABC";
			docAddress.E2_Address1 = "";
			AssertEquals("There should be no errors.", false, docAddress.E2_Address1Info.HasErrors());
			AssertHasMessageError(docAddress.E2_Address1Info, "Address1 is not set");
		}

		public void TestE2_City_Registry()
		{
			JobDocAddress docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_AddressOverride = true;
			docAddress.E2_City = "ABC";

			AssertEquals("There should be no errors.", false, docAddress.E2_CityInfo.HasErrors());

			docAddress.E2_City = "";
			AssertEquals("There should be errors.", true, docAddress.E2_CityInfo.HasErrors());

			docAddress.E2_City = "blah";
			AssertEquals("There should be no errors.", false, docAddress.E2_CityInfo.HasErrors());
			RawDataRegistry.Instance.JobAddressValidation_CityMandatory.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			docAddress.E2_City = "";
			AssertEquals("There should be no errors as registry validation disabled.", false, docAddress.E2_CityInfo.HasErrors());

			RawDataRegistry.Instance.JobAddressValidation_CityMandatory.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			docAddress.E2_City = "blah";
			docAddress.E2_City = "";
			AssertEquals("There should be errors.", true, docAddress.E2_CityInfo.HasErrors());
			AssertNoMessageError(docAddress.E2_CityInfo, "City is not set");

			docAddress.OverrideRequirement = new JobDocAddressRequirement();
			docAddress.Requirement.ValidateCity
				+= delegate(JobDocAddressValidation validation)
				{
					if (validation.Parent.E2_City.IsEmpty)
					{
						validation.Parent.E2_CityInfo.AddMessageError("City is not set");
					}
				};

			docAddress.E2_City = "blah";
			docAddress.E2_City = "";
			AssertEquals("There should be no errors.", false, docAddress.E2_CityInfo.HasErrors());
			AssertHasMessageError(docAddress.E2_CityInfo, "City is not set");
		}

		public void TestE2_State_Registry()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = false;
			JobDocAddress docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_AddressOverride = true;
			docAddress.E2_State = "ABC";
			docAddress.E2_RN_NKCountryCode = "AU";

			AssertEquals("There should be errors.", true, docAddress.E2_StateInfo.HasErrors());

			docAddress.E2_State = "";
			AssertEquals("There should be errors.", true, docAddress.E2_StateInfo.HasErrors());

			docAddress.E2_State = "blah";
			AssertEquals("There should be errors.", true, docAddress.E2_StateInfo.HasErrors());

			RawDataRegistry.Instance.JobAddressValidation_UseStateRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			docAddress.E2_State = "";
			AssertEquals("There should be no errors as registry validation disabled.", false, docAddress.E2_StateInfo.HasErrors());

			RawDataRegistry.Instance.JobAddressValidation_UseStateRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			docAddress.E2_State = "blah";
			docAddress.E2_State = "";
			AssertEquals("There should be errors.", true, docAddress.E2_StateInfo.HasErrors());
			AssertNoMessageError(docAddress.E2_StateInfo, "State is not set");

			docAddress.OverrideRequirement = new JobDocAddressRequirement();
			docAddress.Requirement.ValidateState
				+= delegate(JobDocAddressValidation validation)
				{
					if (validation.Parent.E2_State.IsEmpty)
					{
						validation.Parent.E2_StateInfo.AddMessageError("State is not set");
					}
				};

			docAddress.E2_State = "blah";
			docAddress.E2_State = "";
			AssertEquals("There should be no errors.", false, docAddress.E2_StateInfo.HasErrors());
			AssertHasMessageError(docAddress.E2_StateInfo, "State is not set");
		}

		public void TestE2_Postcode_Registry()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = false;
			JobDocAddress docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_AddressOverride = true;
			docAddress.E2_Postcode = "ABC";
			docAddress.E2_RN_NKCountryCode = "AU";

			AssertEquals("There should be no errors.", false, docAddress.E2_PostcodeInfo.HasErrors());

			docAddress.E2_Postcode = "";
			AssertEquals("There should be errors.", true, docAddress.E2_PostcodeInfo.HasErrors());

			docAddress.E2_Postcode = "blah";
			AssertEquals("There should be no errors.", false, docAddress.E2_PostcodeInfo.HasErrors());
			RawDataRegistry.Instance.JobAddressValidation_UsePostcodeRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			docAddress.E2_Postcode = "";
			AssertEquals("There should be no errors as registry validation disabled.", false, docAddress.E2_PostcodeInfo.HasErrors());

			RawDataRegistry.Instance.JobAddressValidation_UsePostcodeRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			docAddress.E2_Postcode = "blah";
			docAddress.E2_Postcode = "";
			AssertEquals("There should be errors.", true, docAddress.E2_PostcodeInfo.HasErrors());
			AssertNoMessageError(docAddress.E2_PostcodeInfo, "Postcode is not set");

			docAddress.OverrideRequirement = new JobDocAddressRequirement();
			docAddress.Requirement.ValidatePostCode
				+= delegate(JobDocAddressValidation validation)
				{
					if (validation.Parent.E2_Postcode.IsEmpty)
					{
						validation.Parent.E2_PostcodeInfo.AddMessageError("Postcode is not set");
					}
				};

			docAddress.E2_Postcode = "blah";
			docAddress.E2_Postcode = "";
			AssertEquals("There should be no errors.", false, docAddress.E2_PostcodeInfo.HasErrors());
			AssertHasMessageError(docAddress.E2_PostcodeInfo, "Postcode is not set");
		}

		public void TestE2_OA_AddressValidation()
		{
			JobDocAddress docAddress = Factory.New<JobDocAddress>();
			OrgHeader orgH = Factory.New<OrgHeader>();
			OrgAddress orgA = orgH.Addresses.AddNew();
			OrgAddress orgA2 = orgH.Addresses.AddNew();
			orgA.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery.Code);
			orgA.AddressCapability.GetIsMainAddress(OrgAddressType.Delivery.Code);
			orgA2.OA_IsActive = ZBool.False;
			docAddress.E2_OA_Address = orgA.PK;
			docAddress.E2_ParentID = ZGuid.NewZGuid();
			docAddress.DefaultAddressType = AddressType.DLV;
			docAddress.DocAddressType = DocAddressType.None;
			docAddress.RunPreSaveValidation();
			AssertEquals("DocAddress acting as pivot shouldn't validate OrgAddress.", false, docAddress.HasErrors);

			docAddress.E2_OA_Address = ZGuid.Empty;
			docAddress.RunPreSaveValidation();
			AssertEquals("DocAddress was set initially with an OrgAddress - OH will not be stored if OA is cleared", ZGuid.Empty, docAddress.OrganisationPK);
			AssertEquals("DocAddress should not have any errors.", false, docAddress.HasErrors);

			docAddress.OrganisationPK = orgH.PK;
			docAddress.RunPreSaveValidation();
			AssertEquals("DocAddress set from an OH - OA should have defaulted.", orgA.PK, docAddress.E2_OA_Address);
			AssertEquals("DocAddress should not have any errors.", false, docAddress.HasErrors);

			docAddress.E2_OA_Address = ZGuid.Empty;
			docAddress.RunPreSaveValidation();
			AssertEquals("DocAddress has stored the OH because it was set with this", orgH.PK, docAddress.OrganisationPK);
			AssertEquals("DocAddress - Address has been cleared.", ZGuid.Empty, docAddress.E2_OA_Address);
			AssertEquals("DocAddress should have any errors - Can't have an org with no address", true, docAddress.HasErrors);

			docAddress.OverrideRequirement = new JobDocAddressRequirement();
			docAddress.Requirement.ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address
				+= delegate(JobDocAddressValidation validation)
				{
					if (validation.Parent.E2_OA_Address != orgA.PK)
					{
						validation.Parent.E2_OA_AddressInfo.AddMessageError("Address is not OrgA");
					}
				};

			docAddress.RunPreSaveValidation();
			AssertHasMessageError(docAddress.E2_OA_AddressInfo, "Address is not OrgA");

			docAddress.E2_AddressOverride = true;
			docAddress.RunPreSaveValidation();
			AssertNoMessageError(docAddress.E2_OA_AddressInfo, "Address is not OrgA");
		}

		public void TestAddressTypeValidation()
		{
			JobDocAddress docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_AddressType = DocAddressTypes.GetCode(Factory, DocAddressType.LocalCartageYard);
			AssertEquals("There should be no errors or warnings.", false, docAddress.E2_AddressTypeInfo.HasErrors() || docAddress.E2_AddressTypeInfo.HasWarnings());

			docAddress.E2_AddressType = DocAddressTypes.GetCode(Factory, DocAddressType.None);
			AssertEquals("There should be a warning.", true, docAddress.E2_AddressTypeInfo.HasWarnings());

			docAddress.E2_AddressType = "!23";
			AssertEquals("There should be an error.", true, docAddress.E2_AddressTypeInfo.HasErrors());

			docAddress.E2_AddressType = DocAddressTypes.GetCode(Factory, DocAddressType.ImporterPickupDeliveryAddress);
			AssertEquals("There should be no errors or warnings.", false, docAddress.E2_AddressTypeInfo.HasErrors() || docAddress.E2_AddressTypeInfo.HasWarnings());
		}

		public void TestValidateResidentialCommercialAddressType()
		{
			JobDocAddress address = Factory.NewWithValidTestData<JobDocAddress>();
			address.ResidentialCommercialAddressType = "AAA";
			address.Validation.ValidateResidentialCommercialAddressType();
			AssertHasErrors("Value stays invalid, and should have errors", address.ResidentialCommercialAddressTypeInfo);

			address.ResidentialCommercialAddressType = ResidentialCommercialAddressTypeList.Codes.Residential;
			address.Validation.ValidateResidentialCommercialAddressType();
			AssertNoErrors("Value is valid, should not have errors", address.ResidentialCommercialAddressTypeInfo);
		}

		public void TestAddressValidationIgnoresBaseAddress()
		{
			JobDocAddress docAddress = Factory.New<JobDocAddress>();
			OrgHeader orgH = Factory.New<OrgHeader>();
			OrgAddress orgA = orgH.Addresses.AddNew();
			docAddress.E2_OA_Address = orgA.PK;
			docAddress.E2_ParentID = ZGuid.NewZGuid();
			docAddress.DocAddressType = DocAddressType.None;
			docAddress.RunPreSaveValidation();
			AssertEquals("DocAddress acting as pivot shouldn't validate OrgAddress.", false, docAddress.HasErrors);
		}

		public void TestE2_ValidationStatus()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());
			var parentForTest = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var docAddress = JobDocAddress.New(parentForTest);
			docAddress.E2_AddressOverride = true;
			docAddress.IgnoreValidationStatusError = false;
			docAddress.E2_RN_NKCountryCode = "AU";

			using (OrganisationsDataRegistry.Instance.AddressValidationServiceSuppression.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressListCollection()))
			{
				docAddress.E2_ValidationStatus = AddressValidationStatus.Invalid;
				AssertHasError(docAddress.E2_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");

				docAddress.E2_ValidationStatus = AddressValidationStatus.Verified;
				AssertNoError(docAddress.E2_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");

				docAddress.IgnoreValidationStatusError = true;
				docAddress.E2_ValidationStatus = AddressValidationStatus.Invalid;
				AssertNoError(docAddress.E2_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");

				docAddress.IgnoreValidationStatusError = false;
				Env.Instance.Registry.EnableAddressValidationWebService = false;
				docAddress.E2_ValidationStatus = AddressValidationStatus.Invalid;
				AssertNoError(docAddress.E2_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");

				Env.Instance.Registry.EnableAddressValidationWebService = true;
				docAddress.RunPreSaveValidation();
				AssertHasError(docAddress.E2_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");

				Globals.IsWeb = true;
				docAddress.RunPreSaveValidation();
				AssertNoError(docAddress.E2_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
			}
		}

		public void TestShouldSuppressErrorJobDocAddress_IsTrue_WhenJobDocAddressIsOnRegistry()
		{
			var rawRegistryValue = Env.Instance.Registry.EnableAddressValidationWebService;

			try
			{
				OrganisationsDataRegistry.Instance.IsErrorSuppressionEnabled = false;

				Env.Instance.Registry.EnableAddressValidationWebService = true;

				var parentForTest = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
				var docAddress = JobDocAddress.New(parentForTest);
				docAddress.DocAddressType = DocAddressType.ConsignorDocumentaryAddress;

				docAddress.E2_AddressOverride = true;
				docAddress.IgnoreValidationStatusError = false;

				using (OrganisationsDataRegistry.Instance.AddressValidationServiceSuppression.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressListCollection()))
				{
					docAddress.E2_ValidationStatus = AddressValidationStatus.Invalid;

					AssertHasError(docAddress.E2_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
				}

				var jobAddressValidation = new JobDocAddressValidation(docAddress);
				var addressValidationServiceSuppressionRegistry = new AddressListCollection();
				var registryItem = addressValidationServiceSuppressionRegistry.AddNew();
				registryItem.ControllerName = "JobConsol";
				registryItem.AddressType = docAddress.DocAddressType.ToString();

				using (OrganisationsDataRegistry.Instance.AddressValidationServiceSuppression.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, addressValidationServiceSuppressionRegistry))
				{
					docAddress.RunPreSaveValidation();

					CombineAssertions(() =>
					{
						AssertNoError(docAddress.E2_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
						AssertHasWarning(docAddress.E2_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
					});
				}
			}
			finally
			{
				Env.Instance.Registry.EnableAddressValidationWebService = rawRegistryValue;
			}
		}

		public void TestShouldSuppressErrorJobDocAddress_IsFalse_WhenJobDocAddressIsOnRegistryButEnvironmentIsCWService()
		{
			var rawRegistryValue = Env.Instance.Registry.EnableAddressValidationWebService;

			try
			{
				OrganisationsDataRegistry.Instance.IsErrorSuppressionEnabled = false;

				Env.Instance.Registry.EnableAddressValidationWebService = true;

				var parentForTest = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
				var docAddress = JobDocAddress.New(parentForTest);
				docAddress.DocAddressType = DocAddressType.ConsignorDocumentaryAddress;

				docAddress.E2_AddressOverride = true;
				docAddress.IgnoreValidationStatusError = false;
				docAddress.E2_ValidationStatus = AddressValidationStatus.Invalid;

				var addressValidationServiceSuppressionRegistry = new AddressListCollection();
				var registryItem = addressValidationServiceSuppressionRegistry.AddNew();
				registryItem.ControllerName = "JobConsol";
				registryItem.AddressType = docAddress.DocAddressType.ToString();

				using (OrganisationsDataRegistry.Instance.AddressValidationServiceSuppression.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, addressValidationServiceSuppressionRegistry))
				{
					docAddress.RunPreSaveValidation();

					CombineAssertions(() =>
					{
						AssertNoError(docAddress.E2_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
						AssertHasWarning(docAddress.E2_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
					});
				}

				using (EnvProxy.Instance.TemporaryServiceTaskContext("BAV", canRunInAnyBranch: false))
				using (OrganisationsDataRegistry.Instance.AddressValidationServiceSuppression.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, addressValidationServiceSuppressionRegistry))
				{
					docAddress.RunPreSaveValidation();

					CombineAssertions(() =>
					{
						AssertHasError(docAddress.E2_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
					});
				}
			}
			finally
			{
				Env.Instance.Registry.EnableAddressValidationWebService = rawRegistryValue;
			}
		}

		public void TestShouldSuppressErrorJobDocAddress_IsFalse_WhenControllerIsNotOnRegistry()
		{
			var rawRegistryValue = Env.Instance.Registry.EnableAddressValidationWebService;

			try
			{
				OrganisationsDataRegistry.Instance.IsErrorSuppressionEnabled = false;
				Env.Instance.Registry.EnableAddressValidationWebService = true;

				var parentForTest = (BusinessObject)Factory.New<Freight.LocalCartage.Integration.ICommonCartage>();
				var docAddress = JobDocAddress.New(parentForTest);
				docAddress.DocAddressType = DocAddressType.ConsignorDocumentaryAddress;

				docAddress.E2_AddressOverride = true;
				docAddress.IgnoreValidationStatusError = false;

				using (OrganisationsDataRegistry.Instance.AddressValidationServiceSuppression.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressListCollection()))
				{
					docAddress.E2_ValidationStatus = AddressValidationStatus.Invalid;

					AssertHasError(docAddress.E2_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
				}

				var addressValidationServiceSuppressionRegistry = new AddressListCollection();
				var registryItem = addressValidationServiceSuppressionRegistry.AddNew();
				registryItem.ControllerName = "JobShipment";
				registryItem.AddressType = docAddress.DocAddressType.ToString();

				using (OrganisationsDataRegistry.Instance.AddressValidationServiceSuppression.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, addressValidationServiceSuppressionRegistry))
				{
					docAddress.RunPreSaveValidation();

					CombineAssertions(() =>
					{
						AssertHasError(docAddress.E2_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
						AssertNoWarning(docAddress.E2_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
					});
				}
			}
			finally
			{
				Env.Instance.Registry.EnableAddressValidationWebService = rawRegistryValue;
			}
		}

		public void TestShouldSuppressErrorJobDocAddress_IsFalse_WhenAddressTypeIsNotOnRegistry()
		{
			var rawRegistryValue = Env.Instance.Registry.EnableAddressValidationWebService;

			try
			{
				OrganisationsDataRegistry.Instance.IsErrorSuppressionEnabled = false;
				Env.Instance.Registry.EnableAddressValidationWebService = true;

				var parentForTest = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
				var docAddress = JobDocAddress.New(parentForTest);
				docAddress.DocAddressType = DocAddressType.ConsignorDocumentaryAddress;

				docAddress.E2_AddressOverride = true;
				docAddress.IgnoreValidationStatusError = false;

				using (OrganisationsDataRegistry.Instance.AddressValidationServiceSuppression.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressListCollection()))
				{
					docAddress.E2_ValidationStatus = AddressValidationStatus.Invalid;

					AssertHasError(docAddress.E2_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
				}

				var addressValidationServiceSuppressionRegistry = new AddressListCollection();
				var registryItem = addressValidationServiceSuppressionRegistry.AddNew();
				registryItem.ControllerName = "JobConsol";
				registryItem.AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress);

				using (OrganisationsDataRegistry.Instance.AddressValidationServiceSuppression.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, addressValidationServiceSuppressionRegistry))
				{
					docAddress.RunPreSaveValidation();

					CombineAssertions(() =>
					{
						AssertHasError(docAddress.E2_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
						AssertNoWarning(docAddress.E2_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
					});
				}
			}
			finally
			{
				Env.Instance.Registry.EnableAddressValidationWebService = rawRegistryValue;
			}
		}

		public void TestShouldSuppressErrorJobDocAddress_IsTrue_WhenAllAddressTypesWithAllControllers()
		{
			var rawRegistryValue = Env.Instance.Registry.EnableAddressValidationWebService;

			try
			{
				OrganisationsDataRegistry.Instance.IsErrorSuppressionEnabled = false;
				Env.Instance.Registry.EnableAddressValidationWebService = true;

				var parentForTest = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();

				var docAddress = JobDocAddress.New(parentForTest);
				docAddress.DocAddressType = DocAddressType.ConsignorDocumentaryAddress;
				docAddress.E2_AddressOverride = true;
				docAddress.IgnoreValidationStatusError = false;

				using (OrganisationsDataRegistry.Instance.AddressValidationServiceSuppression.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressListCollection()))
				{
					docAddress.E2_ValidationStatus = AddressValidationStatus.Invalid;

					AssertHasError(docAddress.E2_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
				}

				var addressValidationServiceSuppressionRegistry = new AddressListCollection();
				var registryItem = addressValidationServiceSuppressionRegistry.AddNew();
				registryItem.ControllerName = Constants.AVSRegistryConstants.ControllerNames.All;
				registryItem.AddressType = Constants.AVSRegistryConstants.AddressTypes.All;

				using (OrganisationsDataRegistry.Instance.AddressValidationServiceSuppression.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, addressValidationServiceSuppressionRegistry))
				{
					docAddress.RunPreSaveValidation();

					CombineAssertions(() =>
					{
						AssertNoError(docAddress.E2_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
						AssertHasWarning(docAddress.E2_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
					});
				}
			}
			finally
			{
				Env.Instance.Registry.EnableAddressValidationWebService = rawRegistryValue;
			}
		}

		public void TestShouldSuppressErrorJobDocAddress_IsTrue_WhenAddressTypeIsOnRegistryWithAllControllers()
		{
			var rawRegistryValue = Env.Instance.Registry.EnableAddressValidationWebService;

			try
			{
				OrganisationsDataRegistry.Instance.IsErrorSuppressionEnabled = false;
				Env.Instance.Registry.EnableAddressValidationWebService = true;

				var parentForTest = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();

				var docAddress = JobDocAddress.New(parentForTest);
				docAddress.DocAddressType = DocAddressType.ConsignorDocumentaryAddress;
				docAddress.E2_AddressOverride = true;
				docAddress.IgnoreValidationStatusError = false;

				using (OrganisationsDataRegistry.Instance.AddressValidationServiceSuppression.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressListCollection()))
				{
					docAddress.E2_ValidationStatus = AddressValidationStatus.Invalid;

					AssertHasError(docAddress.E2_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
				}

				var addressValidationServiceSuppressionRegistry = new AddressListCollection();
				var registryItem = addressValidationServiceSuppressionRegistry.AddNew();
				registryItem.ControllerName = Constants.AVSRegistryConstants.ControllerNames.All;
				registryItem.AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress);

				using (OrganisationsDataRegistry.Instance.AddressValidationServiceSuppression.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, addressValidationServiceSuppressionRegistry))
				{
					docAddress.RunPreSaveValidation();

					CombineAssertions(() =>
					{
						AssertNoError(docAddress.E2_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
						AssertHasWarning(docAddress.E2_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
					});
				}
			}
			finally
			{
				Env.Instance.Registry.EnableAddressValidationWebService = rawRegistryValue;
			}
		}

		public void TestShouldSuppressErrorJobDocAddress_IsTrue_WhenControllerIsOnRegistryWithAllAddressTypes()
		{
			var rawRegistryValue = Env.Instance.Registry.EnableAddressValidationWebService;

			try
			{
				OrganisationsDataRegistry.Instance.IsErrorSuppressionEnabled = false;
				Env.Instance.Registry.EnableAddressValidationWebService = true;

				var parentForTest = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();

				var docAddress = JobDocAddress.New(parentForTest);
				docAddress.DocAddressType = DocAddressType.ConsignorDocumentaryAddress;
				docAddress.E2_AddressOverride = true;
				docAddress.IgnoreValidationStatusError = false;

				using (OrganisationsDataRegistry.Instance.AddressValidationServiceSuppression.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressListCollection()))
				{
					docAddress.E2_ValidationStatus = AddressValidationStatus.Invalid;

					AssertHasError(docAddress.E2_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
				}

				var addressValidationServiceSuppressionRegistry = new AddressListCollection();
				var registryItem = addressValidationServiceSuppressionRegistry.AddNew();
				registryItem.ControllerName = "JobConsol";
				registryItem.AddressType = Constants.AVSRegistryConstants.AddressTypes.All;

				using (OrganisationsDataRegistry.Instance.AddressValidationServiceSuppression.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, addressValidationServiceSuppressionRegistry))
				{
					docAddress.RunPreSaveValidation();

					CombineAssertions(() =>
					{
						AssertNoError(docAddress.E2_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
						AssertHasWarning(docAddress.E2_ValidationStatusInfo, "There is an invalid address recorded on this job. Please correct the address using the validation service or select 'Accept as Entered' from the suggestion box to verify it manually. The suggestion box is accessed by clicking the envelope icon next to the address.");
					});
				}
			}
			finally
			{
				Env.Instance.Registry.EnableAddressValidationWebService = rawRegistryValue;
			}
		}

		public void TestE2_ValidationStatusNoOverride()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			JobDocAddress docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			OrgAddress orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			docAddress.E2_AddressOverride = false;
			docAddress.E2_OA_Address = orgAddress.PK;
			orgAddress.OA_RN_NKCountryCode = "AU";

			orgAddress.OA_ValidationStatus = AddressValidationStatus.Verified;
			AssertEquals("Not expecting an error on E2_ValidationStatusInfo", false, docAddress.E2_ValidationStatusInfo.HasErrors());

			orgAddress.OA_ValidationStatus = AddressValidationStatus.Invalid;
			AssertEquals("Not expecting an error on E2_ValidationStatusInfo", false, docAddress.E2_ValidationStatusInfo.HasErrors());
		}

		public void TestTestE2_ValidationStatus_WhenBookingSendingXUSToCTO()
		{
			var booking = Factory.NewWithValidTestData(ObjectFactory.GetType<IDtbBooking>());
			var instruction = (BusinessObject)Factory.New<IDtbBookingInstruction>();
			instruction[DtbBookingInstructionSchema.KN_KM_BookingMovement] = booking.PK;
			var docAddress = (JobDocAddress)((IDtbBookingInstruction)instruction).Address;
			docAddress.ValidationStatus = AddressValidationStatus.Invalid;
			docAddress.Validation.ValidateE2_ValidationStatus();

			AssertEquals("Precondition: IsSendingXUSToCTO is false", false, ((IDtbBooking)booking).IsSendingXUSToCTO);
			AssertNoNotifications("Precondition: Address Validation Status doesn't have any kind of notification as ValidateForSendingXUSToCTO has not been run", docAddress.E2_ValidationStatusInfo);
			booking[DtbBookingSchema.KM_Status] = "ACR"; // TransportStatuses.Codes.ActionRequired. Simulate ValidateForSendingXUSToCTO being run.
			AssertEquals("Precondition: IsSendingXUSToCTO is true", true, ((IDtbBooking)booking).IsSendingXUSToCTO);
			docAddress.Validation.ValidateE2_ValidationStatus();

			AssertHasMessageError("Should have message error for Address Validation Status", docAddress.E2_ValidationStatusInfo, "This Booking must have valid Addresses entered for all Instructions.");

			docAddress.ValidationStatus = AddressValidationStatus.Verified;

			AssertNoNotifications("Address Validation Status should not have any kind of notification", docAddress.E2_ValidationStatusInfo);
		}

		public void TestE2_RN_NKCountryCodeValidation()
		{
			JobDocAddress docAddress = Factory.New<JobDocAddress>();

			docAddress.E2_RN_NKCountryCode = Factory.LoadTop1<RefCountry>(new ZQuery()).RN_Code;
			AssertEquals("Valid RefCountry selected, no error on E2_RN_NKCountryCode.", false, docAddress.E2_RN_NKCountryCodeInfo.HasErrors());

			docAddress.E2_RN_NKCountryCode = "00";
			AssertEquals("Invalid RefCountry selected, but not overriden - so no error E2_RN_NKCountryCode.", false, docAddress.E2_RN_NKCountryCodeInfo.HasErrors());

			docAddress.E2_AddressOverride = true;

			RawDataRegistry.Instance.JobAddressValidation_CountryMandatory.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			docAddress.E2_RN_NKCountryCode = "";
			AssertEquals("No RefCountry selected but registry set to false, no error on E2_RN_NKCountryCode.", false, docAddress.E2_RN_NKCountryCodeInfo.HasErrors());

			RawDataRegistry.Instance.JobAddressValidation_CountryMandatory.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			docAddress.E2_RN_NKCountryCode = "a";
			docAddress.E2_RN_NKCountryCode = "";
			AssertEquals("No RefCountry selected and registry set to true, so error on E2_RN_NKCountryCode.", true, docAddress.E2_RN_NKCountryCodeInfo.HasErrors());

			docAddress.E2_RN_NKCountryCode = Factory.LoadTop1<RefCountry>(new ZQuery()).RN_Code;
			AssertEquals("Valid RefCountry selected, no error on E2_RN_NKCountryCode.", false, docAddress.E2_RN_NKCountryCodeInfo.HasErrors());

			docAddress.E2_RN_NKCountryCode = "00";
			AssertEquals("Invalid RefCountry selected, E2_RN_NKCountryCode should have an error.", true, docAddress.E2_RN_NKCountryCodeInfo.HasErrors());
		}

		public void TestOrganizationLinkedToUnMatchedOrg_ValidationWarning()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "UNMATCHED"));
			AssertNotNull("Precondition", org);

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			AssertNoWarnings(docAddress.OrganisationPKInfo);

			docAddress.OrganisationPK = org.PK;
			AssertHasWarning(docAddress.OrganisationPKInfo, "We recommend selecting an existing Organization or creating a new Organization with the details in the 'Unmatched Org Details' note.");
		}

		#region TestE2_AddressOverrideValidation

		public void TestE2_AddressOverrideValidation()
		{
			DummyWithDocAddress dummy = Factory.New<DummyWithDocAddress>();
			JobDocAddress docAddress = JobDocAddress.New(dummy);

			dummy.AllowAddressOverride = true;
			docAddress.E2_AddressOverride = false;
			AssertEquals("Address not overridden, should be no error.", false, docAddress.E2_AddressOverrideInfo.HasErrors());

			dummy.AllowAddressOverride = false;
			docAddress.E2_AddressOverride = true; // to force validation on next set
			docAddress.E2_AddressOverride = false;
			AssertEquals("Address not overridden, should be no error.", false, docAddress.E2_AddressOverrideInfo.HasErrors());

			docAddress.E2_AddressOverride = true;
			AssertEquals("Address is overridden but not allowed, should be an error.", true, docAddress.E2_AddressOverrideInfo.HasErrors());

			dummy.AllowAddressOverride = true;
			docAddress.E2_AddressOverride = false; // to force validation on next set
			docAddress.E2_AddressOverride = true;
			AssertEquals("Address is overridden and allowed, should be no error.", false, docAddress.E2_AddressOverrideInfo.HasErrors());

			docAddress.OverrideRequirement = new JobDocAddressRequirement();
			docAddress.Requirement.ValidateDelegateToBeFiredUponValidationOf_E2_AddressOverride
				+= delegate(JobDocAddressValidation validation)
				{
					if (validation.Parent.E2_AddressOverride)
					{
						validation.Parent.E2_AddressOverrideInfo.AddMessageError("Address is override");
					}
				};

			docAddress.E2_AddressOverride = false;
			AssertNoMessageError(docAddress.E2_AddressOverrideInfo, "Address is override");

			docAddress.E2_AddressOverride = true;
			AssertHasMessageError(docAddress.E2_AddressOverrideInfo, "Address is override");
		}

		#endregion

		#region TestE2_ScreeningStatusValidation

		public void TestE2_ScreeningStatusValidation()
		{
			var docAddress = Factory.New<JobDocAddress>();
			AssertNoErrors(docAddress.E2_ScreeningStatusInfo);

			docAddress.E2_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			AssertNoErrors(docAddress.E2_ScreeningStatusInfo);

			docAddress.E2_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			AssertNoErrors(docAddress.E2_ScreeningStatusInfo);

			docAddress.E2_ScreeningStatus = ScreeningStatusesList.Codes.Canceled;
			AssertNoErrors(docAddress.E2_ScreeningStatusInfo);

			docAddress.E2_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			AssertNoErrors(docAddress.E2_ScreeningStatusInfo);

			docAddress.E2_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			AssertNoErrors(docAddress.E2_ScreeningStatusInfo);

			docAddress.E2_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			AssertNoErrors(docAddress.E2_ScreeningStatusInfo);

			docAddress.E2_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			AssertNoErrors(docAddress.E2_ScreeningStatusInfo);

			docAddress.E2_ScreeningStatus = ScreeningStatusesList.Codes.Block;
			AssertNoErrors(docAddress.E2_ScreeningStatusInfo);

			docAddress.E2_ScreeningStatus = ScreeningStatusesList.Codes.NeedsScreening;
			AssertNoErrors(docAddress.E2_ScreeningStatusInfo);

			docAddress.E2_ScreeningStatus = ScreeningStatusesList.Codes.Release;
			AssertNoErrors(docAddress.E2_ScreeningStatusInfo);

			docAddress.E2_ScreeningStatus = ScreeningStatusesList.Codes.RequiresReview;
			AssertNoErrors(docAddress.E2_ScreeningStatusInfo);

			docAddress.E2_ScreeningStatus = string.Empty;
			AssertHasError(docAddress.E2_ScreeningStatusInfo, "Please enter an Unspecified: Screening Status.");

			docAddress.E2_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			AssertNoErrors(docAddress.E2_ScreeningStatusInfo);

			docAddress.E2_ScreeningStatus = "ZZZ";
			AssertHasError(docAddress.E2_ScreeningStatusInfo, "Enter a valid Unspecified: Screening Status.");
		}

		#endregion

		#region Phone Numbers Formatted

		public void TestCheckE2_Mobile_Formatted()
		{
			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			docAddress.E2_AddressOverride = true;

			docAddress.E2_Mobile_Formatted = "0426829924";
			AssertNoErrors("Valid phone number, no errors expected", docAddress.E2_Mobile_FormattedInfo);

			docAddress.E2_Mobile_Formatted = "04265";
			AssertHasErrors("Incorrect number of digits, error expected", docAddress.E2_Mobile_FormattedInfo);

			docAddress.E2_Mobile_IsManuallyVerified = true;
			AssertNoErrors("Number is invalid but manually verified, no errors expected", docAddress.E2_Mobile_FormattedInfo);
		}

		public void TestCheckE2_Fax_Formatted()
		{
			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			docAddress.E2_AddressOverride = true;

			docAddress.E2_Fax_Formatted = "0426829924";
			AssertNoErrors("Valid phone number, no errors expected", docAddress.E2_Fax_FormattedInfo);

			docAddress.E2_Fax_Formatted = "04265";
			AssertHasErrors("Incorrect number of digits, error expected", docAddress.E2_Fax_FormattedInfo);

			docAddress.E2_Fax_IsManuallyVerified = true;
			AssertNoErrors("Number is invalid but manually verified, no errors expected", docAddress.E2_Fax_FormattedInfo);
		}

		public void TestCheckE2_Phone_Formatted()
		{
			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			docAddress.E2_AddressOverride = true;

			docAddress.E2_Phone_Formatted = "0426829924";
			AssertNoErrors("Valid phone number, no errors expected", docAddress.E2_Phone_FormattedInfo);

			docAddress.E2_Phone_Formatted = "04265";
			AssertHasErrors("Incorrect number of digits, error expected", docAddress.E2_Phone_FormattedInfo);

			docAddress.E2_Phone_IsManuallyVerified = true;
			AssertNoErrors("Number is invalid but manually verified, no errors expected", docAddress.E2_Phone_FormattedInfo);
		}

		#endregion

		#region TestShouldValidateFKToCancelledRecord_Except_E2_ParentID

		public void TestShouldValidateFKToCancelledRecord_Except_E2_ParentID()
		{
			var docAddress = Factory.New<JobDocAddress>();
			var jobDocAddressValidation = new JobDocAddressValidationForTesting(docAddress);

			foreach (ZPropertyInfo info in docAddress.ZPropertyInfoHash)
			{
				AssertEquals("$Only E2_ParentID property does not require ValidateFKToCancelledRecord. prop: { info.Name}", info.Name != JobDocAddressSchema.Constants.E2_ParentID, jobDocAddressValidation.ShouldValidateFKToCancelledRecord_Exposed(info));
			}
		}

		#endregion
	}
}
