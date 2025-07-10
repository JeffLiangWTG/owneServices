using System.Windows.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	class ServiceLevelFilterStrip : ZFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			if (currentModuleFilter is TransitTimeModuleFilter)
			{
				return new Control[] { new TransitTimeModuleFilterControl() };
			}

			return base.GetCurrentFilterControls(currentModuleFilter);
		}
	}
}
