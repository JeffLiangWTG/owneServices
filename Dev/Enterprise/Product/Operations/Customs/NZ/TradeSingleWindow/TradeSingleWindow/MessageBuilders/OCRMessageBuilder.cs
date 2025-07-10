using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.NZ.MessageDefinitions.TSW.Version1_1.DocumentMetadata;
using CargoWise.Customs.NZ.MessageDefinitions.TSW.Version1_1.OCR.Outgoing;
using CargoWise.Types;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders
{
	/// <summary>
	/// Outward Cargo Report
	/// •	Submitted by:
	///		o	Shipping Companies or their agent
	///	•	Data:
	///		o	Clearance details of the cargo exported in a craft.
	///	•	Used for reconciliation that all cargo carried by a departing craft has export clearance.
	///
	/// </summary>
	public class OCRMessageBuilder : TSWMessageBuilder<Declaration>
	{
		public OCRMessageBuilder(IOutwardCargoReportHeader outwardCargoReportHeader, TSWTransactionTypes transactionType, string submitterCode)
			: base()
		{
			oCRHeader = outwardCargoReportHeader;
			this.transactionType = transactionType;
			this.submitterCode = submitterCode;
		}
		readonly IOutwardCargoReportHeader oCRHeader;
		readonly TSWTransactionTypes transactionType;
		readonly string submitterCode;

		#region Message Declaration Defaults

		public override ZString MessageType
		{
			get { return MessageTypeList.Codes.OCR; }
		}

		protected override WcoDocumentNameCode WCODocumentName
		{
			get { return WcoDocumentNameCode.Cre; }
		}

		protected override NzDocumentNameCode NZDocumentName
		{
			get { return NzDocumentNameCode.Ocr; }
		}

		public override ZBool DeclarantPinRequired
		{
			get { return false; }   // Authentication code is not required for OCR messages
		}

		public override ZString DeclarantPinEncrypted
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region xml OCR Declaration message

		protected override Declaration DeclarationMessage()
		{
			var message = new Declaration();

			if (transactionType != TSWTransactionTypes.Original)
			{
				message.Id = PopulateReferenceNo();
			}

			message.TypeCode = PopulateMessageType();
			message.FunctionalReferenceId = PopulateSendersRef();
			message.FunctionCode = PopulateTransType();
			message.Submitter = PopulateSubmitter();
			message.AdditionalDocument = PopulateAdditionalDocs();
			message.AdditionalInformation = PopulateAdditionalInfo();

			if (transactionType != TSWTransactionTypes.Cancel)
			{
				message.BorderTransportMeans = PopulateBorderTM();
				if (oCRHeader.Carrier != null)
				{
					message.Carrier = PopulateCarrier();
				}

				message.Consignment = PopulateConsignments();
				message.ExitOffice = PopulatePortOfDeparture();
			}

			return message;
		}

		#region Declaration Details

		DeclarationIdentificationIdType PopulateReferenceNo()
		{
			var msgID = new DeclarationIdentificationIdType();
			msgID.Value = oCRHeader.TSWReferenceNumber;
			return msgID;
		}

		DeclarationTypeCodeType PopulateMessageType()
		{
			var msgType = new DeclarationTypeCodeType();
			msgType.Value = MessageType;
			return msgType;
		}

		DeclarationFunctionalReferenceIdType PopulateSendersRef()
		{
			var functionalReferenceID = new DeclarationFunctionalReferenceIdType();
			functionalReferenceID.Value = oCRHeader.SenderReferenceNumber;
			return functionalReferenceID;
		}

		DeclarationSubmitter PopulateSubmitter()
		{
			var submitter = new DeclarationSubmitter();
			submitter.Id = new SubmitterIdentificationIdType();
			submitter.Id.Value = submitterCode;
			return submitter;
		}

		DeclarationFunctionCodeType PopulateTransType()
		{
			var functionCode = new DeclarationFunctionCodeType();
			functionCode.Value = GetTransactionTypeCode(transactionType);
			return functionCode;
		}

		Collection<DeclarationAdditionalDocument> PopulateAdditionalDocs()
		{
			var additionalDocs = new Collection<DeclarationAdditionalDocument>();
			if (oCRHeader.AdditionalInformation != null)
			{
				foreach (var attachment in oCRHeader.AdditionalInformation.SupportingDocuments)
				{
					var aDoc = new DeclarationAdditionalDocument();
					aDoc.CategoryCode = new AdditionalDocumentCategoryCodeType();
					aDoc.CategoryCode.Value = attachment.DocType;
					aDoc.ImageBinaryObject = new AdditionalDocumentImageBinaryObjectType();
					aDoc.ImageBinaryObject.Filename = TSWMessageFormatter.FormatAcceptableFileNameForNZC(attachment.FileName);
					aDoc.ImageBinaryObject.MimeCode = PopulateMimeCode(attachment.FileName, MimeMediaTypeContentType.ApplicationPdf);
					aDoc.ImageBinaryObject.Value = GetATTACHEDAsBytes();
					additionalDocs.Add(aDoc);
				}
			}

			return additionalDocs;
		}

		Collection<DeclarationAdditionalInformation> PopulateAdditionalInfo()
		{
			var decAddInfo = new Collection<DeclarationAdditionalInformation>();
			if (oCRHeader.AdditionalInformation != null)
			{
				if (transactionType == TSWTransactionTypes.Original || transactionType == TSWTransactionTypes.Replace)
				{
					if (!oCRHeader.AdditionalInformation.FreeText.IsEmpty)
					{
						decAddInfo.Add(FreeTextInfo());
					}

					if (oCRHeader.IsConsolidation)
					{
						decAddInfo.Add(ConsolidationInfo());
					}

					if (!oCRHeader.AdditionalInformation.ManualOverrideText.IsEmpty)
					{
						decAddInfo.Add(ManualOverrideInfo());
					}
				}

				if (transactionType == TSWTransactionTypes.Cancel || transactionType == TSWTransactionTypes.Replace)
				{
					decAddInfo.Add(ChangeCancelReason());
				}
			}

			return decAddInfo;
		}

		#region Addtional Information

		DeclarationAdditionalInformation FreeTextInfo()
		{
			var freeTextAddInfo = new DeclarationAdditionalInformation();
			freeTextAddInfo.Content = new AdditionalInformationContentTextType();
			freeTextAddInfo.Content.Value = oCRHeader.AdditionalInformation.FreeText;
			return freeTextAddInfo;
		}

		DeclarationAdditionalInformation ConsolidationInfo()
		{
			var consolidationAddInfo = new DeclarationAdditionalInformation();
			consolidationAddInfo.StatementCode = new AdditionalInformationStatementCodeType();
			consolidationAddInfo.StatementCode.Value = "Y";
			consolidationAddInfo.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
			consolidationAddInfo.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.CON;
			return consolidationAddInfo;
		}

		DeclarationAdditionalInformation ManualOverrideInfo()
		{
			var manualOverrideAddInfo = new DeclarationAdditionalInformation();
			manualOverrideAddInfo.RequestOverrideCode = new AdditionalInformationRequestOverrideCodeType();
			manualOverrideAddInfo.RequestOverrideCode.Value = "Y";
			manualOverrideAddInfo.StatementDescription = new AdditionalInformationStatementDescriptionTextType();
			manualOverrideAddInfo.StatementDescription.Value = oCRHeader.AdditionalInformation.ManualOverrideText; // "Must be present to advise the reason for override or manual processing";
			manualOverrideAddInfo.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
			manualOverrideAddInfo.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.ALP;
			return manualOverrideAddInfo;
		}

		DeclarationAdditionalInformation ChangeCancelReason()
		{
			var ccReasonAddInfo = new DeclarationAdditionalInformation();
			ccReasonAddInfo.StatementDescription = new AdditionalInformationStatementDescriptionTextType();
			ccReasonAddInfo.StatementDescription.Value = oCRHeader.AdditionalInformation.AdditionalStatementText; //"Change/Replace/Cancel reason";
			ccReasonAddInfo.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
			ccReasonAddInfo.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.AES;
			return ccReasonAddInfo;
		}

		#endregion

		DeclarationBorderTransportMeans PopulateBorderTM()
		{
			var boarderTM = new DeclarationBorderTransportMeans();
			boarderTM.Name = new BorderTransportMeansNameTextType();
			boarderTM.Name.Value = oCRHeader.IsSea ? oCRHeader.CraftName.ToUpper() : oCRHeader.FlightNo.ToUpper();
			boarderTM.TypeCode = new BorderTransportMeansTypeCodeType();
			boarderTM.TypeCode.Value = oCRHeader.IsSea ? TransportModeTypeList.Codes.T1 : TransportModeTypeList.Codes.T4;
			boarderTM.DepartureDateTime = new BorderTransportMeansDepartureDateTimeType();
			boarderTM.DepartureDateTime.Value = oCRHeader.DepartureDate.ToString("yyyyMMdd");
			boarderTM.DepartureDateTime.FormatCode = DateTimePeriodFormatCode.Item102;
			if (oCRHeader.IsSea)
			{
				boarderTM.Id = new BorderTransportMeansIdentificationIdType();
				boarderTM.Id.Value = oCRHeader.LloydsNo;
				boarderTM.JourneyId = new BorderTransportMeansJourneyIdType();
				boarderTM.JourneyId.Value = oCRHeader.VoyageNo.ToUpper();
			}

			boarderTM.Itinerary = PopulateRoutingCountries();

			return boarderTM;
		}

		Collection<DeclarationBorderTransportMeansItinerary> PopulateRoutingCountries()
		{
			int routingCountriesSequence = 0;
			var itinerary = new Collection<DeclarationBorderTransportMeansItinerary>();
			foreach (string legCountry in oCRHeader.RoutingCountryCodes)
			{
				routingCountriesSequence++;
				var routing = new DeclarationBorderTransportMeansItinerary();
				routing.RoutingCountryCode = new ItineraryRoutingCountryCodeType();
				routing.RoutingCountryCode.Value = legCountry;
				routing.SequenceNumeric = routingCountriesSequence;
				itinerary.Add(routing);
			}

			return itinerary;
		}

		DeclarationCarrier PopulateCarrier()
		{
			var carrier = new DeclarationCarrier();
			carrier.Name = new CarrierNameTextType();
			carrier.Name.Value = oCRHeader.Carrier.Name;
			var clientCode = oCRHeader.Carrier.CustomsClientCode;
			if (!clientCode.IsEmpty)
			{
				carrier.Id = new CarrierIdentificationIdType();
				carrier.Id.Value = clientCode;
			}

			return carrier;
		}

		DeclarationExitOffice PopulatePortOfDeparture()
		{
			var portOfExit = new DeclarationExitOffice();
			portOfExit.Id = new ExitOfficeIdentificationCodeType();
			portOfExit.Id.Value = oCRHeader.PortOfDeparture;
			return portOfExit;
		}

		#endregion

		#region Consignments

		Collection<DeclarationConsignment> PopulateConsignments()
		{
			int consignmentSequence = 0;
			var consignments = new Collection<DeclarationConsignment>();
			foreach (IOCRConsignment shipment in oCRHeader.OCRLines)
			{
				consignmentSequence++;
				var consignment = new DeclarationConsignment();
				consignment.SequenceNumeric = consignmentSequence;

				if (!shipment.CustomsClearanceNo.IsEmpty)
				{
					consignment.AdditionalDocument = PopulateClearanceNumber(shipment.CustomsClearanceNo);
				}

				consignment.AssociatedTransportDocument = PopulateBillNumber(shipment.BillNumber);

				if (consignmentSequence == 1 && oCRHeader.IsConsolidation)
				{
					var notifyParties = PopulateNotifyParty();
					if (notifyParties.Any())
					{
						consignment.NotifyParty = notifyParties;
					}

					consignment.TransportContractDocument = PopulateTransportContract();
					if (oCRHeader.IsSea)
					{
						consignment.TransportEquipment = PopulateContainers();
					}
				}

				consignments.Add(consignment);
			}

			return consignments;
		}

		#region Populate Consignment Methods

		DeclarationConsignmentAdditionalDocument PopulateClearanceNumber(ZString customsClearanceNo)
		{
			var clearanceNo = new DeclarationConsignmentAdditionalDocument();
			clearanceNo.Id = new AdditionalDocumentIdentificationIdType();
			clearanceNo.Id.Value = customsClearanceNo.KeepNumericCharacters();
			clearanceNo.TypeCode = new AdditionalDocumentTypeCodeType();
			clearanceNo.TypeCode.Value = AdditionalDocumentTypeList.Codes.EDO;

			return clearanceNo;
		}

		DeclarationConsignmentAssociatedTransportDocument PopulateBillNumber(IAssociatedTransportDocument bill)
		{
			var billNumber = new DeclarationConsignmentAssociatedTransportDocument();
			billNumber.Id = new AssociatedTransportDocumentIdentificationIdType();
			billNumber.Id.Value = bill.BillNumber;
			billNumber.TypeCode = new AssociatedTransportDocumentTypeCodeType();
			billNumber.TypeCode.Value = bill.BillType;

			return billNumber;
		}

		Collection<DeclarationConsignmentNotifyParty> PopulateNotifyParty()
		{
			var notifyParties = new Collection<DeclarationConsignmentNotifyParty>();

			foreach (var notifyPartyCode in oCRHeader.NotifyPartyCodes)
			{
				var deliveryNotificationParty = CreateDeliveryNotificationParty();
				deliveryNotificationParty.Id = new NotifyPartyIdentificationIdType();
				deliveryNotificationParty.Id.Value = notifyPartyCode;
				notifyParties.Add(deliveryNotificationParty);
			}

			if (!oCRHeader.NotifyPartyName.IsEmpty)
			{
				var deliveryNotificationParty = CreateDeliveryNotificationParty();
				deliveryNotificationParty.Name = new NotifyPartyNameTextType();
				deliveryNotificationParty.Name.Value = oCRHeader.NotifyPartyName;
				var emailComm = new DeclarationConsignmentNotifyPartyCommunication();
				emailComm.Id = new CommunicationIdentificationIdType();
				emailComm.Id.Value = oCRHeader.NotifyPartyEmail;
				emailComm.TypeId = new CommunicationTypeIdType();
				emailComm.TypeId.Value = CommunicationTypeList.Codes.EM;
				deliveryNotificationParty.Communication = emailComm;
				notifyParties.Add(deliveryNotificationParty);
			}

			return notifyParties;
		}

		DeclarationConsignmentNotifyParty CreateDeliveryNotificationParty()
		{
			var deliveryNotificationParty = new DeclarationConsignmentNotifyParty();
			deliveryNotificationParty.RoleCode = new NotifyPartyRoleCodeType();
			deliveryNotificationParty.RoleCode.Value = RoleCodeList.Codes.N2;
			return deliveryNotificationParty;
		}

		DeclarationConsignmentTransportContractDocument PopulateTransportContract()
		{
			var transportContract = new DeclarationConsignmentTransportContractDocument();
			transportContract.Id = new TransportContractDocumentIdentificationIdType();
			transportContract.Id.Value = oCRHeader.MasterBillNumber;
			transportContract.TypeCode = new TransportContractDocumentTypeCodeType();
			transportContract.TypeCode.Value = BillTypeList.Codes.MB;
			transportContract.Consolidator = new DeclarationConsignmentTransportContractDocumentConsolidator();
			var clientCode = oCRHeader.Consolidator.CustomsClientCode;
			if (clientCode.IsEmpty)
			{
				transportContract.Consolidator.Name = new ConsolidatorNameTextType();
				transportContract.Consolidator.Name.Value = oCRHeader.Consolidator.Name;
			}
			else
			{
				transportContract.Consolidator.Id = new ConsolidatorIdentificationIdType();
				transportContract.Consolidator.Id.Value = clientCode;
			}

			return transportContract;
		}

		Collection<DeclarationConsignmentTransportEquipment> PopulateContainers()
		{
			int containerSequence = 0;
			var containers = new Collection<DeclarationConsignmentTransportEquipment>();
			foreach (ITransportEquipment containerDetails in oCRHeader.Containers)
			{
				containerSequence++;
				var transportEquipment = new DeclarationConsignmentTransportEquipment();
				transportEquipment.SequenceNumeric = containerSequence;
				transportEquipment.Id = new TransportEquipmentIdentificationIdType();
				transportEquipment.Id.Value = containerDetails.ContainerNumber;
				transportEquipment.FullnessCode = new TransportEquipmentFullnessCodeType();
				transportEquipment.FullnessCode.Value = containerDetails.Status;
				containers.Add(transportEquipment);
			}

			return containers;
		}

		#endregion

		#endregion

		#endregion
	}
}
