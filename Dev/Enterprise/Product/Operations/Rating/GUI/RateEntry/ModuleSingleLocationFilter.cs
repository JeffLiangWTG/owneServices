using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.GUI
{
	public class ModuleSingleLocationFilter : ModuleNkFilter
	{
		public ModuleSingleLocationFilter(ZString filterCode, MultilingualString description, SchemaStringColumn locationColumn, IBusinessObjectCollection locationList)
			: this(filterCode, description, locationColumn, ModuleIDs.Location, locationList)
		{
		}

		// Using for tests
		public ModuleSingleLocationFilter(ZString filterCode, MultilingualString description, SchemaStringColumn locationColumn, ModuleIdentifier moduleId, IBusinessObjectCollection locationList)
			: base(filterCode, locationColumn, moduleId, locationList)
		{
			Initialize(description);
		}

		public ModuleSingleLocationFilter(ZString filterCode, MultilingualString description, GetNkQuery getLocationQuery, IBusinessObjectCollection locationList)
			: base(filterCode, getLocationQuery, ModuleIDs.Location, locationList)
		{
			Initialize(description);
		}

		void Initialize(MultilingualString description)
		{
			MultilingualDescription = description;
			SupportsFiltersMatchComparisonOperator = false; // Can't determine which module to show since it can be RefUNLOCO, RefCountry, or RefZoneHeader.
		}

		protected override FilterCategory DefaultCategory =>
			FilterCategories.Locations;

		protected override ZQuery GetQueryUsingFilterColumns() =>
			ModuleLocationFilter.GetLocationFilter(
				Property,
				FilterColumn,
				isEmptyComparisonOperation: false,
				allowInternationalZones: true);
	}
}
