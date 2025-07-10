using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public abstract class CusInBondPayInfoCollection<TPayInfo> : ActiveBusinessObjectCollection<TPayInfo> where TPayInfo : CusInBondPayInfo
	{
		protected CusInBondPayInfoCollection(BaseCusInBondMoveHeader master) : base(master)
		{
		}
	}
}
