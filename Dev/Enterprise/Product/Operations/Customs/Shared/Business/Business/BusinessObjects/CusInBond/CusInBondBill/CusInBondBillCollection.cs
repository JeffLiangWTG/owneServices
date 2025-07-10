using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface ICusInBondBillCollection : IActiveBusinessObjectCollection
	{
		CusInBondBill AddNew(ZString issuerCode, ZString billNumber);
	}

	public abstract class CusInBondBillCollection<T> : ActiveBusinessObjectCollection<T>, ICusInBondBillCollection
		where T : CusInBondBill
	{
		protected CusInBondBillCollection(CusInBondHeader master, ZQuery filter)
			: base(master, filter)
		{
		}

		protected CusInBondBillCollection(CusInBondHeader master)
			: base(master)
		{
		}

		public T AddNew(ZString issuerCode, ZString billNumber)
		{
			var result = AddNew();
			result.B0_IssuerCode = issuerCode;
			result.B0_MasterBillNumber = billNumber;
			return result;
		}

		protected override void SetDefaultsForNewElementCore(T newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			if (Count == 0)
			{
				DefaultFromHeader(newElement);
			}
			else if (Count > 0)
			{
				DefaultFromPreviousBill(newElement, GetPreviousBill());
			}
		}

		protected virtual void DefaultFromHeader(T newBill)
		{
		}

		protected virtual void DefaultFromPreviousBill(T newBill, T previousBill)
		{
		}

		protected virtual T GetPreviousBill()
		{
			return this[Count - 1];
		}

		CusInBondBill ICusInBondBillCollection.AddNew(ZString issuerCode, ZString billNumber)
		{
			return this.AddNew(issuerCode, billNumber);
		}
	}
}
