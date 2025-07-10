using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business
{
	public static class TrackingInventorySummaryExtensions
	{
		public static void LoadInventories(this IEnumerable<TrackingInventorySummary> summaries)
		{
			var summariesWithoutInventory = summaries.Where(s => !s.Inventories.IsLoaded).ToArray();
			LoadInventoryPKs(summariesWithoutInventory);

			foreach (var summary in summariesWithoutInventory)
			{
				summary.LoadInventories();
			}
			// For better fetch hints performance: sort all inventories after loading all inventories
			foreach (var summary in summariesWithoutInventory)
			{
				summary.SortInventories();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		static void LoadInventoryPKs(IEnumerable<TrackingInventorySummary> summaries)
		{
			if (summaries.Any())
			{
				var factory = new BusinessObjectFactory();
				var summariesFilter = new ZQuery();
				var references = new Dictionary<(ZGuid, ZGuid, ZGuid, ZString, ZString), TrackingInventorySummary>();

				foreach (var summary in summaries)
				{
					var summaryFilter = new ZQuery();
					summaryFilter.AddToFilter(WhsTrackingInventorySummaryItemViewSchema.WI_OH_Client, summary.WI_OH_Client);
					summaryFilter.AddToFilter(WhsTrackingInventorySummaryItemViewSchema.WI_WW_Whs, summary.WI_WW_Whs);
					summaryFilter.AddToFilter(WhsTrackingInventorySummaryItemViewSchema.WI_OP, summary.WI_OP);
					summaryFilter.AddToFilter(WhsTrackingInventorySummaryItemViewSchema.WI_UnitsUQ, summary.WI_UnitsUQ);
					summaryFilter.AddToFilter(WhsTrackingInventorySummaryItemViewSchema.WI_ClientUQ, summary.WI_ClientUQ);
					summariesFilter.AddToFilter(summaryFilter, JoinCondition.Or);

					references.Add((summary.WI_OH_Client, summary.WI_WW_Whs, summary.WI_OP, summary.WI_UnitsUQ, summary.WI_ClientUQ), summary);
				}

				var summaryItemsFilter = new ZQuery();
				summaryItemsFilter.AddToFilter(summariesFilter);
				summaryItemsFilter.AddToFilter(summaries.First().AdditionalFilter);
				var summaryItems = new WhsTrackingInventorySummaryItemViewCollection(factory);
				summaryItems.Load(summaryItemsFilter);

				foreach (var summaryItem in summaryItems.OfType<WhsTrackingInventorySummaryItemView>())
				{
					var key = (summaryItem.WI_OH_Client, summaryItem.WI_WW_Whs, summaryItem.WI_OP, summaryItem.WI_UnitsUQ, summaryItem.WI_ClientUQ);
					references[key].AddInventoryPK(summaryItem.PK);
				}
			}
		}
	}
}
