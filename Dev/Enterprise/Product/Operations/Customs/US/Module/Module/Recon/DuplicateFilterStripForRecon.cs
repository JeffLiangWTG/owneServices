using System.Collections;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;

namespace Enterprise.Customs.US.Module
{
	sealed class DuplicateFilterStripForRecon
	{
		public DuplicateFilterStripForRecon(FilterStripCollection collection)
		{
			var strips = new FilterStrip[DuplicateFilterBusinessList.Count];

			for (var index = 0; index < DuplicateFilterBusinessList.Count; index++)
			{
				var filter = DuplicateFilterBusinessList[index];
				var strip = collection.AddNew();
				collection.Remove(strip);
				SetStripValueFromFilter(strip, filter);
				strips[index] = strip;
			}

			for (var index = strips.Length - 1; index >= 0; index--)
			{
				((IList)collection).Insert(0, strips[index]);
			}
		}

		List<FilterBusinessObjectDefaultWithOperator> DuplicateFilterBusinessList
		{
			get
			{
				if (filterBusinessList == null)
				{
					filterBusinessList = new List<FilterBusinessObjectDefaultWithOperator>
					{
						new FilterBusinessObjectDefaultWithOperator("ENS (Entry Summary) Status", "Property", (ZString)ImportMessageStatusList.Codes.ClearEntrySummaryDelete, ModuleTextFilter.ComparisonConstants.NotEqual),
						new FilterBusinessObjectDefaultWithOperator("ENS (Entry Summary) Status", "Property", (ZString)ImportMessageStatusList.Codes.EntrySummaryCanceled, ModuleTextFilter.ComparisonConstants.NotEqual)
					};
				}

				return filterBusinessList;
			}
		}
		List<FilterBusinessObjectDefaultWithOperator> filterBusinessList;

		void SetStripValueFromFilter(FilterStrip strip, FilterBusinessObjectDefaultWithOperator filter)
		{
			strip.FilterDescription = filter.FilterName;

			if (strip.CurrentModuleFilter != null)
			{
				strip.CurrentModuleFilter[filter.PropertyName] = filter.Value;
				strip.CurrentModuleFilter.Visibility = FilterVisibility.AlwaysVisible;
			}

			var textFilter = strip.CurrentModuleFilter as ModuleTextFilter;
			if (textFilter != null)
			{
				textFilter.ComparisonOperator = filter.ComparisonOperator;
			}
		}
	}
}
