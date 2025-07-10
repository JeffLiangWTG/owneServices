namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IAllocatePackageLabelsStrategy
	{
		bool RunChecksPriorToCartonisingOrPickingByLabel(WhsPick pick, bool saveFactory = true);
		CartonisationResult CartoniseSplitCases(WhsPick pick, bool saveFactory = true);
		bool PickCasesByLabel(WhsPick pick);
		bool PickPalletsByLabel(WhsPick pick);
	}

	public enum CartonisationResult
	{
		Cartonised,
		NothingToCartonise,
		Error
	}
}
