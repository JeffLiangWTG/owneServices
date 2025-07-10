using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CargoWise.Customs.NZ.MessageDefinitions.TSW.Version1_1.DocumentMetadata;
using CargoWise.Customs.NZ.MessageDefinitions.TSW.Version1_1.IM1.Outgoing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders
{
	/// <summary>
	/// Import Declaration
	/// •	Submitted by
	///		o	Importers or their brokers
	///
	/// •	Data:
	///		o	Details of goods seeking import clearance
	///
	/// •	Used to effect import clearance on behalf of:
	///		o	Customs
	///		o	MAF Bio-security
	///		o	MAF Food
	///
	/// </summary>
	public class IM1MessageBuilder : TSWMessageBuilder<Declaration>
	{
		public IM1MessageBuilder(IImportDeclaration declarationHeader, TSWTransactionTypes transactionType)
			: base()
		{
			this.iM1Header = declarationHeader;
			this.transactionType = transactionType;
		}
		readonly IImportDeclaration iM1Header;
		readonly TSWTransactionTypes transactionType;

		public override ZString MessageType
		{
			get { return iM1Header.MessageType; }
		}

		bool IsPrimaryIndustriesImport
		{
			get { return MessageType == MessageTypeList.Codes.IPI; }
		}

		#region Message Declaration Defaults

		protected override WcoDocumentNameCode WCODocumentName
		{
			get { return WcoDocumentNameCode.Im; }
		}

		protected override NzDocumentNameCode NZDocumentName
		{
			get { return NzDocumentNameCode.Im1; }
		}

		public override ZBool DeclarantPinRequired
		{
			get { return true; }
		}

		public override ZString DeclarantPinEncrypted
		{
			get { return iM1Header.Declarant.DeclarantPinEncrypted; }
		}

		#endregion

		#region xml IM1 Declaration message

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
			if (transactionType != TSWTransactionTypes.Cancel)
			{
				if (iM1Header.IsSea || iM1Header.IsAir)
				{
					message.TotalGrossMassMeasure = PopulateGrossWeight();
				}

				message.JurisdictionDateTime = PopulateJurisdictionDate();
			}

			message.Submitter = PopulateSubmitter();
			message.AdditionalDocument = PopulateAdditionalDocs();
			message.AdditionalInformation = PopulateAdditionalInfo();
			if (transactionType != TSWTransactionTypes.Cancel)
			{
				message.Agent = PopulateAgent();
				if (!iM1Header.PremiseID.IsEmpty)
				{
					message.ApprovedEstablishmentPlace = PopulatePremiseID();
				}

				message.BorderTransportMeans = PopulateBorderTM();
				if (iM1Header.Carrier != null)
				{
					message.Carrier = PopulateCarrier();
				}

				if (!IsPrimaryIndustriesImport)
				{
					message.CurrencyExchange = PopulateCurrencies();
				}
			}

			if (!IsPrimaryIndustriesImport)
			{
				message.Declarant = PopulateDeclarant();
			}

			if (transactionType != TSWTransactionTypes.Cancel)
			{
				if (!IsPrimaryIndustriesImport)
				{
					message.DutyTaxFee = PopulateDutyTaxFees();
				}

				message.GoodsShipment = PopulateGoodsShipment();
				message.Importer = PopulateImporter();
				message.Packaging = PopulatePackaging();
				if (iM1Header.IsCompletionEntry)
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
			msgID.Value = iM1Header.TSWReferenceNumber;
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
			functionalReferenceID.Value = iM1Header.SenderReferenceNumber;
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
			totalGrossMassMeasure.Value = NZWeightHelper.ApplyWeightRounding(iM1Header.TotalGrossWeightInKGM);
			totalGrossMassMeasure.UnitCode = MeasurementUnitCommonCodeContentType.Kgm;
			return totalGrossMassMeasure;
		}

		DeclarationJurisdictionDateTimeType PopulateJurisdictionDate()
		{
			var jurisdictionDate = new DeclarationJurisdictionDateTimeType();
			if (iM1Header.IsPeriodicImport)
			{
				jurisdictionDate.Value = iM1Header.ImportPeriod.ToString("yyyyMM", CultureInfo.InvariantCulture);
				jurisdictionDate.FormatCode = DateTimePeriodFormatCode.Item610;
			}
			else
			{
				jurisdictionDate.Value = iM1Header.DateOfImport.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
				jurisdictionDate.FormatCode = DateTimePeriodFormatCode.Item102;
			}

			return jurisdictionDate;
		}

		DeclarationSubmitter PopulateSubmitter()
		{
			var submitter = new DeclarationSubmitter();
			submitter.Id = new SubmitterIdentificationIdType();
			submitter.Id.Value = iM1Header.SubmitterCode;
			return submitter;
		}

		Collection<DeclarationAdditionalDocument> PopulateAdditionalDocs()
		{
			var additionalDocs = new Collection<DeclarationAdditionalDocument>();
			foreach (ZString permit in iM1Header.Permits)
			{
				var refDoc = new DeclarationAdditionalDocument();
				refDoc.Id = new AdditionalDocumentIdentificationIdType();
				refDoc.Id.Value = permit;
				refDoc.TypeCode = new AdditionalDocumentTypeCodeType();
				refDoc.TypeCode.Value = AdditionalDocumentTypeList.Codes.PER;
				additionalDocs.Add(refDoc);
			}

			foreach (IOtherInfo referenceDocument in iM1Header.OtherReferencedDocuments)
			{
				var refDoc = new DeclarationAdditionalDocument();
				refDoc.Id = new AdditionalDocumentIdentificationIdType();
				refDoc.Id.Value = referenceDocument.Data;
				refDoc.TypeCode = new AdditionalDocumentTypeCodeType();
				refDoc.TypeCode.Value = referenceDocument.Code;
				additionalDocs.Add(refDoc);
			}

			if (iM1Header.AdditionalInformation != null)
			{
				foreach (ITSWAttachment attachment in iM1Header.AdditionalInformation.SupportingDocuments)
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
			if (iM1Header.AdditionalInformation != null)
			{
				if (transactionType == TSWTransactionTypes.Original || transactionType == TSWTransactionTypes.Replace || transactionType == TSWTransactionTypes.Completion)
				{
					if (!iM1Header.AdditionalInformation.FreeText.IsEmpty)
					{
						decAddInfo.Add(FreeTextInfo());
					}

					if (!iM1Header.AdditionalInformation.ManualOverrideText.IsEmpty)
					{
						decAddInfo.Add(ManualOverrideInfo());
					}

					foreach (IOtherInfo otherInfoDetails in iM1Header.OtherInfoCodes)
					{
						decAddInfo.Add(OtherInfoCodes(otherInfoDetails));
					}

					if (!iM1Header.HandlingInformation.IsEmpty)
					{
						decAddInfo.Add(HandlingInfo());
					}

					if (!iM1Header.MPIAccountDetails.IsEmpty)
					{
						decAddInfo.Add(MPIAccountInfo());
					}
				}

				if (transactionType == TSWTransactionTypes.Cancel || transactionType == TSWTransactionTypes.Replace)
				{
					decAddInfo.Add(ChangeCancelReason());
				}
			}

			return decAddInfo;
		}

		#region Addtional Information Details

		DeclarationAdditionalInformation FreeTextInfo()
		{
			var freeTextAddInfo = new DeclarationAdditionalInformation();
			freeTextAddInfo.Content = new AdditionalInformationContentTextType();
			freeTextAddInfo.Content.Value = iM1Header.AdditionalInformation.FreeText;
			return freeTextAddInfo;
		}

		DeclarationAdditionalInformation ManualOverrideInfo()
		{
			var manualOverrideAddInfo = new DeclarationAdditionalInformation();
			manualOverrideAddInfo.RequestOverrideCode = new AdditionalInformationRequestOverrideCodeType();
			manualOverrideAddInfo.RequestOverrideCode.Value = "Y";
			manualOverrideAddInfo.StatementDescription = new AdditionalInformationStatementDescriptionTextType();
			manualOverrideAddInfo.StatementDescription.Value = iM1Header.AdditionalInformation.ManualOverrideText; // "Must be present to advise the reason for override or manual processing";
			manualOverrideAddInfo.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
			manualOverrideAddInfo.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.ALP;
			return manualOverrideAddInfo;
		}

		DeclarationAdditionalInformation ChangeCancelReason()
		{
			var ccReasonAddInfo = new DeclarationAdditionalInformation();
			ccReasonAddInfo.StatementDescription = new AdditionalInformationStatementDescriptionTextType();
			ccReasonAddInfo.StatementDescription.Value = iM1Header.AdditionalInformation.AdditionalStatementText; //"Change/Replace/Cancel reason";
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
			handlingInfoAddInfo.StatementDescription.Value = iM1Header.HandlingInformation;
			handlingInfoAddInfo.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
			handlingInfoAddInfo.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.HAN;
			return handlingInfoAddInfo;
		}

		DeclarationAdditionalInformation MPIAccountInfo()
		{
			var mpiDetailsInfoAddInfo = new DeclarationAdditionalInformation();
			mpiDetailsInfoAddInfo.StatementDescription = new AdditionalInformationStatementDescriptionTextType();
			mpiDetailsInfoAddInfo.StatementDescription.Value = iM1Header.MPIAccountDetails;
			mpiDetailsInfoAddInfo.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
			mpiDetailsInfoAddInfo.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.MAC;
			return mpiDetailsInfoAddInfo;
		}

		#endregion

		DeclarationAgent PopulateAgent()
		{
			var agent = new DeclarationAgent();
			agent.Id = new AgentIdentificationIdType();
			agent.Id.Value = iM1Header.BrokerCode;
			agent.RoleCode = new AgentRoleCodeType();
			agent.RoleCode.Value = RoleCodeList.Codes.CB;
			return agent;
		}

		Collection<DeclarationApprovedEstablishmentPlace> PopulatePremiseID()
		{
			// specification states 0..*, but testing confirms only 1 ATF code is acceptable: **Error** in {Declaration/ApprovedEstablishmentPlace}:-There must be no more than one Transitional Facility
			// needs to be array however to match with xsd element structure
			var establishments = new Collection<DeclarationApprovedEstablishmentPlace>();
			var establishment = new DeclarationApprovedEstablishmentPlace();
			establishment.Id = new ApprovedEstablishmentPlaceIdentificationIdType();
			establishment.Id.Value = iM1Header.PremiseID;
			establishments.Add(establishment);
			return establishments;
		}

		DeclarationBorderTransportMeans PopulateBorderTM()
		{
			var boarderTM = new DeclarationBorderTransportMeans();
			if (iM1Header.IsSea || iM1Header.IsAir)
			{
				boarderTM.Name = new BorderTransportMeansNameTextType();
				boarderTM.Name.Value = iM1Header.IsSea ? iM1Header.CraftName.ToUpper() : iM1Header.FlightNo.ToUpper();
			}

			boarderTM.TypeCode = new BorderTransportMeansTypeCodeType();
			if (iM1Header.IsSea)
			{
				boarderTM.TypeCode.Value = TransportModeTypeList.Codes.T1;
				boarderTM.Id = new BorderTransportMeansIdentificationIdType();
				boarderTM.Id.Value = iM1Header.LloydsNo;
				boarderTM.JourneyId = new BorderTransportMeansJourneyIdType();
				boarderTM.JourneyId.Value = iM1Header.VoyageNo.ToUpper();
			}
			else if (iM1Header.IsAir)
			{
				boarderTM.TypeCode.Value = TransportModeTypeList.Codes.T4;
			}
			else
			{
				boarderTM.TypeCode.Value = TransportModeTypeList.Codes.T5;
			}

			return boarderTM;
		}

		DeclarationCarrier PopulateCarrier()
		{
			var carrier = new DeclarationCarrier();
			carrier.Name = new CarrierNameTextType();
			carrier.Name.Value = iM1Header.Carrier.Name;
			return carrier;
		}

		Collection<DeclarationCurrencyExchange> PopulateCurrencies()
		{
			var currencyExchanges = new Collection<DeclarationCurrencyExchange>();
			foreach (ICurrency currencyDetails in iM1Header.ExchangeRates)
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
			if (iM1Header.Declarant != null)
			{
				declarant.Id = new DeclarantIdentificationIdType();
				declarant.Id.Value = iM1Header.Declarant.DeclarantID;

				var declarantComms = new Collection<DeclarationDeclarantCommunication>();
				foreach (ICommunication comms in iM1Header.Declarant.Communications)
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

			// Where transmitted a separate instance must be sent for Payment Method
			if (!iM1Header.PaymentType.IsEmpty)
			{
				var dutyTaxFee = new DeclarationDutyTaxFee();
				dutyTaxFee.Payment = new DeclarationDutyTaxFeePayment();
				dutyTaxFee.Payment.MethodCode = new PaymentMethodCodeType();
				dutyTaxFee.Payment.MethodCode.Value = iM1Header.PaymentType;
				dutyTaxFees.Add(dutyTaxFee);
			}

			ZBool hasCustomsDuty = ChargesIncludesDuty();
			ZBool hasGST = ChargesIncludeGST();
			ZBool dutyFeeGenerated = false;
			ZBool gstFeeGenerated = false;
			ZBool totalFeeGenerated = false;

			foreach (IDutyTaxFee charge in iM1Header.DutyTaxFees)
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
			return iM1Header.DutyTaxFees.Cast<IDutyTaxFee>().FirstOrDefault(x => x.DutyTaxFeeType == DutyTaxFeeTypeList.Codes.CUD) != null;
		}

		ZBool ChargesIncludeGST()
		{
			return iM1Header.DutyTaxFees.Cast<IDutyTaxFee>().FirstOrDefault(x => x.DutyTaxFeeType == DutyTaxFeeTypeList.Codes.GST) != null;
		}

		#endregion

		DeclarationImporter PopulateImporter()
		{
			if (iM1Header.IsMiscImporter && iM1Header.MiscImporterAddress != null)
			{
				return MiscellaneousImporter();
			}
			else if (iM1Header.Importer != null)
			{
				return ImporterDetails();
			}
			return null;
		}

		DeclarationImporter ImporterDetails()
		{
			var importer = new DeclarationImporter();
			var registration = iM1Header.Importer.CustomsClientCode;
			if (registration.IsEmpty)
			{
				importer.Name = new ImporterNameTextType();
				importer.Name.Value = iM1Header.Importer.Name;
				importer.Address = new DeclarationImporterAddress();
				importer.Address.CityName = new AddressCityNameTextType();
				importer.Address.CityName.Value = iM1Header.Importer.City;
				importer.Address.CountryCode = new AddressCountryCodeType();
				importer.Address.CountryCode.Value = iM1Header.Importer.CountryCode;
				importer.Address.CountrySubDivisionName = new AddressCountrySubDivisionNameTextType();
				importer.Address.CountrySubDivisionName.Value = iM1Header.Importer.CountryRegion;
				importer.Address.Line = new AddressLineTextType();
				importer.Address.Line.Value = iM1Header.Importer.Address;
				importer.Address.PostcodeId = new AddressPostcodeIdType();
				importer.Address.PostcodeId.Value = iM1Header.Importer.PostCode;
			}
			else
			{
				importer.Id = new ImporterIdentificationIdType();
				importer.Id.Value = registration;
			}

			if (iM1Header.Importer.Communications.Any())
			{
				importer.Contact = new DeclarationImporterContact();
				importer.Contact.Name = new ContactNameTextType();
				string contactName = iM1Header.Importer.ContactPerson.IsEmpty ? "Unknown" : (string)iM1Header.Importer.ContactPerson;
				importer.Contact.Name.Value = contactName;

				if (iM1Header.Importer.Communications.Any())
				{
					var communications = new Collection<DeclarationImporterContactCommunication>();
					foreach (ICommunication comms in iM1Header.Importer.Communications)
					{
						var communication = new DeclarationImporterContactCommunication();
						communication.Id = new CommunicationIdentificationIdType();
						communication.Id.Value = comms.ContactDetail;
						communication.TypeId = new CommunicationTypeIdType();
						communication.TypeId.Value = comms.ContactType;
						communications.Add(communication);
					}
					importer.Contact.Communication = communications;
				}
			}

			return importer;
		}

		DeclarationImporter MiscellaneousImporter()
		{
			var importer = new DeclarationImporter();
			importer.Name = new ImporterNameTextType();
			importer.Name.Value = iM1Header.MiscImporterAddress.E2_CompanyName;
			importer.Address = new DeclarationImporterAddress();
			importer.Address.CityName = new AddressCityNameTextType();
			importer.Address.CityName.Value = iM1Header.MiscImporterAddress.E2_City;
			importer.Address.CountryCode = new AddressCountryCodeType();
			importer.Address.CountryCode.Value = iM1Header.MiscImporterAddress.E2_RN_NKCountryCode;
			importer.Address.CountrySubDivisionName = new AddressCountrySubDivisionNameTextType();
			importer.Address.CountrySubDivisionName.Value = iM1Header.MiscImporterAddress.E2_State;
			importer.Address.Line = new AddressLineTextType();
			importer.Address.Line.Value = iM1Header.MiscImporterAddress.E2_Address1 + " " + iM1Header.MiscImporterAddress.E2_Address2;
			importer.Address.PostcodeId = new AddressPostcodeIdType();
			importer.Address.PostcodeId.Value = iM1Header.MiscImporterAddress.E2_Postcode;

			if (!iM1Header.MiscImporterAddress.E2_Contact.IsEmpty)
			{
				importer.Contact = new DeclarationImporterContact();
				importer.Contact.Name = new ContactNameTextType();
				importer.Contact.Name.Value = iM1Header.MiscImporterAddress.E2_Contact;

				var communications = new Collection<DeclarationImporterContactCommunication>();
				PopulateContactCommunication(iM1Header.MiscImporterAddress.E2_Email, CommunicationTypeList.Codes.EM, communications);
				PopulateContactCommunication(iM1Header.MiscImporterAddress.E2_Phone, CommunicationTypeList.Codes.TE, communications);
				PopulateContactCommunication(iM1Header.MiscImporterAddress.E2_Mobile, CommunicationTypeList.Codes.AL, communications);
				PopulateContactCommunication(iM1Header.MiscImporterAddress.E2_Fax, CommunicationTypeList.Codes.FX, communications);
				if (communications.Any())
				{
					importer.Contact.Communication = communications;
				}
			}

			return importer;
		}

		void PopulateContactCommunication(ZString value, string type, Collection<DeclarationImporterContactCommunication> communications)
		{
			if (!value.IsEmpty)
			{
				var communication = new DeclarationImporterContactCommunication();
				communication.Id = new CommunicationIdentificationIdType();
				communication.Id.Value = value;
				communication.TypeId = new CommunicationTypeIdType();
				communication.TypeId.Value = type;
				communications.Add(communication);
			}
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
				packaging.TypeCode.Value = decPackaging.PackageType.IsEmpty ? "PK" : decPackaging.PackageType.ToString();
				declarationPackaging.Add(packaging);
			}

			return declarationPackaging;
		}

		Collection<DeclarationPreviousDocument> PopulatePreviousDocument()
		{
			var declarationPreviousDocument = new Collection<DeclarationPreviousDocument>();
			var previousDocument = new DeclarationPreviousDocument();
			previousDocument.Id = new PreviousDocumentIdentificationIdType();
			previousDocument.Id.Value = iM1Header.PreviousDocumentNo;
			previousDocument.TypeCode = new PreviousDocumentTypeCodeType();
			previousDocument.TypeCode.Value = iM1Header.PreviousDocumentType;
			declarationPreviousDocument.Add(previousDocument);
			return declarationPreviousDocument;
		}

		#endregion

		#region Goods Shipment

		DeclarationGoodsShipment PopulateGoodsShipment()
		{
			var shipment = new DeclarationGoodsShipment();
			shipment.ExportationCountryCode = PopulateShipmentOrigin();
			shipment.TransactionNatureCode = PopulateNatureOfTransaction();
			shipment.Consignment = PopulateConsignment();
			if (!IsPrimaryIndustriesImport)
			{
				shipment.CustomsValuation = PopulateCustomsValuation();
			}

			if (iM1Header.GoodsShipment.DeliverToParty != null)
			{
				if (iM1Header.GoodsShipment.DeliverToParty != iM1Header.Importer)
				{
					shipment.DeliveryDestination = PopulateDeliveryDestination();
				}
			}

			shipment.GovernmentAgencyGoodsItem = PopulateGoodsItems();
			shipment.Invoice = PopulateInvoices();
			shipment.NotifyParty = PopulateNotifyParty();
			shipment.Seller = PopulateSeller();
			if (iM1Header.IsSea)
			{
				shipment.StuffingEstablishment = PopulateStuffingEstablishment();
			}

			shipment.Supplier = PopulateSupplier();
			if (!iM1Header.GoodsShipment.CustomsControlledArea.IsEmpty)
			{
				shipment.Warehouse = PopulateWarehouse();
			}

			return shipment;
		}

		#region Goods Shipment Details

		GoodsShipmentExportationCountryCodeType PopulateShipmentOrigin()
		{
			var origin = new GoodsShipmentExportationCountryCodeType();
			origin.Value = iM1Header.GoodsShipment.ShipmentOrigin;
			return origin;
		}

		GoodsShipmentTransactionNatureCodeType PopulateNatureOfTransaction()
		{
			var transactionNatureCode = new GoodsShipmentTransactionNatureCodeType();
			transactionNatureCode.Value = iM1Header.GoodsShipment.NatureOfTransaction;
			return transactionNatureCode;
		}

		DeclarationGoodsShipmentConsignment PopulateConsignment()
		{
			var consignment = new DeclarationGoodsShipmentConsignment();
			if (iM1Header.IsContainerised)
			{
				consignment.AdditionalInformation = PopulateConsignmentAddInfo();
			}

			if (iM1Header.IsAir || iM1Header.IsMail || (iM1Header.IsSea && !iM1Header.GoodsShipment.LocationOfGoods.IsEmpty))
			{
				consignment.GoodsLocation = PopulateGoodsLocation();
			}

			consignment.LoadingLocation = PopulatePortOfLoading();
			StoreBillsPointedTo(iM1Header);
			StoreEquipmentPointedTo(iM1Header.Equipment);
			StorePackagingPointedTo(iM1Header);
			consignment.TransportContractDocument = PopulateBillDetails();
			if (iM1Header.HasContainersOrPallets)
			{
				consignment.TransportEquipment = PopulateTransportEquipmentList();
			}

			consignment.UnloadingLocation = PopulatePortOfDischarge();

			return consignment;
		}

		#region Populate Consignment Methods

		Collection<DeclarationGoodsShipmentConsignmentAdditionalInformation> PopulateConsignmentAddInfo()
		{
			var shipConsignAddInfo = new Collection<DeclarationGoodsShipmentConsignmentAdditionalInformation>();
			if (iM1Header.GoodsShipment.MAFContainerDeclaration)
			{
				foreach (ZString mafContainerStatement in iM1Header.GoodsShipment.MAFContainerStatements)
				{
					var scAddInfo = new DeclarationGoodsShipmentConsignmentAdditionalInformation();
					scAddInfo.StatementDescription = new AdditionalInformationStatementDescriptionTextType();
					scAddInfo.StatementDescription.Value = mafContainerStatement;
					scAddInfo.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
					scAddInfo.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.MCD;
					shipConsignAddInfo.Add(scAddInfo);
				}
			}

			foreach (ZString mpiApprovedSystemNumber in iM1Header.GoodsShipment.MPIApprovedSystemNumbers)
			{
				if (!mpiApprovedSystemNumber.IsEmpty)
				{
					var scAddInfo = new DeclarationGoodsShipmentConsignmentAdditionalInformation();
					scAddInfo.StatementDescription = new AdditionalInformationStatementDescriptionTextType();
					scAddInfo.StatementDescription.Value = mpiApprovedSystemNumber;
					scAddInfo.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
					scAddInfo.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.MAS;
					shipConsignAddInfo.Add(scAddInfo);
				}
			}

			return shipConsignAddInfo;
		}

		DeclarationGoodsShipmentConsignmentGoodsLocation PopulateGoodsLocation()
		{
			var goodsLocation = new DeclarationGoodsShipmentConsignmentGoodsLocation();
			goodsLocation.Id = new GoodsLocationIdentificationIdType();
			goodsLocation.Id.Value = iM1Header.GoodsShipment.LocationOfGoods;
			return goodsLocation;
		}

		DeclarationGoodsShipmentConsignmentLoadingLocation PopulatePortOfLoading()
		{
			var loadingLocation = new DeclarationGoodsShipmentConsignmentLoadingLocation();
			loadingLocation.Id = new LoadingLocationIdentificationIdType();
			loadingLocation.Id.Value = iM1Header.GoodsShipment.PortOfLoading;
			return loadingLocation;
		}

		#region Bill Details

		DeclarationGoodsShipmentConsignmentTransportContractDocument PopulateSingleBillDetails(IAssociatedTransportDocument bill)
		{
			var contractDoc = new DeclarationGoodsShipmentConsignmentTransportContractDocument();
			contractDoc.Id = new TransportContractDocumentIdentificationIdType();
			contractDoc.Id.Value = bill.BillNumber;
			contractDoc.TypeCode = new TransportContractDocumentTypeCodeType();
			contractDoc.TypeCode.Value = bill.BillType;

			var pointers = new Collection<DeclarationGoodsShipmentConsignmentTransportContractDocumentPointer>();

			if (iM1Header.MasterBills.Any() || iM1Header.IsMail)
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
			if (iM1Header.IsMail)
			{
				foreach (IAssociatedTransportDocument postBill in iM1Header.AllBills)
				{
					billNumbers.Add(PopulateSingleBillDetails(postBill));
				}
			}
			else if (iM1Header.MasterBills.Any())
			{
				foreach (IMasterBillTransportDocument bill in iM1Header.MasterBills)
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
				foreach (IAssociatedTransportDocument houseBill in iM1Header.AllBills)
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

		void PopulatePackagingPointers(IAssociatedTransportDocument bill, Collection<DeclarationGoodsShipmentConsignmentTransportContractDocumentPointer> packagesPointers)
		{
			var decPointer = new DeclarationGoodsShipmentConsignmentTransportContractDocumentPointer();
			decPointer.DocumentSectionCode = new PointerDocumentSectionCodeType();
			decPointer.DocumentSectionCode.Value = "42A";
			packagesPointers.Add(decPointer);
			foreach (ZGuid decPackagingPK in bill.RelatedPackages)
			{
				var equipmentPointer = new DeclarationGoodsShipmentConsignmentTransportContractDocumentPointer();
				equipmentPointer.SequenceNumeric = GetPackagingElementSequence(decPackagingPK);
				equipmentPointer.DocumentSectionCode = new PointerDocumentSectionCodeType();
				equipmentPointer.DocumentSectionCode.Value = "93A";
				packagesPointers.Add(equipmentPointer);
			}
		}

		#endregion

		#region Container Details

		Collection<DeclarationGoodsShipmentConsignmentTransportEquipment> PopulateTransportEquipmentList()
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
				transEquipment.Pointer = PopulateEquipmentPointers(equipment, false, iM1Header.GoodsShipment.StuffingEstablishments.Distinct().Take(2).Count() > 1);
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

		Collection<DeclarationGoodsShipmentConsignmentTransportEquipmentPointer> PopulateEquipmentPointers(ITransportEquipment equipment, bool multiTF, bool multiSE)
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

			if (multiTF)
			{ }

			if (multiSE)
			{
				var stuffingEstablishmentDecPointer = new DeclarationGoodsShipmentConsignmentTransportEquipmentPointer();
				stuffingEstablishmentDecPointer.DocumentSectionCode = new PointerDocumentSectionCodeType();
				stuffingEstablishmentDecPointer.DocumentSectionCode.Value = "42A";
				equipmentPointers.Add(stuffingEstablishmentDecPointer);

				var shipmentPointer = new DeclarationGoodsShipmentConsignmentTransportEquipmentPointer();
				shipmentPointer.DocumentSectionCode = new PointerDocumentSectionCodeType();
				shipmentPointer.DocumentSectionCode.Value = "67A";
				equipmentPointers.Add(shipmentPointer);

				var stuffingEstablishmentPointer = new DeclarationGoodsShipmentConsignmentTransportEquipmentPointer();
				stuffingEstablishmentPointer.SequenceNumeric = equipment.MessageSequence;
				stuffingEstablishmentPointer.DocumentSectionCode = new PointerDocumentSectionCodeType();
				stuffingEstablishmentPointer.DocumentSectionCode.Value = "16B";
				equipmentPointers.Add(stuffingEstablishmentPointer);
			}

			return equipmentPointers;
		}

		#endregion

		DeclarationGoodsShipmentConsignmentUnloadingLocation PopulatePortOfDischarge()
		{
			var unloadingLocation = new DeclarationGoodsShipmentConsignmentUnloadingLocation();
			unloadingLocation.Id = new UnloadingLocationIdentificationIdType();
			unloadingLocation.Id.Value = iM1Header.GoodsShipment.PortOfDischarge;
			return unloadingLocation;
		}

		#endregion

		DeclarationGoodsShipmentCustomsValuation PopulateCustomsValuation()
		{
			var customsValuation = new DeclarationGoodsShipmentCustomsValuation();
			customsValuation.FreightChargeAmount = new CustomsValuationFreightChargeAmountType();
			customsValuation.FreightChargeAmount.Value = iM1Header.GoodsShipment.FreightCostsInNZD.Round(0);
			customsValuation.FreightChargeAmount.CurrencyId = Iso3AlphaCurrencyCodeContentType.Nzd;
			customsValuation.FreightChargeApportionmentCode = new CustomsValuationFreightChargeApportionmentCodeType();
			customsValuation.FreightChargeApportionmentCode.Value = iM1Header.GoodsShipment.FreightApportionmentMethod;
			return customsValuation;
		}

		DeclarationGoodsShipmentDeliveryDestination PopulateDeliveryDestination()
		{
			var deliveryDestination = new DeclarationGoodsShipmentDeliveryDestination();
			deliveryDestination.Name = new DeliveryDestinationNameTextType();
			deliveryDestination.Name.Value = iM1Header.GoodsShipment.DeliverToParty.Name;
			deliveryDestination.Address = new DeclarationGoodsShipmentDeliveryDestinationAddress();
			deliveryDestination.Address.CityName = new AddressCityNameTextType();
			deliveryDestination.Address.CityName.Value = iM1Header.GoodsShipment.DeliverToParty.City;
			deliveryDestination.Address.CountryCode = new AddressCountryCodeType();
			deliveryDestination.Address.CountryCode.Value = iM1Header.GoodsShipment.DeliverToParty.CountryCode;
			deliveryDestination.Address.CountrySubDivisionName = new AddressCountrySubDivisionNameTextType();
			deliveryDestination.Address.CountrySubDivisionName.Value = iM1Header.GoodsShipment.DeliverToParty.CountryRegion;
			deliveryDestination.Address.Line = new AddressLineTextType();
			deliveryDestination.Address.Line.Value = iM1Header.GoodsShipment.DeliverToParty.Address;
			deliveryDestination.Address.PostcodeId = new AddressPostcodeIdType();
			deliveryDestination.Address.PostcodeId.Value = iM1Header.GoodsShipment.DeliverToParty.PostCode;
			return deliveryDestination;
		}

		#region Government Agency Goods Item

		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItem> PopulateGoodsItems()
		{
			int itemSequence = 0;
			var items = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItem>();
			foreach (IGoodsItems lineDetail in iM1Header.GoodsShipment.Items)
			{
				itemSequence++;
				var item = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem();
				item.SequenceNumeric = itemSequence;

				if (!IsPrimaryIndustriesImport)
				{
					item.CustomsValueAmount = PopulateItemCustomsValue(lineDetail);
				}

				item.AdditionalDocument = PopulatePermits(lineDetail);
				item.AdditionalInformation = PopulateLineAdditionalInfo(lineDetail);
				if (!lineDetail.TransitionalFacilityCode.IsEmpty)
				{
					item.ApprovedEstablishmentPlace = PopulateTransitionalFacility(lineDetail);
				}

				item.Commodity = PopulateItemCommodity(lineDetail);
				if (lineDetail.TreatmentProvider != null)
				{
					item.ExaminationPlace = PopulateTreatmentProvider(lineDetail);
				}

				item.GoodsMeasure = PopulateGoodsMeasure(lineDetail);
				item.Origin = PopulateItemOriginCountry(lineDetail);
				item.Packaging = PopulateItemPackaging(lineDetail);
				item.ValuationAdjustment = PopulateItemAdjustments(lineDetail);

				items.Add(item);
			}

			return items;
		}

		#region GAGI Details

		GovernmentAgencyGoodsItemCustomsValueAmountType PopulateItemCustomsValue(IGoodsItems lineDetail)
		{
			var customsValueAmount = new GovernmentAgencyGoodsItemCustomsValueAmountType();
			customsValueAmount.Value = lineDetail.ValueForDutyInNZD;
			customsValueAmount.CurrencyId = Iso3AlphaCurrencyCodeContentType.Nzd;
			return customsValueAmount;
		}

		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument> PopulatePermits(IGoodsItems lineDetail)
		{
			var linePermits = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument>();
			foreach (var permit in lineDetail.Permits)
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

		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation> PopulateLineAdditionalInfo(IGoodsItems lineDetail)
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

			if (!lineDetail.RelationshipIndicator.IsEmpty)
			{
				var relationshipInd = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation();
				relationshipInd.StatementCode = new AdditionalInformationStatementCodeType();
				relationshipInd.StatementCode.Value = lineDetail.RelationshipIndicator;
				relationshipInd.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
				relationshipInd.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.REL;
				relationshipInd.Pointer = PopulateRelationshipPointer(lineDetail);
				lineAdditionalInfo.Add(relationshipInd);
			}

			foreach (IOtherInfo otherInfoDetails in lineDetail.OtherInfoCodes)
			{
				if (otherInfoDetails != null)
				{
					var otherInfoCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation();
					otherInfoCode.StatementCode = new AdditionalInformationStatementCodeType();
					otherInfoCode.StatementCode.Value = otherInfoDetails.Code;
					if (!otherInfoDetails.Data.IsEmpty)
					{
						otherInfoCode.StatementDescription = new AdditionalInformationStatementDescriptionTextType();
						otherInfoCode.StatementDescription.Value = otherInfoDetails.Data;
					}

					otherInfoCode.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
					otherInfoCode.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.OIN;
					lineAdditionalInfo.Add(otherInfoCode);
				}
			}

			if (!lineDetail.IsGSTPrePaid.IsEmpty || !lineDetail.VendorIdentifier.IsEmpty)   // Note: OSR and OSP must be used in conjunction – if one provided but not the other a consignment level error will be given.
			{
				var gstPrepaid = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation();
				gstPrepaid.StatementCode = new AdditionalInformationStatementCodeType() { Value = AdditionalStatementTypeList.Codes.OSP };
				gstPrepaid.StatementDescription = new AdditionalInformationStatementDescriptionTextType() { Value = lineDetail.IsGSTPrePaid };
				gstPrepaid.StatementTypeCode = new AdditionalInformationStatementTypeCodeType() { Value = AdditionalStatementTypeList.Codes.OIN };
				lineAdditionalInfo.Add(gstPrepaid);

				var consignorGSTNo = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation();
				consignorGSTNo.StatementCode = new AdditionalInformationStatementCodeType() { Value = AdditionalStatementTypeList.Codes.OSR };
				consignorGSTNo.StatementDescription = new AdditionalInformationStatementDescriptionTextType() { Value = lineDetail.VendorIdentifier };
				consignorGSTNo.StatementTypeCode = new AdditionalInformationStatementTypeCodeType() { Value = AdditionalStatementTypeList.Codes.OIN };
				lineAdditionalInfo.Add(consignorGSTNo);
			}

			return lineAdditionalInfo;
		}

		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformationPointer> PopulateRelationshipPointer(IGoodsItems lineDetail)
		{
			var relationshipPointers = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformationPointer>();
			var decPointer = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformationPointer();
			decPointer.DocumentSectionCode = new PointerDocumentSectionCodeType();
			decPointer.DocumentSectionCode.Value = "42A";
			relationshipPointers.Add(decPointer);

			var shipmentPointer = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformationPointer();
			shipmentPointer.DocumentSectionCode = new PointerDocumentSectionCodeType();
			shipmentPointer.DocumentSectionCode.Value = "67A";
			relationshipPointers.Add(shipmentPointer);

			var relationshipIndicatorPointer = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformationPointer();
			relationshipIndicatorPointer.SequenceNumeric = lineDetail.SupplierLineIsRelatedTo;
			relationshipIndicatorPointer.DocumentSectionCode = new PointerDocumentSectionCodeType();
			relationshipIndicatorPointer.DocumentSectionCode.Value = "18B";
			relationshipPointers.Add(relationshipIndicatorPointer);
			return relationshipPointers;
		}

		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemApprovedEstablishmentPlace> PopulateTransitionalFacility(IGoodsItems lineDetail)
		{
			var facilityCode = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemApprovedEstablishmentPlace>();
			var transitionalFacilityCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemApprovedEstablishmentPlace();
			transitionalFacilityCode.Id = new ApprovedEstablishmentPlaceIdentificationIdType();
			transitionalFacilityCode.Id.Value = lineDetail.TransitionalFacilityCode;
			facilityCode.Add(transitionalFacilityCode);
			return facilityCode;
		}

		#region Item Commodity

		DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity PopulateItemCommodity(IGoodsItems lineDetail)
		{
			var itemCommodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity();
			itemCommodity.Description = PopulateItemDescription(lineDetail);
			if (!lineDetail.LotNumber.IsEmpty)
			{
				itemCommodity.LotNumberId = PopulateItemLotNumber(lineDetail);
			}

			if (!lineDetail.DateMarking.IsEmpty)
			{
				itemCommodity.ProductBestBeforeDateTime = PopulateItemBestBeforeDate(lineDetail);
			}

			if (!IsPrimaryIndustriesImport)
			{
				itemCommodity.ValueAmount = PopulateItemForeignValue(lineDetail);
			}

			if (!lineDetail.IntendedUseCode.IsEmpty)
			{
				itemCommodity.IntendedUseCode = PopulateItemIntendedUseCode(lineDetail);
			}
			else if (!lineDetail.IntendedUse.IsEmpty)
			{
				itemCommodity.IntendedUse = PopulateItemIntendedUse(lineDetail);
			}

			itemCommodity.Classification = PopulateItemClassification(lineDetail);
			itemCommodity.Constituent = PopulateItemConstituents(lineDetail);
			if (!IsPrimaryIndustriesImport)
			{
				itemCommodity.DutyTaxFee = PopulateItemDuties(lineDetail);
			}

			if (lineDetail.Grower != null)
			{
				itemCommodity.Grower = PopulateItemGrower(lineDetail);
			}

			if (lineDetail.RoutingCountryCodes != null)
			{
				itemCommodity.Itinerary = PopulateItemItinerary(lineDetail);
			}

			if (lineDetail.Manufacturer != null) // && lineDetail.Manufacturer != IM1Header.GoodsShipment.Supplier)
			{
				itemCommodity.Manufacturer = PopulateItemManufacturer(lineDetail);
			}

			if (lineDetail.Producer != null) // && lineDetail.Producer != IM1Header.GoodsShipment.Supplier)
			{
				itemCommodity.Producer = PopulateItemProducer(lineDetail);
			}

			if (lineDetail.Products != null)
			{
				itemCommodity.Product = PopulateProducts(lineDetail);
			}

			itemCommodity.ProductName = PopulateProductNames(lineDetail);
			itemCommodity.ProductCharacteristics = PopulateProductCharacteristics(lineDetail);
			itemCommodity.Source = PopulateItemExportCountry(lineDetail);
			if (lineDetail.Temperatures != null)
			{
				itemCommodity.Temperature = PopulateItemTemperatures(lineDetail);
			}

			itemCommodity.TransportEquipment = PopulateItemTransportEquipment(lineDetail);

			return itemCommodity;
		}

		#region Item Commodity Details

		CommodityDescriptionTextType PopulateItemDescription(IGoodsItems lineDetail)
		{
			var itemCommodityDescription = new CommodityDescriptionTextType();
			itemCommodityDescription.Value = lineDetail.GoodsDescription.Replace("\r\n", " ").Replace("\r", "").Replace("\n", "").Left(250);
			return itemCommodityDescription;
		}

		CommodityLotNumberIdType PopulateItemLotNumber(IGoodsItems lineDetail)
		{
			var itemCommodityLotNumberID = new CommodityLotNumberIdType();
			itemCommodityLotNumberID.Value = lineDetail.LotNumber;
			return itemCommodityLotNumberID;
		}

		CommodityProductBestBeforeDateTimeType PopulateItemBestBeforeDate(IGoodsItems lineDetail)
		{
			var itemCommodityProductBestBeforeDateTime = new CommodityProductBestBeforeDateTimeType();
			itemCommodityProductBestBeforeDateTime.Value = lineDetail.DateMarking.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
			itemCommodityProductBestBeforeDateTime.FormatCode = DateTimePeriodFormatCode.Item102;
			return itemCommodityProductBestBeforeDateTime;
		}

		CommodityValueAmountType PopulateItemForeignValue(IGoodsItems lineDetail)
		{
			var itemCommodityValueAmount = new CommodityValueAmountType();
			if (lineDetail.HasForeignCurrency)
			{
				itemCommodityValueAmount.Value = lineDetail.ValueInForeignCurrency.Round(2);
				itemCommodityValueAmount.CurrencyId = CurrencyID<Iso3AlphaCurrencyCodeContentType>(lineDetail.ForeignCurrencyCode);
			}
			else
			{
				itemCommodityValueAmount.Value = lineDetail.ValueForDutyInNZD.Round(2);
				itemCommodityValueAmount.CurrencyId = Iso3AlphaCurrencyCodeContentType.Nzd;
			}
			return itemCommodityValueAmount;
		}

		CommodityIntendedUseCodeType PopulateItemIntendedUseCode(IGoodsItems lineDetail)
		{
			var itemCommodityIntendedUseCode = new CommodityIntendedUseCodeType();
			itemCommodityIntendedUseCode.Value = lineDetail.IntendedUseCode;
			return itemCommodityIntendedUseCode;
		}

		CommodityIntendedUseTextType PopulateItemIntendedUse(IGoodsItems lineDetail)
		{
			var itemCommodityIntendedUse = new CommodityIntendedUseTextType();
			itemCommodityIntendedUse.Value = lineDetail.IntendedUse.Replace("\r\n", " ").Replace("\r", "").Replace("\n", "");
			return itemCommodityIntendedUse;
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

		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituent> PopulateItemConstituents(IGoodsItems lineDetail)
		{
			var constituentDetails = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituent>();
			if (lineDetail.Constituents != null)
			{
				foreach (IConstituent constituent in lineDetail.Constituents)
				{
					if (constituent != null)
					{
						var constituentDetail = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituent();
						constituentDetail.ElementQuantity = new ConstituentElementQuantityType();
						constituentDetail.ElementQuantity.Value = constituent.ConstituentQuantity;
						constituentDetail.ElementName = new ConstituentElementNameTextType();
						constituentDetail.ElementName.Value = constituent.ConstituentName;
						constituentDetails.Add(constituentDetail);
					}
				}
			}

			return constituentDetails;
		}

		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee> PopulateItemDuties(IGoodsItems lineDetail)
		{
			var dutyTaxFees = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee>();
			if (!lineDetail.PreferenceClaimed.IsEmpty)
			{
				var dutyTaxFee = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee();
				dutyTaxFee.DutyRegimeCode = new DutyTaxFeeDutyRegimeCodeType();
				dutyTaxFee.DutyRegimeCode.Value = lineDetail.PreferenceClaimed;
				dutyTaxFees.Add(dutyTaxFee);
			}

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

		DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGrower PopulateItemGrower(IGoodsItems lineDetail)
		{
			var itemCommodityGrower = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGrower();
			itemCommodityGrower.Name = new GrowerNameTextType();
			itemCommodityGrower.Name.Value = lineDetail.Grower.Name;
			itemCommodityGrower.Address = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGrowerAddress();
			itemCommodityGrower.Address.CityName = new AddressCityNameTextType();
			itemCommodityGrower.Address.CityName.Value = lineDetail.Grower.City;
			itemCommodityGrower.Address.CountryCode = new AddressCountryCodeType();
			itemCommodityGrower.Address.CountryCode.Value = lineDetail.Grower.CountryCode;
			itemCommodityGrower.Address.CountrySubDivisionName = new AddressCountrySubDivisionNameTextType();
			itemCommodityGrower.Address.CountrySubDivisionName.Value = lineDetail.Grower.CountryRegion;
			itemCommodityGrower.Address.Line = new AddressLineTextType();
			itemCommodityGrower.Address.Line.Value = lineDetail.Grower.Address;
			itemCommodityGrower.Address.PostcodeId = new AddressPostcodeIdType();
			itemCommodityGrower.Address.PostcodeId.Value = lineDetail.Grower.PostCode;
			return itemCommodityGrower;
		}

		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityItinerary> PopulateItemItinerary(IGoodsItems lineDetail)
		{
			int routingcountryCount = 0;
			var routingCountries = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityItinerary>();
			foreach (ZString countryCode in lineDetail.RoutingCountryCodes)
			{
				routingcountryCount++;
				var routingCountry = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityItinerary();
				routingCountry.SequenceNumeric = routingcountryCount;
				routingCountry.RoutingCountryCode = new ItineraryRoutingCountryCodeType();
				routingCountry.RoutingCountryCode.Value = countryCode;
				routingCountries.Add(routingCountry);
			}

			return routingCountries;
		}

		DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityManufacturer PopulateItemManufacturer(IGoodsItems lineDetail)
		{
			var itemCommodityManufacturer = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityManufacturer();
			itemCommodityManufacturer.Name = new ManufacturerNameTextType();
			itemCommodityManufacturer.Name.Value = lineDetail.Manufacturer.Name;
			itemCommodityManufacturer.Address = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityManufacturerAddress();
			itemCommodityManufacturer.Address.CityName = new AddressCityNameTextType();
			itemCommodityManufacturer.Address.CityName.Value = lineDetail.Manufacturer.City;
			itemCommodityManufacturer.Address.CountryCode = new AddressCountryCodeType();
			itemCommodityManufacturer.Address.CountryCode.Value = lineDetail.Manufacturer.CountryCode;
			itemCommodityManufacturer.Address.CountrySubDivisionName = new AddressCountrySubDivisionNameTextType();
			itemCommodityManufacturer.Address.CountrySubDivisionName.Value = lineDetail.Manufacturer.CountryRegion;
			itemCommodityManufacturer.Address.Line = new AddressLineTextType();
			itemCommodityManufacturer.Address.Line.Value = lineDetail.Manufacturer.Address;
			itemCommodityManufacturer.Address.PostcodeId = new AddressPostcodeIdType();
			itemCommodityManufacturer.Address.PostcodeId.Value = lineDetail.Manufacturer.PostCode;
			return itemCommodityManufacturer;
		}

		DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityProducer PopulateItemProducer(IGoodsItems lineDetail)
		{
			var itemCommodityProducer = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityProducer();
			itemCommodityProducer.Name = new ProducerNameTextType();
			itemCommodityProducer.Name.Value = lineDetail.Producer.Name;
			itemCommodityProducer.Address = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityProducerAddress();
			itemCommodityProducer.Address.CityName = new AddressCityNameTextType();
			itemCommodityProducer.Address.CityName.Value = lineDetail.Producer.City;
			itemCommodityProducer.Address.CountryCode = new AddressCountryCodeType();
			itemCommodityProducer.Address.CountryCode.Value = lineDetail.Producer.CountryCode;
			itemCommodityProducer.Address.CountrySubDivisionName = new AddressCountrySubDivisionNameTextType();
			itemCommodityProducer.Address.CountrySubDivisionName.Value = lineDetail.Producer.CountryRegion;
			itemCommodityProducer.Address.Line = new AddressLineTextType();
			itemCommodityProducer.Address.Line.Value = lineDetail.Producer.Address;
			itemCommodityProducer.Address.PostcodeId = new AddressPostcodeIdType();
			itemCommodityProducer.Address.PostcodeId.Value = lineDetail.Producer.PostCode;
			return itemCommodityProducer;
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
			productCharacteristic.CharacteristicTypeCode = new ProductCharacteristicsCharacteristicTypeCodeType();
			productCharacteristic.CharacteristicTypeCode.Value = "Y";
			productCharacteristic.CharacteristicQualifierCode = new ProductCharacteristicsCharacteristicQualifierCodeType();
			productCharacteristic.CharacteristicQualifierCode.Value = type;
			return productCharacteristic;
		}

		DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommoditySource PopulateItemExportCountry(IGoodsItems lineDetail)
		{
			var countryCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommoditySource();
			countryCode.CountryCode = new SourceCountryCodeType();
			countryCode.CountryCode.Value = lineDetail.ExportCountry;
			return countryCode;
		}

		DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTemperature PopulateItemTemperatures(IGoodsItems lineDetail)
		{
			var itemCommodityTemperature = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTemperature();
			itemCommodityTemperature.StorageRequirementMeasure = new TemperatureStorageRequirementMeasureType();
			itemCommodityTemperature.StorageRequirementMeasure.Value = lineDetail.Temperatures.StorageTemp.Round(0);
			itemCommodityTemperature.StorageRequirementMeasure.UnitCode = MeasurementUnitCommonCodeContentType.Cel;
			itemCommodityTemperature.MinimumStorageRequirementMeasure = new TemperatureMinimumStorageRequirementMeasureType();
			itemCommodityTemperature.MinimumStorageRequirementMeasure.Value = lineDetail.Temperatures.MinStorageTemp.Round(0);
			itemCommodityTemperature.MinimumStorageRequirementMeasure.UnitCode = MeasurementUnitCommonCodeContentType.Cel;
			itemCommodityTemperature.MaximumStorageRequirementMeasure = new TemperatureMaximumStorageRequirementMeasureType();
			itemCommodityTemperature.MaximumStorageRequirementMeasure.Value = lineDetail.Temperatures.MaxStorageTemp.Round(0);
			itemCommodityTemperature.MaximumStorageRequirementMeasure.UnitCode = MeasurementUnitCommonCodeContentType.Cel;
			return itemCommodityTemperature;
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

		#endregion

		DeclarationGoodsShipmentGovernmentAgencyGoodsItemExaminationPlace PopulateTreatmentProvider(IGoodsItems lineDetail)
		{
			var examinationPlace = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemExaminationPlace();
			examinationPlace.Name = new ExaminationPlaceNameTextType();
			examinationPlace.Name.Value = lineDetail.TreatmentProvider.Name;
			examinationPlace.Id = new ExaminationPlaceIdentificationIdType();
			examinationPlace.Id.Value = lineDetail.TreatmentProvider.CustomsClientCode;
			return examinationPlace;
		}

		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasure> PopulateGoodsMeasure(IGoodsItems lineDetail)
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

		DeclarationGoodsShipmentGovernmentAgencyGoodsItemOrigin PopulateItemOriginCountry(IGoodsItems lineDetail)
		{
			var itemOrigin = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemOrigin();
			itemOrigin.CountryCode = new OriginCountryCodeType();
			itemOrigin.CountryCode.Value = lineDetail.OriginCountry;
			if (!lineDetail.OriginRegion.IsEmpty)
			{
				itemOrigin.RegionId = new OriginRegionIdType();
				itemOrigin.RegionId.Value = lineDetail.OriginRegion;
			}

			return itemOrigin;
		}

		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemPackaging> PopulateItemPackaging(IGoodsItems lineDetail)
		{
			int packagingCount = 0;
			var packagingList = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemPackaging>();
			foreach (IPackaging linePackaging in lineDetail.Packaging)
			{
				packagingCount++;
				var packaging = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemPackaging();
				packaging.SequenceNumeric = packagingCount;
				packaging.MarksNumbersId = new PackagingMarksNumbersIdType();
				packaging.MarksNumbersId.Value = linePackaging.ShippingMarks;
				packaging.QuantityQuantity = new PackagingQuantityQuantityType();
				packaging.QuantityQuantity.Value = linePackaging.NumberOfPackages;
				packaging.TypeCode = new PackagingTypeCodeType();
				packaging.TypeCode.Value = linePackaging.PackageType.IsEmpty ? "PK" : linePackaging.PackageType.ToString();
				if (!linePackaging.PackingMaterialDesc.IsEmpty)
				{
					packaging.PackingMaterialDescription = new PackagingPackingMaterialDescriptionTextType();
					packaging.PackingMaterialDescription.Value = linePackaging.PackingMaterialDesc;
				}
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

		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemValuationAdjustment> PopulateItemAdjustments(IGoodsItems lineDetail)
		{
			var adjustments = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemValuationAdjustment>();
			foreach (IValuationAdjustment lineAdjustment in lineDetail.Adjustments)
			{
				var adjustment = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemValuationAdjustment();
				adjustment.AdditionCode = new ValuationAdjustmentAdditionCodeType();
				adjustment.AdditionCode.Value = lineAdjustment.AdjustmentQualifier;
				adjustment.AmountAmount = new ValuationAdjustmentAmountAmountType();
				adjustment.AmountAmount.Value = lineAdjustment.AdjustmentAmountInNZD;
				adjustment.AmountAmount.CurrencyId = Iso3AlphaCurrencyCodeContentType.Nzd;
				adjustments.Add(adjustment);
			}

			return adjustments;
		}

		#endregion

		#endregion

		Collection<DeclarationGoodsShipmentInvoice> PopulateInvoices()
		{
			int invoiceCount = 0;
			var invoices = new Collection<DeclarationGoodsShipmentInvoice>();
			foreach (IInvoice shipmentInvoice in iM1Header.GoodsShipment.Invoices)
			{
				invoiceCount++;
				var invoice = new DeclarationGoodsShipmentInvoice();
				invoice.SequenceNumeric = invoiceCount;
				invoice.Id = new InvoiceIdentificationIdType();
				invoice.Id.Value = shipmentInvoice.InvoiceNumber;
				invoice.ConditionCode = new InvoiceConditionCodeType();
				invoice.ConditionCode.Value = shipmentInvoice.IncoTerms;
				invoices.Add(invoice);
			}

			return invoices;
		}

		Collection<DeclarationGoodsShipmentNotifyParty> PopulateNotifyParty()
		{
			var notifyPartyList = new Collection<DeclarationGoodsShipmentNotifyParty>();
			foreach (IOrganisation shipmentNotifyParty in iM1Header.GoodsShipment.NotifyParties)
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
			foreach (var notifyPartyCode in iM1Header.GoodsShipment.NotifyPartyCodes.Where(x => !x.IsEmpty))
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

		Collection<DeclarationGoodsShipmentSeller> PopulateSeller()
		{
			var sellerList = new Collection<DeclarationGoodsShipmentSeller>();
			foreach (IOrganisation shipmentSeller in iM1Header.GoodsShipment.Sellers)
			{
				if (shipmentSeller != null)
				{
					var seller = new DeclarationGoodsShipmentSeller();
					seller.Name = new SellerNameTextType();
					seller.Name.Value = shipmentSeller.Name;
					seller.Address = new DeclarationGoodsShipmentSellerAddress();
					seller.Address.CityName = new AddressCityNameTextType();
					seller.Address.CityName.Value = shipmentSeller.City;
					seller.Address.CountryCode = new AddressCountryCodeType();
					seller.Address.CountryCode.Value = shipmentSeller.CountryCode;
					seller.Address.CountrySubDivisionName = new AddressCountrySubDivisionNameTextType();
					seller.Address.CountrySubDivisionName.Value = shipmentSeller.CountryRegion;
					seller.Address.Line = new AddressLineTextType();
					seller.Address.Line.Value = shipmentSeller.Address;
					seller.Address.PostcodeId = new AddressPostcodeIdType();
					seller.Address.PostcodeId.Value = shipmentSeller.PostCode;
					sellerList.Add(seller);
				}
			}

			return sellerList;
		}

		Collection<DeclarationGoodsShipmentStuffingEstablishment> PopulateStuffingEstablishment()
		{
			var stuffingEstablishment = new Collection<DeclarationGoodsShipmentStuffingEstablishment>();
			foreach (IOrganisation shipmentContainerPackLocation in iM1Header.GoodsShipment.StuffingEstablishments)
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

		Collection<DeclarationGoodsShipmentSupplier> PopulateSupplier()
		{
			var supplierList = new Collection<DeclarationGoodsShipmentSupplier>();
			foreach (IOrganisation shipmentSupplier in iM1Header.GoodsShipment.Suppliers)
			{
				if (shipmentSupplier != null)
				{
					var supplier = new DeclarationGoodsShipmentSupplier();
					var registration = shipmentSupplier.CustomsSupplierCode;
					if (registration.IsEmpty)
					{
						supplier.Name = new SupplierNameTextType();
						supplier.Name.Value = shipmentSupplier.Name;
						supplier.Address = new DeclarationGoodsShipmentSupplierAddress();
						supplier.Address.CityName = new AddressCityNameTextType();
						supplier.Address.CityName.Value = shipmentSupplier.City;
						supplier.Address.CountryCode = new AddressCountryCodeType();
						supplier.Address.CountryCode.Value = shipmentSupplier.CountryCode;
						supplier.Address.CountrySubDivisionName = new AddressCountrySubDivisionNameTextType();
						supplier.Address.CountrySubDivisionName.Value = shipmentSupplier.CountryRegion;
						supplier.Address.Line = new AddressLineTextType();
						supplier.Address.Line.Value = shipmentSupplier.Address;
						supplier.Address.PostcodeId = new AddressPostcodeIdType();
						supplier.Address.PostcodeId.Value = shipmentSupplier.PostCode;
					}
					else
					{
						supplier.Id = new SupplierIdentificationIdType();
						supplier.Id.Value = registration;
					}

					if (!shipmentSupplier.ContactPerson.IsEmpty)
					{
						if (shipmentSupplier.Communications.Any())
						{
							supplier.Contact = new DeclarationGoodsShipmentSupplierContact();
							supplier.Contact.Name = new ContactNameTextType();
							supplier.Contact.Name.Value = (string)shipmentSupplier.ContactPerson;

							var communications = new Collection<DeclarationGoodsShipmentSupplierContactCommunication>();
							foreach (ICommunication supplierComms in shipmentSupplier.Communications)
							{
								var communication = new DeclarationGoodsShipmentSupplierContactCommunication();
								communication.Id = new CommunicationIdentificationIdType();
								communication.Id.Value = supplierComms.ContactDetail;
								communication.TypeId = new CommunicationTypeIdType();
								communication.TypeId.Value = supplierComms.ContactType;
								communications.Add(communication);
							}

							supplier.Contact.Communication = communications;
						}
					}

					supplierList.Add(supplier);
				}
			}
			return supplierList;
		}

		DeclarationGoodsShipmentWarehouse PopulateWarehouse()
		{
			var warehouse = new DeclarationGoodsShipmentWarehouse();
			warehouse.Id = new WarehouseIdentificationIdType();
			warehouse.Id.Value = iM1Header.GoodsShipment.CustomsControlledArea;
			return warehouse;
		}

		#endregion

		#endregion

		#endregion
	}
}
