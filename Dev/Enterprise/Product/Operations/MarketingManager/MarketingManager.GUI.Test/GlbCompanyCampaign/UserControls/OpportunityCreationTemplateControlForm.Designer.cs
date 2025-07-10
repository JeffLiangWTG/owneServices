using CargoWise.Windows.UI;

namespace Enterprise.MarketingManager.GUI.Test.GlbCompanyCampaign.UserControls
{
	internal partial class OpportunityCreationTemplateControlForm
	{
		protected override void InitializeComponent()
		{
			base.InitializeComponent();

			Size = ControlDpiScalingHelper.NewScaledSize(1200, 768);
			Controls.Add(OpportunityCreationTemplateControl);

			BindingSource.SetBindingMember(OpportunityCreationTemplateControl, ".");
			CaptionRenderingEnabled = true;
		}
	}
}
