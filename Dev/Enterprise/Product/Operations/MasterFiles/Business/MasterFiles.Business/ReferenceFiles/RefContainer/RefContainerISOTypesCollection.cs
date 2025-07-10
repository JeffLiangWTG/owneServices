using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.RefContainerISOTypes)]
	public class RefContainerISOTypesCollection : NonPersistentBusinessObjectCollection<ContainerISOType>
	{
		public RefContainerISOTypesCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ContainerISOType();
		}
	}
}
