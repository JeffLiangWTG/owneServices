using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Business
{
	[ModuleID(ModuleId.AgencyContainerDetention)]
	public class ContainerDetentionCollection : ActiveBusinessObjectCollection<ContainerDetention>
	{
		public ContainerDetentionCollection(BusinessObjectFactory factory)
			: base(factory) { }
	}
}
