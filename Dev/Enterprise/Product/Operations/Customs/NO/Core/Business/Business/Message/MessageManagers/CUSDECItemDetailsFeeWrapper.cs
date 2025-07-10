using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.NO.Business;

sealed class CUSDECItemDetailsFeeWrapper(CusEntryLineFee entryLineFee) : ICUSDECMessageDataProvider.IItemDetailsFee
{
	CusEntryLineFee EntryLineFee { get; } = Argument.NotNull(entryLineFee, nameof(entryLineFee));

	public ZString FeeAmount => EntryLineFee.CF_ChargeAmount.ToNorwegianAmountString();

	public ZString FeeType => EntryLineFee.CF_ChargeType.Left(2);

	public ZString FeeTypeSequence => EntryLineFee.CF_ChargeType.SubstringSafe(2);

	public ZString FeeBaseValueForCalculation => EntryLineFee.CF_BaseValue.ToNorwegianAmountString();

	public ZString FeeRate => GetFeeRate();

	ZString GetFeeRate()
	{
		var invoiceLine = (JobComInvoiceLine)EntryLineFee.EntryLine.RandomLine;
		var rate = EntryLineFee.CF_Rate;

		if (invoiceLine.IsRateGivenInFractionsOfKroner(EntryLineFee.CF_ChargeType))
		{
			rate *= 100;
		}
		return rate.IsEmpty ? ZString.Empty : rate.ToNorwegianAmountString();
	}
}
