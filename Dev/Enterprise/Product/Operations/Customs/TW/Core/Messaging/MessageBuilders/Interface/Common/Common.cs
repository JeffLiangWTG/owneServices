using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging
{
	public interface IDeclarationAgent
	{
		ZString ID { get; }

		ZString RoleCode { get; }

		ZString SubBoxID { get; }
	}

	public interface IConsignment
	{
		ZString ManifestSerialNumber { get; }

		IEnumerable<IAdditionalInformation> AdditionalInformations { get; }

		ZString ArrivalTransportMeansTypeCode { get; }

		ITransportMeans BorderTransportMeans { get; }

		IPartyDetails Carrier { get; }

		IConsignmentItem ConsignmentItem { get; }

		ZString GoodsLocation { get; }

		ILocation LoadingLocation { get; }

		IEnumerable<ITransportContractDocument> TransportContractDocuments { get; }

		IEnumerable<ITransportEquipment> TransportEquipments { get; }

		IBondedGoods BondedGoods { get; }

		ZString ShippingOrderNumber { get; }

		ITransportMeans DepartureTransportMeans { get; }

		ZString TransitTransportMeansTypeCode { get; }

		IEnumerable<ZString> GoodsLocations { get; }

		ILocation UnloadingLocation { get; }

		IGovernmentAgencyGoodsItem GovernmentAgencyGoodsItem { get; }

		ILocation TranshipmentLocation { get; }

		ILocation TransitDeparture { get; }
	}

	public interface IGoodsShipment
	{
		IEnumerable<IAdditionalDocument> AdditionalDocuments { get; }

		IEnumerable<IGovernmentAgencyGoodsItem> GovernmentAgencyGoodsItems { get; }

		ZDateTime ExitDateTime { get; }

		ZDecimal ItemChargeAmount { get; }

		[DecimalPlaces(0)]
		ZDecimal TotalCIFAmount { get; }

		IPartyDetails Consignee { get; }

		IConsignment Consignment { get; }

		IPartyDetails Consignor { get; }

		ICustomsValuation CustomsValuation { get; }

		ZString DeliveryDestinationName { get; }

		IEnumerable<IGoodsShipmentDutyTaxFee> DutyTaxFees { get; }

		IPartyDetails NotifyParty { get; }

		IPartyDetails Seller { get; }

		ZString TradeTermsConditionCode { get; }

		ZString UCR { get; }

		IPartyDetails Buyer { get; }

		IPartyDetails Exporter { get; }

		IEnumerable<IGoodsMeasure> GoodsMeasures { get; }

		IEnumerable<IAdditionalInformation> AdditionalInformations { get; }

		IEnumerable<IAdditionalDeclaration> AdditionalDeclarations { get; }
	}

	public interface ITransportContractDocument
	{
		ZString ID { get; }

		ZString TypeCode { get; }

		IPartyDetails Deconsolidator { get; }
	}

	public interface ITransportEquipment
	{
		ZString CharacteristicCode { get; }

		ZString ID { get; }

		ZString UsedCapacityCode { get; }

		IEnumerable<ZString> Seals { get; }
	}

	public interface IGovernmentAgencyGoodsItem
	{
		ZInt SequenceNumeric { get; }

		ICommodity Commodity { get; }

		IEnumerable<IAdditionalDocument> AdditionalDocuments { get; }

		IEnumerable<IAdditionalInformation> AdditionalInformations { get; }

		IGoodsMeasure GoodsMeasure { get; }

		IPartyDetails Manufacturer { get; }

		IOrigin Origin { get; }

		IPackaging Packaging { get; }

		IPreviousDocument PreviousDocument { get; }

		ILPCODetail ApprovalDocument { get; }

		ICommoditySpecification CommoditySpecification { get; }

		IGoodsLicensingStatisticalMeasure GoodsLicensingStatisticalMeasure { get; }

		IGoodsStatisticalMeasure GoodsStatisticalMeasure { get; }

		ILPCODetail MedicalInstrument { get; }

		IPreviousDocument PreBondedDocument { get; }

		IEnumerable<IShippingIdentification> ShippingIdentifications { get; }

		IGovernmentProcedure GovernmentProcedure { get; }

		ZDateTime ControlInspectionStartDateTime { get; }

		ZString ExaminationPlace { get; }

		IEnumerable<ITransportEquipment> TransportEquipments { get; }

		IAdditionalDeclaration AdditionalDeclaration { get; }

		ZString CriteriaCode { get; }

		ZString PreferentialCriteria { get; }

		ZString ProducerCode { get; }

		ZString OtherCriteria { get; }
	}

	public interface IAdditionalDocument
	{
		ZString ID { get; }

		ZString Content { get; }

		ZString ImageFileFormat { get; }

		ZString ImageFileName { get; }

		ZInt SequenceNumeric { get; }

		ZLong SizeMeasure { get; }

		ZString TypeCode { get; }

		ZString ResponsibleGovernmentAgency { get; }

		ZDate SlaughterDateTime { get; }
	}

	public interface ITransportMeans
	{
		ZDate ArrivalDateTime { get; }

		ZString TypeCode { get; }

		IEnumerable<ZString> ItineraryRoutingCountryCodes { get; }

		ZString ID { get; }

		ZString JourneyID { get; }

		ZString Registration { get; }

		ZString Name { get; }

		ZString CallSignID { get; }
	}

	public interface IAdditionalInformation
	{
		ZInt CopyQuantity { get; }

		ZString StatementCode { get; }

		ZString StatementDescription { get; }

		ZString ProcessNumber { get; }

		ZString DelProcessNumber { get; }

		ZString Content { get; }

		ZString ApprovalID { get; }

		ZString PackingHouse { get; }
	}

	public interface ICommodity
	{
		IEnumerable<IAdditionalDocument> AdditionalDocuments { get; }

		ZString CommercialCategorizationID { get; }

		ZString Description { get; }

		ZString GoodsGroupNameCode { get; }

		ZString Name { get; }

		ZString BarCode { get; }

		ZString ChineseDescription { get; }

		ZString EnglishDescription { get; }

		ZString CITESImportPermitID { get; }

		ZString FTATariffCode { get; }

		ZString SHTCImportPermitID { get; }

		ZString TariffCodeExtensionCode { get; }

		IEnumerable<IClassification> Classifications { get; }

		IClassification Classification { get; }

		ICommodityRelatedPackaging CommodityRelatedPackaging { get; }

		IConstituent Constituent { get; }

		ICommodityDutyTaxFee DutyTaxFee { get; }

		IGovernmentProcedure GovernmentProcedure { get; }

		IEnumerable<ZString> HandlingInstructionsCodes { get; }

		IInvoiceLine InvoiceLine { get; }

		IInvoice Invoice { get; }

		IPreviousDocument PreviousDocument { get; }

		IEnumerable<ICommodityNumber> CommodityNumbers { get; }

		IEnumerable<IDutyOtherTaxFee> DutyOtherTaxFees { get; }

		IDutyTaxFeeAmount DutyTaxFeeAmount { get; }

		IDutyTaxFeeQuantity DutyTaxFeeQuantity { get; }

		IFood Food { get; }

		IQuarantine Quarantine { get; }

		IVehicle Vehicle { get; }

		IWine Wine { get; }

		ZString CargoDescription { get; }

		ZString BondedNoteCode { get; }

		IEnumerable<ZString> VehicleIDs { get; }

		ZString PrintingTariffCode { get; }
	}

	public interface IInvoice
	{
		ZString ID { get; }

		ZDate IssueDateTime { get; }
	}

	public interface IGoodsMeasure
	{
		[DecimalPlaces(6)]
		ZDecimal NetWeightMeasure { get; }

		[DecimalPlaces(4)]
		ZDecimal TariffQuantity { get; }

		ZString UnitCode { get; }

		ZString CustomUnitCode { get; }
	}

	public interface IPackaging
	{
		ZDecimal QuantityQuantity { get; }

		ZString TypeCode { get; }

		ZString MarksNumbers { get; }

		ZString PackagingMaterialDescription { get; }

		ZString Combination { get; }

		ZDate PackingDateTime { get; }
	}

	public interface ICurrencyExchange
	{
		ZString CurrencyTypeCode { get; }

		[DecimalPlaces(5)]
		ZDecimal RateNumeric { get; }
	}

	public interface IDutyTaxFee
	{
		ZString DutyExemptionWaiverNote { get; }

		ZString DutyMemoPrinted { get; }

		ZString DutyMethodCode { get; }

		[DecimalPlaces(0)]
		ZDecimal TotalDutyTaxFeeAmount { get; }

		ZString PaymentObligationGuaranteeReferenceID { get; }

		ZDecimal TotalCashDutyTaxFeeAmount { get; }

		ZDecimal TotalNonCashDutyTaxFeeAmount { get; }
	}

	public interface IBondedGoods
	{
		ZString AddDutyReasonCode { get; }

		IEnumerable<IBondedGoodsInvoice> BondedGoodsInvoices { get; }

		IBondedGoodsMonthlyReport BondedGoodsMonthlyReport { get; }

		IBondedParty InBondedParty { get; }

		IBondedParty OutBondedParty { get; }

		IEnumerable<IBondedParty> PreBondedParties { get; }

		ZString DocumentCode { get; }

		ZString Refundable { get; }

		IEnumerable<IBondedParty> BondedFactories { get; }
	}

	public interface IBondedGoodsInvoice
	{
		ZString ID { get; }

		[DecimalPlaces(0)]
		ZDecimal ValueAmount { get; }
	}

	public interface IBondedGoodsMonthlyReport
	{
		ZInt MonthNumeric { get; }

		ZString TraderReferenceID { get; }
	}

	public interface ICustomsValuation
	{
		[DecimalPlaces(2)]
		ZDecimal ExitToEntryChargeAmount { get; }

		[DecimalPlaces(2)]
		ZDecimal FreightChargeAmount { get; }

		[DecimalPlaces(0)]
		ZDecimal OtherChargeDeductionAmount { get; }

		ZString PartyRelationshipCode { get; }

		[DecimalPlaces(2)]
		ZDecimal OtherChargeAmount { get; }

		[DecimalPlaces(2)]
		ZDecimal OtherDeductionAmount { get; }

		[DecimalPlaces(2)]
		ZDecimal InvoiceAmount { get; }

		[DecimalPlaces(2)]
		ZDecimal TotalDutyTaxFeeAmount { get; }
	}

	public interface IClassification
	{
		ZString ID { get; }

		ZString IdentificationTypeCode { get; }
	}

	public interface IConstituent
	{
		ZString ElementDescription { get; }

		ZString LevelID { get; }

		ZString Thickness { get; }
	}

	public interface IGovernmentProcedure
	{
		ZString TransportTypeCode { get; }

		ZString CurrentCode { get; }

		ZString Description { get; }
	}

	public interface IInvoiceLine
	{
		ZString ChargesTypeCode { get; }

		ZString CurrencyTypeCode { get; }

		[DecimalPlaces(6)]
		ZDecimal UnitPriceAmount { get; }

		ZDecimal ItemChargeAmount { get; }

		[DecimalPlaces(6)]
		ZDecimal SubTotalAmount { get; }
	}

	public interface IPreviousDocument
	{
		ZString FunctionalReferenceID { get; }

		ZString ID { get; }

		ZInt LineNumeric { get; }
	}

	public interface ICommodityNumber
	{
		ZString ID { get; }

		ZString IdentifierTypeCode { get; }
	}

	public interface IDutyOtherTaxFee
	{
		ZString MethodCode { get; }

		ZString MethodOfCalculation { get; }

		[DecimalPlaces(5)]
		ZDecimal TaxRateNumeric { get; }

		ZString TypeCode { get; }

		[DecimalPlaces(4)]
		ZDecimal PercentageNumeric { get; }
	}

	public interface IOrigin
	{
		ZString CountryCode { get; }

		IAdditionalDocument AdditionalDocument { get; }
	}

	public interface IGoodsStatisticalMeasure
	{
		ZString StatisticalUnitCode { get; }

		[DecimalPlaces(4)]
		ZDecimal TariffQuantity { get; }
	}

	public interface IDeclarationPackaging
	{
		[MaxLength(512)]
		ZString MarksNumbers { get; }

		ZString PackagingMaterialDescription { get; }

		ZString Combination { get; }

		ZString TypeCode { get; }
	}

	public interface IConsignmentItem
	{
		ZString Split { get; }

		ICommodity Commodity { get; }

		IGoodsMeasure GoodsMeasure { get; }

		IPackaging Packaging { get; }

		IEnumerable<ITransportContractDocument> TransportContractDocuments { get; }

		IOrigin Origin { get; }

		ZString AssociatedGovernmentProcedureCode { get; }
	}

	public interface ILPCODetail
	{
		ZString LPCOExemptionCode { get; }

		ZString LPCOID { get; }

		ILPCOAuthorizedParty LPCOAuthorizedParty { get; }
	}

	public interface ILocation
	{
		ZString ID { get; }

		ZString Name { get; }

		ZDate LoadingDateTime { get; }

		ZString EstimatedLoadingCode { get; }
	}

	public interface IDeclarationAdditionalDocument
	{
		ZString ID { get; }

		ZDateTime LPCOExpirationDateTime { get; }
	}

	public interface IDeclarationAdditionalInformation
	{
		ZString StatementDescription { get; }
	}

	public interface IApplication
	{
		ZString FunctionalReferenceID { get; }

		ZString ID { get; }

		ZString PurposeCode { get; }

		ZString TypeCode { get; }

		IEnumerable<IAdditionalDocument> AdditionalDocuments { get; }

		IApplicationAdditionalInformation AdditionalInformation { get; }

		IPartyDetails Agent { get; }

		ZString BankAccount { get; }

		ZString ContactOffice { get; }

		IPayment Payment { get; }

		ZString ResponsibleGovernmentAgency { get; }

		IAppointment Appointment { get; }

		ZString ApprovalAuthenticationInformation { get; }

		IAuthorizedInformation AuthorizedInformation { get; }

		IPartyDetails Declarer { get; }

		IEnumerable<ZInt> ItemGroupReferenceSequenceNumerics { get; }

		IEnumerable<ILabel> Labels { get; }

		IPartyDetails LocalManufacturer { get; }

		IApplicationWine Wine { get; }

		IPartyDetails Applicant { get; }
	}

	public interface IApplicationWine
	{
		IEnumerable<IAdditionalDocument> AdditionalDocuments { get; }

		ZString GovernmentProcedurePreviousCode { get; }

		IPreviousDocument PreviousDocument { get; }
	}

	public interface ILabel
	{
		ZString StatusNameCode { get; }

		IEnumerable<ILabelDetail> LabelDetails { get; }
	}

	public interface ILabelDetail
	{
		ZString EndNumber { get; }

		ZString StartNumber { get; }

		ZString Track { get; }

		ZString Year { get; }
	}

	public interface IAuthorizedInformation
	{
		ZString AuthorizedTypeCode { get; }

		IAdditionalDocument AdditionalDocument { get; }
	}

	public interface IAppointment
	{
		ZDateTime ReservationDate { get; }

		ZString ReservationPeriodCode { get; }
	}

	public interface IApplicationAdditionalInformation
	{
		ZString StatementDescription { get; }

		ZString DeductionSample { get; }

		ZString ElectronicReceipt { get; }

		ZString ProvedPaper { get; }

		ZString ReturnSample { get; }

		ZString AddressChineseLine { get; }

		ZString BulkApplicationID { get; }

		ZString BulkPortCode { get; }
	}

	public interface IShippingIdentification
	{
		ZString LotNumberID { get; }

		ZDateTime ProductBestBeforeDateTime { get; }

		[DecimalPlaces(4)]
		ZDecimal ProductLotNumberAmount { get; }

		ZDateTime ProductManufacturedDate { get; }
	}

	public interface IGoodsLicensingStatisticalMeasure
	{
		[DecimalPlaces(4)]
		ZDecimal LicensingQuantity { get; }

		ZString StatisticalUnitCode { get; }

		ZString ResponsibleGovernmentAgency { get; }
	}

	public interface ICommoditySpecification
	{
		ZString CharacteristicQualifierCode { get; }

		ZString ElementDescription { get; }
	}

	public interface IWine
	{
		ZInt AgeNumeric { get; }

		[DecimalPlaces(3)]
		ZDecimal AlcoholContentNumeric { get; }

		ZDateTime BottledDate { get; }

		[DecimalPlaces(4)]
		ZDecimal CoverLotNumberAmount { get; }

		ZString GeographicRegion { get; }

		[DecimalPlaces(4)]
		ZDecimal OriginalNonLotNumberAmount { get; }

		ZDateTime ProductBestBeforeDateTime { get; }

		ZDateTime ProductExpiryDateTime { get; }

		[DecimalPlaces(4)]
		ZDecimal RemoveLotNumberAmount { get; }

		ZInt YearNumeric { get; }
	}

	public interface IVehicle
	{
		ZString Catalyst { get; }

		ZString ClassificationCode { get; }

		ZInt CylinderQuantity { get; }

		ZString Displace { get; }

		ZInt DoorQuantity { get; }

		ZString DrivingSide { get; }

		ZString FuelTypeCode { get; }

		ZInt ModelYearNumeric { get; }

		ZInt SeatQuantity { get; }

		ZString StatusCode { get; }

		ZString TransmissionTypeCode { get; }

		IEnumerable<ZString> VehicleIDs { get; }
	}

	public interface IQuarantine
	{
		ZString ObjectFeature { get; }

		ZString Treatment { get; }

		IAnimal Animal { get; }

		IEnumerable<IAdditionalDocument> AdditionalDocument { get; }

		IEnumerable<IAdditionalInformation> AdditionalInformation { get; }

		IEnumerable<IPackaging> Packing { get; }
	}

	public interface IAnimal
	{
		ZInt AgeMonthNumeric { get; }

		ZInt AgeYearNumeric { get; }

		ZInt FemaleQuantity { get; }

		ZInt MaleQuantity { get; }

		ZString MicrochipID { get; }

		ZString Vaccination { get; }
	}

	public interface IFood
	{
		[DecimalPlaces(1)]
		ZDecimal? PHValueNumeric { get; }

		[DecimalPlaces(1)]
		ZDecimal? SterilizationValueNumeric { get; }

		IEnumerable<IFoodConstituent> Constituents { get; }
	}

	public interface IFoodConstituent
	{
		ZString ElementName { get; }

		[DecimalPlaces(4)]
		ZDecimal ElementPercentNumeric { get; }
	}

	public interface IDutyTaxFeeQuantity
	{
		ZString DutyUnitCode { get; }

		[DecimalPlaces(4)]
		ZDecimal TaxRateNumeric { get; }

		[DecimalPlaces(4)]
		ZDecimal PercentageNumeric { get; }
	}

	public interface IDutyTaxFeeAmount
	{
		[DecimalPlaces(5)]
		ZDecimal TaxRateNumeric { get; }

		[DecimalPlaces(4)]
		ZDecimal PercentageNumeric { get; }
	}

	public interface ICommodityDutyTaxFee
	{
		[DecimalPlaces(0)]
		ZDecimal AdValoremTaxBaseAmount { get; }

		ZString DutyRegimeCode { get; }

		[DecimalPlaces(6)]
		ZDecimal SpecificTaxBaseQuantity { get; }

		[DecimalPlaces(4)]
		ZDecimal PercentageNumeric { get; }
	}

	public interface ICommodityRelatedPackaging
	{
		ZString PackingMethodDescription { get; }

		ZString MaterialCode { get; }

		ZString Specification { get; }
	}

	public interface IGoodsShipmentDutyTaxFee
	{
		[DecimalPlaces(0)]
		ZDecimal AdValoremTaxBaseAmount { get; }

		ZString TypeCode { get; }
	}

	public interface IPayment
	{
		ZString MethodCode { get; }

		ZString ReferenceID { get; }
	}

	public interface IAdditionalDeclaration
	{
		ZString ID { get; }

		ZString TypeCode { get; }

		ZDecimal SequenceNumeric { get; }
	}
}
