using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Freight.DataTransfer.Universal.FlightMonitoring
{
	[Serializable]
	internal class FlightTrackingDeclarationSubscriptionUpdater : FlightTrackingSubscriptionUpdater
	{
		public override string Name => "DeclarationSubscriptionUpdater";

		public override string FriendlyName => (NoResString)"Flight Tracking Declaration Subscription Updater";

		protected override Type GetBusinessObjectType()
		{
			return ObjectFactory.GetType<IBaseJobDeclaration>();
		}

		protected override IQueuedLog GetParentLog(ITransportParent transportParent, IEnumerable<IQueuedLog> queuedLogs)
		{
			return queuedLogs.FirstOrDefault(l => l.SJ_ParentID == transportParent.PK);
		}

		protected override IEnumerable<ITransportParent> GetRelatedTransportParents(BusinessObjectFactory factory, IEnumerable<IQueuedLog> queuedLogs)
		{
			var declarationIDList = queuedLogs.Select(l => l.SJ_ParentID);
			var declarationFilter = new ZQuery(JobDeclarationSchema.PK, declarationIDList);
			declarationFilter.AddToFilter(JobDeclarationSchema.JE_TransportMode, Core.Constants.TransportModes.Air);

			return factory.Load<IBaseJobDeclaration>(declarationFilter)?
				.OfType<ITransportParent>()
				.ToList();
		}
	}
}
