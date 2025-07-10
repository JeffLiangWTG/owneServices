using System.Globalization;
using CargoWise.Types;
using Enterprise.Customs.US.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.Business
{
	static class ValidationConstants
	{
		internal static class Bill
		{
			internal const string CBPEntryNumberRightFormat = "CBP Entry Number should be in the format, FFFNNNNNNNN where F is alphanumeric and N is a number.";
			internal static string BillNumberMaxLength(string billTypeDescription, int maxLength)
			{
				return string.Format("{0} Number should be no longer than {1} characters.", billTypeDescription, maxLength);
			}
			internal static string BillNumberMinLength(string billTypeDescription, int minLength)
			{
				return string.Format(CultureInfo.CurrentCulture, "{0} Number should minimum {1} characters in length, where the first 4 characters is a valid SCAC code.", billTypeDescription, minLength);
			}
			internal static string BillNumberSCACCodeValidation(string billTypeDescription)
			{
				return string.Format(CultureInfo.CurrentCulture, "{0}: SCAC code (first 4 characters) is NOT valid.", billTypeDescription);
			}
			internal const string SuretyCodeRightFormat = "Surety Code should be in the format, NNN where N is a number.";
			internal const string SuretyCodeIsOnlyRequired = "A Surety Code should only be specified when the Bond Activity Code is '16' and the Bond Type is '09'.";
			internal const string SuretyCodeIsOnlyRequiredForShipmentTypes = "A Surety Code is only required when the Shipment Type is '01', '02', '07', '08' or '10'.";
			internal static string ISFBondNumberMaxLength(int maxLength)
			{
				return string.Format("Bond Number should be no longer than {0} characters.", maxLength);
			}
			internal const string OnlyOneSuretyCode = "Surety Code should be entered only once.";
			internal const string OnlyOneBondReferenceNumber = "Bond Reference Number should be entered only once.";
			internal const string OnlyOneFullNameOfISFImporter = "ISF Importer Full Name should be entered only once.";
			internal const string ReferenceDataAlreadyExists = "There is already another record in Reference Data with the same details.";
			internal static string ReferenceDataAlreadyExistsOnAnotherISF(string billTypeDescription, string billNumber, string jobReference)
			{
				return string.Format("{0} ({1}) has already been used in ISF Job ({2}).", billTypeDescription, billNumber, jobReference);
			}
			internal const string CarnetReferenceIsRequired = "At least one Carnet Reference must be specified when the Shipment Type is '06'.";
			internal const string CarnetReferenceRightFormat = "A Carnet Reference should be a 2 character Carnet Issuing Country Code followed by Carnet Number.";
			internal const string CarnetReferenceInvalidCountryCode = "The first 2 characters do not represent a valid Country Code. A Carnet Reference should be a 2 character Carnet Issuing Country Code followed by Carnet Number.";
			internal const string BondReferenceNumberIsRequired = "A Bond Reference Number must be specified when the Bond Activity Code is '16' and the Bond Type is '09'.";
			internal const string BondReferenceNumberIsOnlyRequired = "A Bond Reference Number should only be specified when the Bond Activity Code is '16' and the Bond Type is '09'.";
			internal const string BondReferenceNumberIsOnlyRequiredForShipmentTypes = "A Bond Reference Number is only required when the Shipment Type is '01', '02', '07', '08' or '10'.";
			internal const string OnlySpecifyMasterBillWhenThereIsHouseBill = "Only specify Master Bill when there is a House Bill.";
		}

		internal static class Header
		{
			internal const string LineMergeStyleNotSpecified = "You have not specified the Line Merge Style; the system will treat it as 'Not Merge'.";
			internal const string LineMergeStyleIsInvalid = "You have specified an invalid Line Merge Style; the system will treat it as 'Not Merge'.";
			internal const string BondHolderIsNotRequired = "A Bond Holder must not be specified where 'Bond Indicator' is ticked; a Surety Code should be specified instead.";
			internal const string BondHolderIsInvalid = "A Bond Holder must be a valid IRS Number or CBP Assigned Number or Social Security Number.";
			internal const string ConsigneeCountryOfIssueIsRequiredForPassport = "Country of Issue must be specified if the Consignee Identification Type chosen is a Passport.";
			internal const string ConsigneeDateOfBirthIsRequired = "Date of Birth must be specified if the Consignee Identification Type chosen is a Social Security Number or Passport and Shipment type is '03', '05', '06' or '11'.";
			internal const string ConsigneeFullNameIsRequiredForPassportOrSocialSecurityNumber = "Full legal name must be specified if the Consignee Identification Type chosen is a Passport or a Social Security Number.";
			internal const string CountryOfIssueIsRequiredForPassport = "Country of Issue must be specified if the Importer Identification Type chosen is a Passport.";
			internal const string DateOfBirthIsRequired = "Date of Birth must be specified if the Importer Identification Type chosen is a Social Security Number or Passport and Shipment type is '03', '05', '06' or '11'.";
			internal const string DateOfBirthIsInvalid = "Date of Birth cannot be in the future.";
			internal const string PassportIsOnlyUsedFor03Or05Or06ShipmentType = "A Passport can only be used when Shipment Type is '03', '05' or '06'.";
			internal const string BondSuretyCodeIsRequiredForISFBondSingleTransaction = "A Bond Surety Code must be specified when the Bond Activity Code is '16' and the Bond Type is '09'.";
			internal const string ISFBondNumberIsRequired = "A Bond Number must be specified where 'Bond Indicator' is not ticked and Bold Holder is not entered.";
			internal const string ImporterFullNameIsRequiredForPassportOrSocialSecurityNumber = "Full legal name must be specified if the Importer Identification Type chosen is a Passport or a Social Security Number.";
			internal const string HouseOrOceanBillIsRequired = "Either a House Bill (with optional Master Bill) Or an Ocean Bill must be specified but not both.";
			internal const string NumberOfHarmonizedDigitsToReport = "Number of Harmonized Digits to report must be either 6, 8 or 10; the system will use 10 as default.";
			internal const string AtLeastOneHarmonizedTariffScheduleIsEntered = "At least one Harmonized Tariff Schedule Line is required.";
			internal const string StandardOrRegularFilingsIsRequiredWhenISF5 = "Shipment Type must be '01 - Standard or Regular filings' when submitting ISF-5.";
			internal const string BondType9MayOnlyUsedWithBondActivityCode16 = "A Bond Type '9' may only be used when the Bond Activity Code is '16'.";
			internal const string EstimatedValueIsRequiredForInformalShipmentType = "An estimated monetary value in whole US dollars is required when the Shipment Type is '11'.";
			internal const string EstimatedQuantityIsRequiredForInformalShipmentType = "An estimated quantity of the smallest packaging unit is required when the Shipment Type is '11'.";
			internal const string EstimatedQuantityUQIsRequiredForInformalShipmentType = "An estimated quantity unit is required when the Shipment Type is '11'.";
			internal const string ForeignBasedConsigneeWithForeignBasedImporter = "You have entered a foreign-based Consignee with a foreign-based Importer.";
			internal const string OnlyEnterCustomsReferenceIfModifyingJobFromAnotherSystem = "The Customs Reference should only be entered in the event that you are trying to modify a job originating from another system.";
			internal const string CustomsReferenceFormat = "The Customs Reference format must be FFF-NNNNNNNNNNN (FFF= Entry Filer Code and NNNNNNNNNNN= numeric sequence number).";

			internal static string CustomsReferenceFirst3CharsShouldBeSameAsEntryFilerCode(string entryFilerCode)
			{
				return string.Format("The Customs Reference must start with Entry Filer Code {0}.", entryFilerCode);
			}

			internal static string NoValidBondOnFile(ZString activityCode, ZString bondType, ZDateTime effectiveDate)
			{
				return string.Format(@"This Bond Detail (Activity Code='{0}', Bond Type='{1}', Effective Date='{2}') does not match any valid Bond Details setup against the Importer (Organization > Details > Config > US Defaults).
Please review the Bond Detail as it might not valid in Customs.", activityCode, bondType, effectiveDate.ToSmallDateTime());
			}

			internal static string BondDetailIsRequiredForShipmentTypes(string propertyName)
			{
				return string.Format("A {0} is required when the Shipment Type is '01', '02', '07', '08' or '10'.", propertyName);
			}

			internal static string BondDetailIsOnlyRequiredForShipmentTypes(string propertyName)
			{
				return string.Format("A {0} is only required when the Shipment Type is '01', '02', '07', '08' or '10' and Entry Type is not '5' and '6'.", propertyName);
			}

			internal static string UnrecognizedCharactersInProperty(string propertyName)
			{
				return string.Format("There are foreign characters in {0}. These charaters will be truncated when sent to Customs.", propertyName);
			}

			internal static string ValueIsRequiredForEntity(string consigneeCodeTypeDescription)
			{
				return string.Format("Please enter a value for the {0}.", consigneeCodeTypeDescription);
			}
			internal static string OrganisationShouldBeRegisteredInCustoms(string codeType, string organisationCode)
			{
				return string.Format("This {0} belongs to organization '{1}'. " + OrganisationValidation.OrganisationShouldBeRegisteredInCustoms, codeType, organisationCode);
			}
			internal static string EntityMightNotBeRegisteredInCustoms(string codeType)
			{
				return string.Format("Unable to verify validity of this {0}, since it does not belong to any organization in the system.", codeType);
			}

			internal static string DataNotValidForBranch(string code, string branch, string registryLocation)
			{
				return string.Format("{0} has not been setup for Branch ({1}). Either select a branch with a valid {0} or configure {0} in the Registry (Maintain > System > Registry > {2}).", code, branch, registryLocation);
			}

			internal static string DataNotValidForUSMessagingBranch(string code, string usMessagingbranch, string branch, string registryLocation, string usMessagingBranchRegistryLocation)
			{
				return string.Format("{0} has not been setup for US Messaging Branch ({1}) which sends messages for Branch ({2}). Either select a branch with a valid US Messaging Branch {0} or configure {0} in the Registry (Maintain > System > Registry > {3}) or configure US Messaging Branch in the Registry (Maintain > System > Registry > {4}).", code, usMessagingbranch, branch, registryLocation, usMessagingBranchRegistryLocation);
			}
		}

		internal class Organisation
		{
			internal const string UnmatchedOrganisationNotValidForCustoms = "Please enter a valid Organization.  An unmatched Organization should not be use for Customs messaging.";
			internal static string OrganisationPKRequired(string addressCaption, string entryType)
			{
				return string.Format("A {0} must be specified when Entry Type is '{1}'.", addressCaption, entryType);
			}
			internal const string CompanyNameRequireWhenNoRegistration = "A Company Name is required when a Registration is not entered.";
			internal const string AddressRequireWhenNoRegistration = "An Address is required when a Registration is not entered.";
			internal const string CityRequireWhenNoRegistration = "A City is required when a Registration is not entered.";
			internal const string CountryRequireWhenNoRegistration = "A Country is required when a Registration is not entered.";
			internal const string MissingUNLOCO = @"Cannot determine a valid country for this organization; a valid country is required for messaging.
Please make sure that there is a valid UNLOCO setup for this organization.";

			internal static string CannotDeleteAddress(string addressDescription)
			{
				return string.Format("This {0} is entered on the ISF > Details tab; it cannot be deleted from here.", addressDescription);
			}

			internal const string CountryOfIssueIsRequiredForPassport = "Country of Issue must be specified if the Registration Type chosen is Passport.";
			internal const string DateOfBirthIsRequiredForPassport = "Date of Birth must be specified if the Registration Type chosen is Passport.";
			internal const string IDIsRequiredForPassport = "Passport ID must be specified if the Registration Type chosen is Passport.";
			internal const string OrganizationRequiresEIN_CBN_ECN_SSN_PAS = "This organization does not have an EIN, CBN, ECN, SSN or Passport number configured. Please press F3 in the field and go to Config > Registration Numbers/Codes.";

			internal const string DateOfBirthIsRequiredForSocialSecurityNumber = "Date of Birth must be specified if the Registration Type chosen is Social Security Number.";
			internal const string NumberIsRequiredForSocialSecurityNumber = "Number must be specified if the Registration Type chosen is Social Security Number.";

			internal static string ContactNameIsRequiredFor(string dataType)
			{
				return string.Format("A name must be specified if the Registration Type chosen is {0}.", dataType);
			}

			internal static string PassportDataToReportToCustoms(ZString passportNumber, ZString name, ZDateTime dob, ZString countryOfIssue)
			{
				return string.Format("The following Passport data from the selected organization and contact will be reported to Customs:\r\nPassport Number:{0}\r\nName:{1}\r\nDOB:{2}\r\nCountry Of Issue:{3}", passportNumber, name, dob.ToShortDateString(), countryOfIssue);
			}

			internal static string SocialSecurityDataToReportToCustoms(ZString ssn, ZString name, ZDateTime dob)
			{
				return string.Format("The following Social Security data from the selected organization and contact will be reported to Customs:\r\nSocial Security Number:{0}\r\nName:{1}\r\nDOB:{2}", ssn, name, dob.ToShortDateString());
			}
		}

		internal class Character
		{
			internal const string OnlyAlphaNumericCharactersAreAllowed = "You have entered an invalid character; only alphanumeric characters are allowed.";
			internal const string OnlyNumericCharactersAreAllowed = "You have entered an invalid character; only numeric characters are allowed.";
			internal const string OnlyAlphabeticCharactersAreAllowed = "You have entered an invalid character; only alphabetic characters are allowed.";
		}

		internal static class ISFHeaderRow
		{
			internal static NoResString ValidMasterBill
			{
				get { return (NoResString)"Please select a valid Master Bill from the list."; }
			}

			internal static string BillAlreadyUsedInConsol(string billNumber, int noOfExistingConsols, string consolReference)
			{
				return string.Format("The following {0} is already in used on following the consol{1}.", billNumber, noOfExistingConsols > 1 ? "s (" + consolReference + ")" : " " + consolReference);
			}
		}

		internal static class ISFBillRow
		{
			internal static NoResString ValidBill
			{
				get { return (NoResString)"Please select a valid Bill from the list."; }
			}

			internal static string BillAlreadyUsedInExistingRow(string billTypeDescriptonAndNumber)
			{
				return "There is already a row for this " + billTypeDescriptonAndNumber;
			}

			internal static string BillAlreadyUsedInShipment(string billNumber, int noOfExistingShipments, string shipmentReference)
			{
				return string.Format("The following House Bill '{0}' is already in used on following shipment{1}.", billNumber, noOfExistingShipments > 1 ? "s (" + shipmentReference + ")" : " " + shipmentReference);
			}
		}
	}
}
