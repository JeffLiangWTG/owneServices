using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class OrgRateCommodityDefaultingRuleHelper
	{
		public OrgRateCommodityDefaultingRuleHelper() { }

		public ZString GetDefaultRateCommodity<T>(IReadOnlyCollection<T> rules,
			Func<T, string> originSelector, string origin,
			Func<T, string> destinationSelector, string destination,
			Func<T, string> transportModeSelector, string transportMode,
			Func<T, string> containerModeSelector, string containerMode,
			Func<T, string> serviceLevelSelector, string serviceLevel,
			Func<T, string> directionSelector, string direction,
			Func<T, string> commodityCodeSelector)
		{
			// Apply filters one by one based on hierarchy
			var filteredRules = FilterCommodityDefaultingRules(rules, originSelector, origin, true);
			if (filteredRules.Count == 1)
			{
				return commodityCodeSelector(filteredRules.First());
			}

			filteredRules = FilterCommodityDefaultingRules(filteredRules, destinationSelector, destination, true);
			if (filteredRules.Count == 1)
			{
				return commodityCodeSelector(filteredRules.First());
			}

			filteredRules = FilterCommodityDefaultingRules(filteredRules, transportModeSelector, transportMode);
			if (filteredRules.Count == 1)
			{
				return commodityCodeSelector(filteredRules.First());
			}

			filteredRules = FilterCommodityDefaultingRules(filteredRules, containerModeSelector, containerMode);
			if (filteredRules.Count == 1)
			{
				return commodityCodeSelector(filteredRules.First());
			}

			filteredRules = FilterCommodityDefaultingRules(filteredRules, serviceLevelSelector, serviceLevel);
			if (filteredRules.Count == 1)
			{
				return commodityCodeSelector(filteredRules.First());
			}

			filteredRules = FilterCommodityDefaultingRules(filteredRules, directionSelector, direction);
			if (filteredRules.Count == 1)
			{
				return commodityCodeSelector(filteredRules.First());
			}

			// Prioritize by the hierarchy level of matching rules
			var orderedFilteredRules = filteredRules.OrderByDescending(r => !string.IsNullOrEmpty(originSelector(r)))
													.ThenByDescending(r => !string.IsNullOrEmpty(destinationSelector(r)))
													.ThenByDescending(r => !string.IsNullOrEmpty(transportModeSelector(r)))
													.ThenByDescending(r => !string.IsNullOrEmpty(containerModeSelector(r)))
													.ThenByDescending(r => !string.IsNullOrEmpty(serviceLevelSelector(r)))
													.ThenByDescending(r => !string.IsNullOrEmpty(directionSelector(r)))
													.ToList();

			// Return the first rule if multiple or none found
			return orderedFilteredRules.IsNullOrEmpty() ? ZString.Empty : commodityCodeSelector(orderedFilteredRules.FirstOrDefault());
		}

		static List<T> FilterCommodityDefaultingRules<T>(IReadOnlyCollection<T> rules, Func<T, string> selector, string searchValue, bool matchPartial = false)
		{
			return rules.Where(r =>
			{
				var value = selector(r);
				if (string.IsNullOrEmpty(value))
				{
					return true;
				}

				if (matchPartial && searchValue != null && value.Length == 2)
				{
					return value.StartsWith(searchValue) || value.StartsWith(searchValue.Substring(0, 2));
				}

				return value == searchValue;
			}).ToList();
		}
	}
}
