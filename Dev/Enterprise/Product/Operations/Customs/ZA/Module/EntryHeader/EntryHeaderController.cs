using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ZA.Module
{
	class EntryHeaderController : Customs.Module.EntryHeaderController
	{
		public override ControllerID ID => ZAControllerIDs.EntryHeader;
	}
}
