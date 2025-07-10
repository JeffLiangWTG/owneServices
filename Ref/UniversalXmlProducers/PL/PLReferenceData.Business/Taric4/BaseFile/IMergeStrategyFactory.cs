namespace CargoWise.RefDbRepo.PLReferenceData.Business.Taric4;

internal interface IMergeStrategyFactory
{
	IMergeStrategy CreateMergeStrategy(IsztarHistoryResponseIsztarHistoryItem historyItem);
}
