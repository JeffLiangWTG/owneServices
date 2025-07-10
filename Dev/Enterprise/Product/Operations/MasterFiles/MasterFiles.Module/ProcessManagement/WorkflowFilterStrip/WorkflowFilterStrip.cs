using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.MasterFiles.Module
{
	[SuppressFormDesignerAnalysis]
	public class WorkflowFilterStrip : ZFilterStrip, IWorkflowFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			Control[] result;
			if (currentModuleFilter is WorkflowModuleFilter)
			{
				WorkflowFilterStripControl strip = new WorkflowFilterStripControl(this);
				FilterControlBindingSource.SetBindingMember(strip, ".");
				result = new Control[] { strip };
			}
			else if (currentModuleFilter is WorkflowModuleTextFilter)
			{
				result = GetWorkflowStringFilterControls();
				PreferredHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(45);
			}
			else
			{
				result = base.GetCurrentFilterControls(currentModuleFilter);
			}

			return result;
		}

		protected Control[] GetWorkflowStringFilterControls()
		{
			Control[] controlArray = GetTextWithListFilterControls(false, true);

			ZFilterStripDropEdit milestoneTypeDropEdit = new ZFilterStripDropEdit();
			milestoneTypeDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			milestoneTypeDropEdit.EnableTimeRecording = false;
			milestoneTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 1);
			milestoneTypeDropEdit.Name = "MilestoneTypeDropEdit";
			milestoneTypeDropEdit.MaxItemsToShowInDropDown = 28;
			milestoneTypeDropEdit.ShowDescriptionBox = false;
			milestoneTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20);
			milestoneTypeDropEdit.Controls["CodeBox"].Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20);
			milestoneTypeDropEdit.TabIndex = 0;
			milestoneTypeDropEdit.BindTo = "MilestoneEvent";
			milestoneTypeDropEdit.DisableInvalidation = true;
			FilterControlBindingSource.SetBindingMember(milestoneTypeDropEdit, milestoneTypeDropEdit.BindTo);
			MissingResourceStringChecker.ExcludeFromTest(milestoneTypeDropEdit);

			var eventReferenceCompareDropEdit = new ZDropEdit
			{
				CaptionResourceString = Res.GetData("6601d2c5-af0b-45bb-a849-47cd0de17cb7", "Trg. Cond. Value", "Trigger Condition Value", "The value used in conjunction with the Trigger Condition to restrict whether the trigger fires when the relevant event is raised or field value changes."),
				CharacterCasing = CharacterCasing.Normal,
				ShowDescriptionBox = false,
				PreBoundMaxLength = FilterComparisonOperatorBoxPreBoundMaxLength,
				Location = ControlDpiScalingHelper.NewScaledPoint(204, 24),
				Name = "eventReferenceCompareDropEdit",
				TabIndex = 2
			};
			eventReferenceCompareDropEdit.CodeBox.TextAlign = HorizontalAlignment.Center;
			eventReferenceCompareDropEdit.CodeBox.Font = new Font("Arial", 8.0f, FontStyle.Italic);
			FilterControlBindingSource.SetBindingMember(eventReferenceCompareDropEdit, "EventReferenceComparisonOption");

			var eventReferenceTextBox = new ZTextBox
			{
				Location = ControlDpiScalingHelper.NewScaledPoint(294, 24),
				Name = "eventReferenceTextBox",
				Size = ControlDpiScalingHelper.NewScaledSize(300, 20),
				TabIndex = 3
			};
			FilterControlBindingSource.SetBindingMember(eventReferenceTextBox, "EventReference");
			MissingResourceStringChecker.ExcludeFromTest(eventReferenceTextBox);

			return new[] { controlArray[0], milestoneTypeDropEdit, eventReferenceCompareDropEdit, eventReferenceTextBox };
		}
	}
}
