using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID("InternationalZone")]
	public class RefZoneHeaderCollection : ActiveBusinessObjectCollection<RefZoneHeader>
	{
		public RefZoneHeaderCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RefZoneHeaderCollection(BusinessObjectFactory factory, ZQuery additionalQuery)
			: base(factory, additionalQuery)
		{
		}

		public RefZoneHeaderCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
