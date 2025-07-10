using CargoWise.Integration;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Recruiter.Business
{
	public class AutomatedEmailRejectionHandler : IAutomatedEmailRejectionHandler
	{
		public void QueueRejectionEmail(HRJobApplication application)
		{
			if (!application.AnyPendingRejectionLogs() && ShowDialog() == ZDialogResult.Yes)
			{
				_ = application.QueueRejectionEmail();
			}
		}

		public void CancelRejectionEmail(HRJobApplication application)
		{
			application.CancelRejectionEmail();
		}

		public StmALog SendRejectionEmail(HRJobApplication application, ITransactionParticipant factory, ILogger logger)
		{
			return application.SendRejectionEmail(factory, logger);
		}

		static ZDialogResult ShowDialog()
		{
			var message = ResString.GetMultilingualString("33F987D4-9376-492D-A7F3-6C362185888E", @"Would you like to send an automated rejection email? Yes/no");

#pragma warning disable CW1113 // Do Not Show Message Box From Business Layer
			return Globals.Message.Show(
				message,
				ResString.GetMultilingualString("297DA4C5-63F8-4AB1-B3A3-887D63F1FF1A", "Confirm Rejection Email"),
				ZMessageBoxButtons.YesNo,
				ZMessageBoxIcon.Information);
#pragma warning restore CW1113 // Do Not Show Message Box From Business Layer
		}
	}
}
