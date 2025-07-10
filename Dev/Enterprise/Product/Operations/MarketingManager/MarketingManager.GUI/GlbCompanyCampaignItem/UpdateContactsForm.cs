using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration.Recruiter;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public partial class UpdateContactsForm : ZChildForm
	{
		public UpdateContactsForm(ContactsWithNonDeliveryReportsUpdater bizO)
			: base(bizO)
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				this.BackColor = SystemDataRegistry.Instance.ColorTheme.TabBackgroundColor;
			}

			HookEvents();

			NumCampaignsSentLabel.Text = CampaignsNotSentPostDeliveryLabel;
		}

		public UpdateContactsForm(int numOfEmailsActuallySent, IEnumerable<CampaignContact> contactsNotSentTo, ContactsWithNonDeliveryReportsUpdater contactsWithNonDeliveryReport, bool isScheduling = false)
			: this(contactsWithNonDeliveryReport)
		{
			contactsWithNonDeliveryReport.ContactsCollection.AddContacts(contactsNotSentTo);
			IsFirstSending = true;
			IsScheduling = isScheduling;
			NumCampaignsSentLabel.Text = GetInformationText(numOfEmailsActuallySent);

			DeliveryDetailsGroupBox.Visible = false;
			CargoWise.Windows.UI.ControlDpiScalingHelper.SetWidth(ContactsGrid, 1110, true);
		}

		protected string GetInformationText(int numOfEmailsActuallySent)
		{
			var campaign = BusinessEntity.Campaign;
			if (campaign.IsMasterCampaign)
			{
				return string.Format(CultureInfo.CurrentCulture, CampaignsContactsAddedLabel, numOfEmailsActuallySent);
			}

			if (campaign.IsTargetList)
			{
				return string.Format(CultureInfo.CurrentCulture, TargetListLabel, numOfEmailsActuallySent);
			}

			return string.Format(CultureInfo.CurrentCulture, IsScheduling ? ScheduleListLabel : CampaignsSentLabel, numOfEmailsActuallySent);
		}

		public UpdateContactsForm(Collection<IScheduleItemsProvider> contactsToBeScheduled, IEnumerable<CampaignContact> contactsNotSentTo, ContactsWithNonDeliveryReportsUpdater contactsWithNonDeliveryReport)
			: this(contactsToBeScheduled.Count, contactsNotSentTo, contactsWithNonDeliveryReport, true)
		{
			this.ContactsToBeScheduled = contactsToBeScheduled;
		}

		readonly bool IsFirstSending;
		readonly Collection<IScheduleItemsProvider> ContactsToBeScheduled;
		readonly bool IsScheduling;

		#region GUI Setup
		#pragma warning disable IDE0044 // conflicting warnings ( conflict between IDE0044 and Res.GetString ) suppressing the latest one, i.e., IDE0044
		string CampaignsSentLabel = Res.GetString("1c2e35e6-1fb9-41eb-8f94-7c800dc66f7d", @"Campaign was successfully sent to {0:G} contact(s).

			The campaign however could not be delivered to the following contacts. Possible reasons include:
			- Contact does not have an Email address
			- Contact has recently received a Non Delivery Receipt
			- Email address is malformed");

		string CampaignsContactsAddedLabel = Res.GetString("E534BD58-7292-4CCF-8BF5-499609615EE1", @"{0:G} contact(s) were successfully added to the Campaign List.

			The Campaign however cannot currently be delivered to the following contacts and require action to correct them. Possible reasons include:
			– The Contact does not have an email address
			– The Email address is malformed
			– The Contact has recently received a Non Delivery Receipt
			– The Contact does not have a Home Branch");

		string TargetListLabel = Res.GetString("7673a230-c184-4a85-bfec-36d17980de25", @"{0:G} contact(s) were successfully added.

			The following contacts could not be added to the target list.. Possible reasons include:
			- Contact does not have an Email address
			- Contact has recently received a Non Delivery Receipt
			- Email address is malformed");

		string CampaignsNotSentPostDeliveryLabel = Res.GetString("75e295b1-3527-4455-b96f-d7f80ed0d8f0", @"The following contact(s) have received a Non-Delivery Receipt.

			- To update a contact's Email address, simply edit the email and save your changes
			- Should you wish to deactivate the contacts, select the contact and press the 'Deactivate Contact(s)' button
			- Should you wish to review the failed contacts at a later stage, press the 'Delivery Failure Report' to get a list of the report");

		string ScheduleListLabel = Res.GetString("5023a3b4-5b69-497f-938b-98c8ce83cacf", @"{0:G} contact(s) were successfully added to the scheduling list.

			The following contacts could not be added to the scheduling list.. Possible reasons include:
			- Contact does not have an Email address
			- Contact has recently received a Non Delivery Receipt
			- Email address is malformed");
		#pragma warning restore IDE0044 // Restoring IDE0044

		#endregion

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			ContactsGrid.ListManager.PositionChanged += ListManager_PositionChanged;

			DisableNewAction();
			if (!DesignModeFinder.IsDesigning)
			{
				ZFormPostingButtonsStrategy.SetupPosting(this, postingButtonsUserControl);
			}

			ListManager_PositionChanged(null, e);
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		public override string FormCaption
		{
			get { return Res.GetString("12a3336d-7bc8-42b2-a76e-2480503b0d5b", "Update Contact(s)"); }
		}

		protected override void ZForm_Closing(object sender, CancelEventArgs e)
		{
			//	Empty to prevent a second popup from appearing
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			Hide();
			if (ContactsToBeScheduled != null)
			{
				SendScheduleCampaign();
			}
			base.OnClosing(e);
		}

		public new ContactsWithNonDeliveryReportsUpdater BusinessEntity
		{
			get { return (ContactsWithNonDeliveryReportsUpdater)base.BusinessEntity; }
		}

		static string CorrectAllErrorsMessage
		{
			get { return Res.GetString("740702c8-5a7f-40c5-aaa7-d227a234a119", "All errors must be corrected before saving the form."); }
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			if (BusinessEntity.HasErrors)
			{
				Globals.Message.Show(CorrectAllErrorsMessage, FormCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return CargoWise.EntityFramework.ContinueWithSave.No;
			}

			if (Globals.Message.Show(BusinessEntity.ConfirmSaveMessage, FormCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
			{
				BusinessEntity.UpdateContacts();
				if (!BusinessEntity.Campaign.IsTargetList)
				{
					if (ContactsToBeScheduled != null)
					{
						AddContactsToSchedulingQueue();
						Close();
						return ContinueWithSave.No;
					}

					if (IsFirstSending || changedSenders.Count > 0)
					{
						ResendCampaign(null);
					}
					else
					{
						ResendCampaign(GetCampaignItemsFromContact());
					}
				}

				return ContinueWithSave.Yes;
			}

			return ContinueWithSave.No;
		}

		GlbCompanyCampaignItem[] GetCampaignItemsFromContact()
		{
			List<GlbCompanyCampaignItem> campaignItems = new List<GlbCompanyCampaignItem>();
			foreach (var campaignContact in BusinessEntity.ContactsCollection.Where(contact => contact.IsEditing && !contact.IsDeactivating))
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(GlbCompanyCampaignItem));
				query.AddToFilter(GlbCompanyCampaignItemSchema.G8_RecipientID, campaignContact.PK);
				query.AddToFilter(JoinCondition.And, GlbCompanyCampaignItemSchema.G8_G0, BusinessEntity.Campaign.PK);
				GlbCompanyCampaignItem item = BusinessEntity.Factory.LoadTop1<GlbCompanyCampaignItem>(query);
				if (item != null)
				{
					campaignItems.Add(item);
				}
			}
			return campaignItems.ToArray();
		}

		#region Resend Campaign

		GlbCompanyCampaignSender campaignSender;

		protected void ResendCampaign(IEnumerable<GlbCompanyCampaignItem> campaignItemToResendOverride)
		{
			IEnumerable<GlbCompanyCampaignItem> campaignItems = null;
			var campaign = BusinessEntity.Campaign;

			List<CampaignContact> contactList = null;
			if (campaignItemToResendOverride == null)
			{
				contactList = BusinessEntity.ContactsCollection.Cast<CampaignContact>().Where(contact => !contact.IsDeactivating).ToList();
				if (!contactList.Any())
				{
					return;
				}

				campaignSender = new GlbCompanyCampaignSender(campaign, contactList);
			}
			else
			{
				contactList = BusinessEntity.ContactsCollection.Where(contact => contact.IsEditing && !contact.IsDeactivating).ToList();
				if (!contactList.Any())
				{
					return;
				}

				campaignItems = campaignItemToResendOverride.Where(i => contactList.Any(c => c.PK == i.G8_RecipientID));
				if (!campaignItems.Any())
				{
					return;
				}

				campaignSender = new GlbCompanyCampaignSender(campaign, campaignItems);
			}

			campaignSender.ContactsNotSentCampaign += GlbCompanyCampaignForm_ContactsNotSentCampaign;
			campaignSender.ShouldContinueWithSending += GlbCompanyCampaignForm_ShouldContinueWithSending;
			campaignSender.CampaignSuccessfullySent += GlbCompanyCampaignForm_CampaignSuccessfullySent;

			campaignSender.ItemSent += BusinessEntity_ItemSent;
			campaignSender.CampaignSendBegin += BusinessEntity_CampaignSendBegin;
			campaignSender.CampaignSendEnd += BusinessEntity_CampaignSendEnd;

			try
			{
				using (campaign.SuspendValidationOnNonPersistentProperties())
				{
					campaignSender.CheckAndSendCampaigns();

					if (!campaign.IsTransferring && campaign.IsTouchCampaign)
					{
						if (campaignItems == null)
						{
							campaignItems =
								campaign.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>()
									.Where(i => contactList.Any(c => c.PK == i.G8_RecipientID));
						}

						if (!campaignItems.Any())
						{
							return;
						}

						campaign.TransitionAndSchedule(campaignItems.Select(c => c.PK));
					}
				}
			}
			finally
			{
				campaignSender.ContactsNotSentCampaign -= GlbCompanyCampaignForm_ContactsNotSentCampaign;
				campaignSender.ShouldContinueWithSending -= GlbCompanyCampaignForm_ShouldContinueWithSending;
				campaignSender.CampaignSuccessfullySent -= GlbCompanyCampaignForm_CampaignSuccessfullySent;

				campaignSender.CampaignSendBegin -= BusinessEntity_CampaignSendBegin;
				campaignSender.CampaignSendEnd -= BusinessEntity_CampaignSendEnd;
				campaignSender.ItemSent -= BusinessEntity_ItemSent;

				if (campaignSender.SendErrors.Any())
				{
					Globals.Message.ShowWarning(string.Join("\r\n", campaignSender.SendErrors));
				}

				changedSenders.Clear();
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		protected void SendScheduleCampaign()
		{
			if (ContactsToBeScheduled.Any())
			{
				var newFactory = new BusinessObjectFactory();
				var campaignInNewFactory = newFactory.ImportFromAnotherFactorySafe(BusinessEntity.Campaign);
				ZFormModaliser.ShowDialogAndDispose(new CampaignItemScheduleForm(campaignInNewFactory, ContactsToBeScheduled), Owner);
			}
		}

		bool ShouldContinueContactsScheduling(List<CampaignContact> contactList)
		{
			return DialogResult.Yes == Globals.Message.Show(Res.GetString("e7491131-5725-4ce4-aca4-86aa83daaf31", "Do you want to continue scheduling the campaign for {0} edited contact(s)?", contactList.Count), Res.GetString("8ec4e142-d4c1-4251-a4ff-8d897076d482", "Schedule Campaigns"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
		}

		void AddContactsToSchedulingQueue()
		{
			var contactList = BusinessEntity.ContactsCollection.Cast<CampaignContact>().Where(contact => !contact.IsDeactivating && !contact.VCC_Email.IsEmpty && !contact.IsNDR && EmailAddressValidation.IsEmailAddressValid(contact.VCC_Email)).ToList();
			if (contactList.Any())
			{
				if (ShouldContinueContactsScheduling(contactList))
				{
					var existingContacts = new HashSet<IScheduleItemsProvider>(ContactsToBeScheduled);

					foreach (var contact in contactList.Where(x => !existingContacts.Contains(x)).Distinct())
					{
						ContactsToBeScheduled.Add(contact);
					}
				}
			}
		}

		public void GlbCompanyCampaignForm_CampaignSuccessfullySent(int contactsDeliveredCount)
		{
			BusinessEntity.ContactsCollection.Load(ZQuery.NoResultQuery);

			NumCampaignsSentLabel.Text = GetInformationText(contactsDeliveredCount);
			ContactsGrid.SetDataBinding(BusinessEntity.ContactsCollection, "");
		}

		#region Campaign Sending Messages

		public void GlbCompanyCampaignForm_ContactsNotSentCampaign(int numContactsSent, ReadOnlyCollection<CampaignContact> contactsNotSentTo)
		{
			BusinessEntity.ContactsCollection.Load(ZQuery.NoResultQuery);
			BusinessEntity.ContactsCollection.AddContacts(contactsNotSentTo);

			NumCampaignsSentLabel.Text = GetInformationText(numContactsSent);
			ContactsGrid.SetDataBinding(BusinessEntity.ContactsCollection, "");
		}

		public bool GlbCompanyCampaignForm_ShouldContinueWithSending(int numCampaignsToSend, GlbCompanyCampaignItem[] campaignsToResend)
		{
			if (BusinessEntity.Campaign.IsMasterCampaign)
			{
				return DialogResult.Yes == Globals.Message.Show(Res.GetString("B59C6577-55E2-4F40-99C1-4A1DD558FF56", "Do you want to add the edited contacts to the Campaign list?"), Res.GetString("3D87FEE8-B24D-4EC6-8185-111564305333", "Campaign List"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			}

			return DialogResult.Yes == Globals.Message.Show(Res.GetString("ccd92f6b-7c1c-4414-a3dc-299f7470a422", "Do you want to resend the campaign to the following edited contacts?"), Res.GetString("6318a49f-a610-488a-9011-62224dcc50c1", "Send Campaigns"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
		}

		#endregion

		[SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void BusinessEntity_CampaignSendBegin(object sender, EventArgs e)
		{
			SendProgressForm = new ProgressForm();
			SendProgressForm.Status = Res.GetString("be84000f-4a59-4b55-bf43-97b665a15388", "Sending campaigns to selected...");
			SendProgressForm.ShowCancelButton = true;
			SendProgressForm.ShowProgressBar = true;
			SendProgressForm.Cancelled += new EventHandler(SendProgressForm_Cancelled);
#if DEBUG
			LastSendProgressForm = SendProgressForm;
#endif
			SendProgressForm.ShowModalTo(FindForm());
			Application.DoEvents();
		}

#if DEBUG
		internal ProgressForm LastSendProgressForm;
#endif

		void SendProgressForm_Cancelled(object sender, EventArgs e)
		{
			campaignSender.CancelSendingContacts();
			SendProgressForm.Close();
			SendProgressForm.Dispose();
		}

		void BusinessEntity_CampaignSendEnd(object sender, EventArgs e)
		{
			SendProgressForm.Close();
			SendProgressForm.Dispose();
		}

		void BusinessEntity_ItemSent(object sender, GlbCompanyCampaignSender.ItemSentEventArgs e)
		{
			if (SendProgressForm != null)
			{
				SendProgressForm.Status = Res.GetString("de5f2752-7f22-4d22-a25d-a3db061629a4", "Sending campaigns to contacts ({0} of {1}).", e.Sent, e.Total);
				SendProgressForm.PercentComplete = (int)((e.Sent / (decimal)e.Total) * 100m);
			}
		}

		ProgressForm SendProgressForm;

		#endregion

		void HookEvents()
		{
			ContactsGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("UpdateContactsForm|f74ac9dd-35d2-41d5-8012-5afac40f6304", "Contact Details"), ContactsGrid_DoubleClick));
			ContactsGrid.DoubleClick += new EventHandler(ContactsGrid_DoubleClick);
			NDRContactReportButton.Click += new EventHandler(NDRContactReportButton_Click);
			DeactivateButton.Click += new EventHandler(DeactivateButton_Click);
		}

		#region Events

		void ListManager_PositionChanged(object sender, EventArgs e)
		{
			if (CurrentCampaignItem != null)
			{
				BusinessEntity.BounceBackEmail = CurrentCampaignItem.BounceBackEmail;
			}
		}

		void NDRContactReportButton_Click(object sender, EventArgs e)
		{
			var column1 = Enterprise.ZArchitecture.Excel.ExcelExportColumn.New(new SchemaStringColumn(OrgContactSchema.Instance, CampaignContact.Schema.ContactUrl, 0, System.Data.SqlDbType.VarChar, ZString.Empty, true, 100));
			column1.Description = Res.GetString("0f152e6c-4f54-4708-9811-aab0459ee04f", "Contact Name");
			column1.Width = 100 * 48 * 2;
			var column2 = Enterprise.ZArchitecture.Excel.ExcelExportColumn.New(new SchemaStringColumn(OrgContactSchema.Instance, CampaignContact.Schema.VCC_OrgFullName, 0, System.Data.SqlDbType.VarChar, ZString.Empty, true, 100));
			column2.Description = Res.GetString("3cbf5ea4-82b2-4af1-812d-368bef2a290f", "Client Name");
			column2.Width = 100 * 48 * 2;
			var column3 = Enterprise.ZArchitecture.Excel.ExcelExportColumn.New(new SchemaBoolColumn(OrgContactSchema.Instance, CampaignContact.Schema.IsNDR, 0, ZBool.False, false, false));
			column3.Description = Res.GetString("9ac7de62-d6ed-4b91-90db-ede13fbe5130", "NDR");
			column3.Width = 10 * 48 * 3;
			var column4 = Enterprise.ZArchitecture.Excel.ExcelExportColumn.New(new SchemaStringColumn(OrgContactSchema.Instance, CampaignContact.Schema.VCC_Email, 0, System.Data.SqlDbType.VarChar, ZString.Empty, true, 100));
			column4.Description = Res.GetString("e54b2edb-31be-47f4-a493-828331369c0b", "Email Address");
			column4.Width = 100 * 48 * 2;

			ZArchitecture.Excel.ExcelExporter exporter = new ZArchitecture.Excel.ExcelExporter(BusinessEntity.ContactsCollection, new List<ZArchitecture.Excel.ExcelExportColumnBase>() {
				column1,column2, column3, column4
			}, new ExcelExporterGuiNotifications(FindForm()));

			exporter.ExportIntoAndOpenExcel();
		}

		void ContactsGrid_DoubleClick(object sender, EventArgs e)
		{
			if (ContactsGrid.ListManager.GetCurrent() != null)
			{
				var campaignContact = (CampaignContact)ContactsGrid.ListManager.GetCurrent();
				if (campaignContact.VCC_TableCode == OrgContactSchema.Constants.Prefix)
				{
					ContactController.ShowEditForm(BusinessEntity.Factory.Load<OrgContact>(campaignContact.PK));
				}
				else if (campaignContact.VCC_TableCode == HRJobApplicantSchema.Constants.Prefix)
				{
					ApplicantController.ShowEditForm((BusinessObject)BusinessEntity.Factory.Load<IHRJobApplicant>(campaignContact.PK));
				}
				else if (campaignContact.VCC_TableCode == GlbStaffSchema.Constants.Prefix)
				{
					StaffController.ShowEditForm(BusinessEntity.Factory.Load<GlbStaff>(campaignContact.PK));
				}
				else
				{
					InquiryController.ShowEditForm(BusinessEntity.Factory.Load<SalesEnquiry>(campaignContact.PK));
				}
			}
		}

		void DeactivateButton_Click(object sender, EventArgs e)
		{
			if (!ContactsGrid.SelectedElements.Any())
			{
				Globals.Message.Show(Res.GetString("3610ec1e-d587-4210-a1a6-7915c55e944b", "Please select contact(s) to deactivate."), Res.GetString("fd658e77-7fb1-4865-9135-a5eae5534079", "Selection Required"),
					MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
			else
			{
				foreach (CampaignContact contact in ContactsGrid.SelectedElements.Cast<CampaignContact>())
				{
					contact.IsDeactivating = !contact.IsDeactivating;
				}
			}
		}

		#endregion

		GlbCompanyCampaignItem CurrentCampaignItem
		{
			get
			{
				GlbCompanyCampaignItem campaignItem = null;
				if (ContactsGrid.ListManager.Count > 0)
				{
					CampaignContact campaignContact = (CampaignContact)ContactsGrid.ListManager.GetCurrent();
					campaignItem = BusinessEntity.SelectedCampaignItem(campaignContact);
				}
				return campaignItem;
			}
		}

		void SendersGrid_DoubleClick(object sender, EventArgs e)
		{
			if (SendersGrid.SelectedElements != null && SendersGrid.SelectedElements.Length == 1)
			{
				var form = StaffController.ShowEditForm(SendersGrid.SelectedElements[0]);
				if (form != null)
				{
					form.BusinessEntityForPersistingForm.HasChangesChanged += OnHasChangesChanged;
					form.Closed += delegate
					{
						form.BusinessEntityForPersistingForm.HasChangesChanged -= OnHasChangesChanged;
					};
				}
			}
		}

		readonly List<GlbStaff> changedSenders = new List<GlbStaff>();

		void OnHasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			if (e.ObjectJustWasChanged && BusinessEntity != null)
			{
				BusinessEntity.HasChanges = true;
			}

			var staff = sender as GlbStaff;
			if (staff != null)
			{
				staff.HasChangesChanged -= OnHasChangesChanged;
				if (!changedSenders.Contains(staff))
				{
					changedSenders.Add(staff);
				}
			}
		}

		#region Controllers

		protected ZController StaffController
		{
			get
			{
				if (staffController == null)
				{
					staffController = ZControllerFactory.Create(ControllerIDs.GlbStaff);
				}
				return staffController;
			}
		}
		ZController staffController;

		protected ZController ApplicantController
		{
			get
			{
				if (applicantController == null)
				{
					applicantController = ZControllerFactory.Create(ControllerIDs.HRJobApplicant);
				}
				return applicantController;
			}
		}
		ZController applicantController;

		protected ZController ContactController
		{
			get
			{
				if (contactController == null)
				{
					contactController = ZControllerFactory.Create(ControllerIDs.OrgContacts);
				}
				return contactController;
			}
		}
		ZController contactController;

		protected ZController InquiryController
		{
			get
			{
				if (inquiryController == null)
				{
					inquiryController = ZControllerFactory.Create(ControllerIDs.SalesEnquiry);
				}
				return inquiryController;
			}
		}
		ZController inquiryController;

		#endregion
	}
}
