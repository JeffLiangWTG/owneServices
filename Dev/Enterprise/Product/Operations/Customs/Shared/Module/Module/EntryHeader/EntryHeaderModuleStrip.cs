using System.Windows.Forms;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Module
{
	public class EntryHeaderModuleStrip : WorkflowFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			switch (currentModuleFilter)
			{
				case ReferenceNumberFilter _:
					return ReferenceNumberFilterGUIProvider.GetReferenceNumberFilterControls(this, FilterControlBindingSource);
				default:
					return base.GetCurrentFilterControls(currentModuleFilter);
			}
		}
	}
}
