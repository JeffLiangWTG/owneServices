using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Business
{
	[ModuleID(ModuleId.AgencyVoyageAccounting)]
	public class VoyageAccountCollection : ActiveBusinessObjectCollection<VoyageAccount>
	{
		public VoyageAccountCollection(BusinessObjectFactory factory)
			: base(factory) { }
	}
}
