using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public interface ICusInBondMoveDetailCollection : Customs.Business.ICusInBondMoveDetailCollection, CargoWise.EntityFramework.IActiveBusinessObjectCollection
	{
		void SetAllCustomsStatus(ZString customsStatus);
	}

	public abstract class CusInBondMoveDetailCollection<T> : Customs.Business.CusInBondMoveDetailCollection<T>, ICusInBondMoveDetailCollection
		where T : CusInBondMoveDetail
	{
		protected CusInBondMoveDetailCollection(CusInBondMoveHeader master)
			: base(master)
		{
		}

		protected CusInBondMoveDetailCollection(Customs.Business.CusInBondBill master)
			: base(master)
		{
		}

		public void SetAllCustomsStatus(ZString customsStatus)
		{
			foreach (T moveDetail in this)
			{
				moveDetail.B9_CustomsStatus = customsStatus;
			}
		}
	}
}
