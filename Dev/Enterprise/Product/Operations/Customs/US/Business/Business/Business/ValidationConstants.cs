using System.Globalization;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public static class ValidationConstants
	{
		public const string NegativeAmountNotAllowed = "Please enter a non-negative value.";
		public const string WarnIfStatQTYisZero = "Please confirm that quantity should be Zero.";
		public const string StatQTYRequired = "Quantity is mandatory.";
		public const string PAIIsInvalidAfter_28_01_2011 = "Paired Port Program is invalid for a shipment that arrives after 28th Jan 2011.";
		public const string ClientBranchDesignationNotMatchRegistrySetting = "Client Branch Designation does not match the setting in the registry.  Should be {0}.";
		public const string DeactivatedEntryWithActiveCustomsTransactionAwaitingResponse = "The changes you are trying to make cannot be made to this declaration, as an entry has already been lodged with the current details and it is pending Customs response. These changes can be made when the Customs response is processed.";
		public const string UseZZZZForUnknownCarrierSCAC = "SCAC Code \"UNKN\" cannot be used for ACE Cargo Release. Use a valid carrier or \"ZZZZ\" if unknown.";
		public const string ShouldTheSameAsOriginalCustomsQuantity = "Reconciliation Quantity must be the same as the Original Customs Quantity.";

		public static IMultilingualString InvalidPackTypeMessage
		{
			get { return (NoResString)"The code you selected is not in the list. Please verify that this is correct."; }
		}

		public static ZDateTime InformalEntryLimit2500StartDate
		{
			get { return new ZDateTime(2013, 1, 7); }
		}

		public static ZDateTime ACEDeploymentDate => new ZDateTime(2018, 1, 6);

		public static class AllocateInBondNumber
		{
			public static string MustBeNumeric
			{
				get { return ResString.GetMultilingualString("AllocateInBondNumber|3A8ABA09-9EB7-4d56-9BF3-B17EE3E025CB", "An In-Bond Number must be numeric."); }
			}

			public static string LengthShouldBeNine
			{
				get { return ResString.GetMultilingualString("AllocateInBondNumber|FD09BA78-FCA4-4778-84AF-2206D38D5555", "An In-Bond Number must be 9 digits including the last check digit."); }
			}

			public static string AlreadyExists
			{
				get { return ResString.GetMultilingualString("AllocateInBondNumber|7149EAEF-D19A-4f20-899F-D0320179AA90", "This number is already allocated to an existing job."); }
			}

			public static string InvalidCheckDigit(ZString expectedCheckDigit)
			{
				return ResString.GetMultilingualString("AllocateInBondNumber|EE78EA99-3CCD-4b8f-B637-3070F0490C18", "Invalid check digit. The last digit should be '{0}'", expectedCheckDigit);
			}

			public static string CannotAllocateInBondNumberNotInCompanyRange(string company)
			{
				return ResString.GetMultilingualString("AllocateInBondNumber|D6720C3E-0FA3-457B-BAAA-0DD7CB15F843", "Cannot allocate an In-Bond Number which is not within the range allocated for this company ({0}).", company);
			}

			public static string InvalidPaperless
			{
				get { return ResString.GetMultilingualString("AllocateInBondNumber|8B593B68-0146-430F-920D-9957B616122D", "Paperless IT Number should be in the following format: VXXNNNNNNNC where VXX - a unique CBP assigned alphanumeric code identifying the carrier; NNNNNNN - a 7 position number; C - a check digit."); }
			}

			public static string InBondNumberLengthShouldBeNineOrEleven
			{
				get { return ResString.GetMultilingualString("CusInBondMoveHeader|3C1B2752-2D3A-434E-8274-F01CB4B0CD6F", "An In-Bond Number must be 9 or 11 digits."); }
			}
		}

		public static class Part
		{
			public static string AttributeValueShouldBeUniqueFor(string attributeType)
			{
				return string.Format("Attribute value should be unique for {0}.", attributeType);
			}
		}

		public static class APHIS
		{
			public const string VehicleLengthIsRequiredWhenLicencePlateIsSpecified = "Vehicle Length is required when Licence Plate/Trailer Number is entered.";
			public const string ProcessingDescriptionRequiresForOtherTreatmentType = "Processing Description is required when Processing Type is 'ATR - Other treatment'.";
			public const string IdentityNumberMaxLengthWarningMessage = "Only the last 14 characters from the Identification Number will be sent.";
			public const string AtLeastOneNumberRangeIsRequired = "At least one number range is required when 'Use Multiple Numbers' is set.";
			public const string IdentityEndNumberRequiresStartNumber = "Identity End Number should only be entered when Start Number is entered.";
			public const string ApplicantIsRequired = "Authorized License Holder (or Permit Holder) address is required.";
			public const string ShipperIsRequiredWhenApplicantIsNotSpecified = "Shipper address is requierd when Applicant (or Permit Holder) address is not specified.";
			public const string PermittedDestinationIsRequired = "Permitted Destination address is requierd.";
			public const string APHISA04OnlySelectedIfArrivingFromScrewwormCountry = "Only select A04 if arriving from Screwworm country as determined by APHIS-Veterinary Services.";

			public static string NumberRequired(string fieldName, string numberType)
			{
				return ZString.Format("This {0} does not have {1} configured. Please press F3 in the organization field and add one in Config > Registration Numbers/Codes.", fieldName, numberType);
			}
			public const string ScientificVarietyNameShouldNotBeSpecifiedUnlessOtherScientificDataIs = "Scientific Variety Name should not be entered unless the other Scientific data is entered.";

			public static string QuantityANeedsToBeSpecifiedBeforeQuantityBCanBeSpecified(string quantity1, string quantity2)
			{
				return ZString.Format("Quantity {0} must be specified before Quantity {1} can be specified.", quantity1, quantity2);
			}

			public static string ScientificDataIsRequiredForCategory(string fieldName, ZString categoryType)
			{
				return ZString.Format("{0} must be specified when Category Type is '{1}'.", fieldName, categoryType);
			}

			public static string ScientificDataIsRequired(string fieldName, ZString typeValue, ZString typeName)
			{
				return ZString.Format("{0} must be specified when {1} is '{2}'.", fieldName, typeName, typeValue);
			}

			public static string Item1IsRequiredWhenItem2IsSpecified(string field1, string field2)
			{
				return ZString.Format("{0} must be specified when {1} is specified.", field1, field2);
			}

			public static string NotValidForBreed(ZString breed, ZString code)
			{
				return ZString.Format("'{0}' is not a valid code for Breed Code ('{1}').", code, breed);
			}
			public const string CategoryAndBreedRequiredForLiveAnimals = "Category and Breed required for Live Animals.";
		}

		public static class FWS
		{
			public const string TrackingNumberIsRequiredWhenTypeIsEntered = "Tracking Number is required when Tracking Type is entered.";
			public const string FWSImporterAddresMustBeUS = "FWS Importer's address must be an US addresss.";
			public const string FWEMustBeSetupAgainstImporterOrCompanyOrgProxyOrBranchOrgProxy = "The eDecs Filer Account must be set up at Company and/or Branch level in the Organization proxy Registration Number Type ‘FWE’ – this can also be set up for the Importer’s Organization if the Importer has an eDecs Filer Account";
			public const string FWSExporterAddresMustNotBeUS = "FWS Exporter's address must be a non-US addresss.";
			public const string FWSMultipleExporters = "This Customs Declaration has FWS data with multiple exporters. FWS offers the following advice when there are multiple exporters:  1) Please file eDecs data on the FWS web site.  2) Submit your FWS data in Cargowise using the LDS (Limited Data Set) Processing Code. The eDecs Confirmation Numbers you received from the FWS web site are entered in the License Details grid using a License Type of 'FWC'.";
			public const string CertifyingIndividualIsRequired = "Certifying Individual is required for Long Form messaging.";
			public const string FWSExporterAddressIsRequired = "FWS Exporter is required.";
			public const string FWSImporterAddressIsRequired = "FWS Importer is required.";
			public const string FWSExporterAddressIsRequiredAsCertifyingIndividual = "FWS Exporter is required when Certifying Individual is 'FWE'.";
			public const string FWSImporterAddressIsRequiredAsCertifyingIndividual = "FWS Importer is required when Certifying Individual is 'FWI'.";
			public const string ProductNumberIsRequiredWhenTypeIsEntered = "Product Number is required when Product Type is entered.";
			public const string IssuerCountryIsRequiredForFWFOrFWE = "Issuer's Country is required when License Type is 'FWF' or 'FWE'.";
			public const string IssuerIsRequiredForFWFOrFWE = "Issuer is required when License Type is 'FWF' or 'FWE'.";
			public const string FWLLicenseTypeIsRequiredForWildlifeCommercialPurpose = "A FWS Import/Export License is required for Wildlife Commercial Purpose.";
			public const string FWCLicenseTypeIsRequiredForLDS = "A License of type 'FWC' is mandatory for processing code 'LDS'.";
			public const string FWCShouldOnlyBeEnteredWhenProcessingCodeIsLDS = "License Type FWC should only be entered when the Processing Code is 'LDS'.";

			public const string BrokerContactNameRequiredForEDS = "Broker Name is required when FWS is Declared with a 'EDS' code.";
			public const string BrokerContactEmailRequiredForEDS = "Broker Email is required when FWS is Declared with a 'EDS' code.";
			public const string BrokerContactPhoneRequiredForEDS = "Broker Phone is required when FWS is Declared with a 'EDS' code.";
		}

		public static class PSC
		{
			public const int MaxDaysFromEntryDateToFilePSC = 300;
			public const string MayFilePSCUpTo300DaysFromEntryDate = "You may file a PSC up to {0} days after the date of entry.";

			public const string PSCMayBeFiledOnAcceptedEntry = "You may file a PSC only when the entry is accepted.";
			public const string MayFilePSCOnlyWhenEntryOnStatement = "You may file a PSC only when the entry is scheduled for payment on an ACH statement.";
			public const string MayFilePSCOnlyWhenPeriodicStatementIsFinalised = "You may not file a PSC on a periodic monthly statement until CBP has received payment on the statement.";

			public const string CannotChangeEntryTypeToADD_CVDTypeForPSC = "ADD/CVD Entry Type cannot be changed to a non-ADD/CVD type for PSC.";
			public const string NotAllowedToChangeReconIndicator = "Reconciliation is not allowed to be changed for PSC.";
			public const string NotAllowedToChangeIORNumber = "Importer of record number is not permitted to be changed by PSC.";
			public const string AccLiqRequestNotAllowedForAD_CVDEntry = "Accelerated Liquidation Request is not allowed for AD/CVD entry summaries.";
			public const string NotAllowedToChangeRateType = "A change between ad valorem and specific rates is not permitted for PSC.";
			public const string NotAllowedToChangeEntryPort = "Entry Port is not allowed to be changed for PSC.";
			public const string NotAllowedToChangePaymentType = "Payment Type is not allowed to be changed for PSC.";
			public const string NotAllowedToChangePSD = "Preliminary Statement Date is not allowed to be changed for PSC.";
			public const string NotAllowedToChangeStatementMonth = "Periodic Statement Month is not allowed to be changed for PSC.";
			public const string NotAllowedToChangeClientBranchDesig = "Client Branch Designation is not allowed to be changed for PSC.";
			public const string NotAllowedToChangeLocationOfGoods = "Location of Goods(Firms Code) is not allowed to be changed for PSC.";
			public const string NotAllowedToChangeLiveIndicator = "Live indicator is not allowed to be changed for PSC.";
			public const string CannotCertifyForPSC = "You should not certify for cargo release for PSC.";
			public const string NotAllowedToChangeConsolidated = "Consolidated indicator is not allowed to be changed for PSC.";
			public const string NotAllowedForInformalEntry = "A PSC is not allowed on an informal entry.";
			public const string EntrySummaryWasFiledAsInformalEntry = "A PSC is not allowed on an entry summary that has been filed as an informal entry.";
			public const string NoReasonCodeEntered = "One or more reason codes for PSC change are required at the header and/or line level.";

			public const string CustomsWontSeeThisChange = "Customs will not see this change as it is not included in 7501 messages for PSC.";
			public const string EntryTypeCannotBeChangedFromTIBForPSC = "When PSC is ticked, entry type cannot be changed from 23 to anything else.";
			public const string EntryTypeCannotBeChangedToTIBForPSC = "When PSC is ticked, entry type cannot be changed to 23.";
		}

		public static class PriorNotice
		{
			public const string Organisation = "{0} is mandatory for Prior Notice";
			public const string OrganisationCity = "{0} City is mandatory for Prior Notice";
			public const string OrganisationStateProvince = "{0} State/Province is mandatory for Prior Notice";
			public const string OrganisationCountry = "{0} Country is mandatory for Prior Notice";
			public const string OrganisationFirmName = "{0} Firm Name is mandatory for Prior Notice";
			public const string OrganisationPCode = "{0} ZIP or Mail Code is mandatory for Prior Notice";
			public const string OrganisationFDAContactFirstName = "{0} First Name (FDA Contact) is mandatory for Prior Notice";
			public const string OrganisationFDAContactLastName = "{0} Last Name (FDA Contact) is mandatory for Prior Notice";
			public const string OrganisationAddress = "{0} Address is mandatory for Prior Notice";
			public const string OrganisationFDAContactPhone = "{0} Phone Number (FDA Contact) is mandatory for Prior Notice";
			public const string OrganisationFDAContactPhoneFormat = "Phone numbers (FDA Contact) must be 10 to 14 digits in length.";
			public const string OrganisationFAXFormat = "FAX numbers (FDA Contact) must be 10 to 14 digits in length.";
			public const string OrganisationFAXOrEmailRequired = "Email or Fax on active contact are required for Prior Notice.";

			public const string SubmitterFirmType = "Submitter Firm Type is mandatory for Prior Notice";
			public const string SubmitterFirmTypeValid = "Submitter Firm Type must be one of the following values; M, S, C, I, U or F";

			public const string PortOfArrival = "Port Of Discharge is mandatory for Prior Notice";
			public const string DateOfArrivalRange = "PGA Arrival date can be a future date or a date up to 10 days in the past.";
			public const string DateTimeOfArrivalMandatory = "Date and Time Of Arrival is mandatory for Prior Notice or ACE FDA.";
			public const string TimeOfArrivalFormat = "Time Of Arrival may not be 00:00 nor 24:00";
			public const string DateOfArrivalMismatch = "PGA Arrival date does not match declaration's date of ETA at port of discharge";
			public const string DateOfArrivalEntry = "PGA Arrival date does not match declaration's date of arrival at port of entry";

			public const string CarrierType = "Reporting of the Carrier is mandatory for Prior Notice if Carrier SCAC has not been entered";
			public const string CarrierName = "Carrier Name is mandatory if Carrier Type is Carrier";
			public const string CarrierCountry = "Carrier Country is mandatory if Carrier Type is Carrier";

			public const string PrivatelyOwnedUSVehicleLicense = "Privately Owned Vehicle License Number is mandatory if Carrier Type is Privately Owned US Vehicle";
			public const string PrivatelyOwnedUSVehicleStateCode = "Privately Owned Vehicle State Code is mandatory if Carrier Type is Privately Owned US Vehicle";

			public const string PrivatelyOwnedForeignVehicleStateProvinceName = "Privately Owned Vehicle State/Province is mandatory if Carrier Type is Privately Owned Foreign Vehicle";

			public const string RailCarNumbersNotRequired = "Rail Car Numbers are only accepted for transport mode 20 and 21 - (RAI)";
			public const string DisclaimedAndNotFD3 = "Prior Notice can only be disclaimed if OGA indicator is FD3";
			public const string ForcedFDAIsDisclaimed = "You have forced sending Prior Notice, however you also have indicated that it is disclaimed. System will not send this line for Prior Notice.";
			public const string ConfirmationNumberFormat = "Prior Notice Confirmation Number should be 12 in length";
			public const string OwnerFirmType = "Owner Firm Type is mandatory for Prior Notice";
			public const string CountryOfShipping = "Country of Shipping is mandatory for Prior Notice";
			public const string CountryOfShippingNonUS = "US and PR are not valid Country of Shipping for Prior Notice";
			public const string FoodFacilityRegistrationNumber = "Food Facility Registration Number is mandatory for Prior Notice unless exempt";
			public const string FoodFacilityRegistrationNumberFormat = "Food Facility Registration Number format is invalid. The number should be 11 digits.";
			public const string FoodFacilityRegistrationNumberNotFromManufacturer = "Food Facility Registration Number does not match to number, (Config->Registration Numbers->PFR), registered on the currently selected Manufacturer Address.";
			public const string FoodFacilityReasonNowInvalid = "Reason Codes (G, I, J, L, M & O) are no longer valid. Refer to CSMS# 09-000213.";
			public const string ProducerFirmType = "Producer Firm Type is mandatory for Prior Notice unless exempt";

			public const string RelatedBill = "At least one related bill is mandatory for Prior Notice";
			public const string EntryType = "Entry Type is mandatory for Prior Notice.";
			public const string CarrierSCAC = "Carrier SCAC is required for Prior Notice.";

			public const string ContactEmailRequire = "A Contact Email is mandatory when requires customs broker reporting.";

			public const string EntryNumberRequiredForENTStandAlonePriorNotice = "Declaration Entry Filer Code and Entry Number is required when Stand Alone Prior Notice ID Type is 'ENT – Entry Number'. To assign a new entry number to this job, press the Allocate button next to Entry Number. ";
			public const string ZoneIDRequiredForFTZStandAlonePriorNotice = "FTZ Zone ID is required when Stand Alone Prior Notice ID Type is 'FTZ - FTZ admission number'.";
			public const string ControlNumberRequiredForFTZStandAlonePriorNotice = "FTZ Control Number is required when Stand Alone Prior Notice ID Type is 'FTZ - FTZ admission number'.";
			public const string MasterBillNumberRequiredForBLNStandAlonePriorNotice = "At least one master bill is required when Stand Alone Prior Notice ID Type is 'BLN - Bill Number'.";
			public const string ACSPriorNoticeTurnedOff = "ACS Stand Alone Prior Notice has been turned off.";
		}

		public static class FDA
		{
			public const string ContactForRequiresCustomsBrokerReporting = "A Contact is mandatory when requires customs broker reporting.";
			public const string Contact = "A Contact is mandatory for PGA Prior Notice.";
			public const string OGAInvValue = "This value should be greater than 0 if line price is greater than 0.";
			public const string FEIRequiredForForeignUltimateConsignee = "FDA Establishment Identifier (FEI) is required for a foreign ultimate consignee.";
			public const string FEI = "This address does not have an FEI Number.";
			public const string FEINotEntered = "FEI No. (FDA Establishment Identifier) should be the 'Ship-to-Site' Consignee number. The appropriate address should be entered here if the 'Ship-to-Site' Consignee number differs from the Ultimate Consignee number.";
			public const string ContactPhoneFormat = "If entering a US domestic phone number it must be 10 digits (area code + number).\r\nNon US phone numbers can be 10 to 14 digits in length.";
			public const string ContactEmail = "A Contact email is mandatory for PGA Prior Notice.";
			public const string ShipperRegistrationNumberNotFromFDAShipper = "Shipper Registration Number does not match the number, (Config->Registration Numbers->SFR), registered on the FDA Shipper.";
			public const string ContactFaxFormat = "If entering a US domestic fax number it must be 10 digits (area code + number).\r\nNon US fax numbers can be 10 to 14 digits in length.";
		}

		public static class DDTC
		{
			public const string EitherDDTCLicenseTypeOrDDTCExemptionCodeIsRequired = "For DDTC declaration, please enter either a License Type or an Exemption Code but not both.";
			public const string DDTCLicenseTypeIsRequired = "A DDTC License Type is required when License Number is specified.";
			public const string EitherDDTCLicenseNoOrDDTCExemptionCodeIsRequired = "For DDTC declaration, please entered either a License Number or an Exemption Code but not both.";
			public const string DDTCLicenseNoIsRequired = "A DDTC License Number is required when License Type is specified.";
		}

		public static class DOT
		{
			public const string DuplicateVIN = "Duplicate VINs are not allowed.";
		}

		public static class Statement
		{
			public const string PSDRequiredForNonPayType1 = "Preliminary Statement Date is required if you want to move entry onto another statement";
			public const string PSDNotAllowedForPayType1 = "Preliminary Statement Date is NOT allowed when changing Payment Type to 1 (Payments to be made on an individual basis)";
			public const string PSDMustBeFutureDate = "Preliminary Statement Date can only be moved to a future statement (Preliminary Statement Date must be greater than today's date)";
			public const string PSDIsInvalid = "Please enter a valid date.";
			public const string PayerUnitNoIsMandatory = "Please enter the payer's unit number.";
			public const string PayerUnitNoIsNotRequired = "Payer's unit number is not required for the selected payment method.";
			public const string PayerUnitNoLength = "Payer's unit number should be 6 characters in length";
			public const string PayerUnitNoShouldBeEnteredInRegistry = "It has been detected that this Organization represents customs. Payer Unit Number (PUN) will not be used as the default PUN when a ‘broker pays’ statement is paid. Default PUN’s can be entered in the Registry (Registry > Customs > United States of America > Import > ABI > Statements > Broker's Bank Accounts). PUN can be entered on Importer Organizations so that when the ‘importer pays’ the statement, PUN will default from the Importer Organization.";
			public const string PayByACHCredit = "If paid by ACH Credit, it is not required to send ACH Authorization messages on non-PMS statements.";
			public const string ShouldNotSendWhenWaitingForSUResponse = "A Statement Date update message is currently pending on this entry.   This message should not be sent until that process is completed.  Please exit the transaction and try again later.";
		}

		public static class EntrySummary
		{
			public static string AlreadyOnStatement(string notification)
			{
				return string.Format("Cannot send {0} because the entry is on a statement. This entry may be removed from a statement using the statement delete/add message.", notification);
			}

			public const string EntryType = "Entry Type is mandatory for Entry Summary.";
			public const string HasBeenCancelled = "Entry has been canceled by Customs and you cannot reuse the entry number and send messages.";
			public static string BrokerReferenceNumberDifferentAndReplacementShouldBeSent(string lastLodgedBrokerReference)
			{
				return string.Format("The broker reference number that was submitted to Customs was '{0}' and it is different to the current broker reference number. You should send an amendment message to replace the reference number instead of an original message.", lastLodgedBrokerReference);
			}

			public const string PSCFilingOfEntriesFiledByOtherBroker = "The first 7501 of the PSC entry originally filed by another broker should be filed with the original broker reference number entered in the 'Misc' tab.";
			public const string MinimumAssemblyOperationCostFor9802Products = "A minimum of $1 entered value is required for 9802 transactions. This validation will be refreshed when form is saved or Brokerage > Merge is clicked.";
		}

		public static class CargoRelease
		{
			public const string EntryType = "Entry Type is mandatory for Cargo Release.";
		}

		public static class Declaration
		{
			public static string InvalidPortForTransportMode(string transportType)
			{
				return string.Format(CultureInfo.InvariantCulture, "Invalid port for transport type. This port is not a valid {0} port.", transportType);
			}

			public const string BillTooLong = " is too long. Messages will be sent using the first 12 characters only.";
			public const string VesselRequired = "Vessel Name which is required when transport mode is SEA";
			public const string VesselNameLength = "Vessel Name is too long. Messages will be sent using the first {0} characters only.";
			public const string CarrierNameRequired = "Carrier Name is required when transport mode is Rail or Truck and Carrier SCAC is UNKN.";

			public const string DateOfArrivalRequired = "Date of Arrival";
			public const string DateAtEntryPortRequired = "Date at Entry Port";
			public const string EstimatedDateOfArrival = "Estimated Date of Arrival";
			public const string DestinationStateShouldBeInList = "Please enter a valid Destination State code. The code you have selected is not in the Destination State codes List.";
			public const string PortOfEntrySameAsDischargeWhenITPresent = "Port of Entry may not be the same as Port of Discharge when IT Details are entered.";
			public const string FirmsNotOnTheSameDistrict = "FIRMS location is not in the same district as Port Of Entry.";
			public const string ImportEntryShouldNotHaveUSOrPRAsCountryOfExport = "Goods may not be exported from the United States or Puerto Rico on an import entry.";
			public const string DataWillNoBeSentForEntrySummary = "Please be advised that this will not be sent for 7501 as Entry Type is 06(FTZ Consumption).";
			public const string BranchIsNotConfiguredUnableToSendMsg = "You are not allowed to send {0} messages, because you have no Home Branch configured in your staff profile.";
			public const string NotRLFJob = "You may not send a 'Non-RLF' {0} message for this Entry Port. The Home Branch in your staff profile is not related to the Entry Port (see Registry > Customs > United States of America > Import > ABI > Branch to District/Port Relationship).";
			public const string RLFJob = "You may not send a 'RLF' {0} message for this Entry Port. The Home Branch in your staff profile is related to the Entry Port (see Registry > Customs > United States of America > Import > ABI > Branch to District/Port Relationship).";
		}

		public static class Recon
		{
			public const string EntryFilerEntryNumberMandatory = "Both entry filer code and entry number should be entered.";
			public const string AlreadyReconciled = "This entry already belongs to another recon job, ";
			public const string IssueCodeConflict = "This entry does not have the same recon issue as the current recon job.";
			public const string NoImportEntriesShouldBeRefunded = "Aggregate reconciliation requires that, no entries should be refunded (For each entry, 'Total Recon Amount Payable' - 'Total Original Amount Paid' >= 0)";
			public const string EntryTypeNotReconcilable = "Only entries with entry type 01, 02 and 06 are valid for recon.";
			public const string IORConflict = "The entry must have the same Importer of Record as the recon job.";
			public const string SuretyCodeConflict = "The entry must have the same surety code as the recon job.";
			public const string ClearEntrySummaryDelete = "This entry summary has been deleted at Customs and should not be placed on a reconciliation.";
			public const string EntrySummaryCanceled = "This entry summary has been canceled by an authorized CBP user and should not be placed on a reconciliation.";
			public const string UnderlyingEntriesWithoutNoChangeAggregate = "Line Level Data is required for underlying entries, except for No Change Aggregate recon filings.";
			public const string OneEntryShouldHaveMaximum2Recons = "One entry can only belong to a maximum of 2 recons. One a 'VL' issue code and the other an 'NF' issue code.";
			public const string MaxLengthOfEntryNumberIs11 = "The maximum length of Filer Code + Entry Number should not be greater than 11.";
		}

		public static class InvoiceLine
		{
			public const string ValueShouldBeDeclaredOnParent = "{0} should be entered on parent line.";

			public const string ValueShouldBeDeclaredUnderSecondaryLines = "Values should be declared on a secondary tariff line.";
			public const string RepairTransactionShouldHaveComponentPrice = "This is a 9802 transactions. You should enter values both for 'Prov/Prog. Tariff' as well as the main tariff. For 'Prov/Prog. Tariff', please enter at least one value either at 'US/Orig. Value' or at 'Orig. Value Inv. Curr'.";
			public static string InvoiceQtyShouldBeEqualToSumOfWHSPackedQty(string term)
			{
				return string.Format(CultureInfo.InvariantCulture, "For {0} integration when Warehouse Package Quantity is not specified, the Invoice Quantity should equal to the sum of all Warehouse Packed Quantity entered under 'WHS Packs' tab.", term);
			}

			public static string WarehousePackageQuantityIsRequired(string term)
			{
				return string.Format(CultureInfo.InvariantCulture, "Please enter a Warehouse Package Quantity which is required for {0} integration. This should indicate how many packages this invoice line is packed into. E.g. If Invoice Quantity is 100 and these are packed into 10 packages, enter 10. In the case that goods are packed unequally, then you will need to enter the packing information on the WHS packs tab. Entry of data is also required on the WHS packs tab when multiple invoice lines are packed into the same package(s).", term);
			}

			public static class ImportTariff
			{
				public const string EnteredSecondaryTariffsDoNotMatchRule = "This tariff number requires {0} secondary line(s) to be entered, but you have entered {1} secondary line(s).";
				public const string ChildLineTariffNotFoundInRuleTariffs = "Tariffs are not related.  For a list of valid related tariff numbers see tariff rule STN under Maintain > CBP > Tariff Rules > STN Rule";
				public const string ProductCannotBeEmpty = "Product cannot be empty when an invoice line has secondary lines or a Prov/Prog. Tariff is entered.";
				public static string CannotMatchClassificationForPart(string partNum, string importerCode, string supplierCode, string partAttrib1Name, string partAttrib1Value, string partAttrib2Name, string partAttrib2Value, string partAttrib3Name, string partAttrib3Value, string serialNumberValue, ZDateTime effectiveDate)
				{
					return string.Format("Cannot match classification for product ({0}) based on Importer ({1}), Supplier ({2}), {3} ({4}), {5} ({6}), {7} ({8}), Serial Number ({9}) and Effective Date ({10}).", partNum, importerCode, supplierCode, partAttrib1Name, partAttrib1Value, partAttrib2Name, partAttrib2Value, partAttrib3Name, partAttrib3Value, serialNumberValue, effectiveDate);
				}
			}

			public static class ExportTariff
			{
				public static string CannotMatchClassificationForPart(string partNum, string importerCode, string supplierCode)
				{
					return string.Format("Cannot match classification for product ({0}) based on Importer ({1}) and Supplier ({2}).", partNum, importerCode, supplierCode);
				}
			}
		}

		public static class NMFS
		{
			public static string HarvestingDetailIsRequired(string type)
			{
				return string.Format("At least one Harvesting Detail is required for NMFS {0}.", type);
			}

			public const string DocumentIsRequiredForAMRFreshToothfish = "Document Type is required for NMFS AMR with fresh Toothfish.";
			public const string DocumentIsRequiredForHMSNonShark = "Document Type is required for NMFS HMS unless it's Shark type.";
			public const string DocumentIDIsRequired = "DIS Document ID is required to indicate that data has submitted to DIS.";
			public const string DocumentDetailsWontBeSent = "Please be warned that the document details entered here will not be sent in messages. The details entered at the Document Details grid will be sent instead.";
			public const string ConfidentialMustBeTrue = "Confidential must be true for {0}.";

			public const string AuthorizationTypeRequired = "Authorization Type is required if Authorization Number is entered.";
			public const string AuthorizationNumberRequired = "Authorization Number is required if Authorization Type is entered.";
		}

		public static class AES
		{
			public const string DeactivatedEntryWithActiveCustomsTransaction = "Some changes were made which has caused this entry to be deactivated, as this entry has already been lodged, you will need to withdraw it (Brokerage > File AES Message).\r\nThis entry can be viewed under Messages > Shipper's Export Declarations > Right Click > Show Deactivated Entries.";
		}

		public static class TTB
		{
			public const string ACOLANumberIsRequiredForThisTTBEntry = "A COLA Number is required for TTB entry.";
			public const string AForeignCertificateMaybeRequiredForThisTTBEntry = "A Foreign Certification may be required. Please confirm that none is needed before leaving this field blank.";
			public const string CigarIsRequiredForThisTTBEntry = "Cigar is required for TTB entry.";
			public const string ImporterPermitNumberIsRequiredForThisTTBEntry = "Importer Permit Number is required for TTB entry.";
			public const string MissingForeignCertificateCountry = "This data will not be saved unless a Foreign Certificate Country is specified.";
			public const string IRCRegistryNumberIsRequired = "The IRC registry number for the distilled spirits plant, bonded wine cellar is required when releasing bulk alcohol under IRC bond.";
			public const string TTBIssuedPermitNumberIsRequired = "The TTB-issued permit number indicating the IRC-bonded facility is required when releasing under bond.";
			public const string QuantityMustBeGreaterThanZero = "Quantity must be greater than zero.";
			public const string QuantityCannotBeGreaterThan999999 = "Quantity cannot be greater than 999999.";
			public const string UnitPriceMustBeGreaterThanZero = "Unit Price must be greater than zero.";
			public const string EitherPermitNumberOrExemptionCodeIsRequiredButNotBoth = "Please entered either a Permit Number or a Permit Exemption Code but not both.";
			public const string EitherCOLAOrExemptionCodeIsRequiredButNotBoth = "Please entered either a COLA number or a COLA Exemption Code but not both.";
			public const string UnitPriceMaximumSalePrice = "The tax determined by the unit sale price would result in a higher amount than the maximum tax allowed; please enter 76.322 instead.";
			public const string TTBPermitNumberFormatError = "TTB Permit Number should be in the format AA-A- followed by 1-5 digits or AA-AA- followed by 1-5 where A is alphabetic. (E.g. AA-A-1, AA-A-11111, AA-BB-1, AA-BB-123)";
			public const string ImporterPermitNumberIsNotRequiredWhenExemptionEntered = "Importer Permit Number is not required when Exemption is not empty.";
			public static string EINIsRequired(string organisationType, ZString companyCode)
			{
				return string.Format("This {0} does not have an EIN configured. Please configured under {1} > Organisation > Config.", organisationType, companyCode);
			}
		}

		public static class FTZ
		{
			public const string DataRequired = "{0} is required for Permit To Transfer message.";
			public const string DataRequiredWhenPTTIncluded = "{0} is required when Permit To Transfer is included in FT admission.";

			public const string CarrierWillNotBeSend = "Please be advised that this Carrier will not be sent, because PTT is not included in FT admission.";
			public const string CannotSendFTZMessageConfirmation = "{0} message should not be sent because the admission data has not been accepted by Customs. Do you want to override and send anyway?";
			public const string CannotSendFTZMessageInformation = "{0} message should not be sent because the admission data has not been accepted by Customs. You do not have the security rights to override this error and send.";
			public const string CannotSendFTZUnconcurrenceMessageConfirmation = "{0} message should not be sent because Concurrence has not been accepted by Customs. Do you want to override and send anyway?";
			public const string CannotSendFTZUnconcurrenceMessageInformation = "{0} message should not be sent because Concurrence has not been accepted by Customs. You do not have the security rights to override this error and send.";
			public const string CannotSendCancelPTTMessageConfirmation = "{0} message should not be sent because Permit To Transfer has not been accepted by Customs. Do you want to override and send anyway?";
			public const string CannotSendCancelPTTArrivalMessageConfirmation = "{0} message should not be sent because Permit To Transfer Arrival has not been accepted by Customs. Do you want to override and send anyway?";
			public const string CannotSendCancelPTTMessageInformation = "{0} message should not be sent because Permit To Transfer has not been accepted by Customs. You do not have the security rights to override this error and send.";
			public const string CannotSendCancelPTTArrivalMessageInformation = "{0} message should not be sent because Permit To Transfer Arrival has not been accepted by Customs. You do not have the security rights to override this error and send.";
		}

		public static class WHSPack
		{
			public const string PackageQtyIsRequired = "Please enter a package quantity greater than zero.";
			public const string DuplicatePackageReference = "Package Reference must be unique.";
		}

		public static class WHSPackLine
		{
			public const string PackedQtyIsRequired = "Please enter a packed quantity greater than zero.";
			public const string PackedQtyCannotBeGreaterThanInvoiceQuantity = "Please enter a packed quantity less than Invoice Quantity.";
			public const string DuplicatePackageReferenceAndInvoiceLineReference = "Package Reference and Invoice Line Reference combination must be unique; there is another Pack Line with this combination.";
		}

		public static class NHTSA
		{
			public static string DuplicateDocumentWithSameType(ZString documentType)
			{
				return string.Format("Duplicate {0} documents are not allowed.", documentType);
			}

			public const string InvalidAdditionalNumberFormat = "Number format is invalid, it should be 17 characters.";
			public const string InvalidLPCONumberFormatForRegisteredImporterNumber = "LPCO number should be in the following format: A-NN-NNN where A is alphabetic and N is a number.";
			public const string InvalidLPCONumberFormatForNHTSAImportPermissionLetterr = "LPCO number should be in the following format: NN-NNNN-NNNN where N is a number.";
			public const string InvalidLPCONumberFormatForVehicleEligbilityNumber = "LPCO number should be in the following format: AAA-NNN where A is alphabetic and N is a number.";
			public const string InvalidDOTSuretyCodeFormat = "DOT Surety Code should be 3 digits.";
			public const string ElectronicImageIsRequired = "Some of the documents listed below require electronic images to be submitted via DIS.";
			public const string ElectronicImageIsNotRequired = "Electronic Image was not required for any documents listed below.";
			public const string VINIsMandatoryForAllVehicles = "You have not entered a VIN number. Please go to Details > Additional Numbers and fill a number with type 'VIN'.";
			public const string YouHaveEnteredMultipleNumbersWithSameTypeAndNumber = "You have already entered an additional number with same type and number.";
			public const string DocumentRequired = "Document {0} - {1}{2} {3} required when Box {4} is used.";
		}

		public static class EntrySummaryQuery
		{
			public const string EntryNumberLength = "Entry Number should be 8 digits. Only first 8 characters will be sent in Entry Summary Query message.";
			public const string NotRequired = "If querying a specific entry, you cannot query ACE Entry Summary data based on status as well.\r\n(Outstanding Action and Date Range must be blank for a specific entry query.)";
			public const string ValueRequired = "If querying ACE Entry Summary data, either an Entry Number or Outstanding Action and Date Range must be entered.";
			public const string FilerCodeIsEmpty = "Filer Code is required for an Entry Summary query.";
		}
	}

	public static class USConstants
	{
		public const string NarrativeRejectedMessage = "TRANSACTION DATA REJECTED";
		public const string BatchRejected = "BATCH REJECTED";
		public const string Same = "SAME";
		public const string MultipleValueIndicator = "MULTI";
		public const string SeeAttachedIndicator = "SEE ATTACHED";

		public const string RecordsRequired = "ACCEPTED - RECORDS REQUIRED";
		public const string PaperRequired = "ENT-SUM ACCEPTD-PAPER NOW REQ'D";

		public const string Paperless = "57A";

		public const string AESRejectionDisposition = "R";

		public static ZDateTime PairedPortProgramEndDate
		{
			get { return new ZDateTime(2011, 01, 28); }
		}

		public static class DocsRequiredStatus
		{
			public const string DocsRequired = "57B";
			public const string DocsNowRequired = "57F";
			public const string PaperRequired = "58L";
		}

		public static class EntrySummaryDisposition
		{
			public const string DocsRequired = "DOCS";
			public const string CensusWarning = "Census";
			public const string Paperless = "PPLS";
		}

		public static class FDA
		{
			public const int RoundToDecimalPlaces = 2;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public static class AES
		{
			public const string AESEPAID = "EP1";
			public const string AESAMSID = "AM1";
			public const string AESATFID = "AT6";
			public const string FWSFW7ID = "FW7";
			public const string FWSFW8ID = "FW8";
			public const string AESDEAID = "DEA";
			public const string AESTTBID = "TTB";
			public const string NMFSNM7ID = "NM7";
			public const string NMFSNM8ID = "NM8";
			public const string NMFSNM9ID = "NM9";
		}
	}
}
