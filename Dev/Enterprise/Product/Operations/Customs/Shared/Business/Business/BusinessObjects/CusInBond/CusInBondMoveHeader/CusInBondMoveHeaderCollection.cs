using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public abstract class CusInBondMoveHeaderCollection : ActiveBusinessObjectCollection<CusInBondMoveHeader>
	{
		protected CusInBondMoveHeaderCollection(CusInBondHeader master)
			: base(master)
		{
		}

		protected CusInBondMoveHeaderCollection(CusInBondHeader master, ZQuery filter)
			: base(master, filter)
		{
		}
	}
}
