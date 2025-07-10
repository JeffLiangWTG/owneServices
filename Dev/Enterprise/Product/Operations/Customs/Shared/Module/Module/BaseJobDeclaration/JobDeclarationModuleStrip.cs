using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Module
{
	public class JobDeclarationModuleStrip : WorkflowFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			switch (currentModuleFilter)
			{
				case OrgClientAssignedStaffModuleFilter _:
					var control = new OrgClientAssignedStaffFilterStrip();
					ControlDpiScalingHelper.SetHeight(control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
					PreferredHeight = control.Height;

					return new Control[] { control };

				case ReferenceNumberFilter _:
					return ReferenceNumberFilterGUIProvider.GetReferenceNumberFilterControls(this, FilterControlBindingSource);

				case IJobManagementAmountFilter filter:
					return new Control[] { (Control)filter.GetFilterControl() };

				case EntryStatusFilter entryStatusFilter:
					return EntryStatusFilterGUIProvider.GetEntryStatusFilterControls(this, FilterControlBindingSource, entryStatusFilter.ShowComparisonOperator, entryStatusFilter.ShowFilterType);

				default:
					return base.GetCurrentFilterControls(currentModuleFilter);
			}
		}
	}
}
