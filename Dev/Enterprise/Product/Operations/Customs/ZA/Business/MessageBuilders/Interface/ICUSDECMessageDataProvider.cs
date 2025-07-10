using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.MessageBuilders
{
	public interface IContainerInformation
	{
		ZString ContainerNumber { get; }
		ZString FirstSealNumber { get; }
		ZString SecondSealNumber { get; }
		ZString ContainerMode { get; }
	}

	public interface IInvoiceInformation
	{
		ZString InvoiceNumber { get; }
		ZDateTime InvoiceDate { get; }
	}

	public interface IInvoiceHeaderInformation : IInvoiceInformation
	{
		ZString NameOfIssuer { get; }
		ZString CountryOfIssuer { get; }
		ZString Address1 { get; }
		ZString Address2 { get; }
		ZString Address3 { get; }
		ZString Address4 { get; }
		ZDecimal TotalInvoiceAmount { get; }
		ZString InvoiceCurrencyCoded { get; }
		ZDecimal ExchangeRate { get; }
		ZDecimal TotalChargesInLocalCurrency { get; }
		ZDecimal CommonFactor { get; }
		ZString TermsOfDelivery { get; }
		ZDecimal AdvancePaymentAmount { get; }
		ZString AdvancePaymentCurrencyCode { get; }
		ZString[] AdvancePaymentNotificationDetails { get; }
		IEnumerable<IInvoiceLineInformation> InvoiceLineInformations { get; }
		IEnumerable<IInvoiceChargeInformation> InvoiceChargeInformations { get; }
		ZString PaymentTerms { get; }
	}

	public interface IInvoiceLineInformation
	{
		ZShort InvoiceLineNumber { get; }
		ZShort RelatedDeclarationLineNumber { get; }
		ZString ProductCode { get; }
		ZDecimal Quantity { get; }
		ZString QuantityUnit { get; }
		ZDecimal PriceDetails { get; }
		ZDecimal ItemAmount { get; }
		ZDecimal RateDetails { get; }
		ZString BrandName { get; }
		ZString CommercialInvoiceItemDescription { get; }
		IEnumerable<IInvoiceLineChargeInformation> InvoiceLineChargeInformations { get; }
	}

	public interface IInvoiceChargeInformation
	{
		ZString ChargeDescription { get; }
		ZString ChargeCurrency { get; }
		ZDecimal ChargeAmount { get; }
		ZString MonetaryAmountChargeType { get; }
		ZDecimal MonetaryDiscountAmount { get; }
		ZBool IsOtherCharge { get; }
		ZDecimal GetChargeCurrencyConversionRate(ZDateTime dateForRate);
	}

	public interface IInvoiceLineChargeInformation
	{
		ZString ChargeCurrency { get; }
		ZDecimal MonetaryDiscountAmount { get; }
	}

	public interface IAddressInformation
	{
		ZString OrganizationCode { get; }
		ZString OrganizationCodeQualifier { get; }
		ZString Name { get; }
		ZString Address { get; }
		ZString City { get; }
		ZString PostCode { get; }
		ZString VATRegistrationNo { get; }
	}

	public interface ILineLevelInformation
	{
		BusinessObjectFactory Factory { get; }
		ZString LineNumber { get; }
		ZString TariffCode { get; }
		ZString PreferenceCode { get; }
		ZDateTime DateOfAssessment { get; }

		#region Fields For SG30/FTX

		ZString GoodsDescription { get; }
		IEnumerable<IAdditionalInformation> AdditionalInformations { get; }
		ZString CustomsProcedureCode { get; }
		ZString PreviousProcedureCode { get; }
		ZString ProcedureMeasure { get; }
		ZString TradeStatisticsIndicator { get; }

		#endregion

		ZString CountryOfOrigin { get; }

		#region Fields For SG30/MEA

		ZDecimal CustomsQuantity { get; }
		ZString CustomsUnitQty { get; }
		ZDecimal AdditionalQuantity { get; }
		ZString AdditionalUnitQty { get; }
		ZDecimal ClassificationQuantity { get; }
		ZString ClassificationUnitQty { get; }
		ZDecimal WarehouseCountableQuantity { get; }
		ZString WarehouseCountableUnitQty { get; }

		#endregion

		#region Fields For SG30/NAD

		ZString RebateUserCode { get; }

		#endregion

		#region Fields For SG30/SG33/MOA

		ZDecimal ActualPrice { get; }
		ZDecimal CustomsValue { get; }

		#endregion

		#region Fields For SG30/SG35
		ZString PreviousProcedureMRN { get; }
		ZString WarehousingMRNLineNumber { get; }

		#endregion

		#region Fields For SG30/SG41/MOA

		IEnumerable<IDutyFeeInformation> DutiesAndFees { get; }
		IEnumerable<IDutyFeeInformation> ProvisionalPayments { get; }

		#endregion
	}

	public interface IAdditionalInformation
	{
		ZString Code { get; }
		ZString Value { get; }
		ZInt? Group { get; }
	}

	public interface IDutyFeeInformation
	{
		ZString Code { get; }
		ZDecimal Value { get; }
	}

	public interface ICUSDECMessageDataProvider
	{
		#region Fields For BGM

		ZString ShipmentType { get; }
		ZString LocalReferenceNumber { get; }
		ZInt PartClearanceQuantity { get; }
		ZString MessageType { get; }
		ZString DeclarationType { get; }

		#endregion

		#region Fields For CST

		ZString CustomsProcedureCategory { get; }

		#endregion

		#region Fields For LOC

		ZString TransportDocumentIssuedAt { get; }
		ZString LocationOfGoods { get; }
		ZString FromWarehouse { get; }
		ZString ToWarehouse { get; }
		ZString Consignee { get; }
		ZString PortOfExit { get; }
		ZString CountryOfExport { get; }
		ZString CountryOfDestination { get; }
		ZString CustomsOfficeCode { get; }

		#endregion

		#region Fields For DTM

		ZDateTime DateOfAssessment { get; }
		ZDateTime DateOfArrival { get; }
		ZDateTime DateOfDepartureOrDateOfFlight { get; }
		ZDateTime ShippedOnBoardDate { get; }

		#endregion

		#region Fields For GIS

		ZString RelatedPartyIndicator { get; }
		ZString RelatedPartyIndicatorForSADDocumentPackPurposes { get; }
		ZString ValuationCode { get; }
		ZString ValuationCodeForSADDocumentPackPurposes { get; }
		ZString PaymentMethod { get; }
		ZString RefundAcknowledgementIndicator { get; }

		#endregion

		#region Fields For MEA

		ZDecimal GrossWeightInKG { get; }

		#endregion

		#region Containers

		IEnumerable<IContainerInformation> Containers { get; }

		#endregion

		#region Fields For LIN

		ZInt TotalLineCount { get; }
		ZString CreditTerms { get; }
		ZString VATIndicator { get; }
		ZString TransactionBankCode { get; }
		ZString ChangeAcknowledgementIndicator { get; }

		#endregion

		#region Fields For SG1

		ZString HouseBill { get; }
		ZString OriginalMRN { get; }
		ZString MRNToBeReplaced { get; }
		ZString TransportDocumentNumber { get; }
		ZBool ShouldOutputFinancialAccountNumber { get; }
		ZString FinancialAccountNumber { get; }
		ZString UniqueConsignmentReference { get; }
		ZString MessageNumber { get; }
		ZString CaseNumber { get; }

		ZDateTime HouseBillIssuedDate { get; }
		ZDateTime TransportDocumentDate { get; }

		#region Fields For SG2

		ZString TotalNoOfPacks { get; }

		#region Fields for SG3

		IEnumerable<ZString> MarksAndNumbers { get; }

		#endregion

		#endregion

		#endregion

		#region Fields For SG4

		ZString VoyageFlightNo { get; }
		ZString TransportMode { get; }
		ZString RemovalTransportMode { get; }
		ZString TransportName { get; }

		#endregion

		#region Fields For SG5

		IEnumerable<IInvoiceInformation> InvoiceInformations { get; }

		#endregion

		#region Fields For SG6

		IAddressInformation Importer { get; }
		IAddressInformation ImporterForExportJob { get; }
		ZString AgentCode { get; }
		ZString RemoverTransporterCode { get; }
		IAddressInformation Supplier { get; }
		IAddressInformation Exporter { get; }
		ZString AgentDualProfileCode { get; }
		ZString OwnerCode { get; }
		IAddressInformation UnregisteredTrader { get; }
		ZString VesselAgent { get; }
		ZString MasterCargoCarrier { get; }

		#endregion

		#region Fields For SG10

		ZBool ShouldOutputInvoiceDetails { get; }
		ZDateTime ExchangeRateDateTime { get; }
		ZDateTime DateForDuty { get; }
		ZString PaymentTerms { get; }
		IEnumerable<IInvoiceHeaderInformation> InvoiceHeaderInformations { get; }

		#endregion

		#region Fields for SG30

		IEnumerable<ILineLevelInformation> LineLevelDetails { get; }

		#endregion

		#region Fields For SG49

		ZDecimal TotalCIFCAmount { get; }
		ZDecimal TotalTransactionValue { get; }
		ZString TotalTransactionValueCurrency { get; }
		ZBool ShouldOutputDutiesDueWhenZero { get; }
		ZDecimal TotalDutiesDue { get; }
		ZBool ShouldOutputVATDueWhenZero { get; }
		ZDecimal TotalVATDue { get; }
		ZBool IsIntoWarehouseWarehousing { get; }
		ZDecimal OverpaidExcise { get; }
		ZDecimal UnderpaidExcise { get; }
		ZDecimal TotalCustomsValue { get; }
		ZBool ShouldOutputTotalTransactionValueAndCurrency { get; }

		#endregion
	}
}
