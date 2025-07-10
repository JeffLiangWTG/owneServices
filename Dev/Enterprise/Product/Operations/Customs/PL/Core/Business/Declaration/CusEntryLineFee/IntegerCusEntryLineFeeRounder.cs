using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

internal class IntegerCusEntryLineFeeRounder : IFeeRounder
{
	public ZDecimal Round(ZDecimal chargeAmount) => chargeAmount.Round(0);
}
