using CargoWise.Integration;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Recruiter.Business
{
	public interface IAutomatedEmailRejectionHandler
	{
		void QueueRejectionEmail(HRJobApplication application);

		void CancelRejectionEmail(HRJobApplication application);

		StmALog SendRejectionEmail(HRJobApplication application, ITransactionParticipant factory, ILogger logger);
	}
}
