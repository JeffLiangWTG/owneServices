namespace CargoWise.RefDbRepo.PLReferenceData.Business.Taric4;

class MergeStrategyFactory : IMergeStrategyFactory
{
	public IMergeStrategy CreateMergeStrategy(IsztarHistoryResponseIsztarHistoryItem historyItem) => historyItem.Item switch
	{
		findMeasureByDatesResponseHistory =>
			new MergeStrategy<findMeasureByDatesResponseHistory, measure>(originType.N),
		findPublicationSigleByDatesResponseHistory =>
			new MergeStrategy<findPublicationSigleByDatesResponseHistory, publicationSigle>(originType.N),
		findBaseRegulationByDatesResponseHistory =>
			new MergeStrategy<findBaseRegulationByDatesResponseHistory, baseRegulation>(originType.N),
		findModificationRegulationByDatesResponseHistory =>
			new MergeStrategy<findModificationRegulationByDatesResponseHistory, modificationRegulation>(originType.N),
		findProrogationRegulationByDatesResponseHistory =>
			new MergeStrategy<findProrogationRegulationByDatesResponseHistory, prorogationRegulation>(originType.N),
		findMeasureConditionCodeByDatesResponse =>
			new MergeStrategy<findMeasureConditionCodeByDatesResponse, measureConditionCode>(originType.N),
		findFullTemporaryStopRegulationByDatesResponseHistory =>
			new MergeStrategy<findFullTemporaryStopRegulationByDatesResponseHistory, fullTemporaryStopRegulation>(originType.N),
		findExplicitAbrogationRegulationByDatesResponseHistory =>
			new MergeStrategy<findExplicitAbrogationRegulationByDatesResponseHistory, explicitAbrogationRegulation>(originType.N),
		findCompleteAbrogationRegulationByDatesResponseHistory =>
			new MergeStrategy<findCompleteAbrogationRegulationByDatesResponseHistory, completeAbrogationRegulation>(originType.N),
		findQuotaDefinitionByDatesResponseHistory =>
			new MergeStrategy<findQuotaDefinitionByDatesResponseHistory, quotaDefinition>(originType.T),
		_ => new NullMergeStrategy()
	};
}
