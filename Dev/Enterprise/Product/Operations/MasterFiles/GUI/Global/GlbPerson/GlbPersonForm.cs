using System;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GlbPersonForm : ZTemplateForm
	{
		public GlbPersonForm(GlbPerson person) : base(person)
		{
			Person = person;
			InitializeComponent();
			SetGenderImage();
			WorkflowTabPage.Initialize(person);
			recalculatePatternsInitializer = new RecalculatePatternsInitializer(person);
			recalculatePatternsInitializer.CreateRecalculateMenuItem(ActionsMenuItem, (o, e) => { person.RegeneratePatternTables(); });
			SetupEventHandlers();
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			ControllerID = ControllerID ?? ControllerIDs.GlbPerson;
			updater = ObjectFactory.Get<IAccreditationUpdaterForPerson>("IAccreditationUpdaterForPerson", person.Factory);
			SetupTabPageSecurity();
			UpdateLockedOutControls();
			HidePersonChangedDetailFromLogAndNote();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetupActionsMenu();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			if (initialAttempt != null)
			{
				MainTabControl.SelectedTab = initialTabPage;
				AccreditationsControl.SelectAttempt(initialAttempt);
			}
		}

		public override string FormCaption
		{
			get
			{
				return Person?.HumanReadableName ?? base.FormCaption;
			}
		}

		public void SetInitialAttempt(IGlbAccreditationAttempt attempt)
		{
			initialTabPage = AccreditationsTabPage;
			initialAttempt = attempt;
		}

		ZTabPage initialTabPage;
		IGlbAccreditationAttempt initialAttempt;

		void SetupActionsMenu()
		{
			if (Person != null && !Person.ReadOnly && Env.Security.PersonIntelligenceEdit.IsAllowed && Env.Security.GlbAccreditationAttemptView.IsAllowed && Env.Security.GlbAccreditationAttemptEdit.IsAllowed)
			{
				ActionsMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("1addc952-8c07-4ee7-85cd-f6b7f9a976ed", "Create Accreditation Attempt for Existing Exam Attempts"), CreatePastAccreditationAttempts));
				ActionsMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("053602e8-beee-44c9-af8a-7b3bd1f17faa", "Delete Existing Accreditation Attempts for this Person"), DeleteExistingAccreditations));
				ActionsMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("cfb785a1-344d-4a47-b511-913671088f12", "Delete Existing Accreditation Attempts and their Related Certificates for this Person"), DeleteExistingAccreditationsAndCertificates));
			}
		}

		readonly IAccreditationUpdaterForPerson updater;

		internal void CreatePastAccreditationAttempts(object sender, EventArgs eventArgs)
		{
			if (IsPersonSaved())
			{
				updater.SetPersons(new[] { DataSource as IGlbPerson });
				var updaterForm = ObjectFactory.Get<IAccreditationUpdaterForm>("IAccreditationUpdaterForm", updater);
				ZFormModaliser.ShowDialogAndDispose((Form)updaterForm);
			}
		}

		void DeleteExistingAccreditations(object sender, EventArgs eventArgs)
		{
			if (IsPersonSaved())
			{
				updater.SetPersons(new[] { DataSource as IGlbPerson });
				updater.DeleteExistingCertificates = false;
				updater.DeleteAttemptsAndCertificates();
			}
		}

		void DeleteExistingAccreditationsAndCertificates(object sender, EventArgs eventArgs)
		{
			if (IsPersonSaved())
			{
				updater.SetPersons(new[] { DataSource as IGlbPerson });
				updater.DeleteExistingCertificates = true;
				updater.DeleteAttemptsAndCertificates();
			}
		}

		bool IsPersonSaved()
		{
			if (!Person.IsInDatabase || Person.HasChanges)
			{
				Globals.Message.ShowError(
					Res.GetString("bb7fb4c1-f48f-49e7-9e96-055f8603eacf", "Cannot create Accreditation Attempts until {0} is saved.", Person.HumanReadableName),
					Res.GetString("ff4490f6-51de-4ac1-aa4d-ec2c4f3fd352", "Cannot create Accreditation Attempts"));

				return false;
			}

			return true;
		}

		void SetupTabPageSecurity()
		{
			AccreditationsTabPage.SetupSecurity(new SecurityCheckpoint[] { Env.Security.GlbAccreditationAttemptView });
		}

		void HidePersonChangedDetailFromLogAndNote()
		{
			if (!Env.Security.PersonIntelligenceViewNotesText.IsAllowed)
			{
				NotesTabPage.TabInitialized += (_, __) =>
				{
					NotesTabPage.HideNoteText();
				};
			}

			if (!Env.Security.PersonIntelligenceViewLogsReference.IsAllowed)
			{
				LogsTabPage.TabInitialized += (_, __) =>
				{
					LogsTabPage.HideLogReferenceAndEventDetail();
				};
			}
		}

		void SetupEventHandlers()
		{
			Person.PatternMatchingRecalculator.RecalculateStart += Person_RegeneratStart;
			Person.PatternMatchingRecalculator.Recalculating += Person_Regenerating;
			Person.PatternMatchingRecalculator.Recalculated += Person_Regenerated;
		}

		void UninstallEventHandlers()
		{
			Person.PatternMatchingRecalculator.RecalculateStart -= Person_RegeneratStart;
			Person.PatternMatchingRecalculator.Recalculating -= Person_Regenerating;
			Person.PatternMatchingRecalculator.Recalculated -= Person_Regenerated;
		}

		GlbPerson Person { get; }

		void SetGenderImage()
		{
			if (Person.IsFemale)
			{
				FemaleIcon.Visible = true;
			}
			else if (Person.IsMale)
			{
				MaleIcon.Visible = true;
			}
		}

		void EditButton_Click(object sender, EventArgs e)
		{
			if (!Person.HasErrors)
			{
				var controller = ZControllerFactory.Create(ControllerIDs.GlbPersonNew);
				controller.SetFormsModalTo(this);
				controller.ShowEditForm(Person);
			}
			else
			{
				Globals.Message.Show(Res.GetString("67F57EEE-9903-4E00-B2D9-42FE8A7B20A4", "Please fix all validation errors first."));
			}
		}

		protected override void ShowNewForm()
		{
			ZControllerFactory.Create(ControllerIDs.GlbPersonNew).ShowNewForm().Show();
		}

		protected override bool SupportsEDocs
		{
			get { return true; }
		}

		readonly RecalculatePatternsInitializer recalculatePatternsInitializer;

#if DEBUG
		internal bool isrecalculateprogressFormShownForTest;
#endif
#if DEBUG
		internal
#endif
		ProgressForm recalculateprogressForm;

		void Person_RegeneratStart(object sender, EventArgs e)
		{
			recalculateprogressForm = new ProgressForm();
			recalculateprogressForm.Show(this);
			recalculateprogressForm.ShowCancelButton = false;
#if DEBUG
			if (Globals.IsTest)
			{
				isrecalculateprogressFormShownForTest = true;
			}
#endif
		}

		void Person_Regenerating(object sender, RecalculatingEventArgs e)
		{
			if (recalculateprogressForm != null && !recalculateprogressForm.IsDisposed)
			{
				recalculateprogressForm.SetStatusAndPercentComplete(e.ProgressText, e.Progress);
			}
		}

		void Person_Regenerated(object sender, RecalculatedEventArgs e)
		{
			Person.Factory.Save();
			if (recalculateprogressForm != null && !recalculateprogressForm.IsDisposed)
			{
				recalculateprogressForm.Close();
				recalculateprogressForm = null;
			}

			if (string.IsNullOrEmpty(e.ErrorMessage))
			{
				var effectStr = e.EffectiveCount <= 1 ? e.EffectiveCount + Res.GetString("CBBD41CE-BE07-44C4-9C71-FDFD0AE73B46", "{0}", " record affected.") : e.EffectiveCount + Res.GetString("605E3D3A-42C6-4CB7-B393-9B8759A57E49", "{0}", " records affected.");
				Person_ShowMessage(Res.GetString("6CBEC24B-A47E-4FC1-A3AA-39EB41045A2C", "Regeneration has completed successfully!"), Res.GetString("E3513AA3-A335-4ACB-809E-F04762CC379B", "Result: ") + effectStr);
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("F915153D-69BD-49B6-9C80-B5E496294133", "Error Message: {0}", e.ErrorMessage), Res.GetString("AFC1B299-04D6-4016-A7D1-FD62AD5B1DFE", "Pattern tables regeneration failed"));
			}
		}

		void Person_ShowMessage(string caption, string message)
		{
			Globals.Message.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		void UpdateLockedOutControls()
		{
			var isLockedOut = Person?.IsLockedOut ?? false;
			UnlockButton.Visible = isLockedOut;
			LockoutDateTimeBoundDate.Visible = isLockedOut;
		}

		protected void UnlockButton_Click(object sender, EventArgs e)
		{
			Person?.Unlock();
			UpdateLockedOutControls();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					UninstallEventHandlers();

					if (components != null)
					{
						components.Dispose();
					}

					if (recalculateprogressForm != null && !recalculateprogressForm.IsDisposed)
					{
						recalculateprogressForm.Close();
					}
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}
	}
}
