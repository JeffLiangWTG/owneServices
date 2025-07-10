using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Integration;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.AutomatedRejection
{
	[Serializable]
	public class RejectionEmailLogSubscriber : LogSubscriber
	{
		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			if (queuedLogs == null || queuedLogs.Length == 0)
			{
				return;
			}

			var factory = queuedLogs.First().Factory;
			var applicationPks = queuedLogs.Select(log => log.SJ_ParentID).Distinct().ToList();
			var applications = factory.Load<HRJobApplication>(new ZQuery(HRJobApplicationSchema.PK, applicationPks));
			var handler = ObjectFactory.Get<IAutomatedEmailRejectionHandler>();

			foreach (var app in applications)
			{
				if (app != null && app.AnyPendingRejectionLogs())
				{
					_ = handler.SendRejectionEmail(app, factory, DefaultLogger);
				}
			}
		}

		#region LogSubscriber Overrides

		public override string Name => nameof(RejectionEmailLogSubscriber);

		public override string FriendlyName => (NoResString)"Rejection Email n-Day Log Subscriber";
		public override string[] TableNames => new[] { HRJobApplicationSchema.Constants.TableName };
		public override string[] EventTypes => new[] { AutoEvents.RejectionEmailQueued.Code };

		#endregion
	}
}
