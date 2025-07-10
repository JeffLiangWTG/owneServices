using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.DataTransfer;
using Enterprise.Integration;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using NonPersistentStmALog = Enterprise.Freight.DataTransfer.NonPersistentStmALog;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	[Serializable]
	abstract class ContainerAutomationSubscriptionUpdater : LogSubscriber
	{
		#region LogSubscriber

		public override string[] EventTypes => new[] { AutoEvents.SubscriptionRequested.Code };

		public override string[] TableNames => new[] { BusinessObjectFactory.GetTableNameFromType(GetBusinessObjectType(), false) };

		protected BusinessObjectFactory LogFactory
		{
			get => logFactory;
			private set { logFactory = value; }
		}

		[NonSerialized]
		BusinessObjectFactory logFactory;

		/// <summary>
		///		Returns the type of a business object to handle subscription for.
		/// </summary>
		protected abstract Type GetBusinessObjectType();

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			if (IsTrackingEnabled)
			{
				if (IsEHubIDSet)
				{
					var globalTrackingLogs = queuedLogs.Where(IsGlobalTrackingRequest).ToList();

					if (globalTrackingLogs.Count > 0)
					{
						var businessObjectFactory = new ReadOnlyBusinessObjectFactory { RefreshEnabled = false, NameForDebugging = "DummyLogForGlobalContainerTracking" };
						var businessObjectType = GetBusinessObjectType();
						var schema = BusinessObjectFactory.GetTableSchemaFromType(businessObjectType);
						LogFactory = queuedLogs.First().Factory;
						var parentsQuery = new ZQuery(schema.PK, globalTrackingLogs.Select(l => l.SJ_ParentID));
						var parents = LogFactory.Load(businessObjectType, parentsQuery);

						foreach (var parent in parents)
						{
							SendSubscriptionRequest(businessObjectFactory, parent, globalTrackingLogs.First(l => l.SJ_ParentID == parent.PK));
						}
					}
				}
				else
				{
					DefaultLogger.Log(LogType.Warning, "Subscription requests cannot be sent as eHub ID is not set.");    // Just a log string
				}
			}
			else
			{
				DefaultLogger.Log(LogType.Information, "Subscription requests cannot be sent as Global Container Tracking is disabled."); // Just a log string
			}
		}

		protected virtual EventDataObjectWriter GetEventDataObjectWriter(IDataWritingManager dataWritingManager, BusinessObject parent)
		{
			return new EventDataObjectWriter(dataWritingManager) { PopulateAdditionalContexts = true };
		}

		protected override ILogBatcher GetLogBatcher() => new UserContextSwitchingLogBatcher(GetINotificationsWrapperAroundILogger(), Name);

		#endregion

		#region Internal

		static bool IsGlobalTrackingRequest(IQueuedLog log)
		{
			var parameters = StmALog.GetParametersFromReference(log.SJ_Reference);

			return parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, out var type)
				&& StringComparer.InvariantCultureIgnoreCase.Compare(type, Constants.EventReferenceParameterTypes.ContainerTracking) == 0;
		}

		void SendSubscriptionRequest(ReadOnlyBusinessObjectFactory businessObjectFactory, BusinessObject parent, IQueuedLog log)
		{
			DefaultLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Processing {0}", parent.HumanReadableName)); // Just a log string

			ITopLevelDataObjectWriter DataWriterGetter(IDataWritingManager outboundSessionTracker) => GetEventDataObjectWriter(outboundSessionTracker, parent);

			var dummyLog = NonPersistentStmALog.GetDummyLog(businessObjectFactory, parent, log, Constants.EventReferenceParameterTypes.ContainerTracking);

			var processor = UniversalXmlWorkflowProcessorBuilder.New(
				new LogSubscriptionActionWrapper(parent),
				new UniversalXmlCommunicationModeProvider(() => (CommunicationModes.ToArray(), null)),
				DataWriterGetter,
				dummyLog,
				null,
				null,
				UniversalXmlSchema.Version_2012_11_DO_NOT_USE);

			var replaceThisTokenEventuallyQuestionMarkExclamationMark = CancellationToken.None;
			processor.Process(GetINotificationsWrapperAroundILogger(), replaceThisTokenEventuallyQuestionMarkExclamationMark);
		}

		IEnumerable<NonPersistentEDICommunicationMode> CommunicationModes =>
			communicationModes ??
			(communicationModes = new[]
			{
				new NonPersistentEDICommunicationMode
				{
					EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService,
					EK_Destination = FreightDataRegistry.Instance.ContainerAutomationEHubID.Value
				}
			});

		bool IsTrackingEnabled => FreightDataRegistry.Instance.ContainerAutomation.Value;

		bool IsEHubIDSet => !string.IsNullOrEmpty(FreightDataRegistry.Instance.ContainerAutomationEHubID.Value);

		IEnumerable<NonPersistentEDICommunicationMode> communicationModes;

		#endregion
	}
}
