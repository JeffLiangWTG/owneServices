using CargoWise.Integration;
using Enterprise.Integration;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Recruiter.Testing.AutomatedRejection
{
	sealed class MockAutomatedEmailRejectionHandler : IAutomatedEmailRejectionHandler
	{
		public bool QueueRejectionEmailWasCalled { get; private set; }
		public bool CancelRejectionEmailWasCalled { get; private set; }
		public bool SendRejectionEmailWasCalled { get; private set; }

		public void QueueRejectionEmail(HRJobApplication application)
		{
			QueueRejectionEmailWasCalled = true;
		}

		public void CancelRejectionEmail(HRJobApplication application)
		{
			CancelRejectionEmailWasCalled = true;
		}

		public StmALog SendRejectionEmail(HRJobApplication application, ITransactionParticipant factory, ILogger logger)
		{
			SendRejectionEmailWasCalled = true;
			return application.SendRejectionEmail(factory, logger);
		}

		public void ResetEmailCall()
		{
			QueueRejectionEmailWasCalled = false;
			CancelRejectionEmailWasCalled = false;
			SendRejectionEmailWasCalled = false;
		}
	}
}
