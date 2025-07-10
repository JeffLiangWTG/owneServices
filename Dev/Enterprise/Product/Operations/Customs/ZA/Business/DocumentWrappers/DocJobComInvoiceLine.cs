using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers
{
	public class DocJobComInvoiceLine : DocBaseJobComInvoiceLine
	{
		public DocJobComInvoiceLine(JobComInvoiceLine jobComInvoiceLine, BusinessObjectFactory factoryToWrap)
			: base(jobComInvoiceLine, factoryToWrap)
		{
		}

		public static DocJobComInvoiceLine New(JobComInvoiceLine jobComInvoiceLine, BusinessObjectFactory factoryToWrap)
		{
			if (jobComInvoiceLine == null)
			{
				return null;
			}
			else
			{
				return new DocJobComInvoiceLine(jobComInvoiceLine, factoryToWrap);
			}
		}

		#region Overrides

		protected override DocBaseJobComInvoiceHeader CreateJobComInvoiceHeader(Customs.Business.BaseJobComInvoiceHeader invoiceToWrap)
		{
			return DocJobComInvoiceHeader.New((JobComInvoiceHeader)invoiceToWrap, Factory);
		}

		protected override DocBaseCusEntryLine CreateCusEntryLine(Customs.Business.CusEntryLine entryLineToWrap)
		{
			return DocCusEntryLine.New((CusEntryLine)entryLineToWrap, Factory);
		}

		#endregion

		#region Wrapper Fields

		public DocCusEntryLine EntryLine
		{
			get { return (DocCusEntryLine)CusEntryLineInternal; }
		}

		public DocJobComInvoiceHeader ComInvoiceHeader
		{
			get { return (DocJobComInvoiceHeader)InvoiceHeaderInternal; }
		}

		#endregion

		#region ZDecimal Fields

		public ZDecimal ImportDutyPaid
		{
			get { return JobComInvoiceLine.JI_ImportDutyPaid; }
		}

		public ZDecimal ImportSch1P2BPaid
		{
			get { return JobComInvoiceLine.JI_ImportSch1P2BPaid; }
		}

		public ZDecimal CustomsValueFromRelatedImportDeclaration
		{
			get { return JobComInvoiceLine.JI_ImportCustomsValue; }
		}

		public ZDecimal VATFromRelatedImportDeclaration
		{
			get { return JobComInvoiceLine.JI_ImportVATPaid; }
		}

		public ZDecimal LineCustomsValue
		{
			get { return JobComInvoiceLine.ApportionedCustomsValue; }
		}

		public ZDecimal ConversionFactor
		{
			get { return InvoiceHeader.JZ_Calc_ConversionFactor; }
		}

		public ZDecimal MarkupPercent => JobComInvoiceLine.JI_ValuationMarkup / 100.0m;

		public ZDecimal CustomsValueInLocalCurrency => JobComInvoiceLine.CustomsValueInLocalCurrency;

		public ZDecimal CustomsDuty => TotalDuty - Sch12BDuty;

		ZBool isEntryIntoWarehouse
		{
			get
			{
				var cusEntryInstruction = JobComInvoiceLine.EntryInstruction;
				return cusEntryInstruction != null && (cusEntryInstruction.CEI_Style == UniversalReferenceConstants.ProcedureCodes._40 || cusEntryInstruction.CEI_Style == UniversalReferenceConstants.ProcedureCodes._42);
			}
		}

		public ZDecimal TotalDuty => isEntryIntoWarehouse ? ((IUltimateDistributee)JobComInvoiceLine).LineDutyTaxEntryFeeItems[CustomsDisbursementChargeCode.TotalDuty].Round(2) : JobComInvoiceLine.JI_Calc_DutyAmount;

		public ZDecimal Sch12BDuty => isEntryIntoWarehouse ? JobComInvoiceLine.GetDutySch1P2BAmountIncludingLCOnly() : JobComInvoiceLine.ApportionedDutySch1P2B;

		public ZDecimal CustomsVAT => isEntryIntoWarehouse ? JobComInvoiceLine.GetGSTVATAmountIncludingLCOnly() : JobComInvoiceLine.JI_Calc_GSTVATAmount;

		#region Discount

		public ZString DiscountInInvoiceCurrency => FormatNumberToMinDecimals(JobComInvoiceLine.DiscountInInvoiceCurrency, 2);

		public ZDecimal DiscountNotIncludedInLinesInForeignCurrency => JobComInvoiceLine.DiscountNotIncludedInLines.Amount;

		public ZString DiscountNotIncludedInLinesCurrencyCode => GetCurrencyCode(JobComInvoiceLine.DiscountNotIncludedInLines.Currency?.Code);

		public ZDecimal DiscountNotIncludedInLinesExRate => GetExchangeRate(JobComInvoiceLine.DiscountNotIncludedInLines.Currency);

		#endregion

		public ZDecimal ValuationMarkup => JobComInvoiceLine.JI_Calc_ValuationMarkup;

		public ZDecimal CustomsValueAdjustment => JobComInvoiceLine.JI_CustomsValue - JobComInvoiceLine.JI_Calc_ActualPrice - ValuationMarkup;

		#region DutiableChargesIncludedInLines

		public ZDecimal DutiableChargesIncludedInLinesInForeignCurrency => JobComInvoiceLine.DutiableChargesIncludedInLines.Amount;

		public ZString DutiableChargesIncludedInLinesCurrencyCode => GetCurrencyCode(JobComInvoiceLine.DutiableChargesIncludedInLines.Currency?.Code);

		public ZDecimal DutiableChargesIncludedInLinesExRate => GetExchangeRate(JobComInvoiceLine.DutiableChargesIncludedInLines.Currency);

		#endregion

		#region DutiableChargesNotIncludedInLines

		public ZDecimal DutiableChargesNotIncludedInLinesInForeignCurrency => JobComInvoiceLine.DutiableChargesNotIncludedInLines.Amount;

		public ZString DutiableChargesNotIncludedInLinesCurrencyCode => GetCurrencyCode(JobComInvoiceLine.DutiableChargesNotIncludedInLines.Currency?.Code);

		public ZDecimal DutiableChargesNotIncludedInLinesExRate => GetExchangeRate(JobComInvoiceLine.DutiableChargesNotIncludedInLines.Currency);

		#endregion

		#region NonDutiableChargesIncludedInLinesExcludingFreightAndInsurance

		public ZDecimal NonDutiableChargesIncludedInLinesExcludingFreightAndInsuranceInForeignCurrency => JobComInvoiceLine.NonDutiableChargesIncludedInLinesExcludingFreightAndInsurance.Amount;

		public ZString NonDutiableChargesIncludedInLinesExcludingFreightAndInsuranceCurrencyCode => GetCurrencyCode(JobComInvoiceLine.NonDutiableChargesIncludedInLinesExcludingFreightAndInsurance.Currency?.Code);

		public ZDecimal NonDutiableChargesIncludedInLinesExcludingFreightAndInsuranceExRate => GetExchangeRate(JobComInvoiceLine.NonDutiableChargesIncludedInLinesExcludingFreightAndInsurance.Currency);

		#endregion

		#region NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsurance

		public ZDecimal NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsuranceInForeignCurrency => JobComInvoiceLine.NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsurance.Amount;

		public ZString NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsuranceCurrencyCode => GetCurrencyCode(JobComInvoiceLine.NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsurance.Currency?.Code);

		public ZDecimal NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsuranceExRate => GetExchangeRate(JobComInvoiceLine.NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsurance.Currency);

		#endregion

		#region OverseasFreightIncludedInLines

		public ZDecimal OverseasFreightIncludedInLinesInForeignCurrency => JobComInvoiceLine.OverseasFreightIncludedInLines.Amount;

		public ZString OverseasFreightIncludedInLinesCurrencyCode => GetCurrencyCode(JobComInvoiceLine.OverseasFreightIncludedInLines.Currency?.Code);

		public ZDecimal OverseasFreightIncludedInLinesExRate => GetExchangeRate(JobComInvoiceLine.OverseasFreightIncludedInLines.Currency);

		#endregion

		#region OverseasFreightNotIncludedInLines

		public ZDecimal OverseasFreightNotIncludedInLinesInForeignCurrency => JobComInvoiceLine.OverseasFreightNotIncludedInLines.Amount;

		public ZString OverseasFreightNotIncludedInLinesCurrencyCode => GetCurrencyCode(JobComInvoiceLine.OverseasFreightNotIncludedInLines.Currency?.Code);

		public ZDecimal OverseasFreightNotIncludedInLinesExRate => GetExchangeRate(JobComInvoiceLine.OverseasFreightNotIncludedInLines.Currency);

		#endregion

		#region OverseasInsuranceIncludedInLines

		public ZDecimal OverseasInsuranceIncludedInLinesInForeignCurrency => JobComInvoiceLine.OverseasInsuranceIncludedInLines.Amount;

		public ZString OverseasInsuranceIncludedInLinesCurrencyCode => GetCurrencyCode(JobComInvoiceLine.OverseasInsuranceIncludedInLines.Currency?.Code);

		public ZDecimal OverseasInsuranceIncludedInLinesExRate => GetExchangeRate(JobComInvoiceLine.OverseasInsuranceIncludedInLines.Currency);

		#endregion

		#region OverseasInsuranceNotIncludedInLines

		public ZDecimal OverseasInsuranceNotIncludedInLinesInForeignCurrency => JobComInvoiceLine.OverseasInsuranceNotIncludedInLines.Amount;

		public ZString OverseasInsuranceNotIncludedInLinesCurrencyCode => GetCurrencyCode(JobComInvoiceLine.OverseasInsuranceNotIncludedInLines.Currency?.Code);

		public ZDecimal OverseasInsuranceNotIncludedInLinesExRate => GetExchangeRate(JobComInvoiceLine.OverseasInsuranceNotIncludedInLines.Currency);

		#endregion

		public ZString TariffFormula => JobComInvoiceLine.DutyFormulaDescription;

		public ZString IncoTerm => JobComInvoiceLine.InvoiceHeader?.JZ_IncoTerm ?? ZString.Empty;

		#endregion

		#region ZString Fields

		public ZString EffectiveCountryOfOrigin
		{
			get { return JobComInvoiceLine.EffectiveCountryOfOrigin; }
		}

		public ZString ImportBOENumber
		{
			get { return ZString.Empty; }
		}

		public ZString TradeAgreement
		{
			get { return JobComInvoiceLine.JI_PrimaryPreference; }
		}

		public ZString TakeUpInTradeStatistics
		{
			get
			{
				ZString result = ZString.Empty;

				if (JobComInvoiceLine.JI_TakeUpInTradeStatistics)
				{
					result = "1";
				}
				else if (!JobComInvoiceLine.JI_TakeUpInTradeStatistics)
				{
					result = "2";
				}

				return result;
			}
		}

		public ZString ValueDeterminationNumber
		{
			get { return (JobComInvoiceLine.CusEntryLine != null) ? JobComInvoiceLine.CusEntryLine.ValueDeterminationNumber : ZString.Empty; }
		}

		public ZString Stat2Unit
		{
			get { return JobComInvoiceLine.JI_CustomsSecondUnitQty; }
		}

		public ZDecimal Stat2Qty
		{
			get { return JobComInvoiceLine.JI_CustomsSecondQuantity; }
		}

		public ZString Stat3Unit
		{
			get { return JobComInvoiceLine.JI_CustomsThirdUnitQty; }
		}

		public ZDecimal Stat3Qty
		{
			get { return JobComInvoiceLine.JI_CustomsThirdQuantity; }
		}

		public override ZString PermitNumber => JobComInvoiceLine.JI_PermitNumber;

		public ZString ROOCert => JobComInvoiceLine.JI_ROOCert;

		public ZString PrevMRN => JobComInvoiceLine.JI_PreviousEntryNumber;

		public ZString CPCAndPPC => JobComInvoiceLine.JI_Procedure;

		public ZString AddtionalTariffs => ZString.Join(",", JobComInvoiceLine.CusLineTariffDetails.Cast<CusLineTariffDetail>().Select(x => new ZString(x.BZ_Type + ":" + x.BZ_Tariff)).ToArray());

		public ZString VIN => JobComInvoiceLine.JI_VIN;

		public ZString EngineNumber => JobComInvoiceLine.JI_EngineNumber;

		public ZString Colour => JobComInvoiceLine.JI_Colour;

		public ZString Make => JobComInvoiceLine.JI_Make;

		public ZString Model => JobComInvoiceLine.JI_Model;

		public ZString VehicleFormat => JobComInvoiceLine.JI_VehicleFormat;

		public ZString VehicleType => JobComInvoiceLine.JI_VehicleType;

		public ZString YoM => JobComInvoiceLine.JI_YearOfManufacture;

		#endregion

		#region ZInt Fields

		public ZString CountableQty => FormatNumber(BondedWarehouseQuantity.Truncate(0));

		public ZInt EngineCC => JobComInvoiceLine.JI_EngineCapacity;

		public ZInt EntryLineNumber
		{
			get { return CusEntryLineInternal.LineNumber; }
		}

		#endregion

		#region ZShort Fields

		public ZShort PrevMRNLine => JobComInvoiceLine.JI_PreviousEntryLineNumber;

		public ZShort InvoiceSequenceNum => JobComInvoiceLine.InvoiceHeader.JZ_InvoiceDisplaySequence;

		#endregion

		#region DocCurrency

		public DocCurrency CIFAndCCurrency
		{
			get
			{
				DocCurrency result = null;

				if (JobComInvoiceLine.LocalCurrency != null)
				{
					result = DocCurrency.New(Factory, JobComInvoiceLine.LocalCurrency);
				}

				return result;
			}
		}

		#endregion

		#region Implementation

		JobComInvoiceLine JobComInvoiceLine
		{
			get { return (JobComInvoiceLine)WrappedObject; }
		}

		JobComInvoiceHeader InvoiceHeader
		{
			get { return JobComInvoiceLine.InvoiceHeader; }
		}

		ZDecimal GetExchangeRate(Integration.ZArchitecture.ICurrency sourceCurrency)
		{
			var converter = (RefCurrencyCurrencyConverter)JobComInvoiceLine.CurrencyConverter;
			var result = converter.GetExchangeRate(sourceCurrency, JobComInvoiceLine.InvoiceHeader?.Invoice_Currency ?? JobComInvoiceLine.LinePriceRefCurrency);
			return result == ZDecimal.Zero ? new ZDecimal(1m) : result;
		}

		ZString GetCurrencyCode(string code)
		{
			return code ?? JobComInvoiceLine.InvoiceHeader?.Invoice_Currency?.Code ?? ZString.Empty;
		}

		#endregion

	}
}
