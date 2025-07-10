using CargoWise.Types;

namespace Enterprise.Customs.US.DIS.Business
{
	class DISPackingListDataWrapper : IDISPackingList
	{
		public DISPackingListDataWrapper(DISPackingListData data)
		{
			this.data = data;
		}

		readonly DISPackingListData data;

		ZString IDISPackingList.PackingListNumber
		{
			get { return data.PackingListNumber; }
		}

		ZString IDISPackingList.InvoiceNumber
		{
			get { return data.InvoiceNumber; }
		}

		ZString IDISPackingList.PurchaseOrderNumber
		{
			get { return data.PurchaseOrderNumber; }
		}
	}
}
