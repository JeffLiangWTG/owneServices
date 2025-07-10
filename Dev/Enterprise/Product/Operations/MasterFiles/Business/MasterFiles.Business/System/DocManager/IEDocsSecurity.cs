using Enterprise.Security;

namespace Enterprise.MasterFiles.Business
{
	public interface IEDocsSecurity
	{
		SecurityCheckpoint EdocsSecurityCheckpoint { get; }
	}
}
