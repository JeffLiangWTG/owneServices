using CargoWise.EntityFramework;

namespace Enterprise.Recruiter.Business
{
	public class ExamAttemptCollection : ActiveBusinessObjectCollection<ExamAttempt>
	{
		public ExamAttemptCollection(LearningCentreCampaignItem campaignItem)
			: base(campaignItem)
		{
		}
	}
}
