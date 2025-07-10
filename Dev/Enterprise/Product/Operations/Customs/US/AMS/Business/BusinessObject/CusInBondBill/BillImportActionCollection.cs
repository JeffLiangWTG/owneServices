using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class BillImportActionCollection : SailingBillImportActionCollection<BillImportAction>
	{
		public BillImportActionCollection(CusInBondBillCollection billCollection)
			: base(billCollection.Factory)
		{
			foreach (var bill in billCollection)
			{
				Add(new BillImportAction(bill));
			}
		}
	}
}
