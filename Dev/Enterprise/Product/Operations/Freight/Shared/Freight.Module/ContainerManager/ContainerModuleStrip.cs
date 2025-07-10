using System.Windows.Forms;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Module
{
	public class ContainerModuleStrip : WorkflowFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			Control[] result;

			if (currentModuleFilter is ReferenceNumberFilter)
			{
				result = ReferenceNumberFilterGUIProvider.GetReferenceNumberFilterControls(this, FilterControlBindingSource);
			}
			else if (currentModuleFilter is ServiceTypeDateFilter)
			{
				var control = ServiceTypeDateFilter.GetServiceTypeDateFilterControl(this, FilterControlBindingSource);
				PreferredHeight = control.Height;
				result = new[] { control };
			}
			else
			{
				result = base.GetCurrentFilterControls(currentModuleFilter);
			}

			return result;
		}
	}
}
