using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// LC can be a plug-in to a plug-in like Declaration to Shipment. Shipment should implement this
	/// </summary>
	public interface ILandedCostHeaderProvider
	{
		ILandedCostHeader LCHeaderHost { get; }
	}

	public struct DutyTaxEntryFee
	{
		Dictionary<string, ZDecimal> Fees { get { return fFees ?? (fFees = new Dictionary<string, ZDecimal>()); } }
		Dictionary<string, ZDecimal> fFees;

		public ZDecimal this[string feeType]
		{
			get { return Fees.ContainsKey(feeType) ? Fees[feeType] : ZDecimal.Zero; }
			set { Fees[feeType] = value; }
		}
	}

	/// <summary>
	/// Business Object which LC plugs into should implement this, eg JobDeclaration, Order
	/// </summary>
	public interface ILandedCostHeader
	{
		ZGuid PK { get; }
		ZString TableCode { get; }
		bool IsLCSupported { get; }
		event EventHandler OnLCSupportedChanged;
		event EventHandler OnExchangeRateHolderDeleted;

		string MessageShownWhenLCIsNotSupported { get; }
		bool IsJobInLCRunnableState { get; }
		void DoStuffBeforeRunningLCDistribution();

		bool HasMultiInvoices { get; }
		ZString UniqueReferenceNumber { get; }
		OrgHeader Consignee { get; }
		IEnumerable<ILandedCostChargeHolder> ChargeHolders { get; }
		IEnumerable<ILandedCostDistributeTo> CandidatesToDistributeCostTo { get; }
		IEnumerable<IUltimateDistributee> UltimateDistributees { get; }
		IEnumerable<ILandedCostExchangeRateHolder> ExchangeRateHolders { get; }

		DutyTaxEntryFee TotalDutyTaxEntryFeeItems { get; }

		ZDate DateOfEntry { get; }
		ZString LandedCostType { get; }
		bool IsAir { get; }
		bool IsInDatabase { get; }
		ZGuid CompanyPK { get; }

		BusinessObjectFactory Factory { get; }

		IComparer LineComparer { get; }
		ZDecimal DefaultEstimatedDutyPercent { get; }

		/// <summary>
		/// a set of exchange rates to default LandedCostInput.LI_ExRate from
		/// </summary>
		/// <returns>A dictionary whose key is a currency code and value is an exchange rate</returns>
		Dictionary<ZString, ZDecimal> GetDefaultExchangeRates();

		ZString JobNumber { get; }

		IHaveRequiredDocuments RequiredDocumentsProvider { get; }
		Type DocsAndCartageType { get; }
		Type DocsAndCartageParentType { get; }

		ZBool SupportsNoCostApportionmentItem { get; }
	}

	/// <summary>
	/// Business Object which Landed Cost is distributed to.
	/// Eg JobComInvoiceGroupHeader, JobComInvoiceHeader, JobComInvoiceLine
	/// </summary>
	public interface ILandedCostDistributeTo
	{
		ZGuid PK { get; }
		ZString TableCode { get; }
		ZString UniqueCode { get; }
		ZString Description { get; }
		IEnumerable<IUltimateDistributee> UltimateDistributees { get; }
	}

	/// <summary>
	/// Business Object from which Cost is imported
	/// eg. JobComInvoiceGroupHeader, JobHeader
	/// </summary>
	public interface ILandedCostChargeHolder
	{
		IEnumerable<IDefaultLandedCostInput> ChargesToImportForLandedCosting { get; }
	}

	public interface ILandedCostExchangeRateValidation
	{
		void ValidateExchangeRate();
	}

	public interface ILandedCostExchangeRateHolder
	{
		ZString CurrencyCode { get; }
		ZDecimal LandedCostExchangeRate { get; set; }
		ZDecimal LandedCostExchangeRateDefault { get; }
		ZString ReferenceNumber { get; }
		BusinessObjectFactory Factory { get; }
		ZGuid CompanyPK { get; }
	}

	/// <summary>
	/// Job
	/// </summary>
	public interface ILandedCostExchangeRateProvider
	{
		ZDecimal GetExRateFor(RefCurrency currency);
	}

	/// <summary>
	/// This matches one row of LandedCostInput
	/// </summary>
	public interface IDefaultLandedCostInput
	{
		ZGuid FKToChargeCode { get; }
		ZString ChargeDescription { get; }
		Money AmountToDistribute { get; }
		ZDecimal ExchangeRate { get; }
		ZBool IsValidToImport { get; }
	}

	public interface ILandedCostHistoryMaster
	{
		ZGuid PK { get; }
		SchemaGuidColumn FKSchemaColumnInLandedCostHistory { get; }
		ZBool ShouldMarginPercentagesReadOnly { get; }

		ZString CountryCode { get; }

		IEnumerable<ICustomsChargeLCItemSetting> CustomsChargeLCItemSettings { get; }
	}

	/// <summary>
	/// JobComInvoiceLine or OrderLine
	/// </summary>
	public interface IUltimateDistributee
	{
		ZGuid PK { get; }
		string TableCode { get; }

		ZGuid FKToProduct { get; }
		LCMarginPercentages GetProductSpecificLCMarginPercentagesForFallBack(OrgHeader consignee);
		ZString ProductCode { get; }
		ZString ProductDepartment { get; }
		ZString ProductDivision { get; }

		ZString HumanReadableCode { get; }
		ZPropertyInfo[] HumanReadableCodeInfos { get; }

		/// <summary>
		/// Actual weight for Air, actual volume for Sea
		/// </summary>
		ZDecimal Actual { get; }
		ZDecimal ActualWeightInKG { get; }
		ZDecimal ActualVolumeInM3 { get; }
		ZDecimal ItemCount { get; }

		ZDecimal DutyPercent { get; }
		ZDecimal CustomsValue { get; }
		DutyTaxEntryFee LineDutyTaxEntryFeeItems { get; }

		ZDecimal CostInLocalCurrency { get; }
		ZDecimal UnitPriceInInvoiceCurrency { get; }
		ZDecimal LinePriceInInvoiceCurrency { get; }

		ZString InvoiceNumber { get; }
		ZString SupplierName { get; }
		ZString InvoiceCurrencyCode { get; }

		ZString LineDescription { get; }
		ZString OrderNumber { get; }
		ZInt OrderLineNumber { get; }
		ZDecimal LandedCostingExRateFallingBackToJobInvoicingForInvoiceCurrency { get; }

		ZString InvoiceUQ { get; }
		ZDecimal CustomsQuantity { get; }
		ZString CustomsUQ { get; }
		ZString TariffNumber { get; }
		ZString CountryOfOriginCode { get; }
		ZString DutyRateDescription { get; }
		ZDecimal Weight { get; }
		ZString WeightUQ { get; }
		ZDecimal Volume { get; }
		ZString VolumeUQ { get; }

		/// <summary>
		/// For display only. It will not be part of final cost.
		/// </summary>
		ZDecimal GSTVATAmount { get; }

		IEnumerable<ICustomsFee> Fees { get; }

		ZBool IsCapableOfCalculatingOwnGstVatRate { get; }
		ZDecimal CalculateOwnGstVatRate();
	}

	public interface ICustomsFee
	{
		ZDecimal AmountInLocalCurrency { get; }
		ZString FeeCode { get; }
	}

	public interface ICustomsChargeLCItemSettingsProvider
	{
		IEnumerable<ICustomsChargeLCItemSetting> GetCustomsChargeLCItemSettings(string countryCode, bool? isIntegrated, ZGuid companyPK);
	}

	public interface ICustomsChargeLCItemSetting
	{
		ZString CostType { get; }
		ZString Description { get; }
		ZInt NumberOfDecimals { get; }
		ZBool IsDuty { get; }
		ZString DocumentCustomLabelCode { get; }
	}

	/// <summary>
	/// Quick POD - Accounting Job
	/// </summary>
	public interface IPODCharge
	{
		JobCharge CreateCharge { get; }
	}
}
