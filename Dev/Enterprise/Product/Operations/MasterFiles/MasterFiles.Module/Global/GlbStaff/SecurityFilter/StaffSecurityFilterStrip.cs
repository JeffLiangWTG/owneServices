using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	[SuppressFormDesignerAnalysis]
	public class StaffSecurityFilterStrip : ZFilterStrip
	{
		#region Filter Controls

		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			Control[] result;
			if (currentModuleFilter is StaffSecurityModuleFilter)
			{
				StaffSecurityFilterControl securityControl = new StaffSecurityFilterControl();
				PreferredHeight = securityControl.Height + ControlDpiScalingHelper.OnePixel;

				result = new Control[] { securityControl };
			}
			else if (currentModuleFilter is LeaveDateRangeWithTypeFilter)
			{
				var leaveDateRangeControl = new LeaveDateRangeWithTypeFilterControl(this);
				FilterControlBindingSource.SetBindingMember(leaveDateRangeControl, ".");
				return new Control[] { leaveDateRangeControl };
			}
			else if (currentModuleFilter is StaffReportingManagerRoleModuleFilter)
			{
				var reportingManagerFilterControl = new StaffReportingManagerRoleFilterControl();
				FilterControlBindingSource.SetBindingMember(reportingManagerFilterControl, ".");
				var comparisonOperatorDropEdit = CreateComparisonOperatorDropList("ComparisonOperator", "ComparisonOperator_List");
				comparisonOperatorDropEdit.TabIndex = 2;
				reportingManagerFilterControl.Controls.Add(comparisonOperatorDropEdit);
				PreferredHeight = reportingManagerFilterControl.Height + ControlDpiScalingHelper.OnePixel;

				return new Control[] { reportingManagerFilterControl };
			}
			else
			{
				result = base.GetCurrentFilterControls(currentModuleFilter);
			}
			return result;
		}

		internal Control[] TestGetCurrentFilterControlsInternal(ModuleFilter currentModuleFilter)
		{
			return GetCurrentFilterControls(currentModuleFilter);
		}

		#endregion
	}
}
