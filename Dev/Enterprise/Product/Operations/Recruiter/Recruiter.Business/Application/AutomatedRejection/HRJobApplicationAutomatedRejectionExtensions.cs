using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Recruitment.Registry;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Recruiter.Business
{
	public static class HRJobApplicationAutomatedRejectionExtensions
	{
		static int DaysToQueue
			=> RecruitmentDataRegistry.Instance.AutomatedRejection_DaysToDelaySendingEmail.Value;

		public static bool AnyPendingRejectionLogs(this HRJobApplication application)
			=> application
				.Logs
				.Find(l => l.SL_Parent == application.PK)
				.Where(l => l.Event.SE_Code == AutoEvents.RejectionEmailQueued.Code)
				.Any(l => !l.IsCancelled);

		public static bool AnySentRejectionLogs(this HRJobApplication application)
			=> application
				.Logs
				.Find(l => l.SL_Parent == application.PK)
				.Where(l =>
					l.Event.SE_Code == AutoEvents.RecruitmentCandidateEvent.Code
					&& l.Parameters.TryGetValue((NoResString)"EVT", out var val)
					&& val == nameof(HRJobApplicationEvent.RejectionEmailSent))
				.Any(l => !l.IsCancelled);

		public static void CancelRejectionEmail(this HRJobApplication application)
			=> application
				.Logs
				.Find(l => l.SL_Parent == application.PK)
				.Where(l => l.Event.SE_Code == AutoEvents.RejectionEmailQueued.Code)
				.ForEach(l => l.Cancel());

		public static StmALog QueueRejectionEmail(this HRJobApplication application)
		{
			return application.Logs.AddNew(AutoEvents.RejectionEmailQueued, "Rejection Email Queued", ZDateTimeOffset.UtcNow.AddDays(DaysToQueue));
		}

		public static StmALog SendRejectionEmail(this HRJobApplication application, ITransactionParticipant factory, ILogger logger)
		{
			if (!application.AnySentRejectionLogs())
			{
				var email = RejectionEmailHelpers.BuildRejectionEmail(application, logger);
				if (email != null)
				{
					try
					{
						Env.OutgoingMailManager.Create(factory, email);
						var rejectionLog = application.LogRCEEvent(
							HRJobApplicationEvent.RejectionEmailSent,
							Res.GetString("57875646-5179-4288-8857-d2bf8b4656f", "Rejection Email Sent")
						);
						return rejectionLog;
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						return null;
					}
				}
				return null;
			}
			return null;
		}

		static IRejectionEmailHelpers RejectionEmailHelpers
			=> ObjectFactory.Get<IRejectionEmailHelpers>();
	}
}
