using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.Warehouse.Environment.Business
{
	public interface IAffectLocationView
	{
		SchemaColumn[] GetColumnsThatAffectLocationView();

		void ReloadLocationsFromDB();

		ZGuid ParentThatMayReloadMyLocations { get; }

		ZGuid PK { get; }
	}
}

