using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using static Enterprise.Customs.NO.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.NO.Business;

public class CusEntryLine : Customs.Business.CusEntryLine
{
	public CusEntryLine(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new class Schema : Customs.Business.CusEntryLine.Schema
	{
		public const string CtryOfOrigin = nameof(CtryOfOrigin);
		public const string ExciseDutyAmount = nameof(ExciseDutyAmount);
		public const string Procedure = nameof(Procedure);
		public const string Preference = nameof(Preference);
		public const string ReducedCustomsFlag = nameof(ReducedCustomsFlag);
		public const string TotalAmount = nameof(TotalAmount);
		public const string ValuationCode = nameof(ValuationCode);
		public const string VatAmount = nameof(VatAmount);
		public const string VatCode = nameof(VatCode);
	}

	public new CusEntryLine Clone() => (CusEntryLine)base.Clone();

	public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	public new CusEntryLineFeeCollection Fees => (CusEntryLineFeeCollection)base.Fees;

	public new CusEntryHeader Header => (CusEntryHeader)base.Header;

	public new JobComInvoiceLine RandomLine => base.RandomLine as JobComInvoiceLine;

	public new CusEntryLineLookups Lookups => (CusEntryLineLookups)base.Lookups;

	public new CusEntryLineValidation Validation => (CusEntryLineValidation)base.Validation;

	protected override System.Type GetCusEntryHeaderType() => typeof(CusEntryHeader);

	protected override ICusEntryLineFeeCollection<Customs.Business.CusEntryLineFee, Customs.Business.CusEntryLine> GetCusEntryLineFeeCollection() => new CusEntryLineFeeCollection(this, Factory);

	protected override Customs.Business.CusEntryLineLookups GetNewLookups() => new CusEntryLineLookups(this);

	protected override Customs.Business.CusEntryLineValidation GetNewValidation() => new CusEntryLineValidation(this);

	[DecimalPlaces(0)]
	[DecimalPrecision(16)]
	[ReadOnly(true)]
	public override ZDecimal DutyAmount => base.DutyAmount;

	protected override ZDecimal GetDutyAmountCore() => CustomsDutyAmount + AgriculturalDutyAmount;

	public ZString CtryOfOrigin => RandomLine.JI_CountryOfOrigin;

	[DecimalPlaces(0)]
	[DecimalPrecision(16)]
	[ReadOnly(true)]
	public ZDecimal CustomsDutyAmount => Fees.GetAmount(fee => fee.IsCustomsDuty);

	public ZDecimal AgriculturalDutyAmount => Fees.GetAmount(fee => fee.IsAgriculturalDuty);

	[DecimalPlaces(0)]
	[DecimalPrecision(16)]
	[ReadOnly(true)]
	public ZDecimal ExciseDutyAmount => Fees.GetAmount(fee => fee.IsExciseDuty);

	public ZString Procedure => RandomLine.JI_Procedure;

	public ZString Preference => RandomLine.JI_PrimaryPreference;

	public ZString ReducedCustomsFlag => RandomLine.JI_ReducedCustomsFlag;

	[DecimalPlaces(0)]
	[DecimalPrecision(16)]
	[ReadOnly(true)]
	public ZDecimal TotalAmount => RandomLine.JI_Calc_TotalAmount;

	public ZString ValuationCode
	{
		get
		{
			var valuationCode = RandomLine.JI_ValuationCode;

			if (!valuationCode.IsEmpty)
			{
				return valuationCode;
			}
			return RandomLine.InvoiceHeader?.JZ_ValuationMethod ?? ZString.Empty;
		}
	}

	[DecimalPlaces(0)]
	[DecimalPrecision(16)]
	[ReadOnly(true)]
	public ZDecimal VatAmount => Fees.GetAmount(fee => fee.IsVAT);

	public ZString VatCode => RandomLine.JI_ZZF_NKTaxType.ToUpper().ToString() switch
	{
		RefCusTaxOrFee.MVK => NOCustomDutyCodeList.Codes.MV1,
		var vatCode => vatCode,
	};

	public bool IsArt => RandomLine.JI_ZZF_NKTaxType.ToUpper() == RefCusTaxOrFee.MVK;

	public override ZString CL_CustomsPostedStatus
	{
		get
		{
			return base.CL_CustomsPostedStatus;
		}
		set
		{
			var oldValue = base.CL_CustomsPostedStatus;
			base.CL_CustomsPostedStatus = value;
			if (oldValue != value)
			{
				Header.MarkAsNeedingValidation();
			}
		}
	}

	protected override ZDecimal GetInvoiceLineCustomsValueToAggregate(BaseJobComInvoiceLine invoiceLine)
	{
		return invoiceLine.JI_CustomsValue.Round(0);
	}

	public ZInt AdjustmentsRounded => (ZInt)decimal.Round(CL_CustomsValue - CL_InvoiceAmount, 0);

	public IFeeRounder FeeRounder => feeRounder ??= new IntegerFeeRounder();
	IFeeRounder feeRounder;

	internal IEnumerable<JobComInvoiceLine> AllInvoiceLines => InvoiceLines.Cast<JobComInvoiceLine>();

	internal IEnumerable<CusEntryLineFee> AllFees => Fees.Cast<CusEntryLineFee>();
}
