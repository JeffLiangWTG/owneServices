using System.Collections.Immutable;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public static class ValidationConstants
	{
		#region Shared

		public static class Shared
		{
			#region MRN Format

			public static string InvalidMRNCustomsOffice
			{
				get { return Res.GetString("49A82DA1-FDA6-4615-9782-00A857E762B8", "Characters 1-3 should contain a valid customs office."); }
			}

			public static string InvalidMRNDate
			{
				get { return Res.GetString("B8492C60-C115-4124-B936-0CFCC1EFAB5E", "Characters 4-11 should contain a valid date not greater than today in the format CCYYMMDD."); }
			}

			public static string InvalidMRNNumbersOnly
			{
				get { return Res.GetString("96C06EA3-D039-4C88-90C2-BF4351EF7BDA", "Characters 12-18 should contain numbers only."); }
			}

			#endregion

			#region LRN Format

			public static string LRNFormatInvalidAgentCode => Res.GetString("FD1EB1F2-9D37-48EF-B4F9-EF57EC9D7C31", "Characters 1-8 should be a valid South Africa Agent Code");

			public static string LRNFormatInvalidOfficeCode => Res.GetString("5E880418-FD88-4B92-9375-B645D4B81BD4", "Characters 9-11 should be a valid Customs Office Code");

			public static string LRNFormatFutureDate => Res.GetString("414BDEA8-328E-435D-BE84-22F34FEC85B9", "Characters 12-19 should not be a future date");

			public static string LRNFormatInvalidDate => Res.GetString("3C46D725-287B-4303-9A99-A8A0CAEACAE9", "Characters 12-19 should be a valid date in format 'CCYYMMDD' no greater than today");

			public static string LRNFormatInvalidSequenceNumber => Res.GetString("39166EF0-AFD8-4E5F-9262-E1F865A2B8A9", "Characters 19-25 should be a valid sequence number");

			public static string LRNFormatInvalidSize => Res.GetString("668E77CD-6ED1-4E3B-B199-29667A12AB3F", "LRN Number should be 25 Characters long");

			#endregion

			public static string EntryLineNumberExceedMax => ResString.GetMultilingualString("E38277B9-2286-4773-85DE-37E04D049625", "Entry line number cannot exceed 9999.");

			public static string MessageCannotBeSent => ResString.GetMultilingualString("E67E9AD7-33AC-4115-9CBD-21E8A674722D", "The message cannot be sent.");
		}

		#endregion

		#region Declaration
		public static class Declaration
		{
			public static string ExchangeRateDateIsRequired => Res.GetString("4CFA149B-6FB2-49C5-B02C-5E99A6BBA64D", "Exchange Rate Date is required.");

			public static string LocationOfGoodsDoesNotBelongToCustomsOffice(string locationOfGoods, string customsOffice)
			{
				return Res.GetString("{3008BF0A-73F1-40A7-B17E-BECCC2BB2B36}", "Location Of Goods ('{0}') does not belong to Customs Office ('{1}').", locationOfGoods, customsOffice);
			}

			public static string LocationOfGoodsNotForContainerisedCargo
			{
				get { return Res.GetString("50f78245-4d7a-4814-9c44-6e4c8875e388", "Selected Location of Goods (Depot) not for containerized cargo."); }
			}

			public static string LocationOfGoodsNotForTransportMode
			{
				get { return Res.GetString("d9792976-54f0-42d2-8257-99f784df8d1c", "Selected Location of Goods (Depot) not for selected Transport Mode."); }
			}

			public static string UnregisteredTraderDoesNotHaveIDOrGTXCode(string unregisteredTraderType)
			{
				return Res.GetString("B13B033C-C1C5-4BC5-9ED8-FBA5737CC99E", "The currently selected {0} has a Customs {0} code of {1}. An ID Number or Government Tax File code must be setup when using Customs {0} code {1}", unregisteredTraderType, UnregisteredTraderCustomsCode);
			}
			public static string UnregisteredTraderDoesNotHaveIDPassportOrGTXCode(string unregisteredTraderType)
			{
				return Res.GetString("0BFFC820-90B1-47A7-BC24-9A7D314F2FF2", "The currently selected {0} has a Customs {0} code of {1}. An ID Number, Passport or Government Tax File code must be setup when using Customs {0} code {1}", unregisteredTraderType, UnregisteredTraderCustomsCode);
			}

			public const string UnregisteredTraderCustomsCode = "70707070";

			public static string MasterBillIssuedAtCannotBeEmpty(ZString masterBillLabel)
			{
				return Res.GetString("dc214bc4-c5b6-4f58-a256-8af6946df8fd", "Can not be empty if {0} contains a value.", masterBillLabel);
			}

			public static string MasterBillIsseudAtCountryDifferent
			{
				get { return Res.GetString("a7b7c724-6fba-4215-befc-faba5ba33699", "The country/region this bill is issued at is different from the origin country/region."); }
			}

			public static string MasterBillIssuedAtCannotBeEmptyBLNS
			{
				get { return Res.GetString("09042c72-fed0-45e3-a798-a7626ef6e4d4", "Can not be empty if final destination is BLNS."); }
			}

			public static string MasterBillIssuedAtMustBeEmpty
			{
				get { return Res.GetString("7a54ccc0-559f-4050-97f1-889e6f74ffc7", "This field must be empty if final destination is not BLNS."); }
			}

			public static string MasterBillIssuedDateCannotBeEmpty(ZString masterBillLabel)
			{
				return Res.GetString("2eb8c1ab-9da1-4aff-ac7c-d35e968c4257", "A Date must be provided if {0} contains a value.", masterBillLabel);
			}

			public static string MasterBillIssuedDateMustBeEmpty
			{
				get { return Res.GetString("8a9d7bbc-2f6f-4151-b642-1ae9101b152c", "Must be empty if Removal Transport Code is not Road."); }
			}

			public static string MasterBillIssuedDateMasterBillNotCaptured(string label)
			{
				return Res.GetString("a6bdf273-bd8c-46ca-ade5-640fc7928ff9", "Must be empty if no {0} has been captured.", label);
			}

			public static string CustomsHasCommenced(string declarationReference)
			{
				return Res.GetString("68588B2D-0FB0-403B-8627-AC7F30BEC8EE", "Data Import Failed because Messaging with Customs has commenced on declaration {0}.", declarationReference);
			}
		}
		#endregion

		#region Entry Instruction
		public static class EntryInstruction
		{
			public static ResourceString NoValidCreditTerm
			{
				get { return ResString.GetMultilingualString("2A8D7D08-1F66-476D-94FE-60E01371A7C1", "Credit terms should be either ADV, NEP or any number between 1 and 999."); }
			}

			public static ResourceString EmptyCreditTerms
			{
				get { return ResString.GetMultilingualString("1C20AA1A-4BFF-43A4-8431-0DD068941E8E", "Credit Terms are required."); }
			}

			public static ResourceString CreditTermsRequiredForExport
			{
				get { return ResString.GetMultilingualString("657AA8CB-4069-4D5C-A154-48DCD69C0D08", "Credit Terms Required for Export Jobs."); }
			}

			public static ResourceString CreditTermInvalid
			{
				get { return ResString.GetMultilingualString("CCCD8330-9C53-4787-AE7E-B15F006DBEC7", "Credit Term invalid. Please choose a value from the list."); }
			}

			public static ResourceString CreditTermsOnlyRequiredForExport
			{
				get { return ResString.GetMultilingualString("DC21930C-9E41-4041-B79B-85E17174401A", "Credit Terms only required for Export jobs."); }
			}

			public static string InvalidNEPCreditTermSelection
			{
				get { return ResString.GetMultilingualString("43870A5E-8FA9-4C95-8457-53715BD55D63", "Credit terms should not be NEP if either the bank code is entered or the transaction value is not 0. Please use either ADV or 1-999 if this is the case."); }
			}

			public static string InvalidNumberCreditTermSelection
			{
				get { return ResString.GetMultilingualString("4E1B2CD3-1F01-44D7-AD1A-F35A8539B143", "Credit terms should not be a number if the transaction value is not greater than 0. Please increase the transaction value or change the credit terms."); }
			}

			public static string InvalidUCRNumber
			{
				get { return ResString.GetMultilingualString("95E71717-227B-44C0-92D3-576AE5C3E8F4", "The UCR Order number should be 14-19 alphanumeric characters"); }
			}

			public static string InvalidUCRNumberOverride
			{
				get { return ResString.GetMultilingualString("44F459D1-C82D-4F6D-8461-516A5530C166", "The UCR number should be 17-35 alphanumeric characters"); }
			}

			public static string UCRNumberAlreadyUsedOnJob
			{
				get { return ResString.GetMultilingualString("B86D4A3C-28F8-4D34-AB58-1511EE1F60D8", "The UCR number:{0} already used on job:{1}"); }
			}

			public static string InvalidUCRYear
			{
				get { return ResString.GetMultilingualString("3A41B5B7-C53A-4677-9B22-50F66BDDE8B3", "The first character should be the last digit of the year."); }
			}

			public static string InvalidUCRCountry
			{
				get { return ResString.GetMultilingualString("21E403C1-CA34-4A2E-9318-31F26EE07A8B", "The declaration goods origin has not been captured."); }
			}

			public static string InvalidUCRCountryOverride
			{
				get { return ResString.GetMultilingualString("14087A6C-1A7C-4A7E-8E4B-D3DB87FDFCC7", "Characters 2-3 should represent the declaration goods origin"); }
			}

			public static string InvalidUCRSupplierCode(ZString supplierCode)
			{
				return ResString.GetMultilingualString("53000B4E-2DDA-457A-B7AF-B6F3B47CB364", "Characters 4-12 should be a zero padded supplier code ({0})", supplierCode);
			}

			public static string UCRNotFilledInForImportsOrExports
			{
				get { return ResString.GetMultilingualString("FF5A9496-87DB-45C1-B8B4-23A21898BC2F", "UCR Order number cannot be empty in imports or exports."); }
			}

			public static string InvalidUCRScope
			{
				get { return ResString.GetMultilingualString("8E61EE00-3E7C-4A59-AA31-8DC7D1EBDE3D", "The last character should be the scope ('S' for Single Use, 'M' for Multiple Use)"); }
			}

			public static string UnableToCalculateUCR
			{
				get { return ResString.GetMultilingualString("D6A615A5-080B-4EFE-A70C-AA2A62994F7E", "Please override and enter a UCR Number, a calculated value is not possible once messaging has occurred for this Entry Instruction."); }
			}

			public static string UCRInvoiceRefNumTruncated
			{
				get { return ResString.GetMultilingualString("CE5B0584-3261-4B51-BBC3-FEEEF2369370", "The invoice number found was over 19 alphanumeric characters and was truncated to fit."); }
			}

			public static string UCRNoInvoicesFound
			{
				get { return ResString.GetMultilingualString("08828832-F66A-4656-8212-FD74B0BEA3D1", "No invoices found."); }
			}

			public static string UCRValueWillNotBeUsed
			{
				get { return ResString.GetMultilingualString("F8CEBE63-9A88-4E08-B491-F8137FF3B2BC", "This value will not be used in the calculation of the UCR, as the Ref. Type extracts a value from elsewhere."); }
			}

			public static string RemoverRequiredForCPC
			{
				get { return Res.GetString("E71B2B32-76FD-4012-A539-6C4D64D67A79", "A Licensed Remover required, and SubContractor is optional when the Removal Mode is Road"); }
			}

			public static ZString SameToFromWarehouseRequired
			{
				get { return Res.GetString("99E93FC3-D622-43F0-A562-A83327AB3B27", "The To and From Warehouse is required to be the same for this type of Entry Instruction"); }
			}

			public static string PreviousProcedureMRNRequiredForPPC
			{
				get { return Res.GetString("173DDBFC-C16A-4E9E-8905-A9183AC5B1C8", "Please specify a Previous Procedure MRN for this PPC Code"); }
			}

			public static string FromWarehouseRequiredForPPC
			{
				get { return Res.GetString("2246A2B8-CDFD-46FC-82BF-675FE53E653D", "Please specify a From Warehouse on the Entry Instruction"); }
			}

			public static string FromWarehouseRequiredForCPC
			{
				get { return Res.GetString("ab0381c0-c30d-4f6a-88c5-4809cae3655f", "From Warehouse is required for the selected Procedure Code."); }
			}

			internal static string PleaseSelecteValidOwner(MessageDataProviderKeyFactor factor)
			{
				return Res.GetString("F42914AA-754C-45A3-864E-BC27B78FECCE", "Please select a valid organization with a valid Customs Client Code for Customs Procedure Combination:{0}-{1}.", factor.CPC, factor.PPC);
			}

			public static string AssessmentDateManuallyEntered
			{
				get { return Res.GetString("7FF8CCF8-142C-46BD-8BB0-F4FE9FAA3FDB", "Assessment date should only be filled in manually for Replacement VOC’s or for Change VOC’s where the previous entry was not framed in CW1"); }
			}

			public static string AssessmentDateNotSameToOriginalAssessmentDate
			{
				get { return Res.GetString("ED5C77CA-2AD1-4F32-97D5-265F4B9A789F", "Assessment date should be the same to the original assessment date."); }
			}

			public static string PortOfDestinationOrExitRequired
			{
				get { return Res.GetString("7BFB01BA-36CC-4400-BA9C-D8BFE32C31BC", "Customs Office of Destination / Exit is required for the specified Customs Procedure."); }
			}

			public static string PortOfDestinationOrExitNotRequired
			{
				get { return Res.GetString("78966189-2ABE-4015-9335-0998C7E6B66B", "Customs Office of Destination / Exit not required for the specified Customs Procedure."); }
			}

			public static string PortOfExitMustBeSameAsCustomsOffice
			{
				get { return Res.GetString("bf3c0aa0-03f8-4d15-a759-1b01dc6c2008", "Customs Office of Destination/Exit must match the Customs Office."); }
			}

			public static string OnlyInstructionsFromTheSameGroup
			{
				get { return Res.GetString("4C0BCE80-2C04-41F6-AD80-B84FB51DEF77", "Only entry instructions belonging to the same group may appear on a declaration. Group: "); }
			}

			public static string ImportBlnsToZaRequired
			{
				get { return Res.GetString("435DB46A-E1A9-4C86-8287-AA40AC6C4BA1", "For import entries the country/region of export needs to be from a BLNS country and the destination country/region needs to be ZA to make use of the selected CPC."); }
			}

			public static string ExportZaToBlnsRequired
			{
				get { return Res.GetString("0B9E3E99-6CB1-41CD-985C-3148BF2FC6BB", "For export entries the country/region of export needs to be from ZA and the destination country/region needs to be to a BLNS country/region to make use of the selected CPC."); }
			}

			public static string RemovalTransportCodeRequired
			{
				get { return Res.GetString("b513dce8-b326-4972-b405-990ba5ac164a", "Removal Transport Code is required for the specified Customs Procedure."); }
			}

			public static string RebateUserCodeRequiredOnImporter
			{
				get { return Res.GetString("83FEBA1E-384A-4C33-B69B-96B03DD1F5B9", "Rebate User Code is required to be specified on the Importer for the specific Customs Procedure."); }
			}

			public static string LineOneIsRequiredForHeaderLevelProvisionalPayment
			{
				get { return Res.GetString("ECB64EE1-04BA-461E-84F2-AFF56EA20D08", "Entry Line with Line Number '1' is required to output the Header Level Provisional Payment."); }
			}

			public static string MRNToBeReplacedEntered
			{
				get { return Res.GetString("c2ca746d-429b-4d0c-b516-33d02b8bf0dc", "MRN to be Replaced should be empty when Assessment Date is not entered."); }
			}

			public static string MRNToBeReplacedNotInThisJob
			{
				get { return Res.GetString("68e5ed8d-875b-46b5-b249-24a4aadde46c", "The MRN to be Replaced you selected is not in this job."); }
			}

			public static string ProvisionalPaymentCaseAlreadyClosed
			{
				get { return Res.GetString("23536B74-3355-49FE-A9FD-41B4F13A62E5", "The case of this Provisional Payment Type has already been closed. It may be used in Fee calculation or message sending."); }
			}

			public static string ExchangeRateDateWillBeCalculated
			{
				get { return Res.GetString("FE3F3004-9502-49F5-96A9-F1E7F86ECF10", "Exchange rate date will be calculated by system."); }
			}

			public static string ExchangeRateDateGreaterThanAssessmentDate
			{
				get { return Res.GetString("E9A2C7DF-8238-409E-998E-4FA2E6A11736", "Exchange rate date should not be greater than assessment date."); }
			}

			public static string ExchangeRateDateTooFarFromAssessmentDate
			{
				get { return Res.GetString("CC257878-913D-488E-927F-FD1DA639C3CA", "The difference between Exchange rate date and assessment date should not be more than 2 days."); }
			}

			public static string NewOwnerRequiredForCPC
			{
				get { return Res.GetString("c72d48a8-44fe-4e78-8766-a13e45803436", "New Owner is required for the selected Procedure Code."); }
			}

			public static string PermitQtyValIndicatorInvalid(ZString permitNumber, ZString qtyValIndicator)
			{
				return Res.GetString("697E17B2-09CE-4AA1-ADC2-E73D41012500", "Permit '{0}' does not have permit Qty/Val Indicator of '{1}'.", permitNumber, qtyValIndicator);
			}

			public static string PermitValidityPeriodNotContainingAssessmentdate(ZString permitNumber, ZDateTime assessmentdate)
			{
				return Res.GetString("1E6B8FCA-A12C-4CB6-A156-C575C95AF846", "Permit '{0}' does not have validity period matching assessment date '{1}'.", permitNumber, assessmentdate.Date);
			}

			public static string PermitMustHavePositiveValueBalance(ZString permitNumber)
			{
				return Res.GetString("850CE47A-DA42-4759-9C86-1C89C3E0BB77", "Permit '{0}' should have a positive value balance.", permitNumber);
			}

			public static string NewOwnerShouldBeDifferentToOld
			{
				get { return Res.GetString("faa16834-34c3-43e9-a401-1074e87cd145", "New Owner should be different to the old owner (Importer)."); }
			}

			public static string AllNewOwnerShouldBeSame
			{
				get { return Res.GetString("ec809991-01ed-4059-8238-bd021a79fe2f", "All New Owners on all entry instructions should be the same."); }
			}

			public static string NewOwnerRequireCustomsImporterCode
			{
				get { return Res.GetString("db2a526b-ed20-465b-b5ee-45b4281125a9", "New Owner does not have a customs code setup in Organization>Config"); }
			}

			public static string InvoicesWithDifferentVDNNumberOnEntryInstruction
			{
				get { return Res.GetString("653A4594-CE5A-45C5-8A9B-C1368B6E9516", "Invoices with different VDN Numbers cannot be linked to the same entry instruction."); }
			}

			public static string RebateUserOverrideHasNoRebateCode
			{
				get { return Res.GetString("6A7E38CC-A500-4636-B14C-5927E030BEC7", "The Rebate User Override chosen does not contain a rebate code."); }
			}

			public static string RemoverRequiredIfAutomaticSplitSet
			{
				get { return Res.GetString("099D3C8E-A083-4478-BE0E-DC43A82186B5", "A remover is required if registry item 'Allow automatic split of entries by bond amount' is Yes"); }
			}

			public static string BondGuaranteeValueRequiredIfAutomaticSplitSet
			{
				get { return Res.GetString("3DD1CF18-7699-4CFE-B1C6-CF61AE21E78C", "A bond guarantee value is required if registry item 'Allow automatic split of entries by bond amount' is Yes"); }
			}

			public static string AdditionalBondIsRequiredWhenTotalBNDExceedsBGV
			{
				get { return Res.GetString("0F93ECBC-4807-42F1-8D60-D2C8FF333C0F", "Additional Bond is required when the Total BND exceeds the BGV of the EDI Transport Principal."); }
			}
		}

		#endregion

		#region Invoice Header
		public static class InvoiceHeader
		{
			public static string InvoiceNumberRequiredForImportShipments
			{
				get { return "Invoice Number required for Import Shipments"; }
			}

			public static string InvoiceDateRequiredForImportAndExportShipments
			{
				get { return "Invoice Date required for Invoice"; }
			}

			public static string InvoiceDateCannotBeInTheFuture
			{
				get { return ResString.GetMultilingualString("97945AD9-5408-41A6-A706-29CB2FF199FA", "Invoice Date cannot be a future date."); }
			}

			public static string RelationshipIndicatorRequiredForImports
			{
				get { return ResString.GetMultilingualString("5F1E0030-0F67-49C4-858A-F8D1429AE3C4", "Relationship Indicator is required for imports."); }
			}

			public static string VDNRequiredWhenSupplierHasCSCCode
			{
				get { return Res.GetString("58681116-C0CB-40EB-8FEB-B08D9F216690", "The selected Supplier has a Customs Supplier Code, a VDN number will also be required by Customs."); }
			}

			public static string MustBeZARCurrencyForImportByExternalBroker
			{
				get { return ResString.GetMultilingualString("D8EB743A-84D3-4105-AF4F-2D0E9F8EF44C", "Currency must be 'ZAR' for Import By External Broker jobs."); }
			}

			public static string SingleForeignCurrencyInvoiceToEntryForExports
			{
				get { return Res.GetString("DABE6363-C3FE-40F6-AC17-0274B18182BF", "For Exports job, one invoice cannot be used on multiple entries if currency is not ZAR."); }
			}

			public static string SingleInvoiceToEntryForExportsHasMultipleExchangeRateDate
			{
				get { return Res.GetString("6F82C74F-8109-45F2-AAD0-2BBEAC0DC590", "For Exports job, one invoice cannot be used on multiple entries with different exchange rate date."); }
			}

			public static string PaymentTermsIsRequired
			{
				get { return Res.GetString("73B7C925-BD45-42AC-998E-9C2211697DF5", "Payment Terms is Required."); }
			}

			public static ResourceString PaymentTermsInvalid
			{
				get { return ResString.GetMultilingualString("B542F774-1398-4364-822F-AC51BCF220A6", "Payment Terms invalid. Please choose a value from the list."); }
			}
		}

		#endregion

		#region Invoice Line
		public static class InvoiceLine
		{
			public static string ProductMustBelongToOrganisationForChangeOfOwnership(string organisationCode)
			{
				return Res.GetString("{E04565E9-FD67-4E1A-9171-FC1CF6518C8B}", "This Product must have an Owner relationship for '{0}' as this is required for Bonded Warehousing Change of Ownership.", organisationCode);
			}

			public static string WarningPartCodeFoundButNotRelatedToOwner
			{
				get { return Res.GetString("{828E5570-AF1C-40E4-84DF-2DCDF36407A4}", "A Product with this code exists, but it is inactive or the Owner relationship on that Product does not match this invoice. Either add a new Product (F3), make it active (F4, add a filter to show inactive records, search and edit), or change the Owner relationship on the existing Product (F4 then edit a Product)."); }
			}

			public static string WarningPartCodesFoundButNotRelatedToOwner
			{
				get { return Res.GetString("{8AA5EADB-2D1C-46AC-A83A-5607851210AB}", "Several Products with this code exist, but none of then have an Owner relationship which matches this invoice and are active. Either add a new Product (F3), activate a product (F4, add a filter to show inactive records, search and edit), or change the Owner relationship on an existing Product (F4 then edit a Product)."); }
			}

			public static string NewOwnerProductRequiresForChangeOfOwnership
			{
				get { return ResString.GetMultilingualString("{85026FCF-4A1E-488D-9AE8-643AB8C43027}", "Bonded Warehousing Change of Ownership requires new Owner's product details to be specified."); }
			}

			public static string RemoverRequiredForProcedureCode
			{
				get { return ResString.GetMultilingualString("53C51A8E-87C7-46E1-8CCC-DC085DE3BC2B", "Remover required for CPC/PPC combination."); }
			}

			public static string Schedule1Part1TariffDoesNotExistsForDate(ZString tariff, ZDateTime dateTime)
			{
				return ResString.GetMultilingualString("603D0BD7-0272-4920-9B93-E01F283F4FB5", "No Schedule 1 Part 1 Tariff exists for '{0}' as at {1} {2}.", tariff, dateTime.ToShortDateString(), dateTime.ToShortTimeString());
			}

			public static string Schedule1Part1TariffDoesNotExists(ZString tariff)
			{
				return ResString.GetMultilingualString("9CDD0B02-3AC0-4EAA-B9A4-6FA00581E9A5", "No Schedule 1 Part 1 Tariff exists for '{0}'.", tariff);
			}

			public static string NoValidRate(ZString tariff)
			{
				return ResString.GetMultilingualString("7F6A4CBB-E192-43EE-9E8A-EC880FCF148F", "No valid duty rate is available for this tariff '{0}'. Please contact support.", tariff);
			}

			public static string ToWarehouseOnEntryInstructionRequiredWhenDestinationIsBLNS
			{
				get { return ResString.GetMultilingualString("5F1E0030-0F77-49C4-858A-F8D1429AE3C4", "To Warehouse on the Entry Instruction is required when country/region of destination is a BLNS country/region."); }
			}

			public static string NoROOCertEnteredForTradeAgreement(ZString rooType)
			{
				return Res.GetString("e6fd31c5-89e0-4c7a-9269-d9d73b2cd67f", "Trade Agreement {0}, Rules of Origin Certificate may need to be provided", rooType);
			}

			public static string NoROOCertEnteredForType
			{
				get { return ResString.GetMultilingualString("FFFF07D5-DD84-428B-91D2-53CD2B39E4CD", "Rules of Origin Certificate cannot be empty if ROO Type has been selected."); }
			}

			public static string NoROOTypeEnteredForCert
			{
				get { return ResString.GetMultilingualString("DB7EC264-7393-40E4-988F-8D4806E08860", "ROO Type cannot be empty if Rules of Origin Certificate has been entered."); }
			}

			public static string PreviousMRNSForSameProcedureMustBeSame(ZString customsProcedureCode)
			{
				return Res.GetString("4d71427d-f8f2-4fed-a402-9e637a5c53a5", "All previous MRNs for Customs Procedure Code {0} must be the same.", customsProcedureCode);
			}

			public static string PreviousMRNRequiredForPPC
			{
				get { return Res.GetString("173DDBFC-C16A-4E9E-8905-A9183AC5B1C8", "Please specify a Previous Procedure MRN for this PPC Code"); }
			}

			public static string PreviousMRNLineNumberRequiredForPPC
			{
				get { return Res.GetString("6dc0a384-5a00-4b7c-870c-c6ca1d38d200", "Previous Line Number required for the selected PPC or CPC code"); }
			}

			public static string PreviousMRNLineNumberRequiredForIMX
			{
				get { return Res.GetString("82791FB0-BB08-44E2-B4CB-1638FCD0D0A0", "WHS MRN Line number required for the Import By External Broker job"); }
			}

			public static string DuplicateMRNLineNumberExistPerEntryInstruction
			{
				get { return Res.GetString("2BA33DC8-4C6F-4378-B026-54E0FF89E9DF", "WHS MRN Line number must be unique per Entry Instruction"); }
			}

			public static string FromWarehouseRequiredForPPC
			{
				get { return Res.GetString("2246A2B8-CDFD-46FC-82BF-675FE53E653D", "Please specify a From Warehouse on the Entry Instruction"); }
			}

			public static string MixOfPreviousProceduresOnEntryInstruction
			{
				get { return Res.GetString("8b93f2bc-6221-4fb4-82af-ae284582542b", "There are a mix of previous procedure codes used on the entry instruction that may not be sent on a single entry."); }
			}

			public static string UnsupportedMergeByForNonZeroPreviousProcedureCode
			{
				get { return Res.GetString("8BC8C688-638F-4AAB-B1D4-C882E747C8FA", "The selected Merge By method should not be used when PPC <> 00."); }
			}
			public static string MergingNotRecommendedWherePreviousProcedureCodeIsNonZero
			{
				get { return Res.GetString("C3FB6400-DE4E-4C6C-AD02-457A070736B7", "Merging is not recommended when any InvLines are linked to Customs Procedure Combination (CPC) where the Previous Procedure Code (PPC) is not 00."); }
			}

			public static MultilingualString PreviousProcedureInvalid
			{
				get { return ResString.GetMultilingualString("832A7968-8616-4EC9-882F-9EAABFCC6C21", "Previous Procedure code invalid for shipment type and Procedure code combination."); }
			}

			#region Permit Validation

			public static string PermitNotFound(ZString permitNumber)
			{
				return Res.GetString("5A7D7100-6529-4019-B333-BBC2F485BFBE", "Permit '{0}' not found.", permitNumber);
			}

			public static string PermitHolderInvalid(ZString permitNumber, ZString permitHolderCode)
			{
				return Res.GetString("870500D8-B442-4717-9576-3766480AA2F5", "Permit '{0}' does not exist for permit holder '{1}'.", permitNumber, permitHolderCode);
			}

			public static string PermitTypeInvalid(ZString permitNumber, ZString permitType)
			{
				return Res.GetString("3F28A91A-08E7-4E97-8152-A22E18726406", "Permit '{0}' does not have permit type '{1}'.", permitNumber, permitType);
			}

			public static string PermitTypeInvalid(ZString permitNumber, ZString[] permitTypes)
			{
				return Res.GetString("9A5A3CB7-5718-4267-B06F-744DDD2FE411", "Permit '{0}' does not have permit type '{1}'.", permitNumber, string.Join("', '", permitTypes));
			}

			public static string TariffNotInPermitRange(ZString permitNumber, ZString tariff)
			{
				return Res.GetString("88E3C610-91E7-487C-8065-B8D299138518", "Permit '{0}' does not have a valid rule for tariff '{1}'.", permitNumber, tariff);
			}

			public static string UnitOfMeasureDoesNotMatchPermit(ZString permitNumber, ZString expectedUOM)
			{
				return Res.GetString("3240C605-A138-4F36-AFAA-5675C6F7BF4B", "Permit '{0}' requires a matching unit of measure '{1}' on this invoice line.", permitNumber, expectedUOM);
			}

			public static string PermitIsOptional
			{
				get { return Res.GetString("24882339-3225-4D36-AB76-2D76BDABE7BF", "A permit may be required."); }
			}

			public static string PermitForSecondHandGoods
			{
				get { return Res.GetString("0C92F377-B323-4FFA-917A-7F0324F293CB", "Permit Number may be required for Second Hand or Used Goods."); }
			}

			#endregion

			public static string CountryOfOriginMustBeZA
			{
				get { return Res.GetString("ceddbfb2-eb4a-4ca3-a8cf-18af8bb92fc4", "Goods Origin must be South Africa (ZA) for the selected Procedure Code."); }
			}

			public static string PreviousMRNLineNumberMinMaxValue
			{
				get { return Res.GetString("1B5980BD-E080-4A32-9B21-11F864904381", "Previous MRN line number cannot exceed 9999 and should be greater than 0"); }
			}

			public static readonly ImmutableList<string> MixablePreviousProcedureCodes = new string[]
			{
				UniversalReferenceConstants.ProcedureCodes._40,
				UniversalReferenceConstants.ProcedureCodes._41,
				UniversalReferenceConstants.ProcedureCodes._44
			}.ToImmutableList();

			public static readonly ImmutableList<string> ProcedureCodesWhereCountryOfOriginMustBeZA = new string[]
			{
				UniversalReferenceConstants.ProcedureCodes._51,
				UniversalReferenceConstants.ProcedureCodes._52,
				UniversalReferenceConstants.ProcedureCodes._64,
				UniversalReferenceConstants.ProcedureCodes._68,
				UniversalReferenceConstants.ProcedureCodes._90
			}.ToImmutableList();

			public static string PreviousProcedureCodeIsNotAnIntoWarehouse
			{
				get { return Res.GetString("{82E234E5-E805-49A9-B074-073BFC7D8FD5}", "This PPC is not an Into Warehouse PPC; please select an Into Warehouse PPC if an Entry Instruction has an Into Warehouse PPC."); }
			}

			public static string PreviousProcedureCodeIsNotAnOutOfWarehouse
			{
				get { return Res.GetString("{FCFF6C39-DCC3-4DFF-88DD-D7B25F619506}", "This PPC is not an Out Of Warehouse PPC; please select an Out Of Warehouse PPC if an Entry Instruction has an Out Of Warehouse PPC."); }
			}

			public static string ThisEntryIsUsingAnExciseCPCButNoExciseDataDeclared
			{
				get => Res.GetString("908912E6-FEEF-46D5-BF7F-EFD8D5EF3D46", "This entry is using an Excise CPC but no Excise data declared.");
			}
		}
		#endregion

		#region Additional Information
		public static class AdditionalInformation
		{
			public static string AdditionalInfoCodeShouldNotHaveData(ZString code)
			{
				return Res.GetString("25A6FD4E-7E82-45E7-B6B8-03E0C6D84A3E", "The Additional Information Code {0} should not have a data value.", code);
			}

			public static string DataShouldBeValidNumberic(int decimalPlaces)
			{
				return Res.GetString("E2E8C107-4542-4650-A405-F8CDC5F708C1", "The data should be a numeric value with {} decimal places.", decimalPlaces);
			}

			public static string AdditionalInfoCodeRequireData(ZString code)
			{
				return Res.GetString("6F4E3040-55D0-406F-B457-375F45D7BEF9", "The Additional Information Code {0} should have a data value.", code);
			}

			public static string MissingAdditionalInformationCode(ZString desc, ZString code)
			{
				return Res.GetString("B36F59EE-F741-45F2-8EFA-942E4511AD83", "Please capture {0} using the {1} additional information code in the same grouping.", desc, code);
			}

			public static string AdditionalInformationCodeCannotBeUsedTogether(ZString code1, ZString code2)
			{
				return Res.GetString("925DF239-ABA0-4FF9-9F4D-4746E2297668", "Both {0} and {1} cannot be used together, please select one only.", code1, code2);
			}

			public static string AllLinesNeedBNDWhenBHRIsEntered
			{
				get { return Res.GetString("2BDF2ED5-8F0B-4346-BD09-27260AEDDDA8", "If a Bond Holder is supplied, all lines require a BND Amount."); }
			}

			public static string AdditionalInfoCodeRequireNoSpaceData(ZString code)
			{
				return Res.GetString("D855962D-F678-474C-8EA1-C8048B206CB1", "The Additional Information Code {0} should not have a data value including space.", code);
			}

			public static string RemovalTransportModeRequired => Res.GetString("09A8A707-8577-492D-9B88-7F77C44A3D08", "The Final Destination is a BLNS Country/Region, a Removal Transport Mode may be required.");
		}
		#endregion

		#region Entry Header
		public static class EntryHeader
		{
			public static string FinancialAccountNumberNotSetup(ZString customsOffice)
			{
				return Res.GetString("d359dca6-90c2-4a0b-b7f8-0a2257f1a6ea", "A Financial Account Number has not been set up for this Customs Office: {0}", customsOffice);
			}
		}
		#endregion

		#region Cus Line Tariff Detail
		public static class CusLineTariffDetail
		{
			public static string ScheduleTariffDoesNotExists(ZString schedule, ZString tariff)
			{
				return ResString.GetMultilingualString("585FEB4C-EFB7-4ADA-AC66-5BDED5984C19", "Tariff '{0}' is not a valid '{1}' Tariff Code.", tariff, schedule);
			}

			public static string DuplicateAdditionalDutySchedule(ZString schedule)
			{
				return ResString.GetMultilingualString("AF4C0F21-4327-4031-BA2D-77CE45074423", "There is already another record for {0}.", schedule);
			}

			public static string ScheduleIsNotValidForProcedure(ZString schedule, ZString procedureCode, ZString previousProcedureCode)
			{
				return ResString.GetMultilingualString("{F58783CB-9E25-40A5-8A0A-455C44B1294A}", "This {0} is not valid for CPC/PPC ({1}/{2}) combination.", schedule, procedureCode, previousProcedureCode);
			}

			public static string SimilarAdditionalDutyType(ZString typePrefix)
			{
				return ResString.GetMultilingualString("85C8E4FB-EEAC-4583-AC01-927C97ED6432", "There is already another record with similar Schedule prefix '{0}'.", typePrefix);
			}

			public static string TariffNotValidForSchedule(ZString schedule, ZString tariff)
			{
				return ResString.GetMultilingualString("{1426B0EE-D67B-4B02-ADBD-E35F7CDEB6CB}", "This tariff '{0}' is not valid for {1} based on the current Schedule '1P1' settings.", tariff, schedule);
			}

			public static string MoreThanTwoRCCCertificates(ZString lineNumber)
			{
				return Res.GetString("53907139-49A2-4C27-A72C-906290FB8FC2", "Entry line {0} have more than 2 {1} allocated – this is not allowed.", lineNumber, RCCCertificates);
			}

			public static string InsufficientPermitValue
			{
				get { return Res.GetString("37E98BE3-C2A7-4626-B3E4-013EC3CAFA80", "The {0} that you have selected against the entry instruction have insufficient value to allocate all the lines, either add more certificates or move lines to a different clearing instruction.", RCCCertificates); }
			}

			public static string RCCCertificates => Res.GetString("7EB238D2-5200-48A3-9934-25C32A7D192B", "RCC certificates");

			public static string UOMsExceed => ResString.GetMultilingualString("186B2938-C080-43A5-B626-85ABE738621D", "The tariffs selected have more than 3 units of measure. Currently, only 3 are supported.");

			public static string DutyFreeTariffUsedForIntoWarehouseEntry => Res.GetString("c47444dc-d552-491a-a668-6dbc9fa98857", "A Duty Free Tariff is being used for an 'into Warehouse' Entry");
		}
		#endregion

		#region Document Sending Object
		public static class SupportingDocSendingObject
		{
			public static string CaseNumberOnlyAlphanumeric
			{
				get { return Res.GetString("25f25fe0-7d34-44e5-8a5d-3712eb8683c1", "The Case Number can only be alphanumeric."); }
			}
		}

		public static class SupportingDocSendingManager
		{
			public static string EmptyTradingPartyIDError => Res.GetString("fb818f15-bf4d-414f-9562-6f52eddf9b44", "Agent Code cannot be blank. Please enter a valid Agent Code against the Agent organization selected on the Declaration.");
		}
		#endregion

		#region CusEntryPayInfo
		public static class CusEntryPayInfo
		{
			public static string ReceiptNumberRequiredWithDate => Res.GetString("34D4B97E-DA5E-4C84-8633-BCF3B2F64461", "Receipt Number is required when Receipt Date is entered.");

			public static string ReceiptDateRequiredWithNumber => Res.GetString("A478DCD2-674E-4C39-97F4-0F78B65188B6", "Receipt Date is required when Receipt Number is entered.");

			public static string ReceiptDateCannotBeGreaterThantoday => Res.GetString("1F0CD61A-6610-45CD-83D0-27FF92079D8E", "Receipt Date cannot be greater than today.");
		}
		#endregion

		#region Containers

		public static class Containers
		{
			public static string ContainerNotISO
			{
				get { return ResString.GetMultilingualString("659EE6B2-3B0D-4F41-844E-BAC7836E3688", "Non-ISO Containers will have the letters 'NONU-' automatically appended to them to ensure compliance with SARS requirements."); }
			}
		}

		#endregion

		#region WarehouseOperatorTransactionsFilterStripBusinessObject

		public static class WarehouseOperatorTransactionsFilterStripBusinessObject
		{
			public static string WarehouseFilter => Res.GetString("E06B0443-7CE2-4D83-8D3F-C7E78F791A9B", "A valid warehouse is required.");
			public static string ProductOwnerFilter => Res.GetString("FAA8D267-1F69-4643-9BF5-9EFA7185B8F5]", "A valid product owner is required.");
		}

		#endregion
	}
}
