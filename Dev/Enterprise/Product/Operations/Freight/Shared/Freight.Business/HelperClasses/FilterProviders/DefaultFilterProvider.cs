using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business
{
	public abstract class DefaultFilterProvider
	{
		#region SetDefaultFilters

		public void SetDefaultFilters(IFilterBusinessObjectDefaultsProvider collection)
		{
			SetDefaultFiltersCore(collection);
		}

		#endregion

		#region AddFilterDefault

		protected static void AddFilterDefaults(IFilterBusinessObjectDefaultsProvider collection, string filterName, ZString value)
		{
			if (!value.IsEmpty)
			{
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(filterName, "Property", value));
			}
		}

		protected static void AddFilterDefaults(IFilterBusinessObjectDefaultsProvider collection, string filterName, ZGuid value)
		{
			if (!value.IsEmpty)
			{
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(filterName, "Property", value));
			}
		}

		protected static void AddFilterDefaults(IFilterBusinessObjectDefaultsProvider collection, string filterName, ZString value1, ZString value2)
		{
			if (!value1.IsEmpty || !value2.IsEmpty)
			{
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(filterName, "Property1", value1));
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(filterName, "Property2", value2));
			}
		}

		protected static void AddTextAndNkFilterDefaults(IFilterBusinessObjectDefaultsProvider collection, string filterName, ZString value1, ZString value2)
		{
			if (!value1.IsEmpty || !value2.IsEmpty)
			{
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(filterName, "Property", value1));
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(filterName, "NkProperty", value2));
			}
		}

		protected static void AddFilterDefaults(IFilterBusinessObjectDefaultsProvider collection, string filterName, ZGuid value1, ZGuid value2)
		{
			if (!value1.IsEmpty || !value2.IsEmpty)
			{
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(filterName, "Property1", value1));
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(filterName, "Property2", value2));
			}
		}

		protected static void AddFilterDefaults(IFilterBusinessObjectDefaultsProvider collection, string filterName, ZDateTime fromDate, ZDateTime toDate)
		{
			if (!fromDate.IsEmpty || !toDate.IsEmpty)
			{
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(filterName, "PropertySearch", (ZString)(NoResString)"Date range"));
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(filterName, "Property1", fromDate.Date.ToZDateTime()));
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(filterName, "Property2", toDate.Date.ToZDateTime()));
			}
		}

		protected static void AddCustomFilterDefaults(IFilterBusinessObjectDefaultsProvider collection, string filterName, string propertyName, IZType value)
		{
			if (!value.IsEmpty)
			{
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(filterName, propertyName, value));
			}
		}

		#endregion

		protected abstract void SetDefaultFiltersCore(IFilterBusinessObjectDefaultsProvider collection);
	}
}
