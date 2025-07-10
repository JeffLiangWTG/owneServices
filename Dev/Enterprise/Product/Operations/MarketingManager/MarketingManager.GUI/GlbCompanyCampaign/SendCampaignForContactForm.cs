using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public partial class SendCampaignForContactForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public SendCampaignForContactForm()
		{
			InitializeComponent();
		}

		public SendCampaignForContactForm(SendCampaignForContactBizO sender)
			: base(sender)
		{
			InitializeComponent();
		}

		public override string FormCaption
		{
			get { return Res.GetString("7a34289c-fa2a-4eeb-bc06-0a8e5be74cb8", "Send Campaign"); }
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		new SendCampaignForContactBizO DataSource
		{
			get { return (SendCampaignForContactBizO)base.DataSource; }
		}

		GlbCompanyCampaignSender campaignSender;

		void SendCampaignButton_Click(object sender, EventArgs e)
		{
			DataSource.Validation.ValidateAll();
			if (!DataSource.HasErrors)
			{
				ZQuery campaignFilter = new ZQuery(GlbCompanyCampaignItemSchema.G8_G0, DataSource.CampaignPK);
				if (DataSource.Contact.Campaigns.Find(campaignFilter).Any())
				{
					Globals.Message.ShowError(GlbCompanyCampaignSender.NotificationConstants.HasBeenSentPreviouslyMessage, GlbCompanyCampaignSender.NotificationConstants.CannotSendCampaignsSummary);
				}
				else
				{
					GlbCampaignContactCollection contactCollection = new GlbCampaignContactCollection(DataSource.Campaign);
					contactCollection.Load(new ZQuery(ViewCampaignContactSchema.PK, DataSource.CampaignContact.PK));
					campaignSender = new GlbCompanyCampaignSender(DataSource.Campaign, contactCollection.Cast<CampaignContact>().ToList());

					campaignSender.ShouldContinueWithSending += new GlbCompanyCampaignSender.CheckContinueWithSendingHandler(delegate
					{ return true; });
					campaignSender.MessageOnCampaignSending += new GlbCompanyCampaignSender.CampaignSendingMessageEventHandler(SendCampaignForContactForm_MessageOnCampaignSending);
					campaignSender.ItemSent += new GlbCompanyCampaignSender.ItemSentEventHandler(BusinessEntity_ItemSent);
					campaignSender.CampaignSendBegin += new EventHandler(BusinessEntity_CampaignSendBegin);
					campaignSender.CampaignSendEnd += new EventHandler(BusinessEntity_CampaignSendEnd);

					bool isSent = campaignSender.CheckAndSendCampaigns();

					campaignSender.ShouldContinueWithSending -= new GlbCompanyCampaignSender.CheckContinueWithSendingHandler(delegate
					{ return true; });
					campaignSender.MessageOnCampaignSending -= new GlbCompanyCampaignSender.CampaignSendingMessageEventHandler(SendCampaignForContactForm_MessageOnCampaignSending);
					campaignSender.ItemSent -= new GlbCompanyCampaignSender.ItemSentEventHandler(BusinessEntity_ItemSent);
					campaignSender.CampaignSendBegin -= new EventHandler(BusinessEntity_CampaignSendBegin);
					campaignSender.CampaignSendEnd -= new EventHandler(BusinessEntity_CampaignSendEnd);

					if (isSent)
					{
						Close();
					}
				}
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#region Event Handlers

		void SendCampaignForContactForm_MessageOnCampaignSending(object sender, GlbCompanyCampaignSender.MessageOnCampaignSendingEventArgs e)
		{
			if (e.IsError)
			{
				if (DataSource.HasErrors)
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void BusinessEntity_CampaignSendBegin(object sender, EventArgs e)
		{
			SendProgressForm = new ProgressForm();
			SendProgressForm.Status = Res.GetString("aaba80a7-c0b2-477b-9ad5-87bcc8ff4f08", "Sending campaigns...");
			SendProgressForm.ShowCancelButton = true;
			SendProgressForm.ShowProgressBar = true;
			SendProgressForm.Cancelled += new EventHandler(SendProgressForm_Cancelled);
			SendProgressForm.ShowModalTo(FindForm());
			Application.DoEvents();
		}

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

		void BusinessEntity_ItemSent(object sender, EventArgs e)
		{
			if (SendProgressForm != null)
			{
				SendProgressForm.Status = Res.GetString("90b9cc5a-18f4-4871-baec-d48bab2fcf59", "Sending campaigns to contact.");
			}
		}

		ProgressForm SendProgressForm;

		#endregion
	}
}
