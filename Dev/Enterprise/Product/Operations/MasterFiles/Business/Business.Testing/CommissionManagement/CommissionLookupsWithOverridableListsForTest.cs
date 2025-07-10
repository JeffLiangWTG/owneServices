using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CommissionLookupsWithOverridableListsForTest : CommissionLookups
	{
		public CommissionLookupsWithOverridableListsForTest(BusinessObjectFactory factory, ReadOnlyCodeDescriptionPairList products, ReadOnlyCodeDescriptionPairList services, ReadOnlyCodeDescriptionPairList subModules)
			: base(factory)
		{
			this.products = products;
			this.services = services;
			this.subModules = subModules;
		}

		public override ReadOnlyCodeDescriptionPairList GetProducts()
		{
			return products;
		}
		readonly ReadOnlyCodeDescriptionPairList products;

		public override ReadOnlyCodeDescriptionPairList GetServices(ZString product)
		{
			return services;
		}
		readonly ReadOnlyCodeDescriptionPairList services;

		public override ReadOnlyCodeDescriptionPairList GetSubModules(ZString product, ZString service)
		{
			return subModules;
		}
		readonly ReadOnlyCodeDescriptionPairList subModules;
	}
}
