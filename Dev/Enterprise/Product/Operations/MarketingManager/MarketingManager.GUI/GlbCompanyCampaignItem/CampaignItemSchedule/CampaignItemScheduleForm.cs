using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public partial class CampaignItemScheduleForm : ZTemplateForm
	{
		public CampaignItemScheduleForm(GlbCompanyCampaign campaign, IEnumerable<IScheduleItemsProvider> selectedItems)
			: base(campaign)
		{
			if (!campaign.CampaignItemSchedule.SelectedScheduleItems.Any())
			{
				campaign.CampaignItemSchedule.SelectedScheduleItems.UnionWith(selectedItems);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			DisableNewAction();

			if (!DataSource.CampaignItemSchedule.ScheduleItemsCollection.Any() || !HasAQueuedScheduleCampaignItem())
			{
				DataSource.SetReadOnlyIncludingChildren(true);
			}
			else
			{
				var scheduler = DataSource.CampaignItemSchedule;
				scheduler.SendScheduleEventHandler += CampaignItemSchedule_SendScheduleEventHandler;
				if (scheduler.SelectedScheduleItems.Any() && scheduler.SelectedScheduleItems.FirstOrDefault().TableCode == ViewCampaignContactSchema.Constants.Prefix)
				{
					scheduler.ScheduleSendTimeLocal = ZDateTime.Now.AddDays(1);
				}
			}
		}

		bool HasAQueuedScheduleCampaignItem()
		{
			foreach (ScheduleCampaignItems schedule in DataSource.CampaignItemSchedule.ScheduleItemsCollection)
			{
				if (schedule.Status == TrackingStatusCodes.Codes.QUE)
				{
					return true;
				}
			}
			return false;
		}

		GlbCompanyCampaignSender CampaignSender;

#if DEBUG
		internal
#endif
		void CampaignItemSchedule_SendScheduleEventHandler(object sender, GlbCompanyCampaignItemSchedule.SendScheduleEventArgs e)
		{
			CampaignSender = new GlbCompanyCampaignSender(DataSource, e.Contacts, true);
			CampaignSender.MessageOnCampaignSending += new GlbCompanyCampaignSender.CampaignSendingMessageEventHandler(campaignSender_MessageOnCampaignSending);
			CampaignSender.ContactsNotSentCampaign += new GlbCompanyCampaignSender.ContactsNotSentCampaignEventHandler(GlbCompanyCampaignForm_ContactsNotSentCampaign);
			CampaignSender.ShouldContinueWithSending += new GlbCompanyCampaignSender.CheckContinueWithSendingHandler(campaignSender_ShouldContinueWithSending);

			CampaignSender.ItemSent += new GlbCompanyCampaignSender.ItemSentEventHandler(BusinessEntity_ItemSent);
			CampaignSender.CampaignSendBegin += new EventHandler(BusinessEntity_CampaignSendBegin);
			CampaignSender.CampaignSendEnd += new EventHandler(BusinessEntity_CampaignSendEnd);

			try
			{
				e.Result = CampaignSender.CheckAndSendCampaigns(true);
			}
			finally
			{
				CampaignSender.MessageOnCampaignSending -= new GlbCompanyCampaignSender.CampaignSendingMessageEventHandler(campaignSender_MessageOnCampaignSending);
				CampaignSender.ContactsNotSentCampaign -= new GlbCompanyCampaignSender.ContactsNotSentCampaignEventHandler(GlbCompanyCampaignForm_ContactsNotSentCampaign);
				CampaignSender.ShouldContinueWithSending -= new GlbCompanyCampaignSender.CheckContinueWithSendingHandler(campaignSender_ShouldContinueWithSending);

				CampaignSender.CampaignSendBegin -= new EventHandler(BusinessEntity_CampaignSendBegin);
				CampaignSender.CampaignSendEnd -= new EventHandler(BusinessEntity_CampaignSendEnd);
				CampaignSender.ItemSent -= new GlbCompanyCampaignSender.ItemSentEventHandler(BusinessEntity_ItemSent);
			}
		}

		ProgressForm SendProgressForm;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void BusinessEntity_CampaignSendBegin(object sender, EventArgs e)
		{
			SendProgressForm = new ProgressForm();
			SendProgressForm.Status = Res.GetString("c08156e6-3c8d-462f-b200-e5afbdf3ae03", "Sending campaigns to selected...");
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
			CampaignSender.CancelSendingContacts();
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
				SendProgressForm.Status = Res.GetString("9fa4586b-fc99-4c8e-9c62-3aa22a3b97dd", "Scheduling campaigns to contacts ({0} of {1}).", e.Sent, e.Total);
				SendProgressForm.PercentComplete = (int)((e.Sent / (decimal)e.Total) * 100m);
			}
		}

		void GlbCompanyCampaignForm_ContactsNotSentCampaign(int numContactsSent, System.Collections.ObjectModel.ReadOnlyCollection<CampaignContact> contactsNotSentTo)
		{
			var newFactory = new BusinessObjectFactory();
			var campaignInNewFactory = newFactory.ImportFromAnotherFactorySafe(DataSource);
			if (campaignInNewFactory != null)
			{
				ZFormModaliser.ShowDialogAndDispose(new UpdateContactsForm(numContactsSent, contactsNotSentTo, new ContactsWithNonDeliveryReportsUpdater(newFactory, campaignInNewFactory)));
			}
		}

		bool campaignSender_ShouldContinueWithSending(int numCampaignsToSend, GlbCompanyCampaignItem[] campaignToResend)
		{
			return DialogResult.Yes == Globals.Message.Show(ShouldContinueMessage(numCampaignsToSend), Res.GetString("9a934736-2427-4bfe-b214-59922ff1ad72", "Send Campaigns"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
		}

		string ShouldContinueMessage(int numCampaignsToSend)
		{
			return Res.GetString("798367ef-d475-4df5-ac3e-c0f08cf7e0b6", "You are about to schedule delivery to {0} contacts. Would you like to continue?", numCampaignsToSend);
		}

		void campaignSender_MessageOnCampaignSending(object sender, GlbCompanyCampaignSender.MessageOnCampaignSendingEventArgs e)
		{
			if (e.IsError)
			{
				if (DataSource != null && DataSource.HasErrors)
				{
					ShowErrorsDialog();
				}
				else
				{
					Globals.Message.ShowError(e.Message, e.Summary);
				}
			}
			else
			{
				Globals.Message.ShowInformation(e.Message, e.Summary);
			}
		}

		new public GlbCompanyCampaign DataSource
		{
			get { return (GlbCompanyCampaign)base.DataSource; }
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			DataSource.CampaignItemSchedule.RunPreSaveValidation();

			if (DataSource.CampaignItemSchedule.ScheduleItemsCollection.HasErrors())
			{
				if (ShouldAdjustUTCTimeToOneDayAhead())
				{
					DataSource.CampaignItemSchedule.ScheduleSendTimeLocal = ZDateTime.UtcNow.AddDays(1);
					DataSource.CampaignItemSchedule.RunPreSaveValidation();
				}
				else
				{
					Globals.Message.Show(CorrectAllErrorsMessage, FormCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
					return CargoWise.EntityFramework.ContinueWithSave.No;
				}
			}

			if (DataSource.CampaignItemSchedule.SaveRelatedBusinessObjects())
			{
				return base.ValidateAndSave();
			}
			return CargoWise.EntityFramework.ContinueWithSave.No;
		}

		static string CorrectAllErrorsMessage
		{
			get { return Res.GetString("0f3dff6b-9e53-4fa2-99e2-b2f75bdd1f74", "All errors must be corrected before saving the form."); }
		}

		bool ShouldAdjustUTCTimeToOneDayAhead()
		{
			return DialogResult.Yes == Globals.Message.Show(AutoTimeAdjustmentMessage, Res.GetString("26ce7bda-6eb1-40e9-8e8f-9868c48eac7f", "Time Adjustment"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
		}

		string AutoTimeAdjustmentMessage
		{
			get
			{
				return Res.GetString("5fc49aed-c80e-4354-9dac-259931442a15", "Some UTC time(s) are invalid. Do you want to adjust them to a day ahead?");
			}
		}

		#region Form Caption

		public override string FormCaption
		{
			get { return Res.GetString("CampaignItemScheduleForm|Caption", "Send Schedule"); }
		}

		#endregion

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		protected override void OnApplyButtonClick(object sender, EventArgs e)
		{
			base.OnPostButtonClick(sender, e);
		}
	}
}
