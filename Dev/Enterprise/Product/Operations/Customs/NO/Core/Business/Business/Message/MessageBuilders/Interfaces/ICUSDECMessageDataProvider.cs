using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Edifact.V902.Elements;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NO.Business;

public interface ICUSDECMessageDataProvider
{
	IEDIMessageCollectionProvider AsMessageCollectionProvider();

	#region Fields For UNH - Message Header
	ZString DeclarationReferenceNumber { get; }
	ZString AssociationAssignedCode { get; }
	ZString LocalReferenceNumber { get; }
	#endregion

	#region Fields For BGM - Beginning of Message
	DocumentMessageNameCodedList DocumentMessageName { get; }
	ZString MessageType { get; }
	#endregion

	#region Fields For CST - Customs Status
	ZString DeclarationType { get; }
	ZString TransactionNature { get; }
	#endregion

	#region Fields For RFF - Reference
	ZString ControlNumber { get; }
	ZString GoodsNumber { get; }
	ZString GoodsNumberPosition { get; }
	ZString RelatedDeclaration { get; }
	#endregion

	#region Fields For LOC - Location
	ZString GoodsDestination { get; }
	ZString GoodsOrigin { get; }
	ZString CustomsOfficeOfExit { get; }
	ZString LocationOfGoods { get; }
	#endregion

	#region Fields for TDT - Transport Details
	ZString TransportMode { get; }
	ZString TransportNationality { get; }
	#endregion

	#region Fields for GIS - General Indicator
	ZString ContainerMode { get; }
	#endregion

	#region Fields for NAD - Name and Address
	ZString ExporterCustomsRegNo { get; }
	ZString ImporterCustomsRegNo { get; }
	PartyQualifierList SenderAddressType { get; }
	ZString SenderFullName { get; }
	ZString SenderAddress1 { get; }
	ZString SenderAddress2 { get; }
	ZString SenderAddress3 { get; }
	ZString DeclarantCustomsRegNo { get; }
	ZString PaymentMethod { get; }
	ZString CustomsControllingUnit { get; }
	#endregion

	#region Fields for CTA - Contact
	ZString InitialsOfDeclarant { get; }
	#endregion

	#region Fields for TOD - Terms of Delivery
	ZString IncoTerm { get; }
	ZString IncoTermPlace { get; }
	#endregion

	#region FTX - FreeText
	ZString ReExportReason { get; }
	#endregion

	#region Fields for MOA - Monetary Amount
	ZString CommercialInvoiceAmount { get; }
	ZString CommercialInvoiceCurrency { get; }
	ZString FreightAmountNOK { get; }
	#endregion

	#region Fields for CUX - Currencies
	ZString CurrencyExchangeRate { get; }
	#endregion

	#region Fields for DMS - Document Message Summary
	IReadOnlyCollection<IDocumentMessageSummary> DocumentMessageSummaryCollection { get; }

	public partial interface IDocumentMessageSummary
	{
		ZString InvoiceNumber { get; }
		ZString InvoiceDate { get; }
	}
	#endregion

	#region Fields for CST - Customs Status of Goods
	public partial interface IDocumentMessageSummary
	{
		IReadOnlyCollection<IItemDetails> ItemDetailsCollection { get; }
	}

	public partial interface IItemDetails
	{
		ZString DeclarationLineNumber { get; }
		ZString TariffNumber { get; }
		ZString ProcedureCode { get; }
		ZString PreferenceCode { get; }
		ZString ValuationMethod { get; }
		ZString ReducedCustomsFlag { get; }
	}
	#endregion

	#region Fields for PAC - Package
	public partial interface IItemDetails
	{
		ZString NumberOfPackages { get; }
		ZString PackageType { get; }
	}
	#endregion

	#region Fields for PCI - Package Identification
	public partial interface IItemDetails
	{
		ZString GeneralGoodsMarks { get; }
		ZString ChassisNumber { get; }
		IReadOnlyCollection<ZString> ContainerNumbers { get; }
	}
	#endregion

	#region Fields for LOC - Location
	public partial interface IItemDetails
	{
		ZString CountryOfOrigin { get; }

		ZString RegionOfOrigin { get; }
	}
	#endregion

	#region Fields for MOA - Monetary Amount
	public partial interface IItemDetails
	{
		ZString AdjustedValue { get; }
		ZBool AdjustedValueIsDiscount { get; }
		ZString StatisticValue { get; }
		ZString AddedValueDueToProcessingAbroad { get; }
	}
	#endregion

	#region Fields for MEA - Measurements
	public partial interface IItemDetails
	{
		ZString GrossWeight { get; }
		ZString NetWeight { get; }
		ZString CustomsQtyAmount { get; }
		ZString CustomsQtyUnitOfMeasurement { get; }
	}
	#endregion

	#region Fields for TAX - Duty/Tax/Fee Details
	public partial interface IItemDetails
	{
		IReadOnlyCollection<IItemDetailsFee> ItemDetailsFeeCollection { get; }
	}

	public interface IItemDetailsFee
	{
		ZString FeeAmount { get; }
		ZString FeeType { get; }
		ZString FeeTypeSequence { get; }
		ZString FeeBaseValueForCalculation { get; }
		ZString FeeRate { get; }
	}
	#endregion

	#region Fields for DCR - Documentary Requirement
	public partial interface IItemDetails
	{
		IReadOnlyCollection<IItemDetailsSupportingDocuments> ItemDetailsSupportingDocumentsCollection { get; }
	}

	public interface IItemDetailsSupportingDocuments
	{
		ZString DocumentCode { get; }
		ZString DocumentNumberOrText { get; }
	}
	#endregion

	#region fields for FTX - Free Text
	public partial interface IItemDetails
	{
		ZString[] GoodsDescriptions { get; }
	}
	#endregion

	#region Fields for TAX - Summary section
	IReadOnlyCollection<ITotalFeeLines> TotalFeeCollection { get; }

	ZString TotalFeeAmount { get; }

	public interface ITotalFeeLines
	{
		ZDecimal Amount { get; }
		ZString FeeCode { get; }
	}
	#endregion

	#region Fields for CNT - Control Totals

	ZString TotalNoOfItemLines { get; }
	ZString TotalNoOfPackages { get; }
	#endregion
}
