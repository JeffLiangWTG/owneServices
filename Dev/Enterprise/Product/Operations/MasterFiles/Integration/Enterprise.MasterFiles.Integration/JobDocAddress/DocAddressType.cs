namespace Enterprise.MasterFiles.Integration
{
	/// <summary>
	/// Defines all possible types of JobDocAddress across ediEnterprise. Testing for this is in Enterprise.MasterFiles.Business,
	/// </summary>
	public enum DocAddressType
	{
		None = 0,

		BuyerDocumentaryAddress = 1,

		ConsignorDocumentaryAddress = 2,
		ConsignorPickupDeliveryAddress = 3,

		ConsigneeAddress = 4,
		ConsigneeDocumentaryAddress = 5,
		ConsigneePickupDeliveryAddress = 6,

		GoodsBillToAddress = 7,

		SupplierDocumentaryAddress = 8,
		SupplierPickupDeliveryAddress = 9,

		ImporterDocumentaryAddress = 10,
		ImporterPickupDeliveryAddress = 11,

		NotifyParty = 12,
		NotifyParty2 = 13,
		NotifyParty3 = 14,

		LocalCartagePickupFromAddress = 15,
		LocalCartageDeliverToAddress = 16,

		LocalCartageCTO = 17,
		LocalCartageCFS = 18,
		LocalCartageYard = 19,
		LocalCartageImporter = 20,
		LocalCartageExporter = 21,

		LocalCartageAddress1 = 22,
		LocalCartageAddress2 = 23,
		LocalCartageAddress3 = 24,
		LocalCartageAddress4 = 25,

		ContainerLegPickupAddress = 26,
		ContainerLegWaitPointAddress = 27,
		ContainerLegDeliveryAddress = 28,

		BookingPartyDocumentaryAddress = 29,
		PickUpAddress = 30,
		DropOffAddress = 31,
		ForeignShipperDocumentaryAddress = 32,

		DepartureCYDAddress = 33,
		DepartureCFSAddress = 34,
		DepartureCTOAddress = 35,
		ArrivalCTOAddress = 36,
		ArrivalCFSAddress = 37,
		ArrivalCYDAddress = 38,

		OneOffQuotePickupAddress = 39,
		OneOffQuoteDeliveryAddress = 40,

		InsuredByDocumentaryAddress = 41,
		AssuredPartyDocumentaryAddress = 42,
		ClaimsPayableByDocumentaryAddress = 43,
		SurveyReportPartyDocumentaryAddress = 44,

		TransportCompanyDocumentaryAddress = 45,
		TransportBillToAddress = 46,

		CustomsContainerTerminalOperatorAddress = 47,
		CustomsDepotAddress = 48,
		CustomsContainerYardAddress = 49,
		CustomsWarehouseAddress = 50,

		LocalCartageService = 51,
		LocalCartageMSC = 52,
		NonPersistent = 53,
		CustomsSupervisingOffice = 54,
		GovernmentContractor = 55,
		ProtestantAddress = 56,

		QuotationClientAddress = 57,

		CustomsPlaceOfLoading = 58,

		ControllingCustomer = 59,
		Manufacturer = 60,
		Consolidator = 61,
		ShipToParty = 62,
		SellingParty = 63,
		BuyingParty = 64,
		ScheduledContainerStuffingLocation = 65,
		CustomsTreatmentProviderAddress = 66,

		LocalCartageWarehouse = 67,
		ClientRequestedBillingParty = 68,

		ExternalBroker = 69,

		ArrivalCFSLocalTransportAddress = 70,
		Carrier = 71,
		ContainerYardEmptyPickupAddress = 72,
		ContainerYardEmptyReturnAddress = 73,
		Contractor = 74,
		DepartureCFSLocalTransportAddress = 75,
		LocalClient = 76,
		Location = 77,
		ReceivingForwarderAddress = 78,
		SendingForwarderAddress = 79,
		ShippingLineAddress = 80,
		Creditor = 81,
		IntermediateConsigneeAddress = 82,
		DeliveryAgent,
		ExportBroker,
		ImportBroker,

		DrawbackExporterOrDestroyer,
		DrawbackLocationOfDestruction,
		DrawbackLocationOfMerchandise,
		Principal,

		AQISProcessingEstablishment,

		PickupAgent,
		Warehouse,

		PlaceOfConsolidation,
		CommercialInvoiceOriginator,

		OriginatingConsignorAddress,
		FinalConsigneeAddress,

		DistributionCentreAddress,
		OverseasAgent,

		AdditionalDeliveryAddress,
		AdditionalConsignee,
		OGDProcessInspectionLPCO,
		CFIAPaymentParty,
		ControllingAgent,
		GrossWeightVerifiedBy,
		ImporterOfRecord,

		MasterBillIssuingParty,
		Exporter,
		ClaimantAddress,
		CoLoadWith,
		HouseBillIssuingParty,

		CarrierAgent,
		DestinationWarehouse,
		DispatchWarehouse,
		GoodsOwner,
		Transporter,

		Custodian,
		DisposalEntitledTrader,
		WarehouseClient,
		Representative,
		CarrierBookingAgent,
		CarrierHandlingAgent,
		Acquirer,
		Declarant,
		OutwardCarrierAgent,
		BondedFactory,
		AQISResponsiblePerson,
		AQISTransitDestination,
		AQISEUContactPerson,
		CBPBroker,
		GoodsLocation,
		ContainerPacking,
		GoodsAvailableAt,
		GoodsDeliveredTo,
		RefundParty,
		InwardCarrierAgent,
		FDASubmitter,
		UltimateConsignee,
		IntermediateConsignee,
		USPrincipalPartyInInterest,
		DefermentParty,
		InwardProcessingPlace,
		LocalProcessorAddress,
		MainAccountingAddress,
		Forwarder,
		SupplierTranslatedDocumentaryAddress,
		ImporterTranslatedDocumentaryAddress,
		JustificationContactDetailAddress,
		Supplier,

		AdministratorOfCustomsWork,
		InspectionWitness,
		ContractualPartner,
		Payer,
		BoardingLocalDocumentaryAddress,
		Stevedore,
		LPCOApplicant,
		LPCOHolder,
		LocalProcessorTranslatedDocAddress,
		SellerDocumentaryAddress,
		AQISEUPlaceOfDestination,
		MasterBillShipperOverride,
		MasterBillConsigneeOverride,
		ClearanceLocalInvolvedParty,
		ContainerOwnerAddress,

		BuyerTranslatedDocumentaryAddress,
		CarrierExportCreditor,
		CarrierImportCreditor,

		ConsignorSecurityAddress,
		ConsigneeSecurityAddress,

		ICS2FacilityPlace,

		COLSResponsibleParty,
		COLSDeliveryOrUnpack,
		COLSDirectionAAAddress,

		BranchOrCompanyProxyARAdress,
		DebtorAddress,

		ReturnAddress,
		Applicant,
		ApplicantTranslatedDocumentaryAddress,

		FDAShipperAddress,
		InvoicerAddress,

		FreightPayer,
		ManufacturerTranslatedDocumentaryAddress,
		LocationOfGoods,
		OwnerOfGoods,
		PermitOwner,

		Shipper,
		FSVPImporter,
		InitialImporter,
		Sponsor,
		Grower,
		Laboratory,
		ThirdPartyLaboratory,

		ContainerAgentCodeAddress,

		DutyPayer,

		VanningLocationAddress,

		Holder,
		SurrenderParty,
		ConsigneeElectronicBOLAddress,
		ConsignorAddress,

		AQISLoadingEstablishment,

		SelfFiler,

		AttorneyForCustomsProceduresAddress,
		CustomsExportOrientedUnitsAddress,
		ToOrder,
		SupportingDocumentOrganizationAddress,
		Transhipper,
		AirCargoAgent,
		AuthorizedEconomicOperatorAddress
	}
}
