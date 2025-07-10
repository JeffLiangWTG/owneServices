using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class SplitBillSelectionItem : ISelectionItem
	{
		public SplitBillSelectionItem(AsycudaBill bill, AsycudaArrivalHeader arrival)
		{
			PK = ZGuid.NewZGuid();
			Bill = Argument.NotNull(bill, "bill");
			Arrival = arrival;
		}

		public ZGuid PK { get; private set; }
		public AsycudaBill Bill { get; private set; }
		public AsycudaArrivalHeader Arrival { get; private set; }

		public string SelectionDescription(bool showStatus) => $"{Bill.ABL_BillNumber} {Arrival?.ATH_Reference ?? ZString.Empty}".Trim();
	}
}
