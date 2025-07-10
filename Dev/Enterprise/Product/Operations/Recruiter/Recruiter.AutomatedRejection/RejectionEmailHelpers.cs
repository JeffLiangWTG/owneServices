using Enterprise.Integration;
using Enterprise.Recruiter.Business;
using Enterprise.Recruitment.Registry;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Recruiter.AutomatedRejection
{
	public class RejectionEmailHelpers : IRejectionEmailHelpers
	{
		public EmailDef BuildRejectionEmail(HRJobApplication application, ILogger logger)
		{
			if (string.IsNullOrWhiteSpace(RecruitmentDataRegistry.Instance.SenderAddress.Value))
			{
				logger?.Log(LogType.Error, (NoResString)"'Recruitment > Mail > Outgoing > Sender Address' must be set in the registry.");
				return null;
			}
			var generator = new RejectionEmailGenerator(new DocRecruitmentAutoRejectionEmailParser(application.Factory));

			return generator.BuildEmail(application.Applicant.Email, RecruitmentDataRegistry.Instance.AutomatedRejection_EmailTemplate.Value, generator.CreateLookup(application));
		}
	}
}
