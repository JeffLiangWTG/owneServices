using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CargoWise.Customs.NZ.MessageDefinitions.TSW.Version1.DocumentMetadata;
using CargoWise.Customs.NZ.MessageDefinitions.TSW.Version1.ICR.Outgoing;
using CargoWise.Types;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders
{
	/// <summary>
	/// Inward Cargo Report
	/// •	Submitted by
	///		o	Carriers, freight-forwarders and brokers
	/// •	Data:
	///		o	Details of cargo imported in a craft including empty containers
	/// •	Used for:
	///		o	Primary risk assessment for Customs / MPI
	///		o	Low value clearance (write-off)
	///		o	To initiate Domestic Transhipment Requests (un-cleared cargo movements)
	///		o	To initiate International Transhipment Requests
	///
	/// </summary>
	public class ICRMessageBuilder : TSWMessageBuilderVersion1<Declaration>
	{
		public ICRMessageBuilder(IInwardCargoReport inwardCargoReportHeader, TSWTransactionTypes transactionType, string submitterCode)
			: base()
		{
			iCRHeader = inwardCargoReportHeader;
			this.transactionType = transactionType;
			this.submitterCode = submitterCode;
		}
		readonly IInwardCargoReport iCRHeader;
		readonly TSWTransactionTypes transactionType;
		readonly string submitterCode;

		#region Message Declaration Defaults

		public override ZString MessageType => MessageTypeList.Codes.ICR;

		protected override WcoDocumentNameCode WCODocumentName => WcoDocumentNameCode.Cri;

		protected override NzDocumentNameCode NZDocumentName => NzDocumentNameCode.Icr;

		// Authentication code is required for ICR messages when requesting write-off. Brokers pin needs to be returned in that case.
		public override ZBool DeclarantPinRequired => iCRHeader.Consignments.Any(x => x.WriteOffRequest);

		public override ZString DeclarantPinEncrypted => iCRHeader.Declarant?.DeclarantPinEncrypted ?? ZString.Empty;

		#endregion

		public IEnumerable<ITSWAttachment> SupportingDocuments()
		{
			foreach (var attachment in iCRHeader.SupportingDocuments)
			{
				yield return attachment;
			}

			if (transactionType == TSWTransactionTypes.Cancel)
			{
				yield break;
			}

			foreach (var consignment in iCRHeader.Consignments)
			{
				foreach (var consignmentAttachment in consignment.SupportingDocuments)
				{
					yield return consignmentAttachment;
				}
			}
		}

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
				if (iCRHeader.Carrier != null)
				{
					message.Carrier = PopulateCarrier();
				}

				message.Consignment = PopulateConsignments();
			}

			if (DeclarantPinRequired)
			{
				message.Declarant = PopulateDeclarant();
			}

			return message;
		}

		#region Declaration Details

		DeclarationIdentificationIdType PopulateReferenceNo()
		{
			var msgID = new DeclarationIdentificationIdType();
			msgID.Value = iCRHeader.TSWReferenceNumber;
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
			functionalReferenceID.Value = iCRHeader.SenderReferenceNumber;
			return functionalReferenceID;
		}

		DeclarationFunctionCodeType PopulateTransType()
		{
			var functionCode = new DeclarationFunctionCodeType();
			functionCode.Value = GetTransactionTypeCode(transactionType);
			return functionCode;
		}

		DeclarationSubmitter PopulateSubmitter()
		{
			var submitter = new DeclarationSubmitter();
			submitter.Id = new SubmitterIdentificationIdType();
			submitter.Id.Value = submitterCode;
			return submitter;
		}

		Collection<DeclarationAdditionalDocument> PopulateAdditionalDocs()
		{
			var additionalDocs = new Collection<DeclarationAdditionalDocument>();

			// Either of iCRHeader.AdditionalInformation.SupportingDocuments or iCRHeader.SupportingDocuments may have values to populate
			if (iCRHeader.AdditionalInformation != null)
			{
				foreach (var attachment in iCRHeader.AdditionalInformation.SupportingDocuments)
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

			foreach (var attachment in iCRHeader.SupportingDocuments)
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
			return additionalDocs;
		}

		Collection<DeclarationAdditionalInformation> PopulateAdditionalInfo()
		{
			var decAddInfo = new Collection<DeclarationAdditionalInformation>();
			var additionalStatement = new DeclarationAdditionalInformation();
			if (transactionType == TSWTransactionTypes.Original || transactionType == TSWTransactionTypes.Replace)
			{
				if (iCRHeader.AdditionalInformation != null)
				{
					// Consolidation
					if (!iCRHeader.AdditionalInformation.FreeText.IsEmpty)
					{
						additionalStatement.Content = new AdditionalInformationContentTextType();
						additionalStatement.Content.Value = iCRHeader.AdditionalInformation.FreeText;
						decAddInfo.Add(additionalStatement);
					}

					//Manual Override
					if (!iCRHeader.AdditionalInformation.ManualOverrideText.IsEmpty)
					{
						var manualOverrideAddInfo = new DeclarationAdditionalInformation();
						manualOverrideAddInfo.RequestOverrideCode = new AdditionalInformationRequestOverrideCodeType();
						manualOverrideAddInfo.RequestOverrideCode.Value = "Y";
						manualOverrideAddInfo.StatementDescription = new AdditionalInformationStatementDescriptionTextType();
						manualOverrideAddInfo.StatementDescription.Value = iCRHeader.AdditionalInformation.ManualOverrideText; // "Must be present to advise the reason for override or manual processing";
						manualOverrideAddInfo.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
						manualOverrideAddInfo.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.ALP;
						decAddInfo.Add(manualOverrideAddInfo);
					}
				}

				if (iCRHeader.IsCarrierCargoReport)
				{
					var carrierCargoAddInfo = new DeclarationAdditionalInformation();
					carrierCargoAddInfo.StatementCode = new AdditionalInformationStatementCodeType();
					carrierCargoAddInfo.StatementCode.Value = "Y";
					carrierCargoAddInfo.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
					carrierCargoAddInfo.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.CCR;
					decAddInfo.Add(carrierCargoAddInfo);
				}

				if (!iCRHeader.MPIAccountDetails.IsEmpty)
				{
					var mpiDetailsInfoAddInfo = new DeclarationAdditionalInformation();
					mpiDetailsInfoAddInfo.StatementDescription = new AdditionalInformationStatementDescriptionTextType();
					mpiDetailsInfoAddInfo.StatementDescription.Value = iCRHeader.MPIAccountDetails;
					mpiDetailsInfoAddInfo.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
					mpiDetailsInfoAddInfo.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.MAC;
					decAddInfo.Add(mpiDetailsInfoAddInfo);
				}
			}

			if (transactionType == TSWTransactionTypes.Cancel || transactionType == TSWTransactionTypes.Replace)
			{
				additionalStatement.StatementDescription = new AdditionalInformationStatementDescriptionTextType();
				additionalStatement.StatementDescription.Value = iCRHeader.AdditionalInformation != null ? iCRHeader.AdditionalInformation.AdditionalStatementText : ZString.Empty; //"Change/Replace/Cancel reason";
				additionalStatement.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
				additionalStatement.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.AES;
				decAddInfo.Add(additionalStatement);
			}

			return decAddInfo;
		}

		DeclarationBorderTransportMeans PopulateBorderTM()
		{
			var boarderTM = new DeclarationBorderTransportMeans();
			boarderTM.Name = new BorderTransportMeansNameTextType();
			boarderTM.Name.Value = iCRHeader.IsSea ? iCRHeader.CraftName.ToUpper() : iCRHeader.FlightNo.ToUpper();
			boarderTM.TypeCode = new BorderTransportMeansTypeCodeType();
			boarderTM.TypeCode.Value = iCRHeader.IsSea ? TransportModeTypeList.Codes.T1 : TransportModeTypeList.Codes.T4;
			boarderTM.ArrivalDateTime = new BorderTransportMeansArrivalDateTimeType();
			boarderTM.ArrivalDateTime.Value = iCRHeader.ArrivalDate.ToString("yyyyMMdd");
			boarderTM.ArrivalDateTime.FormatCode = DateTimePeriodFormatCode.Item102;
			boarderTM.FirstArrivalLocationId = new BorderTransportMeansFirstArrivalLocationIdType();
			boarderTM.FirstArrivalLocationId.Value = iCRHeader.PortOfArrival;
			if (iCRHeader.IsSea)
			{
				boarderTM.Id = new BorderTransportMeansIdentificationIdType();
				boarderTM.Id.Value = iCRHeader.LloydsNo;
				boarderTM.JourneyId = new BorderTransportMeansJourneyIdType();
				boarderTM.JourneyId.Value = iCRHeader.VoyageNo.ToUpper();
			}

			return boarderTM;
		}

		DeclarationCarrier PopulateCarrier()
		{
			var carrier = new DeclarationCarrier();
			var clientCode = iCRHeader.Carrier.CustomsClientCode;
			if (clientCode.IsEmpty)
			{
				carrier.Name = new CarrierNameTextType();
				carrier.Name.Value = iCRHeader.Carrier.Name;
			}
			else
			{
				carrier.Id = new CarrierIdentificationIdType();
				carrier.Id.Value = clientCode;
			}

			return carrier;
		}

		DeclarationDeclarant PopulateDeclarant()
		{
			var declarant = new DeclarationDeclarant();
			declarant.Id = new DeclarantIdentificationIdType();
			declarant.Id.Value = iCRHeader.Declarant != null ? iCRHeader.Declarant.DeclarantID : ZString.Empty;

			if (iCRHeader.Declarant != null)
			{
				var declarantComms = new Collection<DeclarationDeclarantCommunication>();
				foreach (var comms in iCRHeader.Declarant.Communications)
				{
					var declarantCommunication = new DeclarationDeclarantCommunication();
					declarantCommunication.Id = new CommunicationIdentificationIdType();
					declarantCommunication.Id.Value = comms.ContactDetail;
					declarantCommunication.TypeId = new CommunicationTypeIdType();
					declarantCommunication.TypeId.Value = comms.ContactType;
					declarantComms.Add(declarantCommunication);
				}
				declarant.Communication = declarantComms;
			}

			return declarant;
		}

		#endregion

		#region Consignments

		Collection<DeclarationConsignment> PopulateConsignments()
		{
			ZShort consignmentSequence = 0;
			var getSequenceNumeric = iCRHeader.UseInterfaceSequenceNumber ? new Func<IICRConsignment, ZInt>((x) => x.SequenceNumber) : new Func<IICRConsignment, ZInt>((x) => ++consignmentSequence);
			var consignments = new Collection<DeclarationConsignment>();
			foreach (var icrConsignment in iCRHeader.Consignments.OrderBy(c => c.SequenceNumber))
			{
				if (iCRHeader.IsSea && icrConsignment.HasContainers)
				{
					StoreEquipmentPointedTo(icrConsignment.Containers);
					StoreStuffingLocationsPointedTo(icrConsignment);
				}

				var consignment = new DeclarationConsignment
				{
					SequenceNumeric = getSequenceNumeric(icrConsignment),
					ValueAmount = PopulateConsignmentValueAmount(icrConsignment),
					AdditionalDocument = PopulateAdditionalDocument(icrConsignment),
					AdditionalInformation = PopulateAdditionalInformation(icrConsignment),
					AssociatedTransportDocument = PopulateAssociatedTransportDocument(icrConsignment),
					Consignee = PopulateConsignee(icrConsignment),
					ConsignmentItem = PopulateConsignmentItems(icrConsignment),
					Consignor = PopulateConsignor(icrConsignment),
					DeliveryDestination = PopulateDeliveryDestination(icrConsignment),
					Freight = PopulateFreight(icrConsignment),
					GoodsConsignedPlace = PopulateGoodsConsignedPlace(icrConsignment),
					GoodsLocation = PopulateGoodsLocation(icrConsignment),
					LoadingLocation = PopulateLoadingLocation(icrConsignment),
					NotifyParty = PopulateNotifyParty(icrConsignment),
					StuffingEstablishment = PopulateStuffingEstablishments(icrConsignment),
					TranshipmentLocation = PopulateTranshipmentPorts(icrConsignment),
					TransitDestination = PopulateTransitDestination(icrConsignment),
					TransportContractDocument = PopulateTransportContractDocument(icrConsignment),
					TransportEquipment = PopulateTransportEquipment(icrConsignment),
					UnloadingLocation = PopulateUnloadingLocation(icrConsignment)
				};
				consignments.Add(consignment);
			}

			return consignments;
		}

		#endregion

		#region Consignment Details

		ConsignmentValueAmountType PopulateConsignmentValueAmount(IICRConsignment icrConsignment)
		{
			ConsignmentValueAmountType consignmentValueAmount = null;
			var itemValueIsRequired = icrConsignment.ConsignmentValueInNZD > 0 && !icrConsignment.IsLinkEmptyContainer;
			if (itemValueIsRequired)
			{
				var valueInNZD = icrConsignment.ConsignmentValueInNZD.Round(2);
				if (!valueInNZD.IsEmpty || (valueInNZD.IsEmpty && !iCRHeader.IsCarrierCargoReport))
				{
					consignmentValueAmount = new ConsignmentValueAmountType
					{
						Value = valueInNZD,
						CurrencyId = Iso3AlphaCurrencyCodeContentType.Nzd,
					};
				}
			}

			return consignmentValueAmount;
		}

		Collection<DeclarationConsignmentAdditionalDocument> PopulateAdditionalDocument(IICRConsignment icrConsignment)
		{
			var additionalDocuments = new Collection<DeclarationConsignmentAdditionalDocument>();
			if (icrConsignment.SupportingDocuments.Any())
			{
				foreach (var attachment in icrConsignment.SupportingDocuments)
				{
					var additionalDocument = new DeclarationConsignmentAdditionalDocument();
					additionalDocument.CategoryCode = new AdditionalDocumentCategoryCodeType();
					additionalDocument.CategoryCode.Value = attachment.DocType;
					additionalDocument.ImageBinaryObject = new AdditionalDocumentImageBinaryObjectType();
					additionalDocument.ImageBinaryObject.Filename = TSWMessageFormatter.FormatAcceptableFileNameForNZC(attachment.FileName);
					additionalDocument.ImageBinaryObject.MimeCode = PopulateMimeCode(attachment.FileName, MimeMediaTypeContentType.ApplicationPdf);
					additionalDocument.ImageBinaryObject.Value = GetATTACHEDAsBytes();
					additionalDocuments.Add(additionalDocument);
				}
			}
			else
			{
				foreach (var permit in icrConsignment.Permits)
				{
					if (!permit.IsEmpty)
					{
						var additionalDocument = new DeclarationConsignmentAdditionalDocument();
						additionalDocument.Id = new AdditionalDocumentIdentificationIdType();
						additionalDocument.Id.Value = permit;
						additionalDocument.TypeCode = new AdditionalDocumentTypeCodeType();
						additionalDocument.TypeCode.Value = AdditionalDocumentTypeList.Codes.PER;
						additionalDocuments.Add(additionalDocument);
					}
				}
			}
			return additionalDocuments;
		}

		Collection<DeclarationConsignmentAdditionalInformation> PopulateAdditionalInformation(IICRConsignment icrConsignment)
		{
			var additionalInfos = new Collection<DeclarationConsignmentAdditionalInformation>();

			bool isITRConsignment = icrConsignment.TranshipmentDetails?.InternationalTranshipmentRequest ?? false;
			bool isDTRConsignment = icrConsignment.TranshipmentDetails?.DomesticTranshipmentRequest ?? false;

			// WOF = Write-off request indicator
			// CON = Consolidation indicator
			// DTR = Domestic transhipment request indicator
			// ITR = International transhipment request indicator
			// MCD = MPI Container Declaration
			// MAS = MPI Approved System number
			// HAN = Handling Information
			// RPK = Repack indicator
			// MTT = Mode of Transport for Transfer

			if (icrConsignment.WriteOffRequest && (!isITRConsignment))
			{
				var writeOffRequest = new DeclarationConsignmentAdditionalInformation();
				writeOffRequest.StatementCode = new AdditionalInformationStatementCodeType();
				writeOffRequest.StatementCode.Value = "Y";
				writeOffRequest.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
				writeOffRequest.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.WOF;
				additionalInfos.Add(writeOffRequest);
			}

			if (iCRHeader.IsSea && icrConsignment.HasContainers)
			{
				if (icrConsignment.MAFContainerDeclaration)
				{
					foreach (var mafContainerStatement in icrConsignment.MAFContainerStatements)
					{
						var scAddInfo = new DeclarationConsignmentAdditionalInformation();
						scAddInfo.StatementDescription = new AdditionalInformationStatementDescriptionTextType();
						scAddInfo.StatementDescription.Value = mafContainerStatement;
						scAddInfo.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
						scAddInfo.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.MCD;
						additionalInfos.Add(scAddInfo);
					}
				}

				foreach (var mpiApprovedSystemNumber in icrConsignment.MPIApprovedSystemNumbers)
				{
					if (!mpiApprovedSystemNumber.IsEmpty)
					{
						var scAddInfo = new DeclarationConsignmentAdditionalInformation();
						scAddInfo.StatementDescription = new AdditionalInformationStatementDescriptionTextType();
						scAddInfo.StatementDescription.Value = mpiApprovedSystemNumber;
						scAddInfo.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
						scAddInfo.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.MAS;
						additionalInfos.Add(scAddInfo);
					}
				}
			}

			if (!icrConsignment.MPIAccountDetails.IsEmpty)
			{
				var mpiDetailsInfoAddInfo = new DeclarationConsignmentAdditionalInformation();
				mpiDetailsInfoAddInfo.StatementDescription = new AdditionalInformationStatementDescriptionTextType();
				mpiDetailsInfoAddInfo.StatementDescription.Value = icrConsignment.MPIAccountDetails;
				mpiDetailsInfoAddInfo.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
				mpiDetailsInfoAddInfo.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.MAC;
				additionalInfos.Add(mpiDetailsInfoAddInfo);
			}

			if (isITRConsignment)
			{
				var itr = new DeclarationConsignmentAdditionalInformation();
				itr.StatementDescription = new AdditionalInformationStatementDescriptionTextType();
				if (icrConsignment.TranshipmentDetails.ITRImportMode == TransportModeTypeList.Codes.T4)
				{
					itr.StatementDescription.Value = "Y";
				}
				else if (IsSeaTranshipment(icrConsignment.TranshipmentDetails.ITRImportMode))
				{
					ZStringBuilder voyageDetails = new ZStringBuilder();
					if (!icrConsignment.TranshipmentDetails.ITRImportCraft.IsEmpty)
					{
						voyageDetails.Append(icrConsignment.TranshipmentDetails.ITRImportCraft.ToUpper() + ",");
					}

					if (!icrConsignment.TranshipmentDetails.ITRImportMode.IsEmpty)
					{
						voyageDetails.Append(icrConsignment.TranshipmentDetails.ITRImportMode + ",");
					}

					if (!icrConsignment.TranshipmentDetails.ITRDepartureDate.IsEmpty)
					{
						voyageDetails.Append(icrConsignment.TranshipmentDetails.ITRDepartureDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + ",");
					}

					voyageDetails.Append(icrConsignment.TranshipmentDetails.ITRVoyageFlight.ToUpper());
					itr.StatementDescription.Value = voyageDetails.ToString();
				}

				itr.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
				itr.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.ITR;
				additionalInfos.Add(itr);

				additionalInfos.Add(PopulateMTT(icrConsignment));
			}
			else if (isDTRConsignment)
			{
				var dtr = new DeclarationConsignmentAdditionalInformation();
				dtr.StatementCode = new AdditionalInformationStatementCodeType();
				dtr.StatementCode.Value = "Y";
				dtr.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
				dtr.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.DTR;
				additionalInfos.Add(dtr);

				additionalInfos.Add(PopulateMTT(icrConsignment));
			}

			if (!icrConsignment.HandlingInformation.IsEmpty)
			{
				var handlingInfo = new DeclarationConsignmentAdditionalInformation();
				handlingInfo.StatementDescription = new AdditionalInformationStatementDescriptionTextType();
				handlingInfo.StatementDescription.Value = icrConsignment.HandlingInformation;
				handlingInfo.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
				handlingInfo.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.HAN;
				additionalInfos.Add(handlingInfo);
			}

			if (icrConsignment.IsConsolidation)
			{
				var isConsignment = new DeclarationConsignmentAdditionalInformation();
				isConsignment.StatementCode = new AdditionalInformationStatementCodeType();
				isConsignment.StatementCode.Value = "Y";
				isConsignment.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
				isConsignment.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.CON;
				additionalInfos.Add(isConsignment);
			}

			if (!icrConsignment.IsGSTPrePaid.IsEmpty || !icrConsignment.VendorIdentifier.IsEmpty)   // Note: OSR and OSP must be used in conjunction – if one provided but not the other a consignment level error will be given.
			{
				var gstPrepaid = new DeclarationConsignmentAdditionalInformation();
				gstPrepaid.StatementCode = new AdditionalInformationStatementCodeType() { Value = icrConsignment.IsGSTPrePaid };
				gstPrepaid.StatementTypeCode = new AdditionalInformationStatementTypeCodeType() { Value = AdditionalStatementTypeList.Codes.OSP };
				additionalInfos.Add(gstPrepaid);

				var consignorGSTNo = new DeclarationConsignmentAdditionalInformation();
				consignorGSTNo.StatementDescription = new AdditionalInformationStatementDescriptionTextType() { Value = icrConsignment.VendorIdentifier };
				consignorGSTNo.StatementTypeCode = new AdditionalInformationStatementTypeCodeType() { Value = AdditionalStatementTypeList.Codes.OSR };
				additionalInfos.Add(consignorGSTNo);
			}

			PopulateExtraAdditionalInformation(icrConsignment, additionalInfos);

			return additionalInfos;
		}

		void PopulateExtraAdditionalInformation(IICRConsignment consignment, Collection<DeclarationConsignmentAdditionalInformation> additionalInfos)
		{
			var extraInfos = (consignment as IExtraAdditionalInformationParent)?.ExtraAdditionalInformations;

			if (extraInfos != null)
			{
				foreach (var extraInfo in extraInfos.Where(c => !c.Code.IsEmpty || !c.Description.IsEmpty || !c.TypeCode.IsEmpty))
				{
					var additionalInformation = new DeclarationConsignmentAdditionalInformation();

					if (!extraInfo.Code.IsEmpty)
					{
						additionalInformation.StatementCode = new AdditionalInformationStatementCodeType();
						additionalInformation.StatementCode.Value = extraInfo.Code;
					}

					if (!extraInfo.Description.IsEmpty)
					{
						additionalInformation.StatementDescription = new AdditionalInformationStatementDescriptionTextType();
						additionalInformation.StatementDescription.Value = extraInfo.Description;
					}

					if (!extraInfo.TypeCode.IsEmpty)
					{
						additionalInformation.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
						additionalInformation.StatementTypeCode.Value = extraInfo.TypeCode;
					}

					additionalInfos.Add(additionalInformation);
				}
			}
		}

		DeclarationConsignmentAdditionalInformation PopulateMTT(IICRConsignment icrConsignment)
		{
			var mtt = new DeclarationConsignmentAdditionalInformation();
			mtt.StatementCode = new AdditionalInformationStatementCodeType();
			mtt.StatementCode.Value = icrConsignment.TranshipmentDetails.ModeOfTransportForTransfer;
			mtt.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
			mtt.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.MTT;
			return mtt;
		}

		static bool IsSeaTranshipment(string itrMode)
		{
			return itrMode == TransportModeTypeList.Codes.T1 || itrMode == TransportModeTypeList.Codes.T1C || itrMode == TransportModeTypeList.Codes.T1O;
		}

		DeclarationConsignmentAssociatedTransportDocument PopulateAssociatedTransportDocument(IICRConsignment icrConsignment)
		{
			DeclarationConsignmentAssociatedTransportDocument associatedTransportDocument = null;
			var masterBill = icrConsignment.MasterBill;

			if (!masterBill.IsEmpty && !icrConsignment.IsLinkEmptyContainer)
			{
				associatedTransportDocument = new DeclarationConsignmentAssociatedTransportDocument();
				associatedTransportDocument.Id = new AssociatedTransportDocumentIdentificationIdType();
				associatedTransportDocument.Id.Value = masterBill;
				associatedTransportDocument.TypeCode = new AssociatedTransportDocumentTypeCodeType();
				associatedTransportDocument.TypeCode.Value = BillTypeList.Codes.MB;
			}

			return associatedTransportDocument;
		}

		DeclarationConsignmentConsignee PopulateConsignee(IICRConsignment icrConsignment)
		{
			if (icrConsignment.Consignee == null)
			{
				return null;
			}

			var consignee = new DeclarationConsignmentConsignee();
			var clientCode = icrConsignment.Consignee.CustomsClientCode;
			if (!clientCode.IsEmpty)
			{
				consignee.Id = new ConsigneeIdentificationIdType();
				consignee.Id.Value = clientCode;
			}
			else
			{
				consignee.Name = new ConsigneeNameTextType();
				consignee.Name.Value = icrConsignment.Consignee.Name.Left(70);
			}

			consignee.Address = new DeclarationConsignmentConsigneeAddress();
			consignee.Address.CityName = new AddressCityNameTextType();
			consignee.Address.CityName.Value = icrConsignment.Consignee.City.Left(35);
			consignee.Address.CountryCode = new AddressCountryCodeType();
			consignee.Address.CountryCode.Value = icrConsignment.Consignee.CountryCode;
			consignee.Address.CountrySubDivisionName = new AddressCountrySubDivisionNameTextType();
			consignee.Address.CountrySubDivisionName.Value = icrConsignment.Consignee.CountryRegion;
			consignee.Address.Line = new AddressLineTextType();
			consignee.Address.Line.Value = icrConsignment.Consignee.Address;
			consignee.Address.PostcodeId = new AddressPostcodeIdType();
			consignee.Address.PostcodeId.Value = icrConsignment.Consignee.PostCode;
			var consigneeComms = new Collection<DeclarationConsignmentConsigneeCommunication>();
			foreach (var comms in icrConsignment.Consignee.Communications)
			{
				if (!comms.ContactDetail.IsEmpty)
				{
					var consigneeCommunication = new DeclarationConsignmentConsigneeCommunication();
					consigneeCommunication.Id = new CommunicationIdentificationIdType();
					consigneeCommunication.Id.Value = comms.ContactDetail;
					consigneeCommunication.TypeId = new CommunicationTypeIdType();
					consigneeCommunication.TypeId.Value = comms.ContactType;
					consigneeComms.Add(consigneeCommunication);
				}
			}

			consignee.Communication = consigneeComms;

			return consignee;
		}

		DeclarationConsignmentConsignor PopulateConsignor(IICRConsignment icrConsignment)
		{
			if (icrConsignment.Consignor == null)
			{
				return null;
			}

			var consignor = new DeclarationConsignmentConsignor();
			consignor.Name = new ConsignorNameTextType();
			consignor.Name.Value = icrConsignment.Consignor.Name.Left(70);
			consignor.Address = new DeclarationConsignmentConsignorAddress();
			consignor.Address.CityName = new AddressCityNameTextType();
			consignor.Address.CityName.Value = icrConsignment.Consignor.City.Left(35);
			consignor.Address.CountryCode = new AddressCountryCodeType();
			consignor.Address.CountryCode.Value = icrConsignment.Consignor.CountryCode;
			consignor.Address.Line = new AddressLineTextType();
			consignor.Address.Line.Value = icrConsignment.Consignor.Address;
			consignor.Address.PostcodeId = new AddressPostcodeIdType();
			consignor.Address.PostcodeId.Value = icrConsignment.Consignor.PostCode;
			var consignorComms = new Collection<DeclarationConsignmentConsignorCommunication>();
			foreach (var comms in icrConsignment.Consignor.Communications)
			{
				if (!comms.ContactDetail.IsEmpty)
				{
					var consignorCommunication = new DeclarationConsignmentConsignorCommunication();
					consignorCommunication.Id = new CommunicationIdentificationIdType();
					consignorCommunication.Id.Value = comms.ContactDetail;
					consignorCommunication.TypeId = new CommunicationTypeIdType();
					consignorCommunication.TypeId.Value = comms.ContactType;
					consignorComms.Add(consignorCommunication);
				}
			}

			consignor.Communication = consignorComms;

			return consignor;
		}

		DeclarationConsignmentDeliveryDestination PopulateDeliveryDestination(IICRConsignment icrConsignment)
		{
			DeclarationConsignmentDeliveryDestination deliveryDestination = null;
			var deliverToParty = icrConsignment.DeliverToParty;
			if (deliverToParty != null && deliverToParty.Name != icrConsignment.Consignee.Name && !icrConsignment.IsLinkEmptyContainer)
			{
				deliveryDestination = new DeclarationConsignmentDeliveryDestination();
				deliveryDestination.Name = new DeliveryDestinationNameTextType();
				deliveryDestination.Name.Value = deliverToParty.Name;
				deliveryDestination.Address = new DeclarationConsignmentDeliveryDestinationAddress();
				deliveryDestination.Address.CityName = new AddressCityNameTextType();
				deliveryDestination.Address.CityName.Value = deliverToParty.City;
				deliveryDestination.Address.CountryCode = new AddressCountryCodeType();
				deliveryDestination.Address.CountryCode.Value = deliverToParty.CountryCode;
				deliveryDestination.Address.Line = new AddressLineTextType();
				deliveryDestination.Address.Line.Value = deliverToParty.Address;
				deliveryDestination.Address.PostcodeId = new AddressPostcodeIdType();
				deliveryDestination.Address.PostcodeId.Value = deliverToParty.PostCode;
			}

			var atfCode = icrConsignment.ApprovedTransitionalFacilityCode;
			if (!atfCode.IsEmpty && icrConsignment.IsLinkEmptyContainer)
			{
				deliveryDestination = deliveryDestination ?? new DeclarationConsignmentDeliveryDestination();
				deliveryDestination.Id = new DeliveryDestinationIdentificationIdType();
				deliveryDestination.Id.Value = atfCode;
			}

			return deliveryDestination;
		}

		DeclarationConsignmentFreight PopulateFreight(IICRConsignment icrConsignment)
		{
			var freight = new DeclarationConsignmentFreight();
			freight.PaymentMethodCode = new FreightPaymentMethodCodeType();
			freight.PaymentMethodCode.Value = icrConsignment.FreightPaymentMethod;
			return freight;
		}

		DeclarationConsignmentGoodsConsignedPlace PopulateGoodsConsignedPlace(IICRConsignment icrConsignment)
		{
			var goodsConsignedPlace = new DeclarationConsignmentGoodsConsignedPlace();
			goodsConsignedPlace.Id = new GoodsConsignedPlaceIdentificationIdType();
			goodsConsignedPlace.Id.Value = icrConsignment.PortOfOrigin;
			return goodsConsignedPlace;
		}

		DeclarationConsignmentGoodsLocation PopulateGoodsLocation(IICRConsignment icrConsignment)
		{
			DeclarationConsignmentGoodsLocation consignmentGoodsLocation = null;
			var goodsLocation = icrConsignment.GoodsLocation;
			if (!goodsLocation.IsEmpty)
			{
				consignmentGoodsLocation = new DeclarationConsignmentGoodsLocation();
				consignmentGoodsLocation.Id = new GoodsLocationIdentificationIdType();
				consignmentGoodsLocation.Id.Value = goodsLocation;
			}
			return consignmentGoodsLocation;
		}

		DeclarationConsignmentLoadingLocation PopulateLoadingLocation(IICRConsignment icrConsignment)
		{
			var loadingLocation = new DeclarationConsignmentLoadingLocation();
			loadingLocation.Id = new LoadingLocationIdentificationIdType();
			loadingLocation.Id.Value = icrConsignment.PortOfLoading;
			return loadingLocation;
		}

		Collection<DeclarationConsignmentNotifyParty> PopulateNotifyParty(IICRConsignment icrConsignment)
		{
			var notifyPartyList = new Collection<DeclarationConsignmentNotifyParty>();
			if (icrConsignment.NotifyParty != null && !icrConsignment.NotifyParty.Name.IsEmpty)
			{
				var notifyParty = new DeclarationConsignmentNotifyParty();
				if (!icrConsignment.NotifyParty.Name.IsEmpty)
				{
					notifyParty.Name = new NotifyPartyNameTextType();
					notifyParty.Name.Value = icrConsignment.NotifyParty.Name;
				}

				notifyParty.RoleCode = new NotifyPartyRoleCodeType();
				notifyParty.RoleCode.Value = RoleCodeList.Codes.NI;

				if (!icrConsignment.NotifyParty.Name.IsEmpty)
				{
					notifyParty.Address = new DeclarationConsignmentNotifyPartyAddress();
					notifyParty.Address.CityName = new AddressCityNameTextType();
					notifyParty.Address.CityName.Value = icrConsignment.NotifyParty.City;
					notifyParty.Address.CountryCode = new AddressCountryCodeType();
					notifyParty.Address.CountryCode.Value = icrConsignment.NotifyParty.CountryCode;
					notifyParty.Address.Line = new AddressLineTextType();
					notifyParty.Address.Line.Value = icrConsignment.NotifyParty.Address;
					notifyParty.Address.PostcodeId = new AddressPostcodeIdType();
					notifyParty.Address.PostcodeId.Value = icrConsignment.NotifyParty.PostCode;
				}

				notifyPartyList.Add(notifyParty);
			}

			foreach (var deliveryNotifyParty in icrConsignment.DeliveryNotifyParties)
			{
				if (deliveryNotifyParty != null && !deliveryNotifyParty.Name.IsEmpty)
				{
					var deliveryNotificationParty = new DeclarationConsignmentNotifyParty();
					deliveryNotificationParty.Name = new NotifyPartyNameTextType();
					deliveryNotificationParty.Name.Value = deliveryNotifyParty.Name;

					deliveryNotificationParty.RoleCode = new NotifyPartyRoleCodeType();
					deliveryNotificationParty.RoleCode.Value = RoleCodeList.Codes.N2;
					var deliveryNotificationPartyComms = new Collection<DeclarationConsignmentNotifyPartyCommunication>();
					foreach (var contact in deliveryNotifyParty.Contacts)
					{
						foreach (var comms in contact.Communications)
						{
							if (!comms.ContactDetail.IsEmpty)
							{
								if (comms.ContactType == CommunicationTypeList.Codes.EM)
								{
									var emailComm = new DeclarationConsignmentNotifyPartyCommunication();
									emailComm.Id = new CommunicationIdentificationIdType();
									emailComm.Id.Value = comms.ContactDetail;
									emailComm.TypeId = new CommunicationTypeIdType();
									emailComm.TypeId.Value = CommunicationTypeList.Codes.EM;
									deliveryNotificationPartyComms.Add(emailComm);
									break;
								}
								break;
							}
						}
					}

					deliveryNotificationParty.Communication = deliveryNotificationPartyComms;
					notifyPartyList.Add(deliveryNotificationParty);
				}
			}
			foreach (var notifyPartyCode in icrConsignment.NotifyPartyCodes.Where(x => !x.IsEmpty))
			{
				notifyPartyList.Add(new DeclarationConsignmentNotifyParty
				{
					Id = new NotifyPartyIdentificationIdType
					{
						Value = notifyPartyCode
					},
					RoleCode = new NotifyPartyRoleCodeType
					{
						Value = RoleCodeList.Codes.N2
					}
				});
			}
			return notifyPartyList;
		}

		Collection<DeclarationConsignmentStuffingEstablishment> PopulateStuffingEstablishments(IICRConsignment icrConsignment)
		{
			Collection<DeclarationConsignmentStuffingEstablishment> stuffingEstablishments = null;
			if (iCRHeader.IsSea && icrConsignment.HasContainers)
			{
				stuffingEstablishments = new Collection<DeclarationConsignmentStuffingEstablishment>();
				foreach (var shipmentContainerPackLocation in icrConsignment.ContainerPackingLocations)
				{
					var containerPackLocation = new DeclarationConsignmentStuffingEstablishment();
					containerPackLocation.Name = new StuffingEstablishmentNameTextType();
					containerPackLocation.Name.Value = shipmentContainerPackLocation.Name;
					containerPackLocation.Address = new DeclarationConsignmentStuffingEstablishmentAddress();
					containerPackLocation.Address.CityName = new AddressCityNameTextType();
					containerPackLocation.Address.CityName.Value = shipmentContainerPackLocation.City;
					containerPackLocation.Address.CountryCode = new AddressCountryCodeType();
					containerPackLocation.Address.CountryCode.Value = shipmentContainerPackLocation.CountryCode;
					containerPackLocation.Address.Line = new AddressLineTextType();
					containerPackLocation.Address.Line.Value = shipmentContainerPackLocation.Address;
					containerPackLocation.Address.PostcodeId = new AddressPostcodeIdType();
					containerPackLocation.Address.PostcodeId.Value = shipmentContainerPackLocation.PostCode;
					stuffingEstablishments.Add(containerPackLocation);
				}
			}
			return stuffingEstablishments;
		}

		Collection<DeclarationConsignmentTranshipmentLocation> PopulateTranshipmentPorts(IICRConsignment icrConsignment)
		{
			var transhipmentPorts = new Collection<DeclarationConsignmentTranshipmentLocation>();
			foreach (var transportPort in icrConsignment.TranshipmentPorts)
			{
				if (!transportPort.IsEmpty)
				{
					var transhipmentPort = new DeclarationConsignmentTranshipmentLocation();
					transhipmentPort.Id = new TranshipmentLocationIdentificationIdType();
					transhipmentPort.Id.Value = transportPort;
					transhipmentPorts.Add(transhipmentPort);
				}
			}

			return transhipmentPorts;
		}

		DeclarationConsignmentTransitDestination PopulateTransitDestination(IICRConsignment icrConsignment)
		{
			DeclarationConsignmentTransitDestination transitDestination = null;
			bool isITRConsignment = icrConsignment.TranshipmentDetails?.InternationalTranshipmentRequest ?? false;
			bool isDTRConsignment = icrConsignment.TranshipmentDetails?.DomesticTranshipmentRequest ?? false;
			if (isITRConsignment || isDTRConsignment)
			{
				var premiseCode = icrConsignment.TranshipmentDetails.PremiseCode;
				if (!premiseCode.IsEmpty)
				{
					transitDestination = new DeclarationConsignmentTransitDestination();
					transitDestination.Id = new TransitDestinationIdentificationIdType();
					transitDestination.Id.Value = premiseCode;
				}
			}

			return transitDestination;
		}

		DeclarationConsignmentTransportContractDocument PopulateTransportContractDocument(IICRConsignment icrConsignment)
		{
			var transportContractDocument = new DeclarationConsignmentTransportContractDocument();

			transportContractDocument.Id = new TransportContractDocumentIdentificationIdType();
			transportContractDocument.Id.Value = icrConsignment.BillNumber;
			transportContractDocument.TypeCode = new TransportContractDocumentTypeCodeType();
			transportContractDocument.TypeCode.Value = icrConsignment.BillType;

			if (icrConsignment.Deconsolidator != null)
			{
				transportContractDocument.Deconsolidator = new DeclarationConsignmentTransportContractDocumentDeconsolidator();
				var clientCode = icrConsignment.Deconsolidator.CustomsClientCode;
				if (!clientCode.IsEmpty)
				{
					transportContractDocument.Deconsolidator.Id = new DeconsolidatorIdentificationIdType();
					transportContractDocument.Deconsolidator.Id.Value = clientCode;
				}
				else
				{
					transportContractDocument.Deconsolidator.Name = new DeconsolidatorNameTextType();
					transportContractDocument.Deconsolidator.Name.Value = icrConsignment.Deconsolidator.Name;
				}
			}

			return transportContractDocument;
		}

		Collection<DeclarationConsignmentTransportEquipment> PopulateTransportEquipment(IICRConsignment icrConsignment)
		{
			Collection<DeclarationConsignmentTransportEquipment> consignmentTransportEquipment = null;
			if (iCRHeader.IsSea && icrConsignment.HasContainers)
			{
				consignmentTransportEquipment = new Collection<DeclarationConsignmentTransportEquipment>();
				var containers = new List<ITransportEquipment>(ContainersPointedTo.Values);
				var orderedContainers = containers.OrderBy(x => x.MessageSequence);
				var sequenceNumeric = 0;

				foreach (var containerDetails in orderedContainers)
				{
					if (containerDetails.ContainerMode != ContainerModeList.Codes.ROR)
					{
						var transportEquipment = new DeclarationConsignmentTransportEquipment();
						transportEquipment.SequenceNumeric = ++sequenceNumeric;
						transportEquipment.CharacteristicCode = new TransportEquipmentCharacteristicCodeType();
						transportEquipment.CharacteristicCode.Value = containerDetails.Size;
						transportEquipment.Id = new TransportEquipmentIdentificationIdType();
						transportEquipment.Id.Value = containerDetails.ContainerNumber;
						transportEquipment.FullnessCode = new TransportEquipmentFullnessCodeType();
						transportEquipment.FullnessCode.Value = containerDetails.Status;
						if (!containerDetails.AttachedEquipmentCode.IsEmpty)
						{
							transportEquipment.AttachedCode = new TransportEquipmentAttachedCodeType();
							transportEquipment.AttachedCode.Value = containerDetails.AttachedEquipmentCode;
						}

						int stuffingLocationSequence = GetStuffingLocationSequence(containerDetails.PK);
						if (stuffingLocationSequence > 0)
						{
							transportEquipment.Pointer = PopulateEquipmentPointers(stuffingLocationSequence);
						}

						if (!containerDetails.StowPosition.IsEmpty)
						{
							transportEquipment.StowPosition = new DeclarationConsignmentTransportEquipmentStowPosition();
							transportEquipment.StowPosition.Id = new StowPositionIdentificationIdType();
							transportEquipment.StowPosition.Id.Value = containerDetails.StowPosition;
						}

						int sealNumberCount = 0;
						var sealNumbers = new Collection<DeclarationConsignmentTransportEquipmentSeal>();
						foreach (var seal in containerDetails.SealNumbers)
						{
							sealNumberCount++;
							var sealNumber = new DeclarationConsignmentTransportEquipmentSeal();
							sealNumber.SequenceNumeric = sealNumberCount;
							sealNumber.Id = new SealIdentificationIdType();
							sealNumber.Id.Value = seal;
							sealNumbers.Add(sealNumber);
						}
						transportEquipment.Seal = sealNumbers;

						if (!containerDetails.StowPosition.IsEmpty)
						{
							transportEquipment.StowPosition = new DeclarationConsignmentTransportEquipmentStowPosition();
							transportEquipment.StowPosition.Id = new StowPositionIdentificationIdType();
							transportEquipment.StowPosition.Id.Value = containerDetails.StowPosition;
						}

						consignmentTransportEquipment.Add(transportEquipment);
					}
				}
			}

			return consignmentTransportEquipment;
		}

		Collection<DeclarationConsignmentTransportEquipmentPointer> PopulateEquipmentPointers(int stuffingLocationSequence)
		{
			var equipmentPointers = new Collection<DeclarationConsignmentTransportEquipmentPointer>();
			var stuffingEstablishmentDecPointer = new DeclarationConsignmentTransportEquipmentPointer();
			stuffingEstablishmentDecPointer.DocumentSectionCode = new PointerDocumentSectionCodeType();
			stuffingEstablishmentDecPointer.DocumentSectionCode.Value = "42A";
			equipmentPointers.Add(stuffingEstablishmentDecPointer);

			var consignmentPointer = new DeclarationConsignmentTransportEquipmentPointer();
			consignmentPointer.DocumentSectionCode = new PointerDocumentSectionCodeType();
			consignmentPointer.DocumentSectionCode.Value = "28A";
			equipmentPointers.Add(consignmentPointer);

			var stuffingEstablishmentPointer = new DeclarationConsignmentTransportEquipmentPointer();
			stuffingEstablishmentPointer.SequenceNumeric = stuffingLocationSequence;
			stuffingEstablishmentPointer.DocumentSectionCode = new PointerDocumentSectionCodeType();
			stuffingEstablishmentPointer.DocumentSectionCode.Value = "16B";
			equipmentPointers.Add(stuffingEstablishmentPointer);

			return equipmentPointers;
		}

		DeclarationConsignmentUnloadingLocation PopulateUnloadingLocation(IICRConsignment icrConsignment)
		{
			var unloadingLocation = new DeclarationConsignmentUnloadingLocation();
			unloadingLocation.Id = new UnloadingLocationIdentificationIdType();
			unloadingLocation.Id.Value = icrConsignment.PortOfDischarge;
			unloadingLocation.ArrivalDateTime = new UnloadingLocationArrivalDateTimeType();
			unloadingLocation.ArrivalDateTime.Value = iCRHeader.ArrivalDate.ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture);
			unloadingLocation.ArrivalDateTime.FormatCode = DateTimePeriodFormatCode.Item203;
			return unloadingLocation;
		}

		#endregion

		#region Consignment Items

		Collection<DeclarationConsignmentConsignmentItem> PopulateConsignmentItems(IICRConsignment icrConsignment)
		{
			int itemSequence = 0;
			var items = new Collection<DeclarationConsignmentConsignmentItem>();
			foreach (var icrConsignmentItem in icrConsignment.ConsignmentItems.OrderBy(c => c.SequenceNumber))
			{
				itemSequence++;
				var item = new DeclarationConsignmentConsignmentItem();
				item.SequenceNumeric = itemSequence;
				item.AdditionalInformation = PopulateItemAdditionalInformation(icrConsignmentItem);
				item.Commodity = PopulateCommodity(icrConsignmentItem);
				item.Commodity.Classification = PopulateClassifications(icrConsignmentItem);
				item.Commodity.Temperature = PopulateItemCommodityTemperature(icrConsignmentItem);
				if (!icrConsignmentItem.IsEmptyContainer)
				{
					item.GoodsMeasure = PopulateGoodsMeasure(icrConsignmentItem);
					item.Origin = PopulateItemOrigin(icrConsignmentItem);
					item.Packaging = PopulateItemPackaging(icrConsignmentItem);
				}

				if (iCRHeader.IsSea && !icrConsignmentItem.ContainerNumber.IsEmpty)
				{
					var container = icrConsignment.Containers.SingleOrDefault(x => x.ContainerNumber == icrConsignmentItem.ContainerNumber);
					if (container != null && container.ContainerMode != ContainerModeList.Codes.ROR)
					{
						item.TransportEquipment = PopulateItemTransportEquipment(icrConsignmentItem);
					}
				}

				items.Add(item);
			}

			return items;
		}

		#region ConsignmentItem Details

		Collection<DeclarationConsignmentConsignmentItemAdditionalInformation> PopulateItemAdditionalInformation(IICRConsignmentItem icrConsignmentItem)
		{
			var additionalInfos = new Collection<DeclarationConsignmentConsignmentItemAdditionalInformation>();
			if (!icrConsignmentItem.MPIApprovedSystemNumber.IsEmpty)
			{
				var mpiDetailsInfoAddInfo = new DeclarationConsignmentConsignmentItemAdditionalInformation();
				mpiDetailsInfoAddInfo.StatementCode = new AdditionalInformationStatementCodeType();
				mpiDetailsInfoAddInfo.StatementCode.Value = icrConsignmentItem.MPIApprovedSystemNumber;
				mpiDetailsInfoAddInfo.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
				mpiDetailsInfoAddInfo.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.MAS;
				additionalInfos.Add(mpiDetailsInfoAddInfo);
			}

			var extraInfos = (icrConsignmentItem as IExtraAdditionalInformationParent)?.ExtraAdditionalInformations;
			if (extraInfos != null)
			{
				foreach (var extraInfo in extraInfos.Where(c => !c.Code.IsEmpty || !c.Description.IsEmpty || !c.TypeCode.IsEmpty))
				{
					var extraInformation = new DeclarationConsignmentConsignmentItemAdditionalInformation();
					if (!extraInfo.Code.IsEmpty)
					{
						extraInformation.StatementCode = new AdditionalInformationStatementCodeType();
						extraInformation.StatementCode.Value = extraInfo.Code;
					}

					if (!extraInfo.Description.IsEmpty)
					{
						extraInformation.StatementDescription = new AdditionalInformationStatementDescriptionTextType();
						extraInformation.StatementDescription.Value = extraInfo.Description;
					}

					if (!extraInfo.TypeCode.IsEmpty)
					{
						extraInformation.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
						extraInformation.StatementTypeCode.Value = extraInfo.TypeCode;
					}

					additionalInfos.Add(extraInformation);
				}
			}

			return additionalInfos;
		}

		DeclarationConsignmentConsignmentItemCommodity PopulateCommodity(IICRConsignmentItem icrConsignmentItem)
		{
			var commodity = new DeclarationConsignmentConsignmentItemCommodity
			{
				CargoDescription = PopulateCargoDescription(icrConsignmentItem),
				ValueAmount = PopulateCommodityValueAmount(icrConsignmentItem),
				Classification = PopulateClassifications(icrConsignmentItem),
				CommercialCategorizationId = PopulateCommercialCategorizationID(icrConsignmentItem),
				IdentityQualifierCode = PoplulateIdentityQualifierCode(icrConsignmentItem)
			};
			return commodity;
		}

		CommodityCargoDescriptionTextType PopulateCargoDescription(IICRConsignmentItem icrConsignmentItem)
		{
			return new CommodityCargoDescriptionTextType
			{
				Value = icrConsignmentItem.GoodsDescription.KeepChars(ZString.AlphanumericCharacters + " ", "")
			};
		}

		CommodityIdentityQualifierCodeType PoplulateIdentityQualifierCode(IICRConsignmentItem icrConsignmentItem)
		{
			CommodityIdentityQualifierCodeType commodityIdentityQualifierCode = null;
			if (!icrConsignmentItem.IdentityNumber.IsEmpty)
			{
				commodityIdentityQualifierCode = new CommodityIdentityQualifierCodeType
				{
					Value = icrConsignmentItem.IdentityType
				};
			}
			return commodityIdentityQualifierCode;
		}

		CommodityCommercialCategorizationIdType PopulateCommercialCategorizationID(IICRConsignmentItem icrConsignmentItem)
		{
			CommodityCommercialCategorizationIdType commodityCommercialCategorizationID = null;
			var identityNumber = icrConsignmentItem.IdentityNumber;
			if (!identityNumber.IsEmpty)
			{
				commodityCommercialCategorizationID = new CommodityCommercialCategorizationIdType
				{
					Value = icrConsignmentItem.IdentityNumber
				};
			}
			return commodityCommercialCategorizationID;
		}

		CommodityValueAmountType PopulateCommodityValueAmount(IICRConsignmentItem icrConsignmentItem)
		{
			CommodityValueAmountType commodityValueAmount = null;
			var itemValueIsRequired = icrConsignmentItem.Value > 0;
			if (itemValueIsRequired)
			{
				var currency = icrConsignmentItem.Currency;
				var consignmentValue = icrConsignmentItem.Value.Round(2);
				if ((!currency.IsEmpty && !iCRHeader.IsCarrierCargoReport) || (!consignmentValue.IsEmpty && !currency.IsEmpty && iCRHeader.IsCarrierCargoReport))
				{
					commodityValueAmount = new CommodityValueAmountType
					{
						Value = consignmentValue,
						CurrencyId = CurrencyID<Iso3AlphaCurrencyCodeContentType>(currency),
					};
				}
			}

			return commodityValueAmount;
		}

		bool HasDangerousGoods(IICRConsignmentItem icrConsignmentItem)
		{
			var result = false;
			foreach (var commodityClassification in icrConsignmentItem.Classifications)
			{
				if (commodityClassification != null)
				{
					if (commodityClassification.ClassificationTypeCode == ClassificationTypeList.Codes.SSO)
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		Collection<DeclarationConsignmentConsignmentItemCommodityClassification> PopulateClassifications(IICRConsignmentItem icrConsignmentItem)
		{
			var classifications = new Collection<DeclarationConsignmentConsignmentItemCommodityClassification>();
			foreach (var commodityClassification in icrConsignmentItem.Classifications)
			{
				if (commodityClassification != null)
				{
					var classification = new DeclarationConsignmentConsignmentItemCommodityClassification();
					classification.Id = new ClassificationIdentificationIdType();
					classification.Id.Value = commodityClassification.Classification;
					classification.IdentificationTypeCode = new ClassificationIdentificationTypeCodeType();
					classification.IdentificationTypeCode.Value = commodityClassification.ClassificationTypeCode;
					classifications.Add(classification);
				}
			}
			return classifications;
		}

		DeclarationConsignmentConsignmentItemCommodityTemperature PopulateItemCommodityTemperature(IICRConsignmentItem icrConsignmentItem)
		{
			DeclarationConsignmentConsignmentItemCommodityTemperature commodityTemperature = null;
			if (icrConsignmentItem.SendFlashpointTemp && HasDangerousGoods(icrConsignmentItem))
			{
				commodityTemperature = new DeclarationConsignmentConsignmentItemCommodityTemperature();
				commodityTemperature.FlashpointMeasure = new TemperatureFlashpointMeasureType();
				commodityTemperature.FlashpointMeasure.Value = icrConsignmentItem.FlashpointTempInCelsius.Round(0);
				commodityTemperature.FlashpointMeasure.UnitCode = MeasurementUnitCommonCodeContentType.Cel;
			}

			var temperatures = icrConsignmentItem.Temperatures;
			if (temperatures != null)
			{
				if (commodityTemperature == null)
				{
					commodityTemperature = new DeclarationConsignmentConsignmentItemCommodityTemperature();
				}
				commodityTemperature.StorageRequirementMeasure = new TemperatureStorageRequirementMeasureType();
				commodityTemperature.StorageRequirementMeasure.Value = temperatures.StorageTemp;
				commodityTemperature.StorageRequirementMeasure.UnitCode = MeasurementUnitCommonCodeContentType.Cel;
				commodityTemperature.MinimumStorageRequirementMeasure = new TemperatureMinimumStorageRequirementMeasureType();
				commodityTemperature.MinimumStorageRequirementMeasure.Value = temperatures.MinStorageTemp;
				commodityTemperature.MinimumStorageRequirementMeasure.UnitCode = MeasurementUnitCommonCodeContentType.Cel;
				commodityTemperature.MaximumStorageRequirementMeasure = new TemperatureMaximumStorageRequirementMeasureType();
				commodityTemperature.MaximumStorageRequirementMeasure.Value = temperatures.MaxStorageTemp;
				commodityTemperature.MaximumStorageRequirementMeasure.UnitCode = MeasurementUnitCommonCodeContentType.Cel;
			}

			return commodityTemperature;
		}

		DeclarationConsignmentConsignmentItemGoodsMeasure PopulateGoodsMeasure(IICRConsignmentItem icrConsignmentItem)
		{
			var goodsMeasure = new DeclarationConsignmentConsignmentItemGoodsMeasure();
			goodsMeasure.GrossMassMeasure = new GoodsMeasureGrossMassMeasureType();
			goodsMeasure.GrossMassMeasure.Value = icrConsignmentItem.GrossWeightInKg.Round(3);
			goodsMeasure.GrossMassMeasure.UnitCode = MeasurementUnitCommonCodeContentType.Kgm;
			return goodsMeasure;
		}

		DeclarationConsignmentConsignmentItemOrigin PopulateItemOrigin(IICRConsignmentItem icrConsignmentItem)
		{
			if (icrConsignmentItem.GoodsOriginCountry.IsEmpty)
			{
				return null;
			}
			var itemOrigin = new DeclarationConsignmentConsignmentItemOrigin();
			itemOrigin.CountryCode = new OriginCountryCodeType();
			itemOrigin.CountryCode.Value = icrConsignmentItem.GoodsOriginCountry;
			return itemOrigin;
		}

		DeclarationConsignmentConsignmentItemPackaging PopulateItemPackaging(IICRConsignmentItem icrConsignmentItem)
		{
			var itemPackaging = new DeclarationConsignmentConsignmentItemPackaging();
			itemPackaging.SequenceNumeric = 1m;
			itemPackaging.QuantityQuantity = new PackagingQuantityQuantityType();
			itemPackaging.QuantityQuantity.Value = icrConsignmentItem.PackageQty;
			itemPackaging.TypeCode = new PackagingTypeCodeType();
			itemPackaging.TypeCode.Value = icrConsignmentItem.PackageType;
			return itemPackaging;
		}

		DeclarationConsignmentConsignmentItemTransportEquipment PopulateItemTransportEquipment(IICRConsignmentItem icrConsignmentItem)
		{
			var itemTransportEquipment = new DeclarationConsignmentConsignmentItemTransportEquipment();
			itemTransportEquipment.Id = new TransportEquipmentIdentificationIdType();
			itemTransportEquipment.Id.Value = icrConsignmentItem.ContainerNumber;
			return itemTransportEquipment;
		}

		#endregion

		#endregion
	}
}
