
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Business
{
	public static class ValidationConstants
	{
		public static class BondedWarehouse
		{
			public const string WarehouseTransactionExistsNeedsCancel = "There is an Inventory transaction created against a movement on this job.\r\nPlease cancel it before changing this value.";
			public const string AtLeastOneCommodityWithProductIsRequired = "For Bonded Warehousing, at least one commodity must have a valid product specified.";
			public const string AProductIsRequiredForBondedWarehousingCommodity = "A Bonded Warehousing commodity must have a valid product; not all Bonded Warehousing commodities have a valid product specified.";
			public const string ABondedWarehousingCommodityRequiresAnInvoiceQuantity = "A Bonded Warehousing commodity must have an invoice quantity; not all Bonded Warehousing commodities have an invoice quantity specified.";
			public const string ABondedWarehousingCommodityRequiresEntryDetails = "A Bonded Warehousing commodity must have an Master Bill Number and a Warehouse Entry Line Number; not all Bonded Warehousing commodities have an Master Bill Number and a Warehouse Entry Line Number specified.";
		}

		public static class Header
		{
			public static MultilingualString AtLeastOneBillExists
			{
				get { return ResString.GetMultilingualString("CusInBondHeader|F81CF52-79A4-480e-B4ED-8C33FB4F7586", "Please enter at least one Bill."); }
			}

			public static MultilingualString FIRMSForFTZorBondedWarehouseWithdrawals
			{
				get { return ResString.GetMultilingualString("CusInBondHeader|A219E821-773A-40e6-9238-6742DEB8A37C", "Please enter a FIRMS code; a FIRMS code is required for FTZ or Bonded Warehouse withdrawals."); }
			}

			public static MultilingualString FIRMSCodeInvalidForInBond
			{
				get { return ResString.GetMultilingualString("CusInBondHeader|C3F6D503-FD1F-497C-B8C6-68E3AB01AC5E", "The FIRMS code entered is not valid for In-bond; a FIRMS code must be active and have a custodial bond, FTZ bond or multi use bond, (facility type 02, 04 or 10)."); }
			}

			public static MultilingualString CarrierCodeNotValidForTransportMode
			{
				get { return ResString.GetMultilingualString("CusInBondHeader|7109D379-FD1B-4212-8BA5-A509CE5CC4D5", "The Carrier code you have selected is not valid for the selected Transport Mode."); }
			}

			public static MultilingualString InvalidAirCarrierCodeLength
			{
				get { return ResString.GetMultilingualString("CusInBondHeader|854670AA-55DD-42DB-9440-3E36FDA81AC4", "Carrier code must not be longer than 3."); }
			}

			public static MultilingualString NonAMSInvalidForTransportModeAir
			{
				get { return ResString.GetMultilingualString("CusInBondHeader|14192691-069D-4E83-9B63-1FB398E65941", "Non-AMS in-bond is not valid for Transport Mode Air (40)"); }
			}

			public static MultilingualString NonAMSCarrierForFTZ
			{
				get { return ResString.GetMultilingualString("CusInBondHeader|0CF8ADE3-837E-4606-894F-AAFD82AA1D63", "Mode should be Non AMS when moving from FTZ or Bonded Warehouse."); }
			}

			public static MultilingualString InvalidHeaderTypeForTransportModeWithPreviousITNumber
			{
				get { return ResString.GetMultilingualString("63CD7C73-64E4-4C98-BEBC-0EFD771D058E", "For in-bonds with Mode of Transport Truck, if a previous in-bond number is being referenced, the in-bond must be sent as an AMS in-bond. The previous in-bond is considered registered in AMS"); }
			}

			public static MultilingualString InvalidHeaderTypeForTransportModeWithoutPreviousITNumber
			{
				get { return ResString.GetMultilingualString("17CD9F59-DDF5-4250-BC3A-5653AC0E659E", "In-bonds with Mode of Transport Truck, must be entered as non-AMS, unless a Previous In-Bond Number was filed and is entered"); }
			}

			public static MultilingualString InvalidFlightNumber
			{
				get { return ResString.GetMultilingualString("CusInBondHeader|AFA3A842-97F3-464A-858F-218A5FDB4D54", "Please enter a valid flight number; valid flight number must be NNN, NNNA, NNNN or NNNNA where N is a numeric and A is alphabetic."); }
			}

			public static MultilingualString VoyageTripNumberLengthExceeded
			{
				get { return ResString.GetMultilingualString("1F8B728A-B301-483B-86B6-5E734C80D892", "This value is too long. Only 5 characters are allowed."); }
			}

			public static MultilingualString CodeInvalidMessage
			{
				get { return ResString.GetMultilingualString("9F2F1648-16C8-48B3-A155-39ED57E6E72E", "A 2-character IATA Airline Designator ending with a number 0-9 is not accepted by Air AMS for flight identification. Use the ICAO three letter designator."); }
			}

			public static MultilingualString CodeIgnoredMessage
			{
				get { return ResString.GetMultilingualString("F3535D9A-1E41-4976-9169-427409990F50", "If both the 2 character and 3 character carrier codes are entered, the system will send the 3 character code in the ABI message."); }
			}

			public static MultilingualString ConveyanceNameLengthExceed
			{
				get { return ResString.GetMultilingualString("CusInBondHeader|01AFAE8A-DF4A-4EFD-8079-9338E0AC5312", "CBP only permits us to send up to 23 characters of this data element. We will therefore only send the first 23 characters you have entered."); }
			}

			public static MultilingualString ContinueMessage
			{
				get { return ResString.GetMultilingualString("USInBond|MessagingForm|592BB311-2384-4a75-AA3C-F9810936DFE8", "Continue?"); }
			}
		}

		public static class Bill
		{
			public static MultilingualString ManifestQuantityMustBeGreaterThanZero
			{
				get { return ResString.GetMultilingualString("CusInBondBill|D22196A2-4A6E-4171-9557-BDA8BDB75DC2", "Please enter a value greater than zero; the Manifest Quantity is the total number of pieces on the bill of lading."); }
			}

			public static MultilingualString BillNumberIsDuplicated
			{
				get { return ResString.GetMultilingualString("CusInBondBill|28C3E976-DA7A-4661-8A38-99A792F0FE3D", "Please enter a different Bill Number; there is already a Bill with this Bill Number."); }
			}

			public static MultilingualString OriginalEntryAdmissionNumberIsDuplicated
			{
				get { return ResString.GetMultilingualString("CusInBondBill|5D3DD1A9-84B6-4A72-9E1E-D60A373E7171", "Please enter a different Master Bill Number; there is already a Bill with this Bill Number."); }
			}

			public static MultilingualString BillUniqueCodeIsDuplicated
			{
				get { return ResString.GetMultilingualString("CusInBondBill|4D4E718F-3AEC-4569-BF9A-56D8A4AC45AB", "Please enter a different combination of Master Bill and House Bill; there is already a combination of these bill numbers."); }
			}

			public static MultilingualString IssuerCodeShouldBeTheSameAsFirms
			{
				get { return ResString.GetMultilingualString("CusInBondHeader|EA6BA509-568E-4AF3-B198-246D0254FCE0", "If a FIRMS code is selected, it must match the FIRMS code on the Main Details Tab."); }
			}

			public static MultilingualString BillNumberLengthExceeded
			{
				get { return ResString.GetMultilingualString("CusInBondHeader|23C681A4-38DE-43AC-AC25-B6A3E757E96C", "The value is too long. Only 12 characters should be sent in the message."); }
			}

			public static MultilingualString BillNumberContainsSpecialCharacters
			{
				get { return ResString.GetMultilingualString("CusInBondBill|9714B069-68B9-4FF5-8D0D-6F548F8E463D", "Do not include spaces, hyphens, slashes or special characters."); }
			}

			public static MultilingualString OrignalEntryAdmissionNumberContainsSpecialCharacters
			{
				get { return ResString.GetMultilingualString("CusInBondBill|29459BF9-87F8-4E7D-90A4-BB83A5B7006F", "Do not include spaces, slashes or special characters."); }
			}

			public static MultilingualString FTZForeignPortOfLading
			{
				get { return ResString.GetMultilingualString("7CD0EDA2-851F-4238-8297-A11FCBA196DD", "Foreign Port Of Lading is mandatory. Please use '99999' for the Foreign Port Of Lading when moving from FTZ or Bonded Warehouse."); }
			}
		}

		public static class JobDocAddress
		{
			public static MultilingualString GetOrganisationPKRequired(string addressType)
			{
				return ResString.GetMultilingualString("B1223D39-9108-4D9A-B705-2AFD2480E974", "A {0} must be specified.", addressType);
			}

			public static MultilingualString CompanyNameRequired
			{
				get { return ResString.GetMultilingualString("CF21309F-C596-485B-924F-8C43CA0AC503", "A Company Name is required."); }
			}

			public static MultilingualString AddressRequired
			{
				get { return ResString.GetMultilingualString("5BF03433-EAF5-49D9-B8A3-223A7C3906FD", "An Address is required."); }
			}
		}

		public static class Container
		{
			public static MultilingualString MustOnlyContainAlphaNumerics
			{
				get { return ResString.GetMultilingualString("CusInBondContainer|BEE3BB06-64DD-40a6-877B-34737CD994F7", "Invalid Characters In Container Number - Container number must only contain alphanumeric characters."); }
			}

			public static MultilingualString AtLeastOneCommodityIsRequired
			{
				get { return ResString.GetMultilingualString("CusInBondContainer|2FC18E66-1463-4303-88E4-A78F4AE8F962", "A Container should have at least one Commodity Detail."); }
			}

			public static MultilingualString MaximumNumberOfUNDGExceeded
			{
				get { return ResString.GetMultilingualString("CusInBondContainer|9B82286C-0723-4820-84F7-31DE353DA1FF", "A Container should only have a maximum of 99 Hazardous records."); }
			}

			public static MultilingualString ContainerNumberIsRequired
			{
				get { return ResString.GetMultilingualString("CusInBondContainer|F73D5A8A-7BDC-4105-B2C8-E82035B2C7C5", "Please enter a valid container number associated with the bill of lading exactly as it physically appears on the container.\r\nEnter 'NC' for non-containerized freight."); }
			}

			public static MultilingualString ContainerNumberIsDuplicated
			{
				get { return ResString.GetMultilingualString("CusInBondContainer|DA91EDF5-0BED-4b9d-93BC-8447DE25F553", "Please enter a different Container Number; there is already a Container entered with this number."); }
			}

			public static MultilingualString MaxLengthOfSealExceeded
			{
				get { return ResString.GetMultilingualString("CusInBondContainer|6A4E8B34-ECC6-44CD-BEDC-6ED581B1B622", "Seal Number is too long. Only 15 characters allowed. Messages will be rejected by Customs."); }
			}
		}

		public static class Commodity
		{
			public static MultilingualString MonetaryValueIsRequired
			{
				get { return ResString.GetMultilingualString("CusInBondCargoDesc|6D1EDE00-5C39-4366-8291-526E6065B9DA", "Please enter a value of commodity greater than zero in whole dollars; $20 per kilo may be used if the value is unknown."); }
			}

			public static MultilingualString WeightMustBeGreaterThanZero
			{
				get { return ResString.GetMultilingualString("CusInBondCargoDesc|7D845FCA-7FD6-4961-AD88-A9EA1FB92CE1", "Please enter the net weight of the commodity, in pounds or kilos, greater than zero."); }
			}

			public static MultilingualString PieceCountIsNotRequired
			{
				get { return ResString.GetMultilingualString("CusInBondCargoDesc|9E8C29FE-F77C-4E31-9393-EB26E7D0EF20", "The Container already has a piece count entered. This count will not be sent in the message."); }
			}

			public static MultilingualString PieceCountMustBeGreaterThanZero
			{
				get { return ResString.GetMultilingualString("CusInBondCargoDesc|C1F1CBAD-93BE-4e6c-A23B-737CE98DF844", "Please enter the total number of pieces in the container, greater than zero."); }
			}

			public static MultilingualString GetTariffNotFound(string tariff)
			{
				return ResString.GetMultilingualString("CusInBondCargoDesc|296F9577-F74C-4a75-B9CB-ED3C757D1AED", "Tariff '{0}' is not recognized as a valid tariff.\r\nPlease check the tariff or, if necessary, send a query to customs for the latest tariff information (Customs Declarations->Actions->Reference File Request->Tariff).\r\nIf the last four digits are not known then zero fill them.", tariff);
			}

			public static MultilingualString TariffWasFoundButNotOnFile(string effectiveDate)
			{
				return ResString.GetMultilingualString("CusInBondCargoDesc|19F12CB5-9641-404A-8813-9E513AD9CF7A", "Tariff was found but is not on file for '{0}'.", effectiveDate);
			}

			public static MultilingualString TariffNumShouldBeAtLeast6Digits
			{
				get { return ResString.GetMultilingualString("CusInBondCargoDesc|B4386DD0-86F9-4999-B562-6559E5D7FBA9", "Please enter a valid Tariff; a Tariff should be at least 6 digits."); }
			}

			public const string WarningPartCannotBeFoundBeforeEnteringASupplierOrAnImporter = "You must enter either a supplier or an importer before the Classification details can be copied in from a Part.";
			public const string WarningPartCodeFoundButNotRelatedToSupplierImporterCombination = "A Product with this code exists, but it is inactive or the Supplier/Importer relationship on that Product does not match this commodity. Either add a new Product (F3), make it active (F4, add a filter to show inactive records, search and edit), or change the Supplier/Importer relationship on the existing Product (F4 then edit a Product).";
			public const string WarningPartCodesFoundButNotRelatedToSupplierImporterCombination = "Several Products with this code exist, but none of then have a Supplier/Importer relationship which matches this commodity and are active. Either add a new Product (F3), activate a product (F4, add a filter to show inactive records, search and edit), or change the Supplier/Importer relationship on an existing Product (F4 then edit a Product).";
			public const string WarningPartCodeNotFoundAtAll = "No Product(s) found with this code. Either add a new Product (F3) or select a valid Product (F4).";
			public const string WarningMoreThanOneProductMatchFound = @"The system has found more than one potential match for the Product Code entered with the same score, and has randomly picked one of the matched products.
Please make sure that the right Product has been picked (F4).";
			public const string ABondedWarehousingCommodityRequiresAnInvoiceQuantity = "A Bonded Warehousing commodity must have an invoice quantity specified.";
			public const string ABondedWarehousingCommodityRequiresEntryDetails = "A Bonded Warehousing commodity must have a Warehouse entry number and entry line number specified.";
		}

		public static class MoveHeader
		{
			public static MultilingualString MaxValueExceeded
			{
				get { return ResString.GetMultilingualString("CusInBondHeader|A2451C10-EBF5-473E-AC76-21A740B5A1BC", "CBP will only accept a value of up to 99,999,999"); }
			}

			public static MultilingualString SplitCarrierWarnForDifferentValue(string checkFileName, string differentValueFiledName)
			{
				return ResString.GetMultilingualString("{2A17BF17-FDF8-45BE-B492-0434EC6E1AC6}", "The {0} will not sent in the message, because the {1} has different value.", checkFileName, differentValueFiledName);
			}

			public static MultilingualString SplitCarrierCodeForAIR
			{
				get { return ResString.GetMultilingualString("72BA1600-0F2E-424A-AC78-B9D2458349F2", "Please enter a split carrier code, if the inbond job is not split please enter a carrier code for importing carrier details."); }
			}

			public static MultilingualString ShouldEnterSplitCarrierCode
			{
				get { return ResString.GetMultilingualString("0E11D7D8-FB97-49EC-912B-7A0D2C5D2D6D", "Please enter a split carrier code."); }
			}

			public static MultilingualString SplitFlightForAIR
			{
				get { return ResString.GetMultilingualString("9A8506A6-DA64-459A-A89D-1288C7691CE1", "Please enter a split flight number, if the inbond job is not split please enter a flight number for importing carrier details."); }
			}

			public static MultilingualString SholudEnterSplitFlightNo
			{
				get { return ResString.GetMultilingualString("AD4B0444-0758-4721-AB40-85716769244D", "Please enter a split flight number."); }
			}

			public static MultilingualString NoWarehouseForThisAddress(string importerCode)
			{
				return ResString.GetMultilingualString("3DC7195E-278C-41B8-ACA3-418B3685F811", "Inventory recording/Bonded Warehouse Integration is active for Importer '{0}'. This address is not valid as there is no setup Warehouse for this address.", importerCode);
			}

			public static MultilingualString WarehouseShouldBeInsideHeaderCountry(string importerCode, string country)
			{
				return ResString.GetMultilingualString("9235EDB2-127A-4F87-B8E2-BE863B1F54CB", "Inventory recording/Bonded Warehouse Integration is active for Importer '{0}'. Please enter a Warehouse that is located within {1}.", importerCode, country);
			}

			public static MultilingualString MonetaryValueIsRequiredFor62Or63EntryType
			{
				get { return ResString.GetMultilingualString("CusInBondMoveHeader|611BA79C-1C47-4b2c-845F-735D6D53AC46", "Please enter a value of the In-Bond Movement in whole dollars; a value is required for Entry Type '62' or '63'; $20 per kilo may be used if the value is unknown."); }
			}

			public static MultilingualString ForeignDestinationIsOnlyRequiredFor62Or63EntryType
			{
				get { return ResString.GetMultilingualString("CusInBondMoveHeader|6CFE0A84-9154-440f-BD5C-A263E458A75D", "The Foreign Destination is only required for Entry Type '62' or '63'."); }
			}

			public static MultilingualString AtLeastOneMovementDetailsIsRequired
			{
				get { return ResString.GetMultilingualString("CusInBondMoveHeader|2FADC1D7-8ADF-46c8-9B37-37120F753A2B", "A Movement Header should have at least one Movement Detail."); }
			}

			public static MultilingualString MoveHeaderMustHaveCustomsClearance(string inBondNumber)
			{
				return ResString.GetMultilingualString("CusInBondMoveHeader|531584BB-F94E-406F-A328-D21F6D5C0095", "Submitting this message for a movement that has not been accepted by Customs will be rejected; please ensure that In-Bond movement ‘{0}’ has already been accepted by Customs before sending this message.", inBondNumber);
			}

			public static MultilingualString InBondCarrierIDValid
			{
				get { return ResString.GetMultilingualString("CusInBondMoveHeader|A3C81665-ED93-4d6f-91AB-96BCAC3578DA", "An In-Bond Carrier ID must be a valid IRS Number (NN-NNNNNNNXX) or CBP Assigned Number (YYDDPP-NNNNN) or Social Security Number (NNN-NN-NNNN) where N is a number and X is alphanumeric, YY is the last two digits of the calendar year, DDPP is the district/port code where the number is assigned."); }
			}

			public static MultilingualString USDestinationCanNotMatchPortOfArrivalFor62EntryType
			{
				get { return ResString.GetMultilingualString("CusInBondMoveHeader|E4D9DEEC-B4AC-4FF7-973F-CAD670399BFC", "The US Port Of Destination cannot be the same as the Port Of Arrival when the Entry Type is '62'."); }
			}

			public static MultilingualString USDestinationShouldMatchPortOfArrivalFor63EntryType
			{
				get { return ResString.GetMultilingualString("CusInBondMoveHeader|2E41AC00-B8BA-4ad7-ADFA-C699D6410F7D", "The US Port Of Destination must be the same as the Port Of Arrival when the Entry Type is '63'."); }
			}

			public static MultilingualString USDestinationShouldNotMatchPortOfArrivalFor61EntryType
			{
				get { return ResString.GetMultilingualString("CusInBondMoveHeader|A7110747-C2F6-4765-97E6-D10C23FF4C72", "The US Port Of Destination must not be the same as the Port Of Arrival when the Entry Type is '61'."); }
			}

			public static MultilingualString TOLStateCodeIsRequired
			{
				get { return ResString.GetMultilingualString("CusInBondMoveHeader|FDD75584-D6C1-4f5e-81AF-C30ED8923305", "A State Code is required when the City Name of the place where the Transfer Of Liability is specified."); }
			}

			public static MultilingualString GetNoPOADocumentForString(string countrySpecificNameForPoa, string carrierID)
			{
				return ResString.GetMultilingualString("CusInBondMoveHeader|59483D16-E1C0-411e-A511-C06D58336255", "There is no {0:G} against this job applicable for Carrier ID '{1}'.\r\nPlease ensure there is a valid {0:G} setup under the document tracking in the eDocs tab.", countrySpecificNameForPoa, carrierID);
			}

			public static MultilingualString GetMultipleCarrierIDsPOAWarning(string countrySpecificNameForPoa)
			{
				return ResString.GetMultilingualString("CusInBondMoveHeader|5F2CB032-712B-4b39-8065-EF1693CFE8BA", "Multiple Carrier ID’s have been entered: Ensure you have a {0:G} that covers each Carrier.\r\nWhere multiple Carriers are required, enter a {0:G} for each ID by setting the country to ‘US’ and document owner to the organization that the Carrier ID belongs to.", countrySpecificNameForPoa);
			}

			public static MultilingualString ArrivalDateExceedsTodaysDate
			{
				get { return ResString.GetMultilingualString("CusInBondMoveHeader|6E8B196B-588C-4159-90BD-4B07D5A04436", "The Arrival Date cannot exceeds today's date."); }
			}

			public static MultilingualString InBondNumberAllocationIsInProgress(string lockInfo)
			{
				return ResString.GetMultilingualString("CusInBondMoveHeader|FB8ACE35-B4CF-4D17-947A-866545554545", "{0} is in the process of allocating a new InBond Number for this movement; this movement cannot be sent now.", lockInfo);
			}

			public static MultilingualString PortOfPresentation
			{
				get { return ResString.GetMultilingualString("CusInBondMoveHeader|FB8ACE35-B4CF-4D17-947A-61224DB5D457", "The district/port code shown in the HTSUS Schedule D must be used for the port where the in-bond entry is being presented. This is required if printing the 7512 Document"); }
			}

			public static MultilingualString MexicanPedimentoNumberRequired
			{
				get { return ResString.GetMultilingualString("CusInBondMoveHeader|0FBF8C25-2563-4FDE-84F2-A127B02BA5D8", "Mexican Pedimento Number is required for shipments to Mexico."); }
			}

			public static MultilingualString BondedWarehouseIsRequired
			{
				get { return ResString.GetMultilingualString("CusInBondMoveHeader|713CCE01-79F9-46D8-B0B5-DF501315146E", "Bonded Warehouse/FTZ Address is required when the importer is enabled for bonded warehousing and Move from WHS/FTZ is ticked"); }
			}

			public static MultilingualString MoveToFTZIndicatorIsRequired
			{
				get { return ResString.GetMultilingualString("{9415D31D-A5BB-4C37-805D-11814AAE739B}", "Move To FTZ indicator is required when moving from FTZ."); }
			}
			public static MultilingualString MoveToFTZIndicatorIsNotApplicable
			{
				get { return ResString.GetMultilingualString("{41CBB25D-6CC1-4CCC-9759-5E510DB1A8CD}", "Move To FTZ indicator is not applicable and should not be entered."); }
			}
			public static MultilingualString InBondNumberIsRequired
			{
				get { return ResString.GetMultilingualString("{AF6A33C8-AB16-4BA1-A13B-5B9A54FB2C87}", "In-Bond Number is required. Please cancel this, go back to the form, click the Allocate button and enter an In-Bond number its departure was declared under."); }
			}
		}

		public static class MoveDetail
		{
			public static MultilingualString NoAirTransportForWHSFTZ
			{
				get { return ResString.GetMultilingualString("CusInBondMoveDetail|894461F4-C942-4A59-82F0-19586C33C662", "Transport Mode 40 cannot be used for moves from a bonded warehouse or FTZ. Select transport mode 10 or 11 for best results. Note: The in-bond must report bill of lading and transportation information representing the movement from the FTZ not the original import bills or any export bills."); }
			}

			public static MultilingualString InBondQtyGreaterThanBillManifestQty
			{
				get { return ResString.GetMultilingualString("CusInBondMoveDetail|DDBFEA18-E509-40af-836C-1138D936DA0D", "The In-Bond quantity cannot be greater than the Bill's manifest quantity."); }
			}

			public static MultilingualString TotalManifestQtyNotEqualSumOfPieceCount
			{
				get { return ResString.GetMultilingualString("CusInBondMoveDetail|747B6EBD-3380-4005-968C-EF0DA02BDDE8", "The Master Bill's manifest quantity should equal the sum of the piece counts."); }
			}

			public static MultilingualString CannotAddMovementDetailsWhilePendingCustoms
			{
				get { return ResString.GetMultilingualString("CusInBondMoveDetail|E2AE3E3D-4CD4-405d-910D-AD116FBE4A55", "Cannot add a new Movement Detail to this Movement while it is pending Customs response."); }
			}

			public static MultilingualString CannotAddMovementDetailsWhenInBondHasBeenSubmitted
			{
				get { return ResString.GetMultilingualString("CusInBondMoveDetail|C7AFEB3D-1D38-4d96-8842-21A857202AD7", "Cannot add a new Movement Detail to this Movement as it has been submitted to Customs.\r\nPlease send an In-Bond Delete message before adding a new Movement Detail."); }
			}

			public static MultilingualString AtLeastOneContainerIsRequired
			{
				get { return ResString.GetMultilingualString("CusInBondMoveDetail|C74C6AA3-8C9A-41b1-A89E-8E7BFA1EC189", "A Movement Detail should have at least one Container Detail."); }
			}

			public static MultilingualString SecondaryNotifyPartyIsNotSubmitter
			{
				get { return ResString.GetMultilingualString("CusInBondMoveDetail|936A18CB-FE52-4327-B1C1-BA85F3BB6C97", "In order to receive status notifications, please nominate yourself as an SNP by placing your ABI Routing Code as one of the Secondary Notify Parties."); }
			}

			public static MultilingualString FIRMSNotMatchJobFIRMSForFTZorBondedWarehouseWithdrawals
			{
				get { return ResString.GetMultilingualString("CusInBondMoveDetail|637F82A8-7E8C-4b3f-890D-877254AE03E3", "The code entered does not match the Job's FIRMS; for a FTZ or Bonded Warehouse withdrawals, the FIRMS code of the FTZ or Bonded Warehouse may be used in lieu of the SCAC as long as it's the same as the Job's FIRMS."); }
			}

			public static MultilingualString SecondaryNotifyPartyNotValidFIRMSForFTZorBondedWarehouseWithdrawals
			{
				get { return ResString.GetMultilingualString("CusInBondMoveDetail|1B6CD55E-EA43-4917-B7BD-E6DD7AB143FE", "Please entered a valid Secondary Notify Party (SNP); a valid SNP can either be a Standard Alpha Carrier Code (SCAC) or an ABI Filer (NNNNXXXNN where XXX is the Census Schedule D Code representing the CBP port of the SNP; XXX is the filer code; and NN is the office code); for a FTZ or Bonded Warehouse withdrawals, the FIRMS code of the FTZ or Bonded Warehouse may be used in lieu of the SCAC."); }
			}

			public static MultilingualString SecondaryNotifyPartyNotValid
			{
				get { return ResString.GetMultilingualString("CusInBondMoveDetail|FC985C8F-DF56-4a50-8EE1-E5A171860DD7", "Please entered a valid Secondary Notify Party (SNP); a valid SNP can either be a Standard Alpha Carrier Code (SCAC) or an ABI Filer (NNNNXXXNN where XXX is the Census Schedule D Code representing the CBP port of the SNP; XXX is the filer code; and NN is the office code)."); }
			}

			public static MultilingualString GetSecondaryNotifyPartyIsSpecifiedOutOfOrderMessage(string first, string second)
			{
				return ResString.GetMultilingualString("CusInBondMoveDetail|7EA89363-DF46-4a06-AB6C-60FA5DDE7481", "{0} Secondary Notify Party should not be entered unless there is a {1} Secondary Notify Party.", first, second);
			}

			public static MultilingualString MovementDetailForBillIsDuplicated
			{
				get { return ResString.GetMultilingualString("CusInBondMoveDetail|83001209-80C5-4938-9325-C484213010A7", "Please select a different Bill; this Movement already has the detail for this Bill."); }
			}
		}

		public static class Synchronize
		{
			public static MultilingualString SynchronizedFromParent(string parentTableCode)
			{
				return ResString.GetMultilingualString("AD02554D-0067-4C55-87A8-A9E1870C9CBF", "as it's copied from {0}. If you wish to delete this record, please tick 'Override Default Values' first.", parentTableCode);
			}
		}
	}
}
