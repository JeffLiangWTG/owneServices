using System.Windows.Forms;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.TransportCommon.Module
{
	public class DtbTransportWorkflowFilterStrip : WorkflowFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			Control[] result;

			var amountFilter = currentModuleFilter as IJobManagementAmountFilter;

			if (amountFilter != null)
			{
				result = new Control[] { (Control)amountFilter.GetFilterControl() };
			}
			else if (currentModuleFilter is ReferenceNumberFilter)
			{
				result = ReferenceNumberFilterGUIProvider.GetReferenceNumberFilterControls(this, FilterControlBindingSource);
			}
			else
			{
				result = base.GetCurrentFilterControls(currentModuleFilter);
			}

			return result;
		}
	}
}
