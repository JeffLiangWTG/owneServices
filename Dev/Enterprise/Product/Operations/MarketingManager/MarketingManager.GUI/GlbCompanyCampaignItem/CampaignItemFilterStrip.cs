using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	[SuppressFormDesignerAnalysis]
	public partial class CampaignItemFilterStrip : ZFilterStrip
	{
		public CampaignItemFilterStrip()
		{
			InitializeComponent();
		}

		protected override Control[] GetCurrentFilterControls(ZArchitecture.Business.ModuleFilter currentModuleFilter)
		{
			Control[] result;

			if (currentModuleFilter is LinkActivityModuleFilter)
			{
				LinkActivityFilterControl control = new LinkActivityFilterControl(this);
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				FilterControlBindingSource.SetBindingMember(control, ".");
				PreferredHeight = control.Height;
				this.SetHeight(PreferredHeight);
				if (currentModuleFilter is ContextLinkActivityModuleFilter)
				{
					control.ContextURLDropEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("LinkActivityFilterControl|98385252-1e0b-4591-996b-fe4c4216e895", "Context");
				}
				else if (currentModuleFilter is DestinationURLLinkActivityModuleFilter)
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
			else if (currentModuleFilter is TouchReceivedModuleFilter)
			{
				var control = new TouchReceivedFilterControl();
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				FilterControlBindingSource.SetBindingMember(control, ".");
				PreferredHeight = control.Height;
				this.SetHeight(PreferredHeight);
				result = new Control[] { control };
			}
			else
			{
				result = base.GetCurrentFilterControls(currentModuleFilter);
			}

			return result;
		}
	}
}
