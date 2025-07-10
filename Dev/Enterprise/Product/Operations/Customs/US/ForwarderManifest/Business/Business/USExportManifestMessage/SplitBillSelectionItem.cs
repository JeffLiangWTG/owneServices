using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class SplitBillSelectionItem : ISelectionItem
	{
		public SplitBillSelectionItem(AsycudaBill bill)
		{
			PK = ZGuid.NewZGuid();
			Bill = Argument.NotNull(bill, "bill");
			DefaultActionType();
			SetMessage();
		}

		public ZGuid PK { get; private set; }
		public AsycudaBill Bill { get; private set; }
		public ZString ActionType { get; set; }
		public ZString Message { get; set; }

		public string SelectionDescription(bool showStatus) => $"{Bill.ABL_BillNumber}".Trim();

		void DefaultActionType()
		{
			if (ActionType.IsEmpty)
			{
				ActionType = USExportBillOfLadingActionCodeType.Codes.A;
			}
		}

		public void SetMessage()
		{
			var sender = new UEMEDIMessageSender((USExportAsycudaBill)Bill, ActionType);
			Message = sender.SerializeToMessageString();
		}
	}
}
