using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.Business;
using Enterprise.Workflow.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

#pragma warning disable IDE0001 // Simplify names. Designer requires fully qualified names to correctly deserialize properties

namespace Enterprise.Recruiter.GUI
{
	public partial class HRJobOpeningsForm : ZTemplateForm
	{
		public HRJobOpeningsForm(HRRecruitmentJobCampaign campaign)
			: base(campaign)
		{
			SetupEmailMenuItems();

			campaign.NewApplicationsAdded += new EventHandler<HRRecruitmentJobCampaign.NewApplicationsAddedEventArgs>(Campaign_NewApplicationsAdded);

			if (Globals.IsTest)
			{
				TypeDescriptor.AddAttributes(EmailApplicantsButton, new SuppressFormsLocalizedTestAttribute());
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			var menuItem = ReapplyWorkflowTemplateMenuItemProvider.GetReapplyWorkflowTemplateMenuItemForFilterGrid(ApplicationsGrid.GetSelectedElements<HRJobApplication>());
			ApplicationsGrid.ContextMenu.MenuItems.Add(menuItem);

			AddAuditColumns(BusinessEntity.Applications);
		}

		#region Controllers

		protected ZController JobApplicantController
		{
			get
			{
				if (contactController == null)
				{
					contactController = ZControllerFactory.Create(ControllerIDs.HRJobApplicant);
				}
				return contactController;
			}
		}
		ZController contactController;

		protected void ResetJobApplicantController()
		{
			contactController = null;
		}

		protected ZController JobApplicationController
		{
			get
			{
				if (jobApplicationController == null)
				{
					jobApplicationController = ZControllerFactory.Create(ControllerIDs.HRJobApplication);
				}
				return jobApplicationController;
			}
		}
		ZController jobApplicationController;

		protected void ResetJobApplicationController()
		{
			jobApplicationController = null;
		}

		#endregion

		bool IsApplicantSelected => ApplicationsGrid.GetSelectedElements<HRJobApplication>().Length > 0;

		void AddAuditColumns(HRJobApplicationDependentCollection applications)
		{
			if (applications != null)
			{
				FilterStripAuditDetails.AddAuditDetailsColumns(ApplicationsGrid, ((IBusinessObjectCollection)applications).TableName, typeof(HRJobApplication));
			}
		}

		internal void ShowControllerNewForm()
		{
			ResetJobApplicantController();
			var form = (HRJobApplicantForm)JobApplicantController.ShowNewForm();
			form.Saved += (sender, e) =>
			{
				var applicant = form.BusinessEntity;

				var application = BusinessEntity.Applications.FirstOrDefault(a => a.HP_HA == applicant.PK);
				if (application == null)
				{
					application = BusinessEntity.Applications.AddNew();
					application.HP_HA = applicant.PK;
				}
				this.ContactPhoneDiallerUserControl.RefreshDialInfo(e);
			};
		}

		internal void ShowControllerEditForm()
		{
			HRJobApplication selectedApplication = (HRJobApplication)ApplicationsGrid.ListManager.GetCurrent();
			if (selectedApplication != null && BusinessEntity != null && selectedApplication.HP_HA.IsValid)
			{
				ResetJobApplicantController();
				var form = JobApplicantController.ShowEditForm(BusinessEntity.Factory.Load<HRJobApplicant>(selectedApplication.HP_HA));
				if (form != null)
				{
					form.Closed += (_, e) => ContactPhoneDiallerUserControl.RefreshDialInfo(e);
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("f8369921-086c-4dca-8242-54aa94fb57e7", "Ensure that a valid applicant and a valid application is selected."));
			}
		}

		protected void ShowControllerEditApplicationForm()
		{
			if (!BusinessEntity.IsInDatabase || BusinessEntity.HasChanges)
			{
				Globals.Message.ShowError(
					Res.GetString("7d115aa8-9a69-414b-b985-40b538e44ae2", "Cannot edit application values until the job opening is saved."));

				return;
			}

			HRJobApplication selectedApplication = (HRJobApplication)ApplicationsGrid.ListManager.GetCurrent();
			if (selectedApplication != null && BusinessEntity != null && selectedApplication.HP_HV.IsValid)
			{
				ResetJobApplicationController();
				JobApplicationController.ShowEditForm(selectedApplication);
			}
			else
			{
				Globals.Message.Show(Res.GetString("9c96810c-8760-4521-aecc-1786096abbbe", "Ensure that a valid applicant and a valid application is selected."));
			}
		}

		void ApplicationGrid_DoubleClick(object sender, EventArgs e)
		{
			ShowControllerEditForm();
		}

		void ApplicationGrid_ContextMenuPopup(object sender, EventArgs e)
		{
			EnableMenuItem();
		}

		internal void EnableMenuItem()
		{
			var menuItem = ApplicationsGrid.ContextMenu.MenuItems.Find(ReapplyWorkflowTemplateMenuItemProvider.MenuItemName, false).Single();
			menuItem.Enabled = IsApplicantSelected;
		}

		void SetupEmailMenuItems()
		{
			EmailApplicantsButton.DropDownItems.Clear();

			foreach (ApplicationStatus status in RecruiterDataRegistry.Instance.ApplicationStatuses.Value)
			{
				var stripItem = EmailApplicantsButton.DropDownItems.Add(status.Description, null, EmailMenuItem_Click);
				stripItem.Tag = status.Code;
			}
		}

		void NewApplicantButton_Click(object sender, EventArgs e)
		{
			ShowControllerNewForm();
		}

		void EditApplicantButton_Click(object sender, EventArgs e)
		{
			if (BusinessEntity.Applications.Cast<HRJobApplication>().Any())
			{
				ShowControllerEditForm();
			}
			else
			{
				Globals.Message.Show(Res.GetString("20a01d61-928d-4cf6-8c34-5c070a0499a4", "Please select an application to edit"));
			}
		}

		void AttachApplicantButton_Click(object sender, EventArgs e)
		{
			AttachApplicants();
			this.ContactPhoneDiallerUserControl.RefreshDialInfo(e);
		}

		void ApplicationEditButton_Click(object sender, EventArgs e)
		{
			if (BusinessEntity.Applications.Cast<HRJobApplication>().Any())
			{
				ShowControllerEditApplicationForm();
			}
			else
			{
				Globals.Message.Show(Res.GetString("20a01d61-928d-4cf6-8c34-5c070a0499a4", "Please select an application to edit"));
			}
		}

		protected virtual void InstantiateEmbeddedModulePopup(ZFilterModule filterModule)
		{
			lastShownAttachPopup = new ZArchitecture.GUI.Internal.EmbeddedModulePopup(filterModule);
		}

		protected virtual void HandlePopupUserEntry()
		{
			lastShownAttachPopup.ShowDialog();
		}

		protected internal void AttachApplicants()
		{
			var filterModule = ZFilterModule.GetZFilterModule(ModuleIDs.HRJobApplicant);

			InstantiateEmbeddedModulePopup(filterModule);
			filterModule.OverrideModuleDecisionProvider(new ApplicantDescisionProvider(lastShownAttachPopup));
			lastShownAttachPopup.EmbeddedModulePopupOKButtonStrategy = filterModule.ModuleDecisionProvider;

			HandlePopupUserEntry();

			var applicants = filterModule.ModuleDecisionProvider.List;
			if (applicants == null)
			{
				return;
			}

			foreach (var applicant in applicants.OfType<HRJobApplicant>())
			{
				if (!BusinessEntity.Applications.Any(a => a.HP_HA == applicant.PK))
				{
					var newApplication = BusinessEntity.Applications.AddNew();
					newApplication.HP_HA = applicant.PK;
				}
			}
		}

		protected Enterprise.ZArchitecture.GUI.Internal.EmbeddedModulePopup lastShownAttachPopup;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not using ZorKArchitecture")]
		void DetachApplicantButton_Click(object sender, EventArgs e)
		{
#if !WINZOR
			if (System.Windows.MessageBox.Show("Are you sure you want to detach the selected applicants?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No, System.Windows.MessageBoxOptions.None) == MessageBoxResult.Yes)
			{
				DetachApplicants();
				this.ContactPhoneDiallerUserControl.RefreshDialInfo(e);
			}
#endif
		}

		protected internal void DetachApplicants()
		{
			foreach (var selectedApplication in ApplicationsGrid.SelectedElements.OfType<HRJobApplication>())
			{
				BusinessEntity.Applications.Delete(selectedApplication);
			}
		}

		void EmailMenuItem_Click(object sender, EventArgs e)
		{
			ToolStripItem item = sender as ToolStripItem;
			if (item != null)
			{
				string status = item.Tag.ToString();
				SendEmailsToApplicants(status);
			}
		}

		void Campaign_NewApplicationsAdded(object sender, HRRecruitmentJobCampaign.NewApplicationsAddedEventArgs e)
		{
			NotificationEmailTemplate template = RecruiterDataRegistry.Instance.NewJobApplicationEmailTemplate.Value;
			MultipleEmailToContactSender emailSender = new MultipleEmailToContactSenderCreator().Create(BusinessEntity, e.applications, template);
			ZFormModaliser.ShowDialogAndDispose(new EmailMultipleContactsForm(emailSender));
		}

		void BottomTabControl_Selecting(object sender, TabControlCancelEventArgs e)
		{
			if (e.TabPage == ApplicantsTabPage)
			{
				ApplicationsGrid.Refresh();
			}
		}

		public new HRRecruitmentJobCampaign BusinessEntity
		{
			get { return (HRRecruitmentJobCampaign)base.BusinessEntity; }
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				if (BusinessEntity != null)
				{
					BusinessEntity.NewApplicationsAdded -= Campaign_NewApplicationsAdded;
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Send Emails To Applicants

		void SendEmailsToApplicants(string status)
		{
			if (BusinessEntity.HasChanges || !BusinessEntity.IsInDatabase)
			{
				Globals.Message.ShowError(SaveBeforeSendingErrorMessage, Res.GetString("40503183-8c52-402f-bc2a-f6a6807db30e", "Cannot send to applicants"));
			}
			else
			{
				IEnumerable<HRJobApplication> selectedApplications = ApplicationsGrid.SelectedElements.Length > 0
					? ApplicationsGrid.SelectedElements.Cast<HRJobApplication>()
					: BusinessEntity.Applications.Cast<HRJobApplication>();
				HRJobApplication[] applicationsToSendTo = selectedApplications.Where(a => a.Applicant != null && a.HP_CurrentStatus == status).ToArray();

				if (applicationsToSendTo.Length > 0)
				{
					NotificationEmailTemplate template = RecruiterDataRegistry.Instance.ApplicationStatuses.Value.GetTemplateFromCode(status);
					MultipleEmailToContactSender sender = new MultipleEmailToContactSenderCreator().Create(BusinessEntity, applicationsToSendTo, template);
					sender.EmailsSent += delegate
					{
						foreach (EmailToApplicantBusinessObject emailBizO in sender.EmailsToContacts)
						{
							emailBizO.application.HP_CurrentStatus = status;
						}
					};
					ZFormModaliser.ShowDialogAndDispose(new EmailMultipleContactsForm(sender));
				}
				else
				{
					Globals.Message.ShowError(InvalidApplicationErrorMessage, Res.GetString("a8482e08-3215-42ab-b442-341bb59bf996", "No Applicants found"));
				}
			}
		}

		internal static string InvalidApplicationErrorMessage
		{
			get { return Res.GetString("654bc97a-892b-4191-8ae3-17c91fde13d0", "Applicants matching the selected status were not found. Please choose a status associated with at least one applicant"); }
		}
		internal static string SaveBeforeSendingErrorMessage
		{
			get { return Res.GetString("52737eee-4b95-41d0-a8bf-19fe746c18ec", "Changes have been made to this Campaign. You must save before sending Emails."); }
		}

		#endregion

		#region DragDrop

		protected override void OnDragDrop(System.Windows.Forms.DragEventArgs dragEvent)
		{
			if (!RecruiterDataRegistry.Instance.DaxtraEnable.Value)
			{
				base.OnDragDrop(dragEvent);
				return;
			}

			var insertResult = ResumeDragDropHelper.ProcessResumeDragDrop(dragEvent?.Data, this);

			if (insertResult.ShouldAddToEdocs)
			{
				using (var suspender = insertResult.ShouldSuspendEDocPopup ? EDocPopupSuspender.GetSuspender() : null)
				{
					base.OnDragDrop(dragEvent);
				}
			}
		}

		readonly FunctionalitySuspender EDocPopupSuspender = new FunctionalitySuspender();

		#endregion

		public class ApplicantDescisionProvider : IModuleDecisionProvider
		{
			public ApplicantDescisionProvider(Enterprise.ZArchitecture.GUI.Internal.EmbeddedModulePopup popup)
			{
				Popup = popup;
			}

			readonly Enterprise.ZArchitecture.GUI.Internal.EmbeddedModulePopup Popup;

			public bool ShouldDisplayNotifications => false;

			public bool ShouldLoadFilterBizObj => false;

			public bool ShouldSaveFilterBizObj => false;

			public bool ShouldIgnoreAdditionalFilter => false;

			public bool AllowExcelExport => false;

			public bool EnablePreviousNextSupport => false;

			public IBusinessObjectCollection List => selectedItems;

			public void HandleDefaultAction(BusinessObject[] selectedBusinessObject)
			{
				selectedItems = new HRJobApplicantCollection(new BusinessObjectFactory());
				foreach (var selectedItem in selectedBusinessObject)
				{
					selectedItems.Add(selectedItem);
				}
				Popup.Close();
			}

			public void HandleFindBoxOKButton(BusinessObject[] selectedBusinessObject)
			{
				selectedItems = new HRJobApplicantCollection(new BusinessObjectFactory());
				foreach (var selectedItem in selectedBusinessObject)
				{
					selectedItems.Add(selectedItem);
				}
				Popup.Close();
			}

			public void HandleFindBoxOKButton(FilterStripBusinessObject selectedFilters)
			{
				throw new NotImplementedException();
			}

			public void InitialiseFindBoxControllerLink(ZController controller)
			{
			}

			public void SetFindBoxCodeDescription(BusinessObject bizo)
			{
				throw new NotImplementedException();
			}

			HRJobApplicantCollection selectedItems;
		}
	}
}
