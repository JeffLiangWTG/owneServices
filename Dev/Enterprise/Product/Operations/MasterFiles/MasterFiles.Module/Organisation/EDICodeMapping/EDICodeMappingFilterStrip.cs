using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public class EDICodeMappingFilterStrip : ZFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			if (currentModuleFilter is EDICodeMappingRelationshipLocalCodeModuleFilter)
			{
				var ediCodeMappingRelationshipLocalCodeFilterControl = new EDICodeMappingRelationshipLocalCodeFilterControl();
				FilterControlBindingSource.SetBindingMember(ediCodeMappingRelationshipLocalCodeFilterControl, ".");
				var comparisonOperatorDropEdit = CreateComparisonOperatorDropList("ComparisonOperator", "ComparisonOperator_List");
				comparisonOperatorDropEdit.TabIndex = 2;
				ediCodeMappingRelationshipLocalCodeFilterControl.Controls.Add(comparisonOperatorDropEdit);
				PreferredHeight = ediCodeMappingRelationshipLocalCodeFilterControl.Height + ControlDpiScalingHelper.OnePixel;

				return new Control[] { ediCodeMappingRelationshipLocalCodeFilterControl };
			}

			return base.GetCurrentFilterControls(currentModuleFilter);
		}
	}
}
