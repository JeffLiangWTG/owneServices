using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CargoWise.Customs.NZ.MessageDefinitions.TSW.Version1_1.DocumentMetadata;
using CargoWise.Customs.NZ.MessageDefinitions.TSW.Version1_1.EX1.Outgoing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders
{
	/// <summary>
	/// Export Declaration
	/// •	Submitted by
	///		o	Exporters or their brokers
	///
	///	•	Data:
	///		o	Details of goods seeking export clearance
	///
	///	•	Used to effect export clearance on behalf of:
	///		o	Customs
	///		o	Contains placeholders for elements required by MAF’s Animal Products Export Certification as a future phase of TSW
	///
	/// </summary>
	public class EX1MessageBuilder : TSWMessageBuilder<Declaration>
	{
		public EX1MessageBuilder(IExportDeclaration declarationHeader, TSWTransactionTypes transactionType)
			: base()
		{
			eX1Header = declarationHeader;
			this.transactionType = transactionType;
		}
		readonly IExportDeclaration eX1Header;
		readonly TSWTransactionTypes transactionType;

		public override ZString MessageType
		{
			get { return eX1Header.MessageType; }
		}

		bool IsDrawbackEntry
		{
			get { return MessageType == MessageTypeList.Codes.E41; }
		}

		#region Message Declaration Defaults

		protected override WcoDocumentNameCode WCODocumentName
		{
			get { return WcoDocumentNameCode.Ex; }
		}

		protected override NzDocumentNameCode NZDocumentName
		{
			get { return NzDocumentNameCode.Ex1; }
		}

		public override ZBool DeclarantPinRequired
		{
			get { return true; }
		}

		public override ZString DeclarantPinEncrypted
		{
			get { return eX1Header.Declarant.DeclarantPinEncrypted; }
		}

		#endregion

		#region xml EX1 Declaration message

		protected override Declaration DeclarationMessage()
		{
			var message = new Declaration();

			if (transactionType != TSWTransactionTypes.Original && transactionType != TSWTransactionTypes.Completion)
			{
				message.Id = PopulateReferenceNo();
			}

			message.TypeCode = PopulateMessageType();
			message.FunctionalReferenceId = PopulateSendersRef();
			message.FunctionCode = PopulateTransType();
			message.Submitter = PopulateSubmitter();
			message.AdditionalDocument = PopulateAdditionalDocs();
			message.AdditionalInformation = PopulateAdditionalInfo();
			message.Declarant = PopulateDeclarant();
			if (transactionType != TSWTransactionTypes.Cancel)
			{
				message.TotalGrossMassMeasure = PopulateGrossWeight();
				message.Agent = PopulateAgent();
				message.BorderTransportMeans = PopulateBorderTM();
				if (eX1Header.Carrier != null)
				{
					message.Carrier = PopulateCarrier();
				}

				message.CurrencyExchange = PopulateCurrencies();
				if (IsDrawbackEntry)
				{
					message.DutyTaxFee = PopulateDutyTaxFees();
				}

				message.Exporter = PopulateExporter();
				message.GoodsShipment = PopulateGoodsShipment();
				message.Packaging = PopulatePackaging();

				if (eX1Header.IsCompletionEntry)
				{
					message.PreviousDocument = PopulatePreviousDocument();
				}
			}

			return message;
		}

		#region Declaration Details

		DeclarationIdentificationIdType PopulateReferenceNo()
		{
			var msgID = new DeclarationIdentificationIdType();
			msgID.Value = eX1Header.TSWReferenceNumber;
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
			functionalReferenceID.Value = eX1Header.SenderReferenceNumber;
			return functionalReferenceID;
		}

		DeclarationFunctionCodeType PopulateTransType()
		{
			var functionCode = new DeclarationFunctionCodeType();
			functionCode.Value = GetTransactionTypeCode(transactionType);
			return functionCode;
		}

		DeclarationTotalGrossMassMeasureType PopulateGrossWeight()
		{
			var totalGrossMassMeasure = new DeclarationTotalGrossMassMeasureType();
			totalGrossMassMeasure.Value = NZWeightHelper.ApplyWeightRounding(eX1Header.TotalGrossWeightInKGM);
			totalGrossMassMeasure.UnitCode = MeasurementUnitCommonCodeContentType.Kgm;
			return totalGrossMassMeasure;
		}

		DeclarationSubmitter PopulateSubmitter()
		{
			var submitter = new DeclarationSubmitter();
			submitter.Id = new SubmitterIdentificationIdType();
			submitter.Id.Value = eX1Header.SubmitterCode;
			return submitter;
		}

		Collection<DeclarationAdditionalDocument> PopulateAdditionalDocs()
		{
			var additionalDocs = new Collection<DeclarationAdditionalDocument>();
			foreach (ZString permit in eX1Header.Permits)
			{
				var refDoc = new DeclarationAdditionalDocument();
				refDoc.Id = new AdditionalDocumentIdentificationIdType();
				refDoc.Id.Value = permit;
				refDoc.TypeCode = new AdditionalDocumentTypeCodeType();
				refDoc.TypeCode.Value = AdditionalDocumentTypeList.Codes.PER;
				additionalDocs.Add(refDoc);
			}

			foreach (IOtherInfo referenceDocument in eX1Header.OtherReferencedDocuments)
			{
				var refDoc = new DeclarationAdditionalDocument();
				refDoc.Id = new AdditionalDocumentIdentificationIdType();
				refDoc.Id.Value = referenceDocument.Data;
				refDoc.TypeCode = new AdditionalDocumentTypeCodeType();
				refDoc.TypeCode.Value = referenceDocument.Code;
				additionalDocs.Add(refDoc);
			}

			if (eX1Header.AdditionalInformation != null)
			{
				foreach (ITSWAttachment attachment in eX1Header.AdditionalInformation.SupportingDocuments)
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
			if (eX1Header.AdditionalInformation != null)
			{
				if (transactionType == TSWTransactionTypes.Original || transactionType == TSWTransactionTypes.Replace || transactionType == TSWTransactionTypes.Completion)
				{
					if (!eX1Header.AdditionalInformation.FreeText.IsEmpty)
					{
						decAddInfo.Add(FreeTextInfo());
					}

					if (!eX1Header.AdditionalInformation.ManualOverrideText.IsEmpty)
					{
						decAddInfo.Add(ManualOverrideInfo());
					}

					int rateSequence = 0;
					foreach (ICurrency currencyDetails in eX1Header.ExchangeRates)
					{
						rateSequence++;
						decAddInfo.Add(ExchangeRateIndicator(rateSequence, currencyDetails.ExchangeRateIndicator));
					}

					foreach (IOtherInfo otherInfoDetails in eX1Header.OtherInfoCodes)
					{
						decAddInfo.Add(OtherInfoCodes(otherInfoDetails));
					}

					if (!eX1Header.HandlingInformation.IsEmpty)
					{
						decAddInfo.Add(HandlingInfo());
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
			freeTextAddInfo.Content.Value = eX1Header.AdditionalInformation.FreeText;
			return freeTextAddInfo;
		}

		DeclarationAdditionalInformation ExchangeRateIndicator(int sequence, string rateIndicator)
		{
			var exchangeRateIndAddInfo = new DeclarationAdditionalInformation();
			exchangeRateIndAddInfo.StatementCode = new AdditionalInformationStatementCodeType();
			exchangeRateIndAddInfo.StatementCode.Value = rateIndicator;
			exchangeRateIndAddInfo.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
			exchangeRateIndAddInfo.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.ERI;

			var exchangeRateIndPointers = new Collection<DeclarationAdditionalInformationPointer>();
			var decPointer = new DeclarationAdditionalInformationPointer();
			decPointer.DocumentSectionCode = new PointerDocumentSectionCodeType();
			decPointer.DocumentSectionCode.Value = "42A";
			exchangeRateIndPointers.Add(decPointer);
			var exchangeRateIndPointer = new DeclarationAdditionalInformationPointer();
			exchangeRateIndPointer.SequenceNumeric = sequence;
			exchangeRateIndPointer.DocumentSectionCode = new PointerDocumentSectionCodeType();
			exchangeRateIndPointer.DocumentSectionCode.Value = "40A";
			exchangeRateIndPointers.Add(exchangeRateIndPointer);
			exchangeRateIndAddInfo.Pointer = exchangeRateIndPointers;

			return exchangeRateIndAddInfo;
		}

		DeclarationAdditionalInformation ManualOverrideInfo()
		{
			var manualOverrideAddInfo = new DeclarationAdditionalInformation();
			manualOverrideAddInfo.RequestOverrideCode = new AdditionalInformationRequestOverrideCodeType();
			manualOverrideAddInfo.RequestOverrideCode.Value = "Y";
			manualOverrideAddInfo.StatementDescription = new AdditionalInformationStatementDescriptionTextType();
			manualOverrideAddInfo.StatementDescription.Value = eX1Header.AdditionalInformation.ManualOverrideText; // "Must be present to advise the reason for override or manual processing";
			manualOverrideAddInfo.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
			manualOverrideAddInfo.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.ALP;
			return manualOverrideAddInfo;
		}

		DeclarationAdditionalInformation ChangeCancelReason()
		{
			var ccReasonAddInfo = new DeclarationAdditionalInformation();
			ccReasonAddInfo.StatementDescription = new AdditionalInformationStatementDescriptionTextType();
			ccReasonAddInfo.StatementDescription.Value = eX1Header.AdditionalInformation.AdditionalStatementText; //"Change/Replace/Cancel reason";
			ccReasonAddInfo.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
			ccReasonAddInfo.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.AES;
			return ccReasonAddInfo;
		}

		DeclarationAdditionalInformation OtherInfoCodes(IOtherInfo otherInfoDetails)
		{
			var otherInfoCodesAddInfo = new DeclarationAdditionalInformation();
			otherInfoCodesAddInfo.StatementCode = new AdditionalInformationStatementCodeType();
			otherInfoCodesAddInfo.StatementCode.Value = otherInfoDetails.Code;
			if (!otherInfoDetails.Data.IsEmpty)
			{
				otherInfoCodesAddInfo.StatementDescription = new AdditionalInformationStatementDescriptionTextType();
				otherInfoCodesAddInfo.StatementDescription.Value = otherInfoDetails.Data;
			}
			otherInfoCodesAddInfo.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
			otherInfoCodesAddInfo.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.OIN;
			return otherInfoCodesAddInfo;
		}

		DeclarationAdditionalInformation HandlingInfo()
		{
			var handlingInfoAddInfo = new DeclarationAdditionalInformation();
			handlingInfoAddInfo.StatementDescription = new AdditionalInformationStatementDescriptionTextType();
			handlingInfoAddInfo.StatementDescription.Value = eX1Header.HandlingInformation;
			handlingInfoAddInfo.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
			handlingInfoAddInfo.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.HAN;
			return handlingInfoAddInfo;
		}

		#endregion

		Collection<DeclarationAgent> PopulateAgent()
		{
			var agentList = new Collection<DeclarationAgent>();
			var agent = new DeclarationAgent();
			agent.Id = new AgentIdentificationIdType();
			agent.Id.Value = eX1Header.BrokerCode;
			agent.RoleCode = new AgentRoleCodeType();
			agent.RoleCode.Value = RoleCodeList.Codes.CB;
			agentList.Add(agent);
			return agentList;
		}

		DeclarationBorderTransportMeans PopulateBorderTM()
		{
			var boarderTM = new DeclarationBorderTransportMeans();
			if (eX1Header.IsSea || eX1Header.IsAir)
			{
				boarderTM.Name = new BorderTransportMeansNameTextType();
				boarderTM.Name.Value = eX1Header.IsSea ? eX1Header.CraftName.ToUpper() : eX1Header.FlightNo.ToUpper();
			}

			boarderTM.TypeCode = new BorderTransportMeansTypeCodeType();
			if (eX1Header.IsSea)
			{
				boarderTM.TypeCode.Value = TransportModeTypeList.Codes.T1;
				boarderTM.Id = new BorderTransportMeansIdentificationIdType();
				boarderTM.Id.Value = eX1Header.LloydsNo;
				boarderTM.JourneyId = new BorderTransportMeansJourneyIdType();
				boarderTM.JourneyId.Value = eX1Header.VoyageNo.ToUpper();
			}
			else if (eX1Header.IsAir)
			{
				boarderTM.TypeCode.Value = TransportModeTypeList.Codes.T4;
			}
			else if (eX1Header.IsMail)
			{
				boarderTM.TypeCode.Value = TransportModeTypeList.Codes.T5;
			}

			return boarderTM;
		}

		DeclarationCarrier PopulateCarrier()
		{
			var carrier = new DeclarationCarrier();
			carrier.Name = new CarrierNameTextType();
			carrier.Name.Value = eX1Header.Carrier.Name;
			return carrier;
		}

		Collection<DeclarationCurrencyExchange> PopulateCurrencies()
		{
			var currencyExchanges = new Collection<DeclarationCurrencyExchange>();
			foreach (ICurrency currencyDetails in eX1Header.ExchangeRates)
			{
				var declarationCurrency = new DeclarationCurrencyExchange();
				declarationCurrency.CurrencyTypeCode = new CurrencyExchangeCurrencyTypeCodeType();
				declarationCurrency.CurrencyTypeCode.Value = currencyDetails.CurrencyCode;
				declarationCurrency.RateNumeric = currencyDetails.ExchangeRate.Round(2);
				currencyExchanges.Add(declarationCurrency);
			}

			return currencyExchanges;
		}

		DeclarationDeclarant PopulateDeclarant()
		{
			var declarant = new DeclarationDeclarant();
			declarant.Id = new DeclarantIdentificationIdType();
			declarant.Id.Value = eX1Header.Declarant != null ? eX1Header.Declarant.DeclarantID : ZString.Empty;

			if (eX1Header.Declarant != null)
			{
				var declarantComms = new Collection<DeclarationDeclarantCommunication>();
				foreach (ICommunication comms in eX1Header.Declarant.Communications)
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

		#region DutyTaxFees details

		Collection<DeclarationDutyTaxFee> PopulateDutyTaxFees()
		{
			var dutyTaxFees = new Collection<DeclarationDutyTaxFee>();

			var dutyTaxFeePayment = new DeclarationDutyTaxFee();
			dutyTaxFeePayment.Payment = new DeclarationDutyTaxFeePayment();
			dutyTaxFeePayment.Payment.MethodCode = new PaymentMethodCodeType();
			dutyTaxFeePayment.Payment.MethodCode.Value = eX1Header.PaymentType;
			dutyTaxFees.Add(dutyTaxFeePayment);

			ZBool hasCustomsDuty = ChargesIncludesDuty();
			ZBool hasGST = ChargesIncludesGST();
			ZBool dutyFeeGenerated = false;
			ZBool gstFeeGenerated = false;
			ZBool totalFeeGenerated = false;

			foreach (IDutyTaxFee charge in eX1Header.DutyTaxFees)
			{
				if (!dutyFeeGenerated)
				{
					dutyFeeGenerated = charge.DutyTaxFeeType == DutyTaxFeeTypeList.Codes.CUD;
				}

				if (!gstFeeGenerated)
				{
					gstFeeGenerated = charge.DutyTaxFeeType == DutyTaxFeeTypeList.Codes.GST;
				}

				if (!totalFeeGenerated)
				{
					totalFeeGenerated = charge.DutyTaxFeeType == DutyTaxFeeTypeList.Codes.TOT;
				}

				if (charge.DutyTaxFeeType == DutyTaxFeeTypeList.Codes.TOT)
				{
					if (!hasCustomsDuty)
					{
						dutyTaxFees.Add(HeaderDutyTaxFee(DutyTaxFeeTypeList.Codes.CUD, Core.Constants.CurrencyCodes.NewZealand, 0m));
						dutyFeeGenerated = true;
					}

					if (!hasGST)
					{
						dutyTaxFees.Add(HeaderDutyTaxFee(DutyTaxFeeTypeList.Codes.GST, Core.Constants.CurrencyCodes.NewZealand, 0m));
						gstFeeGenerated = true;
					}
				}

				dutyTaxFees.Add(HeaderDutyTaxFee(charge.DutyTaxFeeType, charge.CurrencyCode, charge.Amount));
			}

			if (!dutyFeeGenerated)
			{
				dutyTaxFees.Add(HeaderDutyTaxFee(DutyTaxFeeTypeList.Codes.CUD, Core.Constants.CurrencyCodes.NewZealand, 0m));
			}

			if (!gstFeeGenerated)
			{
				dutyTaxFees.Add(HeaderDutyTaxFee(DutyTaxFeeTypeList.Codes.GST, Core.Constants.CurrencyCodes.NewZealand, 0m));
			}

			if (!totalFeeGenerated)
			{
				dutyTaxFees.Add(HeaderDutyTaxFee(DutyTaxFeeTypeList.Codes.TOT, Core.Constants.CurrencyCodes.NewZealand, 0m));
			}

			return dutyTaxFees;
		}

		DeclarationDutyTaxFee HeaderDutyTaxFee(string feeType, string currencyCode, ZDecimal amount)
		{
			var dutyTaxFee = new DeclarationDutyTaxFee();
			dutyTaxFee.TypeCode = new DutyTaxFeeTypeCodeType();
			dutyTaxFee.TypeCode.Value = feeType;
			dutyTaxFee.Payment = new DeclarationDutyTaxFeePayment();
			dutyTaxFee.Payment.TaxAssessedAmount = new PaymentTaxAssessedAmountType();
			dutyTaxFee.Payment.TaxAssessedAmount.CurrencyId = CurrencyID<Iso3AlphaCurrencyCodeContentType>(currencyCode);
			dutyTaxFee.Payment.TaxAssessedAmount.Value = amount.Round(2);
			return dutyTaxFee;
		}

		ZBool ChargesIncludesDuty()
		{
			return eX1Header.DutyTaxFees.Cast<IDutyTaxFee>().FirstOrDefault(x => x.DutyTaxFeeType == DutyTaxFeeTypeList.Codes.CUD) != null;
		}

		ZBool ChargesIncludesGST()
		{
			return eX1Header.DutyTaxFees.Cast<IDutyTaxFee>().FirstOrDefault(x => x.DutyTaxFeeType == DutyTaxFeeTypeList.Codes.GST) != null;
		}

		#endregion

		DeclarationExporter PopulateExporter()
		{
			var exporter = new DeclarationExporter();
			exporter.Id = new ExporterIdentificationIdType();
			exporter.Id.Value = eX1Header.Exporter != null ? eX1Header.Exporter.CustomsClientCode : ZString.Empty;
			return exporter;
		}

		Collection<DeclarationPackaging> PopulatePackaging()
		{
			var declarationPackaging = new Collection<DeclarationPackaging>();
			var packagingInMessage = new List<IPackaging>(PackagingPointedTo.Values);
			var orderedPackaging = packagingInMessage.OrderBy(x => x.MessageSequence);
			foreach (IPackaging decPackaging in orderedPackaging)
			{
				var packaging = new DeclarationPackaging();
				packaging.SequenceNumeric = decPackaging.MessageSequence;
				packaging.QuantityQuantity = new PackagingQuantityQuantityType();
				packaging.QuantityQuantity.Value = decPackaging.NumberOfPackages;
				packaging.TypeCode = new PackagingTypeCodeType();
				packaging.TypeCode.Value = decPackaging.PackageType;
				declarationPackaging.Add(packaging);
			}

			return declarationPackaging;
		}

		Collection<DeclarationPreviousDocument> PopulatePreviousDocument()
		{
			var declarationPreviousDocument = new Collection<DeclarationPreviousDocument>();
			var previousDocument = new DeclarationPreviousDocument();
			previousDocument.Id = new PreviousDocumentIdentificationIdType();
			previousDocument.Id.Value = eX1Header.PreviousDocumentNo;
			previousDocument.TypeCode = new PreviousDocumentTypeCodeType();
			previousDocument.TypeCode.Value = eX1Header.PreviousDocumentType;
			declarationPreviousDocument.Add(previousDocument);
			return declarationPreviousDocument;
		}

		#endregion

		#region Goods Shipment

		DeclarationGoodsShipment PopulateGoodsShipment()
		{
			var shipment = new DeclarationGoodsShipment();
			shipment.ExitDateTime = PopulateShipmentExportDate();
			shipment.TransactionNatureCode = PopulateNatureOfTransaction();
			shipment.Consignment = PopulateConsignment();

			if (eX1Header.GoodsShipment.DeliverToParty != null)
			{
				if (eX1Header.GoodsShipment.DeliverToParty != eX1Header.Importer)
				{
					shipment.DeliveryDestination = PopulateDeliveryParty();
				}
			}

			shipment.GovernmentAgencyGoodsItem = PopulateGoodsItems();
			shipment.Importer = PopulateImporter();
			shipment.Invoice = PopulateInvoices();
			shipment.NotifyParty = PopulateNotifyParties();

			if (eX1Header.IsSea && eX1Header.IsContainerised)
			{
				shipment.StuffingEstablishment = PopulateStuffingEstablishments();
			}

			if (!eX1Header.GoodsShipment.CustomsControlledArea.IsEmpty)
			{
				shipment.Warehouse = PopulateCustomsControlledArea();
			}

			return shipment;
		}

		#region Goods Shipment Methods

		GoodsShipmentExitDateTimeType PopulateShipmentExportDate()
		{
			var exitDateTime = new GoodsShipmentExitDateTimeType();
			exitDateTime.Value = eX1Header.DateOfExport.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
			exitDateTime.FormatCode = DateTimePeriodFormatCode.Item102;
			return exitDateTime;
		}

		GoodsShipmentTransactionNatureCodeType PopulateNatureOfTransaction()
		{
			var transactionNatureCode = new GoodsShipmentTransactionNatureCodeType();
			transactionNatureCode.Value = eX1Header.GoodsShipment.NatureOfTransaction;
			return transactionNatureCode;
		}

		DeclarationGoodsShipmentConsignment PopulateConsignment()
		{
			var consignment = new DeclarationGoodsShipmentConsignment();
			if (eX1Header.IsAir || eX1Header.IsMail || (eX1Header.IsSea && !eX1Header.GoodsShipment.LocationOfGoods.IsEmpty))
			{
				consignment.GoodsLocation = PopulateLocationOfGoods();
			}

			consignment.LoadingLocation = PopulatePortOfLoading();
			StoreBillsPointedTo(eX1Header);
			StoreEquipmentPointedTo(eX1Header.Equipment);
			StorePackagingPointedTo(eX1Header);
			consignment.TransportContractDocument = PopulateBillDetails();
			if (eX1Header.IsContainerised)
			{
				consignment.TransportEquipment = PopulateContainerDetails();
			}

			consignment.UnloadingLocation = PopulatePortOfDischarge();
			return consignment;
		}

		#region Populate Consignment Methods

		DeclarationGoodsShipmentConsignmentGoodsLocation PopulateLocationOfGoods()
		{
			var goodsLocation = new DeclarationGoodsShipmentConsignmentGoodsLocation();
			goodsLocation.Id = new GoodsLocationIdentificationIdType();
			goodsLocation.Id.Value = eX1Header.GoodsShipment.LocationOfGoods;
			return goodsLocation;
		}

		DeclarationGoodsShipmentConsignmentLoadingLocation PopulatePortOfLoading()
		{
			var loadingLocation = new DeclarationGoodsShipmentConsignmentLoadingLocation();
			loadingLocation.Id = new LoadingLocationIdentificationIdType();
			loadingLocation.Id.Value = eX1Header.GoodsShipment.PortOfLoading;
			return loadingLocation;
		}

		DeclarationGoodsShipmentConsignmentTransportContractDocument PopulateSingleBillDetails(IAssociatedTransportDocument bill)
		{
			var contractDoc = new DeclarationGoodsShipmentConsignmentTransportContractDocument();
			contractDoc.Id = new TransportContractDocumentIdentificationIdType();
			contractDoc.Id.Value = bill.BillNumber;
			contractDoc.TypeCode = new TransportContractDocumentTypeCodeType();
			contractDoc.TypeCode.Value = bill.BillType;

			var pointers = new Collection<DeclarationGoodsShipmentConsignmentTransportContractDocumentPointer>();

			if (eX1Header.MasterBills.Any() || eX1Header.IsMail)
			{
				var masterBill = bill as IMasterBillTransportDocument;
				if (masterBill != null && masterBill.ChildBills.Any())
				{
					PopulateTransportContractPointers(masterBill, pointers);
				}
				else
				{
					if (bill.RelatedEquipment.Any())
					{
						PopulateTransportEquipmentPointers(bill, pointers);
					}

					if (bill.RelatedPackages.Any())
					{
						PopulatePackagingPointers(bill, pointers);
					}
				}
			}
			else // entry with No Masterbill
			{
				PopulateTransportPointers(pointers);
				if (bill.RelatedEquipment.Any())
				{
					PopulateTransportEquipmentPointers(bill, pointers);
				}

				if (bill.RelatedPackages.Any())
				{
					PopulatePackagingPointers(bill, pointers);
				}
			}

			contractDoc.Pointer = pointers;
			return contractDoc;
		}

		Collection<DeclarationGoodsShipmentConsignmentTransportContractDocument> PopulateBillDetails()
		{
			var billNumbers = new Collection<DeclarationGoodsShipmentConsignmentTransportContractDocument>();
			if (eX1Header.IsMail)
			{
				foreach (IAssociatedTransportDocument postBill in eX1Header.AllBills)
				{
					billNumbers.Add(PopulateSingleBillDetails(postBill));
				}
			}
			else if (eX1Header.MasterBills.Any())
			{
				foreach (IMasterBillTransportDocument bill in eX1Header.MasterBills)
				{
					var contractDoc = PopulateSingleBillDetails(bill);
					billNumbers.Add(contractDoc);

					foreach (ZGuid childBillPK in bill.ChildBills)
					{
						var childBill = GetBillElementDetails(childBillPK);
						billNumbers.Add(PopulateSingleBillDetails(childBill));
					}
				}
			}
			else
			{
				foreach (IAssociatedTransportDocument houseBill in eX1Header.AllBills)
				{
					billNumbers.Add(PopulateSingleBillDetails(houseBill));
				}
			}

			return billNumbers;
		}

		void PopulateTransportContractPointers(IMasterBillTransportDocument masterBill, Collection<DeclarationGoodsShipmentConsignmentTransportContractDocumentPointer> billPointers)
		{
			PopulateTransportPointers(billPointers);
			foreach (ZGuid billPK in masterBill.ChildBills)
			{
				var billPointer = new DeclarationGoodsShipmentConsignmentTransportContractDocumentPointer();
				billPointer.SequenceNumeric = GetBillElementSequence(billPK);
				billPointer.DocumentSectionCode = new PointerDocumentSectionCodeType();
				billPointer.DocumentSectionCode.Value = "30B";
				billPointers.Add(billPointer);
			}
		}

		void PopulateTransportPointers(Collection<DeclarationGoodsShipmentConsignmentTransportContractDocumentPointer> billPointers)
		{
			var decPointer = new DeclarationGoodsShipmentConsignmentTransportContractDocumentPointer();
			decPointer.DocumentSectionCode = new PointerDocumentSectionCodeType();
			decPointer.DocumentSectionCode.Value = "42A";
			billPointers.Add(decPointer);

			var shipmentPointer = new DeclarationGoodsShipmentConsignmentTransportContractDocumentPointer();
			shipmentPointer.DocumentSectionCode = new PointerDocumentSectionCodeType();
			shipmentPointer.DocumentSectionCode.Value = "67A";
			billPointers.Add(shipmentPointer);

			var consignmentPointer = new DeclarationGoodsShipmentConsignmentTransportContractDocumentPointer();
			consignmentPointer.DocumentSectionCode = new PointerDocumentSectionCodeType();
			consignmentPointer.DocumentSectionCode.Value = "28A";
			billPointers.Add(consignmentPointer);
		}

		void PopulateTransportEquipmentPointers(IAssociatedTransportDocument bill, Collection<DeclarationGoodsShipmentConsignmentTransportContractDocumentPointer> equipmentPointers)
		{
			var decPointer = new DeclarationGoodsShipmentConsignmentTransportContractDocumentPointer();
			decPointer.DocumentSectionCode = new PointerDocumentSectionCodeType();
			decPointer.DocumentSectionCode.Value = "42A";
			equipmentPointers.Add(decPointer);

			var shipmentPointer = new DeclarationGoodsShipmentConsignmentTransportContractDocumentPointer();
			shipmentPointer.DocumentSectionCode = new PointerDocumentSectionCodeType();
			shipmentPointer.DocumentSectionCode.Value = "67A";
			equipmentPointers.Add(shipmentPointer);

			var consignmentPointer = new DeclarationGoodsShipmentConsignmentTransportContractDocumentPointer();
			consignmentPointer.DocumentSectionCode = new PointerDocumentSectionCodeType();
			consignmentPointer.DocumentSectionCode.Value = "28A";
			equipmentPointers.Add(consignmentPointer);

			int pointersCreated = 0;
			foreach (ZGuid packGroupPK in bill.RelatedEquipment)
			{
				var equipmentPointer = new DeclarationGoodsShipmentConsignmentTransportContractDocumentPointer();
				equipmentPointer.SequenceNumeric = GetContainerElementSequence(packGroupPK);
				equipmentPointer.DocumentSectionCode = new PointerDocumentSectionCodeType();
				equipmentPointer.DocumentSectionCode.Value = "31B";
				equipmentPointers.Add(equipmentPointer);
				pointersCreated++;
			}
		}

		void PopulatePackagingPointers(IAssociatedTransportDocument currentHB, Collection<DeclarationGoodsShipmentConsignmentTransportContractDocumentPointer> packagesPointers)
		{
			var decPointer = new DeclarationGoodsShipmentConsignmentTransportContractDocumentPointer();
			decPointer.DocumentSectionCode = new PointerDocumentSectionCodeType();
			decPointer.DocumentSectionCode.Value = "42A";
			packagesPointers.Add(decPointer);
			foreach (ZGuid packagingPK in currentHB.RelatedPackages)
			{
				var packagingPointer = new DeclarationGoodsShipmentConsignmentTransportContractDocumentPointer();
				packagingPointer.SequenceNumeric = GetPackagingElementSequence(packagingPK);
				packagingPointer.DocumentSectionCode = new PointerDocumentSectionCodeType();
				packagingPointer.DocumentSectionCode.Value = "93A";
				packagesPointers.Add(packagingPointer);
			}
		}

		#region Container Details

		Collection<DeclarationGoodsShipmentConsignmentTransportEquipment> PopulateContainerDetails()
		{
			var transportEquipment = new Collection<DeclarationGoodsShipmentConsignmentTransportEquipment>();
			var containers = new List<ITransportEquipment>(ContainersPointedTo.Values);
			var orderedContainers = containers.OrderBy(x => x.MessageSequence);
			foreach (ITransportEquipment equipment in orderedContainers)
			{
				var transEquipment = new DeclarationGoodsShipmentConsignmentTransportEquipment();
				transEquipment.SequenceNumeric = equipment.MessageSequence;
				transEquipment.CharacteristicCode = new TransportEquipmentCharacteristicCodeType();
				if (equipment.IsPallet)
				{
					transEquipment.CharacteristicCode.Value = ContainerSizeTypeList.Codes.C16;
				}
				else
				{
					transEquipment.CharacteristicCode.Value = equipment.Size;
					transEquipment.FullnessCode = new TransportEquipmentFullnessCodeType();
					transEquipment.FullnessCode.Value = equipment.Status;
				}

				transEquipment.Id = new TransportEquipmentIdentificationIdType();
				transEquipment.Id.Value = equipment.IsPallet ? (ZString)equipment.MessageSequence.ToString() : equipment.ContainerNumber;
				transEquipment.Pointer = PopulateEquipmentPointers(equipment);

				if (!equipment.IsPallet)
				{
					int equipmentSealsCount = 0;
					var equipmentSeals = new Collection<DeclarationGoodsShipmentConsignmentTransportEquipmentSeal>();
					foreach (ZString containerSeal in equipment.SealNumbers)
					{
						equipmentSealsCount++;
						var seal = new DeclarationGoodsShipmentConsignmentTransportEquipmentSeal();
						seal.SequenceNumeric = equipmentSealsCount;
						seal.Id = new SealIdentificationIdType();
						seal.Id.Value = containerSeal;
						equipmentSeals.Add(seal);
					}
					transEquipment.Seal = equipmentSeals;
				}

				transportEquipment.Add(transEquipment);
			}

			return transportEquipment;
		}

		Collection<DeclarationGoodsShipmentConsignmentTransportEquipmentPointer> PopulateEquipmentPointers(ITransportEquipment equipment)
		{
			var equipmentPointers = new Collection<DeclarationGoodsShipmentConsignmentTransportEquipmentPointer>();
			var decPointer = new DeclarationGoodsShipmentConsignmentTransportEquipmentPointer();
			decPointer.DocumentSectionCode = new PointerDocumentSectionCodeType();
			decPointer.DocumentSectionCode.Value = "42A";
			equipmentPointers.Add(decPointer);
			foreach (ZGuid packagingPK in equipment.RelatedPackages)
			{
				var equipmentPointer = new DeclarationGoodsShipmentConsignmentTransportEquipmentPointer();
				equipmentPointer.SequenceNumeric = GetPackagingElementSequence(packagingPK);
				equipmentPointer.DocumentSectionCode = new PointerDocumentSectionCodeType();
				equipmentPointer.DocumentSectionCode.Value = "93A";
				equipmentPointers.Add(equipmentPointer);
			}

			return equipmentPointers;
		}

		#endregion

		DeclarationGoodsShipmentConsignmentUnloadingLocation PopulatePortOfDischarge()
		{
			var unloadingLocation = new DeclarationGoodsShipmentConsignmentUnloadingLocation();
			unloadingLocation.Id = new UnloadingLocationIdentificationIdType();
			unloadingLocation.Id.Value = eX1Header.GoodsShipment.PortOfDischarge;
			return unloadingLocation;
		}

		#endregion

		DeclarationGoodsShipmentDeliveryDestination PopulateDeliveryParty()
		{
			var deliveryDestination = new DeclarationGoodsShipmentDeliveryDestination();
			deliveryDestination.Name = new DeliveryDestinationNameTextType();
			deliveryDestination.Name.Value = eX1Header.GoodsShipment.DeliverToParty.Name;
			deliveryDestination.Address = new DeclarationGoodsShipmentDeliveryDestinationAddress();
			deliveryDestination.Address.CityName = new AddressCityNameTextType();
			deliveryDestination.Address.CityName.Value = eX1Header.GoodsShipment.DeliverToParty.City;
			deliveryDestination.Address.CountryCode = new AddressCountryCodeType();
			deliveryDestination.Address.CountryCode.Value = eX1Header.GoodsShipment.DeliverToParty.CountryCode;
			deliveryDestination.Address.CountrySubDivisionName = new AddressCountrySubDivisionNameTextType();
			deliveryDestination.Address.CountrySubDivisionName.Value = eX1Header.GoodsShipment.DeliverToParty.CountryRegion;
			deliveryDestination.Address.Line = new AddressLineTextType();
			deliveryDestination.Address.Line.Value = eX1Header.GoodsShipment.DeliverToParty.Address;
			deliveryDestination.Address.PostcodeId = new AddressPostcodeIdType();
			deliveryDestination.Address.PostcodeId.Value = eX1Header.GoodsShipment.DeliverToParty.PostCode;
			return deliveryDestination;
		}

		DeclarationGoodsShipmentImporter PopulateImporter()
		{
			var importer = new DeclarationGoodsShipmentImporter();
			if (eX1Header.Importer != null)
			{
				importer.Name = new ImporterNameTextType();
				importer.Name.Value = eX1Header.Importer.Name;
				importer.Address = new DeclarationGoodsShipmentImporterAddress();
				importer.Address.CityName = new AddressCityNameTextType();
				importer.Address.CityName.Value = eX1Header.Importer.City;
				importer.Address.CountryCode = new AddressCountryCodeType();
				importer.Address.CountryCode.Value = eX1Header.Importer.CountryCode;
				importer.Address.CountrySubDivisionName = new AddressCountrySubDivisionNameTextType();
				importer.Address.CountrySubDivisionName.Value = eX1Header.Importer.CountryRegion;
				importer.Address.Line = new AddressLineTextType();
				importer.Address.Line.Value = eX1Header.Importer.Address;
				importer.Address.PostcodeId = new AddressPostcodeIdType();
				importer.Address.PostcodeId.Value = eX1Header.Importer.PostCode;
			}
			return importer;
		}

		Collection<DeclarationGoodsShipmentInvoice> PopulateInvoices()
		{
			int invoiceCount = 0;
			var invoices = new Collection<DeclarationGoodsShipmentInvoice>();
			foreach (IInvoice shipmentInvoice in eX1Header.GoodsShipment.Invoices)
			{
				invoiceCount++;
				var invoice = new DeclarationGoodsShipmentInvoice();
				invoice.IssueDateTime = new InvoiceIssueDateTimeType();
				invoice.IssueDateTime.Value = shipmentInvoice.InvoiceDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
				invoice.IssueDateTime.FormatCode = DateTimePeriodFormatCode.Item102;
				invoice.Id = new InvoiceIdentificationIdType();
				invoice.Id.Value = shipmentInvoice.InvoiceNumber;
				invoice.SequenceNumeric = invoiceCount;
				invoices.Add(invoice);
			}

			return invoices;
		}

		Collection<DeclarationGoodsShipmentNotifyParty> PopulateNotifyParties()
		{
			var notifyPartyList = new Collection<DeclarationGoodsShipmentNotifyParty>();
			foreach (IOrganisation shipmentNotifyParty in eX1Header.GoodsShipment.NotifyParties)
			{
				if (shipmentNotifyParty != null)
				{
					var name = shipmentNotifyParty.Name;
					if (!name.IsEmpty)
					{
						var notifyParty = new DeclarationGoodsShipmentNotifyParty();
						if (!name.IsEmpty)
						{
							notifyParty.Name = new NotifyPartyNameTextType();
							notifyParty.Name.Value = name;
						}

						notifyParty.RoleCode = new NotifyPartyRoleCodeType();
						notifyParty.RoleCode.Value = RoleCodeList.Codes.N2;
						notifyParty.Communication = new DeclarationGoodsShipmentNotifyPartyCommunication();
						foreach (ICommunication notifyPartyComms in shipmentNotifyParty.Communications)
						{
							if (notifyPartyComms.ContactType == CommunicationTypeList.Codes.EM)
							{
								notifyParty.Communication.Id = new CommunicationIdentificationIdType();
								notifyParty.Communication.Id.Value = notifyPartyComms.ContactDetail;
								notifyParty.Communication.TypeId = new CommunicationTypeIdType();
								notifyParty.Communication.TypeId.Value = CommunicationTypeList.Codes.EM;
								break;
							}
							break;
						}
						notifyPartyList.Add(notifyParty);
					}
				}
			}
			foreach (var notifyPartyCode in eX1Header.GoodsShipment.NotifyPartyCodes.Where(x => !x.IsEmpty))
			{
				notifyPartyList.Add(new DeclarationGoodsShipmentNotifyParty
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

		Collection<DeclarationGoodsShipmentStuffingEstablishment> PopulateStuffingEstablishments()
		{
			var stuffingEstablishment = new Collection<DeclarationGoodsShipmentStuffingEstablishment>();
			foreach (IOrganisation shipmentContainerPackLocation in eX1Header.GoodsShipment.StuffingEstablishments)
			{
				if (shipmentContainerPackLocation != null)
				{
					var containerPackLocation = new DeclarationGoodsShipmentStuffingEstablishment();
					containerPackLocation.Name = new StuffingEstablishmentNameTextType();
					containerPackLocation.Name.Value = shipmentContainerPackLocation.Name;
					containerPackLocation.Address = new DeclarationGoodsShipmentStuffingEstablishmentAddress();
					containerPackLocation.Address.CityName = new AddressCityNameTextType();
					containerPackLocation.Address.CityName.Value = shipmentContainerPackLocation.City;
					containerPackLocation.Address.CountryCode = new AddressCountryCodeType();
					containerPackLocation.Address.CountryCode.Value = shipmentContainerPackLocation.CountryCode;
					containerPackLocation.Address.CountrySubDivisionName = new AddressCountrySubDivisionNameTextType();
					containerPackLocation.Address.CountrySubDivisionName.Value = shipmentContainerPackLocation.CountryRegion;
					containerPackLocation.Address.Line = new AddressLineTextType();
					containerPackLocation.Address.Line.Value = shipmentContainerPackLocation.Address;
					containerPackLocation.Address.PostcodeId = new AddressPostcodeIdType();
					containerPackLocation.Address.PostcodeId.Value = shipmentContainerPackLocation.PostCode;
					stuffingEstablishment.Add(containerPackLocation);
				}
			}

			return stuffingEstablishment;
		}

		DeclarationGoodsShipmentWarehouse PopulateCustomsControlledArea()
		{
			var licensedPremise = new DeclarationGoodsShipmentWarehouse();
			licensedPremise.Id = new WarehouseIdentificationIdType();
			licensedPremise.Id.Value = eX1Header.GoodsShipment.CustomsControlledArea;
			return licensedPremise;
		}

		#endregion

		#endregion

		#region Government Agency Goods Item

		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItem> PopulateGoodsItems()
		{
			int itemSequence = 0;
			var items = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItem>();
			foreach (IGoodsItems lineDetail in eX1Header.GoodsShipment.Items)
			{
				itemSequence++;
				var item = PopulateItemDetails(lineDetail);
				item.SequenceNumeric = itemSequence;
				items.Add(item);
			}

			return items;
		}

		#region GAGI Methods
		//Government Agency Goods Item

		DeclarationGoodsShipmentGovernmentAgencyGoodsItem PopulateItemDetails(IGoodsItems lineDetail)
		{
			var item = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem();
			item.AdditionalDocument = PopulatePermits(lineDetail);
			item.AdditionalInformation = PopulateAdditionalInformation(lineDetail);
			item.Commodity = PopulateCommodityData(lineDetail);
			item.GoodsMeasure = PopulateMeasures(lineDetail);
			item.Origin = PopulateOrigin(lineDetail);
			item.Packaging = PopulateGAGIPackaging(lineDetail);
			return item;
		}

		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument> PopulatePermits(IGoodsItems lineDetail)
		{
			var linePermits = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument>();
			foreach (ZString permit in lineDetail.Permits)
			{
				var linePermit = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument();
				linePermit.Id = new AdditionalDocumentIdentificationIdType();
				linePermit.Id.Value = permit;
				linePermit.TypeCode = new AdditionalDocumentTypeCodeType();
				linePermit.TypeCode.Value = AdditionalDocumentTypeList.Codes.PER;
				linePermits.Add(linePermit);
			}

			return linePermits;
		}

		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation> PopulateAdditionalInformation(IGoodsItems lineDetail)
		{
			var lineAdditionalInfo = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation>();
			foreach (ZString prohibitedCode in lineDetail.ProhibitedCodes)
			{
				var lineProhibitedCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation();
				lineProhibitedCode.StatementCode = new AdditionalInformationStatementCodeType();
				lineProhibitedCode.StatementCode.Value = prohibitedCode;
				lineProhibitedCode.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
				lineProhibitedCode.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.PRO;
				lineAdditionalInfo.Add(lineProhibitedCode);
			}

			foreach (IOtherInfo otherInfoCode in lineDetail.OtherInfoCodes)
			{
				var lineOtherInfoCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation();
				lineOtherInfoCode.StatementCode = new AdditionalInformationStatementCodeType();
				lineOtherInfoCode.StatementCode.Value = otherInfoCode.Code;
				if (!otherInfoCode.Data.IsEmpty)
				{
					lineOtherInfoCode.StatementDescription = new AdditionalInformationStatementDescriptionTextType();
					lineOtherInfoCode.StatementDescription.Value = otherInfoCode.Data;
				}

				lineOtherInfoCode.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
				lineOtherInfoCode.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.OIN;
				lineAdditionalInfo.Add(lineOtherInfoCode);
			}

			return lineAdditionalInfo;
		}

		DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity PopulateCommodityData(IGoodsItems lineDetail)
		{
			var commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity();
			commodity.Description = PopulateItemDescription(lineDetail);
			commodity.ValueAmount = PopulateValueAmount(lineDetail);
			commodity.Classification = PopulateItemClassification(lineDetail);
			if (IsDrawbackEntry)
			{
				commodity.DutyTaxFee = PopulateLineDutyTaxFees(lineDetail);
			}

			if (lineDetail.Products != null)
			{
				commodity.Product = PopulateProducts(lineDetail);
			}

			commodity.ProductName = PopulateProductNames(lineDetail);
			commodity.ProductCharacteristics = PopulateProductCharacteristics(lineDetail);

			if (lineDetail.Temperatures != null)
			{
				commodity.Temperature = PopulateTemperatureData(lineDetail);
			}

			commodity.TransportEquipment = PopulateItemTransportEquipment(lineDetail);

			return commodity;
		}

		#region Item Commodity Details

		CommodityDescriptionTextType PopulateItemDescription(IGoodsItems lineDetail)
		{
			var itemCommodityDescription = new CommodityDescriptionTextType();
			itemCommodityDescription.Value = lineDetail.GoodsDescription.Replace("\r\n", " ").Replace("\r", "").Replace("\n", "").Left(250);
			return itemCommodityDescription;
		}

		CommodityValueAmountType PopulateValueAmount(IGoodsItems lineDetail)
		{
			var commodityValueAmount = new CommodityValueAmountType();
			commodityValueAmount.Value = lineDetail.ValueInForeignCurrency.Round(2);
			commodityValueAmount.CurrencyId = CurrencyID<Iso3AlphaCurrencyCodeContentType>(lineDetail.ForeignCurrencyCode);
			return commodityValueAmount;
		}

		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification> PopulateItemClassification(IGoodsItems lineDetail)
		{
			var classifications = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification>();
			foreach (IClassification tariff in lineDetail.Classifications)
			{
				var classification = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification();
				classification.Id = new ClassificationIdentificationIdType();
				classification.Id.Value = tariff.Classification;
				classification.IdentificationTypeCode = new ClassificationIdentificationTypeCodeType();
				classification.IdentificationTypeCode.Value = tariff.ClassificationTypeCode;
				classifications.Add(classification);
			}
			return classifications;
		}

		//Duty Tax Fee: Conditional – required for  type 41 drawbacks
		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee> PopulateLineDutyTaxFees(IGoodsItems lineDetail)
		{
			var dutyTaxFees = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee>();
			ZBool dutyFeeGenerated = false;
			ZBool gstFeeGenerated = false;

			foreach (IDutyTaxFee commodityDutyTaxFee in lineDetail.LineDutyTaxFees)
			{
				if (!dutyFeeGenerated)
				{
					dutyFeeGenerated = commodityDutyTaxFee.DutyTaxFeeType == DutyTaxFeeTypeList.Codes.CUD;
				}

				if (!gstFeeGenerated)
				{
					gstFeeGenerated = commodityDutyTaxFee.DutyTaxFeeType == DutyTaxFeeTypeList.Codes.GST;
				}

				dutyTaxFees.Add(ItemDutyTaxFee(commodityDutyTaxFee.DutyTaxFeeType, commodityDutyTaxFee.CurrencyCode, commodityDutyTaxFee.Amount));
			}

			if (!dutyFeeGenerated)
			{
				dutyTaxFees.Add(ItemDutyTaxFee(DutyTaxFeeTypeList.Codes.CUD, Core.Constants.CurrencyCodes.NewZealand, 0m));
			}

			if (!gstFeeGenerated)
			{
				dutyTaxFees.Add(ItemDutyTaxFee(DutyTaxFeeTypeList.Codes.GST, Core.Constants.CurrencyCodes.NewZealand, 0m));
			}

			return dutyTaxFees;
		}

		DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee ItemDutyTaxFee(string feeType, string currencyCode, ZDecimal amount)
		{
			var dutyTaxFee = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee();
			dutyTaxFee.TypeCode = new DutyTaxFeeTypeCodeType();
			dutyTaxFee.TypeCode.Value = feeType;
			dutyTaxFee.Payment = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFeePayment();
			dutyTaxFee.Payment.TaxAssessedAmount = new PaymentTaxAssessedAmountType();
			dutyTaxFee.Payment.TaxAssessedAmount.CurrencyId = CurrencyID<Iso3AlphaCurrencyCodeContentType>(currencyCode);
			dutyTaxFee.Payment.TaxAssessedAmount.Value = amount.Round(2);
			return dutyTaxFee;
		}

		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityProduct> PopulateProducts(IGoodsItems lineDetail)
		{
			var products = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityProduct>();
			foreach (IProduct lineProduct in lineDetail.Products)
			{
				if (!lineProduct.Id.IsEmpty)
				{
					var product = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityProduct();
					product.Id = new ProductIdentificationIdType();
					product.Id.Value = lineProduct.Id;
					product.IdentifierTypeCode = new ProductIdentifierTypeCodeType();
					product.IdentifierTypeCode.Value = lineProduct.IdType;
					products.Add(product);
				}
			}

			return products;
		}

		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityProductName> PopulateProductNames(IGoodsItems lineDetail)
		{
			var productNames = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityProductName>();
			if (!lineDetail.BrandName.IsEmpty)
			{
				productNames.Add(ProductName(lineDetail.BrandName, ProductNameTypeList.Codes.P223));
			}

			if (!lineDetail.CommonName.IsEmpty)
			{
				productNames.Add(ProductName(lineDetail.CommonName, ProductNameTypeList.Codes.P226));
			}

			if (!lineDetail.RegisteredName.IsEmpty)
			{
				productNames.Add(ProductName(lineDetail.RegisteredName, ProductNameTypeList.Codes.P55));
			}

			if (!lineDetail.TradeName.IsEmpty)
			{
				productNames.Add(ProductName(lineDetail.TradeName, ProductNameTypeList.Codes.P57));
			}

			return productNames;
		}

		DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityProductName ProductName(string productName, string type)
		{
			var commodityProductName = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityProductName();
			commodityProductName.Name = new ProductNameNameTextType();
			commodityProductName.Name.Value = productName;
			commodityProductName.NameQualifierCode = new ProductNameNameQualifierCodeType();
			commodityProductName.NameQualifierCode.Value = type;
			return commodityProductName;
		}

		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityProductCharacteristics> PopulateProductCharacteristics(IGoodsItems lineDetail)
		{
			var productCharacteristics = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityProductCharacteristics>();
			if (lineDetail.UsedGoods)
			{
				productCharacteristics.Add(ProductCharacteristic(CommodityCharacteristicTypeList.Codes.C61));
			}

			if (lineDetail.GeneticallyModified)
			{
				productCharacteristics.Add(ProductCharacteristic(CommodityCharacteristicTypeList.Codes.C211));
			}

			return productCharacteristics;
		}

		DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityProductCharacteristics ProductCharacteristic(string type)
		{
			var productCharacteristic = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityProductCharacteristics();
			productCharacteristic.CharacteristicQualifierCode = new ProductCharacteristicsCharacteristicQualifierCodeType();
			productCharacteristic.CharacteristicQualifierCode.Value = "Y";
			productCharacteristic.CharacteristicTypeCode = new ProductCharacteristicsCharacteristicTypeCodeType();
			productCharacteristic.CharacteristicTypeCode.Value = type;
			return productCharacteristic;
		}

		DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTemperature PopulateTemperatureData(IGoodsItems lineDetail)
		{
			var commodityTemperature = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTemperature();
			commodityTemperature.StorageRequirementMeasure = new TemperatureStorageRequirementMeasureType();
			commodityTemperature.StorageRequirementMeasure.Value = lineDetail.Temperatures.StorageTemp;
			commodityTemperature.StorageRequirementMeasure.UnitCode = MeasurementUnitCommonCodeContentType.Cel;
			commodityTemperature.MinimumStorageRequirementMeasure = new TemperatureMinimumStorageRequirementMeasureType();
			commodityTemperature.MinimumStorageRequirementMeasure.Value = lineDetail.Temperatures.MinStorageTemp;
			commodityTemperature.MinimumStorageRequirementMeasure.UnitCode = MeasurementUnitCommonCodeContentType.Cel;
			commodityTemperature.MaximumStorageRequirementMeasure = new TemperatureMaximumStorageRequirementMeasureType();
			commodityTemperature.MaximumStorageRequirementMeasure.Value = lineDetail.Temperatures.MaxStorageTemp;
			commodityTemperature.MaximumStorageRequirementMeasure.UnitCode = MeasurementUnitCommonCodeContentType.Cel;
			return commodityTemperature;
		}

		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTransportEquipment> PopulateItemTransportEquipment(IGoodsItems lineDetail)
		{
			/**		Baseline Version 1.1 Errata
			 **
			 **			Page 37, id 159 and 006 Cardinality GAGI /Commodity/Transport Equipment
			 **
			 **			This relates to the number of shipping containers or pallets able to be listed at detail line level.  In version 1.0, cardinality was specified as from zero to one (0…1).  It should be zero to many (0…*).  There is a corresponding omission of the associated sequence element 006
			 **
			 **				This error means only one container/pallet number can be listed, when multiple containers/pallets should be provided for.
			 **				This version 1.1 of the MIGs has been corrected, but the required change to the JBMS system cannot be made till Phase 3 (indicatively planned for release in February 2014).
			 **				Messaging software should enable the correct cardinality, but note that users cannot list more than one container till the change is made in JBMS.  In the interim, separate detail lines would be required if users need to differentiate which detail lines relate to which containers/pallets.
			 **				In addition there is currently no sequence element 006, so until the cardinality change is made the sequence must be omitted.
			 **
			 **/
			var containers = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTransportEquipment>();
			int containerSequence = 0;
			foreach (ZString container in lineDetail.ContainerNumbers)
			{
				containerSequence++;
				var itemCommodityTransportEquipment = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTransportEquipment();
				itemCommodityTransportEquipment.SequenceNumeric = containerSequence;
				itemCommodityTransportEquipment.Id = new TransportEquipmentIdentificationIdType();
				itemCommodityTransportEquipment.Id.Value = container;
				containers.Add(itemCommodityTransportEquipment);
				break;  // TODO: see Errata above
			}

			return containers;
		}

		#endregion

		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasure> PopulateMeasures(IGoodsItems lineDetail)
		{
			var goodsMeasure = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasure>();
			var lineMeasure = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasure();
			lineMeasure.GrossMassMeasure = new GoodsMeasureGrossMassMeasureType();
			lineMeasure.GrossMassMeasure.Value = lineDetail.ItemGrossWeightInKGM.Round(3);
			lineMeasure.GrossMassMeasure.UnitCode = MeasurementUnitCommonCodeContentType.Kgm;
			lineMeasure.NetNetWeightMeasure = new GoodsMeasureNetNetWeightMeasureType();
			lineMeasure.NetNetWeightMeasure.Value = lineDetail.ItemNetWeightInKGM;
			lineMeasure.NetNetWeightMeasure.UnitCode = MeasurementUnitCommonCodeContentType.Kgm;

			if (!lineDetail.StatisticalQtyUnit.IsEmpty)
			{
				lineMeasure.TariffQuantity = new GoodsMeasureTariffQuantityType();
				lineMeasure.TariffQuantity.Value = lineDetail.StatisticalQty;
				lineMeasure.TariffQuantity.UnitCode = new MeasurementUnitCommonCodeContentType();
				lineMeasure.TariffQuantity.UnitCode = MeasurementType<MeasurementUnitCommonCodeContentType>(lineDetail.StatisticalQtyUnit);
			}
			goodsMeasure.Add(lineMeasure);

			if (!lineDetail.SupplementaryQty.IsEmpty)
			{
				var supplementaryQty = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasure();
				supplementaryQty.TariffQuantity = new GoodsMeasureTariffQuantityType();
				supplementaryQty.TariffQuantity.Value = lineDetail.SupplementaryQty;
				supplementaryQty.TariffQuantity.UnitCode = new MeasurementUnitCommonCodeContentType();
				supplementaryQty.TariffQuantity.UnitCode = MeasurementType<MeasurementUnitCommonCodeContentType>(lineDetail.SupplementaryQtyUnit);
				goodsMeasure.Add(supplementaryQty);
			}

			return goodsMeasure;
		}

		DeclarationGoodsShipmentGovernmentAgencyGoodsItemOrigin PopulateOrigin(IGoodsItems lineDetail)
		{
			var itemOrigin = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemOrigin();
			itemOrigin.CountryCode = new OriginCountryCodeType();
			itemOrigin.CountryCode.Value = lineDetail.OriginCountry;
			return itemOrigin;
		}

		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemPackaging> PopulateGAGIPackaging(IGoodsItems lineDetail)
		{
			int packagingCount = 0;
			var packagingList = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemPackaging>();
			foreach (IPackaging linePackaging in lineDetail.Packaging.OrderBy(x => x.ShippingMarks))  // sorting just for a unit test to expect the packages in a certain order
			{
				packagingCount++;
				var packaging = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemPackaging();
				packaging.SequenceNumeric = packagingCount;
				packaging.MarksNumbersId = new PackagingMarksNumbersIdType();
				packaging.MarksNumbersId.Value = linePackaging.ShippingMarks;
				packaging.QuantityQuantity = new PackagingQuantityQuantityType();
				packaging.QuantityQuantity.Value = linePackaging.NumberOfPackages;
				packaging.TypeCode = new PackagingTypeCodeType();
				packaging.TypeCode.Value = linePackaging.PackageType;
				/*
				 * Before I forget – can you please make sure that the volume field is a whole number as we have a problem with our system when this field has decimal in it.
				 * I know the MIGs state that this should allow decimals but at the moment the system cannot handle this.
				 * Thank you and regards,
				 * 	Fleur Savage
				 */
				packaging.VolumeMeasure = new PackagingVolumeMeasureType();
				packaging.VolumeMeasure.Value = linePackaging.PackageVolumeInMTQ.Round(0);
				packaging.VolumeMeasure.UnitCode = MeasurementUnitCommonCodeContentType.Mtq;
				packagingList.Add(packaging);
			}
			return packagingList;
		}

		#endregion

		#endregion

		#endregion
	}
}
