using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class CampaignFilterStrip : ZFilterStrip
	{
		public CampaignFilterStrip()
		{
		}

		protected override Control[] GetCurrentFilterControls(ZArchitecture.Business.ModuleFilter currentModuleFilter)
		{
			Control[] result;

			if (currentModuleFilter is StaffAssignmentPersonAndRoleModuleFilter)
			{
				StaffAssignmentPersonAndRoleFilterControl control = new StaffAssignmentPersonAndRoleFilterControl();
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				ZDropEdit comparisonOperatorDropEdit = CreateComparisonOperatorDropList("ComparisonOperator", "ComparisonOperator_List");
				comparisonOperatorDropEdit.TabIndex = 2;
				control.Controls.Add(comparisonOperatorDropEdit);
				PreferredHeight = control.Height;
				result = new Control[] { control };
			}
			else if (currentModuleFilter is CampaignContactLinkActivityModuleFilter)
			{
				LinkActivityFilterControl control = new LinkActivityFilterControl(this);
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				FilterControlBindingSource.SetBindingMember(control, ".");
				PreferredHeight = control.Height;
				this.SetHeight(PreferredHeight);
				if (currentModuleFilter is CampaignContactContextLinkActivityModuleFilter)
				{
					control.ContextURLDropEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("LinkActivityFilterControl|98385252-1e0b-4591-996b-fe4c4216e895", "Context");
				}
				else if (currentModuleFilter is CampaignContactDestinationURLLinkActivityModuleFilter)
				{
					control.ContextURLDropEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("LinkActivityFilterControl|c9c34610-f12f-4ef4-a99a-fd40d5f2f5a9", "URL");
				}
				result = new Control[] { control };
			}
			else if (currentModuleFilter is CampaignContactNumberFilter || currentModuleFilter is UniqueDaysActivityCountFilter)
			{
				CampaignContactNumberFilterControl control = new CampaignContactNumberFilterControl();
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				ZDropEdit comparisonOperatorDropEdit = CreateComparisonOperatorDropList("ComparisonOperator", "ComparisonOperator_List");
				comparisonOperatorDropEdit.TabIndex = 2;
				control.Controls.Add(comparisonOperatorDropEdit);
				PreferredHeight = control.Height;
				result = new Control[] { control };
			}
			else if (currentModuleFilter is OrganisationHasSalesRelationFilter)
			{
				result = GetOrganisationHasSalesRelationFilterControls();
			}
			else if (currentModuleFilter is UtcOffsetFilter)
			{
				var filterControl = new UtcOffsetFilterControl();
				PreferredHeight = filterControl.Height;
				result = new Control[] { filterControl };
			}
			else if (currentModuleFilter is OrgSalesMainCompetitorModuleFilter)
			{
				var control = new OrgSalesMainCompetitorFilterControl();
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				PreferredHeight = control.Height;
				result = new Control[] { control };
			}
			else if (currentModuleFilter is OrgHasMainCompetitorModuleFilter)
			{
				var control = new OrgHasMainCompetitorFilterControl();
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				PreferredHeight = control.Height;
				result = new Control[] { control };
			}
			else
			{
				result = base.GetCurrentFilterControls(currentModuleFilter);
			}

			return result;
		}

		Control[] GetOrganisationHasSalesRelationFilterControls()
		{
			var checkBox = new ZCheckBox();
			checkBox.CaptionResourceString = Res.GetData("530EEB70-AC8A-4FB3-A9AC-501B41FA7C2E", "Yes");
			checkBox.AutoSize = true;
			ControlDpiScalingHelper.SetTop(ref checkBox, LabelTop, false);
			ControlDpiScalingHelper.SetLeft(ref checkBox, FilterControlsStart, true);
			checkBox.BindTo = "BoolProperty";
			checkBox.TabIndex = 1;
			FilterControlBindingSource.SetBindingMember(checkBox, checkBox.BindTo);

			var dropEdit = new ZDropEdit();
			dropEdit.CharacterCasing = CharacterCasing.Normal;
			ControlDpiScalingHelper.SetTop(ref dropEdit, FilterControlTop, false);
			ControlDpiScalingHelper.SetLeft(ref dropEdit, FilterControlsBox1Start, true);
			ControlDpiScalingHelper.SetWidth(ref dropEdit, FilterControlBoxWidth, true);
			ControlDpiScalingHelper.SetWidth(dropEdit.CodeBox, ZFilterStrip.DropListCodeBoxWidth, true);
			dropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
			dropEdit.ShowDescriptionBox = true;
			dropEdit.TabIndex = 2;
			dropEdit.BindTo = "TypeProperty";
			FilterControlBindingSource.SetBindingMember(dropEdit, dropEdit.BindTo);

			return new Control[] { dropEdit, checkBox };
		}
	}
}
