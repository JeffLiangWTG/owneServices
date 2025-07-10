using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[Serializable]
	public class RecordAuditedLogSubscriber : LogSubscriber
	{
		public override string Name => nameof(RecordAuditedLogSubscriber);

		public override string FriendlyName => (NoResString)"RecordAudited Log Subscriber";

		public override string[] TableNames => new[] { JobDeclarationSchema.Constants.TableName };

		public override string[] EventTypes => eventTypes ?? (eventTypes = new[] { AutoEvents.RecordAuditedCode });
		string[] eventTypes;

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			var factory = queuedLogs.FirstOrDefault()?.Factory;

			if (factory != null)
			{
				var parentsQuery = new ZQuery(JobDeclarationSchema.PK, queuedLogs.Select(l => l.SJ_ParentID));
				var parentsDictionary = factory.Load<BaseJobDeclaration>(parentsQuery).ToDictionary(l => l.PK, l => l);

				foreach (var log in queuedLogs)
				{
					if (parentsDictionary.TryGetValue(log.SJ_ParentID, out var parent))
					{
						parent.AuditDateUtc = log.EventTimeOffset.ToUtcZDateTime();
						parent.AuditLogUser = log.SJ_GS_NKUser;
						parent.AuditReference = log.Reference;
					}
				}
			}
		}
	}
}
