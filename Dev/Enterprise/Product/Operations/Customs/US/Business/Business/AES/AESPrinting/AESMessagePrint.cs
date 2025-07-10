using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Input;

namespace Enterprise.Customs.US.AES
{
	public interface IAESMessagePrint
	{
		ZString MessageType { get; }
		ZString MessageText { get; }
		ZGuid LinkUniqueID { get; }
		BusinessObjectFactory Factory { get; }
	}

	public class AESMessagePrint : AESPrint
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public AESMessagePrint(IAESMessagePrint outgoingMessage)
			: base(outgoingMessage.Factory.Load<CusEntryHeader>(outgoingMessage.LinkUniqueID))
		{
			PopulatePropertiesFromAESTIRMessage(outgoingMessage.MessageText);
		}

		ZString partyType;
		AESTIRMessageCommodityLine printLineObject;

		void PopulatePropertiesFromAESTIRMessage(ZString messageText)
		{
			ZString[] outgoingMessageBlocks = messageText.Split(80);

			foreach (var block in outgoingMessageBlocks)
			{
				string blockType = block.Substring(0, 3);
				switch (blockType)
				{
					case "SC1":
						var sC1 = new AESCommShipSC1XP();
						sC1.Deserialise(block);

						relatedCompaniesIndicator = sC1.RelatedCompanyIndicator;
						modeOfTransportationCode = GetModeOfTransportationDescription(sC1.ModeOfTransportationCodeMOT);
						countryOfDestination = GetCountryOfDestinationName(sC1.CountryOfUltimateDestinationCode);
						stateOfOrigin = GetStateDescription(sC1.USStateOfOriginCode);
						carrierSCAC = sC1.CarrierIDSCACIATA;
						shipmentReferenceNumber = sC1.ShipmentReferenceNumber;
						conveyanceName = sC1.ConveyanceNameCarrierName;
						portOfExport = GetPortOfExportDescription(sC1.PortOfExportationCode);
						departureDate = sC1.EstimatedDateOfExport;
						hazardousIndicator = sC1.HazardousMaterialIndicatorHAZMAT;
						break;
					case "SC2":
						var sC2 = new AESCommShipSC2XP();
						sC2.Deserialise(block);

						inBondType = sC2.InbondCode;
						importEntryNumber = sC2.EntryNumber;
						ftz = sC2.ForeignTradeZoneIdentifier;
						routedTransactionIndicator = sC2.RoutedExportTransactionIndicator;
						break;
					case "SC3":
						if (transportationReferenceNo.IsEmpty)
						{
							var sC3 = new AESCommShipSC3XP();
							sC3.Deserialise(block);
							transportationReferenceNo = sC3.TransportationReferenceNumber;
						}
						break;
					case "N01":
						var n01 = new AESCommShipN01XP();
						n01.Deserialise(block);

						partyType = n01.PartyType;
						if (partyType == AESTIRPartyTypeList.Codes.USPPI)
						{
							usPPICompanyName = n01.PartyName;
							usPPIIDNumber = n01.PartyID + GetIDTypeFormatted(n01.PartyIDType);
							usPPIContactName = n01.ContactFirstName +
												(!n01.ContactLastName.IsEmpty ? " " + n01.ContactLastName : "");
						}
						else if (partyType == AESTIRPartyTypeList.Codes.UltimateConsignee)
						{
							ultimateConsigneeCompanyName = n01.PartyName;
							ultimateConsigneeContact = n01.ContactFirstName +
														(!n01.ContactLastName.IsEmpty ? " " + n01.ContactLastName : "");
						}
						else if (partyType == AESTIRPartyTypeList.Codes.IntermediateConsignee)
						{
							intermediateConsigneeCompanyName = n01.PartyName;
							intermediateConsigneeContact = n01.ContactFirstName +
														(!n01.ContactLastName.IsEmpty ? " " + n01.ContactLastName : "");
						}
						else if (partyType == AESTIRPartyTypeList.Codes.ForwardingAgent)
						{
							freightForwarderName = n01.PartyName;
							freightForwarderIDNumber = n01.PartyID + GetIDTypeFormatted(n01.PartyIDType);
							freightForwarderContact = n01.ContactFirstName +
													(!n01.ContactLastName.IsEmpty ? " " + n01.ContactLastName : "");
						}
						break;
					case "N02":
						var n02 = new AESCommShipN02XP();
						n02.Deserialise(block);

						if (partyType == AESTIRPartyTypeList.Codes.USPPI)
						{
							usPPICargoOriginLine1 = n02.AddressLine1;
							usPPICargoOriginLine2 = n02.AddressLine2;
							usPPIPhone = n02.ContactPhoneNumber;
						}
						else if (partyType == AESTIRPartyTypeList.Codes.UltimateConsignee)
						{
							ultimateConsigneeAddress1 = n02.AddressLine1;
							ultimateConsigneeAddress2 = n02.AddressLine2;
							ultimateConsigneePhone = n02.ContactPhoneNumber;
						}
						else if (partyType == AESTIRPartyTypeList.Codes.IntermediateConsignee)
						{
							intermediateConsigneeAddress1 = n02.AddressLine1;
							intermediateConsigneeAddress2 = n02.AddressLine2;
							intermediateConsigneePhone = n02.ContactPhoneNumber;
						}
						else if (partyType == AESTIRPartyTypeList.Codes.ForwardingAgent)
						{
							freightForwarderPhone = n02.ContactPhoneNumber;
							freightForwarderAddress1 = n02.AddressLine1;
							freightForwarderAddress2 = n02.AddressLine2;
						}
						break;
					case "N03":
						var n03 = new AESCommShipN03XP();
						n03.Deserialise(block);

						if (partyType == AESTIRPartyTypeList.Codes.USPPI)
						{
							usPPICargoOriginLine3 = GetCompoundAddressLine(n03);
						}
						else if (partyType == AESTIRPartyTypeList.Codes.UltimateConsignee)
						{
							ultimateConsigneeAddress3 = GetCompoundAddressLine(n03);
							ultimateConsigneeType = n03.UltimateConsigneeType;
						}
						else if (partyType == AESTIRPartyTypeList.Codes.IntermediateConsignee)
						{
							intermediateConsigneeAddress3 = GetCompoundAddressLine(n03);
						}
						else if (partyType == AESTIRPartyTypeList.Codes.ForwardingAgent)
						{
							freightForwarderAddress3 = GetCompoundAddressLine(n03);
						}
						break;
					case "CL1":
						var cL1 = new AESCommShipCL1XP();
						cL1.Deserialise(block);

						printLineObject = new AESTIRMessageCommodityLine(Factory);
						printLineObject.CL1Block = cL1;
						Commodities.Add(printLineObject);
						break;
					case "CL2":
						var cL2 = new AESCommShipCL2XP();
						cL2.Deserialise(block);

						printLineObject.CL2Block = cL2;
						break;
					case "PGA":
						var pga = new AESCommShipPGAXP();
						pga.Deserialise(block);

						printLineObject.PGABlock.Add(pga);
						break;
					case "ODT":
						var oDT = new AESCommShipODTXP();
						oDT.Deserialise(block);

						printLineObject.ODTBlock = oDT;
						break;
					case "EV1":
						var eV1 = new AESCommShipEV1XP();
						eV1.Deserialise(block);

						printLineObject.EV1Block = eV1;
						break;
				}
			}
		}

		ZString GetCompoundAddressLine(AESCommShipN03XP n03Block)
		{
			var builder = new ZStringBuilder();
			builder.AppendIfNotEmpty(n03Block.City);
			builder.AppendIfNotEmpty(n03Block.StateCode);
			builder.AppendIfNotEmpty(n03Block.CountryCode);
			builder.AppendIfNotEmpty(n03Block.PostalCode);
			return builder.ToStringWithDelimiterBetweenAppends(" ");
		}

		public override ZString TransmittedVia
		{
			get { return "AES Shipment Record"; }
		}

		public override ZString ShipmentReferenceNumber
		{
			get { return shipmentReferenceNumber; }
		}
		ZString shipmentReferenceNumber;

		public override ZString ITN
		{
			get { return entry.EntryNumber; }
		}

		public override ZDate DepartureDate
		{
			get { return departureDate; }
		}
		ZDate departureDate;

		public override ZString TransportationReferenceNo
		{
			get { return transportationReferenceNo; }
		}
		ZString transportationReferenceNo;

		public override ZString StateOfOrigin
		{
			get { return stateOfOrigin; }
		}
		ZString stateOfOrigin;

		public override ZString CountryOfDestination
		{
			get { return countryOfDestination; }
		}
		ZString countryOfDestination;

		public override ZString PortOfExport
		{
			get { return portOfExport; }
		}
		ZString portOfExport;

		public override ZString ModeOfTransportation
		{
			get { return modeOfTransportationCode; }
		}
		ZString modeOfTransportationCode;

		public override ZString CarrierSCAC
		{
			get { return carrierSCAC; }
		}
		ZString carrierSCAC;

		public override ZString ConveyanceName
		{
			get { return conveyanceName; }
		}
		ZString conveyanceName;

		public override ZString RoutedTransactionIndicator
		{
			get { return new YesNoDefaultList().GetDescriptionFromCode(routedTransactionIndicator); }
		}
		ZString routedTransactionIndicator;

		public override ZString RelatedCompaniesIndicator
		{
			get { return new YesNoDefaultList().GetDescriptionFromCode(relatedCompaniesIndicator); }
		}
		ZString relatedCompaniesIndicator;

		public override ZString HazardousIndicator
		{
			get { return new YesNoDefaultList().GetDescriptionFromCode(hazardousIndicator); }
		}
		ZString hazardousIndicator;

		public override ZString InBondType
		{
			get { return GetInBondTypeAndDescription(inBondType); }
		}
		ZString inBondType;

		public override ZString FTZ
		{
			get { return ftz; }
		}
		ZString ftz;

		public override ZString ImportEntryNumber
		{
			get { return GetFormattedEntryNumber(importEntryNumber); }
		}
		ZString importEntryNumber;

		#region USPPI

		public override ZString USPPIName
		{
			get { return usPPICompanyName; }
		}
		ZString usPPICompanyName;

		public override ZString USPPIIDNumber
		{
			get { return usPPIIDNumber; }
		}
		ZString usPPIIDNumber;

		public override ZString USPPIContactName
		{
			get { return usPPIContactName; }
		}
		ZString usPPIContactName;

		public override ZString USPPIPhone
		{
			get { return usPPIPhone; }
		}
		ZString usPPIPhone;

		public override ZString USPPICargoOriginLine1
		{
			get { return usPPICargoOriginLine1; }
		}
		ZString usPPICargoOriginLine1;

		public override ZString USPPICargoOriginLine2
		{
			get { return usPPICargoOriginLine2; }
		}
		ZString usPPICargoOriginLine2;

		public override ZString USPPICargoOriginLine3
		{
			get { return usPPICargoOriginLine3; }
		}
		ZString usPPICargoOriginLine3;

		#endregion

		#region Ultimate Consignee

		public override ZString UltimateConsigneeName
		{
			get { return ultimateConsigneeCompanyName; }
		}
		ZString ultimateConsigneeCompanyName;

		public override ZString UltimateConsigneeContact
		{
			get { return ultimateConsigneeContact; }
		}
		ZString ultimateConsigneeContact;

		public override ZString UltimateConsigneePhone
		{
			get { return ultimateConsigneePhone; }
		}
		ZString ultimateConsigneePhone;

		public override ZString UltimateConsigneeAddress1
		{
			get { return ultimateConsigneeAddress1; }
		}
		ZString ultimateConsigneeAddress1;

		public override ZString UltimateConsigneeAddress2
		{
			get { return !ultimateConsigneeAddress2.IsEmpty ? ultimateConsigneeAddress2 : ultimateConsigneeAddress3; }
		}
		ZString ultimateConsigneeAddress2;

		public override ZString UltimateConsigneeAddress3
		{
			get { return !ultimateConsigneeAddress2.IsEmpty ? ultimateConsigneeAddress3 : ZString.Empty; }
		}
		ZString ultimateConsigneeAddress3;

		#endregion

		#region Intermediate Consignee

		public override ZString IntermediateConsigneeName
		{
			get { return intermediateConsigneeCompanyName; }
		}
		ZString intermediateConsigneeCompanyName;

		public override ZString IntermediateConsigneeContact
		{
			get { return intermediateConsigneeContact; }
		}
		ZString intermediateConsigneeContact;

		public override ZString IntermediateConsigneePhone
		{
			get { return intermediateConsigneePhone; }
		}
		ZString intermediateConsigneePhone;

		public override ZString IntermediateConsigneeAddress1
		{
			get { return intermediateConsigneeAddress1; }
		}
		ZString intermediateConsigneeAddress1;

		public override ZString IntermediateConsigneeAddress2
		{
			get { return !intermediateConsigneeAddress2.IsEmpty ? intermediateConsigneeAddress2 : intermediateConsigneeAddress3; }
		}
		ZString intermediateConsigneeAddress2;

		public override ZString IntermediateConsigneeAddress3
		{
			get { return !intermediateConsigneeAddress2.IsEmpty ? intermediateConsigneeAddress3 : ZString.Empty; }
		}
		ZString intermediateConsigneeAddress3;

		#endregion

		#region Freight Forwarder

		public override ZString FreightForwarderName
		{
			get { return freightForwarderName; }
		}
		ZString freightForwarderName;

		public override ZString FreightForwarderIDNumber
		{
			get { return freightForwarderIDNumber; }
		}
		ZString freightForwarderIDNumber;

		public override ZString UltimateConsigneeType
		{
			get { return ultimateConsigneeType; }
		}
		ZString ultimateConsigneeType;

		public override ZString FreightForwarderContact
		{
			get { return freightForwarderContact; }
		}
		ZString freightForwarderContact;

		public override ZString FreightForwarderPhone
		{
			get { return freightForwarderPhone; }
		}
		ZString freightForwarderPhone;

		public override ZString FreightForwarderAddress1
		{
			get { return freightForwarderAddress1; }
		}
		ZString freightForwarderAddress1;

		public override ZString FreightForwarderAddress2
		{
			get { return freightForwarderAddress2; }
		}
		ZString freightForwarderAddress2;

		public override ZString FreightForwarderAddress3
		{
			get { return freightForwarderAddress3; }
		}
		ZString freightForwarderAddress3;

		#endregion

		public override AESPrintCommodityLinesCollection Commodities
		{
			get
			{
				if (printCommodityLines == null)
				{
					printCommodityLines = new AESPrintCommodityLinesCollection(Factory);
				}
				return printCommodityLines;
			}
		}
		AESPrintCommodityLinesCollection printCommodityLines;
	}
}
