using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.RefCountry)]
	public class RefCountryCollection : ActiveBusinessObjectCollection<RefCountry>, IRefCountryCollection
	{
		public RefCountryCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RefCountryCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public RefCountryCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
