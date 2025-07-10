using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Module
{
	public class RelatedContainersOfConsolFilter : ModuleGuidForeignCollectionFilter
	{
		protected RelatedContainersOfConsolFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public RelatedContainersOfConsolFilter(ZString description, BusinessObjectFactory factory)
			: base(description, ModuleIDs.Containers, JobConsolSchema.PK, JobContainerSchema.JC_JK, new ContainerNonDependentCollection(factory), typeof(ForwardingConsol))
		{
		}
	}
}
