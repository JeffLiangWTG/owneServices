using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.RefCurrency)]
	public class RefCurrencyCollection : ActiveBusinessObjectCollection<RefCurrency>, IRefCurrencyCollection
	{
		public RefCurrencyCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RefCurrencyCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public RefCurrencyCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
