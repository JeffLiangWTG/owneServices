using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public partial class OpportunityCommissionAgreementsTab : ZUserControl, IReadOnlyToggleControl
	{
		public OpportunityCommissionAgreementsTab()
		{
			InitializeComponent();
			AddCommissionAgreementControl();
		}

		#region CurrentDataItem

		new OrgOpportunity CurrentDataItem
		{
			get { return (OrgOpportunity)base.CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			savedConflicts = CurrentDataItem != null ? ConflictsFinder.GetMoreGenericCommissionAgreementConflicts(CurrentDataItem, false) : null;
		}

		#endregion

		#region Load

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				SetupAgreementsGrid();
				var form = ParentForm as ZForm;
				if (form != null)
				{
					form.ValidatingForSave += Form_ValidatingForSave;
					form.Saved += Form_Saved;
				}
			}
		}

		void Form_ValidatingForSave(object sender, ValidatingForSaveEventArgs e)
		{
			EmailCreatorToCreateOnSuccessfulSave = null;
			if (e.ContinueWithSave == ContinueWithSave.Yes)
			{
				e.ContinueWithSave = CheckCommissionAgreementConflicts();
			}
		}

		void Form_Saved(object sender, EventArgs e)
		{
			RefreshControlReadOnly();

			savedConflicts = CurrentDataItem != null ? ConflictsFinder.GetMoreGenericCommissionAgreementConflicts(CurrentDataItem, false) : null;

			if (EmailCreatorToCreateOnSuccessfulSave != null)
			{
				try
				{
					EmailCreatorToCreateOnSuccessfulSave.CreateAndSave();
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
				finally
				{
					EmailCreatorToCreateOnSuccessfulSave = null;
				}
			}
		}

		#endregion

		#region CommissionAgreementControl

		public CommissionAgreementControl CommissionAgreementControl
		{
			get { return commissionAgreementControl; }
		}

		CommissionAgreementControl commissionAgreementControl;

		void AddCommissionAgreementControl()
		{
			commissionAgreementControl = CommissionAgreementControl.New();
			commissionAgreementControl.SuspendLayout();

			this.mainSplitContainer.Panel2.Controls.Add(this.commissionAgreementControl);

			this.BindingSource.SetBindingMember(this.commissionAgreementControl, "CommissionAgreementsForEdit");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((OrgCommissionAgreement)(((System.Collections.IList)((OrgOpportunity)null).CommissionAgreementsForEdit).SyncRoot));
			this.commissionAgreementControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.commissionAgreementControl.Name = "CommissionAgreementControl";
			this.commissionAgreementControl.TabIndex = 0;

			this.commissionAgreementControl.ResumeLayout(true);
			this.commissionAgreementControl.PerformLayout();
		}

		#endregion

		#region AgreementsGrid

		void SetupAgreementsGrid()
		{
			AgreementsGrid.Sort = OrgCommissionAgreementSchema.Constants.CA0_Name;
			NewAgreementButton.Image = Icons.GetImage(IconTypes.NewButtonRest);
			RefreshAgreementsGridReadOnly();
			SetupAgreementsGridContextMenu();

			var opportunity = CurrentDataItem;
			if (opportunity != null)
			{
				opportunity.BeforeUpdatedByDataRefresh += BeforeUpdateByDataRefresh;
			}
		}

		void BeforeUpdateByDataRefresh(object sender, EventArgs e)
		{
			var opportunity = CurrentDataItem;
			if (opportunity != null)
			{
				opportunity.RemoveDuplicatedDrafts();
			}
		}

		void RefreshAgreementsGridReadOnly()
		{
			NewAgreementButton.Enabled = !readOnly && CurrentDataItem != null && CurrentDataItem.CreateCommissionAgreementsAllowed;
		}

		void NewAgreementButton_Click(object sender, EventArgs e)
		{
			var opportunity = CurrentDataItem;
			if (opportunity != null)
			{
				var newAgreement = opportunity.CommissionAgreementsForEdit.AddNew();
				var index = AgreementsGrid.ListManager.List != null ? AgreementsGrid.ListManager.List.IndexOf(newAgreement) : -1;
				if (index >= 0)
				{
					AgreementsGrid.ListManager.Position = index;
				}
			}
		}

		#region AgreementsGrid Context Menu

		void SetupAgreementsGridContextMenu()
		{
			DisableMenuItem = new ZMenuItem(ResString.GetMultilingualString("2b4e64f7-26be-4f1c-ace2-17cc1a080f10", "Disable"), OnDisableMenuItemClick);
			ReverseMenuItem = new ZMenuItem(ResString.GetMultilingualString("5f0eea7f-c9c6-4415-80e2-d78dacf61024", "Reverse"), OnReverseMenuItemClick);
			var insertIndex = AgreementsGrid.ContextMenu.MenuItems.IndexOf(AgreementsGrid.DeleteMenuItem);
			if (insertIndex == -1)
			{ insertIndex = AgreementsGrid.ContextMenu.MenuItems.Count; }

			AgreementsGrid.ContextMenu.MenuItems.Add(insertIndex + 1, DisableMenuItem);
			AgreementsGrid.ContextMenu.MenuItems.Add(insertIndex + 2, ReverseMenuItem);

			AgreementsGrid.ContextMenu.Popup += AgreementsGridContextMenu_Popup;
		}

		void OnDisableMenuItemClick(object sender, EventArgs e)
		{
			if (!OrgCommissionAgreement.ExpireAgreementSecurityCheckpoint.IsAllowed)
			{
				OrgCommissionAgreement.ExpireAgreementSecurityCheckpoint.ShowError();
				return;
			}

			var selectedAgreements = GetAgreementsGridSelectedElements();
			if (!selectedAgreements.Any())
			{
				Globals.Message.ShowError(Res.GetString("2d326b6e-49bb-419f-b0f8-ed4fb5f1b34d", "Please select a commission agreement to disable."), CannotDisableCaption);
				return;
			}

			var nonReversedSelectedAgreements = selectedAgreements.Where(x => !x.IsReversed).ToArray();
			if (nonReversedSelectedAgreements.Length == 0)
			{
				Globals.Message.ShowError(Res.GetString("e0c7d9b5-442b-4fd1-be6e-0c11bcf3aa37", "All selected agreements have already been reversed."), CannotDisableCaption);
				return;
			}

			var expireAgreementAction = new ExpireCommissionAgreementAction(nonReversedSelectedAgreements);
			var expireAgreementForm = new ExpireCommissionAgreementForm(expireAgreementAction);
			ZFormModaliser.ShowDialogAndDispose(expireAgreementForm);
		}

		static ZString CannotDisableCaption
		{
			get { return Res.GetString("da5bd512-10f9-42f3-873b-8d046913e402", "Cannot Disable"); }
		}

		void OnReverseMenuItemClick(object sender, EventArgs e)
		{
			if (!OrgCommissionAgreement.ReverseAgreementSecurityCheckpoint.IsAllowed)
			{
				OrgCommissionAgreement.ReverseAgreementSecurityCheckpoint.ShowError();
				return;
			}

			var selectedAgreements = GetAgreementsGridSelectedElements();
			if (!selectedAgreements.Any())
			{
				Globals.Message.ShowError(Res.GetString("8ac67078-07cb-4008-ae42-4a4632fbe497", "Please select a commission agreement to reverse."), CannotReverseCaption);
				return;
			}

			var nonReversedSelectedAgreements = selectedAgreements.Where(x => !x.IsReversed).ToArray();
			if (nonReversedSelectedAgreements.Length == 0)
			{
				Globals.Message.ShowError(Res.GetString("e0c7d9b5-442b-4fd1-be6e-0c11bcf3aa37", "All selected agreements have already been reversed."), CannotReverseCaption);
				return;
			}

			foreach (var selectedAgreement in nonReversedSelectedAgreements)
			{
				selectedAgreement.Reverse();
			}
		}

		static ZString CannotReverseCaption
		{
			get { return Res.GetString("c3094700-d827-47a4-9ae7-e38f6f197297", "Cannot Reverse"); }
		}

		void AgreementsGridContextMenu_Popup(object sender, EventArgs e)
		{
			var isListEditable = AgreementsGrid.List.AllowEdit;
			var hasElementSelected = GetAgreementsGridSelectedElements().Any();
			DisableMenuItem.Visible = isListEditable;
			DisableMenuItem.Enabled = hasElementSelected;
			ReverseMenuItem.Visible = isListEditable;
			ReverseMenuItem.Enabled = hasElementSelected;
		}

		IEnumerable<OrgCommissionAgreement> GetAgreementsGridSelectedElements()
		{
			var result = AgreementsGrid.SelectedElements.OfType<OrgCommissionAgreement>();
			if (!result.Any() && AgreementsGrid.ListManager.Position >= 0)
			{
				result = new[] { (OrgCommissionAgreement)AgreementsGrid.ListManager.GetCurrent() };
			}

			return result;
		}

		ZMenuItem DisableMenuItem;
		ZMenuItem ReverseMenuItem;

		#endregion

		#endregion

		#region CheckCommissionAgreementConflicts

		CommissionAgreementConflictsFinder ConflictsFinder
		{
			get { return conflictsFinder ?? (conflictsFinder = CommissionAgreementConflictsFinder.New()); }
		}
		CommissionAgreementConflictsFinder conflictsFinder;

		IEnumerable<ICommissionAgreementConflict> savedConflicts;

		ContinueWithSave CheckCommissionAgreementConflicts()
		{
			var result = ContinueWithSave.Yes;

			if (CurrentDataItem != null)
			{
				var moreSpecificConflicts = ConflictsFinder.GetMoreSpecificCommissionAgreementConflicts(CurrentDataItem, true);
				if (moreSpecificConflicts.Any())
				{
					var message = CommissionAgreementConflictsTextProvider.New().ToConflictWarningMessage(moreSpecificConflicts);
					Globals.Message.ShowWarning(message, Res.GetString("b4889a5e-105d-475a-af5b-683c21cd7083", "More generic commission agreement has been added"));
				}
				else
				{
					var newConflicts = ConflictsFinder.GetNewCommissionAgreementConflicts(CurrentDataItem, savedConflicts);
					if (newConflicts.Count > 0)
					{
						var emailCreator = new BulkCommissionAgreementConflictEmailCreator(CurrentDataItem.Factory, newConflicts);
						var notificationPromptMessage = emailCreator.GetNotificationPromptMessage();
						if (!notificationPromptMessage.IsEmpty)
						{
							var dialogResult = Globals.Message.Show(
									notificationPromptMessage,
									Res.GetString("5a6d0878-c928-4ba0-8ba3-4f7e04feaa0a", "More specific commission agreement has been added"),
									MessageBoxButtons.YesNo,
									DialogResult.No);

							if (dialogResult == DialogResult.Yes)
							{
								EmailCreatorToCreateOnSuccessfulSave = emailCreator;
							}
							else
							{
								result = ContinueWithSave.No;
							}
						}
					}
				}
			}

			return result;
		}

		BulkCommissionAgreementConflictEmailCreator EmailCreatorToCreateOnSuccessfulSave;

		#endregion

		#region ReadOnly

		[DefaultValue(false)]
		public bool ReadOnly
		{
			get { return readOnly; }
			set
			{
				if (readOnly != value)
				{
					readOnly = value;
					RefreshControlReadOnly();
				}
			}
		}
		bool readOnly;

		void RefreshControlReadOnly()
		{
			RefreshAgreementsGridReadOnly();
		}

		#endregion
	}
}
