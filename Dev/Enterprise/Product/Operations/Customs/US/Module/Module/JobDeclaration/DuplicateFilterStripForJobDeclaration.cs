using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;

namespace Enterprise.Customs.US.Module
{
	sealed class DuplicateFilterStripForJobDeclaration
	{
		public DuplicateFilterStripForJobDeclaration(FilterStripCollection collection)
		{
			var strips = new FilterStrip[EntryTypeFilterBusinessList.Count];

			for (var index = 0; index < EntryTypeFilterBusinessList.Count; index++)
			{
				var filter = EntryTypeFilterBusinessList[index];
				{
					var strip = collection.AddNew();
					collection.Remove(strip);
					SetStripValueFromFilter(strip, filter);
					strip.OrCategory = FilterOrCategory.Red;
					strips[index] = strip;
				}
			}

			for (var index = strips.Length - 1; index >= 0; index--)
			{
				((IList)collection).Insert(0, strips[index]);
			}
		}

		List<FilterBusinessObjectDefault> EntryTypeFilterBusinessList
		{
			get
			{
				if (filterBusinessList == null)
				{
					filterBusinessList = new List<FilterBusinessObjectDefault>
					{
						new FilterBusinessObjectDefault(DeclarationFilterConstants.EntryType, "Property", (ZString)EntryTypeList.Codes.ConsumptionFreeDutiable, true),
						new FilterBusinessObjectDefault(DeclarationFilterConstants.EntryType, "Property", (ZString)EntryTypeList.Codes.InformalFreeDutiable, true)
					};
				}
				return filterBusinessList;
			}
		}
		List<FilterBusinessObjectDefault> filterBusinessList;

		void SetStripValueFromFilter(FilterStrip strip, FilterBusinessObjectDefault filter)
		{
			strip.FilterDescription = filter.FilterName;

			if (strip.CurrentModuleFilter != null)
			{
				strip.CurrentModuleFilter[filter.PropertyName] = filter.Value;
				strip.CurrentModuleFilter.Visibility = FilterVisibility.AlwaysVisible;
			}
		}
	}
}
