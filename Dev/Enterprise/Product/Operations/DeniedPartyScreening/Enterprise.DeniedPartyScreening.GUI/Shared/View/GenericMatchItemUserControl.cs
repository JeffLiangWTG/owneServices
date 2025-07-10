using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public partial class GenericMatchItemUserControl : ZUserControl
	{
		public GenericMatchItemUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource is ScreenedDeniedItemWinModel screenedDeniedItemWinModel)
			{
				ScreenedPartyInnerPanel.Controls.RemoveAndDisposeAll();
				DeniedPartyInnerPanel.Controls.RemoveAndDisposeAll();

				ScreenedPartyInnerPanel.Controls.Add(CreatePanel(screenedDeniedItemWinModel.ScreenedParty));
				DeniedPartyInnerPanel.Controls.Add(CreatePanel(screenedDeniedItemWinModel.DeniedParty));
				ScoreLabel.Text = screenedDeniedItemWinModel.DisplayScore;
				ScoreGradePanel.BackColor = GetScoreGradePanelBackgroundColor(screenedDeniedItemWinModel.ScoreGrade);
				DeniedPartyPanel.BackColor = GetDeniedPartyBackgroundColor(screenedDeniedItemWinModel.ScoreGrade);
				BottomBorderPanel.Visible = screenedDeniedItemWinModel.BottomLineVisibility;
			}
		}

		ZPanel CreatePanel(string text)
		{
			var wholePanel = new ZPanel();
			wholePanel.Name = "WholePanel";
			wholePanel.Dock = DockStyle.Top;
			wholePanel.AutoSize = true;
			wholePanel.Size = ControlDpiScalingHelper.NewScaledSize(718, WinformConstants.MatchItemPanelHeight, true);

			var textParts = text.Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.RemoveEmptyEntries).Reverse();

			foreach (var textPart in textParts)
			{
				var textBlockPanel = new ZPanel();
				textBlockPanel.Name = "TextBlockPanel";
				textBlockPanel.Dock = DockStyle.Top;
				textBlockPanel.Size = ControlDpiScalingHelper.NewScaledSize(718, WinformConstants.MatchItemPanelHeight, true);

				var textLabel = new ZLabel();
				textLabel.Name = "TextLabel";
				textLabel.Dock = DockStyle.Fill;
				textLabel.AutoSize = false;
				textLabel.AutoEllipsis = true;
				textLabel.Text = textPart;
				textLabel.Padding = ControlDpiScalingHelper.NewScaledPadding(6, 0, 0, 0, true);

				textBlockPanel.Controls.Add(textLabel);
				wholePanel.Controls.Add(textBlockPanel);
			}

			return wholePanel;
		}

		Color GetScoreGradePanelBackgroundColor(ScoreGrades scoreGrade)
		{
			switch (scoreGrade)
			{
				case ScoreGrades.High:
					return Color.FromArgb(255, 214, 104, 104);
				case ScoreGrades.Medium:
					return Color.FromArgb(255, 235, 171, 76);
				default:
					return Color.Transparent;
			}
		}

		Color GetDeniedPartyBackgroundColor(ScoreGrades scoreGrade)
		{
			switch (scoreGrade)
			{
				case ScoreGrades.High:
					return Color.FromArgb(255, 252, 244, 244);
				case ScoreGrades.Medium:
					return Color.FromArgb(255, 254, 249, 242);
				default:
					return Color.Transparent;
			}
		}
	}
}
