//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoExamSettingsLookups
//
//    This class should be used for overriding collections in AutoExamSettingsLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Registry.Business;

namespace Enterprise.Recruiter.Business
{
	public class ExamSettingLookups : AutoExamSettingLookups
	{
		public ExamSettingLookups(AutoExamSetting parent) : base(parent)
		{
		}

		public CodeDescriptionBoolCollection VersionList
		{
			get { return RecruiterDataRegistry.Instance.JobSkillTestVersionList.Value; }
		}

		public LearningCentreCampaignCollection TestCampaigns
		{
			get { return new LearningCentreCampaignCollection(Factory); }
		}
	}
}
