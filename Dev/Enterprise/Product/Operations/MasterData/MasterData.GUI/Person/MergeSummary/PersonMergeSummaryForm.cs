using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	public partial class PersonMergeSummaryForm : ZChildForm
	{
		public GlbPerson retainedPerson;
		public List<GlbPerson> dissolvedPersons;
		protected PersonMergeParticipants personMergeParticipantsBizO;
		protected PersonMergeBusinessObjectCollection retainedCollection;
		protected PersonMergeBusinessObjectCollection dissolvedCollection;

		public PersonMergeSummaryForm(GlbPerson personToRetained, List<GlbPerson> personToDissolved)
		{
			retainedPerson = personToRetained;
			dissolvedPersons = personToDissolved;

			InitializeComponent();

			CandidatesControls.CandidatesBoundGrid.ColourDeciding += CandidatesBoundGrid_ColourDeciding;
			RetainedControls.RetainedBoundGrid.ColourDeciding += CandidatesBoundGrid_ColourDeciding;
			CandidatesControls.CandidatesBoundGrid.ForeColourDeciding += GetForColourFromCellText;
		}

		void LoadData()
		{
			retainedCollection = new PersonMergeBusinessObjectCollection();
			dissolvedCollection = new PersonMergeBusinessObjectCollection();

			retainedCollection.Add(new PersonMergeBusinessObject(retainedPerson));

			foreach (var person in dissolvedPersons)
			{
				dissolvedCollection.Add(new PersonMergeBusinessObject(person)
				{
					MergeStatus = ParticipantStatus.Queued
				});
			}

			personMergeParticipantsBizO = new PersonMergeParticipants(retainedCollection, dissolvedCollection);

			SetDataBinding(personMergeParticipantsBizO, "");
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			LoadData();
			SetMandatoryColumns();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			CloseButton.Focus();
		}

		void SetMandatoryColumns()
		{
			RetainedControls.RetainedBoundGrid.Columns["FullName"].IsMandatory = true;
			CandidatesControls.CandidatesBoundGrid.Columns["FullName"].IsMandatory = true;
			CandidatesControls.CandidatesBoundGrid.Columns["MergeStatus"].IsMandatory = true;
		}

		void CandidatesBoundGrid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			e.Colour = Color.White;
		}

		public Color GetForColourFromCellText(string cellText)
		{
			Color color;
			if (cellText == ParticipantStatus.Queued)
			{
				color = Color.Orange;
			}
			else if (cellText == ParticipantStatus.Merging)
			{
				color = Color.Blue;
			}
			else if (cellText == ParticipantStatus.MergedWithErrors || cellText == ParticipantStatus.FailedWithCriticalError)
			{
				color = Color.Red;
			}
			else if (cellText == ParticipantStatus.Completed)
			{
				color = Color.Green;
			}
			else
			{
				color = Color.Black;
			}

			return color;
		}

		public override string FormCaption => Res.GetString("A8CA4108-98BC-471D-B74F-F6C5F34E25A0", "Person Merge Summary");

		public bool IsMergingInProgress { get; protected set; }
		bool IsMergingComplete;

		public void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		public async void MergeButton_Click(object sender, EventArgs e)
		{
			MergeWarningLabel.Visible = false;
			IsMergingInProgress = true;

			SetEnabledButons(false);
			CandidatesControls.CandidatesBoundGrid.Enabled = false;

			var multiPerson = GetMultiPersonMerger(retainedCollection, dissolvedCollection);

			multiPerson.MergeProgressNotification += OnMergeProgressNotification;

			var result = await multiPerson.MergeSelected();

			CandidatesControls.CandidatesBoundGrid.Enabled = true;
			SetEnabledButons(true);

			MergeButton.Visible = !result;
			IsMergingComplete = result;
			if (!IsMergingComplete)
			{
				MergeWarningLabel.Visible = true;
			}

			CloseButton.Text = Res.GetString("E952A2A8-E523-415C-BFBB-7291C7E10AE7", "OK");

			IsMergingInProgress = false;
		}

		protected virtual MultiPersonMerger GetMultiPersonMerger(PersonMergeBusinessObjectCollection retainedPersonCollection, PersonMergeBusinessObjectCollection dissolvedPersonCollection)
		{
			return new MultiPersonMerger(retainedPersonCollection, dissolvedPersonCollection);
		}

		protected virtual void OnMergeProgressNotification(object sender, MergeProgressEventArgs e)
		{
			UpdateStatusBar(Res.GetString("57D68738-3696-4207-AA57-DD47D102E9EB", "Merge ({0} of {1}) were processed ...", e.ProgressCount.ToString(CultureInfo.InvariantCulture), e.ProgressTotal.ToString(CultureInfo.InvariantCulture)), null);

			CandidatesControls.CandidatesBoundGrid.CurrentRowIndex = e.ProgressCount - 1;
		}

		void SetEnabledButons(bool value)
		{
			MergeButton.Enabled = value;
			CloseButton.Enabled = value;
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			if (IsMergingInProgress)
			{
				e.Cancel = true;
				DialogResult = DialogResult.Cancel;
			}
			if (IsMergingComplete)
			{
				DialogResult = DialogResult.Yes;
			}
			else
			{
				base.OnFormClosing(e);
			}
		}
	}
}
