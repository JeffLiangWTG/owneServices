using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.GUI
{
	public interface IContactCampaignSenderGUIManager
	{
		void SendCampaign(IOrgContact contact);
		void ResendCampaign(IGlbCompanyCampaignItem sentCampaign);
	}
}