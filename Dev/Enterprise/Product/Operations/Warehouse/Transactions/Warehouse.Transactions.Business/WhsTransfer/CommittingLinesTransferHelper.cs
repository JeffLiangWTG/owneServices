
namespace Enterprise.Warehouse.Transactions.Business
{
	class CommittingLinesTransferHelper : CommittingLinesParentHelper<WhsTransferLine>
	{
		public CommittingLinesTransferHelper(WhsTransfer transfer)
			: base(transfer)
		{
		}

		protected override void UncommitOverPickedOrNotMatchingInventory(WhsTransferLine line)
		{
			line.UncommitOverPickedOrNotMatchingInventory();
		}

		protected override void CommitInventory(WhsTransferLine line)
		{
			line.CommitInventory();
		}
	}
}
