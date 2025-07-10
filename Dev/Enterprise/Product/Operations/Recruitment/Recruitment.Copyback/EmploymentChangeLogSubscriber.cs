using System;
using System.Linq;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruitment.Copyback
{
	[Serializable]
	public class EmploymentChangeLogSubscriber : LogSubscriber
	{
		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			var factory = queuedLogs.First().Factory;
			var handler = new CopybackHandler();
			foreach (var log in queuedLogs)
			{
				handler.Copyback(factory, log);
			}
		}

		public override string Name => nameof(EmploymentChangeLogSubscriber);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "For log purposes")]
		public override string FriendlyName => "Employment Change Log Subscriber";

		public override string[] TableNames => new string[] { GlbStaffSchema.Constants.TableName };

		public override string[] EventTypes => new string[] { AutoEvents.EmploymentChangeLive.Code };
	}
}
