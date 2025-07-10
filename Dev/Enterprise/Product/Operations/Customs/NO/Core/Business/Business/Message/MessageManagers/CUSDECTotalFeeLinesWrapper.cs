using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.NO.Business;

sealed class CUSDECTotalFeeLinesWrapper(CusEntryHeaderFee fee) : ICUSDECMessageDataProvider.ITotalFeeLines
{
	CusEntryHeaderFee Fee { get; } = Argument.NotNull(fee, nameof(fee));

	public ZDecimal Amount => Fee.Amount;

	public ZString AmountAsString => Fee.Amount.ToString();

	public ZString FeeCode => Fee.DutyCode;
}
