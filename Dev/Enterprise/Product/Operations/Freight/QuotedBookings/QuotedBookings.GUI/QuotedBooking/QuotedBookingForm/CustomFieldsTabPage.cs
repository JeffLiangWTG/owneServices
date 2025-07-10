using System;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	public class CustomFieldsTabPage : ZTabPage, ITabVisibilityOverride
	{
		public CustomFieldsTabPage()
		{
			processTemplateCustomFieldsControl = new ProcessTemplateCustomFieldsControl();

			processTemplateCustomFieldsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			processTemplateCustomFieldsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
			processTemplateCustomFieldsControl.Name = "processTemplateCustomFieldsControl";
			processTemplateCustomFieldsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 500);
			processTemplateCustomFieldsControl.TabIndex = 0;
			processTemplateCustomFieldsControl.NothingSetupMessageLabelText = Res.GetString("879a440a-a6d6-46d4-a1c4-8857267056d8", "To make use of this tab, please setup quoted booking custom fields in Workflow Manager.");

			Controls.Add(processTemplateCustomFieldsControl);
		}

		readonly ProcessTemplateCustomFieldsControl processTemplateCustomFieldsControl;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006: Do not nest generic types in member signatures")]
		public void SetVisibilityRule(Func<bool?> rule)
		{
			tabVisiblilityRule = rule;
		}

		bool? ITabVisibilityOverride.IsTabVisible
		{
			get
			{
				return tabVisiblilityRule != null
					? tabVisiblilityRule()
					: null;
			}
		}

		Func<bool?> tabVisiblilityRule;
	}
}
