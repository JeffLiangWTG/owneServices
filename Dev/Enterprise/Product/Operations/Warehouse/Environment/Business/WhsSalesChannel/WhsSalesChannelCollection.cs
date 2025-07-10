using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Environment.Business
{
	[ModuleID(ModuleId.WhsSalesChannel)]
	public class WhsSalesChannelCollection : ActiveBusinessObjectCollection<WhsSalesChannel>, IWhsSalesChannelCollection
	{
		public WhsSalesChannelCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
