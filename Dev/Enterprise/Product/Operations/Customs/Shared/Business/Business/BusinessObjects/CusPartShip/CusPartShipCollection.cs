using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusPartShipCollection<TCusPartShip> : DependentBusinessObjectCollection<TCusPartShip, CusHAWB>
		where TCusPartShip : CusPartShip
	{
		public CusPartShipCollection(CusHAWB cusHAWBParent, BusinessObjectFactory factory)
			: base(cusHAWBParent, factory)
		{
			this.CusHAWBParent = cusHAWBParent;
		}

		protected readonly CusHAWB CusHAWBParent;
	}
}
