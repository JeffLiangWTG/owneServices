using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.AES
{
	public class AESHeaderPrint : AESPrint
	{
		public AESHeaderPrint(CusEntryHeader entry)
			: base(entry)
		{
		}

		IAESTIRMessageAttachee TIRMsgAttacheeHeader
		{
			get { return entry; }
		}

		public override ZString ITN
		{
			get { return entry.EntryNumber; }
		}

		public override ZString TransmittedVia
		{
			get { return "AES Shipment Record"; }
		}

		public override ZString ShipmentReferenceNumber
		{
			get { return TIRMsgAttacheeHeader.ShipmentReferenceNumber; }
		}

		public override ZDate DepartureDate
		{
			get { return entry.ExportDate.Date; }
		}

		public override ZString TransportationReferenceNo
		{
			get { return entry.Declaration.US_TransportReference; }
		}

		public override ZString StateOfOrigin
		{
			get { return GetStateDescription(TIRMsgAttacheeHeader.USStateOfOriginCode); }
		}

		public override ZString CountryOfDestination
		{
			get { return GetCountryOfDestinationName(TIRMsgAttacheeHeader.CountryOfUltimateDestinationCode); }
		}

		public override ZString PortOfExport
		{
			get { return GetPortOfExportDescription(TIRMsgAttacheeHeader.PortOfExportationCode); }
		}

		public override ZString ModeOfTransportation
		{
			get { return GetModeOfTransportationDescription(TIRMsgAttacheeHeader.ModeOfTransportationCodeMOT); }
		}

		public override ZString CarrierSCAC
		{
			get { return TIRMsgAttacheeHeader.CarrierIDSCACIATA; }
		}

		public override ZString ConveyanceName
		{
			get { return TIRMsgAttacheeHeader.ConveyanceNameCarrierName; }
		}

		public override ZString RoutedTransactionIndicator
		{
			get { return new YesNoDefaultList().GetDescriptionFromCode(TIRMsgAttacheeHeader.RoutedExportTransactionIndicator); }
		}

		public override ZString RelatedCompaniesIndicator
		{
			get { return new YesNoDefaultList().GetDescriptionFromCode(TIRMsgAttacheeHeader.RelatedCompanyIndicator); }
		}

		public override ZString HazardousIndicator
		{
			get { return new YesNoDefaultList().GetDescriptionFromCode(TIRMsgAttacheeHeader.HazardousMaterialIndicatorHAZMAT); }
		}

		public override ZString InBondType
		{
			get { return GetInBondTypeAndDescription(entry.InbondType); }
		}

		public override ZString FTZ
		{
			get { return TIRMsgAttacheeHeader.ForeignTradeZoneIdentifier; }
		}

		public override ZString ImportEntryNumber
		{
			get { return GetFormattedEntryNumber(TIRMsgAttacheeHeader.EntryNumber); }
		}

		#region USPPI

		public override ZString USPPIName
		{
			get { return USPPI != null ? USPPI.PartyName : ZString.Empty; }
		}

		public override ZString USPPIIDNumber
		{
			get { return USPPI != null ? USPPI.PartyID + GetIDTypeFormatted(USPPI.PartyIDType) : ""; }
		}

		public override ZString USPPIContactName
		{
			get { return GetContact(USPPI); }
		}

		ZString GetContact(IAESTIRParty party)
		{
			var result = ZString.Empty;
			if (party != null)
			{
				var builder = new ZStringBuilder();
				builder.AppendIfNotEmpty(party.ContactFirstName);
				builder.AppendIfNotEmpty(party.ContactLastName);
				result = builder.ToStringWithDelimiterBetweenAppends(" ");
			}
			return result;
		}

		public override ZString USPPIPhone
		{
			get { return USPPI != null ? USPPI.ContactPhoneNumber : ZString.Empty; }
		}

		public override ZString USPPICargoOriginLine1
		{
			get { return USPPI != null ? USPPI.AddressLine1 : ZString.Empty; }
		}

		public override ZString USPPICargoOriginLine2
		{
			get { return USPPI != null ? USPPI.AddressLine2 : ZString.Empty; }
		}

		public override ZString USPPICargoOriginLine3
		{
			get { return USPPI != null ? GetCompoundPartyAddress(USPPI) : ZString.Empty; }
		}

		IAESTIRParty USPPI
		{
			get { return usppi ?? (usppi = TIRMsgAttacheeHeader.USPPI); }
		}
		IAESTIRParty usppi;

		#endregion

		#region Ultimate Consignee

		public override ZString UltimateConsigneeName
		{
			get { return UltimateConsignee != null ? UltimateConsignee.PartyName : ZString.Empty; }
		}

		public override ZString UltimateConsigneeContact
		{
			get { return GetContact(UltimateConsignee); }
		}

		public override ZString UltimateConsigneePhone
		{
			get { return UltimateConsignee != null ? UltimateConsignee.ContactPhoneNumber : ZString.Empty; }
		}

		public override ZString UltimateConsigneeAddress1
		{
			get { return UltimateConsignee != null ? UltimateConsignee.AddressLine1 : ZString.Empty; }
		}

		public override ZString UltimateConsigneeAddress2
		{
			get
			{
				var result = ZString.Empty;
				if (UltimateConsignee != null)
				{
					result = !UltimateConsignee.AddressLine2.IsEmpty
							? UltimateConsignee.AddressLine2
							: GetCompoundPartyAddress(UltimateConsignee);
				}
				return result;
			}
		}

		public override ZString UltimateConsigneeAddress3
		{
			get { return UltimateConsignee != null && !UltimateConsignee.AddressLine2.IsEmpty ? GetCompoundPartyAddress(UltimateConsignee) : ZString.Empty; }
		}

		IAESTIRParty UltimateConsignee
		{
			get { return ultimateConsignee ?? (ultimateConsignee = TIRMsgAttacheeHeader.UltimateConsignee); }
		}
		IAESTIRParty ultimateConsignee;

		public override ZString UltimateConsigneeType
		{
			get { return TIRMsgAttacheeHeader != null ? TIRMsgAttacheeHeader.UltimateConsigneeType : ZString.Empty; }
		}

		#endregion

		#region Intermediate Consignee

		public override ZString IntermediateConsigneeName
		{
			get { return IntermediateConsignee != null ? IntermediateConsignee.PartyName : ZString.Empty; }
		}

		public override ZString IntermediateConsigneeContact
		{
			get { return GetContact(IntermediateConsignee); }
		}

		public override ZString IntermediateConsigneePhone
		{
			get { return IntermediateConsignee != null ? IntermediateConsignee.ContactPhoneNumber : ZString.Empty; }
		}

		public override ZString IntermediateConsigneeAddress1
		{
			get { return IntermediateConsignee != null ? IntermediateConsignee.AddressLine1 : ZString.Empty; }
		}

		public override ZString IntermediateConsigneeAddress2
		{
			get
			{
				var result = ZString.Empty;
				if (IntermediateConsignee != null)
				{
					result = !IntermediateConsignee.AddressLine2.IsEmpty
							? IntermediateConsignee.AddressLine2
							: GetCompoundPartyAddress(IntermediateConsignee);
				}
				return result;
			}
		}

		public override ZString IntermediateConsigneeAddress3
		{
			get { return IntermediateConsignee != null && !IntermediateConsignee.AddressLine2.IsEmpty ? GetCompoundPartyAddress(IntermediateConsignee) : ZString.Empty; }
		}

		IAESTIRParty IntermediateConsignee
		{
			get { return intermediateConsignee ?? (intermediateConsignee = TIRMsgAttacheeHeader.IntermediateConsignee); }
		}
		IAESTIRParty intermediateConsignee;

		#endregion

		#region Freight Forwarder

		public override ZString FreightForwarderName
		{
			get { return ForwardingAgent != null ? ForwardingAgent.PartyName : ZString.Empty; }
		}

		public override ZString FreightForwarderIDNumber
		{
			get { return ForwardingAgent != null ? ForwardingAgent.PartyID + GetIDTypeFormatted(ForwardingAgent.PartyIDType) : ""; }
		}

		public override ZString FreightForwarderContact
		{
			get { return GetContact(ForwardingAgent); }
		}

		public override ZString FreightForwarderPhone
		{
			get { return ForwardingAgent != null ? ForwardingAgent.ContactPhoneNumber : ZString.Empty; }
		}

		public override ZString FreightForwarderAddress1
		{
			get { return ForwardingAgent != null ? ForwardingAgent.AddressLine1 : ZString.Empty; }
		}

		public override ZString FreightForwarderAddress2
		{
			get { return ForwardingAgent != null ? ForwardingAgent.AddressLine2 : ZString.Empty; }
		}

		public override ZString FreightForwarderAddress3
		{
			get { return GetCompoundPartyAddress(ForwardingAgent); }
		}

		IAESTIRParty ForwardingAgent
		{
			get { return forwardingAgent ?? (forwardingAgent = TIRMsgAttacheeHeader.ForwardingAgent); }
		}
		IAESTIRParty forwardingAgent;

		#endregion

		ZString GetCompoundPartyAddress(IAESTIRParty party)
		{
			var result = ZString.Empty;
			if (party != null)
			{
				var builder = new ZStringBuilder();
				builder.AppendIfNotEmpty(party.City);
				builder.AppendIfNotEmpty(party.StateCode);
				builder.AppendIfNotEmpty(party.CountryCode);
				builder.AppendIfNotEmpty(party.PostalCode);
				result = builder.ToStringWithDelimiterBetweenAppends(" ");
			}
			return result;
		}

		public override AESPrintCommodityLinesCollection Commodities
		{
			get
			{
				var commodityLinesForDocumentPrinting = new AESPrintCommodityLinesCollection(Factory);

				foreach (var entryLine in entry.EntryLines)
				{
					var commodityLine = new AESHeaderCommodityLine(entryLine);
					commodityLinesForDocumentPrinting.Add(commodityLine);
				}

				return commodityLinesForDocumentPrinting;
			}
		}
	}
}
