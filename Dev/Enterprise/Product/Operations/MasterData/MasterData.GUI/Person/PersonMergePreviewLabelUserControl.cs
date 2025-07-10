using System.Diagnostics.CodeAnalysis;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	public partial class PersonMergePreviewLabelUserControl : ZUserControl
	{
		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "ControlDpiScalingHelper is already used")]
		public PersonMergePreviewLabelUserControl(string humanReadableName, string value)
		{
			InitializeComponent();
			ValueLabel.TextChanged += (sender, arg) =>
			{
				ValueLabel.Height = ValueLabel.PreferredHeight;
				Height = ValueLabel.Height;
			};
			LabelCaptionRenderProvider.SetLabelCaptionVisible(HumanReadableNameLabel, false);
			LabelCaptionRenderProvider.SetLabelCaptionVisible(ValueLabel, false);
			HumanReadableNameLabel.IsFontBold = true;
			HumanReadableNameLabel.Text = humanReadableName + " :";
			ValueLabel.Text = value;
		}
	}
}
