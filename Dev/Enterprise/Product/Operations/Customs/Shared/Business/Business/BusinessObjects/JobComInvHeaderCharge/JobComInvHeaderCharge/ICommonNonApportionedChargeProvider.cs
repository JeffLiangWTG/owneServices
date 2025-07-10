using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface ICommonNonApportionedChargeProvider<T> where T : CommonNonApportionedCharge
	{
		T CreateNew();
		T GetChargeWithZeroAmount(ZString chargeCode);
		BusinessObjectFactory Factory { get; }
	}
}
