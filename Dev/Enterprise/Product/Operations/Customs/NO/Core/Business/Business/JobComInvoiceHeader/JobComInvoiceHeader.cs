using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using CustomsChargeTypeList = Enterprise.Customs.Common.CustomsChargeTypeList;

namespace Enterprise.Customs.NO.Business;

public class JobComInvoiceHeader(BusinessObjectFactory factory, DataRow row) : AutoNOJobComInvoiceHeader(factory, row), IChargeApportionee
{
	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		JZ_ValuationMethod = ValuationMethodList.Codes.ValueOfImportedGoods;
		JZ_ValuationCode = UniversalReferenceConstants.NatureOfTransactions.Code01;
	}

	public new JobComInvoiceHeader Clone() => (JobComInvoiceHeader)base.Clone();

	public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

	public new JobComInvoiceHeaderLookups Lookups => (JobComInvoiceHeaderLookups)base.Lookups;

	public new JobComInvoiceHeaderValidation Validation => (JobComInvoiceHeaderValidation)base.Validation;

	public new JobComInvoiceLineViewCollection JobComInvoiceLines => (JobComInvoiceLineViewCollection)base.JobComInvoiceLines;

	[ChildEditable(true)]
	public new JobComInvChargeCollection<InvoiceCharge> Charges => (JobComInvChargeCollection<InvoiceCharge>)base.Charges;

	[ChildEditable(true)]
	public new JobComInvApportionedChargeCollection<InvoiceApportionCharge> GroupCharges => (JobComInvApportionedChargeCollection<InvoiceApportionCharge>)base.GroupCharges;

	public new JobComInvoiceGroupHeader Master => (JobComInvoiceGroupHeader)base.Master;

	public new JobComInvoiceGroupHeader GroupHeader => (JobComInvoiceGroupHeader)base.GroupHeader;

	public new Bill Bill => (Bill)base.Bill;

	protected override Customs.Business.JobComInvoiceHeaderLookups GetNewLookups() => new JobComInvoiceHeaderLookups(this);

	protected override Customs.Business.JobComInvoiceHeaderValidation GetNewValidation() => new JobComInvoiceHeaderValidation(this);

	protected override BaseJobComInvoiceLineViewCollection CreateNewJobComInvoiceLineCollection()
	{
		if (JobDeclaration != null)
		{
			return new JobComInvoiceLineViewCollection(this, JobDeclaration.InvoiceLines);
		}
		return null;
	}

	protected override IJobComInvChargeCollection<BaseInvoiceCharge> CreateNewJobComInvHeaderCharges() => new JobComInvChargeCollection<InvoiceCharge>(this);

	protected override IJobComInvApportionedChargeCollection<BaseApportionedCharge> CreateNewApportionedChargeCollection() => new JobComInvApportionedChargeCollection<InvoiceApportionCharge>(this);

	protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new JobComInvoiceHeaderFetchStrategy(this);

	protected override ZString LocalCurrencyCodeCore => Core.Constants.CurrencyCodes.Norway;

	public ZDecimal JZ_Calc_ChargesAmount => Factory.GetValue(ref jZ_Calc_ChargesAmountCached, () => JZ_Calc_CIFAmount_InLocalCurrency - JZ_InvoiceAmountInLocalCurrency);
	CachedProperty<ZDecimal> jZ_Calc_ChargesAmountCached;

	public ZGuid JZ_Calc_ChargesCurrency => LocalCurrency?.PK ?? ZGuid.Empty;

	[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.ValuationCodeList))]
	[ResourceStringData("AE7D5CBF-6EB0-4C0E-8FB0-0C7A9872344E", Caption = "[24] Tran. Nature", FullDescription = "The nature of the transaction.")]
	public override ZString JZ_ValuationCode
	{
		get => base.JZ_ValuationCode;
		set => base.JZ_ValuationCode = value;
	}

	public override ZDateTime JZ_ValuationDateOverride
	{
		get => base.JZ_ValuationDateOverride;
		set
		{
			var oldValue = JZ_ValuationDateOverride;
			base.JZ_ValuationDateOverride = value;
			if (!IsCopying && oldValue != JZ_ValuationDateOverride)
			{
				InvoiceLines?.MarkAsNeedingValidation();
			}
		}
	}

	[List(nameof(Lookups) + "." + nameof(JobComInvoiceHeaderLookups.ValuationMethodList))]
	[ResourceStringData("26A5AC38-43CD-4815-A0A4-45D1E09A7793", Caption = "Valuation Method")]
	public override ZString JZ_ValuationMethod
	{
		get => base.JZ_ValuationMethod;
		set => base.JZ_ValuationMethod = value;
	}

	protected override CurrencyConverter GetNewCurrencyConverter()
	{
		return IsJZ_InvoiceCurrExRateUserEnterable ? new CurrencyConverterWithFixedExchangeRatesDataProvider(Factory, this) : base.GetNewCurrencyConverter();
	}

	protected override ZDateTime EffectiveValuationDateCore
	{
		get
		{
			var originalEntryInstruction = InvoiceLines
				.Select(ji => ji.EntryInstruction)
				.WhereNotNull()
				.Distinct()
				.Where(cei => cei.CEI_DateForDuty.IsValid)
				.OrderBy(cei => cei.CEI_DateForDuty)
				.FirstOrDefault();
			return originalEntryInstruction?.CEI_DateForDuty ?? base.EffectiveValuationDateCore;
		}
	}

	#region IChargeApportionee members

	void IChargeApportionee.CalculateAmountBasedOnPercentage(JobComInvCharge charge)
	{
		if (LinePriceRefCurrency != null)
		{
			ZDecimal amountCalculated = 0m;
			if (charge.J7_ChargeType == CustomsChargeTypeList.Codes.OverseasInsurance ||
				charge.J7_ChargeType == CustomsChargeTypeList.Codes.OtherCharges ||
				charge.J7_ChargeType == CustomsChargeTypeList.Codes.DeductionCharge)
			{
				amountCalculated = charge.J7_Percentage > 0m ? JZ_InvoiceAmountInLocalCurrency * charge.J7_Percentage / 100m : 0m;
			}

			charge.J7_Amount = amountCalculated.Round(2);

			if (amountCalculated > 0m)
			{
				charge.J7_RX_NKCurrency = LinePriceRefCurrency.RX_Code;
			}
		}
	}

	public RefCurrency LinePriceRefCurrency => RefCurrency.LoadFromCurrencyCode(base.Factory, JZ_RX_NKInvoice_Currency);
	#endregion

	internal IEnumerable<JobComInvoiceLine> AllLines => JobComInvoiceLines.Cast<JobComInvoiceLine>();
}
