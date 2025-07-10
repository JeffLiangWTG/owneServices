using System;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.GUI
{
	public partial class GlbAccreditationForm : ZTemplateForm
	{
		public GlbAccreditationForm(GlbAccreditation accreditation)
			: base(accreditation)
		{
			InitializeComponent();
			SetupActionsMenu();
			updater = new AccreditationUpdaterForAccreditation(accreditation);
		}

		readonly AccreditationUpdater updater;

		ZMenuItem goBackInTimeMenuItem;
		ZMenuItem deleteaccreditationsMenuItem;
		ZMenuItem deleteaccreditationsAndCertificatesMenuItem;
		void SetupActionsMenu()
		{
			goBackInTimeMenuItem = new ZMenuItem(ResString.GetMultilingualString("e1fdedcc-497f-4683-bcf8-9c2d78b16ec1", "Create Accreditation Attempts for Existing Exam Attempts"), CreatePastAccreditationAttempts);
			deleteaccreditationsMenuItem = new ZMenuItem(ResString.GetMultilingualString("4740903f-021d-496e-85f2-a4c62c32239c", "Delete Existing Accreditation Attempts for this Accreditation"), DeleteExistingAccreditations);
			deleteaccreditationsAndCertificatesMenuItem = new ZMenuItem(ResString.GetMultilingualString("51fa9bd9-b7a7-4fdd-997a-c55d737ac86e", "Delete Existing Accreditation Attempts and their Related Certificates for this Accreditation"), DeleteExistingAccreditationsAndCertificates);
			ActionsMenuItem.MenuItems.Add(goBackInTimeMenuItem);
			ActionsMenuItem.MenuItems.Add(deleteaccreditationsMenuItem);
			ActionsMenuItem.MenuItems.Add(deleteaccreditationsAndCertificatesMenuItem);
		}

		void CreatePastAccreditationAttempts(object sender, EventArgs eventArgs)
		{
			if (IsAccreditationSaved())
			{
				ZFormModaliser.ShowDialogAndDispose(new AccreditationUpdaterForm(updater));
			}
		}

		void DeleteExistingAccreditations(object sender, EventArgs eventArgs)
		{
			if (IsAccreditationSaved())
			{
				updater.DeleteExistingCertificates = false;
				updater.DeleteAttemptsAndCertificates();
			}
		}

		void DeleteExistingAccreditationsAndCertificates(object sender, EventArgs eventArgs)
		{
			if (IsAccreditationSaved())
			{
				updater.DeleteExistingCertificates = true;
				updater.DeleteAttemptsAndCertificates();
			}
		}

		bool IsAccreditationSaved()
		{
			var accreditation = DataSource as GlbAccreditation;
			if (!accreditation.IsInDatabase || accreditation.HasChanges)
			{
				Globals.Message.ShowError(
					Res.GetString("fa76605b-c314-4403-8139-6a515002065f", "Cannot create Accreditation Attempts until {0} is saved.", accreditation.HumanReadableName),
					Res.GetString("96e1c7d1-0b1b-425e-8ff0-6002e990de0a", "Cannot create Accreditation Attempts"));

				return false;
			}

			return true;
		}

		protected override bool SupportsEDocs => false;

		protected override bool ShowNotesTab => false;

		public override string FormCaption
		{
			get { return Res.GetString("EA292EEE-D377-4969-BDD4-BA6E86D4D487", "Accreditation"); }
		}

		void PreReqGrid_DoubleClick(object sender, EventArgs e)
		{
			if (preReqGrid.InnerGrid.SelectedElements == null || preReqGrid.InnerGrid.SelectedElements.Length == 0)
			{
				return;
			}

			var accred = preReqGrid.InnerGrid.SelectedElements[0] as GlbAccreditation;

			if (accred == null)
			{
				return;
			}

			var controller = ZControllerFactory.Create(ControllerIDs.GlbAccreditation);
			controller.ShowEditForm(accred);
		}
	}
}
