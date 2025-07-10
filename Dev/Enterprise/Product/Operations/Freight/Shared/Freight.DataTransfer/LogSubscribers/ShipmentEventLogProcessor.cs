using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.EntityFramework;
using CargoWise.EventReference;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.DataTransfer
{
	[Serializable]
	public abstract class ShipmentEventLogProcessor : ShipmentEventLogSubscriber
	{
		#region LogSubscriber

		const string eventReferenceParameterType = Core.Constants.EventReferenceParameterTypes.ShipmentVisibility;

		protected IEnumerable<GlobalTrackingShipmentVisibilityServiceEhubID> registryValues = FreightDataRegistry.Instance.GlobalTrackingShipmentVisibility.Value.ServiceEhubIDs.Select(x => (GlobalTrackingShipmentVisibilityServiceEhubID)x).ToList();

		protected abstract BusinessObject GetLogParent(IQueuedLog log);

		protected abstract BusinessObject GetLogParentForBusinessObject(BusinessObject logParent, IQueuedLog log);

		protected abstract bool ShouldProcess(BusinessObject logParent);

		protected abstract bool IsLCLDatesOverrideConsol(BusinessObject logParent);

		protected abstract string GetPackingMode(BusinessObject logParent);

		protected abstract IEnumerable<BusinessObject> GetContainerShipmentParent(CommonContainer container);

		protected abstract IEnumerable<CommonContainer> GetContainers(BusinessObject logParent);

		protected IEnumerable<StmALog> GetSubscriptionList(BusinessObject logParent, string eventReference)
		{
			var includedService = registryValues.Select(x => x.Code);

			var selectOnes = (logParent as IStmALogParent)?.Logs.GetAllLogs()
				.Cast<StmALog>()
				.Where(x => x.SL_SE_NKEvent == AutoEvents.SubscriptionRequested.Code &&
								x.SL_Reference.Contains(eventReference) &&
								includedService.Any(service => x.SL_Reference.Contains(service)));
#if NETFRAMEWORK
			return selectOnes.DistinctBy(_ => _.SL_Reference);
#elif NET
			return Enumerable.DistinctBy(selectOnes, _ => _.SL_Reference);
#else
#error Unexpected target platform
#endif
		}

		protected StmALog GetSourceLog(IQueuedLog log, BusinessObject logParent)
		{
			return (logParent as IStmALogParent)?.Logs.GetAllLogs().Where(_ => _.PK == log.SJ_ALogReference).FirstOrDefault();
		}

		public void ProcessShipmentLogs(IQueuedLog queuedShipmentLog, List<IQueuedLog> containerLogs)
		{
			var logParent = GetLogParent(queuedShipmentLog);

			if (logParent == null)
			{
				return;
			}

			var relatedContainers = GetContainers(logParent)?.ToList();
			var shouldCheckDuplicate = queuedShipmentLog.SJ_SE_NKEvent == AutoEvents.CargoAvailableCode &&
									   relatedContainers != null && relatedContainers.Count > 0 &&
									   IsEventParametersMatched_EventSourceContainer(queuedShipmentLog.SJ_SE_NKEvent, queuedShipmentLog.SJ_Reference, logParent);

			bool isDuplicatedInContainerLogs = shouldCheckDuplicate && CheckDuplicateForCAVevent(containerLogs, queuedShipmentLog, relatedContainers);

			if (isDuplicatedInContainerLogs)
			{
				return;
			}

			var contextMappings = GetEventContextValuesFromEventLogEDIMessage(queuedShipmentLog, logParent);
			ProcessLogForSubscriptions(queuedShipmentLog, logParent, (NoResString)"Shipment", contextMappings);
		}

		public void ProcessTransportLogs(IQueuedLog queuedTransportLog)
		{
			var readOnlyBusinessObjectFactory = new ReadOnlyBusinessObjectFactory
			{
				RefreshEnabled = false,
				NameForDebugging = "DummyLogFor_GlobalTrackingShipmentVisibility_GlobalContainerTracking"
			};

			var transport = readOnlyBusinessObjectFactory.Load<Transport>(queuedTransportLog.SJ_ParentID);
			var contextMappings = GetEventContextValuesFromEventLogEDIMessage(queuedTransportLog, transport);
			var transportSpecificContextMappings = GetFallbackEventContextValues_EventSourceTransport(contextMappings, transport);

			if (transport?.JW_ParentType.ToString() == Core.Constants.TransportParentTypes.Consol)
			{
				var consol = readOnlyBusinessObjectFactory.Load(JobConsolSchema.Constants.Prefix, transport.JW_ParentGUID) as CommonConsol;
				var parentShipments = consol.Shipments?.WhereNotNull()?.ToList();

				foreach (var shipment in parentShipments)
				{
					ProcessLogForSubscriptions(queuedTransportLog, shipment, (NoResString)"Transport", transportSpecificContextMappings);
				}
			}

			if (transport?.JW_ParentType.ToString() == Core.Constants.TransportParentTypes.Shipment || transport?.JW_ParentType.ToString() == Core.Constants.TransportParentTypes.AgencyShipment)
			{
				var parent = readOnlyBusinessObjectFactory.Load(JobShipmentSchema.Constants.Prefix, transport.JW_ParentGUID) as CommonShipment;
				ProcessLogForSubscriptions(queuedTransportLog, parent, (NoResString)"Transport", transportSpecificContextMappings);
			}
		}

		public void ProcessContainerLogs(IQueuedLog queuedContainerLog)
		{
			var readOnlyBusinessObjectFactory = new ReadOnlyBusinessObjectFactory
			{
				RefreshEnabled = false,
				NameForDebugging = "DummyLogFor_GlobalTrackingShipmentVisibility_GlobalContainerTracking"
			};

			var container = readOnlyBusinessObjectFactory.Load(JobContainerSchema.Constants.Prefix, queuedContainerLog.SJ_ParentID) as CommonContainer;
			var parentShipments = container != null ? GetContainerShipmentParent(container) : null;

			if (parentShipments.IsNullOrEmpty())
			{
				return;
			}

			var contextMappings = GetEventContextValuesFromEventLogEDIMessage(queuedContainerLog, container);
			var containerSpecificContextMappinsg = GetFallbackEventContextValues_EventSourceContainer(contextMappings, container);

			foreach (var parent in parentShipments.WhereNotNull()?.ToList())
			{
				ProcessLogForSubscriptions(queuedContainerLog, parent, (NoResString)"Container", containerSpecificContextMappinsg);
			}
		}

		bool CheckDuplicateForCAVevent(List<IQueuedLog> containerLogs, IQueuedLog queuedLog, List<CommonContainer> containers)
		{
			var isDuplicate = false;

			foreach (var container in containers)
			{
				if (containerLogs != null &&
					containerLogs.Any(_ => _.SJ_ParentID == container.PK &&
										   _.SJ_SE_NKEvent == queuedLog.SJ_SE_NKEvent &&
										   _.SJ_EventTime == queuedLog.SJ_EventTime &&
										   _.SJ_Reference == queuedLog.SJ_Reference))
				{
					isDuplicate = true;
				}
			}

			return isDuplicate;
		}

		void ProcessLogForSubscriptions(IQueuedLog queuedLog, BusinessObject parent, string eventSource, IDictionary<string, string> contextMappings)
		{
			var readOnlyBusinessObjectFactory = new ReadOnlyBusinessObjectFactory
			{
				RefreshEnabled = false,
				NameForDebugging = "DummyLogFor_GlobalTrackingShipmentVisibility_GlobalContainerTracking"
			};

			var subscriptions = ShouldProcess(parent)
				? GetSubscriptionList(parent, eventReferenceParameterType).ToList()
				: null;

			if (subscriptions?.Any() == true && IsEventParametersMatched(queuedLog, parent, eventSource))
			{
				var parentToProcess = GetLogParentForBusinessObject(parent, queuedLog);

				foreach (var subscription in subscriptions)
				{
					SendSubscriptionRequest(readOnlyBusinessObjectFactory, parentToProcess, queuedLog, subscription, contextMappings);
				}
			}
		}

		protected virtual EventDataObjectWriter GetEventDataObjectWriter(IDataWritingManager dataWritingManager,
			BusinessObject parent, string subscriptionReference, IDictionary<string, string> contextMappings)
		{
			return new EventDataObjectWriter(dataWritingManager) { PopulateAdditionalContexts = true };
		}

		protected override ILogBatcher GetLogBatcher() =>
			new UserContextSwitchingLogBatcher(GetINotificationsWrapperAroundILogger(), Name);

		#endregion

		#region Internal

		void SendSubscriptionRequest(ReadOnlyBusinessObjectFactory businessObjectFactory, BusinessObject parent,
			IQueuedLog log, StmALog subscription, IDictionary<string, string> contextMappings)
		{
			DefaultLogger.Log(LogType.Information,
				string.Format(CultureInfo.InvariantCulture, "Processing {0}",
					parent.HumanReadableName));

			ITopLevelDataObjectWriter DataWriterGetter(IDataWritingManager outboundSessionTracker) =>
				GetEventDataObjectWriter(outboundSessionTracker, parent, subscription.SL_Reference, contextMappings);

			var communicationModes = CreateCommunicationModes(subscription);

			if (communicationModes.IsNullOrEmpty())
			{
				return;
			}

			var dummyLog =
				NonPersistentStmALog.GetDummyLog(businessObjectFactory, parent, log);

			var processor = UniversalXmlWorkflowProcessorBuilder.New(
				new LogSubscriptionActionWrapper(parent),
				new UniversalXmlCommunicationModeProvider(() => (communicationModes.ToArray(), null)),
				DataWriterGetter,
				dummyLog,
				null,
				null,
				UniversalXmlSchema.Version_2012_11_DO_NOT_USE);

			var replaceThisTokenEventuallyQuestionMarkExclamationMark = CancellationToken.None;
			processor.Process(GetINotificationsWrapperAroundILogger(),
				replaceThisTokenEventuallyQuestionMarkExclamationMark);
		}

		IEnumerable<NonPersistentEDICommunicationMode> CreateCommunicationModes(StmALog subscription)
		{
			var registryItem = registryValues.FirstOrDefault(x => x.Code == subscription.Parameters[Params.Service]);
			if (registryItem != null)
			{
				return new[]
				{
					new NonPersistentEDICommunicationMode
					{
						EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService,
						EK_Destination = registryItem.EhubID
					}
				};
			}
			else
			{
				ErrorReporter.ReportDeveloperExceptionOnce($"Registry is missing for {Params.Service} service", null);
				return Enumerable.Empty<NonPersistentEDICommunicationMode>();
			}
		}

		string GetParameterValue(ObservableDictionary<string, string> logParameters, string key)
		{
			var result = string.Empty;
			logParameters?.TryGetValue(key, out result);

			return result;
		}

		bool HasParameterEqualTo(ObservableDictionary<string, string> logParameters, string key, string expectedValue)
		{
			var parameterValue = GetParameterValue(logParameters, key);
			var parameterKeyExists = !string.IsNullOrWhiteSpace(parameterValue) && !parameterValue.IsNullOrEmpty();

			if (expectedValue.IsNullOrEmpty())
			{
				return parameterKeyExists;
			}

			return parameterKeyExists &&
				   parameterValue.Equals(expectedValue, StringComparison.OrdinalIgnoreCase);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter parameter with constant value")]
		bool IsEventParametersMatched(IQueuedLog queuedLog, BusinessObject logParent, string eventSource)
		{
			var eventCode = queuedLog.SJ_SE_NKEvent;
			var logReference = queuedLog.SJ_Reference;
			var isEstimate = queuedLog.SJ_IsEstimate;

			return eventSource switch
			{
				"Transport" => IsEventParametersMatched_EventSourceTransport(eventCode, logReference, logParent, isEstimate),
				"Shipment" => IsEventParametersMatched_EventSourceShipment(eventCode, logReference, logParent),
				"Container" => IsEventParametersMatched_EventSourceContainer(eventCode, logReference, logParent),
				_ => false,
			};
		}
		bool IsEventParametersMatched_EventSourceTransport(string eventCode, string logReference, BusinessObject logParent, bool isEstimate)
		{
			var logParameters = StmALog.GetParametersFromReference(logReference);

			switch (eventCode)
			{
				case AutoEvents.ArrivalCode:
				case AutoEvents.DepartureCode:
					return HasParameterEqualTo(logParameters, Constants.EventReferenceParameters.Codes.Location, null) &&
						   HasParameterEqualTo(logParameters, Constants.EventReferenceParameters.Codes.Mode, null) &&
						   HasParameterEqualTo(logParameters, Constants.EventReferenceParameters.Codes.Facility, Constants.Facilities.Code.Terminal);

				case AutoEvents.CutOffDateCode:
					return HasParameterEqualTo(logParameters, Constants.EventReferenceParameters.Codes.Location, null) &&
						   CheckPackingModeForFAC(logParameters, GetPackingMode(logParent));

				case AutoEvents.ReceiptCommencedCode:
					return !isEstimate &&
						   HasParameterEqualTo(logParameters, Constants.EventReferenceParameters.Codes.Location, null) &&
						   CheckPackingModeForFAC(logParameters, GetPackingMode(logParent));
				default:
					return false;
			}
		}

		bool IsEventParametersMatched_EventSourceContainer(string eventCode, string logReference, BusinessObject logParent)
		{
			var logParameters = StmALog.GetParametersFromReference(logReference);

			switch (eventCode)
			{
				case AutoEvents.FreightLoadedCode:
				case AutoEvents.FreightUnloadedCode:
					return HasParameterEqualTo(logParameters, Constants.EventReferenceParameters.Codes.Location, null) &&
						   HasParameterEqualTo(logParameters, Constants.EventReferenceParameters.Codes.Mode, null) &&
						   HasParameterEqualTo(logParameters, Constants.EventReferenceParameters.Codes.Facility, Constants.Facilities.Code.Terminal);

				case AutoEvents.CargoAvailableCode:
					return HasParameterEqualTo(logParameters, Constants.EventReferenceParameters.Codes.Location, null) &&
						   CheckPackingModeForFAC(logParameters, GetPackingMode(logParent))
						   && !IsLCLDatesOverrideConsol(logParent);

				case AutoEvents.GateOutCode:
				case AutoEvents.GateInCode:
					return HasParameterEqualTo(logParameters, Constants.EventReferenceParameters.Codes.Location, null) &&
						   (HasParameterEqualTo(logParameters, Constants.EventReferenceParameters.Codes.Facility, Constants.Facilities.Code.Depot) ||
						   HasParameterEqualTo(logParameters, Constants.EventReferenceParameters.Codes.Facility, Constants.Facilities.Code.ContainerYard) ||
						   HasParameterEqualTo(logParameters, Constants.EventReferenceParameters.Codes.Facility, Constants.Facilities.Code.Terminal));

				case AutoEvents.CustomsClearedCode:
					return true;

				case AutoEvents.ReleasedCode:
				case AutoEvents.HeldCode:
					return HasParameterEqualTo(logParameters, Constants.EventReferenceParameters.Codes.Department, null) &&
						   HasParameterEqualTo(logParameters, Constants.EventReferenceParameters.Codes.Location, null);

				case AutoEvents.DehireCode:
					return HasParameterEqualTo(logParameters, Constants.EventReferenceParameters.Codes.Location, null) &&
						   HasParameterEqualTo(logParameters, Constants.EventReferenceParameters.Codes.Facility, Constants.Facilities.Code.ContainerYard);
				default:
					return false;
			}
		}

		bool IsEventParametersMatched_EventSourceShipment(string eventCode, string logReference, BusinessObject logParent)
		{
			var logParameters = StmALog.GetParametersFromReference(logReference);

			switch (eventCode)
			{
				case AutoEvents.CargoAvailableCode:
					return HasParameterEqualTo(logParameters, Constants.EventReferenceParameters.Codes.Location, null) &&
						   CheckPackingModeForFAC(logParameters, GetPackingMode(logParent));

				case AutoEvents.StorageCommencedCode:
					return HasParameterEqualTo(logParameters, Constants.EventReferenceParameters.Codes.Location, null) &&
						   CheckPackingModeForFAC(logParameters, GetPackingMode(logParent));

				case AutoEvents.GateOutCode:
				case AutoEvents.GateInCode:
					return HasParameterEqualTo(logParameters, Constants.EventReferenceParameters.Codes.Location, null) &&
						   HasParameterEqualTo(logParameters, Constants.EventReferenceParameters.Codes.Facility, Constants.Facilities.Code.Depot);

				case AutoEvents.PickupCartageCompleteFinalisedCode:
				case AutoEvents.DeliveryCartageCompleteFinalisedCode:
					return true;

				case AutoEvents.UnpackingCompletedCode:
				case AutoEvents.PackingCompletedCode:
					return HasParameterEqualTo(logParameters, Constants.EventReferenceParameters.Codes.Location, null) &&
						   HasParameterEqualTo(logParameters, Constants.EventReferenceParameters.Codes.Facility, Constants.Facilities.Code.Depot);

				case AutoEvents.CustomsClearedCode:
					return true;

				case AutoEvents.ReleasedCode:
				case AutoEvents.HeldCode:
					return HasParameterEqualTo(logParameters, Constants.EventReferenceParameters.Codes.Department, null) &&
						   HasParameterEqualTo(logParameters, Constants.EventReferenceParameters.Codes.Location, null);
				default:
					return false;
			}
		}

		bool CheckPackingModeForFAC(ObservableDictionary<string, string> logParameters, string packingMode)
		{
			return packingMode == Core.Constants.ContainerModes.FCL ? HasParameterEqualTo(logParameters, Constants.EventReferenceParameters.Codes.Facility, Constants.Facilities.Code.Terminal) :
				packingMode == Core.Constants.ContainerModes.LCL && HasParameterEqualTo(logParameters, Constants.EventReferenceParameters.Codes.Facility, Constants.Facilities.Code.Depot);
		}

		IDictionary<string, string> GetDefaultShipmentEventContextCollection()
		{
			var contextPairs = new Dictionary<string, string>
			{
				{ nameof(Event.ContextTypes.CarrierC1CCode), null },
				{ nameof(Event.ContextTypes.CarriersBookingReference), null },
				{ nameof(Event.ContextTypes.MBOLNumber), null },
				{ nameof(Event.ContextTypes.ContainerNumber), null },
				{ nameof(Event.ContextTypes.ContainerISOCode), null },
				{ nameof(Event.ContextTypes.VesselName), null },
				{ nameof(Event.ContextTypes.LloydsNumber), null },
				{ nameof(Event.ContextTypes.VoyageNumber), null },
				{ nameof(Event.ContextTypes.LegOriginUNLOCO), null },
				{ nameof(Event.ContextTypes.LegDestinationUNLOCO), null },
				{ nameof(Event.ContextTypes.MBOLOriginUNLOCO), null },
				{ nameof(Event.ContextTypes.MBOLDestinationUNLOCO), null },
				{ ShipmentEventContextType.Reference, null },
				{ ShipmentEventContextType.EventSource, "CargoWise" }
			};

			return contextPairs;
		}

		IDictionary<string, string> GetEventContextValuesFromEventLogEDIMessage(IQueuedLog queuedLog, BusinessObject logParent)
		{
			var log = GetSourceLog(queuedLog, logParent);
			var contextValues = GetDefaultShipmentEventContextCollection();

			if (log == null || log == default)
			{
				return contextValues;
			}

			var relatedEDIMessage = log.RelatedEDIMessage;
			if (relatedEDIMessage != null)
			{
				var ediMessage = relatedEDIMessage.Message as EDIMessage;
				var xmlEvent = ediMessage?.GetEM_MessageTextReader().Parse<Event>();
				contextValues[nameof(Event.ContextTypes.ContainerNumber)] = xmlEvent.ContextCollection.FirstOrDefault(c => c.Type == nameof(Event.ContextTypes.ContainerNumber))?.Value;
				contextValues[nameof(Event.ContextTypes.ContainerISOCode)] = xmlEvent.ContextCollection.FirstOrDefault(c => c.Type == nameof(Event.ContextTypes.ContainerISOCode))?.Value;
				contextValues[nameof(Event.ContextTypes.VesselName)] = xmlEvent.ContextCollection.FirstOrDefault(c => c.Type == nameof(Event.ContextTypes.VesselName))?.Value;
				contextValues[nameof(Event.ContextTypes.LloydsNumber)] = xmlEvent.ContextCollection.FirstOrDefault(c => c.Type == nameof(Event.ContextTypes.LloydsNumber))?.Value;
				contextValues[nameof(Event.ContextTypes.VoyageNumber)] = xmlEvent.ContextCollection.FirstOrDefault(c => c.Type == nameof(Event.ContextTypes.VoyageNumber))?.Value;
				contextValues[nameof(Event.ContextTypes.LegOriginUNLOCO)] = xmlEvent.ContextCollection.FirstOrDefault(c => c.Type == nameof(Event.ContextTypes.LegOriginUNLOCO))?.Value;
				contextValues[nameof(Event.ContextTypes.LegDestinationUNLOCO)] = xmlEvent.ContextCollection.FirstOrDefault(c => c.Type == nameof(Event.ContextTypes.LegDestinationUNLOCO))?.Value;
			}

			return contextValues;
		}

		IDictionary<string, string> GetFallbackEventContextValues_EventSourceTransport(IDictionary<string, string> contextMappings, Transport transport)
		{
			contextMappings[nameof(Event.ContextTypes.VesselName)] ??= transport?.JW_Vessel;
			contextMappings[nameof(Event.ContextTypes.LloydsNumber)] ??= transport?.Vessel?.RV_LloydsNumber;
			contextMappings[nameof(Event.ContextTypes.VoyageNumber)] ??= transport?.JW_VoyageFlight;
			contextMappings[nameof(Event.ContextTypes.LegOriginUNLOCO)] ??= transport?.JW_RL_NKLoadPort;
			contextMappings[nameof(Event.ContextTypes.LegDestinationUNLOCO)] ??= transport?.JW_RL_NKDiscPort;

			return contextMappings;
		}

		IDictionary<string, string> GetFallbackEventContextValues_EventSourceContainer(IDictionary<string, string> contextMappings, CommonContainer container)
		{
			contextMappings[nameof(Event.ContextTypes.ContainerNumber)] ??= container?.ContainerCode;
			contextMappings[nameof(Event.ContextTypes.ContainerISOCode)] ??= container?.Container?.RC_ISOType;

			return contextMappings;
		}

		#endregion
	}
}
