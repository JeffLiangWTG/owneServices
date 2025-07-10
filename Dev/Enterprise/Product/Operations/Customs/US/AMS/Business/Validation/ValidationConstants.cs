using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Business
{
	public static class ValidationConstants
	{
		public static class Header
		{
			public static MultilingualString ImportingConveyanceRequiresWhenLloydsNumberIsEmpty
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondHeader|F9A3EEAB-776D-49C6-BA94-4D429C754FD8", "Importing Conveyance is required when Lloyds Number is not specified."); }
			}

			public static MultilingualString VesselNameNotBeSentOnlyLloydsNumberBeSent
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondHeader|B9497566-A5B7-4B0C-BB31-D6C933A87BE3", "Please note: the Vessel Name will not be sent to CBP – only the Lloyds Number will be sent."); }
			}

			public static MultilingualString VesselNameOnlySendFirst23Characters
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondHeader|DB6E9920-FE51-4945-B65C-04ECEBD28F8E", "Please note: This is a long Vessel Name - CBP only permits us to send up to 23 characters for this data element.  We will therefore only send the first 23 characters of the Vessel Name you have entered."); }
			}

			public static MultilingualString OceanBillOfLadingIsRequiredForNVOCC
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondHeader|B8DC0281-E6DA-48B7-A843-E11E04FED703", "Ocean Bill of Lading is required for NVOCC bill."); }
			}

			public static MultilingualString AtLeastOneBillExists
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondHeader|F81CF52-79A4-480e-B4ED-8C33FB4F7586", "Please enter at least one Bill of Lading."); }
			}

			public static MultilingualString VoyageNumberLengthExceeded
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondHeader|BAE0C916-2FEA-4A6B-B2FE-5C9E7ED506FE", "Voyage Number is limited to 5 characters. Only the first 5 characters of the voyage number will be included in the message."); }
			}

			public static string SCACOfVesselOperatorShouldBeSubmitted
			{
				get { return ResString.GetMultilingualString("CusInBondBill|B667BAD9-D215-48FE-A179-FE5F13742704", "Value should be the SCAC code of the NVOCC filing the AMS message."); }
			}
		}

		public static class Bill
		{
			public static MultilingualString ChangingBOLNumberWhenBillIsOnFile(string oldBOL, string newBOL)
			{
				return ResString.GetMultilingualString("AMS|CusInBondBill|EE36DFB2-6F17-4A49-B3FE-E87B14F58040", "Cannot change Bill of Lading from '{0}' to '{1}' when '{0}' is on Customs file.\r\nYou need to delete it from Customs file before changing it.", oldBOL, newBOL);
			}

			public static MultilingualString ShouldBeIdenticalWithSCACCode
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondBill|0ACD287F-BFD4-4C7B-AB9A-9EAB357BAEB3", "Issuer Code does not match the NVO Carrier Code. This would normally match unless you are filing AMS for a third party. Please confirm the SCAC is correct."); }
			}

			public static MultilingualString ChangingBOLNumberWhenMessagingIsInProgress(string oldBOL, string newBOL)
			{
				return ResString.GetMultilingualString("AMS|CusInBondBill|4B814C79-BB17-4E2C-A4BE-E2E43D323248", "Cannot change Bill of Lading from '{0}' to '{1}' when '{0}' is being reported to Customs.\r\nYou need to wait for Customs response before changing it.", oldBOL, newBOL);
			}

			public static MultilingualString ChangingIssuerCodeWhenBillIsOnFile(string oldIssuerCode, string newIssuerCode)
			{
				return ResString.GetMultilingualString("AMS|CusInBondBill|592C8453-C32B-44DF-A647-A8E4E88705E6", "Cannot change Issuer Code from '{0}' to '{1}' when '{0}' is on Customs file.\r\nYou need to delete it from Customs file before changing it.", oldIssuerCode, newIssuerCode);
			}

			public static MultilingualString ChangingIssuerCodeWhenMessagingIsInProgress(string oldIssuerCode, string newIssuerCode)
			{
				return ResString.GetMultilingualString("AMS|CusInBondBill|99AA7EB9-A3A0-4DDA-B392-225F254A45A0", "Cannot change Issuer Code from '{0}' to '{1}' when '{0}' is being reported to Customs.\r\nYou need to wait for Customs response before changing it.", oldIssuerCode, newIssuerCode);
			}

			public static MultilingualString TotalManifestQtyNotEqualSumOfPieceCount
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondBill|747B6EBD-3380-4005-968C-EF0DA02BDDE8", "The Bill of Lading's manifest quantity should be equal to the sum of Commodities' piece count."); }
			}

			public static MultilingualString FIRMSDoesNotMatchDDPP
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondBill|63769EA9-AF01-410E-8B9E-11B1700EBFBD", "FIRMS location is not in the same district as Port Of Unlading."); }
			}

			public static MultilingualString MasterInBondIsRequiredWhenIndicatorIsTrue
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondBill|8ED91C0C-67D8-4B77-9896-7DF9E489A533", "The Bill Of Lading has been flagged as a Master In-Bond but there isn't any Master In-Bond Movement linked to it.\r\nYou need to add this Bill Of Lading to a Master In-Bond Movement under the In-Bond tab."); }
			}

			public static MultilingualString MasterInBondIsRequiredWhenInBondType62_63WithISF
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondBill|730A2DE3-0A38-43F2-82CE-D27592A69C79", "This Bill Status requires Master In-Bond data but there isn't any Master In-Bond Movement linked to it.\r\nYou need to add this Bill Of Lading to a Master In-Bond Movement under the In-Bond tab."); }
			}

			public static MultilingualString BillStatusNotForPTT
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondBill|7A17E366-53A7-4B0B-B466-C2D425BADBC7", "Bill Status is not eligible for Permit To Transfer. Must be Regular, Master, or In-bond."); }
			}

			public static MultilingualString BillOfLadingNumberIsTooLong(ZString billOfLadingNumber)
			{
				return ResString.GetMultilingualString("AMS|CusInBondBill|8FC61733-856B-4F73-A1D6-D84F3D73070B", "Bill of Lading '{0}' is invalid. A valid Bill of Lading cannot be greater than 12 alphanumeric characters.\r\nIf not corrected, only the last 12 characters of the Bill of Lading will be sent. \"{1}\".", billOfLadingNumber, billOfLadingNumber.Right(12));
			}

			public static MultilingualString MasterNumberIsInvalidWithSCACCode
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondBill|7A15384E-C6B8-4CE2-B587-2E8C00D5470B", "SCAC code is repeated in the bill number. Please remove the SCAC code from the bill number."); }
			}

			public static MultilingualString BillOfLadingNumberAlphanumeric
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondBill|FC86B349-A700-48E1-BCE1-D7B4A0EACF53", "Bill Of Lading number format is invalid - must be alphanumeric characters only."); }
			}

			public static MultilingualString BillOfLadingNumberIsDuplicated(CusInBondHeader header, ZString anotherJobReference, ZString anotherCompanyName, ZString anotherBranchName)
			{
				if (header.IsNVOCCHeader)
				{
					if (header.BH_ImportTransportMode == TransportTypeList.Codes.Rail)
					{
						return ResString.GetMultilingualString("AMS|CusInBondBill|F1A78688-1752-4809-A1DE-D4D87FBAF8AD", "Another AMS job already contains the master bill and house bill numbers (AMS: '{0}', Company: '{1}', Branch: '{2}').", anotherJobReference, anotherCompanyName, anotherBranchName);
					}
					else
					{
						return ResString.GetMultilingualString("AMS|CusInBondBill|690566A5-68B2-462E-B61C-637D89430F55", "Another AMS job already contains the ocean bill and house bill numbers (AMS: '{0}', Company: '{1}', Branch: '{2}').", anotherJobReference, anotherCompanyName, anotherBranchName);
					}
				}
				else
				{
					return ResString.GetMultilingualString("AMS|CusInBondBill|A857F59C-F78C-4BB6-92B3-5954A55E6D22", "Another AMS job already contains the house bill number (AMS: '{0}', Company: '{1}', Branch: '{2}').", anotherJobReference, anotherCompanyName, anotherBranchName);
				}
			}

			public static MultilingualString SamePortHasSameEstUnloadingDate(ZString portOfUnloading)
			{
				return ResString.GetMultilingualString("AMS|CusInBondBill|7308C765-4F19-41D3-A274-67E96018A907", "Port '{0}' can only have one estimated unloading date.", portOfUnloading);
			}
		}

		public static class MoveDetail
		{
			public static MultilingualString AtLeastOneContainerIsRequired
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondMoveDetail|C74C6AA3-8C9A-41b1-A89E-8E7BFA1EC189", "A Bill of Lading should have at least one Container Detail."); }
			}

			public static MultilingualString BillOfLadingShouldHaveOneMasterInBondOnly
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondMoveDetail|A0721DA1-727C-48AB-AB68-AA5C3E6134F2", "A Bill of Lading should only be on one Master In-Bond Movement; it can be on multiple Subsequent In-Bond Movement."); }
			}

			public static MultilingualString PTTQtyCannotExceedBOLQty
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondMoveDetail|B52E06AC-C8D2-4890-9CD6-1EA77865F903", "Permit To Transfer Quantity cannot exceed full BOL quantity."); }
			}

			public static MultilingualString CannotDeleteMoveDetailBeforeMessageDelete(string type)
			{
				return ResString.GetMultilingualString("AMS|CusInBondMoveDetail|E30DE289-9788-4331-A883-158BA5F3B562", "This Movement Detail cannot be deleted as it has been submitted to Customs.\r\nPlease send {0} message first before deleting this Movement Detail.", type);
			}

			public static MultilingualString InvalidReferencesForInBond(string message)
			{
				return ResString.GetMultilingualString("AMS|CusInBondMoveDetail|C1607498-17B7-4B14-BA90-08D051741C3E", "The following references will not be sent in the Subsequent In-Bond messaging as they are not supported:\r\n{0}", message);
			}

			public static MultilingualString MonetaryValueIsRequired
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondMoveDetail|E9AB2329-747D-4145-B67C-0680EE589AC3", "Please enter a value of the In-Bond Movement in whole dollars; $20 per kilo may be used if the value is unknown."); }
			}

			public static MultilingualString ForeignDestinationIsOnlyRequiredFor62Or63EntryType
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondMoveDetail|6CFE0A84-9154-440f-BD5C-A263E458A75D", "The Foreign Destination is only required for Entry Type '62' or '63'."); }
			}
		}

		public static class MoveHeader
		{
			public static MultilingualString CannotDeleteMovementWhenMessageExists
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondMoveHeader|0D16125A-9918-49B7-A0D0-CFD4F424E36E", "This Movement cannot be deleted as it has been submitted to Customs."); }
			}

			public static MultilingualString ManifestSequenceNumberIsRequired
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondMoveHeader|40026B05-98DA-45C6-A7D4-4498B03361D0", "Manifest Sequence Number is required for additional messaging to Customs; please ensure that it match the Manifest Sequence Number on INPMO1 record on a 'MR' message (Messages Tab > AMS Vessel Movement > Message Type = 'MR' > Message Details)."); }
			}

			public static MultilingualString AtLeastOneMoveDetailIsRequired
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondMoveHeader|9EFF5D2B-04BC-4EB4-9092-981F1B517611", "At least one movement detail should be entered."); }
			}

			public static MultilingualString InBondCarrierIDValid
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondMoveHeader|A3C81665-ED93-4d6f-91AB-96BCAC3578DA", "An In-Bond Carrier ID must be a valid IRS Number (NN-NNNNNNNXX) or CBP Assigned Number (YYDDPP-NNNNN) or Social Security Number (NNN-NN-NNNN) where N is a number and X is alphanumeric, YY is the last two digits of the calendar year, DDPP is the district/port code where the number is assigned."); }
			}

			public static MultilingualString USDestinationShouldNotMatchPortOfArrivalFor61EntryType
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondMoveHeader|A7110747-C2F6-4765-97E6-D10C23FF4C72", "The US Port Of Destination must not be the same as the Port Of Arrival when the Entry Type is '61'."); }
			}

			public static MultilingualString TOLStateCodeIsRequired
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondMoveHeader|FDD75584-D6C1-4f5e-81AF-C30ED8923305", "A State Code is required when the City Name of the place where the Transfer Of Liability is specified."); }
			}

			public static MultilingualString ArrivalDateExceedsTodaysDate
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondMoveHeader|6E8B196B-588C-4159-90BD-4B07D5A04436", "The Arrival Date cannot exceeds today's date."); }
			}
		}

		public static class JobDocAddress
		{
			public static MultilingualString GetOrganisationPKRequired(string addressType)
			{
				return ResString.GetMultilingualString("51966B7C-EB23-4C94-8710-16561519E67F", "A {0} must be specified.", addressType);
			}

			public static MultilingualString CompanyNameRequired
			{
				get { return ResString.GetMultilingualString("A278F68F-A2A6-4F53-8EAA-6C627D8B644F", "A Company Name is required."); }
			}

			public static MultilingualString AddressRequired
			{
				get { return ResString.GetMultilingualString("F39F63AF-E8FE-4101-B560-0C524174DECB", "An Address is required."); }
			}

			public static MultilingualString CustomsBrokerAddressRequiresABIRouting
			{
				get { return ResString.GetMultilingualString("5635B30C-D037-4E0E-9C26-B69CEB4B5E59", "There is not ABI Routing setup against this address."); }
			}
		}

		public static class Container
		{
			public static MultilingualString MustOnlyContainAlphaNumerics
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondContainer|BEE3BB06-64DD-40a6-877B-34737CD994F7", "Invalid Characters in Container Number - Container number must only contain alphanumeric characters."); }
			}

			public static MultilingualString ContainerNumberCanNotContainLeadingSpaces
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondContainer|0D34FF75-DB25-4D0F-9F24-8A069735E480", "Invalid Characters in Container Number - Container Number cannot contain leading spaces."); }
			}

			public static MultilingualString AtLeastOneCommodityIsRequired
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondContainer|2FC18E66-1463-4303-88E4-A78F4AE8F962", "A Container should have at least one Commodity Detail."); }
			}

			public static MultilingualString MaximumNumberOfUNDGExceeded
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondContainer|9B82286C-0723-4820-84F7-31DE353DA1FF", "A Container should only have a maximum of 99 Hazardous records."); }
			}

			public static MultilingualString ContainerNumberIsRequired
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondContainer|F73D5A8A-7BDC-4105-B2C8-E82035B2C7C5", "Please enter a valid container number associated with the bill of lading exactly as it physically appears on the container.\r\nEnter 'NC' for non-containerized freight."); }
			}

			public static MultilingualString NotEmptyWhenBillStatusIsEmptyContainer
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondContainer|26F0F0E4-ADE7-41CF-9110-50AEBF6C1E96", "The Bill Status is '2' however the container has not been flagged as empty."); }
			}

			public static MultilingualString ContainerNumberIsDuplicated
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondContainer|DA91EDF5-0BED-4b9d-93BC-8447DE25F553", "Please enter a different Container Number; there is already a Container entered with this number."); }
			}

			public static MultilingualString ForeignPortSchKIsRequired
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondContainer|BD3D9A66-F70A-4D4F-AED7-B6070D6E25BD", "A Foreign Port Schedule K is required when the container is empty."); }
			}

			public static MultilingualString MaxLengthOfSealNoExceeded
			{
				get { return ResString.GetMultilingualString("AMS|CusInbondContainer|227A5489-447F-4B93-8B1D-F38277BF8A3F", "Seal Number is too long. Only 15 chars allowed in manifest submission. Messages will be rejected by Customs."); }
			}

			public static MultilingualString MissingUSContainerCode
			{
				get { return ResString.GetMultilingualString("AMS|CusInBondContainer|5A1E1632-9459-4DE9-9AE0-A1A5DEBFD53C", "Please enter a container type and its associate equipment description code."); }
			}
		}

		public static class MessageSending
		{
			public static MultilingualString InBondNumberAllocationIsInProgress(string lockInfo)
			{
				return ResString.GetMultilingualString("AMS|MessageSendingBill|6EF2F35D-D985-40D3-B9C6-AC2A01ACFD99", "{0} is in the process of allocating a new InBond Number for this In-Bond movement ; this In-Bond movement cannot be sent now.", lockInfo);
			}

			public static MultilingualString NotUseAmendmentManifestIsNotOnFile
			{
				get { return ResString.GetMultilingualString("AMS|MessageSendingBill|4440D407-0F4A-44FE-B90D-3E48CE8ACFB2", "Manifest is not on file. If the vessel has not been arrived yet, you should send an Original Manifest, not an amendment."); }
			}

			public static MultilingualString BillIsAlreadyOnFileUseUpdateInsteadForCreatingAction
			{
				get { return ResString.GetMultilingualString("AMS|MessageSendingBill|9DE34FA4-834C-4B51-903B-505EC5614F45", "This Bill Of Lading is already on Customs' File; please send an Amendment Message with Action Code 'R' instead."); }
			}

			public static MultilingualString ReplaceMessageWillOnlyReplaceManifestQuantity
			{
				get { return ResString.GetMultilingualString("AMS|MessageSendingBill|40613320-D3BC-4C41-A3D4-A1F8508E6F30", "Replace message will only replace the Manifest Quantity. To replace the entire bill use 'X' - Delete/Add option."); }
			}

			public static MultilingualString BillIsAlreadyOnFileUseUpdateInstead
			{
				get { return ResString.GetMultilingualString("AMS|MessageSendingBill|4F79F48C-F7BC-402F-B5C3-83A56F2F7D3E", "Action Code 'A' should not be used when Bill Of Lading is already on Customs' File; please use Action Code 'R' instead."); }
			}

			public static MultilingualString ExportDateForVesselDeparture
			{
				get { return ResString.GetMultilingualString("AMS|MessageSendingMovement|1EBC3847-54BC-499A-A726-57B52B1E4528", "Export Date for Vessel Departure should be in the past."); }
			}

			public static MultilingualString ArrivalDateForVesselArrivalOrChangeDateEvent
			{
				get { return ResString.GetMultilingualString("AMS|MessageSendingMovement|C2438D16-0D7C-48F1-9BED-A2BD5A73A229", "Arrival Date should be in the past."); }
			}

			public static MultilingualString NewEstimatedDateOfArrival
			{
				get { return ResString.GetMultilingualString("AMS|MessageSendingMovement|CC637DC3-6F3D-4792-B613-2A0C5DE89CB5", "The existing EDA is currently within 5 days of the date the EDA change is requested."); }
			}

			public static MultilingualString AmendmentMessageShouldBeFiled
			{
				get { return ResString.GetMultilingualString("AMS|MessageSendingMovement|C7BA56FE-2782-493C-B10D-957403E4C226", "Vessel has already arrived. In order to add a bill after vessel arrival, a Manifest Amendment message should be filed."); }
			}
		}

		public static class SecondaryNotifyParty
		{
			public static MultilingualString FirstSNPShouldBeCarrierSCACForNVOCC
			{
				get { return ResString.GetMultilingualString("AMS|SecondaryNotifyParty|F646311D-EE61-4486-BCD5-1BC5F1B20AB8", "The first Secondary Notify Party should be the SCAC of the vessel operator for a NVOCC Bill."); }
			}

			public static MultilingualString MaximumNumberOfSecondaryNotifyPartyExceeded
			{
				get { return ResString.GetMultilingualString("AMS|SecondaryNotifyParty|001B6465-E94E-4B93-871B-59ED6D119BF2", "A Bill Of Lading should only have a maximum of 8 Secondary Notify Party records."); }
			}
		}

		public static class ShipmentReference
		{
			public static MultilingualString BillOfLadingFormat
			{
				get { return ResString.GetMultilingualString("AMS|CusInbondBillAddRef|EFC66F22-426C-43FE-93CE-FC26B15FA3C2", "Bill Of Lading should be 5 to 16 characters; the first 4 characters is the SCAC of the issuer of the Bill Of Lading."); }
			}

			public static IMultilingualString CensusScheduleK
			{
				get { return ResString.GetMultilingualString("AMS|CusInbondBillAddRef|6B2CA53A-6764-440C-BFFC-A89EC7774C4A", "Please enter a valid Census Schedule K code."); }
			}

			public static IMultilingualString UNLOCode
			{
				get { return ResString.GetMultilingualString("AMS|CusInbondBillAddRef|3DB58FA9-0FC7-4B12-BAF7-5E79A9AEB8D3", "Please enter a valid UNLOCO code."); }
			}

			public static MultilingualString SCACCode(ZString code)
			{
				return ResString.GetMultilingualString("AMS|CusInbondBillAddRef|CF936DC3-D1AD-4357-828A-4ED20DFB4A06", "The Standard Carrier Alpha Code (SCAC) '{0}' is not valid.", code);
			}
		}

		public static class SailingSynchronisation
		{
			public static MultilingualString ContainerMightBeIncorectlyAdded
			{
				get { return ResString.GetMultilingualString("AMS|ImportingFromSailing|7A6CC947-F025-4300-97D1-F9E68878B8D5", "Container is not linked to the Shipping -> Bill of Lading. It may have been incorrectly added."); }
			}

			public static MultilingualString VehicleMightBeIncorectlyAdded
			{
				get { return ResString.GetMultilingualString("AMS|ImportingFromSailing|C814F696-E54C-43DD-A033-9B3F46513980", "Vehicle is not linked to the Shipping -> Vehicles. It may have been incorrectly added."); }
			}

			public static MultilingualString BillMightBeIncorectlyAdded
			{
				get { return ResString.GetMultilingualString("AMS|ImportingFromSailing|C267B2B9-7B74-4A6C-ABF1-7C997B00E490", "Bill is not linked to the same Sailing. It may have been incorrectly added."); }
			}

			public static MultilingualString CommodityMightBeIncorectlyAdded
			{
				get { return ResString.GetMultilingualString("AMS|ImportingFromSailing|E7B9D20D-7E7E-4517-8009-D2317449CC26", "Commodity is not linked to the Shipping -> Bill of Lading -> Container ( or Shipping -> Bill of Lading -> Packing). It may have been incorrectly added."); }
			}

			public static MultilingualString HazardousDetailMightBeIncorectlyAdded
			{
				get { return ResString.GetMultilingualString("AMS|ImportingFromSailing|98EC0D53-8EEA-413D-9118-71BC83BA65CB", "Hazardous detail is not linked to the Shipping -> Bill of Lading -> Container. It may have been incorrectly added."); }
			}
		}
	}
}
