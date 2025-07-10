using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.GUI
{
	public interface IContactSkillRatingGUIManager
	{
		void ShowExamDetails(IGlbCompanyCampaignItem examCampaignItem);
		void ShowExamHistory(IGlbCompanyCampaignItem examCampaignItem, ZString version);
		void ResitExam(IGlbCompanyCampaignItem examCampaignItem);
	}
}