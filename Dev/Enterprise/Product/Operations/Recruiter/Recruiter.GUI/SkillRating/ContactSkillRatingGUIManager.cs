using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Recruiter.GUI
{
	public class ContactSkillRatingGUIManager : IContactSkillRatingGUIManager
	{
		public void ShowExamDetails(IGlbCompanyCampaignItem examCampaignItem)
		{
			var learningCentreCampaignItem = examCampaignItem as LearningCentreCampaignItem;
			if (learningCentreCampaignItem != null && learningCentreCampaignItem.CompanyCampaign != null)
			{
				ZFormModaliser.ShowDialogAndDispose(new LearningCentreCampaignForm(learningCentreCampaignItem.CompanyCampaign));
			}
		}

		public void ShowExamHistory(IGlbCompanyCampaignItem examCampaignItem, ZString version)
		{
			throw new System.NotImplementedException();
		}

		public void ResitExam(IGlbCompanyCampaignItem examCampaignItem)
		{
			if (Globals.Message.Show(Res.GetString("18a12aa4-9f88-4886-871e-4899d60f3016", "You are about to allow contact to resit this exam, do you wish to continue?"), Res.GetString("45dc5faa-e17a-48a5-a4b4-0271d05b4ba9", "Confirm Resit Exam"), MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
			{
				var learningCentreCampaignItem = examCampaignItem as LearningCentreCampaignItem;
				if (learningCentreCampaignItem != null)
				{
					var factory = new BusinessObjectFactory();
					var campaignItemToResit = factory.Load<LearningCentreCampaignItem>(learningCentreCampaignItem.PK);
					if (campaignItemToResit.ResetVoteSurveyExam(forceReset: true))
					{
						factory.Save();

						Globals.Message.Show(Res.GetString("3fd499b8-39ab-4022-aa4a-3ea8f08018c5", "Contact is able to resit this exam now."));
					}
					else
					{
						Globals.Message.Show(Res.GetString("dc3ceae3-382b-468c-a0f5-68eb52f32aa8", "Contact has no current exam to resit."));
					}
				}
			}
		}
	}
}
