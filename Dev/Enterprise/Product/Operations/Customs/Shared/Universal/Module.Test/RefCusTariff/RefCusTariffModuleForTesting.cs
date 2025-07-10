using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Universal.Module.Testing
{
	class RefCusTariffModuleForTesting : RefCusTariffModule
	{
		public FilterModuleMenuItemDescriptorCollection ExportMenuItems_Exposed => base.ExportMenuItems;
	}
}
