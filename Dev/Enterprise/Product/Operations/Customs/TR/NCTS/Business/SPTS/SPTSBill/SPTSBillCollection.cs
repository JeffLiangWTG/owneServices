using Enterprise.Customs.Business;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class SPTSBillCollection : CusInBondBillCollection<SPTSBill>
	{
		public SPTSBillCollection(SPTSHeader master)
			: base(master)
		{
		}
	}
}
