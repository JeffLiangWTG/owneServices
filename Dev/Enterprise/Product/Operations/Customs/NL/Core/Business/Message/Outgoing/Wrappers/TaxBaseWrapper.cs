using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.EU.Business.Declaration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.NL.Business;

public class TaxBaseWrapper : ITaxBase
{
	public TaxBaseWrapper(CusEntryLineFee cusEntryLineFee, int sequenceNumeric)
	{
		this.cusEntryLineFee = cusEntryLineFee;
		SequenceNumeric = sequenceNumeric;
	}
	readonly CusEntryLineFee cusEntryLineFee;

	public int SequenceNumeric { get; }

	public decimal TaxAmount => decimal.Zero;

	public decimal AdValoremTaxBaseAmount => cusEntryLineFee.CF_MethodOfCalculation == Mathematics.Percentage ? cusEntryLineFee.CF_BaseValue : 0;

	public decimal SpecificTaxBaseQuantity => cusEntryLineFee.CF_MethodOfCalculation != Mathematics.Percentage ? cusEntryLineFee.CF_BaseValue : 0;

	public string UnitCode => cusEntryLineFee.CF_MethodOfCalculation != Mathematics.Percentage ? cusEntryLineFee.CF_MethodOfCalculation.ToString() : string.Empty;

	public int TaxRateNumeric => cusEntryLineFee.CF_Rate.ToZInt();
}
