using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public interface ICusInBondFeeCollection<out T> : IActiveBusinessObjectCollection<T>
		where T : CusInBondFee
	{
		new T this[int index] { get; }
	}

	public class CusInBondFeeCollection<T> : ActiveBusinessObjectCollection<T>, ICusInBondFeeCollection<T>
		where T : CusInBondFee
	{
		public CusInBondFeeCollection(CusInBondCargoDesc cusInBondCargoDesc)
			: base(cusInBondCargoDesc)
		{
			this.cusInBondCargoDesc = cusInBondCargoDesc;
		}
		readonly CusInBondCargoDesc cusInBondCargoDesc;

		protected override void SetDefaultsForNewElementCore(T newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.BFE_BY = cusInBondCargoDesc.PK;
		}
	}
}
