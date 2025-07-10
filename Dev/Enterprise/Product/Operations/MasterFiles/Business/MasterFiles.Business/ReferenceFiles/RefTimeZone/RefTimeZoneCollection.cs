using CargoWise.EntityFramework;
using CargoWise.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefTimeZoneCollection : ActiveBusinessObjectCollection<RefTimeZone>
	{
		public RefTimeZoneCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public RefTimeZoneCollection(RefTimeZoneSet parent, SchemaGuidColumn pivotTableFKToMaster)
			: base(parent, typeof(RefTimeZoneSet), new ZQuery(), pivotTableFKToMaster, null)
		{
		}
	}
}
