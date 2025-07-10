using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.SG;
using Enterprise.Customs.SG.Registry;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging;
using Enterprise.Customs.SG.V4.Business.TypeSafe;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business
{
	public class CusEntryHeader : TypeSafeCusEntryHeader, Integration.Customs.SG.ICusEntryHeader, ICustomsDec
	{
		public enum EntryTypes { Unspecified, InPayment, InNonPayment, Outward, OutwardWithCO, Transhipment, CertificateOfOrigin }
		const string HandCarried = "HandCarried";

		public CusEntryHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CH_Status = Core.SGConstants.DeclarationStatus.JobOpenButNoMessageSent;
		}

		#region Entry Type/SubType

		public EntryTypes EntryType
		{
			get
			{
				switch (Declaration.JE_MessageType)
				{
					case MessageTypeCodeList.Codes.IPT:
						return EntryTypes.InPayment;
					case MessageTypeCodeList.Codes.INP:
						return EntryTypes.InNonPayment;
					case MessageTypeCodeList.Codes.OUT:
						return IncludesCO ? EntryTypes.OutwardWithCO : EntryTypes.Outward;
					case MessageTypeCodeList.Codes.TNP:
						return EntryTypes.Transhipment;
					case MessageTypeCodeList.Codes.COO:
						return EntryTypes.CertificateOfOrigin;
					default:
						return EntryTypes.Unspecified;
				}
			}
		}

		bool IncludesCO
		{
			get { return Declaration.IsOUTDEC && Declaration.JE_MessageSubType != DeclarationTypeCodeList.Codes.BKO && ((ITCODEC)this).ApplicationProductType != ""; }
		}

		public string EntrySubType
		{
			get { return Declaration.JE_MessageSubType; }
		}

		#endregion

		#region Entry/Message Status

		public bool CanSendOriginal
		{
			get
			{
				return
					CH_Status == ""
					|| CH_Status == Core.SGConstants.DeclarationStatus.JobOpenButNoMessageSent
					|| CH_Status == Core.SGConstants.DeclarationStatus.DeclarationRejectedByCustoms
					|| CH_Status == Core.SGConstants.DeclarationStatus.DeclarationHadSyntaxErrors
					|| EntryType == EntryTypes.CertificateOfOrigin && CH_Status == Core.SGConstants.DeclarationStatus.DeclarationPermitReceived
					|| CH_Status == Core.SGConstants.DeclarationStatus.CancellationAccepted;
			}
		}

		public bool CanSendWithdrawal
		{
			get
			{
				return Declaration.JE_MessageType != MessageTypeCodeList.Codes.COO && CH_Status == Core.SGConstants.DeclarationStatus.DeclarationPermitReceived
					|| CH_Status == Core.SGConstants.DeclarationStatus.AmendmentRejectedByCustoms
					|| CH_Status == Core.SGConstants.DeclarationStatus.AmendmentHadSyntaxErrors
					|| CH_Status == Core.SGConstants.DeclarationStatus.AmendmentRejectedByCustoms
					|| CH_Status == Core.SGConstants.DeclarationStatus.AmendmentPermitReceived
					|| CH_Status == Core.SGConstants.DeclarationStatus.RefundRejectedByCustoms
					|| CH_Status == Core.SGConstants.DeclarationStatus.RefundHadSyntaxErrors
					|| CH_Status == Core.SGConstants.DeclarationStatus.RefundRejectedByCustoms
					|| CH_Status == Core.SGConstants.DeclarationStatus.RefundPermitReceived
					|| CH_Status == Core.SGConstants.DeclarationStatus.CancellationRejectedByCustoms
					|| CH_Status == Core.SGConstants.DeclarationStatus.CancellationHadSyntaxErrors;
			}
		}

		public override bool IsWaitingForResponse
		{
			get { return IsOriginalPending || IsAmendmentPending || IsRefundPending || IsCancellationPending || IsEntryQueried; }
		}

		public bool IsOriginalPending
		{
			get { return CH_Status == Core.SGConstants.DeclarationStatus.DeclarationSent || CH_Status == Core.SGConstants.DeclarationStatus.DeclarationPending; }
		}

		public bool IsAmendmentPending
		{
			get { return CH_Status == Core.SGConstants.DeclarationStatus.AmendmentSent || CH_Status == Core.SGConstants.DeclarationStatus.AmendmentPending; }
		}

		public bool IsRefundPending
		{
			get { return CH_Status == Core.SGConstants.DeclarationStatus.RefundSent || CH_Status == Core.SGConstants.DeclarationStatus.RefundPending; }
		}

		public bool IsCancellationPending
		{
			get { return CH_Status == Core.SGConstants.DeclarationStatus.CancellationSent || CH_Status == Core.SGConstants.DeclarationStatus.CancellationPending; }
		}

		public bool IsEntryQueried
		{
			get { return CH_Status == Core.SGConstants.DeclarationStatus.DeclarationQuery || CH_Status == Core.SGConstants.DeclarationStatus.AmendmentQuery || CH_Status == Core.SGConstants.DeclarationStatus.RefundQuery || CH_Status == Core.SGConstants.DeclarationStatus.CancellationQuery; }
		}

		public bool IsCancelledByStatus
		{
			get { return CH_Status == Core.SGConstants.DeclarationStatus.CancellationAccepted; }
		}

		#endregion

		#region Properties

		public bool ShouldCalculateGST
		{
			get { return ShouldCalculatePayables; }
		}

		public bool ShouldCalculateDuty
		{
			get { return ShouldCalculatePayables; }
		}

		bool ShouldCalculatePayables
		{
			get { return EntryType == EntryTypes.InPayment || EntryType == EntryTypes.InNonPayment; }
		}

		public ZBool IsTestMessage
		{
			get { return SGCustomsDataRegistry.Instance.SendTestMessages.Value; }
		}

		#endregion

		#region Interface Implementation

		ISGCUSDEC SGCUSDEC
		{
			get { return this; }
		}

		#region ISGCUSDEC Members

		ZBool ISGCUSDEC.IsImport
		{
			get { return Declaration.IsImport; }
		}

		ZBool ISGCUSDEC.HasInwardTransport
		{
			get { return !Declaration.JE_TransportMode.IsEmpty; }
		}

		ZBool ISGCUSDEC.IsExport
		{
			get { return Declaration.IsExport; }
		}

		ZBool ISGCUSDEC.HasOutwardTransport
		{
			get { return !Declaration.SG_OutwardTransportMode.IsEmpty; }
		}

		ZBool ISGCUSDEC.IsSea
		{
			get { return Declaration.IsSea || Declaration.IsOutwardTransportModeSea; }
		}

		ZBool ISGCUSDEC.IsAir
		{
			get { return Declaration.IsAir || Declaration.IsOutwardTransportModeAir; }
		}

		ZString ISGCUSDEC.InwardJourneyIdentifier
		{
			get
			{
				var result = ZString.Empty;
				if ((Declaration.IsAir || Declaration.IsSea) && !Declaration.IsOutwardTransportOnly)
				{
					result = Declaration.JE_VoyageFlightNo.IsEmpty ? noFlightVoyageConstant : Declaration.JE_VoyageFlightNo;
				}

				return result;
			}
		}
		readonly ZString noFlightVoyageConstant = "NA";

		ZString ISGCUSDEC.InwardTransportIdentifier
		{
			get
			{
				var result = ZString.Empty;

				if (!Declaration.IsOutwardTransportOnly)
				{
					if (Declaration.IsSea)
					{
						result = Declaration.JE_VesselName;
					}
					else if (Declaration.IsAir)
					{
						result = Declaration.JE_Folio;  // will contain Charter Flight Aircraft Registration
					}
					else if (Declaration.IsRoad)
					{
						result = Declaration.JE_VoyageFlightNo; // will contain Vehicle registration number
					}
				}

				return result;
			}
		}

		ZString ISGCUSDEC.OutwardJourneyIdentifier
		{
			get
			{
				var result = ZString.Empty;
				if (Declaration.IsOutwardTransportModeAir || Declaration.IsOutwardTransportModeSea)
				{
					result = Declaration.SG_OutwardVoyageFlightNo.IsEmpty ? noFlightVoyageConstant : Declaration.SG_OutwardVoyageFlightNo;
				}

				return result;
			}
		}

		ZString ISGCUSDEC.OutwardTransportIdentifier
		{
			get
			{
				var result = ZString.Empty;
				if (Declaration.IsOutwardTransportModeSea)
				{
					result = Declaration.SG_OutwardVesselName;
				}
				else if (Declaration.IsOutwardTransportModeAir)
				{
					result = Declaration.SG_OutwardFolio;   // will contain Charter Flight Aircraft Registration
				}
				else if (Declaration.IsOutwardTransportModeRoad)
				{
					result = Declaration.SG_OutwardVoyageFlightNo; // will contain Vehicle registration number
				}

				return result;
			}
		}

		ZBool ISGCUSDEC.IsInwardDeclaration
		{
			get { return Declaration.JE_MessageType == MessageTypeCodeList.Codes.INP || Declaration.JE_MessageType == MessageTypeCodeList.Codes.IPT; }
		}

		ZBool ISGCUSDEC.IsOutwardDeclaration
		{
			get { return Declaration.IsOUTDEC; }
		}

		ZBool ISGCUSDEC.IsTranshipmentDeclaration
		{
			get { return Declaration.JE_MessageType == MessageTypeCodeList.Codes.TNP; }
		}

		#region Vessels

		#region Inward Vessel

		ZString ISGCUSDEC.InwardVesselType
		{
			get
			{
				ZString result = "";
				if (Declaration.Vessel != null)
				{
					result = Declaration.Vessel.RV_VesselType.Length > 2 ? (ZString)Core.Constants.VesselType.CargoVessel : Declaration.Vessel.RV_VesselType;
				}
				else if (!Declaration.JE_VesselName.IsEmpty)
				{
					result = Core.Constants.VesselType.CargoVessel;
				}

				return result;
			}
		}

		#endregion

		#region Outward Vessel

		ZInt ISGCUSDEC.OutwardVesselNRT => Declaration.SGE_OutwardVesselNRT;

		ZString ISGCUSDEC.OutwardVesselType => Declaration.SGE_OutwardVesselType;

		ZString ISGCUSDEC.OutwardVesselNationality => Declaration.SGE_RN_NKOutwardVesselNationality;

		#endregion

		#region Towing Vessel

		ZString ISGCUSDEC.TowingVesselName => Declaration.SG_TowingVesselName;

		ZString ISGCUSDEC.TowingVesselVoyageNo => Declaration.SG_TowingVoyageNumber;

		#endregion

		#endregion

		ZBool ISGCUSDEC.IsShortPayment => Declaration.PlaceOfReceipt.IsShortPayment();

		ZBool ISGCUSDEC.IsExempted => Declaration.SG_GoodsPreviouslyExemptedFromDuties;

		ZBool ISGCUSDEC.IsRecoveryPayment => Declaration.PlaceOfReceipt.IsRecoveryPayment();

		ZString ISGCUSDEC.CargoPackingType => Declaration.JE_ContainerMode;

		ZString ISGCUSDEC.JobNumber
		{
			get { return Declaration.JobNumber; }
		}

		ZString ISGCUSDEC.DeclarationType
		{
			get { return Declaration.JE_MessageSubType; }
		}

		ZInt ISGCUSDEC.InwardTransportCode
		{
			get { return GetModeFromCode(Declaration.JE_TransportMode); }
		}

		ZInt ISGCUSDEC.OutwardTransportCode
		{
			get { return GetModeFromCode(Declaration.SG_OutwardTransportMode); }
		}

		ZInt GetModeFromCode(ZString transportCode)
		{
			ZInt transportModeNumber = 0;
			switch (transportCode)
			{
				case Core.Constants.TransportModes.Sea:
					transportModeNumber = 1;
					break;
				case Core.Constants.TransportModes.Rail:
					transportModeNumber = 2;
					break;
				case Core.Constants.TransportModes.Road:
					transportModeNumber = 3;
					break;
				case Core.Constants.TransportModes.Air:
					transportModeNumber = 4;
					break;
				case Core.Constants.TransportModes.Mail:
					transportModeNumber = 5;
					break;
				case Core.Constants.TransportModes.Other: //Pipeline
					transportModeNumber = 7;
					break;
				default:
					transportModeNumber = 0;
					break;
			}

			return transportModeNumber;
		}

		ZString ISGCUSDEC.SupplyIndicator
		{
			get { return Declaration.SG_SupplyIndicator; }
		}

		IEnumerable<ZString> ISGCUSDEC.AdditionalRecipients
		{
			get
			{
				StringCollectionX additionalRecipients = new StringCollectionX();

				if (!Declaration.SG_AdditionalRecipientID1.IsEmpty)
				{
					additionalRecipients.Add(Declaration.SG_AdditionalRecipientID1);
				}

				if (!Declaration.SG_AdditionalRecipientID2.IsEmpty)
				{
					additionalRecipients.Add(Declaration.SG_AdditionalRecipientID2);
				}

				if (!Declaration.SG_AdditionalRecipientID3.IsEmpty)
				{
					additionalRecipients.Add(Declaration.SG_AdditionalRecipientID3);
				}

				ZString[] result = new ZString[additionalRecipients.Count];
				for (int i = 0; i < additionalRecipients.Count; i++)
				{
					result[i] = additionalRecipients[i];
				}

				return result;
			}
		}

		#region NumberOfRequestsForUpdate

		ZInt ISGCUSDEC.NumberOfRequestsForUpdate
		{
			get
			{
				var ediPermitSegment = "RFF+ABT:" + EntryNumber;
				var xmlPermitNode = FormattableString.Invariant($"<cbc:PermitNumber>{EntryNumber}</cbc:PermitNumber>");

				var latestCancellationMessageTime = GetLatestCancellationMessageTimeForPermit(ediPermitSegment, xmlPermitNode);
				var noNeedCheckTime = latestCancellationMessageTime <= ZDateTime.MinSmallDateTimeValue;

				var permitsTotal = Messages.OfType<EDIMessage>().Count(c => IsValidPermitMessage(c, noNeedCheckTime, latestCancellationMessageTime, ediPermitSegment, xmlPermitNode));
				return permitsTotal == 0 ? 1 : permitsTotal;
			}
		}

		bool IsValidPermitMessage(EDIMessage message, bool noNeedCheckTime, ZDateTime latestCancellationMessageTime, string ediPermitSegment, string xmlPermitNode)
		{
			var result = !message.IsTransmitMessage
				&& message.EM_MessageType == Cuspmt09bMessageProcessor.MessageType
				&& message.EM_Status == EDIMessage.Status.Received
				&& (noNeedCheckTime || message.EM_SystemCreateTimeUtc > latestCancellationMessageTime);

			if (message is SGXmlEDIMessage xmlMessage)
			{
				result = result && xmlMessage.EM_MessageText.Contains(xmlPermitNode, StringComparison.OrdinalIgnoreCase);
			}
			else
			{
				result = result && message.EM_MessageText.Contains(ediPermitSegment, StringComparison.OrdinalIgnoreCase);
			}

			return result;
		}

		ZDateTime GetLatestCancellationMessageTimeForPermit(string ediPermitSegment, string xmlPermitNode)
		{
			var result = ZDateTime.MinSmallDateTimeValue;

			void FindLatestCancellationMessageTimeCore(IEnumerable<EDIMessage> messages, string match)
			{
				foreach (var message in messages)
				{
					if (message.EM_SystemCreateTimeUtc > result)
					{
						if (message.EM_MessageText.Contains(match, StringComparison.OrdinalIgnoreCase))
						{
							result = message.EM_SystemCreateTimeUtc;
						}
					}
				}
			}

			var ediMessages = Messages.GetMatchingMessages(ApplicationCodeList.Codes.SGCustomsTradenet4, new ZString[] { Cusres09bMessageProcessor.MessageType }, EDIMessage.Direction.Receive);
			var xmlMessages = Messages.GetMatchingMessages(ApplicationCodeList.Codes.SGCustomsTradenetXML, new ZString[] { Cusres09bMessageProcessor.MessageType }, EDIMessage.Direction.Receive);

			FindLatestCancellationMessageTimeCore(ediMessages, ediPermitSegment);
			FindLatestCancellationMessageTimeCore(xmlMessages, xmlPermitNode);

			return result;
		}

		#endregion

		ZString ISGCUSDEC.InwardMasterBill
		{
			get
			{
				ZString result = Declaration.JE_MasterBill;
				if (result.IsEmpty && Declaration.SG_IsInwardHandCarried)
				{
					result = HandCarried;
				}

				return result;
			}
		}

		ZString ISGCUSDEC.OutwardMasterBill
		{
			get
			{
				ZString result = Declaration.SG_OutwardMAWB;
				if (result.IsEmpty && Declaration.SG_IsOutwardHandCarried)
				{
					result = HandCarried;
				}

				return result;
			}
		}

		ZString ISGCUSDEC.InwardHouseBill
		{
			get { return Declaration.JE_HouseBill; }
		}

		ZString ISGCUSDEC.OutwardHouseBill
		{
			get { return Declaration.SG_OutwardHAWB; }
		}

		ZDecimal ISGCUSDEC.TotalOuterPack
		{
			get { return Declaration.JE_TotalNoOfPacksDecimal; }
		}

		ZString ISGCUSDEC.TotalOuterPackUnitOfQty
		{
			get { return Declaration.JE_TotalNoOfPacksPackType; }
		}

		ZDecimal ISGCUSDEC.TotalGrossWeight
		{
			get { return Declaration.JE_TotalWeight; }
		}

		ZString ISGCUSDEC.TotalGrossWeightUnitOfQty
		{
			get { return Declaration.JE_TotalWeightUnit; }
		}

		ZBool ISGCUSDEC.IsContainerised
		{
			get { return Declaration.IsContainerised; }
		}

		ZString ISGCUSDEC.PortOfLoading
		{
			get { return Declaration.JE_RL_NKPortOfLoading; }
		}

		ZString ISGCUSDEC.PortOfDischarge
		{
			get { return Declaration.JE_RL_NKPortOfArrival; }
		}

		ZString ISGCUSDEC.NextPortOfCall
		{
			get { return Declaration.SG_RL_NKNextPortOfCall; }
		}

		ZString ISGCUSDEC.FinalPortOfCall
		{
			get { return Declaration.SG_RL_NKFinalPortOfCall; }
		}

		ZDate ISGCUSDEC.ArrivalDate
		{
			get { return (ZDate)Declaration.JE_DateOfArrival; }
		}

		ZDate ISGCUSDEC.DepartureDate
		{
			get { return (ZDate)Declaration.JE_ExportDate; }
		}

		ZDate ISGCUSDEC.StartDateOfBlanket
		{
			get { return (ZDate)Declaration.SG_RemovalStartDate; }
		}

		ZBool ISGCUSDEC.IsStorageInFTZ => Declaration.PlaceOfStorage.IsFTZ();

		ZBool ISGCUSDEC.IsSeaStoreDeclaration
		{
			get { return Declaration.IsSeaStore; }
		}

		ZInt ISGCUSDEC.NumberOfCrew
		{
			get { return Declaration.SG_NoOfCrew; }
		}

		ZInt ISGCUSDEC.VoyageDuration
		{
			get { return Declaration.SG_VoyageDuration; }
		}

		ZBool ISGCUSDEC.HasLiquorOrTobacco
		{
			get { return Declaration.HasLiquorOrTobacco; }
		}

		ISGCPlace ISGCUSDEC.PlaceOfRelease
		{
			get { return Declaration.SG_US_NKPlaceOfCargoRelease != "" ? new EntrySGCPlaceInfo(Declaration.SG_US_NKPlaceOfCargoRelease) : null; }
		}

		ZString ISGCUSDEC.PlaceOfStorage
		{
			get { return Declaration.SG_US_NKPlaceOfStorage; }
		}

		ISGCPlace ISGCUSDEC.PlaceOfReceipt
		{
			get { return Declaration.SG_US_NKPlaceOfReceipt != "" ? new EntrySGCPlaceInfo(Declaration.SG_US_NKPlaceOfReceipt) : null; }
		}

		ISGCPlace ISGCUSDEC.InwardVesselBerth
		{
			get { return Declaration.SG_US_NKInwardVesselBerth != "" ? new EntrySGCPlaceInfo(Declaration.SG_US_NKInwardVesselBerth) : null; }
		}

		ISGCPlace ISGCUSDEC.OutwardVesselBerth
		{
			get { return Declaration.SG_US_NKOutwardVesselBerth != "" ? new EntrySGCPlaceInfo(Declaration.SG_US_NKOutwardVesselBerth) : null; }
		}

		ZString ISGCUSDEC.CountryOfFinalDestination
		{
			get { return Declaration.SG_RN_NKFinalDestination; }
		}

		IEnumerable<ICusContainer> ISGCUSDEC.Containers
		{
			get
			{
				foreach (ICusContainer cusContainer in Declaration.CusContainers)
				{
					yield return cusContainer;
				}
			}
		}

		#region GetPreviousMessageContainerSequence

		IContainerSequenceStore ISGCUSDEC.GetPreviousMessageContainerSequence()
		{
			return new ContainerSequenceStore(GetContainerSequence());
		}

		Dictionary<string, int> GetContainerSequence()
		{
			var result = new Dictionary<string, int>();

			EDIMessage permitMessage = (Declaration.IsTradeNet4Point1) ?
				GetLastVersionFourPointOnePermitMessage() :
				null;

			if (permitMessage != null)
			{
				var edifactMessage = permitMessage.GetAutoEdifactMessageUsingNamedFactory(
					Enterprise.Customs.SG.Business.CustomsMessaging.D09B.Sg09bEdifactMessageFactory.SG41MessageFactory,
					new Edifact.UNOASGCharacterSet());
				var cuspmtMessage = edifactMessage as Edifact.D09B.Messages.CUSPMT.CUSPMTMessage;

				if (cuspmtMessage != null)
				{
					foreach (Edifact.D09B.Segments.EQDSegment eqd in cuspmtMessage.EQD)
					{
						int seqNumber;

						if (int.TryParse(eqd.EquipmentIdentification.CodeListIdentificationCode, out seqNumber))
						{
							result[eqd.EquipmentIdentification.EquipmentIdentifier] = seqNumber;
						}
					}
				}
			}

			return result;
		}

		EDIMessage GetLastVersionFourPointOnePermitMessage()
		{
			var lastPermitMessage = Messages.GetLastMessage(
				SGConstants.TradeNetVersion.Four, Cuspmt09bMessageProcessor.MessageType, EDIMessage.Direction.Receive,
				EDIMessage.Status.Received, Cuspmt09bMessageProcessor.MessageType);

			return lastPermitMessage;
		}

		#endregion

		ZString ISGCUSDEC.DeclarantId
		{
			get { return DeclaringBroker != null ? SGCUSDEC.Declarant.EntityIdentifier : ZString.Empty; }
		}

		ZString ISGCUSDEC.PreviousPermitNumber
		{
			get { return Declaration.SG_PreviousPermitNo; }
		}

		ZString ISGCUSDEC.PermitNoToUpdateOrCancel
		{
			get { return Declaration.DeclarationNumber; }
		}

		IEnumerable<ICusDocument> ISGCUSDEC.LicencesAndDocuments => Declaration.CALicences;

		IEnumerable<ZString> ISGCUSDEC.TradersRemarksForMessage => Declaration.TradersRemarks.Select(x => x.CSI_Description);

		IEnumerable<ICusCPC> ISGCUSDEC.CPCs => Declaration.CPCs;

		ZBool ISGCUSDEC.IsReleasedInLicensedPremiseExclBWCY
		{
			get
			{
				var placeOfRelease = Declaration.PlaceOfRelease;
				return placeOfRelease.IsLicencedPremise() && !placeOfRelease.IsBWCY();
			}
		}

		ZBool ISGCUSDEC.IsStoredInLicensedPremise => Declaration.PlaceOfStorage.IsLicencedPremise();

		ZBool ISGCUSDEC.IsForStorage
		{
			get { return Declaration.SG_US_NKPlaceOfStorage != ""; }
		}

		ZString ISGCUSDEC.BGIndicator
		{
			get { return Declaration.JE_PaymentMethod; }
		}

		ICusAgentInfo ISGCUSDEC.Declarant
		{
			get { return new EntryDeclarantInfo(DeclaringBroker); }
		}

		public GlbStaff DeclaringBroker
		{
			get { return Declaration.CusAgent; }
		}

		ZString ISGCUSDEC.ClaimantCode
		{
			get { return Declaration.SG_ClaimantCode; }
		}

		ZString ISGCUSDEC.ClaimantName
		{
			get { return Declaration.SG_ClaimantName; }
		}

		#region IOrganisation

		IOrganisation ISGCUSDEC.Claimant
		{
			get
			{
				var claimant = Declaration.Claimant;
				return claimant != null ? new EntryOrganisationsInfo(claimant) : null;
			}
		}

		IOrganisation ISGCUSDEC.Manufacturer
		{
			get { return ManufacturerInfo; }
		}

		IOrganisation ISGCUSDEC.Importer
		{
			get { return ImporterInfo; }
		}

		IOrganisation ISGCUSDEC.Exporter
		{
			get { return ExporterInfo; }
		}

		IOrganisation ISGCUSDEC.FreightForwarder
		{
			get { return ForwarderInfo; }
		}

		IOrganisation ISGCUSDEC.InwardCarrierAgent
		{
			get { return InwardCarrierAgentInfo; }
		}

		IOrganisation ISGCUSDEC.OutwardCarrierAgent
		{
			get { return OutwardCarrierAgentInfo; }
		}

		IOrganisation ISGCUSDEC.HandlingAgent
		{
			get { return HandlingAgentInfo; }
		}

		IOrganisation ISGCUSDEC.Consignee
		{
			get { return ConsigneeInfo; }
		}

		IOrganisation ISGCUSDEC.EndUser
		{
			get { return EndUserInfo; }
		}

		#endregion

		IEnumerable<ICusInvoice> ISGCUSDEC.Invoices
		{
			get
			{
				var uniqueInvoices = new List<ZGuid>();
				foreach (ICusInvoice cusInvoice in MergedLines)
				{
					if (!uniqueInvoices.Contains(cusInvoice.InvoicePK))
					{
						uniqueInvoices.Add(cusInvoice.InvoicePK);
						yield return cusInvoice;
					}
				}
			}
		}

		IEnumerable<ICusItem> ISGCUSDEC.Items => MergedLines;

		ZBool ISGCUSDEC.IsDG
		{
			get
			{
				foreach (CusEntryLine cusEntryLine in MergedLines)
				{
					if (cusEntryLine.IsDG)
					{
						return true;
					}
				}

				return false;
			}
		}

		#region Totals

		ZDecimal ISGCUSDEC.TotalDutyPayable
		{
			get
			{
				ZDecimal result = 0m;

				foreach (CusEntryLine cusEntryLine in MergedLines)
				{
					result += cusEntryLine.DutyAmount;
				}

				return result;
			}
		}

		ZDecimal ISGCUSDEC.TotalExcisePayable
		{
			get
			{
				ZDecimal result = 0m;

				foreach (CusEntryLine cusEntryLine in MergedLines)
				{
					result += cusEntryLine.ExciseAmount;
				}

				return result;
			}
		}

		ZDecimal ISGCUSDEC.TotalOtherTaxPayable
		{
			get
			{
				ZDecimal result = 0m;

				foreach (CusEntryLine cusEntryLine in MergedLines)
				{
					result += cusEntryLine.OtherTaxAmount;
				}

				return result;
			}
		}

		ZDecimal ISGCUSDEC.TotalGSTPayable
		{
			get
			{
				ZDecimal result = 0m;

				foreach (CusEntryLine cusEntryLine in MergedLines)
				{
					result += cusEntryLine.GSTVATAmount;
				}

				return result;
			}
		}

		ZDecimal TotalGSTNotPayable
		{
			get
			{
				ZDecimal result = 0m;

				if (EntrySubType == DeclarationTypeCodeList.Codes.BKT && SGCUSDEC.SupplyIndicator.IsEmpty)
				{
					foreach (CusEntryLine cusEntryLine in MergedLines)
					{
						bool originIsSingapore = cusEntryLine.CountryOfOrigin != null && cusEntryLine.CountryOfOrigin.Code == Core.Constants.CountryCodes.Singapore;

						if (originIsSingapore)
						{
							result += cusEntryLine.GSTVATAmount;
						}
					}
				}

				return result;
			}
		}

		ZDecimal ISGCUSDEC.TotalPayable
		{
			get
			{
				ZDecimal result = 0m;

				if (EntryType == EntryTypes.InPayment)
				{
					ZDecimal totalDutyExciseAndOtherTaxPayable = SGCUSDEC.TotalDutyPayable + SGCUSDEC.TotalExcisePayable + SGCUSDEC.TotalOtherTaxPayable;

					if (EntrySubType == DeclarationTypeCodeList.Codes.GST
						|| Declaration.PlaceOfReceipt.IsExemptPlaceCodePresident())
					{
						result = SGCUSDEC.TotalGSTPayable;
					}
					else if (EntrySubType == DeclarationTypeCodeList.Codes.DUT)
					{
						result = totalDutyExciseAndOtherTaxPayable;
					}
					else
					{
						result = totalDutyExciseAndOtherTaxPayable + SGCUSDEC.TotalGSTPayable - TotalGSTNotPayable;
					}
				}

				return result;
			}
		}

		ZDecimal ISGCUSDEC.TotalCustomsValue
		{
			get
			{
				ZDecimal result = 0m;

				foreach (CusEntryLine cusEntryLine in MergedLines)
				{
					result += cusEntryLine.CustomsValue.Amount;
				}

				return result;
			}
		}

		#endregion

		ZString ISGCUSDEC.ReplacementPermitNumber
		{
			get { return Declaration.SG_ReplacementPermitNo; }
		}

		#endregion

		#region IOrganisation Members

		#region Importer

		IOrganisation ImporterInfo
		{
			get { return Declaration.Importer != null ? new EntryOrganisationsInfo(Declaration.Importer, Declaration.SG_ImporterNameOverride) : null; }
		}

		#endregion

		#region Exporter

		IOrganisation ExporterInfo
		{
			get
			{
				OrgHeader exporter = null;
				if (Declaration.JE_OH_Exporter.IsValid)
				{
					exporter = Factory.Load<OrgHeader>(Declaration.JE_OH_Exporter);
				}

				return exporter != null ? new EntryOrganisationsInfo(exporter) : null;
			}
		}

		#endregion

		#region Forwarder

		IOrganisation ForwarderInfo
		{
			get { return Declaration.Forwarder != null ? new EntryOrganisationsInfo(Declaration.Forwarder) : null; }
		}

		#endregion

		#region Manufacturer

		IOrganisation ManufacturerInfo
		{
			get
			{
				OrgHeader manufacturer = null;
				if (Declaration.JE_OH_Manufacturer.IsValid)
				{
					manufacturer = Factory.Load<OrgHeader>(Declaration.JE_OH_Manufacturer);
				}

				return manufacturer != null ? new EntryOrganisationsInfo(manufacturer) : null;
			}
		}

		#endregion

		#region InwardCarrierAgent

		IOrganisation InwardCarrierAgentInfo
		{
			get
			{
				var inwardCarrierAgent = Declaration.InwardCarrierAgent;
				return inwardCarrierAgent != null ? new EntryOrganisationsInfo(inwardCarrierAgent) : null;
			}
		}

		#endregion

		#region OutwardCarrierAgent

		IOrganisation OutwardCarrierAgentInfo
		{
			get
			{
				OrgHeader outwardCarrierAgent = null;
				if (Declaration.OutwardShippingLineForwarderPK.IsValid)
				{
					outwardCarrierAgent = Factory.Load<OrgHeader>(Declaration.OutwardShippingLineForwarderPK);
				}

				return outwardCarrierAgent != null ? new EntryOrganisationsInfo(outwardCarrierAgent) : null;
			}
		}

		#endregion

		#region Consignee

		IOrganisation ConsigneeInfo
		{
			get
			{
				OrgHeader consignee = null;
				if (Declaration.JE_OH_Consignee.IsValid)
				{
					consignee = Factory.Load<OrgHeader>(Declaration.JE_OH_Consignee);
				}

				return consignee != null ? new EntryOrganisationsInfo(consignee) : null;
			}
		}

		#endregion

		#region EndUser

		IOrganisation EndUserInfo
		{
			get
			{
				return Declaration.Buyer != null ? new EntryOrganisationsInfo(Declaration.Buyer) : null;
			}
		}

		#endregion

		#region HandlingAgent

		IOrganisation HandlingAgentInfo
		{
			get
			{
				OrgHeader handlingAgent = null;
				if (Declaration.JE_OH_HandlingAgent.IsValid)
				{
					handlingAgent = Factory.Load<OrgHeader>(Declaration.JE_OH_HandlingAgent);
				}

				return handlingAgent != null ? new EntryOrganisationsInfo(handlingAgent) : null;
			}
		}

		#endregion

		#endregion

		#region IINPDEC Members

		ZBool IINPDEC.IsTemporaryConsignment
		{
			get { return Declaration.IsTemporaryConsignment; }
		}

		ZDate IINPDEC.StartDateOfTemporaryImport
		{
			get { return (ZDate)Declaration.SG_RemovalStartDate; }
		}

		ZDate IINPDEC.EndDateOfTemporaryImport
		{
			get { return (ZDate)Declaration.SG_EndDateTempImport; }
		}

		ZBool IINPDEC.GoodsImportedUnderMESorBWS
		{
			get { return Declaration.SG_GoodsImportedUnderMESorBWS; }
		}

		#endregion

		#region IOUTDEC Members

		ITCODEC IOUTDEC.CO
		{
			get { return this; }
		}

		ZBool IOUTDEC.IsReceiptInLicensedPremise => Declaration.PlaceOfReceipt.IsLicencedPremise();

		#endregion

		#region ITNPDEC Members

		ZDate ITNPDEC.StartDateOfCargoRemoval
		{
			get { return (ZDate)Declaration.SG_RemovalStartDate; }
		}

		#endregion

		#region ITCODEC Members

		ZString ITCODEC.AdditionalInformation
		{
			get { return Declaration.SG_CertAdditionalInformation; }
		}

		ZString ITCODEC.ApplicationProductType
		{
			get { return Declaration.SG_ApplicationProductType; }
		}

		ZString ITCODEC.DonorCountryCode
		{
			get { return Declaration.SG_RN_NKDonorCountry; }
		}

		ZInt ITCODEC.YearOfEntry
		{
			get { return Declaration.SG_EntryYear; }
		}

		ZString ITCODEC.AdditionalDetails1
		{
			get { return Declaration.SG_Cert1AdditionalDetails; }
		}

		ZString ITCODEC.TransportDetails1
		{
			get { return Declaration.SG_Cert1TransportDetails; }
		}

		ZInt ITCODEC.PercCommContent1
		{
			get { return Declaration.SG_Cert1PercCommContent; }
		}

		ZBool ITCODEC.SendInvoiceDetails
		{
			get { return Declaration.SG_CertSendInvDetails; }
		}

		ZInt ITCODEC.NumberOfCopies1
		{
			get { return Declaration.SG_Cert1CopiesNo; }
		}

		ZInt ITCODEC.NumberOfCopies2
		{
			get { return Declaration.SG_Cert2CopiesNo; }
		}

		ZString ITCODEC.CertificateType1
		{
			get { return Declaration.SG_Cert1Type; }
		}

		ZString ITCODEC.CertificateType2
		{
			get { return Declaration.SG_Cert2Type; }
		}

		ZString ITCODEC.CurrencyCode
		{
			get { return Declaration.SG_RX_NKCertReferenceCurrency; }
		}

		IEnumerable<ICusCertItem> ITCODEC.CertItems
		{
			get
			{
				foreach (ICusCertItem cusCertItem in MergedLines)
				{
					yield return cusCertItem;
				}
			}
		}

		#endregion

		#region Generic

		public IAdditionalMessageInformation AdditionalMessageInformation
		{
			get { return Declaration.AdditionalMessageInformation; }
		}

		public ZBool Is2bStoredBWCY => Declaration.PlaceOfStorage.IsBWCY();

		public ZBool Is2bStoredCfw => Declaration.PlaceOfStorage.IsCFW();

		public ZBool Is2bStoredC2y => Declaration.PlaceOfStorage.IsC2Y();

		#endregion

		#endregion

		#region Overrides

		public override ZString CH_Status
		{
			get { return base.CH_Status; }
			set
			{
				if (base.CH_Status != value)
				{
					SG_PreviousEntryStatus = CH_Status;
					base.CH_Status = value;
					if (Declaration != null)
					{
						Declaration.JE_EntryStatus = CH_Status;
						if (CancelledOrOriginalEntryRejected)
						{
							ClearValuationDateForAllInvoices();
						}
					}
				}
			}
		}

		bool CancelledOrOriginalEntryRejected
		{
			get
			{
				return Declaration.JE_EntryStatus == Core.SGConstants.DeclarationStatus.DeclarationHadSyntaxErrors ||
					   Declaration.JE_EntryStatus == Core.SGConstants.DeclarationStatus.DeclarationRejectedByCustoms ||
					   Declaration.JE_EntryStatus == Core.SGConstants.DeclarationStatus.CancellationAccepted;
			}
		}

		void ClearValuationDateForAllInvoices()
		{
			foreach (JobComInvoiceHeader invoiceHeader in Declaration.Invoices)
			{
				invoiceHeader.JZ_ValuationDateOverride = ZDateTime.Empty;
			}
		}

		public override bool HasBeenWithdrawn
		{
			get { return IsCancelledByStatus; }
		}

		protected override bool ShouldBeIncludedInCusEntryNumberFilterCore()
		{
			return !HasBeenWithdrawn;
		}

		[BusinessObjectTestExclude]
		public override ZString CH_BGMReference
		{
			get
			{
				ZString result = "";
				EDIMessage message = Messages.LastOutgoingMessage;
				if (message != null)
				{
					string uEN = GlbCompany.CurrentCompany.GC_CustomsRegistrationNo.Trim();
					result = uEN.PadRight(20, ' ') + message.EM_MessageNum;
				}
				return result;
			}
		}

		protected override ZString EntryNumberType
		{
			get { return "PMT"; }
		}

		protected override CusEntryNumber LoadCusEntryNumber()
		{
			return CusEntryNumber.Load(this, EntryNumberType, Core.Constants.CountryCodes.Singapore);
		}

		public override ZString EntryNumber
		{
			get { return base.EntryNumber; }
			set
			{
				var hasChanges = base.EntryNumber != value;
				base.EntryNumber = value;

				var shipment = Declaration.Shipment;
				if (hasChanges && shipment != null && shipment.JS_TransportMode == TransportTypeList.Codes.Air)
				{
					var cmdPermitQuery = new ZQuery(CusCodeDataSchema.CY_ParentID, shipment.PK);
					cmdPermitQuery.AddToFilter(CusCodeDataSchema.CY_ParentTableCode, shipment.TablePrefix);
					cmdPermitQuery.AddToFilter(CusCodeDataSchema.CY_Type, CusCodeDataTypeList.Codes.CMD);
					cmdPermitQuery.AddToFilter(CusCodeDataSchema.CY_Code, CustomsEntryTypeList.Singapore.Permit);
					cmdPermitQuery.AddToFilter(CusCodeDataSchema.CY_Data, value);
					var cmdPermit = Factory.LoadTop1<CMDPermitNumber>(cmdPermitQuery);
					if (cmdPermit == null)
					{
						cmdPermit = Factory.New<CMDPermitNumber>();
						cmdPermit.CY_ParentID = shipment.PK;
						cmdPermit.CY_ParentTableCode = shipment.TablePrefix;
						cmdPermit.CY_Code = CustomsEntryTypeList.Singapore.Permit;
						cmdPermit.CY_Data = value;
					}
				}
			}
		}

		#region Certificate Number

		public ZString CertificateNumber
		{
			get { return CertificateCusEntryNumber != null ? CertificateCusEntryNumber.CE_EntryNum : ZString.Empty; }
			set
			{
				if (value.IsEmpty)
				{
					if (certificateCusEntryNumber != null)
					{
						certificateCusEntryNumber.Delete();
						certificateCusEntryNumber = null;
					}
				}
				else
				{
					if (CertificateCusEntryNumber == null)
					{
						certificateCusEntryNumber = CreateCertificateCusEntryNumber();
					}

					if (CertificateCusEntryNumber.CE_EntryNum != value)
					{
						CertificateCusEntryNumber.CE_EntryNum = value;
					}
				}
			}
		}

		CusEntryNumber CertificateCusEntryNumber
		{
			get
			{
				if (certificateCusEntryNumber == null)
				{
					certificateCusEntryNumber = TryLoadCertificateCusEntryNumber();
				}
				return certificateCusEntryNumber;
			}
		}
		CusEntryNumber certificateCusEntryNumber;

		CusEntryNumber TryLoadCertificateCusEntryNumber()
		{
			ZQuery query = new ZQuery(CusEntryNumSchema.CE_ParentID, PK);
			query.AddToFilter(CusEntryNumSchema.CE_EntryType, "CER");

			if (Declaration != null && Declaration.Country != null)
			{
				query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Declaration.Country.Code);
			}

			return Factory.LoadTop1<CusEntryNumber>(query);
		}

		CusEntryNumber CreateCertificateCusEntryNumber()
		{
			CusEntryNumber result = Factory.New<CusEntryNumber>();
			result.CE_EntryIsSystemGenerated = true;
			result.CE_ParentID = PK;
			result.CE_ParentTable = TableName;
			result.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			result.CE_EntryType = "CER";
			return result;
		}

		#endregion

		#endregion

		#region ICustomsCharges Members

		public override bool IsFeePaidByBroker(string feeCode, ZString methodOfPayment, ILogger logger)
		{
			return Declaration.JE_PaymentMethod == BGIndicatorCodeList.Codes.D;
		}

		protected override Enterprise.Registry.Business.Customs.EntryChargeTypeList GetEntryChargeTypeList()
		{
			return Declaration.IsImport ? Factory.GetCachedValue<Registry.EntryChargeTypeList>() : Factory.GetCachedValue<Enterprise.Registry.Business.Customs.EmptyEntryChargeTypeList>();
		}

		protected override ZDecimal GetTotalChargeValueFor(Enterprise.Registry.Business.Customs.EntryChargeType chargeTypeElement, ZString methodOfPayment)
		{
			ZDecimal result = base.GetTotalChargeValueFor(chargeTypeElement, methodOfPayment);

			if (chargeTypeElement.Code == Registry.EntryChargeTypeList.Codes.GST && Declaration.SG_SupplyIndicator.IsEmpty)
			{
				if (Declaration.JE_MessageType == MessageTypeCodeList.Codes.IPT && Declaration.JE_MessageSubType == DeclarationTypeCodeList.Codes.BKT)
				{
					foreach (CusEntryLine entryLine in MergedLines)
					{
						if (entryLine.RandomLine.JI_CountryOfOrigin == Core.Constants.CountryCodes.Singapore)
						{
							result -= entryLine.Fees.GetAmount(chargeTypeElement.Code);
						}
					}
				}
			}

			return result;
		}

		#endregion
	}
}
