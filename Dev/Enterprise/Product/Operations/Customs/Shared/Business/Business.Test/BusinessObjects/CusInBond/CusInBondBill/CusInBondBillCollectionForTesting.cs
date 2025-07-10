using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	class CusInBondBillCollectionForTesting : CusInBondBillCollection<CusInBondBillForTesting>
	{
		public CusInBondBillCollectionForTesting(CusInBondHeader master, ZQuery filter)
			: base(master, filter)
		{
		}
		public CusInBondBillCollectionForTesting(CusInBondHeader master)
			: base(master)
		{
		}
	}
}
