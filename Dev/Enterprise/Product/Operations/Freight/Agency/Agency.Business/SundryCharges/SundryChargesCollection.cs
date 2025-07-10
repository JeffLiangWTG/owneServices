using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Business
{
	[ModuleID(ModuleId.AgencySundryCharges)]
	public class SundryChargesCollection : ActiveBusinessObjectCollection<SundryCharges>
	{
		public SundryChargesCollection(BusinessObjectFactory factory)
			: base(factory) { }
	}
}
