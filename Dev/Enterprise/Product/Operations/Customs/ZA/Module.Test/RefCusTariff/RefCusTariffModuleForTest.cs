using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ZA.Module.Testing
{
	class RefCusTariffModuleForTest : RefCusTariffModule
	{
		public IModuleDecisionProvider GetModuleDecisionProviderForFindBox_Exposed(IFindBox findBox) => GetModuleDecisionProviderForFindBox(findBox);
		public IModuleDecisionProvider GetModuleDecisionProviderForFindBoxPopup_Exposed(IFindBox findBox) => GetModuleDecisionProviderForFindBoxPopup(findBox);
	}
}
