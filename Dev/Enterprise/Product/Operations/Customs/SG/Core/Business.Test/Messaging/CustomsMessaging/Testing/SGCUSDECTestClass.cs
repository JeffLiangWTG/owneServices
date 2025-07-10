using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing
{
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.Types;
	using Enterprise.MasterFiles.Business;

	public class SGCUSDECTestClass : ISGCUSDEC, IIPTDEC, IIPTUPD
	{
		public SGCUSDECTestClass()
		{
			AgentInfoTestClass agent = new AgentInfoTestClass();
			OrganisationTestClass org = new OrganisationTestClass();
			ISGCPlace place = new SGCPlaceTestClass();
			InwardCarrierAgent = org;
			OutwardCarrierAgent = org;
			HandlingAgent = org;
			Importer = org;
			Exporter = org;
			Consignee = org;
			Declarant = agent;
			FreightForwarder = org;
			EndUser = org;
		}

		#region ISGCUSDEC Members

		public ZString DeclarantId
		{
			get { return fDeclarantId; }
			set { fDeclarantId = value; }
		}
		ZString fDeclarantId;

		public ZBool IsImport
		{
			get { return fIsImport; }
			set { fIsImport = value; }
		}
		ZBool fIsImport;

		public ZBool HasInwardTransport
		{
			get { return fHasInwardTransport; }
			set { fHasInwardTransport = value; }
		}
		ZBool fHasInwardTransport;

		public ZBool IsExport
		{
			get { return fIsExport; }
			set { fIsExport = value; }
		}
		ZBool fIsExport;

		public ZBool HasOutwardTransport
		{
			get { return fHasOutwardTransport; }
			set { fHasOutwardTransport = value; }
		}
		ZBool fHasOutwardTransport;

		public ZBool IsSea
		{
			get { return fIsSea; }
			set { fIsSea = value; }
		}
		ZBool fIsSea;

		public ZBool IsAir
		{
			get { return fIsAir; }
			set { fIsAir = value; }
		}
		ZBool fIsAir;

		public ZBool IsInwardDeclaration
		{
			get { return fIsInwardDeclaration; }
			set { fIsInwardDeclaration = value; }
		}
		ZBool fIsInwardDeclaration;

		public ZBool IsOutwardDeclaration
		{
			get { return fIsOutwardDeclaration; }
			set { fIsOutwardDeclaration = value; }
		}
		ZBool fIsOutwardDeclaration;

		public ZBool IsTranshipmentDeclaration
		{
			get { return fIsTranshipmentDeclaration; }
			set { fIsTranshipmentDeclaration = value; }
		}
		ZBool fIsTranshipmentDeclaration;

		#region Inward Vessel

		public ZString InwardTransportIdentifier
		{
			get { return inwardVesselName; }
			set { inwardVesselName = value; }
		}
		ZString inwardVesselName;

		public ZString InwardJourneyIdentifier
		{
			get
			{
				var result = inwardVesselVoyageNo;
				if (InwardTransportCode == 1 || InwardTransportCode == 4)
				{
					if (result.IsEmpty)
					{
						result = "NA";
					}
				}
				return result;
			}
			set { inwardVesselVoyageNo = value; }
		}
		ZString inwardVesselVoyageNo;

		public ZString InwardVesselType
		{
			get { return inwardVesselType; }
			set { inwardVesselType = value; }
		}
		ZString inwardVesselType;

		#endregion

		#region OutwardVessel

		public ZString OutwardTransportIdentifier
		{
			get { return outwardVesselName; }
			set { outwardVesselName = value; }
		}
		ZString outwardVesselName;

		public ZString OutwardJourneyIdentifier
		{
			get { return outwardVesselVoyageNo; }
			set { outwardVesselVoyageNo = value; }
		}
		ZString outwardVesselVoyageNo;

		public ZInt OutwardVesselNRT
		{
			get { return outwardVesselNRT; }
			set { outwardVesselNRT = value; }
		}
		ZInt outwardVesselNRT;

		public ZString OutwardVesselType
		{
			get { return outwardVesselType; }
			set { outwardVesselType = value; }
		}
		ZString outwardVesselType;

		public ZString OutwardVesselNationality
		{
			get { return outwardVesselNationality; }
			set { outwardVesselNationality = value; }
		}
		ZString outwardVesselNationality;

		#endregion

		#region Towing Vessel

		public ZString TowingVesselName
		{
			get { return towingVesselName; }
			set { towingVesselName = value; }
		}
		ZString towingVesselName;

		public ZString TowingVesselVoyageNo
		{
			get { return towingVesselVoyageNo; }
			set { towingVesselVoyageNo = value; }
		}
		ZString towingVesselVoyageNo;

		public ZString TowingVesselType
		{
			get { return towingVesselType; }
			set { towingVesselType = value; }
		}
		ZString towingVesselType;

		#endregion

		public ZBool IsShortPayment
		{
			get { return fIsShortPayment; }
			set { fIsShortPayment = value; }
		}
		ZBool fIsShortPayment;

		public ZBool IsExempted
		{
			get { return fIsExempted; }
			set { fIsExempted = value; }
		}
		ZBool fIsExempted;

		public ZBool IsRecoveryPayment
		{
			get { return fIsRecoveryPayment; }
			set { fIsRecoveryPayment = value; }
		}
		ZBool fIsRecoveryPayment;

		public ZString PortOfLoading
		{
			get { return fPortOfLoading; }
			set { fPortOfLoading = value; }
		}
		ZString fPortOfLoading;

		public ZString PortOfDischarge
		{
			get { return fPortOfDischargee; }
			set { fPortOfDischargee = value; }
		}
		ZString fPortOfDischargee;

		public ZString NextPortOfCall
		{
			get { return fPortOfNextCall; }
			set { fPortOfNextCall = value; }
		}
		ZString fPortOfNextCall;

		public ZString FinalPortOfCall
		{
			get { return fFinalPortOfCall; }
			set { fFinalPortOfCall = value; }
		}
		ZString fFinalPortOfCall;

		public ZDate ArrivalDate
		{
			get { return fArrivalDate; }
			set { fArrivalDate = value; }
		}
		ZDate fArrivalDate;

		public ZDate DepartureDate
		{
			get { return fDepartureDate; }
			set { fDepartureDate = value; }
		}
		ZDate fDepartureDate;

		public ZDate StartDateOfBlanket
		{
			get { return fStartDateOfBlanket; }
			set { fStartDateOfBlanket = value; }
		}
		ZDate fStartDateOfBlanket;

		public ZBool IsContainerised
		{
			get { return fIsContainerised; }
			set { fIsContainerised = value; }
		}
		ZBool fIsContainerised;

		public ZBool IsStorageInFTZ
		{
			get { return fIsStorageInFTZ; }
			set { fIsStorageInFTZ = value; }
		}
		ZBool fIsStorageInFTZ;

		public ZBool IsSeaStoreDeclaration
		{
			get { return fIsSeaStoreDeclaration; }
			set { fIsSeaStoreDeclaration = value; }
		}
		ZBool fIsSeaStoreDeclaration;

		public ZBool HasLiquorOrTobacco
		{
			get { return fHasLiquorOrTobacco; }
			set { fHasLiquorOrTobacco = value; }
		}
		ZBool fHasLiquorOrTobacco;

		public ISGCPlace PlaceOfRelease
		{
			get { return fPlaceOfRelease; }
			set { fPlaceOfRelease = value; }
		}
		ISGCPlace fPlaceOfRelease;

		public ZString PlaceOfStorage
		{
			get { return fPlaceOfStorage; }
			set { fPlaceOfStorage = value; }
		}
		ZString fPlaceOfStorage;

		public ISGCPlace PlaceOfReceipt
		{
			get { return fPlaceOfReceipt; }
			set { fPlaceOfReceipt = value; }
		}
		ISGCPlace fPlaceOfReceipt;

		public ISGCPlace InwardVesselBerth
		{
			get { return fInwardVesselBerth; }
			set { fInwardVesselBerth = value; }
		}
		ISGCPlace fInwardVesselBerth;

		public ISGCPlace OutwardVesselBerth
		{
			get { return fOutwardVesselBerth; }
			set { fOutwardVesselBerth = value; }
		}
		ISGCPlace fOutwardVesselBerth;

		public ZString PlaceOfStorageNameAddr
		{
			get { return fPlaceOfStorageNameAddr; }
			set { fPlaceOfStorageNameAddr = value; }
		}
		ZString fPlaceOfStorageNameAddr;

		public ZString CountryOfFinalDestination
		{
			get { return fUltimateDestinationCode; }
			set { fUltimateDestinationCode = value; }
		}
		ZString fUltimateDestinationCode;

		public ZBool IsReleasedInLicensedPremiseExclBWCY
		{
			get { return fIsReleasedInLicensedPremiseExclBWCY; }
			set { fIsReleasedInLicensedPremiseExclBWCY = value; }
		}
		ZBool fIsReleasedInLicensedPremiseExclBWCY;

		public ZBool IsStoredInLicensedPremise
		{
			get { return fIsStoredInLicensedPremise; }
			set { fIsStoredInLicensedPremise = value; }
		}
		ZBool fIsStoredInLicensedPremise;

		public IOrganisation InwardCarrierAgent
		{
			get { return fInwardCarrierAgent; }
			set { fInwardCarrierAgent = value; }
		}
		IOrganisation fInwardCarrierAgent;

		public IOrganisation OutwardCarrierAgent
		{
			get { return fOutwardCarrierAgent; }
			set { fOutwardCarrierAgent = value; }
		}
		IOrganisation fOutwardCarrierAgent;

		public IOrganisation HandlingAgent
		{
			get { return fHandlingAgent; }
			set { fHandlingAgent = value; }
		}
		IOrganisation fHandlingAgent;

		public IOrganisation Importer
		{
			get { return fImporter; }
			set { fImporter = value; }
		}
		IOrganisation fImporter;

		public IOrganisation Exporter
		{
			get { return fExporter; }
			set { fExporter = value; }
		}
		IOrganisation fExporter;

		public IOrganisation Consignee
		{
			get { return fConsignee; }
			set { fConsignee = value; }
		}
		IOrganisation fConsignee;

		public IOrganisation EndUser
		{
			get { return fEndUser; }
			set { fEndUser = value; }
		}
		IOrganisation fEndUser;

		public ICusAgentInfo Declarant
		{
			get { return fDeclarant; }
			set { fDeclarant = value; }
		}
		ICusAgentInfo fDeclarant;

		public IOrganisation FreightForwarder
		{
			get { return fFreightForwarder; }
			set { fFreightForwarder = value; }
		}
		IOrganisation fFreightForwarder;

		public IOrganisation Claimant
		{
			get { return claimant; }
			set { claimant = value; }
		}
		IOrganisation claimant;

		public ZString ClaimantCode
		{
			get { return fClaimantCode; }
			set { fClaimantCode = value; }
		}
		ZString fClaimantCode;

		public ZString ClaimantName
		{
			get { return fClaimantName; }
			set { fClaimantName = value; }
		}
		ZString fClaimantName;

		public IEnumerable<ICusItem> Items
		{
			get
			{
				if (fItems == null)
				{
					fItems = System.Array.Empty<ICusItem>();
				}
				return fItems;
			}
			set { fItems = value; }
		}
		IEnumerable<ICusItem> fItems;

		public ZBool IsCASCProductCodeNeeded
		{
			get { return fIsCASCProductCodeNeeded; }
			set { fIsCASCProductCodeNeeded = value; }
		}
		ZBool fIsCASCProductCodeNeeded;

		public ZBool IsDG
		{
			get { return fIsDG; }
			set { fIsDG = value; }
		}
		ZBool fIsDG;

		public ZString PreviousPermitNumber
		{
			get { return fPreviousPermitNumber; }
			set { fPreviousPermitNumber = value; }
		}
		ZString fPreviousPermitNumber;

		public ZString PermitNoToUpdateOrCancel
		{
			get { return fPermitNoToUpdateOrCancel; }
			set { fPermitNoToUpdateOrCancel = value; }
		}
		ZString fPermitNoToUpdateOrCancel;

		public ZString ReplacementPermitNumber
		{
			get { return fReplacementPermitNumber; }
			set { fReplacementPermitNumber = value; }
		}
		ZString fReplacementPermitNumber;

		public IEnumerable<ICusContainer> Containers
		{
			get { return fContainers; }
			set { fContainers = value; }
		}
		IEnumerable<ICusContainer> fContainers;

		public ZDecimal TotalOuterPack
		{
			get { return fTotalOuterPack; }
			set { fTotalOuterPack = value; }
		}
		ZDecimal fTotalOuterPack;

		public ZString TotalOuterPackUnitOfQty
		{
			get { return fTotalOuterPackUnitOfQty; }
			set { fTotalOuterPackUnitOfQty = value; }
		}
		ZString fTotalOuterPackUnitOfQty;

		public ZDecimal TotalGrossWeight
		{
			get { return fTotalGrossWeight; }
			set { fTotalGrossWeight = value; }
		}
		ZDecimal fTotalGrossWeight;

		public ZString TotalGrossWeightUnitOfQty
		{
			get { return fTotalGrossWeightUnitOfQty; }
			set { fTotalGrossWeightUnitOfQty = value; }
		}
		ZString fTotalGrossWeightUnitOfQty;

		public ZString CargoPackingType
		{
			get { return fCargoPackingType; }
			set { fCargoPackingType = value; }
		}
		ZString fCargoPackingType;

		public ZDecimal TotalCustomsValue
		{
			get { return fTotalCustomsValue; }
			set { fTotalCustomsValue = value; }
		}
		ZDecimal fTotalCustomsValue;

		public ZDecimal TotalDutyPayable
		{
			get { return fTotalDutyPayable; }
			set { fTotalDutyPayable = value; }
		}
		ZDecimal fTotalDutyPayable;

		public ZDecimal TotalExcisePayable
		{
			get { return fTotalExcisePayable; }
			set { fTotalExcisePayable = value; }
		}
		ZDecimal fTotalExcisePayable;

		public ZDecimal TotalOtherTaxPayable
		{
			get { return fTotalOtherTaxPayable; }
			set { fTotalOtherTaxPayable = value; }
		}
		ZDecimal fTotalOtherTaxPayable;

		public ZDecimal TotalGSTPayable
		{
			get { return fTotalGSTPayable; }
			set { fTotalGSTPayable = value; }
		}
		ZDecimal fTotalGSTPayable;

		public ZDecimal TotalPayable
		{
			get { return fTotalPayable; }
			set { fTotalPayable = value; }
		}
		ZDecimal fTotalPayable;

		public ZString DeclarationType
		{
			get { return fDeclarationType; }
			set { fDeclarationType = value; }
		}
		ZString fDeclarationType;

		public ZString JobNumber
		{
			get { return fJobNumber; }
			set { fJobNumber = value; }
		}
		ZString fJobNumber;

		public ZString SupplyIndicator
		{
			get { return fSupplyIndicator; }
			set { fSupplyIndicator = value; }
		}
		ZString fSupplyIndicator;

		public ZInt InwardTransportCode
		{
			get { return fInwardTransportCode; }
			set { fInwardTransportCode = value; }
		}
		ZInt fInwardTransportCode;

		public ZInt OutwardTransportCode
		{
			get { return fOutwardTransportCode; }
			set { fOutwardTransportCode = value; }
		}
		ZInt fOutwardTransportCode;

		public IEnumerable<ZString> TradersRemarksForMessage
		{
			get { return fTradersRemarksForMessage; }
			set { fTradersRemarksForMessage = value; }
		}
		IEnumerable<ZString> fTradersRemarksForMessage;

		public IEnumerable<ICusInvoice> Invoices
		{
			get
			{
				if (fInvoices == null)
				{
					fInvoices = System.Array.Empty<ICusInvoice>();
				}
				return fInvoices;
			}
			set { fInvoices = value; }
		}
		IEnumerable<ICusInvoice> fInvoices;

		public ZString InwardHouseBill
		{
			get { return fInwardHouseBill; }
			set { fInwardHouseBill = value; }
		}
		ZString fInwardHouseBill;

		public ZString OutwardHouseBill
		{
			get { return fOutwardHouseBill; }
			set { fOutwardHouseBill = value; }
		}
		ZString fOutwardHouseBill;

		public ZInt NumberOfRequestsForUpdate
		{
			get { return fNumberOfRequestsForUpdate; }
			set { fNumberOfRequestsForUpdate = value; }
		}
		ZInt fNumberOfRequestsForUpdate;

		public IEnumerable<ZString> AdditionalRecipients
		{
			get { return fAdditionalRecipients; }
			set { fAdditionalRecipients = value; }
		}
		IEnumerable<ZString> fAdditionalRecipients;

		public IEnumerable<ICusDocument> LicencesAndDocuments
		{
			get { return fLicencesAndDocuments; }
			set { fLicencesAndDocuments = value; }
		}
		IEnumerable<ICusDocument> fLicencesAndDocuments;

		public ZString InwardMasterBill
		{
			get { return fInwardMasterBill; }
			set { fInwardMasterBill = value; }
		}
		ZString fInwardMasterBill;

		public ZString OutwardMasterBill
		{
			get { return fOutwardMasterBill; }
			set { fOutwardMasterBill = value; }
		}
		ZString fOutwardMasterBill;

		public ZString BGIndicator
		{
			get { return fBGIndicatorl; }
			set { fBGIndicatorl = value; }
		}
		ZString fBGIndicatorl;

		public IOrganisation Manufacturer
		{
			get { return fManufacturer; }
			set { fManufacturer = value; }
		}
		IOrganisation fManufacturer;

		public ZBool IsForStorage
		{
			get { return fIsForStorage; }
			set { fIsForStorage = value; }
		}
		ZBool fIsForStorage;

		public ZBool IsRefundDeclaration
		{
			get { return fIsRefundDeclaration; }
			set { fIsRefundDeclaration = value; }
		}
		ZBool fIsRefundDeclaration;

		public ZDecimal TotalDutyRefund
		{
			get { return fTotalDutyRefund; }
			set { fTotalDutyRefund = value; }
		}
		ZDecimal fTotalDutyRefund;

		public ZDecimal TotalExciseRefund
		{
			get { return fTotalExciseRefund; }
			set { fTotalExciseRefund = value; }
		}
		ZDecimal fTotalExciseRefund;

		public ZDecimal TotalGSTRefund
		{
			get { return fTotalGSTRefund; }
			set { fTotalGSTRefund = value; }
		}
		ZDecimal fTotalGSTRefund;

		public ZInt NumberOfCrew
		{
			get { return numberOfCrew; }
			set { numberOfCrew = value; }
		}
		ZInt numberOfCrew;

		public ZInt VoyageDuration
		{
			get { return voyageDuration; }
			set { voyageDuration = value; }
		}
		ZInt voyageDuration;

		public IAdditionalMessageInformation AdditionalMessageInformation
		{
			get { return additionalMessageInformation ?? (additionalMessageInformation = new ImplementsAdditionalMessageInformation()); }
		}
		IAdditionalMessageInformation additionalMessageInformation;

		public ImplementsAdditionalMessageInformation AdditionalMessageInfo
		{
			get { return (ImplementsAdditionalMessageInformation)AdditionalMessageInformation; }
		}

		#region ImplementsAdditionalMessageInformation

		public class ImplementsAdditionalMessageInformation : IAdditionalMessageInformation
		{
			#region IAdditionalMessageInformation Members

			public ZDecimal GSTRefundAmount
			{
				get { return gstRefundAmount; }
				set { gstRefundAmount = value; }
			}
			ZDecimal gstRefundAmount;

			public ZDecimal DutyRefundAmount
			{
				get { return dutyRefundAmount; }
				set { dutyRefundAmount = value; }
			}
			ZDecimal dutyRefundAmount;

			public ZDecimal ExciseRefundAmount
			{
				get { return exciseRefundAmount; }
				set { exciseRefundAmount = value; }
			}
			ZDecimal exciseRefundAmount;

			public ZString ReasonForAmending
			{
				get { return reasonForAmending; }
				set { reasonForAmending = value; }
			}
			ZString reasonForAmending;

			public ZString RefundCode
			{
				get { return refundCode; }
				set { refundCode = value; }
			}
			ZString refundCode;

			public ZString ReasonForRefund
			{
				get { return reasonForRefund; }
				set { reasonForRefund = value; }
			}
			ZString reasonForRefund;

			public ZBool ExtendingTemporaryImportPeriod
			{
				get { return extendingTemporaryImportPeriod; }
				set { extendingTemporaryImportPeriod = value; }
			}
			ZBool extendingTemporaryImportPeriod;

			public ZString ReasonForExtendingTemporaryImportPeriod
			{
				get { return reasonForExtendingTemporaryImportPeriod; }
				set { reasonForExtendingTemporaryImportPeriod = value; }
			}
			ZString reasonForExtendingTemporaryImportPeriod;

			public ZString CancellationCode
			{
				get { return cancellationCode; }
				set { cancellationCode = value; }
			}
			ZString cancellationCode;

			public ZString UpdateIndicator
			{
				get { return updateIndicator; }
				set { updateIndicator = value; }
			}
			ZString updateIndicator;

			public ZString Broker
			{
				get { return broker; }
				set { broker = value; }
			}
			ZString broker;

			public IEnumerable<ICusAttachment> SupportingDocuments
			{
				get { return supportingDocuments ?? Enumerable.Empty<ICusAttachment>(); }
				set { supportingDocuments = value; }
			}
			IEnumerable<ICusAttachment> supportingDocuments;

			public event MessageEventHandler OnMessageCreated;

			public string MessageCreated(string messageText)
			{
				MessageEventArgs args = new MessageEventArgs(messageText);
				OnMessageCreated(args);
				return messageText;
			}

			#endregion
		}

		#endregion

		public IContainerSequenceStore GetPreviousMessageContainerSequence()
		{
			return PreviousMessageContainerSequence;
		}
		public IContainerSequenceStore PreviousMessageContainerSequence = new ContainerSequenceStore(null);

		public IEnumerable<ICusCPC> CPCs
		{
			get { return fCPCs; }
			set { fCPCs = value; }
		}
		IEnumerable<ICusCPC> fCPCs;

		#endregion

		#region TestData

		public class SGCUSDECTestData
		{
			public SGCUSDECTestData()
			{
			}

			SGCUSDECTestClass DataProvider
			{
				get
				{
					if (fDataProvider == null)
					{
						fDataProvider = new SGCUSDECTestClass();
					}
					return fDataProvider;
				}
			}
			SGCUSDECTestClass fDataProvider;

			#region TestDataValues

			public SGCUSDECTestClass SetTestData()
			{
				DataProvider.DeclarationType = DeclarationTypeCodeList.Codes.GST;
				DataProvider.AdditionalMessageInfo.UpdateIndicator = SGConstants.UpdateIndicators.AME;
				DataProvider.CargoPackingType = "1";
				DataProvider.PortOfLoading = "HKHKG";
				SGCPlaceTestClass receiptRelease = new SGCPlaceTestClass();
				receiptRelease.Code = "CZ";
				receiptRelease.Type = "CZ";
				DataProvider.PlaceOfRelease = receiptRelease;
				DataProvider.PlaceOfReceipt = receiptRelease;
				DataProvider.HasInwardTransport = true;
				DataProvider.ArrivalDate = new ZDate(2006, 12, 12);
				DataProvider.TotalGrossWeight = 85.75m;
				DataProvider.TotalGrossWeightUnitOfQty = Core.Constants.Weight.Kilograms;
				DataProvider.TotalOuterPack = 18;
				DataProvider.TotalOuterPackUnitOfQty = Core.Constants.PkgUnit.Carton;

				DataProvider.ClaimantName = "Chin Wee";
				DataProvider.ClaimantCode = "17905748J";

				ItemsTestClass[] invoices = new ItemsTestClass[1];
				invoices[0] = new ItemsTestClass();
				invoices[0].InvoicePK = ZGuid.NewZGuid();
				invoices[0].InvoiceCurrency = "SGD";
				invoices[0].InvoiceDate = new ZDate(2006, 12, 20);
				invoices[0].InvoiceNumber = "INV0103101.9";
				invoices[0].InvoiceCurrExchangeRate = 1;
				invoices[0].InvoiceTotalAmount = 23012;
				invoices[0].FreightCharge = new ChargeTestClass(Core.Constants.CurrencyCodes.Singapore, 1m, 100m, 0m);
				invoices[0].InsuranceCharge = new ChargeTestClass(Core.Constants.CurrencyCodes.Singapore, 1m, 200m, 0m);
				DataProvider.Invoices = invoices;

				return DataProvider;
			}

			public SGCUSDECTestClass SetFullTestData(string declarationType)
			{
				DataProvider.DeclarantId = "V13T001";
				DataProvider.DeclarationType = declarationType;
				AgentInfoTestClass declarant = new AgentInfoTestClass();
				declarant.Name = "Chow Lew Wee";
				declarant.Code = "V13T001";
				declarant.Phone = "+64 9760 2847";
				DataProvider.Declarant = declarant;
				DataProvider.CargoPackingType = "1";
				DataProvider.PortOfLoading = "HKHKG";
				SGCPlaceTestClass release = new SGCPlaceTestClass();
				release.Code = "CCJ";
				release.Type = "CCJ";
				DataProvider.PlaceOfRelease = release;
				SGCPlaceTestClass receipt = new SGCPlaceTestClass();
				receipt.Code = "CW";
				receipt.Type = "CW";
				DataProvider.PlaceOfReceipt = receipt;
				DataProvider.InwardMasterBill = "OBL00384758";
				DataProvider.InwardTransportCode = SGConstants.TransportCodes.Sea;
				DataProvider.InwardTransportCode = SGConstants.TransportCodes.Sea;
				DataProvider.InwardTransportIdentifier = "PACIFIC STAR";
				DataProvider.InwardJourneyIdentifier = "18W";
				DataProvider.IsImport = true;
				DataProvider.HasInwardTransport = true;
				DataProvider.IsSea = true;
				DataProvider.IsContainerised = true;
				DataProvider.BGIndicator = BGIndicatorCodeList.Codes.I;
				DataProvider.ArrivalDate = new ZDate(2006, 12, 13);
				DataProvider.TotalGrossWeight = 85.75m;
				DataProvider.TotalGrossWeightUnitOfQty = Core.Constants.Weight.Tonnes;
				DataProvider.TotalOuterPack = 4;
				DataProvider.TotalOuterPackUnitOfQty = Core.Constants.PkgUnit.Container;
				var tradersRemarks = new ZString[1];
				tradersRemarks[0] = "Test Declaration Message: exch rate 0.9565";
				DataProvider.TradersRemarksForMessage = tradersRemarks;

				ContainersTestClass[] containers = new ContainersTestClass[3];
				containers[0] = new ContainersTestClass();
				containers[0].ContainerNumber = "FLMU0039485";
				containers[0].ContainerSize = 40;
				containers[0].ContainerType = "FCL";
				containers[0].ContainerWeight = 15.75m;
				containers[0].ContainerWeightUnit = Core.Constants.Weight.Tonnes;
				containers[0].SealNumber = "AK0038-92";

				containers[1] = new ContainersTestClass();
				containers[1].ContainerNumber = "FLMU8528974";
				containers[1].ContainerSize = 40;
				containers[1].ContainerType = "FCL";
				containers[1].ContainerWeight = 14.8m;
				containers[1].ContainerWeightUnit = Core.Constants.Weight.Tonnes;

				containers[2] = new ContainersTestClass();
				containers[2].ContainerNumber = "JJYU497621";
				containers[2].ContainerSize = 20;
				containers[2].ContainerType = "LCL";
				containers[2].ContainerWeight = 3.595m;
				containers[2].ContainerWeightUnit = Core.Constants.Weight.Tonnes;
				containers[2].SealNumber = "127ST5";
				DataProvider.Containers = containers;

				ItemsTestClass[] invoices = new ItemsTestClass[1];
				invoices[0] = new ItemsTestClass();
				invoices[0].InvoicePK = ZGuid.NewZGuid();
				invoices[0].InvoiceCurrency = "SGD";
				invoices[0].IncoTerm = "FOB";
				invoices[0].InvoiceDate = new ZDate(2006, 12, 20);
				OrgHeader supplier = new BusinessObjectFactory().New<OrgHeader>();
				supplier.OH_FullName = "PACECO INDUSTRIAL SUPPLIES PTE LTD";
				supplier.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "14242880000R");
				supplier.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "14242880000R");
				invoices[0].Supplier = new EntryOrganisationsInfo(supplier);
				invoices[0].InvoiceNumber = "INV0103101.9";
				invoices[0].InvoiceCurrExchangeRate = 1;
				invoices[0].InvoiceTotalAmount = 23012;
				invoices[0].FreightCharge = new ChargeTestClass(Core.Constants.CurrencyCodes.Singapore, 1m, 100m, 1.5m);
				invoices[0].InsuranceCharge = new ChargeTestClass(Core.Constants.CurrencyCodes.UnitedStates, 1.54m, 178.90m, 1.38m);
				DataProvider.Invoices = invoices;

				serialNumber = 0;
				ItemsTestClass[] items = new ItemsTestClass[3];
				items[0] = new ItemsTestClass();
				InitItem(items[0], "48063000", "TRACING PAPERS", 144, 750);
				items[1] = new ItemsTestClass();
				InitItem(items[1], "48083000", "OTHER KRAFT PAPER CREPED OR CRINKLED", 250, 1095);
				items[2] = new ItemsTestClass();
				InitItem(items[2], "48091010", "CARBON PAPER IN ROLLS OR SHEETS", 500, 1575);
				DataProvider.Items = items;

				return DataProvider;
			}

			void InitItem(ItemsTestClass item, string hSCode, string description, int qty, decimal unitPrice)
			{
				item.BrandName = "Spicers";
				item.CountryOfOriginCode = "AU";
				item.DutyAmount = 12;
				item.DutyUnitRate = 7;
				item.DGIndicator = "N";
				item.E_SDNPIndicator = "Y";
				item.GoodsDescription = description;
				item.GSTPayable = 111;
				item.HSCode = hSCode;
				item.HSQuantity = qty;
				item.InvoiceNumber = "INV0103101.9";
				item.IsBasedOnRates = true;
				item.IsDangerous = false;
				item.IsLiquor = false;
				item.IsMotorVehicle = false;
				item.PreferenceIndicator = "";
				item.IsTobacco = false;

				item.TotalDutiableQuantity = 13;
				item.TotalDutiableQuantityUnitType = UnitOfQuantityCodeList.Codes.KGM;
				serialNumber++;
				item.SerialNumber = serialNumber.ToString();
				item.UnitPrice = unitPrice;
				item.UnitDutiableQuantity = 17;
				item.CustomsValue = 25000m;
				item.LSPValue = 2.28m;
			}

			public SGCUSDECTestClass SetLiqourTestData(string declarationType)
			{
				DataProvider.DeclarantId = "V13T001";
				DataProvider.DeclarationType = declarationType;
				AgentInfoTestClass declarant = new AgentInfoTestClass();
				declarant.Name = "Chow Lew Wee";
				declarant.Code = "V13T001";
				declarant.Phone = "+64 9760 2847";
				DataProvider.Declarant = declarant;
				DataProvider.CargoPackingType = "1";
				DataProvider.PortOfLoading = "USLAX";
				SGCPlaceTestClass release = new SGCPlaceTestClass();
				release.Code = "CCJ";
				release.Type = "CCJ";
				DataProvider.PlaceOfRelease = release;
				SGCPlaceTestClass receipt = new SGCPlaceTestClass();
				receipt.Code = "CW";
				receipt.Type = "CW";
				DataProvider.PlaceOfReceipt = receipt;
				DataProvider.InwardMasterBill = "OBL00384758";
				DataProvider.InwardHouseBill = "HB-304858";
				DataProvider.InwardTransportCode = SGConstants.TransportCodes.Sea;

				DataProvider.InwardTransportCode = SGConstants.TransportCodes.Sea;
				DataProvider.InwardTransportIdentifier = "PACIFIC STAR";
				DataProvider.InwardJourneyIdentifier = "18W";
				DataProvider.IsImport = true;
				DataProvider.HasInwardTransport = true;
				DataProvider.IsSea = true;
				DataProvider.IsContainerised = true;
				DataProvider.BGIndicator = BGIndicatorCodeList.Codes.D;
				DataProvider.ArrivalDate = new ZDate(2007, 01, 08);
				DataProvider.TotalGrossWeight = 85.75m;
				DataProvider.TotalGrossWeightUnitOfQty = Core.Constants.Weight.Tonnes;
				DataProvider.TotalOuterPack = 4;
				DataProvider.TotalOuterPackUnitOfQty = Core.Constants.PkgUnit.Container;
				var tradersRemarks = new ZString[1];
				tradersRemarks[0] = "15000 litres @ $38USD / ltr";
				DataProvider.TradersRemarksForMessage = tradersRemarks;

				ContainersTestClass[] containers = new ContainersTestClass[1];
				containers[0] = new ContainersTestClass();
				containers[0].ContainerNumber = "FLMU0039485";
				containers[0].ContainerSize = 40;
				containers[0].ContainerType = "FCL";
				containers[0].ContainerWeight = 15.75m;
				containers[0].ContainerWeightUnit = Core.Constants.Weight.Tonnes;
				containers[0].SealNumber = "AK0038-92";
				DataProvider.Containers = containers;

				ItemsTestClass[] invoices = new ItemsTestClass[1];
				invoices[0] = new ItemsTestClass();
				invoices[0].InvoicePK = ZGuid.NewZGuid();
				invoices[0].InvoiceCurrency = "USD";
				invoices[0].IncoTerm = "FOB";
				invoices[0].InvoiceDate = new ZDate(2006, 12, 20);

				BusinessObjectFactory factory = new BusinessObjectFactory();

				OrgHeader supplier = factory.New<OrgHeader>();
				supplier.OH_FullName = "JIM BEAM ASIA PACIFIC DISTRIBUTION";
				supplier.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "14242880000R");
				supplier.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "14242880000R");
				invoices[0].Supplier = new EntryOrganisationsInfo(supplier);
				invoices[0].InvoiceNumber = "JB200629837";
				invoices[0].InvoiceCurrExchangeRate = 1.54;
				invoices[0].InvoiceCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				invoices[0].InvoiceTotalAmount = 570000m; // 877800m SGD				
				invoices[0].FreightCharge = new ChargeTestClass(Core.Constants.CurrencyCodes.Singapore, 1m, 100m, 1.5m);
				invoices[0].InsuranceCharge = new ChargeTestClass(Core.Constants.CurrencyCodes.UnitedStates, 1.54m, 178.90m, 1.38m);
				DataProvider.Invoices = invoices;

				ItemsTestClass[] items = new ItemsTestClass[1];
				items[0] = new ItemsTestClass();
				items[0].BrandName = "Jim Beam Texas Bourbon";
				items[0].CustomsValue = 79950m;
				items[0].CountryOfOriginCode = "US";
				items[0].CurrentLotNumber = "AA00389";
				items[0].DutyAmount = 0;
				items[0].DutyUnitRate = 70.00m;
				items[0].DGIndicator = "N";
				items[0].UnitDutiableQuantity = 100;
				items[0].E_SDNPIndicator = "ESDNP Ind";
				items[0].ExciseAmount = 504000.00m;  // 15000 ltrs @ 48% alcohol by volume by $70 per litre of alcohol
				items[0].GoodsDescription = "WHISKIES OVER 46% VOL";
				items[0].GSTPayable = 35112.00m;
				items[0].HSCode = "22083020";
				items[0].HSQuantity = 15000;
				items[0].InvoiceNumber = "JB200629837";
				items[0].InvoiceCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				items[0].InvoiceCurrExchangeRate = 1.54;
				items[0].IsBasedOnRates = false;
				items[0].IsDangerous = false;
				items[0].IsInBondedWarehouse = true;
				items[0].IsLiquor = true;
				items[0].IsMotorVehicle = false;
				items[0].IsTobacco = false;
				items[0].LSPValue = 57.38;
				items[0].MarksAndNumbers = "xx-394775, 349.4985";
				items[0].PackingUnitType = "CTN";
				items[0].PackInmostQuantity = 19800;
				items[0].PackInmostUnitType = "BOT";
				items[0].PackInnerQuantity = 1650;
				items[0].PackInnerUnitType = "BOX";
				items[0].PackInQuantity = 25;
				items[0].PackInUnitType = "CRT";
				items[0].PackOuterQuantity = 1;
				items[0].PackOuterUnitType = "CTN";
				items[0].PercentageOfAlcohol = 48m;
				items[0].PreviousLotNumber = "AC03948";
				items[0].SerialNumber = "1";
				items[0].UnitPrice = 37.28m;
				items[0].PreferenceIndicator = PreferentialIndicatorCodeList.Codes.PRI;
				items[0].InwardHAWB = "InwardHAWB";
				DataProvider.Items = items;

				return DataProvider;
			}

			int serialNumber;

			#endregion
		}

		#endregion
	}

	public class CusCertificateTestClass : SGCUSDECTestClass, ITCODEC
	{
		#region ITCODEC Members

		public ZString ApplicationProductType
		{
			get { return fApplicationProductType; }
			set { fApplicationProductType = value; }
		}
		ZString fApplicationProductType;

		public ZString DonorCountryCode
		{
			get { return fDonorCountryCode; }
			set { fDonorCountryCode = value; }
		}
		ZString fDonorCountryCode;

		public ZInt YearOfEntry
		{
			get { return fYearOfEntry; }
			set { fYearOfEntry = value; }
		}
		ZInt fYearOfEntry;

		public ZString AdditionalInformation
		{
			get { return fAdditionalInformation; }
			set { fAdditionalInformation = value; }
		}
		ZString fAdditionalInformation;

		public ZString AdditionalDetails1
		{
			get { return fAdditionalDetails1; }
			set { fAdditionalDetails1 = value; }
		}
		ZString fAdditionalDetails1;

		public ZString AdditionalDetails2
		{
			get { return fAdditionalDetails2; }
			set { fAdditionalDetails2 = value; }
		}
		ZString fAdditionalDetails2;

		public ZString TransportDetails1
		{
			get { return fTransportDetails1; }
			set { fTransportDetails1 = value; }
		}
		ZString fTransportDetails1;

		public ZString TransportDetails2
		{
			get { return fTransportDetails2; }
			set { fTransportDetails2 = value; }
		}
		ZString fTransportDetails2;

		public ZInt PercCommContent1
		{
			get { return percCommContent1; }
			set { percCommContent1 = value; }
		}
		ZInt percCommContent1;

		public ZBool SendInvoiceDetails
		{
			get { return sendInvoiceNo; }
			set { sendInvoiceNo = value; }
		}
		ZBool sendInvoiceNo;

		public ZInt NumberOfCopies1
		{
			get { return numberOfCopies1; }
			set { numberOfCopies1 = value; }
		}
		ZInt numberOfCopies1;

		public ZInt NumberOfCopies2
		{
			get { return numberOfCopies2; }
			set { numberOfCopies2 = value; }
		}
		ZInt numberOfCopies2;

		public ZString CertificateType1
		{
			get { return certificateType1; }
			set { certificateType1 = value; }
		}
		ZString certificateType1;

		public ZString CertificateType2
		{
			get { return certificateType2; }
			set { certificateType2 = value; }
		}
		ZString certificateType2;

		public ZString CurrencyCode
		{
			get { return currencyCode; }
			set { currencyCode = value; }
		}
		ZString currencyCode;

		public IEnumerable<ICusCertItem> CertItems
		{
			get
			{
				if (fCertItems == null)
				{
					fCertItems = System.Array.Empty<ICusCertItem>();
				}
				return fCertItems;
			}
			set { fCertItems = value; }
		}
		IEnumerable<ICusCertItem> fCertItems;

		#endregion
	}
}
