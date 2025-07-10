using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public interface IACEDrawbackSummary
	{
		BusinessObjectFactory Factory { get; }
		void AddMessage(MQEDIMessage message);
		ZString MessageStatus { get; set; }
		ZBool IsTFTEARequired { get; }

		// B-Record
		ZString ProcessingOfficeCode { get; }
		ZString ProcessingPort { get; }

		// 10-Record
		ZString ActionRequestCode { get; }
		ZString EntryFilerCode { get; }
		ZString EntryNumber { get; }
		ZString ClaimPort { get; }
		ZString BrokerReferenceNumber { get; }
		ZString ClaimType { get; }
		ZString BondWaiverIndicator { get; }
		ZString BondWaiverReasonCode { get; }
		ZString AcceleratedClaimIndicator { get; }
		ZString OneTimeWaiverIndicator { get; }
		ZString WavierOfPriorNoticeIndicator { get; }
		ZString CommercialInterchangeability { get; }
		ZString ElectronicPetroleumCertification { get; }
		ZString ElectronicManufacturingPetroleumCertification { get; }
		ZString OilSpillTaxCertification { get; }
		ZString NAFTADrawbackClaimIndicator { get; }
		ZString USMCADrawbackClaimIndicator { get; }
		ZString ImporterOfRecordNumber { get; }
		ZString NotifyParty4811Number { get; }
		ZString SubstitutedUnusedWineCertification { get; }
		ZString BillOfMaterialsFormulaCertification { get; }
		ZString CertificationForValuationOfDestroyedMerchandise { get; }
		ZString RetailSalesSubstitution { get; }
		ZString SuperfundTaxCertification { get; }

		// 62-Record
		ZString IntendedPort { get; }
		ZString ExaminationWitnessIndicator { get; }
		ZString LocationOfDestruction { get; }
		ZString ResultsOfExamination { get; }

		// 90-Record
		ZDecimal GrandTotalDuty { get; }
		ZDecimal GrandTotalUserFee { get; }
		ZDecimal GrandTotalIRTax { get; }

		IEnumerable<IACEDrawbackBondInfo> BondDetails { get; }
		IEnumerable<IACEDrawbackImportClaim> ImportsEntrySummaryDetails { get; }
		IEnumerable<IACEDrawbackManufactureClaim> ManufacturedArticles { get; }
		IEnumerable<IACEDrawbackExportClaim> ExportArticles { get; }
		IEnumerable<IACEDrawbackNoticeOfIntent> NoticeOfIntentDetais { get; }
		IEnumerable<IACEDrawbackNAFATTariff> NAFTADetails { get; }
		IEnumerable<IACEDrawbackTFTEAClaim> TFTEADetails { get; }
		IEnumerable<IACEDrawbackRevenueTotals> RevenueTotals { get; }
		IEnumerable<IACEDrawbackTrackingNumberLine> TrackingNumberLines { get; }
	}

	public interface IACEDrawbackBondInfo
	{
		// 31-Record
		ZString BondType { get; }
		ZString BondDesignationTypeCode { get; }
		ZString SuretyCode { get; }
		ZDecimal BondAmount { get; }
		ZString ProducerAccountNumber { get; }
	}

	public interface IACEDrawbackImportClaim
	{
		// 40-Record
		ZString ActionIndicator { get; }
		ZString EntryFilerCode { get; }
		ZString EntryNumber { get; }
		ZString CBPESLine { get; }
		ZString CertificateofDeliveryIndicator { get; }
		ZString ManufacturerRulingNumber { get; }
		ZString BasisOfClaim { get; }
		ZDateTime ManufDateReceived { get; }
		ZDateTime ManufDateUsed { get; }
		ZString TrackingIdentificationNumber { get; }
		ZString DrawbackAccountingMethodCode { get; }

		IEnumerable<IACEDrawbackImportClassification> ImportClassifications { get; }
		IEnumerable<IACEDrawbackRevenueClaimed> RevenueAmounts { get; }
	}

	public interface IACEDrawbackImportClassification
	{
		// 41-Record
		ZString HTSNumber { get; }
		ZString DescriptionText { get; }

		IACEDrawbackExportQuantityAndUnit ExportQuantityAndUnit { get; }
	}

	public interface IACEDrawbackExportQuantityAndUnit
	{
		// 42-Record
		ZDecimal Quantity { get; }
		ZString UnitOfMeasure { get; }
		ZDecimal AllowableQuantity { get; }
		ZDecimal GoodsValuePerUnit { get; }
		ZDecimal SubstitutedValuePerUnit { get; }
	}

	public interface IACEDrawbackRevenueClaimed
	{
		// 43-Record
		ZString AccountingClassCode { get; }
		ZDecimal ClaimAmount { get; }
		ZDecimal CalculatedAmount { get; }
		ZDecimal AdjustedClaimAmount { get; }
		ZString QualifierIndicator { get; }
	}

	public interface IACEDrawbackManufactureClaim
	{
		// 50-Record
		ZString ActionIndicator { get; }
		ZString ImportManufactureRulingNumber { get; }
		ZString HTSNumber { get; }
		ZDecimal Quantity { get; }
		ZString UnitOfMeasure { get; }
		ZDateTime ProductionDate { get; }
		ZString FactoryLocation { get; }

		// 51-Record
		ZString DescriptionText { get; }
		ZString ManufactureRulingNumber { get; }

		ZString ImportTrackingID { get; }
		ZString ManufacturedTrackingID { get; }
	}

	public interface IACEDrawbackExportClaim
	{
		// 60/70-Record
		ZString ExportDestroyIndicator { get; }
		ZString HTSNumber { get; }
		ZDecimal Quantity { get; }
		ZString UnitOfMeasure { get; }
		ZDateTime ExportDate { get; }
		ZString NoticeOfIntentIndicator { get; }
		ZString WaiverToDrawbackIndicator { get; }
		ZString NameOfExporter { get; }
		ZString CountryOfUltimateDestination { get; }
		ZString BOLIndicator { get; }
		ZString BOLCarrierCode { get; }

		// 61/71-Record
		ZString DescriptionText { get; }
		ZString UniqueIdentifierNumber { get; }

		// 72 & 73 Record
		ZString ImportTrackingNumber { get; }
		ZString ManufacturedTrackingNumber { get; }
	}

	public interface IACEDrawbackTFTEAClaim : IACEDrawbackExportClaim
	{
		// 70-Record
		ZString ScheduleBCode { get; }
	}

	public interface IACEDrawbackNoticeOfIntent
	{
		// 63-Record
		ZString RecordIndicator { get; }
		ZString NameOfCBPPersonnel { get; }
		ZString CBPPersonnelBadge { get; }
		ZString CBPPersonnelPhone { get; }
		ZDateTime ProcessingExaminAtionDate { get; }
	}

	public interface IACEDrawbackNAFATTariff
	{
		// 64-Record
		ZString EntryNumber { get; }
		ZDateTime EntryDate { get; }
		ZDecimal DutyPaidToForeignGov { get; }
		ZDecimal ExchangeRate { get; }
		ZString TariffNumber1 { get; }
		ZString TariffNumber2 { get; }
		ZString TariffNumber3 { get; }
		ZString CountryOfExport { get; }
	}

	public interface IACEDrawbackRevenueTotals
	{
		// 89-Record
		ZString AccountingClassCode { get; }
		ZDecimal TotalAmount { get; }
	}

	public interface IACEDrawbackTrackingNumberLine
	{
		ZInt ArrangeTrackingNumbers(ZInt startIndex);
	}
}
