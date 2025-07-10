namespace Enterprise.Customs.TW.Business
{
	public class CusInBondBillCollection : Customs.Business.CusInBondBillCollection<CusInBondBill>
	{
		public CusInBondBillCollection(CusInBondHeader master)
			: base(master)
		{
		}
	}
}
