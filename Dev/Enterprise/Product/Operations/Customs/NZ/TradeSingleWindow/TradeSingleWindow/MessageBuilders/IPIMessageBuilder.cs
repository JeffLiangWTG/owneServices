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
	/// Import Primary Industries Declaration
	/// •	Submitted by
	///		o	Forwarder
	///
	/// •	Used to effect import clearance on behalf of:
	///		o	MAF Bio-security
	///		o	MAF Food
	///
	/// </summary>
	public class IPIMessageBuilder : TSWMessageBuilder<Declaration>
	{
		public IPIMessageBuilder(IImportDeclaration declarationHeader, TSWTransactionTypes transactionType, IAdditionalInformation additionalMessageInformation)
			: base()
		{
			this.iM1Header = declarationHeader;
			this.transactionType = transactionType;
			this.additionalMessageInformation = additionalMessageInformation;
		}
		readonly IImportDeclaration iM1Header;
		readonly TSWTransactionTypes transactionType;
		readonly IAdditionalInformation additionalMessageInformation;

		public override ZString MessageType
		{
			get { return iM1Header.MessageType; }
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
			get { return false; }
		}

		public override ZString DeclarantPinEncrypted
		{
			get { return ZString.Empty; }
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
				if (iM1Header.IsSea)
				{
					var premiseID = iM1Header.PremiseID;
					if (!premiseID.IsEmpty)
					{
						message.ApprovedEstablishmentPlace = PopulatePremiseID(premiseID);
					}
				}

				message.BorderTransportMeans = PopulateBorderTM();
				if (iM1Header.Carrier != null)
				{
					message.Carrier = PopulateCarrier();
				}
			}

			if (transactionType != TSWTransactionTypes.Cancel)
			{
				if (iM1Header.GoodsShipment != null)
				{
					message.GoodsShipment = PopulateGoodsShipment();
				}

				message.Importer = PopulateImporter();
				message.Packaging = PopulatePackaging();
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
			jurisdictionDate.Value = iM1Header.DateOfImport.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
			jurisdictionDate.FormatCode = DateTimePeriodFormatCode.Item102;
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
			if (iM1Header.OtherReferencedDocuments != null)
			{
				foreach (IOtherInfo referenceDocument in iM1Header.OtherReferencedDocuments)
				{
					var refDoc = new DeclarationAdditionalDocument();
					refDoc.Id = new AdditionalDocumentIdentificationIdType();
					refDoc.Id.Value = referenceDocument.Data;
					refDoc.TypeCode = new AdditionalDocumentTypeCodeType();
					refDoc.TypeCode.Value = referenceDocument.Code;
					additionalDocs.Add(refDoc);
				}
			}

			if (additionalMessageInformation != null)
			{
				foreach (ITSWAttachment attachment in additionalMessageInformation.SupportingDocuments)
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
			if (additionalMessageInformation != null)
			{
				if (transactionType == TSWTransactionTypes.Original || transactionType == TSWTransactionTypes.Replace || transactionType == TSWTransactionTypes.Completion)
				{
					if (!additionalMessageInformation.FreeText.IsEmpty)
					{
						decAddInfo.Add(FreeTextInfo());
					}

					if (!additionalMessageInformation.ManualOverrideText.IsEmpty)
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
			freeTextAddInfo.Content.Value = additionalMessageInformation.FreeText;
			return freeTextAddInfo;
		}

		DeclarationAdditionalInformation ManualOverrideInfo()
		{
			var manualOverrideAddInfo = new DeclarationAdditionalInformation();
			manualOverrideAddInfo.RequestOverrideCode = new AdditionalInformationRequestOverrideCodeType();
			manualOverrideAddInfo.RequestOverrideCode.Value = "Y";
			manualOverrideAddInfo.StatementDescription = new AdditionalInformationStatementDescriptionTextType();
			manualOverrideAddInfo.StatementDescription.Value = additionalMessageInformation.ManualOverrideText; // "Must be present to advise the reason for override or manual processing";
			manualOverrideAddInfo.StatementTypeCode = new AdditionalInformationStatementTypeCodeType();
			manualOverrideAddInfo.StatementTypeCode.Value = AdditionalStatementTypeList.Codes.ALP;
			return manualOverrideAddInfo;
		}

		DeclarationAdditionalInformation ChangeCancelReason()
		{
			var ccReasonAddInfo = new DeclarationAdditionalInformation();
			ccReasonAddInfo.StatementDescription = new AdditionalInformationStatementDescriptionTextType();
			ccReasonAddInfo.StatementDescription.Value = additionalMessageInformation.AdditionalStatementText; //"Change/Replace/Cancel reason";
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

		Collection<DeclarationApprovedEstablishmentPlace> PopulatePremiseID(string premiseID)
		{
			// specification states 0..*, but testing confirms only 1 ATF code is acceptable: **Error** in {Declaration/ApprovedEstablishmentPlace}:-There must be no more than one Transitional Facility
			// needs to be array however to match with xsd element structure
			var establishments = new Collection<DeclarationApprovedEstablishmentPlace>();
			var establishment = new DeclarationApprovedEstablishmentPlace();
			establishment.Id = new ApprovedEstablishmentPlaceIdentificationIdType();
			establishment.Id.Value = premiseID;
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

		DeclarationImporter PopulateImporter()
		{
			if (iM1Header.Importer != null)
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

		#endregion

		#region Goods Shipment

		DeclarationGoodsShipment PopulateGoodsShipment()
		{
			var shipment = new DeclarationGoodsShipment();
			shipment.ExportationCountryCode = PopulateShipmentOrigin();
			shipment.TransactionNatureCode = PopulateNatureOfTransaction();
			shipment.Consignment = PopulateConsignment();
			if (iM1Header.GoodsShipment.DeliverToParty != null)
			{
				if (iM1Header.GoodsShipment.DeliverToParty != iM1Header.Importer)
				{
					shipment.DeliveryDestination = PopulateDeliveryDestination();
				}
			}

			shipment.GovernmentAgencyGoodsItem = PopulateGoodsItems();
			shipment.Invoice = PopulateInvoices();
			shipment.Supplier = PopulateSupplier();

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

			if (iM1Header.IsAir || iM1Header.IsMail || iM1Header.IsSea)
			{
				var locationOfGoods = iM1Header.GoodsShipment.LocationOfGoods;
				if (!locationOfGoods.IsEmpty)
				{
					consignment.GoodsLocation = PopulateGoodsLocation(locationOfGoods);
				}
			}

			consignment.LoadingLocation = PopulatePortOfLoading();
			StoreBillsPointedTo(iM1Header);
			if (iM1Header.HasContainersOrPallets)
			{
				StoreEquipmentPointedTo(iM1Header.Equipment);
			}

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

		DeclarationGoodsShipmentConsignmentGoodsLocation PopulateGoodsLocation(string locationOfGoods)
		{
			var goodsLocation = new DeclarationGoodsShipmentConsignmentGoodsLocation();
			goodsLocation.Id = new GoodsLocationIdentificationIdType();
			goodsLocation.Id.Value = locationOfGoods;
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
				else if (bill.RelatedPackages.Any())
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
			foreach (IMasterBillTransportDocument bill in iM1Header.MasterBills)
			{
				var contractDoc = PopulateSingleBillDetails(bill);
				billNumbers.Add(contractDoc);

				foreach (ZGuid childBillPK in bill.ChildBills)
				{
					var childBill = GetBillElementDetails(childBillPK);
					if (childBill != null)
					{
						billNumbers.Add(PopulateSingleBillDetails(childBill));
					}
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

			if (iM1Header.HasContainersOrPallets)
			{
				foreach (ZGuid equipmentPK in bill.RelatedEquipment)
				{
					var equipmentPointer = new DeclarationGoodsShipmentConsignmentTransportContractDocumentPointer();
					equipmentPointer.SequenceNumeric = GetContainerElementSequence(equipmentPK);
					equipmentPointer.DocumentSectionCode = new PointerDocumentSectionCodeType();
					equipmentPointer.DocumentSectionCode.Value = "31B";
					equipmentPointers.Add(equipmentPointer);
				}
			}
			else
			{
				foreach (ZGuid decPackagingPK in bill.RelatedPackages)
				{
					var equipmentPointer = new DeclarationGoodsShipmentConsignmentTransportContractDocumentPointer();
					equipmentPointer.SequenceNumeric = GetPackagingElementSequence(decPackagingPK);
					equipmentPointer.DocumentSectionCode = new PointerDocumentSectionCodeType();
					equipmentPointer.DocumentSectionCode.Value = "93A";
					equipmentPointers.Add(equipmentPointer);
				}
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
					transEquipment.CharacteristicCode.Value = ContainerSizeTypeList.Codes.C23;
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
			unloadingLocation.Id.Value = iM1Header.GoodsShipment.PortOfDischarge;
			return unloadingLocation;
		}

		#endregion

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
			/*
			 *	Line details
For FAKs, only enter one line in this screen. It is not required to enter a separate line for every
consignment within an FAK container. Eg, 1x FAK Sea container with 20x consignments inside
= 1 line in the details screen of TSW.
Goods Description Field = FAK container
Tariff code = 8609000916L (tariff code is a required field – this is the tariff for containers)
*Any other fields that have a red asterisk need to be completed in order to submit the
lodgement.
			 */
			int itemSequence = 0;
			var items = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItem>();
			foreach (IGoodsItems lineDetail in iM1Header.GoodsShipment.Items)
			{
				itemSequence++;
				var item = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem();
				item.SequenceNumeric = itemSequence;
				item.AdditionalInformation = PopulateLineAdditionalInfo(lineDetail);
				item.Commodity = PopulateItemCommodity(lineDetail);
				item.GoodsMeasure = PopulateGoodsMeasure(lineDetail);
				item.Origin = PopulateItemOriginCountry(lineDetail);
				items.Add(item);
			}

			return items;
		}

		#region GAGI Details
		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation> PopulateLineAdditionalInfo(IGoodsItems lineDetail)
		{
			var lineAdditionalInfo = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalInformation>();
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

		#region Item Commodity

		DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity PopulateItemCommodity(IGoodsItems lineDetail)
		{
			var itemCommodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity();
			itemCommodity.Description = PopulateItemDescription();
			itemCommodity.Classification = PopulateItemClassification();
			itemCommodity.Source = PopulateItemExportCountry(lineDetail);

			return itemCommodity;
		}

		#region Item Commodity Details

		CommodityDescriptionTextType PopulateItemDescription()
		{
			var itemCommodityDescription = new CommodityDescriptionTextType();
			itemCommodityDescription.Value = "FAK container";
			return itemCommodityDescription;
		}

		Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification> PopulateItemClassification()
		{
			var classifications = new Collection<DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification>();
			var classification = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification();
			classification.Id = new ClassificationIdentificationIdType();
			classification.Id.Value = "8609000916L";
			classification.IdentificationTypeCode = new ClassificationIdentificationTypeCodeType();
			classification.IdentificationTypeCode.Value = ClassificationTypeList.Codes.HS;
			classifications.Add(classification);
			return classifications;
		}

		DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommoditySource PopulateItemExportCountry(IGoodsItems lineDetail)
		{
			var countryCode = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommoditySource();
			countryCode.CountryCode = new SourceCountryCodeType();
			countryCode.CountryCode.Value = lineDetail.ExportCountry;
			return countryCode;
		}

		#endregion

		#endregion

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
			goodsMeasure.Add(lineMeasure);

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

		#endregion

		#endregion

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

		#endregion

		Collection<DeclarationGoodsShipmentInvoice> PopulateInvoices()
		{
			var invoices = new Collection<DeclarationGoodsShipmentInvoice>();
			var invoice = new DeclarationGoodsShipmentInvoice();
			invoice.SequenceNumeric = 1;
			invoice.Id = new InvoiceIdentificationIdType();
			invoice.Id.Value = "INV1";
			invoice.ConditionCode = new InvoiceConditionCodeType();
			invoice.ConditionCode.Value = "FOB";
			invoices.Add(invoice);

			return invoices;
		}

		#endregion

		#endregion
	}
}
