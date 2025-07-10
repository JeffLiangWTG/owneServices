using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.Testing
{
	sealed class CUSDECMessageDataProviderForTest : ICUSDECMessageDataProvider
	{
		public CUSDECMessageDataProviderForTest()
		{
		}

		#region Fields For BGM
		public ZString ShipmentType { get; set; }

		public ZString LocalReferenceNumber { get; set; }

		public ZInt PartClearanceQuantity { get; set; }

		public ZString MessageType { get; set; }

		public ZString DeclarationType { get; set; }

		#endregion
		#region Fields For CST
		public ZString CustomsProcedureCategory { get; set; }

		#endregion
		#region Fields For LOC
		public ZString TransportDocumentIssuedAt { get; set; }

		public ZString LocationOfGoods { get; set; }

		public ZString FromWarehouse { get; set; }

		public ZString ToWarehouse { get; set; }

		public ZString Consignee { get; set; }

		public ZString PortOfExit { get; set; }

		public ZString CountryOfExport { get; set; }

		public ZString CountryOfDestination { get; set; }

		public ZString CustomsOfficeCode { get; set; }

		#endregion
		#region Fields For DTM
		public ZDateTime DateOfAssessment { get; set; }

		public ZDateTime DateOfArrival { get; set; }

		public ZDateTime DateOfDepartureOrDateOfFlight { get; set; }

		public ZDateTime ShippedOnBoardDate { get; set; }
		#endregion
		#region Fields For GIS
		public ZString RelatedPartyIndicator { get; set; }

		public ZString RelatedPartyIndicatorForSADDocumentPackPurposes => RelatedPartyIndicator;
		public ZString ValuationCode { get; set; }

		public ZString ValuationCodeForSADDocumentPackPurposes => ValuationCode;
		public ZString PaymentMethod { get; set; }

		public ZString RefundAcknowledgementIndicator { get; set; }

		#endregion
		#region Fields For MEA
		public ZDecimal GrossWeightInKG { get; set; }

		#endregion
		#region Containers
		public IEnumerable<IContainerInformation> Containers { get; set; }

		#endregion
		#region Fields For LIN
		public ZInt TotalLineCount { get; set; }

		public ZString CreditTerms { get; set; }

		public ZString VATIndicator { get; set; }

		public ZString TransactionBankCode { get; set; }

		public ZString ChangeAcknowledgementIndicator { get; set; }

		#endregion
		#region Fields For SG1
		public ZString HouseBill { get; set; }

		public ZDateTime HouseBillIssuedDate { get; set; }

		public ZString OriginalMRN { get; set; }

		public ZString MRNToBeReplaced { get; set; }

		public ZString TransportDocumentNumber { get; set; }

		public ZDateTime TransportDocumentDate { get; set; }

		public ZString FinancialAccountNumber { get; set; }

		public ZBool ShouldOutputFinancialAccountNumber { get; set; }

		public ZString UniqueConsignmentReference { get; set; }

		public ZString MessageNumber { get; set; }

		public ZString CaseNumber { get; set; }

		#region Fields For SG2
		public ZString TotalNoOfPacks { get; set; }

		#region Fields for SG3
		public IEnumerable<ZString> MarksAndNumbers { get; set; }

		#endregion
		#endregion
		#endregion
		#region Fields For SG4
		public ZString TransportMode { get; set; }

		public ZString VoyageFlightNo { get; set; }

		public ZString RemovalTransportMode { get; set; }

		public ZString TransportName { get; set; }

		#endregion
		#region Fields For SG5
		public IEnumerable<IInvoiceInformation> InvoiceInformations { get; set; }

		#endregion
		#region Fields For SG6
		public ZString AgentCode { get; set; }

		public ZString AgentDualProfileCode { get; set; }

		public ZString OwnerCode { get; set; }

		public ZString RemoverTransporterCode { get; set; }

		public IAddressInformation Importer { get; set; }

		public IAddressInformation ImporterForExportJob { get; set; }

		public IAddressInformation Supplier { get; set; }

		public IAddressInformation Exporter { get; set; }

		public IAddressInformation UnregisteredTrader { get; set; }

		public ZString VesselAgent { get; set; }

		public ZString MasterCargoCarrier { get; set; }

		#endregion
		#region Fields For SG10
		public ZBool ShouldOutputInvoiceDetails { get; set; }

		public ZDateTime ExchangeRateDateTime { get; set; }

		public ZDateTime DateForDuty { get; set; }

		public ZString PaymentTerms { get; set; }

		public IEnumerable<IInvoiceHeaderInformation> InvoiceHeaderInformations { get; set; }
		#endregion
		#region Fields for SG30
		public IEnumerable<ILineLevelInformation> LineLevelDetails { get; set; }

		#endregion
		#region Fields For SG49
		public ZDecimal TotalCIFCAmount { get; set; }

		public ZDecimal TotalTransactionValue { get; set; }

		public ZString TotalTransactionValueCurrency { get; set; }

		public ZBool ShouldOutputDutiesDueWhenZero { get; set; }

		public ZDecimal TotalDutiesDue { get; set; }

		public ZBool ShouldOutputVATDueWhenZero { get; set; }

		public ZDecimal TotalVATDue { get; set; }

		public ZBool IsIntoWarehouseWarehousing { get; set; }

		public ZDecimal OverpaidExcise { get; set; }

		public ZDecimal UnderpaidExcise { get; set; }

		public ZDecimal TotalCustomsValue { get; set; }

		public ZBool ShouldOutputTotalTransactionValueAndCurrency => true;
		#endregion
	}
}
