using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

#region JobVoyageEventLogSynchronizer

namespace Enterprise.Freight.Forwarding.Business
{
	/// <summary>
	/// Log Subscriber to handle synchronisation of ARV/DEP event logs to transport legs when a voyage updates
	/// </summary>
	[Serializable]
	public class JobVoyageEventLogSynchronizer : LogSubscriber
	{
		public JobVoyageEventLogSynchronizer()
		{
			maximumElementsWithoutAllowTableValuedParameters = ZSQLInFilter.MAXIMUM_ELEMENTS_FOR_PARAMETERISATION;
		}

		public JobVoyageEventLogSynchronizer(int maximumElementsWithoutAllowTableValuedParameters)
		{
			this.maximumElementsWithoutAllowTableValuedParameters = maximumElementsWithoutAllowTableValuedParameters;
		}

		public override string[] EventTypes
		{
			get { return new string[] { Events.EditedARecord.Code }; }
		}

		public override string Name
		{
			get { return "JobVoyageEventLogSynchronizer"; }
		}

		public override string FriendlyName
		{
			get { return (NoResString)"Job Voyage Event Log Synchronizer"; } // Log subscriber names should be in English only
		}

		public override string[] TableNames
		{
			get { return new[] { JobVoyageSchema.Constants.TableName }; }
		}

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			var uniqueVoyageLogs = queuedLogs.Distinct(new QueuedLogUniqueParentComparer()).ToList();
			if (uniqueVoyageLogs.Count == 0)
			{
				return;
			}

			var factory = uniqueVoyageLogs.First().Factory;
			using (factory.SetTempContext(Transport.DebugLogContext.LogIfSetJW_IsLinkedFromTrueToFalse))
			{
				var transports = LoadVoyageTransports(factory, uniqueVoyageLogs).ToList();
				SyncTransportEvents(transports);
			}
		}

		#region Database Loaders

		IEnumerable<Transport> LoadVoyageTransports(BusinessObjectFactory factory, IEnumerable<IQueuedLog> voyageLogs)
		{
			var jobVoyOriginSubQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
			jobVoyOriginSubQuery.AddToFilter(JobVoyOriginSchema.JA_JV, voyageLogs.Select(x => x.SJ_ParentID));
			var jobSailingSubQuery = new ZDBOnlySubQuery(typeof(JobSailing), JobConsolTransportSchema.JW_JX);
			jobSailingSubQuery.AddSubQuery(jobVoyOriginSubQuery, JoinCondition.And);

			var consolTransportQuery = new ZDBOnlyQuery(typeof(Transport));
			consolTransportQuery.AddToFilter(JobConsolTransportSchema.JW_ParentType, new[]
			{
				Constants.TransportParentTypes.Consol,
				Constants.TransportParentTypes.Shipment,
				Constants.TransportParentTypes.AgencyShipment,
				Constants.TransportParentTypes.ShipmentPreAdvice,
				Constants.TransportParentTypes.Declaration
			});

			consolTransportQuery.AddSubQuery(jobSailingSubQuery, JoinCondition.And);

			var transports = factory.Load<Transport>(consolTransportQuery);

			foreach (var transport in transports)
			{
				switch (transport.JW_ParentType)
				{
					case Constants.TransportParentTypes.Consol:
						transport.ParentType = typeof(CommonConsol);
						yield return transport;
						break;

					case Constants.TransportParentTypes.Shipment:
						transport.ParentType = typeof(CommonShipment);
						yield return transport;
						break;

					case Constants.TransportParentTypes.AgencyShipment:
						transport.ParentType = ObjectFactory.GetType<Integration.Agency.IAgencyShipment>();
						yield return transport;
						break;

					case Constants.TransportParentTypes.ShipmentPreAdvice:
						transport.ParentType = ObjectFactory.GetType<Integration.Forwarding.IJobShipmentPreplanning>();
						yield return transport;
						break;

					case Constants.TransportParentTypes.Declaration:
						transport.ParentType = ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>();
						yield return transport;
						break;
				}
			}
		}

		#endregion

		#region SyncTransportEvents

#if DEBUG
		public
#endif
		void SyncTransportEvents(IEnumerable<Transport> transports)
		{
			Argument.NotNull(transports, nameof(transports));
			var oldTransportLogs = GetExistingTransportLogs(transports);

			foreach (var (transport, index) in transports.Select((value, i) => (value, i)))
			{
				if (transport.Sailing == null)
				{
					continue;
				}
				else if (transport.Sailing.Origin == null)
				{
					ErrorReporter.ReportOnce("SailingOriginShouldNotBeNull", "Sailing.Origin is null for a transport in SyncTransportEvents.");
					continue;
				}
				else if (transport.Sailing.Destination == null)
				{
					ErrorReporter.ReportOnce("SailingDestinationShouldNotBeNull", "Sailing.Destination is null for a transport in SyncTransportEvents.");
					continue;
				}

				var availableDate = transport.IsArrivalContainerModeFCLorULD
					? transport.Sailing.JX_JB_CTOAvailabilityDate
					: transport.Sailing.JX_DepotAvailabilityDate;

				var cutOffDate = transport.IsDepartureContainerModeFCLorULD
					? transport.Sailing.Origin.JA_CutOff
					: transport.Sailing.JX_DepotCutOff;

				var origin = transport.Sailing.Origin;
				var destination = transport.Sailing.Destination;

				SyncTransportEventLog(transport, Events.Departure, EstimateActual.Estimate, origin.JA_E_DEP, oldTransportLogs);
				SyncTransportEventLog(transport, Events.Departure, EstimateActual.Actual, origin.JA_A_DEP, oldTransportLogs);
				SyncTransportEventLog(transport, Events.Arrival, EstimateActual.Estimate, destination.JB_E_ARV, oldTransportLogs);
				SyncTransportEventLog(transport, Events.Arrival, EstimateActual.Actual, destination.JB_A_ARV, oldTransportLogs);
				SyncTransportEventLog(transport, Events.CutOffDate, EstimateActual.Estimate, cutOffDate, oldTransportLogs);
				SyncTransportEventLog(transport, Events.CargoAvailable, EstimateActual.Actual, availableDate, oldTransportLogs);
				SyncTransportEventLog(transport, Events.ReceiptCommenced, EstimateActual.Actual, origin.JA_ReceivalCommences, oldTransportLogs);
				SyncTransportEventLog(transport, Events.StorageCommenced, EstimateActual.Actual, destination.JB_StorageDate, oldTransportLogs);

				transport.TransportSupporterWithSchedule?.NotifyVoyageUpdated(transport);
			}
		}

		void SyncTransportEventLog(Transport transport, Event eventType, EstimateActual estimateActual, ZDateTime newValue, StmALog[] oldTransportLogs)
		{
			var oldLogs = oldTransportLogs.Where(x => x.SL_Parent == transport.PK
																				&& x.SL_SE_NKEvent == eventType.Code
																				&& x.SL_IsEstimate == (estimateActual == EstimateActual.Estimate))
																			   .OrderByDescending(x => x.SL_PostedTimeUtc)
																			   .ToArray();
			var unloco = transport.GetUNLOCOForEvent(eventType);
			var newValueOffset = newValue.ToDateTimeOffset(unloco);
			var oldValueOffset = oldLogs.Length > 0 ? oldLogs[0].SL_EventTimeOffset : ZDateTimeOffset.Empty;

			var newValueOffsetWithoutSeconds = newValueOffset.IsEmpty ? ZString.Empty : (ZString)newValueOffset.FormatDateTimeOffset();
			var oldValueOffsetWithoutSeconds = oldValueOffset.IsEmpty ? ZString.Empty : (ZString)oldValueOffset.FormatDateTimeOffset();

			if (oldValueOffsetWithoutSeconds != newValueOffsetWithoutSeconds)
			{
				if (!newValue.IsEmpty)
				{
					var logReference = transport.GetReferenceFreeTextForEvent(eventType, oldValueOffset, newValueOffset);
					var logParameters = transport.GetParametersForEvent(eventType).ToArray();

					transport.Logs.AddNew(eventType, logReference, newValueOffset, estimateActual == EstimateActual.Estimate, logParameters);
				}

				foreach (var log in oldLogs)
				{
					log.Cancel();
				}
			}
		}

		#endregion

		#region Implementation

		protected StmALog[] GetExistingTransportLogs(IEnumerable<Transport> transports)
		{
			if (transports.Any())
			{
				var existingLogsFilter = new ZQuery(StmALogSchema.SL_IsCancelled, false);
				AddToFilter(existingLogsFilter, StmALogSchema.SL_Parent, transports.Select(x => x.PK));
				AddToFilter(existingLogsFilter, StmALogSchema.SL_SE_NKEvent, new[] { Events.Departure.Code, Events.Arrival.Code, Events.CutOffDate.Code, Events.CargoAvailable.Code, Events.StorageCommenced.Code, Events.ReceiptCommenced.Code });
				return transports.First().Factory.Load<StmALog>(existingLogsFilter);
			}
			else
			{
				return Array.Empty<StmALog>();
			}
		}

		void AddToFilter<T>(ZQuery filter, CargoWise.Schema.SchemaColumn schemaColumn, IEnumerable<T> value)
		{
			var oldAllowTableValuedParameters = filter.AllowTableValuedParameters;
			filter.AllowTableValuedParameters = value.Count() > maximumElementsWithoutAllowTableValuedParameters;
			filter.AddToFilter(schemaColumn, value);
			filter.AllowTableValuedParameters = oldAllowTableValuedParameters;
		}

		#endregion

		#region EqualityComparer

		/// <summary>
		/// Comparer to Select only logs with a unique parent
		/// </summary>
		internal class QueuedLogUniqueParentComparer : IEqualityComparer<IQueuedLog>
		{
			public bool Equals(IQueuedLog log1, IQueuedLog log2)
			{
				if (Object.ReferenceEquals(log1, log2))
				{
					return true;
				}

				if (log1 == null || log2 == null)
				{
					return false;
				}

				return log1.SJ_ParentID == log2.SJ_ParentID;
			}

			public int GetHashCode(IQueuedLog log)
			{
				return log == null ? 0 : log.SJ_ParentID.GetHashCode();
			}
		}

		#endregion

		readonly int maximumElementsWithoutAllowTableValuedParameters;
	}
}

#endregion
