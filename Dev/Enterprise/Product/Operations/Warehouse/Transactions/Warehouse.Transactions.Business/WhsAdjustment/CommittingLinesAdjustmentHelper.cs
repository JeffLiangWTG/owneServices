
namespace Enterprise.Warehouse.Transactions.Business
{
	public class CommittingLinesAdjustmentHelper : CommittingLinesParentHelper<WhsAdjustmentLine>
	{
		public CommittingLinesAdjustmentHelper(WhsAdjustment parent)
			: base(parent)
		{
		}

		protected override void UncommitOverPickedOrNotMatchingInventory(WhsAdjustmentLine line)
		{
			line.UncommitOverPickedOrNotMatchingInventory();
		}

		protected override void CommitInventory(WhsAdjustmentLine line)
		{
			line.CommitInventory();
		}
	}
}
