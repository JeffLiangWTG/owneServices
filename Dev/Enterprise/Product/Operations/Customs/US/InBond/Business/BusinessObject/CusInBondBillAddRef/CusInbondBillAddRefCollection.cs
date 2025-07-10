using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInbondBillAddRefCollection : ActiveBusinessObjectCollection<CusInbondBillAddRef>
	{
		public CusInbondBillAddRefCollection(CusInBondBill master)
			: base(master)
		{
		}
	}
}
