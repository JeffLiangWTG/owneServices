using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public partial class PotentialMatchUserControl : ZUserControl
	{
		public PotentialMatchUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource is PotentialMatchWinModel potentialMatchModel)
			{
				base.SetDataBinding(dataSource, dataMember);

				// Prevent flicker
				foreach (Control control in PotentialMatchInfoPanel.Controls)
				{
					control.Visible = false;
				}

				PotentialMatchInfoPanel.Controls.RemoveAndDisposeAll();

				var controls = new List<Control>();
				var scoreGradeBackColor = ColorConverter.MapScoreGrade(potentialMatchModel.ScoreGrade);

				PotentialMatchHeaderPanel.Visible = potentialMatchModel.ScoreGrade != ScoreGrades.Low;

				PotentialMatchHeaderPanel.BackColor = scoreGradeBackColor;
				ScoreGradeReviewLabel.BackColor = scoreGradeBackColor;
				MiddleScoreGradeLabel.ForeColor = scoreGradeBackColor;

				HeaderScoreGradePictureBox.Image = potentialMatchModel.RequireReviewIcon.ToBitmap();
				ProfilePictureBox.Image = potentialMatchModel.EntityTypeIcon.ToBitmap();
				MiddleScoreGradePictureBox.Image = potentialMatchModel.WarningIcon.ToBitmap();

				ScoreGradeReviewLabel.Text = potentialMatchModel.ScoreGradeReviewText;
				ProfileNameLabel.Text = potentialMatchModel.ProfileName;
				MiddleScoreGradeLabel.Text = potentialMatchModel.ScoreGradeText;

				if (potentialMatchModel.NameMatchWinModel.MatchViewVisibility)
				{
					var nameMatchUserControl = new GenericMatchUserControl();
					nameMatchUserControl.SetDataBinding(potentialMatchModel.NameMatchWinModel, "");
					nameMatchUserControl.Dock = DockStyle.Top;
					controls.Add(nameMatchUserControl);
				}

				if (potentialMatchModel.AddressMatchWinModel.MatchViewVisibility)
				{
					var addressMatchUserControl = new GenericMatchUserControl();
					addressMatchUserControl.SetDataBinding(potentialMatchModel.AddressMatchWinModel, "");
					addressMatchUserControl.Dock = DockStyle.Top;
					controls.Add(addressMatchUserControl);
				}

				if (potentialMatchModel.RegistrationCodeWinModel.MatchViewVisibility)
				{
					var registrationMatchUserControl = new GenericMatchUserControl();
					registrationMatchUserControl.SetDataBinding(potentialMatchModel.RegistrationCodeWinModel, "");
					registrationMatchUserControl.Dock = DockStyle.Top;
					controls.Add(registrationMatchUserControl);
				}

				var profileNotesUserControl = new ProfileNotesUserControl();
				profileNotesUserControl.SetDataBinding(potentialMatchModel.ProfileNotesWinModel, "");
				profileNotesUserControl.Dock = DockStyle.Top;
				controls.Add(profileNotesUserControl);

				var sourceListNamesUserControl = new SourceListNamesUserControl();
				sourceListNamesUserControl.SetDataBinding(potentialMatchModel.SourceListNamesWinModel, "");
				sourceListNamesUserControl.Dock = DockStyle.Top;
				controls.Add(sourceListNamesUserControl);

				controls.Reverse();
				PotentialMatchInfoPanel.Controls.AddRange(controls.ToArray());
			}
		}
	}
}
