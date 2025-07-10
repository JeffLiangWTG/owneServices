namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IPartiallyReplenishedPickSplitter
	{
		WhsPick SplitOrdersFromPartiallyReplenishedPick(WhsPick pick);
	}
}
