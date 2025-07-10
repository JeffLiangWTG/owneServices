using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Universal;

[ModuleID(ModuleId.RefHarbourRate)]
public class RefHarbourRateCollection : BusinessObjectCollection<RefHarbourRate>
{
	public RefHarbourRateCollection(BusinessObjectFactory factory)
		: base(factory)
	{
	}
}
