using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDynamicWorkOrderValidationCache
	{
		public WhsDynamicWorkOrderValidationCache(WhsPickableDocketLineCollection workOrderLines)
		{
			Argument.NotNull(workOrderLines, nameof(workOrderLines));

			MainProcessedItemCount = new Lazy<int>(() =>
			{
				return
					workOrderLines
					.Cast<WhsDynamicWorkOrderLine>()
					.Count(a => a.IsMainInwardProcessedItem);
			});

			ComponentRunningTotalCache = new Lazy<Dictionary<ZGuid, ZDecimal>>(() =>
			{
				var parentLineAttributes =
					workOrderLines
					.Where(l => !l.WE_WE_ParentDocketLine.IsValid)
					.Select(l => l.CustomsData);

				var processedItemsWithMain = new Dictionary<ZGuid, bool>();
				foreach (var item in parentLineAttributes)
				{
					processedItemsWithMain[item.WB_ParentID] = item.WB_IsMainInwardsProcessedItem;
				}

				var componentRunningTotalCache = new Dictionary<ZGuid, ZDecimal>();
				foreach (var componentLine in workOrderLines.Where(l => l.WE_WE_ParentDocketLine.IsValid).ToList())
				{
					if (componentRunningTotalCache.TryGetValue(componentLine.WE_OP, out var total))
					{
						componentRunningTotalCache[componentLine.WE_OP] =
							total + CalculateLineValue(componentLine);
					}
					else
					{
						componentRunningTotalCache[componentLine.WE_OP] = CalculateLineValue(componentLine);
					}
				}

				return componentRunningTotalCache;

				decimal CalculateLineValue(WhsDocketLine componentLine)
					=> componentLine.WE_TransactionQuantity
						* (processedItemsWithMain[componentLine.WE_WE_ParentDocketLine] ? 1m : -1m);
			});
		}

		public bool DoesDynamicWorkOrderHaveNoMainProcessedItems()
			=> MainProcessedItemCount.Value == 0;

		public bool DoesDynamicWorkOrderHaveMoreThanOneMainProcessedItem()
			=> MainProcessedItemCount.Value > 1;

		public bool IsSecondaryComponentSumMoreThanMainComponent(ZGuid componentPK)
			=> componentPK.IsValid
			&& ComponentRunningTotalCache.Value.TryGetValue(componentPK, out var runningTotal)
			&& runningTotal < 0m;

		Lazy<int> MainProcessedItemCount { get; }
		Lazy<Dictionary<ZGuid, ZDecimal>> ComponentRunningTotalCache { get; }
	}
}
