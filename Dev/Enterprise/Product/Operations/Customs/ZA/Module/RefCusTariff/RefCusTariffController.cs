using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ZA.Module
{
	public class RefCusTariffController : Universal.Module.RefCusTariffController
	{
		public override ControllerID ID => ZAControllerIDs.RefCusTariff;
	}
}
