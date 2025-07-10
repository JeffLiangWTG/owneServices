using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class FeeNoRounder : IFeeRounder
	{
		public ZDecimal Round(ZDecimal chargeAmount) => chargeAmount;
	}
}
