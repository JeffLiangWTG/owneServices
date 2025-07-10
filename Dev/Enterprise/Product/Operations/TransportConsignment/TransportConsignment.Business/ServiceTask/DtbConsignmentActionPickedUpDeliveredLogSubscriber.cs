using System;
using CargoWise.Application;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportConsignment.Registry;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	[Serializable]
	class DtbConsignmentActionPickedUpDeliveredLogSubscriber : LogSubscriber
	{
		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			if (LandTransportRegistry.Instance.DisableDtbConsignmentActionPickedUpDeliveredLogSubscriber.Value)
			{
				return;
			}

			var dtbConsignmentActionPUPDLVUpdater = ObjectFactory.Get<IDtbConsignmentActionPickedUpDeliveredUpdater>(nameof(IDtbConsignmentActionPickedUpDeliveredUpdater));

			foreach (var log in queuedLogs)
			{
				dtbConsignmentActionPUPDLVUpdater.ProcessLog(log.Factory, log.ParentID);
			}
		}

		public override string Name => "CNActionPUPDLVLogSubscriber";

		public override string[] EventTypes => new[] { Events.PickedUpCode, Events.DeliveredCode };

		public override string[] TableNames => new[] { DtbConsignmentActionSchema.Constants.TableName };
	}
}
