using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public static class ValidationConstants
	{
		public static string MessageErrorIfNonAlphanumericCharacters => Res.GetString("0e8faf28-6513-4492-bdcf-897d35425db3", "Please enter alphanumeric characters only.");

		public static string AlphanumericCharactersOnly(string caption) => Res.GetString("0b58dd58-cb35-48ba-8ba7-d0286dfa1a53", "{0} must only contain alphanumeric characters.", caption);

		public static class Declaration
		{
			public static string LengthForSONoOrManifest => Res.GetString("fbaeacee-78a7-4bc9-9c8c-64ec4502f163", "SO No / Manifest must be 4 characters in length.");

			public static string CallSignMandatory => Res.GetString("46e3edc5-32d1-45ec-9814-5e95f481d559", "Call-Sign cannot be empty when the job is Sea freight.");

			public static string LocationOfGoodsDoesNotBelongToCustomsOffice(string locationOfGoods, string customsOffice) => Res.GetString("f8567276-01ba-4f62-bad7-6dc0411e2cc7", "Location Of Goods ('{0}') does not belong to Customs Office ('{1}').", locationOfGoods, customsOffice);

			public static string CustomsOfficeNotForTransportMode => Res.GetString("3925cf9d-6055-4a65-ab3f-da5bfe745b85", "Selected Customs Office not for selected Transport Mode.");

			public static string ExportLocationDoesNotBelongToCustomsOffice(string locationOfGoods, string customsOffice) => Res.GetString("a7c0ff95-f62f-4bfb-a173-93a74ffed1a2", "Export Location ('{0}') does not belong to Customs Office ('{1}').", locationOfGoods, customsOffice);

			public static string ReceiptLocationDoesNotBelongToCustomsOffice(string locationOfGoods, string customsOffice) => Res.GetString("57D96F8C-F2A7-4D94-B6A8-BEF52275D03C", "Receipt Location ('{0}') does not belong to Customs Office ('{1}').", locationOfGoods, customsOffice);

			public static string GoodsLocationDoesNotBelongToCustomsOffice(CusEntryInstruction entryInstruction, string locationOfGoods, string customsOffice) => (entryInstruction?.JobDeclaration?.IsImport ?? false) ? ReceiptLocationDoesNotBelongToCustomsOffice(locationOfGoods, customsOffice) : ExportLocationDoesNotBelongToCustomsOffice(locationOfGoods, customsOffice);

			public static string BankAccountOnlyAllowsAlphanumericCharacters => Res.GetString("BAD2DD8D-2C0B-4B97-A07A-F1139A359857", "Bank Account only allows alphanumeric characters.");

			public static string CassNoOrAccountNoMandatory => Res.GetString("00bb44ac-c5bb-47a6-a51c-4619e8d10321", "Case No/ Account No cannot be empty when Payment Method is 2, 3, 4, 5, or 8.");

			public static string BrokerStaffShouldNotBeEmpty => Res.GetString("9FB84463-440F-40DA-AB47-5E14452419E4", "Broker Staff cannot be empty");

			public static string ShouldHasGovernmentVATCodeOrRodIdCardOrPassportNumber => Res.GetString("ec6d0c21-7fd9-458b-b780-8e8b7de834a5", "The selected organization does not have a Government VAT Code/ ROD ID card/ Passport Number.");

			public static string ApplicationCodeShouldNotBeEmpty => Res.GetString("15ECECD6-873D-443D-BBD3-8E1A505EE15B", "You must select a Message Mode.");

			public static string TheOrganizationShouldHaveEnglishCompanyName => Res.GetString("459C5259-805E-462F-B9A9-39C04A795EB4", "The selected organization does not have an English Name.");

			public static string TheOrganizationShouldHaveEnglishAddress => Res.GetString("03C0CD10-3B62-49CA-A5C7-8A2B72A19544", "The selected organization does not have an English Address.");

			public static string MailboxNotForTesting => Res.GetString("7EAE9644-A9BA-4B9E-A239-B4D20E2AA593", "The TW Customs message has been configured to be send in Test Mode, but the selected Mail Box is a Production account.");

			public static string MailboxNotForProduction => Res.GetString("1D4FDBCB-2C8B-417E-8136-B6A81A89DAF5", "The TW Customs message has been configured to be send in Production Mode, but the selected Mail Box is a Test account.");

			public static string ContainerModeMustbeOther(string declarationType) => Res.GetString("ED602265-8077-4E72-AF08-887B7E6B4BD2", "When Declaration Type is '{0}', Container Mode must be 'OTH'.", declarationType);

			public static string CertificateWillExpire(string caption) => Res.GetString("CB99FE16-B03E-4338-904E-2C0992624531", "The selected mail box certificate will expire on {0}. Please remember to renew it.", caption);

			public static string CertificateExpired => Res.GetString("6A692BDD-C325-49FA-A6CD-144EDC8E340B", "The selected mail box certificate has expired.");

			public static string Z99UNLOCOCodeMustBe5Characters => Res.GetString("3E5EDB29-B4D0-484E-BA1E-09735C4A0480", "A Z99 UNLOCO Code must be 5 characters longs.");

			public static string Z99UNLOCOCodeMustHasValidCountryCode => Res.GetString("1BE405E7-5574-4E4A-84F7-F416B48D30E5", "The first two characters of a Z99 location must be a valid country code.");

			public static string EnterForeignPortCodeMessage => Res.GetString("80C2D60F-FE2F-4D5C-8EE0-75E7FD94677A", "This port code is invalid. Please enter a foreign port code.");

			public static string EnterTWPortCodeMessage => Res.GetString("E99E8369-0C41-49DE-822B-94A23DF8590D", "This port code is invalid. Please enter a port code from Taiwan.");

			public static string AutomaticallyDeclareNIL(string caption) => Res.GetString("6b03e812-3010-4f56-94fc-4c0615742341", "System will automatically declare 'NIL' when '{0}' is empty.", caption);

			public static string AutomaticallyImporterDeclareEntryNumber(string caption) => Res.GetString("ec8b6d30-df51-45c6-8ae2-a7e6461e9bfd", "System will automatically declare 'Entry Number' into '{0}' when Importer is Free Trade Zone.", caption);

			public static string AutomaticallySupplierDeclareEntryNumber(string caption) => Res.GetString("eb4845c7-d069-4d97-ac51-7a51b51ef820", "System will automatically declare 'Entry Number' into '{0}' when Supplier is Free Trade Zone.", caption);

			public static class Declarant
			{
				public static string LocalCompanyNameResString => Res.GetString("EF1AA3B3-7C29-4793-BFC9-C2188FE6C907", "Declarant Local Company Name");

				public static string LocalAddressResString => Res.GetString("7577CBA8-A17A-4AF9-B9AB-3A7909C7A515", "Declarant Local Address");

				public static string PhoneResString => Res.GetString("1612E9B3-212D-41F4-AC16-5436485F30F2", "Declarant Telephone Number");

				public static string EmailResString => Res.GetString("E947248B-C7BB-4183-8BDD-D3DF503D447C", "Declarant Email Address");

				public static string ContactNameResString => Res.GetString("D449616B-CC6E-450F-9AFD-136F5D876DC5", "Declarant Contact Name");

				public static string AValidIDIsReruired => Res.GetString("DC210077-39C7-498F-97BE-EFF5194FFB48", "You have not entered a Declarant ID: A valid TW-VAT or TW-PAS or TW- PID number is required for Declarant.");
			}
		}

		public static class InvoiceLine
		{
			public static string WarningPartCodeFoundButNotRelatedToOwner => Res.GetString("0f8bbbf5-c5b2-48d4-896c-f004d81099f7", "A Product with this code exists, but it is inactive or the Owner relationship on that Product does not match this invoice. Either add a new Product (F3), make it active (F4, add a filter to show inactive records, search and edit), or change the Owner relationship on the existing Product (F4 then edit a Product).");

			public static string WarningPartCodesFoundButNotRelatedToOwner => Res.GetString("b417f431-7c56-48d9-9150-978142973999", "Several Products with this code exist, but none of then have an Owner relationship which matches this invoice and are active. Either add a new Product (F3), activate a product (F4, add a filter to show inactive records, search and edit), or change the Owner relationship on an existing Product (F4 then edit a Product).");

			public static string WarningTariffAdditionalCodeNotAlphanumericCharacters => Res.GetString("2D8718E2-36C7-4264-B975-4BDAAEACB4B2", "Tariff additional code only accepts alphanumeric characters.");

			public static string CusValueConvRatioNegative => Res.GetString("55C9C8FD-0EEA-487D-8E0D-7D5626F6F8F3", "Conversion Ratio cannot be negative.");

			public static string GradeOnlyAllowsAlphanumericCharacters => Res.GetString("BB1B6A0A-33D8-4F67-B374-593E56790230", "Grade only allows alphanumeric characters.");

			public static string ThicknessOnlyAllowsAlphanumericCharacters => Res.GetString("E17204DE-FB84-4E11-BED0-3BEF44A7F3B7", "Thickness only allows alphanumeric characters.");

			public static string RangeMustBeBetween0And1 => Res.GetString("0E5DCBE5-D44F-4FD1-8181-388B6091F52E", "The range must be between 0 and 1.");

			public static string InvalidValue(string caption) => Res.GetString("7cd4a181-9185-4702-ad2d-3629326f59c3", "The {0} you entered is invalid.", caption);

			public static string BarCodeShouldBe13Characters => Res.GetString("cdcbcb81-336e-49ed-b237-1606e8843140", "Bar Code should be 13 characters long.");

			public static string PHScaleRanges => Res.GetString("3e78c80a-5f36-44d7-a432-3c2a88dd7d33", "The PH scale ranges from 0 to 14.");

			public static string SterilizationScaleRanges => Res.GetString("3817a551-3a8f-479c-9c74-3a8950d531d4", "The Sterilization scale ranges from 0 to 9.9");

			public static string FoodContentMaximumRows => Res.GetString("0a28f09f-e282-4748-860b-31e245721f03", "Food Content can have up to 99 rows.");

			public static string CustomsSupplierPartNoShouldNotBeEmpty => Res.GetString("AD3A951E-47B9-47FE-B002-A5B50F54B388", "Supplier Part No. is required when Declaration Type is on of B2, B8, B9, D2, D5, D7, F2, F3 or F5");

			public static string CustomsSupplierPartNoIsRequiredWhenSupplierIsFTZ => Res.GetString("31F67877-69D4-4866-BCF9-1544C81B88F1", "Supplier Part No. is required when Supplier is Free Trade Zone.");

			public static string CustomsOwnerPartNoShouldNotBeEmptyForDeclarationType => Res.GetString("F73A08C6-C673-4B43-B27D-460F41BBA78E", "Owner Part No. is required when Declaration Type is one of B1, B2, D1, D7, D8, F1, F3");

			public static string CustomsOwnerPartNoShouldNotBeEmptyForProcedure => Res.GetString("35D16528-6DCC-43E3-949F-883B8B8B66A9", "Owner Part No. is required when Duty Treatment is one of 56, 5Y, 58, 5C, 98, 99.");

			public static string CustomsOwnerPartNoIsRequiredWhenImporterIsFTZ => Res.GetString("2393764A-22A3-4816-898E-8E49438BDE22", "Owner Part No. is required when Importer is Free Trade Zone.");

			public static string TariffDoesnotExist => Res.GetString("0F08E099-6E4C-4CFA-9BED-12A446A810B7", "The entered Tariff Code is invalid.");

			public static string TotalQuantityForPackagesPivotNotEqualToInvoiceQuantity(ZDecimal totalQuantityForPackagesPivot, ZDecimal invoiceQuantity) => Res.GetString("FA526FD2-A24A-4619-B32B-F1A978A4AD8D", "The total number of items included in the packages is ({0}). It does not add up to the invoice quantity ({1}).", totalQuantityForPackagesPivot.ToStringTrimZeros(), invoiceQuantity.ToStringTrimZeros());

			public static string ExtraInfoErrorMessage => Res.GetString("f444a315-4751-413b-9585-e59f5c9829b7", "Extra Info. is can only be used when Exam Mode is Written Review (8).");

			public static string IncorrectDutyTreatmentTypesForD5 => Res.GetString("0e773328-1790-4245-a6ab-3bcf549a9a93", "When shipping goods from Bonded Warehouse/Logistic Center to Free Trade Zone, Duty Treatment must be '97', '98', '9U', '1A', or '8A'.");

			public static string IncorrectDutyTreatmentTypesForB8 => Res.GetString("8e4f5556-1bb6-4d21-ad51-d18724599e23", "When shipping goods from Export Processing Zone/Bonded Factory/Agricultural Biotechnology Park/Science Industrial Park to Oversea, Duty Treatment must be  '81', '8A', '82', '92', or '9M'.");

			public static string IncorrectDutyTreatmentTypes => Res.GetString("82b42ef6-f535-49ca-b047-d3d4cebb1884", "The selected Duty Treatment is invalid.");

			public static string DisplacementShouldBeOnlyNumerics => Res.GetString("d7202f86-87ff-4ede-a2de-1aae6e91a6d3", "The Displacement(cc) format is invalid, should be only numerics.");

			public static string DisplacementOutOfRange => Res.GetString("b5f6af19-6180-4480-b62f-6bb2a3aaad4c", "The Displacement(cc) should be greater than zero and less than 1000000.");

			public static string CustomsRequirementT => Res.GetString("CCC094E5-4B01-4EB2-A38D-D05354618B6F", "Customs Requirement T - Subject to commodity tax.");

			public static string CustomsRequirementB => Res.GetString("10CCD6E8-F456-487E-8435-59563468E3CA", "Customs Requirement B - Subject to alcohol tax.");

			public static string IncorrectDutyTreatmentTypesForD8 => Res.GetString("49B9C5E7-2D67-4E3D-867F-CE167BEF6653", "When goods are being shipped from Oversea to Bonded Warehouse or Logistic Center, Duty Treatment must be '98' or '92'.");

			public static string IncorrectDutyTreatmentTypesForL1 => Res.GetString("3434C0D0-5F88-45C4-9EE0-F68BD1412A10", "When Declaration Type is 'L1', Duty Treatment must be '99'.");

			public static string IncorrectDutyTreatmentTypesForNonDutyFreeTariff => Res.GetString("FDFE85FE-95CF-4214-8DAD-187D69C18FB1", "Duty Treatment '50' can only be used when the selected tariff rate is duty-free.");

			public static string IncorrectDutyTreatmentTypesForD1 => Res.GetString("D33F2D47-FB94-448D-8C7F-773E1C37A8F5", "Mode of Statistics must be '97' or '98' or '9T' when Declaration Type is 'D1'.");

			public static string IncorrectDutyTreatmentTypesForB1 => Res.GetString("265BBBBE-C516-41F9-B4D4-E79DAE8CC4DB", "Mode of Statistics must be '97' or '9T' or '9U' when Declaration Type is 'B1'.");

			public static string IncorrectDutyTreatmentTypesForB6 => Res.GetString("61C7B6AF-C01E-4838-983D-D2F8FEF5A4AA", "The Duty Treatment of the first item must be '56' or '5Y' or '58' or '5C' or '99' when Declaration Type is 'B6'.");

			public static string IncorrectDutyTreatmentTypesForF2 => Res.GetString("AEE77534-BF10-41A9-8B6D-F4E0241F2819", "Duty Treatment must be 'EF' when one of the Duty Treatment is 'EF'.");

			public static string MoreThan10AssignedNumbersPerInvoiceLine => Res.GetString("FFF9DE6C-BE41-4724-B2AC-F9812798DA08", "The environmental protection code entered in this invoice line will be included in the message as an assigned number. You cannot have more than 10 assigned numbers per message. The extra assigned numbers will not be included in the message.");

			public static string TextileWidthIsNotRequired => Res.GetString("e49cbf73-b5b8-4407-8bf4-43f26a3a11f4", "Textile Width is not required for the selected tariff.");

			public static string CustomsRequirementC => Res.GetString("b726af11-9449-4aa0-8a59-7a842b96e016", "Customs Requirement C - Subject to tobacco tax & health and welfare surcharge.");

			public static string CustomsRequirementSPartially => Res.GetString("c914966d-2f44-433e-8e26-d13753db54dd", "Customs Requirement S* - Some of import countries shall extra enclose the certificate of origin issued by either the government of the country of origin or the government authorized units.");

			public static string CustomsSecondQuantityWarningMessage => Res.GetString("e266bb47-07d4-44ea-930f-ab0a61821f64", "In case where a complete set of machinery or a commodity made of several different component parts has be imported in a knockdown state and packed separately, the quantity of the first batch of such machinery or commodity shall be declared as one single unit, and the quantity for the rest of the batches to be imported subsequently as \"0\".");

			public static string TariffCodeIsEmpty => Res.GetString("cd5af7bd-4442-46e0-bf3f-9f6ed0a952a5", "Tariff may not be empty.");

			public static string TariffCodeIsNotValid => Res.GetString("db450e2e-51ee-46d9-aa42-d19697d0da76", "The Tariff Code entered is not valid for the current context.");

			public static string WarningForImportRegulation602(string caption) => Res.GetString("9bec31cd-4832-4e36-ba47-7db53c2f28cd", "{0} might be required when the goods are subject to Import Regulation: 602.", caption);

			public static string IsModeOfStatisticsRequirePermitNumberMessage => Res.GetString("8D2F9CAF-8DE5-40DB-BF62-9706FBA58DF6", "The selected Mode of Statistics requires a Permit Number.");
		}

		public static class Package
		{
			public static string SameWeightUnit => Res.GetString("575f0908-889f-45d8-b2df-0596030f2e91", "Net Weight and Gross Weight must have the same unit.");

			public static string NetWeightLessThanGrossWeight => Res.GetString("9bda98e2-1aba-4cc7-bdcb-adec0b852fb1", "Net Weight must be less than Gross Weight.");

			public static string ValueShouldNotBeNegative => Res.GetString("d60b5591-00fd-4e58-9604-66e1c514d5ec", "Value should not be negative.");

			public static string NetWeightUQIsRequiredWhenNetWeightIsGreaterThanZero => Res.GetString("62ef2562-bc67-4c45-9863-b9ace82ac9a1", "Net Weight Unit is required when Net Weight is greater than 0.");

			public static string GrossWeightUQIsRequiredWhenGrossWeightIsGreaterThanZero => Res.GetString("2893b059-40ef-4d71-b430-15c90554bd68", "Gross Weight Unit is required when Gross Weight is greater than 0.");

			public static string DimensionUQIsRequiredWhenLengthOrHeightOrWidthIsGreaterThanZero => Res.GetString("64d91bf1-b695-4983-b113-edb828df7341", "Dimension Unit is required when one of Length/ Height/ Width is greater than 0.");

			public static string VolumeUQIsRequiredWhenVolumeIsGreaterThanZero => Res.GetString("cd52a36a-a752-4df2-8053-cb77b5dac370", "Volume Unit is required when Volume is greater than 0.");
		}

		public static class Bill
		{
			public static string ErrorOneHouseBillOnly => Res.GetString("74a465de-1300-4fe4-b17f-1df0966c78d8", "There is already a house bill and a job can have only one house bill.");

			public static string ContainerNoteShouldNotExistAtTheSameTime => Res.GetString("0531a8e1-f72a-4ff1-b082-dc4efb360a29", "Container Note should not exist when there is a House Bill or a Master Bill.");

			public static string ContainerNoteMaximumRows => Res.GetString("57f45c9c-0269-4a7a-975d-813fda72199c", "Only 99 Container Notes will be sent to the customs.");
		}

		public static class EntryInstruction
		{
			public static string WHSMonthAllowedValue => Res.GetString("8730341F-AC6A-4AA9-8464-1BCBECEC7ADA", "Month should be 1-12");

			public static string DaysOfDelayedDeclarationAllowedValue => Res.GetString("55A27EF3-C263-431D-9344-8EB9C1C43692", "Days of Delayed Declaration should be 0 - 20");

			public static string DeclarationDuplicateDuplicated => Res.GetString("d1aa71d4-01b5-47fa-a880-47a68711136c", "The select Type already exists.");

			public static string AllInvoiceLineDutyTreatmentMustBe35ForShortInventory => Res.GetString("51B1054F-D32B-4F64-8B98-11A421D9A653", "When the declaration is about repaying duties for short inventory, all invoice lines must report Duty Treatment of '35'.");

			public static string ReasonForDutyMustBe01or13or99 => Res.GetString("4C2CB0EB-08ED-407E-A1E7-79992DD6F9E7", "When goods are being shipped from Factory to Duty Levying Area, if an invoice line with Duty Treatment of '31', '35', or '50' exists, Reason for Duty must be '01', '13', or '99'.");

			public static string ReasonForDutyMustBe04or05or06or07or13 => Res.GetString("5066A066-BF14-41F1-B893-56E4EAB36596", "When goods are being shipped from Free Trade Zone to Duty Levying Area, if there is an invoice line with Duty Treatment of '35', Reason for Duty must be '04', '05', '06', '07', or '13'.");
		}

		public static class CusInBondMoveHeader
		{
			public static string TW_ExportVesselREGLength => Res.GetString("9971b6c4-1827-4f6c-a93f-abaa0d37defc", "The length of Vessel Reg must be 6 characters.");
			public static string OnlyLettersAndNumbersForExportVesselREG => Res.GetString("d2590f80-010b-4e21-92a3-20d8c1b03f84", "Vessel Reg must be only letters and numbers.");

			public static string DestinationAndDestinationUnBothNotEmpty => Res.GetString("E8660ECE-5770-49B4-B972-7B8AD95A0911", "Destination and Destination (UN) cannot co-exist.");
		}

		public static class MedicalInstrumentOrFood
		{
			public static string CertificateNumberLength => Res.GetString("d2286cac-cbce-4a64-93d5-896049a83d6f", "The length of Certificate Number must be 14 characters.");
		}

		public static class PreviousBonded
		{
			public static string PreviousBondedEntryNumberLength => Res.GetString("05D5E37B-CE59-4768-9A88-FF60F6E79B03", "Previous Bonded Entry Number must be 14 characters long.");
		}

		public static class TypeApprovalCertificateNumber
		{
			public static string AlphanumericCharactersOnlyForCertificateNo => Res.GetString("b2722b32-5260-489a-bc75-1ee8914a8af0", "Certificate No contains non alphanumeric characters.");

			public static string LengthForCertificateNo => Res.GetString("663c6d3b-ffde-4f57-94e3-bb38369b55b4", "The length of Certificate No must be 14 characters.");

			public static string AlphanumericCharactersOnlyForAuthorizedParty => Res.GetString("bc15eb40-8768-4a3a-9d7e-2f5e89d425af", "Authorized Party contains non alphanumeric characters.");
		}

		public static class CusInBondHeader
		{
			public static string BrokerStaffIsNotEmpty => Res.GetString("A20F57FD-068F-497C-803B-04B25B33ADF8", "Broker Staff cannot be empty.");

			public static string BrokerStaffNotHaveValidCertificateNumber => Res.GetString("71A8F4D2-8F0D-46A1-8236-FB433F9B0A9D", "The selected Broker Staff does not have a Customs Clearance Agent of Special Examination Certificate Number.");

			public static string MissingVATorPIDorPAS => Res.GetString("15B529B9-907E-4E87-946A-A54EAEABD079", "The selected Importer is missing a VAT/PID/PAS code.");

			public static string DifferentTransportMode => Res.GetString("299C35EF-C040-4B72-8E0F-D453B01567E1", "The selected Transport Mode is not valid because it does not share the same transport mode with the selected Office of Receipt.");
		}

		public static class CusInBondBill
		{
			public static string LengthForSoNo => Res.GetString("6d0cb5e1-e9eb-4dae-994d-08ed7708d3ad", "So No should be exactly 4 characters long.");

			public static string LengthForMenifestNo => Res.GetString("887f9a38-762d-4f94-b330-a580469cbc5a", "Manifest No should be exactly 4 characters long.");

			public static string NumbersAndLettersOnlyForSoNo => Res.GetString("679b1c4f-4940-4572-8ba0-0656e2dbfee8", "SO No must consist of numbers and letters only.");

			public static string NumbersAndLettersOnlyForMenifestNo => Res.GetString("71ffbaa2-bd82-4356-b763-b2ae3b61c0f3", "Manifest No must consist of numbers and letters only.");

			public static string TotalGrossWeightUnitIsNotEmpty => Res.GetString("F20B1653-B3A7-497D-871C-AFE387E827DC", "Total Gross Weight Unit cannot be empty.");

			public static string ManifestQuantityUnitIsNotEmpty => Res.GetString("784E560C-3C75-43D2-BA3C-D1FD097D0EDC", "Manifest Quantity Unit cannot be empty.");
		}

		public static class InvoiceLineTax
		{
			public static string DuplicateTariffTypeFound(string typeDesc) => Res.GetString("59a950c8-f50c-40f4-8f39-d4878811f8f1", "There is already another record for {0}.", typeDesc);

			public static string TariffDoesNotBelongToType(string tariff, string type) => Res.GetString("eccaf548-ff88-4b39-ac5e-c204f9b1f82b", "Tariff ('{0}') does not belong to tariff type ('{1}').", tariff, type);

			public static string ChildTariffDoesNotBelongToMainTariff(string childTariff, string mainTariff) => Res.GetString("34d58486-cc35-47d6-b2a4-7e20b4ebcf49", "Child tariff ('{0}') does not belong to main tariff ('{1}').", childTariff, mainTariff);
		}

		public static class CusInBondContainer
		{
			public static string ContainerNumberAlphanumericCharactersOnly => Res.GetString("03aa8dce-3363-4616-8170-73f82433cc9c", "Container Number must only contain alphanumeric characters.");

			public static string ContainerNumberAlreadyExists => Res.GetString("1D82D766-846E-4D3B-AA39-F781DDE4A9BF", "This Container Number already exists.");
		}

		public static class MessageSendingObject
		{
			public static string EntryStatusShouldBeIEM => Res.GetString("15BD3B0A-842F-43A0-B514-F007294BC3DF", "No Goods Examination is requested for this Declaration.");

			public static string DeclarationDateShouldBeToday => Res.GetString("A98D7DE9-4DAF-4512-888E-35B2BA2C76DD", "The Declaration Date is not Today.");

			public static string ActionCodeIsInvalidWhenNotReceived => Res.GetString("69F02E24-B062-43C8-BBE1-5067323E60D2", "The selected value is invalid because this job has not received any customs responses. To force send, tick the below 'Continue to send event though the selected message(s) contains validation errors.' check box.");

			public static string ActionCodeIsInvalidWhenReceived => Res.GetString("55EF1EAA-8A85-4099-A167-71AE86C404FE", "The selected value is invalid because this job has been received by Taiwan Customs. To force send, tick the below 'Continue to send event though the selected message(s) contains validation errors.' check box.");
		}

		public static class CusTWControllingMessageHeader
		{
			public static string LengthForPermitNo => Res.GetString("03A30E75-C0C9-4529-A3EC-334B0D04F11A", "Permit No must be 14 characters long.");

			public static string ProcessingUnitDoesNotBelongToControllingAgency(string processingUnit, string controllingAgency) => Res.GetString("10c4a691-4187-4cf6-a0c8-d969a1d15837", "Processing Unit ('{0}') does not belong to Controlling Agency ('{1}').", processingUnit, controllingAgency);

			public static string PreviousPermitNumberLengthMustbe14 => Res.GetString("E3430FF5-81AF-4B8D-B98F-D3C1A955D56D", "Previous Permit Number must be 14 characters long.");

			public static string EthanolPermitNumberCannotBeDuplicated => Res.GetString("159b5a6c-4ef4-4c12-8f69-a402a34bfdb3", "Ethanol Permit Number cannot be duplicated.");

			public static string SpecialCodeCannotBeDuplicated => Res.GetString("D352D814-337D-45B1-894C-93DF7B622ACF", "Special Code cannot be duplicated.");

			public static string CapitalLettersAndNumbersOnly => Res.GetString("5226e855-f532-4a1e-890d-52207d59ba69", "Allow only capitalized letters and numbers.");

			public static string SpecialApplicationMustBeTickedForOriginalQuantity => Res.GetString("0ECCE13D-B2D0-4772-84BF-548392AD32C3", "Special Application must be ticked when Original Copy more than '5'.");

			public static string SpecialApplicationMustBeTickedForCopyQuantity => Res.GetString("F0A331B7-9246-4E4E-98F4-7B1EC05F9DF0", "Special Application must be ticked when Duplicate Copy more than '10'.");

			public static string OriginalQuantityCannotMoreThanOne => Res.GetString("35516459-E17C-45EE-9E00-C933CF00F626", "Original Copy cannot more than 1 when Certificate Type is '15'.");

			public static string CanNotAssignECFAHeaderToLines => Res.GetString("F69944B9-EEAD-44E7-87BA-B38C23CFDA9D", "Cannot link Invoice Lines more than 20 when Certificate Type is '15'.");

			public static string ProcessingNumberIsRequired => Res.GetString("C0683450-D233-4BF0-9885-B9079ABBBDA0", "Processing Number is required when Action is '4' or '17'");

			public static string Address_ChineseAddressIsRequired => Res.GetString("23446F0C-7036-4081-B90E-B2D415CCB58F", "The selected address does not have Chinese address.");

			public static string Address_ChineseCompanyNameIsRequired => Res.GetString("865A2F0A-90A2-47A7-AB8F-0382B5EE7B3C", "The selected address does not have Chinese Company name.");

			public static string Address_PhoneIsRequire => Res.GetString("4543A333-F890-4669-9EDA-3C050B678801", "The selected address does not have phone.");

			public static class Importer
			{
				public static string CompanyNameResString => Res.GetString("58C93030-72B5-4863-942D-02F23AE012EC", "Importer Company Name");

				public static string AddressResString => Res.GetString("00CC6201-95EB-4735-8B6E-AC9C7B3ECFF6", "Importer Address");

				public static string AValidIDIsReruired => Res.GetString("cf70dc42-2e7a-4b3a-a7c1-ad050bc6c7e0", "You have not entered an Importer ID: A valid TW-VAT or TW-PAS or TW-PID number is required for Importer. To create a valid TW-VAT or TW-PAS or TW-PID, visit Organization > Details > Config > Registration.");

				public static string ContactInformationResString => Res.GetString("DED0001E-BE51-43F6-800D-041AC11732AB", "Importer Contact Information");

				public static string CountryCodeResString => Res.GetString("6CFCBBFA-82C2-415B-9568-A397BFA513CA", "Importer Country/Region Code");

				public static string PhoneResString => Res.GetString("0E39C070-48FB-479A-B094-85E8670F7E12", "Importer Telephone Number");

				public static string LocalCompanyNameResString => Res.GetString("AEA9AA46-5A99-44F7-B9BA-B6EE612872F2", "Importer Local Company Name");

				public static string LocalAddressResString => Res.GetString("06774759-3B5C-42AA-9359-5F18256E4CD0", "Importer Local Address");
			}

			public static class Supplier
			{
				public static string CompanyNameResString => Res.GetString("138EC9C9-4FB0-41D0-B368-DDEAB20E6C66", "Supplier Company Name");

				public static string AddressResString => Res.GetString("B9403FD0-688B-42AE-ADB4-F368BAEDBC71", "Supplier Address");

				public static string CountryRegionResString => Res.GetString("2275EA56-81F6-4072-838D-435DA513B8F4", "Supplier Country/Region Code");

				public static string AValidIDIsReruired => Res.GetString("3a2f7ccf-1128-4742-b932-461b2c088c33", "You have not entered a Supplier ID: A valid TW-VAT or TW-PAS or TW-PID number is required for Supplier. To create a valid TW-VAT or TW-PAS or TW-PID, visit Organization > Details > Config > Registration.");

				public static string IdShouldBeSameAsApplicantId => Res.GetString("6ba45340-4246-4e53-8407-758cd03c4171", "Please make sure the Supplier ID is same as Applicant.");

				public static string ContactPhoneResString => Res.GetString("5de14241-0971-45d3-b247-b4ff98a8b027", "Supplier Contact Phone");

				public static string ContactFaxResString => Res.GetString("1a652b76-d6ea-4926-a88f-960c0396dd59", "Supplier Contact Fax");

				public static string ContactEmailResString => Res.GetString("ded48e53-87c2-418b-849b-ae6778dac328", "Supplier Contact Email");

				public static string LocalCompanyNameResString => Res.GetString("907417E6-A8B4-47BF-BA9E-4E85822F3FA4", "Supplier Local Company Name");

				public static string LocalAddressResString => Res.GetString("2CA6D962-2FB5-4E3F-A6C5-04E2422AA41A", "Supplier Local Address");
			}

			public static class Applicant
			{
				public static string LocalAddressIsRequired => Res.GetString("71194170-36D8-4B78-B169-9CE7801AB60E", "You have not entered an Applicant Local Address");

				public static string LocalCompanyNameIsRequired => Res.GetString("579AE5D0-468B-4C54-984E-4A0F55973E04", "You have not entered an Applicant Local Company Name");

				public static string CompanyNameIsRequired => Res.GetString("96049B29-022F-43FC-B845-0FED1C9656FC", "You have not entered an Applicant Company Name");

				public static string TelephoneNumberIsRequired => Res.GetString("97FBCB91-8340-4918-9420-26B37EC6737D", "You have not entered an Applicant Telephone Number");

				public static string IDIsRequired => Res.GetString("DCDC5854-568B-4E61-BDA9-E571582CE7B5", "You have not entered a Applicant ID: A valid TW-VAT or TW-PAS or TW- PID number is required for Applicant");
			}

			public static class Buyer
			{
				public static string CompanyNameResString => Res.GetString("92542B05-2764-4DEA-8AD0-CA5EA2B3DA45", "Buyer Company Name");
			}

			public static class LocalProcessor
			{
				public static string CompanyNameResString => Res.GetString("3524A834-2E52-4826-850D-3FAD5D88A040", "Local Processor Company Name");

				public static string AddressResString => Res.GetString("1B5053E7-0DD3-43E2-834A-5FCB5927F28F", "Local Processor Address");

				public static string AValidIDIsReruired => Res.GetString("E99F6BFD-E1AB-45FB-9EBA-6E540075686D", "You have not entered a Local Processor ID: A valid TW-VAT or TW-PAS or TW-PID or TW-FRI number is required for Local Processor. To create a valid TW-VAT or TW-PAS or TW-PID or TW-FRI, visit Organization > Details > Config > Registration.");

				public static string ContactInformationResString => Res.GetString("AEABF727-B53E-4564-9867-8D2B8DBEBC45", "Local Processor Contact Information");

				public static string LocalAddressResString => Res.GetString("09A0A415-F139-4633-B980-092BAA1732DE", "Local Processor Local Address");

				public static string LocalCompanyNameResString => Res.GetString("DF57640B-A516-4F4C-8F53-F5CC0D7CDC4A", "Local Processor Local Company Name");

				public static string LocalPhoneResString => Res.GetString("011230E6-E081-4306-828E-381E2A3C7ADF", "Local Processor Telephone Number");
			}
		}

		public static class BondedFactory
		{
			public static string ImpCusCodenotCBFnorEPZnorFTZ => Res.GetString("8D9551D6-45FB-4F72-BAC2-6246FE6557F8", "Previous Bonded Parties should be of Organization Registration Code EPZ or CBF or FTZ. To create a valid TW-EPZ or TW-CBF or TW-FTZ, visit Organization > Details > Config > Registration.");

			public static string ExpCusCodenotCBFnorEPZnorFTZ => Res.GetString("5A34AED9-A336-4776-8794-C5644CE7B483", "Related Bonded Parties should be of Organization Registration Code EPZ or CBF or FTZ. To create a valid TW-EPZ or TW-CBF or TW-FTZ, visit Organization > Details > Config > Registration.");
		}

		public static class InvoiceHeader
		{
			public static string HavingMultipleINCOTermsOnSingleJob => Res.GetString("B45DD96D-483A-4834-99CE-1E1869F6A52D", "Having multiple INCO terms on a single job can result in incorrect calculations which can lead to message rejections.");

			public static string GetWarningDeclarationAndInvoicesIsUnbalanceForPackageNumber(ZString packType, ZDecimal totalNoOfPacks, ZDecimal totalNoOfPacksForInvoices) => Res.GetString("28B957F2-E74D-4E8B-AE05-178DB40BA4F6", "The sum of all invoice header package numbers {{{1} {0}}} does not balance with the declaration total package number {{{2} {0}}}.", packType, totalNoOfPacksForInvoices, totalNoOfPacks);

			public static string MultipleCurrencyError => Res.GetString("d337a68c-cf9b-4772-afef-646288310bb3", "Multiple currencies are not allowed on a customs declaration job.");
		}

		public static class OrgCusCode
		{
			public static string FEIMaximumAllowedLengthExceeded => Res.GetString("92314776-412d-47a2-9721-7a962bfb5a8c", "The length of FEI shouldn't be more than 15.");

			public static string ATPMaximumAllowedLengthExceeded => Res.GetString("E03D4A50-A796-40AB-B2CE-DF4B87A81300", "The length of ATP (Agricultural Technology Park Bonded ID) shouldn't be more than 5.");

			public static string SPKMaximumAllowedLengthExceeded => Res.GetString("54ACD873-5F34-4FDE-A84A-B9E414C27B9C", "The length of SPK (Science Park Bonded ID) shouldn't be more than 5.");

			public static string CCCMaximumAllowedLengthExceeded => Res.GetString("F9C8137F-E638-42C2-A441-38CC3CE21984", "The length of CCC (Customs Carrier Code) shouldn't be more than 14.");
		}

		public static class CusBrokerageBoxNumber
		{
			public static string BoxNumberAgain => Res.GetString("433BC3A2-787C-436D-8FC7-EB89DFA37162", "The same box number already exists for the customs office area.");

			public static string CustomsOfficeAreaIsDefaultAgain => Res.GetString("4767989A-BE79-47DF-9448-9DA21C886D6F", "Each customs office area can only have one default box number.");

			public static string UnselectedDefaultBoxNumber => Res.GetString("AF6DF3A1-434E-4A09-9F79-9EDBF050FB83", "Each customs office area requires at least one default box number.");

			public static string InvalidBoxNumber => Res.GetString("1e34e439-6ee8-4bc3-b611-05ab617cb418", "A box number must consist of exactly three alphanumeric characters.");
		}

		public static class CusCodeData
		{
			public static string CodeIsLettersAndNumbersOnly => Res.GetString("D55412D0-C3BB-48F9-8374-6E38447C5F78", "Reserved field code can only be a number digit or an English capital letter.");

			public static string ReservedFieldDuplicated => Res.GetString("C9BAD7B3-1265-4B79-B08E-42BAF2AEB63F", "The entered code already exists. Please enter a different value.");
		}

		public static class ProductLabelRange
		{
			public static string NumberMustBeEightCharacters(string caption) => Res.GetString("7d77df90-5968-493e-ad68-c2a561e1f646", "{0} must be 8 characters long.", caption);
		}

		public static class CusGoodsLocation
		{
			public static string CannotBeDuplicated => Res.GetString("804D3A9A-F32D-46BE-9ECB-0AC4216A79FA", "(Customs Office + Message Type) cannot be duplicated.");

			public static string InvalidGoodsLocation => Res.GetString("FFA24FA6-602F-4E9F-9F2C-5C006E67D86A", "The selected Customs Office is not a CUSTOMSOFFICE attribute value of the selected Goods Location.");
		}

		public static class NCATKMessageSendingObject
		{
			public static string MissingNCATKRegistryConfiguration => Res.GetString("36dec69f-9f65-4878-b74d-1643b497dcf2", "A valid configuration is not found. Please visit Maintain > System > System Registry > Customs > Taiwan > NCATK Message Sending Configuration to set up the configuration.");
		}

		public static class TWCustomsNumberViewStmNumsWrapper
		{
			public static string InvalidNumber => Res.GetString("5FAF723E-C553-49C1-93E1-EEFF60E42633", "Please enter a valid number.");

			public static string HasUsedRange => Res.GetString("6945B6D8-0B5D-454E-BB8A-3335AA90AB69", "The new range is smaller than the Current Value. If the specified range has been used before, there might be a chance that duplicated Entry Number will be generated.");

			public static string EndNumberCannotBeSmallerThanStartNumber => Res.GetString("E2072953-62D7-41D3-848A-551CB22CC54A", "End Number cannot be smaller than Start Number");

			public static string CurrentValueCannotBeSmallerThanStartNumber => Res.GetString("5F1270B5-9AD9-4A44-AC33-C6A6945353B6", "Current Value cannot be smaller than Start Number");
		}

		public static class AllocateNumber
		{
			public static string AlreadyExists => Res.GetString("B131B8DC-4939-4896-8773-61AF4DDFE922", "This Entry Number is already allocated to an existing job.");
			public static string Part2NumberDoesNotMatch => Res.GetString("A672A8B3-1743-475C-AE18-843380ADD574", "The 3rd-4th character of entry number does not match the captured entry number key component values. Ignore this validation when confirmed.");
			public static string Part4NumberDoesNotMatch => Res.GetString("4E5219E5-905A-455D-B06B-EA44D7B025E0", "The entered Box Number does not match the captured entry number key component values. Ignore this validation when confirmed.");
			public static string KeyComponentValuesChanged => Res.GetString("DD13567A-ADA8-426B-AF55-E92441A5F667", "The allocated entry number does not match the captured entry number key component values. Please re-capture the entry number key component values, or allocate a new entry number using the currently captured entry number key component values.");
		}

		public static class TWJobDocAddressValidationMessages
		{
			public static int ChineseNameMaxlength => 70;

			public static int ChineseAddressMaxlength => 100;

			public static int ForeignNameMaxlength => 80;

			public static int ForeignAddressMaxlength => 120;

			public static string MaxCharactersLength(ZInt length) => Res.GetString("8640fe39-8fa1-4893-b0ea-1632feaa5627", "Only the first {0} characters will be sent to the customs.", length);

			public static string MaxChineseCompanyNameCharactersLength(ZInt length) => Res.GetString("A3E1B72B-9871-4BD0-AF2B-D3D07A54B99B", "Chinese Company Name Only the first {0} characters will be sent to the customs.", length);

			public static string MaxChineseAddressCharactersLength(ZInt length) => Res.GetString("AE31C0BC-0584-48C7-BC64-91ED5CC4A25E", "Chinese Address Only the first {0} characters will be sent to the customs.", length);

			public static string MaxForeignCompanyNameCharactersLength(ZInt length) => Res.GetString("3791DF80-0B71-45A2-ADE1-50828FDCD1E0", "Foreign Company Name Only the first {0} characters will be sent to the customs.", length);

			public static string MaxForeignAddressCharactersLength(ZInt length) => Res.GetString("DB890D90-6F8A-4EB5-AC3E-99838BA7F58F", "Foreign Address Only the first {0} characters will be sent to the customs.", length);

			public static string CountryOfForeignManufacturerMustNotBeTW => Res.GetString("fe01a7e4-d60c-4574-9414-44d431188e54", "The country of Foreign Manufacturer must not be Taiwan.");

			public static string CountryOfManufacturerMustBeTW => Res.GetString("13cff6db-1897-4f5e-90fe-a8a6b7f4c7e8", "The country of Manufacturer must be Taiwan.");
		}

		public static class TWJobDocAddress
		{
			public static string ImporterBondedIdCanNotCoExistOrEmptyWithToBondedWarehouse => Res.GetString("320FB75F-CFDC-44FC-8112-FE87E14DAB71", "Please enter To Bonded Warehouse or Importer Bonded ID but not both.");
			public static string SupplierBondedIdCanNotCoExistOrEmptyWithBondedFactories => Res.GetString("2A72F832-2B74-4B20-A1B8-8C99A89F5BD9", "Please enter Related Bonded Party or Supplier Bonded ID but not both.");
			public static string ImporterBondedIdCanNotCoExistWithBondedFactories => Res.GetString("FD543E30-9793-4E70-AC39-1F809E041023", "Importer Bonded ID and Previous Bonded Party cannot exist at the same time.");
		}

		public static class MessageProcessor
		{
			public static string TranshipmentEntryHeaderNotFound(string messageNumber, string entryNumber, string entryType) => Res.GetString("2C68A362-5CA1-4813-B632-529A8AC9AB35", "Can not find the corresponding Transhipment Entry Header for Message Number: {0}, Entry Number: {1}, Entry Type: {2}", messageNumber, entryNumber, entryType);

			public static string EventTypeOrEntryTypeIsNotSupported(string messageNumber, string entryNumber, string entryType) => Res.GetString("8C06169C-AC3D-4FA8-A139-A187FE9BC4AE", "Event Type or Entry Type is not supported for Message Number: {0}, Entry Number: {1}, Entry Type: {2}", messageNumber, entryNumber, entryType);

			public static string ControllingMessageHeaderNotFound(string messageNumber, string entryNumber, string functionalReferenceID) => Res.GetString("1614FA90-CE85-4E91-AD9D-A2FA4C98B67E", "Can not find the corresponding Controlling Message Header for Message Number: {0}, Entry Number: {1}, Functional Reference ID: {2}", messageNumber, entryNumber, functionalReferenceID);

			public static string ControllingMessageHeaderWithMessageTyepNotFound(string messageNumber, string functionalReferenceID, string messageType) => Res.GetString("D5D9AFAF-8348-4201-A957-AA4E062A7487", "Can not find the corresponding Controlling Message Header for Message Number: {0}, Functional Reference ID: {1}, Message Type: {2}", messageNumber, functionalReferenceID, messageType);
		}

		public static class CusEntryHeader
		{
			public static string NetWeightNotBeGreaterThanGrossWeight => Res.GetString("7afa6645-0e7a-4939-9bfd-0f6803524f88", "The Net Weight should not be greater than the Gross Weight.");
		}
	}
}
