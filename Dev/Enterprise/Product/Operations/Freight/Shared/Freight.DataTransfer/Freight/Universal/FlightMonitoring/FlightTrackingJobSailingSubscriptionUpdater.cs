using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using TransportParentTypes = Enterprise.Core.Constants.TransportParentTypes;

namespace Enterprise.Freight.DataTransfer.Universal.FlightMonitoring
{
	[Serializable]
	internal class FlightTrackingJobSailingSubscriptionUpdater : LogSubscriber
	{
		#region LogSubscriber

		public override string Name => "FlightJobSailingSubscriptionUpdater";

		public override string FriendlyName => (NoResString)"Flight Tracking Job Sailing Subscription Updater";

		public override string[] EventTypes => new[] { AutoEvents.SubscriptionRequested.Code };

		public override string[] TableNames => new[] { BusinessObjectFactory.GetTableNameFromType(typeof(JobSailing), false) };

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			if (FlightTrackingSubscriptionUpdater.IsTrackingEnabled)
			{
				if (FlightTrackingSubscriptionUpdater.IsEHubIDSet)
				{
					var flightTrackingFeedLogs = queuedLogs.Where(FlightTrackingSubscriptionUpdater.IsFlightTrackingFeedRequest).ToList();
					if (flightTrackingFeedLogs.Count > 0)
					{
						//Get the factory from first Log, there is no need to save it, as it will be called automatically
						var factory = flightTrackingFeedLogs[0].Factory;
						var parents = GetRelatedTransportParents(factory, flightTrackingFeedLogs);

						foreach (var parent in parents)
						{
							if (parent is IStmALogParent logParent)
							{
								var mawbNumber = parent.TransportSupporter?.BillOfLading ?? ZString.Empty;
								if (MasterBillValidator.GetMAWBFormatValidMessage(mawbNumber, parent.Factory).IsEmpty)
								{
									var log = GetParentLog(parent, queuedLogs);

									logParent.Logs.CreateRecreateOrUpdateEventLog(
										AutoEvents.SubscriptionRequested,
										log.SJ_IsEstimate ? EstimateActual.Estimate : EstimateActual.Actual,
										log.EventTimeOffset,
										ZString.Empty,
										GetFlightSubscriptionEventParameters(mawbNumber).ToArray());
								}
							}
						}
					}
				}
				else
				{
					DefaultLogger.Log(LogType.Warning, "Subscription requests cannot be created as eHub ID is not set.");
				}
			}
			else
			{
				DefaultLogger.Log(LogType.Information, "Subscription requests cannot be created as Flight Monitoring System is disabled.");
			}
		}

		protected override ILogBatcher GetLogBatcher() => new UserContextSwitchingLogBatcher(GetINotificationsWrapperAroundILogger(), Name);

		#endregion

		#region Internal

		IEnumerable<ITransportParent> GetRelatedTransportParents(BusinessObjectFactory factory, IEnumerable<IQueuedLog> queuedLogs)
		{
			var sailingList = queuedLogs.Select(l => l.SJ_ParentID).ToArray();
			var consolTransportFilter = GetTransportParentFilter(sailingList, TransportParentTypes.Consol);
			var declarationTransportFilter = GetTransportParentFilter(sailingList, TransportParentTypes.Declaration);

			var consolFilter = new ZDBOnlyQuery(typeof(CommonConsol));
			consolFilter.AddSubQuery(consolTransportFilter, JoinCondition.And);
			consolFilter.AddToFilter(JobConsolSchema.JK_TransportMode, Core.Constants.TransportModes.Air);
			consolFilter.AddToFilter(JobConsolSchema.JK_IsForwarding, true);

			var declarationFilter = new ZDBOnlyQuery(ObjectFactory.GetType<IBaseJobDeclaration>());
			declarationFilter.AddSubQuery(declarationTransportFilter, JoinCondition.And);
			declarationFilter.AddToFilter(JobDeclarationSchema.JE_TransportMode, Core.Constants.TransportModes.Air);

			return factory.Load<CommonConsol>(consolFilter)
				.OfType<ITransportParent>()
				.Concat(factory.Load<IBaseJobDeclaration>(declarationFilter)
					.OfType<ITransportParent>());
		}

		ZDBOnlySubQuery GetTransportParentFilter(IEnumerable<ZGuid> sailingPks, string parentType)
		{
			var transportParentFilter = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
			transportParentFilter.AddToFilter(JobConsolTransportSchema.JW_JX, sailingPks);
			transportParentFilter.AddToFilter(JobConsolTransportSchema.JW_TransportMode, Core.Constants.TransportModes.Air);
			transportParentFilter.AddToFilter(JobConsolTransportSchema.JW_ParentType, parentType);
			transportParentFilter.AddToFilter(JobConsolTransportSchema.JW_IsLinked, true);

			return transportParentFilter;
		}

		IQueuedLog GetParentLog(ITransportParent parent, IEnumerable<IQueuedLog> queuedLogs)
		{
			var sailingPKs = parent?
				.Transports
				.OfType<Transport>()
				.Select(x => x.JW_JX);

			return queuedLogs.FirstOrDefault(l => sailingPKs.Contains(l.SJ_ParentID));
		}

		Dictionary<string, string> GetFlightSubscriptionEventParameters(ZString mawbNumber)
		{
			return new Dictionary<string, string>
			{
				[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type] = Core.Constants.EventReferenceParameterTypes.AWBAutomation,
				[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber] = mawbNumber
			};
		}
		#endregion
	}
}
