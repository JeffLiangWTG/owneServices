using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.NZ.MessageDefinitions.TSW.Version1_1.CRE.Outgoing;
using CargoWise.Customs.NZ.MessageDefinitions.TSW.Version1_1.DocumentMetadata;
using CargoWise.Types;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders
{
	/// <summary>
	/// Cargo Report Export
	///	•	Submitted by
	///		o	Carriers and freight-forwarders
	///
	///	•	Data:
	///		o	Details of cargo (parties, commodities and locations)
	///
	///	•	Used to effect low-value clearance (write-off) of export cargo and empty containers
	/// </summary>
	public class CREMessageBuilder : TSWMessageBuilder<Declaration>
	{
		public CREMessageBuilder(ICargoReportExport declarationHeader, TSWTransactionTypes transactionType, string submitterCode)
			: base()
		{
			cREHeader = declarationHeader;
			this.transactionType = transactionType;
			this.submitterCode = submitterCode;
		}
		readonly ICargoReportExport cREHeader;
		readonly TSWTransactionTypes transactionType;
		readonly string submitterCode;

		#region Message Declaration Defaults

		public override ZString MessageType => MessageTypeList.Codes.CRE;

		protected override WcoDocumentNameCode WCODocumentName => WcoDocumentNameCode.Cre;

		protected override NzDocumentNameCode NZDocumentName => NzDocumentNameCode.Cre;

		// Authentication code is required for CRE messages when requesting write-off. Brokers pin needs to be returned in that case.
		// CRE for Empty Containers - note that as you do not have any write-off requests (empty containers only) you do not need the declarant and pin.
		public override ZBool DeclarantPinRequired => !cREHeader.HasEmptyContainersOnly && cREHeader.Consignments.Any(x => x.WriteOffRequest);

		public override ZString DeclarantPinEncrypted => cREHeader.Declarant?.DeclarantPinEncrypted ?? ZString.Empty;

		#endregion

		#region xml CRE Declaration message

		public IEnumerable<ITSWAttachment> SupportingDocuments()
		{
			foreach (var attachment in cREHeader.SupportingDocuments)
			{
				yield return attachment;
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
				if (cREHeader.Carrier != null)
				{
					message.Carrier = PopulateCarrier();
				}

				message.Consignment = PopulateConsignments();
			}

			if (DeclarantPinRequired)
			{
				message.Declarant = PopulateDeclarant();
			}

			message.ExitOffice = PopulatePortOfDeparture();

			return message;
		}

		#region Declaration Details

		DeclarationIdentificationIdType PopulateReferenceNo()
		{
			var msgID = new DeclarationIdentificationIdType();
			msgID.Value = cREHeader.TSWReferenceNumber;
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
			functionalReferenceID.Value = cREHeader.SenderReferenceNumber;
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

			// Either of cREHeader.AdditionalInformation.SupportingDocuments or cREHeader.SupportingDocuments may has values to populate
			if (cREHeader.AdditionalInformation != null)
			{
				foreach (var attachment in cREHeader.AdditionalInformation.SupportingDocuments)
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

			foreach (var attachment in cREHeader.SupportingDocuments)
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
			if (cREHeader.AdditionalInformation != null)
			{
				if (transactionType == TSWTransactionTypes.Original || transactionType == TSWTransactionTypes.Replace)
				{
					if (!cREHeader.AdditionalInformation.FreeText.IsEmpty)
					{
						additionalStatement.Content = new AdditionalInformationContentTextType();
						additionalStatement.Content.Value = cREHeader.AdditionalInformation.FreeText;
						decAddInfo.Add(additionalStatement);
					}

					if (!cREHeader.AdditionalInformation.ManualOverrideText.IsEmpty)
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

		DeclarationAdditionalInformation ManualOverrideInfo()
		{
			var manualOverrideAddInfo = new DeclarationAdditionalInformation();
			manualOverrideAddInfo.RequestOverrideCode = new AdditionalInformationRequestOverrideCodeType();
			manualOverrideAddInfo.RequestOverrideCode.Value = "Y";
			manualOverrideAddInfo.StatementDescription = new AdditionalInformationStatementDescriptionTextType();
			manualOverrideAddInfo.StatementDescription.Value = cREHeader.AdditionalInformation.ManualOverrideText; // "Must be present to advise the reason for override or manual processing";
			manualOverrideAddInfo.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
			manualOverrideAddInfo.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.ALP;
			return manualOverrideAddInfo;
		}

		DeclarationAdditionalInformation ChangeCancelReason()
		{
			var ccReasonAddInfo = new DeclarationAdditionalInformation();
			ccReasonAddInfo.StatementDescription = new AdditionalInformationStatementDescriptionTextType();
			ccReasonAddInfo.StatementDescription.Value = cREHeader.AdditionalInformation.AdditionalStatementText; //"Change/Replace/Cancel reason";
			ccReasonAddInfo.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
			ccReasonAddInfo.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.AES;
			return ccReasonAddInfo;
		}

		#endregion

		DeclarationBorderTransportMeans PopulateBorderTM()
		{
			var boarderTM = new DeclarationBorderTransportMeans();
			boarderTM.Name = new BorderTransportMeansNameTextType();
			boarderTM.Name.Value = cREHeader.IsSea ? cREHeader.CraftName.ToUpper() : cREHeader.FlightNo.ToUpper();
			boarderTM.TypeCode = new BorderTransportMeansTypeCodeType();
			boarderTM.DepartureDateTime = new BorderTransportMeansDepartureDateTimeType();
			boarderTM.DepartureDateTime.Value = cREHeader.DepartureDate.ToString("yyyyMMdd");
			boarderTM.DepartureDateTime.FormatCode = DateTimePeriodFormatCode.Item102;
			if (cREHeader.IsSea)
			{
				boarderTM.TypeCode.Value = TransportModeTypeList.Codes.T1;
				boarderTM.Id = new BorderTransportMeansIdentificationIdType();
				boarderTM.Id.Value = cREHeader.LloydsNo;
				boarderTM.JourneyId = new BorderTransportMeansJourneyIdType();
				boarderTM.JourneyId.Value = cREHeader.VoyageNo.ToUpper();
			}
			else if (cREHeader.IsAir)
			{
				boarderTM.TypeCode.Value = TransportModeTypeList.Codes.T4;
			}
			else if (cREHeader.IsMail)
			{
				boarderTM.TypeCode.Value = TransportModeTypeList.Codes.T5;
			}

			return boarderTM;
		}

		DeclarationCarrier PopulateCarrier()
		{
			var carrier = new DeclarationCarrier();
			carrier.Name = new CarrierNameTextType();
			carrier.Name.Value = cREHeader.Carrier.Name;
			var clientCode = cREHeader.Carrier.CustomsClientCode;
			if (!clientCode.IsEmpty)
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
			declarant.Id.Value = cREHeader.Declarant != null ? cREHeader.Declarant.DeclarantID : ZString.Empty;

			if (cREHeader.Declarant != null)
			{
				var declarantComms = new Collection<DeclarationDeclarantCommunication>();
				foreach (var comms in cREHeader.Declarant.Communications)
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

		DeclarationExitOffice PopulatePortOfDeparture()
		{
			var portOfDeparture = new DeclarationExitOffice();
			portOfDeparture.Id = new ExitOfficeIdentificationCodeType();
			portOfDeparture.Id.Value = cREHeader.PortOfDeparture;
			return portOfDeparture;
		}

		#endregion

		#region Consignments

		Collection<DeclarationConsignment> PopulateConsignments()
		{
			ZShort consignmentSequence = 0;
			var getSequenceNumeric = cREHeader.UseInterfaceSequenceNumber ? new Func<ICREConsignment, ZShort>((x) => x.SequenceNumber) : new Func<ICREConsignment, ZShort>((x) => ++consignmentSequence);
			var consignments = new Collection<DeclarationConsignment>();
			foreach (var creConsignment in cREHeader.Consignments)
			{
				var consignment = new DeclarationConsignment();
				consignment.SequenceNumeric = getSequenceNumeric(creConsignment);

				consignment.ValueAmount = PopulateConsignmentValueAmount(creConsignment);
				consignment.AdditionalInformation = PopulateAdditionalInformation(creConsignment);
				consignment.Consignee = PopulateConsignee(creConsignment);
				consignment.ConsignmentItem = PopulateConsignmentItems(creConsignment);
				consignment.Consignor = PopulateConsignor(creConsignment);
				consignment.DeliveryDestination = PopulateDeliveryDestination(creConsignment);
				consignment.Freight = PopulateFreightPayment(creConsignment);
				consignment.GoodsLocation = PopulateGoodsLocation(creConsignment);
				consignment.LoadingLocation = PopulateLoadingLocation(creConsignment);
				consignment.NotifyParty = PopulateNotifyParties(creConsignment);
				consignment.TransportContractDocument = PopulateTransportContractDocument(creConsignment);
				if (cREHeader.IsSea && creConsignment.HasContainers)
				{
					consignment.TransportEquipment = PopulateTransportEquipment(creConsignment);
				}

				consignment.UnloadingLocation = PopulateUnloadingLocation(creConsignment);
				consignments.Add(consignment);
			}

			return consignments;
		}

		#region Consignment Details

		ConsignmentValueAmountType PopulateConsignmentValueAmount(ICREConsignment creConsignment)
		{
			ConsignmentValueAmountType consignmentValueAmount = null;
			var valueInNZD = creConsignment.ConsignmentValueInNZD.Round(2);
			if (!cREHeader.HasEmptyContainersOnly && !valueInNZD.IsEmpty)
			{
				consignmentValueAmount = new ConsignmentValueAmountType
				{
					Value = valueInNZD,
					CurrencyId = Iso3AlphaCurrencyCodeContentType.Nzd,
				};
			}
			return consignmentValueAmount;
		}

		Collection<DeclarationConsignmentAdditionalInformation> PopulateAdditionalInformation(ICREConsignment creConsignment)
		{
			var additionalInfo = new Collection<DeclarationConsignmentAdditionalInformation>();

			bool isITRConsignment = creConsignment.TranshipmentDetails?.InternationalTranshipmentRequest ?? false;

			// WOF = Write-off request indicator
			// ITR = International transhipment request indicator
			// HAN = Handling Information
			// MTT = Mode of Transport for Transfer

			if (creConsignment.WriteOffRequest && (!cREHeader.HasEmptyContainersOnly) && (!isITRConsignment))
			{
				/*
				 *	Advice from Customs
				 *	WOF is not necessary for empties (along with values and origin) as they are automatically written off by CusMod.
				 *	By requesting WOF, Goods Transactions fees will be triggered and the client could be charged unnecessarily.
				 *
				 *	Further update, when generating a CRE message with ITR details, the message should NOT include WOF element
				 */
				var writeOffRequest = new DeclarationConsignmentAdditionalInformation();
				writeOffRequest.StatementCode = new AdditionalInformationStatementCodeType();
				writeOffRequest.StatementCode.Value = "Y";
				writeOffRequest.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
				writeOffRequest.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.WOF;
				additionalInfo.Add(writeOffRequest);
			}

			if (isITRConsignment)
			{
				var itr = new DeclarationConsignmentAdditionalInformation();
				itr.StatementDescription = new AdditionalInformationStatementDescriptionTextType();
				if (creConsignment.TranshipmentDetails.ITRImportMode == TransportModeTypeList.Codes.T4)
				{
					itr.StatementDescription.Value = creConsignment.TranshipmentDetails.ITRVoyageFlight.ToUpper() + "," + creConsignment.TranshipmentDetails.ITRImportMode + "," + creConsignment.TranshipmentDetails.ITRArrivalDate.ToString("yyyyMMdd") + ",";
				}
				else if (creConsignment.TranshipmentDetails.ITRImportMode == TransportModeTypeList.Codes.T1)
				{
					itr.StatementDescription.Value = creConsignment.TranshipmentDetails.ITRImportCraft.ToUpper() + "," + creConsignment.TranshipmentDetails.ITRImportMode + "," + creConsignment.TranshipmentDetails.ITRArrivalDate.ToString("yyyyMMdd") + "," + creConsignment.TranshipmentDetails.ITRVoyageFlight;
				}

				itr.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
				itr.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.ITR;
				additionalInfo.Add(itr);

				var mtt = new DeclarationConsignmentAdditionalInformation();
				mtt.StatementCode = new AdditionalInformationStatementCodeType();
				mtt.StatementCode.Value = creConsignment.TranshipmentDetails.ModeOfTransportForTransfer;
				mtt.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
				mtt.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.MTT;
				additionalInfo.Add(mtt);
			}

			if (!creConsignment.HandlingInfo.IsEmpty)
			{
				var handlingInfo = new DeclarationConsignmentAdditionalInformation();
				handlingInfo.StatementDescription = new AdditionalInformationStatementDescriptionTextType();
				handlingInfo.StatementDescription.Value = creConsignment.HandlingInfo;
				handlingInfo.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
				handlingInfo.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.HAN;
				additionalInfo.Add(handlingInfo);
			}

			return additionalInfo;
		}

		DeclarationConsignmentConsignee PopulateConsignee(ICREConsignment creConsignment)
		{
			var consignee = new DeclarationConsignmentConsignee();
			consignee.Name = new ConsigneeNameTextType();
			consignee.Name.Value = creConsignment.Consignee.Name.Left(70);
			consignee.Address = new DeclarationConsignmentConsigneeAddress();
			consignee.Address.CityName = new AddressCityNameTextType();
			consignee.Address.CityName.Value = creConsignment.Consignee.City.Left(35);
			consignee.Address.CountryCode = new AddressCountryCodeType();
			consignee.Address.CountryCode.Value = creConsignment.Consignee.CountryCode;
			consignee.Address.CountrySubDivisionName = new AddressCountrySubDivisionNameTextType();
			consignee.Address.CountrySubDivisionName.Value = creConsignment.Consignee.CountryRegion;
			consignee.Address.Line = new AddressLineTextType();
			consignee.Address.Line.Value = creConsignment.Consignee.Address;
			consignee.Address.PostcodeId = new AddressPostcodeIdType();
			consignee.Address.PostcodeId.Value = creConsignment.Consignee.PostCode;
			return consignee;
		}

		DeclarationConsignmentConsignor PopulateConsignor(ICREConsignment creConsignment)
		{
			var consignor = new DeclarationConsignmentConsignor();
			// This code is temporarily commented out. It should be put back after NZ Customs fix their issue.
			//var clientCode = creConsignment.Consignor.CustomsClientCode;
			//if (!clientCode.IsEmpty)
			//{
			//	consignor.Id = new ConsignorIdentificationIDType();
			//	consignor.Id.Value = clientCode;
			//}
			//else
			consignor.Name = new ConsignorNameTextType();
			consignor.Name.Value = creConsignment.Consignor.Name.Left(70);
			consignor.Address = new DeclarationConsignmentConsignorAddress();
			consignor.Address.CityName = new AddressCityNameTextType();
			consignor.Address.CityName.Value = creConsignment.Consignor.City.Left(35);
			consignor.Address.CountryCode = new AddressCountryCodeType();
			consignor.Address.CountryCode.Value = creConsignment.Consignor.CountryCode;
			consignor.Address.CountrySubDivisionName = new AddressCountrySubDivisionNameTextType();
			consignor.Address.CountrySubDivisionName.Value = creConsignment.Consignor.CountryRegion;
			consignor.Address.Line = new AddressLineTextType();
			consignor.Address.Line.Value = creConsignment.Consignor.Address;
			consignor.Address.PostcodeId = new AddressPostcodeIdType();
			consignor.Address.PostcodeId.Value = creConsignment.Consignor.PostCode;
			return consignor;
		}

		DeclarationConsignmentDeliveryDestination PopulateDeliveryDestination(ICREConsignment creConsignment)
		{
			DeclarationConsignmentDeliveryDestination deliveryDestination = null;
			var deliverToParty = creConsignment.DeliverToParty;
			if (deliverToParty != null && deliverToParty.Name != creConsignment.Consignee.Name)
			{
				deliveryDestination = new DeclarationConsignmentDeliveryDestination();
				deliveryDestination.Name = new DeliveryDestinationNameTextType();
				deliveryDestination.Name.Value = deliverToParty.Name;
				deliveryDestination.Address = new DeclarationConsignmentDeliveryDestinationAddress();
				deliveryDestination.Address.CityName = new AddressCityNameTextType();
				deliveryDestination.Address.CityName.Value = deliverToParty.City;
				deliveryDestination.Address.CountryCode = new AddressCountryCodeType();
				deliveryDestination.Address.CountryCode.Value = deliverToParty.CountryCode;
				deliveryDestination.Address.CountrySubDivisionName = new AddressCountrySubDivisionNameTextType();
				deliveryDestination.Address.CountrySubDivisionName.Value = deliverToParty.CountryRegion;
				deliveryDestination.Address.Line = new AddressLineTextType();
				deliveryDestination.Address.Line.Value = deliverToParty.Address;
				deliveryDestination.Address.PostcodeId = new AddressPostcodeIdType();
				deliveryDestination.Address.PostcodeId.Value = deliverToParty.PostCode;
			}
			return deliveryDestination;
		}

		DeclarationConsignmentFreight PopulateFreightPayment(ICREConsignment creConsignment)
		{
			DeclarationConsignmentFreight freightPayment = null;
			var freightPaymentMethod = creConsignment.FreightPaymentMethod;
			if (!freightPaymentMethod.IsEmpty)
			{
				freightPayment = new DeclarationConsignmentFreight();
				freightPayment.PaymentMethodCode = new FreightPaymentMethodCodeType();
				freightPayment.PaymentMethodCode.Value = freightPaymentMethod;
			}
			return freightPayment;
		}

		DeclarationConsignmentGoodsLocation PopulateGoodsLocation(ICREConsignment creConsignment)
		{
			DeclarationConsignmentGoodsLocation premiseCode = null;
			var goodsLocation = creConsignment.GoodsLocation;
			if (!goodsLocation.IsEmpty)
			{
				premiseCode = new DeclarationConsignmentGoodsLocation();
				premiseCode.Id = new GoodsLocationIdentificationIdType();
				premiseCode.Id.Value = goodsLocation;
			}
			return premiseCode;
		}

		DeclarationConsignmentLoadingLocation PopulateLoadingLocation(ICREConsignment creConsignment)
		{
			var portOfLoading = new DeclarationConsignmentLoadingLocation();
			portOfLoading.Id = new LoadingLocationIdentificationIdType();
			portOfLoading.Id.Value = creConsignment.PortOfLoading;
			return portOfLoading;
		}

		Collection<DeclarationConsignmentNotifyParty> PopulateNotifyParties(ICREConsignment creConsignment)
		{
			var notifyParties = new Collection<DeclarationConsignmentNotifyParty>();
			foreach (var notifyParty in creConsignment.NotifyParties)
			{
				if (notifyParty != null && !notifyParty.Name.IsEmpty)
				{
					var notificationParty = new DeclarationConsignmentNotifyParty();
					notificationParty.Name = new NotifyPartyNameTextType();
					notificationParty.Name.Value = notifyParty.Name;
					notificationParty.RoleCode = new NotifyPartyRoleCodeType();
					notificationParty.RoleCode.Value = RoleCodeList.Codes.NI;
					notificationParty.Address = new DeclarationConsignmentNotifyPartyAddress();
					notificationParty.Address.CityName = new AddressCityNameTextType();
					notificationParty.Address.CityName.Value = notifyParty.City;
					notificationParty.Address.CountryCode = new AddressCountryCodeType();
					notificationParty.Address.CountryCode.Value = notifyParty.CountryCode;
					notificationParty.Address.CountrySubDivisionName = new AddressCountrySubDivisionNameTextType();
					notificationParty.Address.CountrySubDivisionName.Value = notifyParty.CountryRegion;
					notificationParty.Address.Line = new AddressLineTextType();
					notificationParty.Address.Line.Value = notifyParty.Address;
					notificationParty.Address.PostcodeId = new AddressPostcodeIdType();
					notificationParty.Address.PostcodeId.Value = notifyParty.PostCode;
					notifyParties.Add(notificationParty);
				}
			}

			foreach (var deliveryNotifyParty in creConsignment.DeliveryNotifyParties)
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

					deliveryNotificationParty.Communication = deliveryNotificationPartyComms;
					notifyParties.Add(deliveryNotificationParty);
				}
			}
			foreach (var notifyPartyCode in creConsignment.NotifyPartyCodes.Where(x => !x.IsEmpty))
			{
				notifyParties.Add(new DeclarationConsignmentNotifyParty
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
			return notifyParties;
		}

		DeclarationConsignmentTransportContractDocument PopulateTransportContractDocument(ICREConsignment creConsignment)
		{
			var billNumber = new DeclarationConsignmentTransportContractDocument();
			billNumber.Id = new TransportContractDocumentIdentificationIdType();
			billNumber.Id.Value = creConsignment.BillNumber == null ? ZString.Empty : creConsignment.BillNumber.BillNumber;
			billNumber.TypeCode = new TransportContractDocumentTypeCodeType();
			billNumber.TypeCode.Value = creConsignment.BillNumber == null ? ZString.Empty : creConsignment.BillNumber.BillType;
			if (creConsignment.Consolidator != null)
			{
				billNumber.Consolidator = new DeclarationConsignmentTransportContractDocumentConsolidator();
				var clientCode = creConsignment.Consolidator.CustomsClientCode;
				if (!clientCode.IsEmpty)
				{
					billNumber.Consolidator.Id = new ConsolidatorIdentificationIdType();
					billNumber.Consolidator.Id.Value = clientCode;
				}
				else
				{
					billNumber.Consolidator.Name = new ConsolidatorNameTextType();
					billNumber.Consolidator.Name.Value = creConsignment.Consolidator.Name;
				}
			}

			return billNumber;
		}

		Collection<DeclarationConsignmentTransportEquipment> PopulateTransportEquipment(ICREConsignment creConsignment)
		{
			int sequence = 0;
			var containerList = new Collection<DeclarationConsignmentTransportEquipment>();
			foreach (var container in creConsignment.Containers)
			{
				if (container != null && container.ContainerMode != ContainerModeList.Codes.ROR)
				{
					sequence++;
					var transportEquipment = new DeclarationConsignmentTransportEquipment();
					transportEquipment.SequenceNumeric = sequence;
					transportEquipment.CharacteristicCode = new TransportEquipmentCharacteristicCodeType();
					transportEquipment.CharacteristicCode.Value = container.Size;
					transportEquipment.FullnessCode = new TransportEquipmentFullnessCodeType();
					transportEquipment.FullnessCode.Value = container.Status;
					if (!container.AttachedEquipmentCode.IsEmpty)
					{
						transportEquipment.AttachedCode = new TransportEquipmentAttachedCodeType();
						transportEquipment.AttachedCode.Value = container.AttachedEquipmentCode;
					}

					transportEquipment.Id = new TransportEquipmentIdentificationIdType();
					transportEquipment.Id.Value = container.ContainerNumber;
					var containerSeals = new Collection<DeclarationConsignmentTransportEquipmentSeal>();
					int sealSequence = 0;
					foreach (var sealNo in container.SealNumbers)
					{
						sealSequence++;
						var containerSeal = new DeclarationConsignmentTransportEquipmentSeal();
						containerSeal.SequenceNumeric = sealSequence;
						containerSeal.Id = new SealIdentificationIdType();
						containerSeal.Id.Value = sealNo;
						containerSeals.Add(containerSeal);
					}

					transportEquipment.Seal = containerSeals;
					containerList.Add(transportEquipment);
				}
			}

			return containerList;
		}

		DeclarationConsignmentUnloadingLocation PopulateUnloadingLocation(ICREConsignment creConsignment)
		{
			var portOfDischarge = new DeclarationConsignmentUnloadingLocation();
			portOfDischarge.Id = new UnloadingLocationIdentificationIdType();
			portOfDischarge.Id.Value = creConsignment.PortOfDischarge;
			return portOfDischarge;
		}

		#endregion

		#endregion

		#region ConsignmentItems

		Collection<DeclarationConsignmentConsignmentItem> PopulateConsignmentItems(ICREConsignment creConsignment)
		{
			int itemSequence = 0;
			var consignmentItems = new Collection<DeclarationConsignmentConsignmentItem>();

			foreach (var creConsignmentItem in creConsignment.ConsignmentItems.OrderBy(x => x.SequenceNumber))
			{
				itemSequence++;
				var consignmentItem = new DeclarationConsignmentConsignmentItem();
				consignmentItem.SequenceNumeric = creConsignmentItem.SequenceNumber > 0 ? creConsignmentItem.SequenceNumber : itemSequence;
				consignmentItem.Commodity = PopulateCommodity(creConsignmentItem);

				if (!creConsignmentItem.IsEmptyContainer)
				{
					consignmentItem.GoodsMeasure = PopulateGoodsMeasure(creConsignmentItem);
					consignmentItem.Origin = PopulateOrigin(creConsignmentItem);
					consignmentItem.Packaging = PopulatePackaging(creConsignmentItem);
				}

				if (cREHeader.IsSea && !creConsignmentItem.ContainerNumber.IsEmpty)
				{
					var container = creConsignment.Containers.SingleOrDefault(x => x.ContainerNumber == creConsignmentItem.ContainerNumber);
					if (container != null && container.ContainerMode != ContainerModeList.Codes.ROR)
					{
						consignmentItem.TransportEquipment = PopulateTransportEquipment(creConsignmentItem);
					}
				}

				consignmentItems.Add(consignmentItem);
			}

			return consignmentItems;
		}

		#region ConsignmentItem Details

		DeclarationConsignmentConsignmentItemCommodity PopulateCommodity(ICREConsignmentItem creConsignmentItem)
		{
			var commodity = new DeclarationConsignmentConsignmentItemCommodity();
			commodity.CargoDescription = new CommodityCargoDescriptionTextType();
			commodity.CargoDescription.Value = creConsignmentItem.GoodsDescription.KeepChars(ZString.AlphanumericCharacters + " ", "");
			if (!creConsignmentItem.IsEmptyContainer)
			{
				foreach (var identifier in creConsignmentItem.Identifiers)
				{
					if (identifier != null)
					{
						commodity.CommercialCategorizationId = new CommodityCommercialCategorizationIdType();
						commodity.CommercialCategorizationId.Value = identifier.CommodityNumber;
						commodity.IdentityQualifierCode = new CommodityIdentityQualifierCodeType();
						commodity.IdentityQualifierCode.Value = identifier.CommodityType;
					}
				}

				commodity.ValueAmount = PopulateCommodityValueAmount(creConsignmentItem);
				commodity.Classification = PopulateItemClassifications(creConsignmentItem);
			}

			return commodity;
		}

		CommodityValueAmountType PopulateCommodityValueAmount(ICREConsignmentItem creConsignmentItem)
		{
			CommodityValueAmountType commodityValueAmount = null;
			var currency = creConsignmentItem.Currency;
			var value = creConsignmentItem.Value.Round(2);
			if (value.IsEmpty && currency.IsEmpty)
			{
				currency =  Core.Constants.CurrencyCodes.NewZealand;
			}

			commodityValueAmount = new CommodityValueAmountType
			{
				Value = value,
				CurrencyId = CurrencyID<Iso3AlphaCurrencyCodeContentType>(currency),
			};

			return commodityValueAmount;
		}

		Collection<DeclarationConsignmentConsignmentItemCommodityClassification> PopulateItemClassifications(ICREConsignmentItem consignmentItem)
		{
			var classifications = new Collection<DeclarationConsignmentConsignmentItemCommodityClassification>();
			if (!consignmentItem.UNDGHazardousGoodsCode.IsEmpty)
			{
				var classification = new DeclarationConsignmentConsignmentItemCommodityClassification();
				classification.Id = new ClassificationIdentificationIdType();
				classification.Id.Value = consignmentItem.UNDGHazardousGoodsCode;
				classification.IdentificationTypeCode = new ClassificationIdentificationTypeCodeType();
				classification.IdentificationTypeCode.Value = ClassificationTypeList.Codes.SSO;
				classifications.Add(classification);
			}

			foreach (var classificationItem in consignmentItem.Classifications)
			{
				if (classificationItem != null)
				{
					var commodityClassification = new DeclarationConsignmentConsignmentItemCommodityClassification();
					commodityClassification.Id = new ClassificationIdentificationIdType();
					commodityClassification.Id.Value = classificationItem.Classification;
					commodityClassification.IdentificationTypeCode = new ClassificationIdentificationTypeCodeType();
					commodityClassification.IdentificationTypeCode.Value = classificationItem.ClassificationTypeCode;
					classifications.Add(commodityClassification);
				}
			}

			return classifications;
		}

		DeclarationConsignmentConsignmentItemGoodsMeasure PopulateGoodsMeasure(ICREConsignmentItem creConsignmentItem)
		{
			var grossWeight = new DeclarationConsignmentConsignmentItemGoodsMeasure();
			grossWeight.GrossMassMeasure = new GoodsMeasureGrossMassMeasureType();
			grossWeight.GrossMassMeasure.Value = creConsignmentItem.GrossWeightInKg.Round(3);
			grossWeight.GrossMassMeasure.UnitCode = MeasurementUnitCommonCodeContentType.Kgm;
			return grossWeight;
		}

		DeclarationConsignmentConsignmentItemOrigin PopulateOrigin(ICREConsignmentItem creConsignmentItem)
		{
			var countryOfOrigin = new DeclarationConsignmentConsignmentItemOrigin();
			countryOfOrigin.CountryCode = new OriginCountryCodeType();
			countryOfOrigin.CountryCode.Value = creConsignmentItem.GoodsOriginCountry;
			return countryOfOrigin;
		}

		DeclarationConsignmentConsignmentItemPackaging PopulatePackaging(ICREConsignmentItem creConsignmentItem)
		{
			var packages = new DeclarationConsignmentConsignmentItemPackaging();
			packages.SequenceNumeric = 1m;  // must contain 1
			packages.QuantityQuantity = new PackagingQuantityQuantityType();
			packages.QuantityQuantity.Value = creConsignmentItem.PackageQty;
			packages.TypeCode = new PackagingTypeCodeType();
			packages.TypeCode.Value = creConsignmentItem.PackageType;
			return packages;
		}

		DeclarationConsignmentConsignmentItemTransportEquipment PopulateTransportEquipment(ICREConsignmentItem creConsignmentItem)
		{
			var container = new DeclarationConsignmentConsignmentItemTransportEquipment();
			container.Id = new TransportEquipmentIdentificationIdType();
			container.Id.Value = creConsignmentItem.ContainerNumber;
			return container;
		}

		#endregion

		#endregion

		#endregion
	}
}
