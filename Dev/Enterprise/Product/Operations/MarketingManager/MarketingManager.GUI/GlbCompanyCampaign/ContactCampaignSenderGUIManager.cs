using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public class ContactCampaignSenderGUIManager : MasterFiles.GUI.IContactCampaignSenderGUIManager
	{
		#region IContactCampaignSenderGUIManager Members

		public void SendCampaign(IOrgContact contact)
		{
			SendCampaignForContactBizO bizO = new SendCampaignForContactBizO((OrgContact)contact);
			ZFormModaliser.ShowDialogAndDispose(new SendCampaignForContactForm(bizO));
		}

		public void ResendCampaign(IGlbCompanyCampaignItem sentCampaign)
		{
			if (!((GlbCompanyCampaign)sentCampaign.Campaign).IsTargetList)
			{
				GlbCompanyCampaignSender sender = new GlbCompanyCampaignSender((GlbCompanyCampaign)sentCampaign.Campaign, new[] { (GlbCompanyCampaignItem)sentCampaign });
				using (CampaignTrackingControl control = new CampaignTrackingControl())
				{
					sender.MessageOnCampaignSending += new GlbCompanyCampaignSender.CampaignSendingMessageEventHandler(control.CampaignTrackingControl_MessageOnCampaignSending);
					sender.ShouldContinueWithSending += new GlbCompanyCampaignSender.CheckContinueWithSendingHandler(control.CampaignTrackingControl_ShouldContinueWithResending);
					sender.CheckAndSendCampaigns();
					sender.MessageOnCampaignSending -= new GlbCompanyCampaignSender.CampaignSendingMessageEventHandler(control.CampaignTrackingControl_MessageOnCampaignSending);
					sender.ShouldContinueWithSending -= new GlbCompanyCampaignSender.CheckContinueWithSendingHandler(control.CampaignTrackingControl_ShouldContinueWithResending);
				}
			}
		}

		#endregion
	}
}
