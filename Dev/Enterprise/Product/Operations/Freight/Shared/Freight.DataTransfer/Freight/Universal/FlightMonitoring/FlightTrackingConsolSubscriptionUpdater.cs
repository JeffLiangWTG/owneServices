using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.DataTransfer.Universal.FlightMonitoring
{
	[Serializable]
	internal class FlightTrackingConsolSubscriptionUpdater : FlightTrackingSubscriptionUpdater
	{
		public override string Name => "FlightConsolSubscriptionUpdater";

		public override string FriendlyName => (NoResString)"Flight Tracking Consol Subscription Updater";

		protected override Type GetBusinessObjectType()
		{
			return typeof(CommonConsol);
		}

		protected override IQueuedLog GetParentLog(ITransportParent parent, IEnumerable<IQueuedLog> queuedLogs)
		{
			return queuedLogs.FirstOrDefault(l => l.SJ_ParentID == parent.PK);
		}

		protected override IEnumerable<ITransportParent> GetRelatedTransportParents(BusinessObjectFactory factory, IEnumerable<IQueuedLog> queuedLogs)
		{
			var consolIDList = queuedLogs.Select(l => l.SJ_ParentID);
			var consolFilter = new ZQuery(JobConsolSchema.PK, consolIDList);
			consolFilter.AddToFilter(JobConsolSchema.JK_TransportMode, Core.Constants.TransportModes.Air);
			consolFilter.AddToFilter(JobConsolSchema.JK_IsForwarding, true);

			var consols = factory.Load<CommonConsol>(consolFilter);

			return consols;
		}
	}
}
