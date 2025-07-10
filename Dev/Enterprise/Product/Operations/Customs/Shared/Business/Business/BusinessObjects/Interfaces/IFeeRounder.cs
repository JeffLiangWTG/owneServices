using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface IFeeRounder
	{
		ZDecimal Round(ZDecimal chargeAmount);
	}
}
