using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.ExternalRequestTypes)]
	public class ExternalRequestTypeCollection : ActiveBusinessObjectCollection<ExternalRequestType>
	{
		public ExternalRequestTypeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ExternalRequestTypeCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
