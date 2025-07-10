using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class CusInBondMoveDetailCollection : CusInBondMoveDetailCollection<CusInBondMoveDetail>
	{
		public CusInBondMoveDetailCollection(CusInBondMoveHeader master)
			: base(master)
		{
		}
	}
}
