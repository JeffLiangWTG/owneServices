using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class OrgAddressMessageDataValidation : AutoOrgAddressMessageDataValidation
	{
		public OrgAddressMessageDataValidation(AutoOrgAddressMessageData parent)
			: base(parent)
		{
		}

		public new OrgAddressMessageData Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (OrgAddressMessageData)base.Parent; }
		}

		protected override void CheckUS_ActionCode()
		{
			base.CheckUS_ActionCode();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ActionCodeInfo, "action code");

			if (!Parent.Lookups.ActionCodeList.ContainsCode(Parent.US_ActionCode))
			{
				Parent.US_ActionCodeInfo.AddMessageError(InvalidActionCode);
			}

			ValidateUS_ImporterNumber();
			if (Parent.US_ActionCode == ImporterADDActionCodeList.Codes.RequestCBPNumber)
			{
				if (!Parent.US_HaveSSNIndicator && !Parent.US_NoSSNIndicator && !Parent.US_NotAppliedIndicator && !Parent.US_NoIRSIndicator && !Parent.US_NotResidentIndicator)
				{
					Parent.US_ActionCodeInfo.AddMessageError(OneOfRequestReasonMustBeTicked);
				}
			}
			ValidateUS_ImporterName();
			ValidateUS_ImporterType();
			ValidateUS_OA_Address1();
			ValidateUS_OA_Address2();
			ValidateUS_AcknowledgeAndSign();
			ValidateUS_ImporterPhoneNumber();
			ValidateUS_ImporterEmail();
			ValidateUS_AddressType1();
			ValidateUS_OC_CertifyIndividual();
		}
		internal const string InvalidActionCode = "Please select a valid action code.";
		internal const string OneOfRequestReasonMustBeTicked = "One of the reason codes must be provided when action code is 'N'.";

		protected override void CheckUS_NameQualifier()
		{
			base.CheckUS_NameQualifier();
			if (Parent.US_NameQualifier.IsEmpty)
			{
				if (!Parent.US_AlternativeImporterName.IsEmpty)
				{
					Parent.US_NameQualifierInfo.AddMessageError(NameQualifierShouldBeEnteredForAlternativeName);
				}
			}
			else if (!Parent.Lookups.NameQualifierList.ContainsCode(Parent.US_NameQualifier))
			{
				Parent.US_NameQualifierInfo.AddMessageError(InvalidNameQualifier);
			}
			ValidateUS_AlternativeImporterName();
		}

		internal const string NameQualifierShouldBeEnteredForAlternativeName = "An alternative name type is required when the name is specified.";
		internal const string InvalidNameQualifier = "Please select a valid alternative name type.";

		protected override void CheckUS_AlternativeImporterName()
		{
			base.CheckUS_AlternativeImporterName();
			if (!Parent.US_NameQualifier.IsEmpty && Parent.US_AlternativeImporterName.IsEmpty)
			{
				Parent.US_AlternativeImporterNameInfo.AddMessageError(AlternativeNameShouldBeEnteredForAType);
			}
			ValidateUS_NameQualifier();
		}
		internal const string AlternativeNameShouldBeEnteredForAType = "An alternative name is required when its type is specified.";

		protected override void CheckUS_ImporterNumber()
		{
			base.CheckUS_ImporterNumber();
			if (Parent.US_ActionCode != ImporterADDActionCodeList.Codes.RequestCBPNumber)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ImporterNumberInfo, "importer number");
				if (!Parent.US_ImporterNumber.IsEmpty)
				{
					if (!EmployerIdentificationNumberValidator.IsValidEIN(Parent.US_ImporterNumber)
						&& !SocialSecurityNumberValidator.IsValidSSN(Parent.US_ImporterNumber)
						&& !CBPAssignedNumberValidator.IsValidCBPAssignedNumber(Parent.US_ImporterNumber))
					{
						Parent.US_ImporterNumberInfo.AddMessageError(ImporterNumberNotInRightFormatForNew5106);
					}
					else if (EmployerIdentificationNumberValidator.IsValidEIN(Parent.US_ImporterNumber))
					{
						var last2Characters = Parent.US_ImporterNumber.SubstringSafe(10, 2);
						if (last2Characters.Contains("O", System.StringComparison.OrdinalIgnoreCase) || last2Characters.Contains("I", System.StringComparison.OrdinalIgnoreCase) || last2Characters.Contains("Z", System.StringComparison.OrdinalIgnoreCase))
						{
							Parent.US_ImporterNumberInfo.AddMessageError(IRSFormatCannotContainsOIZ);
						}
					}
					else if (CBPAssignedNumberValidator.IsValidCBPAssignedNumber(Parent.US_ImporterNumber) && Parent.US_ActionCode != ImporterADDActionCodeList.Codes.ChangeImporter)
					{
						Parent.US_ImporterNumberInfo.AddMessageError(PreviousCBPAssignedNumberOnlyAllowedForUpdate);
					}
				}
			}
			else if (!Parent.US_ImporterNumber.IsEmpty)
			{
				Parent.US_ImporterNumberInfo.AddMessageError(ImporterNumberNotRequiredForCBPNumberRequest);
			}
		}

		internal const string ImporterNumberNotRequiredForCBPNumberRequest = "You have indicated CBP-assigned number is to be requested, however the number already exists for this organisation.";
		internal const string ImporterNumberNotInRightFormat = "The number is not in a valid format of EIN or SSN.\n" +
			EmployerIdentificationNumberValidator.EINNumberRightFormat + "\n" + SocialSecurityNumberValidator.SocialSecurityNumberRightFormat;
		internal const string IRSFormatCannotContainsOIZ = "The number you have entered is in IRS format, however the last two-position suffix (XX) cannot be the letters O, I and/or Z.";
		internal const string ImporterNumberNotInRightFormatForNew5106 = "The number is not in a valid format of EIN or SSN or Previously assigned CBP Number.\n" +
			EmployerIdentificationNumberValidator.EINNumberRightFormat + "\n" + SocialSecurityNumberValidator.SocialSecurityNumberRightFormat + "\n\n" + CBPAssignedNumberValidator.CBPAssignedNumberRightFormat;
		internal const string PreviousCBPAssignedNumberOnlyAllowedForUpdate = "Previous assigned CBP Number is only allowed when the action code is 'U'.";

		protected override void CheckUS_ImporterName()
		{
			base.CheckUS_ImporterName();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ImporterNameInfo, "importer name");
		}

		protected override void CheckUS_ImporterType()
		{
			base.CheckUS_ImporterType();
			if (Parent.US_ImporterType.IsEmpty)
			{
				if (Parent.IsUSImporter(Parent.US_RN_NKCountry1))
				{
					Parent.US_ImporterTypeInfo.AddMessageError(ImporterTypeMandatory);
				}
			}
			else if (!Parent.Lookups.ImporterTypesList.ContainsCode(Parent.US_ImporterType))
			{
				Parent.US_ImporterTypeInfo.AddMessageError(InvalidImporterType);
			}
			else if (!Parent.IsUSImporter(Parent.US_RN_NKCountry1) && (Parent.US_ImporterType == ImporterTypeList.Codes.StateGovernment || Parent.US_ImporterType == ImporterTypeList.Codes.USGovernment))
			{
				Parent.US_ImporterTypeInfo.AddMessageError(InvalidImporterTypeForForgienAddress);
			}
		}

		internal const string ImporterTypeMandatory = "Importer type is mandatory for a US-based importer.";
		internal const string InvalidImporterType = "Please select a valid importer type.";
		internal const string InvalidImporterTypeForForgienAddress = "Importer type cannot be 'L' or 'G' if address is a foreign address.";

		#region Address 1 Details

		protected override void CheckUS_OA_Address1()
		{
			base.CheckUS_OA_Address1();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_OA_Address1Info, "mailing address");

			ValidateUS_LineOneAddress1();
			ValidateUS_CityAddress1();
			ValidateUS_StateAddress1();
			ValidateUS_ZipAddress1();
			ValidateUS_RN_NKCountry1();
			ValidateUS_AddressType1();
		}

		protected override void CheckUS_LineOneAddress1()
		{
			base.CheckUS_LineOneAddress1();

			if (Parent.FirstAddress != null)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_LineOneAddress1Info, "line one address");
			}
		}

		protected override void CheckUS_LineTwoAddress1()
		{
			base.CheckUS_LineTwoAddress1();

			if (Parent.IsForeignImporter(Parent.US_RN_NKCountry1) && !Parent.US_LineTwoAddress1.IsEmpty)
			{
				Parent.US_LineTwoAddress1Info.AddMessageError(LineTwoAddressEnteredForUSBasedImporter);
			}
		}
		internal const string LineTwoAddressEnteredForUSBasedImporter = "A line two address should be entered only for a US-based importer.";

		protected override void CheckUS_CityAddress1()
		{
			base.CheckUS_CityAddress1();

			if (Parent.FirstAddress != null)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_CityAddress1Info, "city");
			}
		}

		protected override void CheckUS_RN_NKCountry1()
		{
			base.CheckUS_RN_NKCountry1();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_RN_NKCountry1Info, "country");

			ValidateUS_ImporterType();
			ValidateUS_LineTwoAddress1();
			ValidateUS_StateAddress1();
			ValidateUS_ZipAddress1();
		}

		protected override void CheckUS_StateAddress1()
		{
			base.CheckUS_StateAddress1();

			CheckState(Parent.US_StateAddress1Info, Parent.US_RN_NKCountry1, Parent.Lookups.StateAddress1List);
			ValidateUS_ZipAddress1();
		}

		protected override void CheckUS_ZipAddress1()
		{
			base.CheckUS_ZipAddress1();

			CheckZipCode(Parent.US_ZipAddress1Info, Parent.US_StateAddress1, Parent.US_RN_NKCountry1);
		}

		#endregion

		#region Address 2 Details

		protected override void CheckUS_RN_NKCountry2()
		{
			base.CheckUS_RN_NKCountry2();

			if (Parent.SecondAddress != null)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_RN_NKCountry2Info, "country");
			}

			ValidateUS_LineTwoAddress2();
			ValidateUS_StateAddress2();
			ValidateUS_ZipAddress2();
		}

		protected override void CheckUS_OA_Address2()
		{
			base.CheckUS_OA_Address2();

			if (Parent.US_OA_Address2.IsEmpty && ImporterAddressTypesList.IsPhysicalRelatedMailingAddressType(Parent.US_AddressType1))
			{
				Parent.US_OA_Address2Info.AddMessageError(PhysicalLocationAddressRequired);
			}
			else if (Parent.US_OA_Address2.IsValid && Parent.US_OA_Address1 == Parent.US_OA_Address2)
			{
				Parent.US_OA_Address2Info.AddMessageError(PhysicalLocationAddressSameAsMailingAddress);
			}

			ValidateUS_LineOneAddress2();
			ValidateUS_CityAddress2();
			ValidateUS_StateAddress2();
			ValidateUS_ZipAddress2();
			ValidateUS_RN_NKCountry2();
			ValidateUS_AddressType2();
		}
		internal const string PhysicalLocationAddressRequired = "Physical location address is required when mailing address type is Business Service Center or PO Box or Other.";
		internal const string PhysicalLocationAddressSameAsMailingAddress = "Physical location address is the same as the mailing address. You should enter this if it is different from the mailing address.";

		protected override void CheckUS_LineOneAddress2()
		{
			base.CheckUS_LineOneAddress2();

			if (Parent.SecondAddress != null)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_LineOneAddress2Info, "line one address");
			}
		}

		protected override void CheckUS_CityAddress2()
		{
			base.CheckUS_CityAddress2();

			if (Parent.SecondAddress != null)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_CityAddress2Info, "city");
			}
		}

		protected override void CheckUS_StateAddress2()
		{
			base.CheckUS_StateAddress2();

			if (Parent.SecondAddress != null)
			{
				CheckState(Parent.US_StateAddress2Info, Parent.US_RN_NKCountry2, Parent.Lookups.StateAddress2List);
			}

			ValidateUS_ZipAddress2();
		}

		protected override void CheckUS_ZipAddress2()
		{
			base.CheckUS_ZipAddress2();

			if (Parent.SecondAddress != null)
			{
				CheckZipCode(Parent.US_ZipAddress2Info, Parent.US_StateAddress2, Parent.US_RN_NKCountry2);
			}
		}

		protected override void CheckUS_LineTwoAddress2()
		{
			base.CheckUS_LineTwoAddress2();

			if (Parent.IsForeignImporter(Parent.US_RN_NKCountry2) && !Parent.US_LineTwoAddress2.IsEmpty)
			{
				Parent.US_LineTwoAddress2Info.AddMessageError(LineTwoAddressEnteredForUSBasedImporter);
			}
		}

		#endregion

		#region Importer/Consignee Create/Update Validations

		protected override void CheckUS_HaveSSNIndicator()
		{
			base.CheckUS_HaveSSNIndicator();
			if (Parent.US_HaveSSNIndicator && Parent.US_ActionCode != ImporterADDActionCodeList.Codes.RequestCBPNumber)
			{
				Parent.US_HaveSSNIndicatorInfo.AddMessageError(RequestReasonNotRequired);
			}
			ValidateUS_ActionCode();
			ValidateUS_ImporterNumber();
		}
		internal const string RequestReasonNotRequired = "CBP assigned number indicator is only required when action code is 'N'.";

		protected override void CheckUS_NoSSNIndicator()
		{
			base.CheckUS_NoSSNIndicator();
			if (Parent.US_NoSSNIndicator && Parent.US_ActionCode != ImporterADDActionCodeList.Codes.RequestCBPNumber)
			{
				Parent.US_NoSSNIndicatorInfo.AddMessageError(RequestReasonNotRequired);
			}
			ValidateUS_ActionCode();
		}

		protected override void CheckUS_NotAppliedIndicator()
		{
			base.CheckUS_NotAppliedIndicator();
			if (Parent.US_NotAppliedIndicator && Parent.US_ActionCode != ImporterADDActionCodeList.Codes.RequestCBPNumber)
			{
				Parent.US_NotAppliedIndicatorInfo.AddMessageError(RequestReasonNotRequired);
			}
			ValidateUS_ActionCode();
		}

		protected override void CheckUS_NoIRSIndicator()
		{
			base.CheckUS_NoIRSIndicator();
			if (Parent.US_NoIRSIndicator && Parent.US_ActionCode != ImporterADDActionCodeList.Codes.RequestCBPNumber)
			{
				Parent.US_NoIRSIndicatorInfo.AddMessageError(RequestReasonNotRequired);
			}
			ValidateUS_ActionCode();
		}

		protected override void CheckUS_NotResidentIndicator()
		{
			base.CheckUS_NotResidentIndicator();
			if (Parent.US_NotResidentIndicator && Parent.US_ActionCode != ImporterADDActionCodeList.Codes.RequestCBPNumber)
			{
				Parent.US_NotResidentIndicatorInfo.AddMessageError(RequestReasonNotRequired);
			}
			ValidateUS_ActionCode();
		}

		protected override void CheckUS_NumberOfEntries()
		{
			base.CheckUS_NumberOfEntries();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_NumberOfEntriesInfo, Parent.Lookups.NumberOfEntriesList);
		}

		protected override void CheckUS_ProgramCode1()
		{
			base.CheckUS_ProgramCode1();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_ProgramCode1Info, Parent.Lookups.ProgramCodesList);
		}

		protected override void CheckUS_ProgramCode2()
		{
			base.CheckUS_ProgramCode2();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_ProgramCode2Info, Parent.Lookups.ProgramCodesList);
		}

		protected override void CheckUS_ProgramCode3()
		{
			base.CheckUS_ProgramCode3();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_ProgramCode3Info, Parent.Lookups.ProgramCodesList);
		}

		protected override void CheckUS_ProgramCode4()
		{
			base.CheckUS_ProgramCode4();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_ProgramCode4Info, Parent.Lookups.ProgramCodesList);
		}

		protected override void CheckUS_UtlOtherIndicator()
		{
			base.CheckUS_UtlOtherIndicator();
			ValidateUS_UtlOtherDescription();
		}

		protected override void CheckUS_UtlOtherDescription()
		{
			base.CheckUS_UtlOtherDescription();
			if (Parent.IsUtlizationOtherDescriptionRequired)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_UtlOtherDescriptionInfo);
			}
		}

		protected override void CheckUS_AddressType1()
		{
			base.CheckUS_AddressType1();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_AddressType1Info, Parent.Lookups.MailingAddressTypeList);

			ValidateUS_LineTwoAddress1();
			ValidateUS_AddressExplanation1();
		}

		protected override void CheckUS_AddressExplanation1()
		{
			base.CheckUS_AddressExplanation1();
			if (Parent.IsAddressExplanation1Required)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_AddressExplanation1Info);
			}
		}

		protected override void CheckUS_AddressType2()
		{
			base.CheckUS_AddressType2();
			if (!Parent.US_OA_Address2.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_AddressType2Info, Parent.Lookups.PhysicalAddressTypeList);
			}
			ValidateUS_AddressExplanation2();
		}

		protected override void CheckUS_AddressExplanation2()
		{
			base.CheckUS_AddressExplanation2();
			if (Parent.IsAddressExplanation2Required)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_AddressExplanation2Info);
			}
		}

		protected override void CheckUS_ImporterPhoneNumber()
		{
			base.CheckUS_ImporterPhoneNumber();
			if (Parent.US_ImporterPhoneNumber.IsEmpty)
			{
				Parent.US_ImporterPhoneNumberInfo.AddMessageError(ImporterPhoneNumberRequired);
			}
			else if (!Parent.US_ImporterPhoneNumber.IsEmpty && !Parent.US_ImporterPhoneNumber.IsNumbersOnlyOrEmpty)
			{
				Parent.US_ImporterPhoneNumberInfo.AddMessageError(InvalidPhoneNumberFormat);
			}
		}
		internal const string ImporterPhoneNumberRequired = "Please provide a phone number to contact the importer.";

		protected override void CheckUS_ImporterEmail()
		{
			base.CheckUS_ImporterEmail();
			if (Parent.US_ImporterEmail.IsEmpty)
			{
				Parent.US_ImporterEmailInfo.AddMessageError(ImporterEmailRequired);
			}
			else if (!EmailAddressValidation.IsEmailAddressValid(Parent.US_ImporterEmail))
			{
				Parent.US_ImporterEmailInfo.AddWarning(ImporterEmailInvalidFormat);
			}
		}
		internal const string ImporterEmailRequired = "Please provide an email address to contact the importer.";
		internal const string ImporterEmailInvalidFormat = "The email address format is invalid";

		protected override void CheckUS_ImporterFaxNumber()
		{
			base.CheckUS_ImporterFaxNumber();
			if (!Parent.US_ImporterFaxNumber.IsEmpty && !Parent.US_ImporterFaxNumber.IsNumbersOnlyOrEmpty)
			{
				Parent.US_ImporterFaxNumberInfo.AddMessageError(InvalidFaxNumberFormat);
			}
		}
		internal const string InvalidFaxNumberFormat = "The fax number format is invalid, should be only numerics.";

		protected override void CheckUS_ImporterWebsite()
		{
			base.CheckUS_ImporterWebsite();
			if (!Parent.US_ImporterWebsite.IsEmpty && !UrlValidation.IsValidUrl(Parent.US_ImporterWebsite))
			{
				Parent.US_ImporterWebsiteInfo.AddWarning(WebsiteURLInvalidFormat);
			}
		}
		internal const string WebsiteURLInvalidFormat = "Please enter a valid website address (URL).\r\n\r\nA valid address is commonly found in the format 'http://' or 'www.'";

		protected override void CheckUS_NAICSCode()
		{
			base.CheckUS_NAICSCode();
			if (!Parent.US_NAICSCode.IsEmpty && (Parent.US_NAICSCode.Length != 6 || !Parent.US_NAICSCode.IsNumbersOnlyOrEmpty))
			{
				Parent.US_NAICSCodeInfo.AddMessageError(NAICSCodeInvalidFormat);
			}
		}
		internal const string NAICSCodeInvalidFormat = "NAICS Code should be 6 digits.";

		protected override void CheckUS_DUNS()
		{
			base.CheckUS_DUNS();
			if (!Parent.US_DUNS.IsEmpty && (Parent.US_DUNS.Length != 9 || !Parent.US_DUNS.IsNumbersOnlyOrEmpty))
			{
				Parent.US_DUNSInfo.AddMessageError(DUNSInvalidFormat);
			}
		}
		internal const string DUNSInvalidFormat = "DUNS# should be 9 digits.";

		protected override void CheckUS_YearEstablished()
		{
			base.CheckUS_YearEstablished();
			if (!Parent.US_YearEstablished.IsEmpty && (Parent.US_YearEstablished.Length != 4 || !Parent.US_YearEstablished.IsNumbersOnlyOrEmpty))
			{
				Parent.US_YearEstablishedInfo.AddMessageError(YearInvalidFormat);
			}
		}
		internal const string YearInvalidFormat = "Year should be 4 digits, in a format CCYY, where CC is century and YY is year.";

		protected override void CheckUS_BankCountry()
		{
			base.CheckUS_BankCountry();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_BankCountryInfo, Parent.Lookups.CountryList);
			ValidateUS_BankState();
		}

		protected override void CheckUS_BankState()
		{
			base.CheckUS_BankState();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_BankStateInfo, Parent.Lookups.BankCountryStateList);
		}

		protected override void CheckUS_CountryISOCode()
		{
			base.CheckUS_CountryISOCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_CountryISOCodeInfo, Parent.Lookups.CountryList);
			ValidateUS_StateCode();
		}

		protected override void CheckUS_StateCode()
		{
			base.CheckUS_StateCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_StateCodeInfo, Parent.Lookups.CertificateCountryStateList);
		}

		protected override void CheckUS_OC_CertifyIndividual()
		{
			base.CheckUS_OC_CertifyIndividual();
			ValidateUS_IndividualName();
			ValidateUS_IndividualTitle();
		}

		protected override void CheckUS_IndividualName()
		{
			base.CheckUS_IndividualName();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_IndividualNameInfo);
			var errMsg = ContactNameHelper.GetWarningMessageIfInvalidFormat(Parent.US_IndividualName);
			if (!errMsg.IsEmpty)
			{
				Parent.US_IndividualNameInfo.AddMessageError(errMsg);
			}
		}

		protected override void CheckUS_IndividualTitle()
		{
			base.CheckUS_IndividualTitle();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_IndividualTitleInfo);
		}

		protected override void CheckUS_AcknowledgeAndSign()
		{
			base.CheckUS_AcknowledgeAndSign();
			if (!Parent.US_AcknowledgeAndSign)
			{
				Parent.US_AcknowledgeAndSignInfo.AddMessageError(ShouldBeSigned);
			}
		}
		internal const string ShouldBeSigned = "Should be acknowledged and signed.";

		protected override void CheckUS_IndividualPhone()
		{
			base.CheckUS_IndividualPhone();
			if (!Parent.US_IndividualPhone.IsEmpty && !Parent.US_IndividualPhone.IsNumbersOnlyOrEmpty)
			{
				Parent.US_IndividualPhoneInfo.AddMessageError(InvalidPhoneNumberFormat);
			}
		}
		internal const string InvalidPhoneNumberFormat = "The phone number format is invalid, should be only numerics.";

		protected override void CheckUS_BrokerPhone()
		{
			base.CheckUS_BrokerPhone();
			if (!Parent.US_BrokerPhone.IsEmpty && !Parent.US_BrokerPhone.IsNumbersOnlyOrEmpty)
			{
				Parent.US_BrokerPhoneInfo.AddMessageError(InvalidPhoneNumberFormat);
			}
		}

		#endregion

		#region Implementation

		void CheckState(ZPropertyInfo stateInfo, ZString countryCode, CodeDescriptionPairList stateAddressList)
		{
			ZString state = (ZString)stateInfo.Value;
			if (!countryCode.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(stateInfo, "state");
			}

			if (!state.IsEmpty)
			{
				if (Parent.IsUSImporter(countryCode) || Parent.IsCAImporter(countryCode) || Parent.IsMXImporter(countryCode))
				{
					if (!stateAddressList.ContainsCode(state))
					{
						stateInfo.AddMessageError(ZString.Format(InvalidState, countryCode));
					}
				}
				else if (state != OrgAddressMessageData.ForeignBasedImporterStateCode)
				{
					stateInfo.AddMessageError(StateForImportersOtherThanUS_CA);
				}
			}
		}
		internal const string StateForImportersOtherThanUS_CA = "For foreign countries other than CA, the state should be 'FN'.";
		internal const string InvalidState = "This is an invalid {0} state.";

		void CheckZipCode(ZPropertyInfo zipCodeInfo, ZString state, ZString country)
		{
			if (Parent.IsZipMandatory(country))
			{
				MandatoryValidation.MessageErrorIfNotEntered(zipCodeInfo, "ZIP code");
			}

			ZString zipCode = (ZString)zipCodeInfo.Value;
			if (!zipCode.IsEmpty)
			{
				if (Parent.IsUSImporter(country))
				{
					if (!state.IsEmpty)
					{
						string message = new ZipCodeValidation().ValidateForZipCode(Parent.Factory, zipCode, state, country);
						if (!string.IsNullOrEmpty(message))
						{
							zipCodeInfo.AddMessageError(message);
						}
					}
				}
				else if (Parent.IsCAImporter(country))
				{
					string message = new ZipCodeValidation().ValidateCAZipCodeAndGetMessage(zipCode);
					if (!string.IsNullOrEmpty(message))
					{
						zipCodeInfo.AddMessageError(message);
					}
				}
				else if (Parent.IsMXImporter(country))
				{
					string message = new ZipCodeValidation().ValidateMXZipCodeAndGetMessage(zipCode);
					if (!string.IsNullOrEmpty(message))
					{
						zipCodeInfo.AddMessageError(message);
					}
				}
				else
				{
					if (OrgCountryZipCodeInfo.CountryZipCodeInfoMap.ContainsKey(country))
					{
						int? length = OrgCountryZipCodeInfo.CountryZipCodeInfoMap[country].validationLength;
						string message = new ZipCodeValidation().ValidateZipCodeLengthAndGetMessage(zipCode, length);
						if (!string.IsNullOrEmpty(message))
						{
							zipCodeInfo.AddMessageError(message);
						}
					}
				}
			}
		}

		#endregion
	}
}
