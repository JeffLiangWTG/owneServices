using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Recruitment.Module.CandidateManagement;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruitment.Module
{
	public class CandidateManagementModule : ZPopupModule
	{
		public CandidateManagementModule()
		{ }

		public override ModuleIdentifier ID => ModuleIDs.RecruitmentCandidateManagement;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Recruiter;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.Recruitment;

		protected override ZPopupController GetNewController()
		{
			return new CandidateManagementPopupFormController();
		}

#if DEBUG
		internal ZPopupController GetNewControllerForTest() => GetNewController();
#endif

		public override bool SupportsConversations { get; } = true;
	}
}
