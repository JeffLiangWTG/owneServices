
namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders
{
	using System.Collections.Generic;
	using CargoWise.Types;

	/// <summary>
	/// o	Details of goods for import or export clearance
	///	o	Submitted by Traders or their brokers
	/// </summary>
	public interface IDeclaration : ITSWSubmitter
	{
		ZBool IsSea { get; }
		ZBool IsAir { get; }
		ZBool IsMail { get; }
		ZBool IsContainerised { get; }
		ZBool IsCompletionEntry { get; }
		ZBool HasContainersOrPallets { get; }
		ZString MessageType { get; }
		ZString PaymentType { get; }
		ZString SenderReferenceNumber { get; }
		ZString TSWReferenceNumber { get; }
		IAdditionalInformation AdditionalInformation { get; }
		IEnumerable<IOtherInfo> OtherInfoCodes { get; }
		IEnumerable<ZString> Permits { get; }
		IEnumerable<IOtherInfo> OtherReferencedDocuments { get; }
		ZString HandlingInformation { get; }
		ZString MPIAccountDetails { get; }
		ZInt TransactionType { get; }
		ZDecimal TotalGrossWeightInKGM { get; }
		ZString TotalGrossWeightUnit { get; }
		ZString BrokerCode { get; }
		ZString PremiseID { get; }
		ZString CraftName { get; }
		ZString LloydsNo { get; }
		ZString VoyageNo { get; }
		ZString FlightNo { get; }
		IEnumerable<ICurrency> ExchangeRates { get; }
		ZDateTime DepartureDate { get; }
		IOrganisationSimple Carrier { get; }
		IDeclarant Declarant { get; }
		IEnumerable<IDutyTaxFee> DutyTaxFees { get; }
		IEnumerable<IMasterBillTransportDocument> MasterBills { get; }
		IEnumerable<IAssociatedTransportDocument> AllBills { get; }
		IEnumerable<ITransportEquipment> Equipment { get; }
		IEnumerable<IPackaging> Packaging { get; }
		ZString PreviousDocumentNo { get; }
		ZString PreviousDocumentType { get; }
	}

	public interface IGoodsShipment
	{
		ZString ShipmentOrigin { get; }
		ZString NatureOfTransaction { get; }
		ZBool MAFContainerDeclaration { get; }
		IEnumerable<ZString> MAFContainerStatements { get; }
		IEnumerable<ZString> MPIApprovedSystemNumbers { get; }
		ZString LocationOfGoods { get; }
		ZString PortOfLoading { get; }
		ZString PortOfDischarge { get; }
		ZDecimal FreightCostsInNZD { get; }
		ZString FreightApportionmentMethod { get; }
		IOrganisation DeliverToParty { get; }
		ZString CustomsControlledArea { get; }

		IEnumerable<IGoodsItems> Items { get; }
		IEnumerable<IInvoice> Invoices { get; }
		IEnumerable<IOrganisationSimple> NotifyParties { get; }
		IEnumerable<ZString> NotifyPartyCodes { get; }
		IEnumerable<IOrganisation> Sellers { get; }
		IEnumerable<IOrganisation> StuffingEstablishments { get; }
		IEnumerable<IOrganisation> Suppliers { get; }
	}

	public interface IGoodsItems
	{
		ZDecimal ValueForDutyInNZD { get; }
		ZString TransitionalFacilityCode { get; }
		ZString GoodsDescription { get; }
		ZString LotNumber { get; }
		ZDate DateMarking { get; }
		ZBool HasForeignCurrency { get; }
		ZDecimal ValueInForeignCurrency { get; }
		ZString ForeignCurrencyCode { get; }
		ZString IntendedUse { get; }
		ZString IntendedUseCode { get; }
		IEnumerable<IClassification> Classifications { get; }
		IEnumerable<IConstituent> Constituents { get; }
		ZString PreferenceClaimed { get; }
		IEnumerable<IDutyTaxFee> LineDutyTaxFees { get; }
		IOrganisation Grower { get; }
		IEnumerable<ZString> RoutingCountryCodes { get; }
		IOrganisation Manufacturer { get; }
		IOrganisation Producer { get; }
		IEnumerable<IProduct> Products { get; }
		ZString BrandName { get; }
		ZString CommonName { get; }
		ZString RegisteredName { get; }
		ZString TradeName { get; }
		ZBool UsedGoods { get; }
		ZBool GeneticallyModified { get; }
		ZString ExportCountry { get; }
		ITemperatureRequirements Temperatures { get; }
		IEnumerable<ZString> ContainerNumbers { get; }
		IOrganisationSimple TreatmentProvider { get; }
		ZDecimal ItemGrossWeightInKGM { get; }
		ZDecimal ItemNetWeightInKGM { get; }
		ZDecimal StatisticalQty { get; }
		ZString StatisticalQtyUnit { get; }
		ZDecimal SupplementaryQty { get; }
		ZString SupplementaryQtyUnit { get; }
		ZString OriginCountry { get; }
		ZString OriginRegion { get; }
		IEnumerable<IPackaging> Packaging { get; }
		IEnumerable<IValuationAdjustment> Adjustments { get; }
		IEnumerable<ZString> Permits { get; }
		IEnumerable<ZString> ProhibitedCodes { get; }
		IEnumerable<IOtherInfo> OtherInfoCodes { get; }
		ZString RelationshipIndicator { get; }
		ZInt SupplierLineIsRelatedTo { get; }
		ZBool IsPartsRelated { get; }
		ZString IsGSTPrePaid { get; }
		ZString VendorIdentifier { get; }
	}

	public interface IClassification
	{
		ZString Classification { get; }
		ZString ClassificationTypeCode { get; }
	}

	public interface IConstituent
	{
		ZDecimal ConstituentQuantity { get; }
		ZString ConstituentName { get; }
	}

	public interface IProduct
	{
		ZString Id { get; }
		ZString IdType { get; }
	}

	public interface ITemperatureRequirements
	{
		ZDecimal StorageTemp { get; }
		ZString StorageTempUnit { get; }
		ZDecimal MinStorageTemp { get; }
		ZString MinStorageTempUnit { get; }
		ZDecimal MaxStorageTemp { get; }
		ZString MaxStorageTempUnit { get; }
	}
}
