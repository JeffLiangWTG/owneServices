using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Recruiter.Business
{
	public interface IRejectionEmailHelpers
	{
		EmailDef BuildRejectionEmail(HRJobApplication application, ILogger logger);
	}
}
