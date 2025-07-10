using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class CusInBondMoveLineItemCollection : ActiveBusinessObjectCollection<CusInBondMoveLineItem>
	{
		public CusInBondMoveLineItemCollection(CusInBondMoveDetail master)
			: base(master)
		{
		}
	}
}
