using System.Linq;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Taric4;

internal class TaricBaseMerger(IMergeStrategyFactory mergeStrategyFactory)
{
	public void MergeUpdateToBase(IsztarHistoryResponse baseXml, IsztarHistoryResponse updateXml)
	{
		foreach (var baseHistoryItem in baseXml.IsztarHistoryItem)
		{
			var dataGroupType = baseHistoryItem.Item.GetType();
			var updateHistoryItem = updateXml.IsztarHistoryItem.FirstOrDefault(historyItem => historyItem.Item.GetType() == dataGroupType);
			if (updateHistoryItem == null)
			{
				continue;
			}

			IMergeStrategy mergeStrategy = mergeStrategyFactory.CreateMergeStrategy(baseHistoryItem);
			baseHistoryItem.Item = mergeStrategy.Merge(baseHistoryItem.Item, updateHistoryItem.Item);
		}

		RemoveEmptyElements(baseXml);
	}

	static void RemoveEmptyElements(IsztarHistoryResponse baseXml)
	{
		var emptyItems = baseXml.IsztarHistoryItem.Where(historyItem => historyItem.Item is null);
		baseXml.IsztarHistoryItem = baseXml.IsztarHistoryItem.Except(emptyItems).ToArray();
	}
}
