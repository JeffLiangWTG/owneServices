using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	public static class FilterBusinessObjectTestHelper
	{
		static public void SetFilterProperty(FilterStripBusinessObject filterObject, ZString description, params IZType[] values)
		{
			ModuleFilter filter = filterObject[description];

			if (values.Length == 0)
			{
				if (filterObject.ActiveModuleFilters.Contains(filter))
				{
					filter.IsActive = false;
				}
			}
			else
			{
				if (!filterObject.ActiveModuleFilters.Contains(filter))
				{
					filter.IsActive = true;
				}

				ModuleTextBaseFilter textFilter = filter as ModuleTextBaseFilter;
				if (textFilter != null)
				{
					textFilter.Property = (ZString)values[0];
					if (!(textFilter is ModuleNkFilter))
					{
						textFilter.ComparisonOperator = values.Length > 1 ? (ZString)values[1] : (ZString)"starts with";
					}
				}
				else if (filter is ModuleGuidFilter)
				{
					((ModuleGuidFilter)filter).Property = (ZGuid)values[0];
				}
				else if (filter is ModuleDateFilter)
				{
					ModuleDateFilter datefilter = (ModuleDateFilter)filter;
					datefilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
					datefilter.Property1 = (ZDateTime)values[0];
					datefilter.Property2 = (ZDateTime)values[values.Length > 1 ? 1 : 0];
				}
				else if (filter is ModuleFlagsFilter)
				{
					((ModuleFlagsFilter)filter).Property0 = (ZBool)values[0];
				}
				else if (filter is ModuleNumberRangeFilter)
				{
					((ModuleNumberRangeFilter)filter).Property1 = ZDecimal.ParseSafe(values[0].ToString(), 0m); // for some reason ZShort can't be converted into ZDecimal any other way.
					((ModuleNumberRangeFilter)filter).Property2 = ZDecimal.ParseSafe(values[values.Length > 1 ? 1 : 0].ToString(), 0m);
				}
				else
				{
					throw new NotSupportedException(string.Format("Unknown filter type - {0}", filter.GetType()));
				}
			}
		}
	}
}
