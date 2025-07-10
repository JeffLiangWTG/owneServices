using CargoWise.Types;

namespace Enterprise.Customs.Business;

public class IntegerFeeRounder : IFeeRounder
{
	public ZDecimal Round(ZDecimal chargeAmount)
	{
		return chargeAmount.Round(0).Normalize();
	}
}
