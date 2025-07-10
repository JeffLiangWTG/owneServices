using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Environment.Business
{
	[ModuleID(ModuleId.WhsConfigProductStyle)]
	public class WhsProductStyleCollection : ActiveBusinessObjectCollection<WhsProductStyle>
	{
		public WhsProductStyleCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
