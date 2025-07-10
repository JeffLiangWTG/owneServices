using System;
using System.Windows.Forms;
using Enterprise.Integration.Recruiter;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Recruiter.GUI
{
	public partial class AccreditationUpdaterForm : ZChildForm, IAccreditationUpdaterForm
	{
		public AccreditationUpdaterForm(AccreditationUpdater updater) : base(updater)
		{
			InitializeComponent();
		}

		AccreditationUpdater Updater
		{
			get { return (AccreditationUpdater)CurrentDataItem; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (Updater != null)
			{
				Updater.IsTimePeriodTypeInfo.ValueChanged -= IsTimePeriodTypeInfo_ValueChanged;
				Updater.IsTimePeriodTypeInfo.ValueChanged += IsTimePeriodTypeInfo_ValueChanged;
			}

			UpdateControlVisibility();
		}

		void IsTimePeriodTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateControlVisibility();
		}

		void UpdateControlVisibility()
		{
			SuspendLayout();
			try
			{
				var updater = Updater;
				if (updater != null)
				{
					completionToleranceDaysNumericUpDown.Visible = updater.IsFirstExamType;
					startDateEdit.Visible = updater.IsTimePeriodType;
					endDateEdit.Visible = updater.IsTimePeriodType;
					additionalToleranceDaysNumericUpDown.Visible = updater.IsTimePeriodType;
				}
				else
				{
					completionToleranceDaysNumericUpDown.Visible = false;
					startDateEdit.Visible = false;
					endDateEdit.Visible = false;
					additionalToleranceDaysNumericUpDown.Visible = false;
				}
			}
			finally
			{
				ResumeLayout();
			}
		}

		void RunUpdateButton_Click(object sender, EventArgs e)
		{
			Updater.ValidateAll();
			if (Updater.HasErrors)
			{
				Globals.Message.ShowError(Res.GetString("257ef184-4585-4b74-b432-c8ed4c7a7501", "The form's errors must be fixed before the update can be run"));
			}
			else
			{
				Updater.RunBegin += Updater_RunBegin;
				Updater.RunEnd += Updater_RunEnd;

				if (Updater.IsForAccreditation)
				{
					Updater.ExamAttemptProcessed += Updater_AccreditationExamAttemptProcessed;
				}
				else if (Updater.IsForPerson)
				{
					Updater.ExamAttemptProcessed += Updater_PersonExamAttemptProcessed;
					Updater.PersonProcessed += Updater_PersonProcessed;
				}

				try
				{
					Updater.Run();
				}
				catch (ArgumentException)
				{
					Globals.Message.ShowError(Res.GetString("81a27a99-798f-49a2-9509-842d9a722e7d",
						"The updater has neither an allocated accreditation nor person. One of the two is required to run the update."));
				}
				finally
				{
					Close();
				}
			}
		}

		ProgressForm progressForm;

		void Updater_PersonExamAttemptProcessed(object sender, ExamAttemptProcessedEventArgs e)
		{
			progressForm.SetStatusAndPercentComplete(
				Res.GetString("673d2def-21c9-4dc7-a265-89a7d47c4394", "Walking through past exam attempts... Exam Attempts {0}/{1}",
					e.ExamsProcessed, e.ExamsTotal), (int)((e.ExamsProcessed / (decimal)e.ExamsTotal) * 100m));
		}

		void Updater_PersonProcessed(object sender, PersonProcessedEventArgs e)
		{
			progressForm.SetStatusAndPercentComplete(
				Res.GetString("6C55948F-E0BF-4075-84C3-64A59155760D", "Creating accreditation attempts for Persons...Persons {0}/{1}.",
					e.PersonsProcessed, e.PersonsTotal), (int)((e.PersonsProcessed / (decimal)e.PersonsTotal) * 100m));
		}

		void Updater_AccreditationExamAttemptProcessed(object sender, ExamAttemptProcessedEventArgs e)
		{
			progressForm.SetStatusAndPercentComplete(
				Res.GetString("91925ff7-5ddf-44b5-864a-fdbf4283a7af", "Walking through past exam attempts...\r\nApplicants {0}/{1}, Exams {2}/{3}",
					e.ApplicantsProcessed, e.ApplicantsTotal, e.ExamsProcessed, e.ExamsTotal), (int)((e.ExamsProcessed / (decimal)e.ExamsTotal) * 100m));
		}

		void Updater_RunEnd(object sender, EventArgs e)
		{
			Updater.RunBegin -= Updater_RunBegin;
			Updater.RunEnd -= Updater_RunEnd;
			if (Updater.IsForAccreditation)
			{
				Updater.ExamAttemptProcessed -= Updater_AccreditationExamAttemptProcessed;
			}
			else if (Updater.IsForPerson)
			{
				Updater.ExamAttemptProcessed -= Updater_PersonExamAttemptProcessed;
				Updater.PersonProcessed -= Updater_PersonProcessed;
			}

			progressForm.Close();
			progressForm.Dispose();

			Globals.Message.Show(
				Res.GetString("FD97042A-9A0F-4EA5-92DD-3FAB29A704D7", "The process has been successfully completed."),
				Res.GetString("93A346BC-DAAF-4BD3-A8E2-65925F861C95", "Process finished"),
				MessageBoxButtons.OK, DialogResult.OK);
		}

		void Updater_RunBegin(object sender, EventArgs e)
		{
			progressForm = new ProgressForm();
			progressForm.ShowCancelButton = true;
			progressForm.ShowProgressBar = true;
			progressForm.Cancelled += Updater_Cancel;
			progressForm.ShowModalTo(FindForm());
		}

		void Updater_Cancel(object sender, EventArgs e)
		{
			Updater.Cancel();
			progressForm.Close();
			progressForm.Dispose();
		}
	}
}
