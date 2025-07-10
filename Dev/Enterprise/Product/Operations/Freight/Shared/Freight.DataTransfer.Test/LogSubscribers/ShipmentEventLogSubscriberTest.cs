using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.DataTransfer.Testing
{
	[TestedType(typeof(ShipmentEventLogSubscriber))]
	public class ShipmentEventLogSubscriberTest : LogSubscriberTest<ShipmentEventLogSubscriber>
	{
		protected static string _eHubIdForCA = "CA_EHubID";
		protected static string _eHubIdForAWBA = "AWBA_EHubID";

		public void TestShipmentsEventLogsAreProcessed()
		{
			var logSubscriber = new ShipmentEventLogSubscriber();
			var events = logSubscriber.GetEventTypes();

			var quotedBookingEventLogSubscriber = new ShipmentEventLogProcessorForTest();
			GenerateTestLog(events[0], ViewQuotedBookingSchema.Constants.TableName);
			Factory.Save();

			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			using (SetEventVisibilityOverride())
			using (ObjectFactory.Substitute("QuotedBookingEventLogSubscriber", quotedBookingEventLogSubscriber))
			{
				RunLogWalkerCycleForTest();
				AssertEquals(1, quotedBookingEventLogSubscriber.InvokeCount);
			}
		}

		public void TestEventTypesReturnsListOnlySHPEvents_MarkedActiveAndSystem()
		{
			using (SetEventVisibilityOverride())
			{
				var logSubscriber = new ShipmentEventLogSubscriber();
				var events = logSubscriber.GetEventTypes();
				var isARVEventIncluded = events.Contains("ARV");
				var isAEDEventIncluded = events.Contains("AED");

				AssertEquals(true, isARVEventIncluded);
				AssertEquals(false, isAEDEventIncluded);
			}
		}
		
		public void AssertProcessQueueLogs_XML_HasVesselVoyageParametersPopulated(string referenceITN, IDictionary<string, string> eventParameters, IDictionary<string, string> contextParameters, bool isEstimate)
		{
			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			using (SetEventVisibilityOverride())
			{
				RunLogWalkerCycleForTest();
				var ediMessage = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging)).OrderByDescending(x => ((BusinessObject)x)[EDIMessageSchema.EM_SystemCreateTimeUtc]).FirstOrDefault();

				AssertNotNull(ediMessage);

				var xmlEvent = ediMessage.GetEM_MessageTextReader().Parse<Event>();
				var eventParams = xmlEvent.EventParameters;

				AssertsEqualEventParameters(eventParameters, eventParams);
				AssertEqualsCommonContextParameters(referenceITN, contextParameters, xmlEvent);
				AssertsEqualVesselVoyageContextParameters(contextParameters, xmlEvent);
				AssertEquals(xmlEvent.IsEstimate, isEstimate);
			}
		}

		public void AssertProcessQueueLogs_XML_HasCommonEventAndContextParametersPopulated(string referenceITN, IDictionary<string, string> eventParameters, IDictionary<string, string> contextParameters)
		{
			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			using (SetEventVisibilityOverride())
			{
				RunLogWalkerCycleForTest();
				var ediMessage = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging)).OrderByDescending(x => ((BusinessObject)x)[EDIMessageSchema.EM_SystemCreateTimeUtc]).FirstOrDefault();

				AssertNotNull(ediMessage);

				var xmlEvent = ediMessage.GetEM_MessageTextReader().Parse<Event>();
				var eventParams = xmlEvent.EventParameters;

				AssertsEqualEventParameters(eventParameters, eventParams);
				AssertEqualsCommonContextParameters(referenceITN, contextParameters, xmlEvent);
			}
		}

		public void AssertProcessQueueLogs_XML_HasContainerDetailsPopulated(string referenceITN, IDictionary<string, string> eventParameters, IDictionary<string, string> contextParameters)
		{
			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			using (SetEventVisibilityOverride())
			{
				RunLogWalkerCycleForTest();
				var ediMessage = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging)).OrderByDescending(x => ((BusinessObject)x)[EDIMessageSchema.EM_SystemCreateTimeUtc]).FirstOrDefault();

				AssertNotNull(ediMessage);

				var xmlEvent = ediMessage.GetEM_MessageTextReader().Parse<Event>();
				var eventParams = xmlEvent.EventParameters;

				AssertsEqualEventParameters(eventParameters, eventParams);
				AssertEqualsCommonContextParameters(referenceITN, contextParameters, xmlEvent);
				AssertsEqualContainerContextParameters(contextParameters, xmlEvent);
			}
		}

		public void AssertProcessQueueLogs_XML_HasAllContextParametersPopulated(string referenceITN, IDictionary<string, string> eventParameters, IDictionary<string, string> contextParameters)
		{
			using (SetContainerAutomationEnabled())
			using (SetEHubId())
			using (SetEventVisibilityOverride())
			{
				RunLogWalkerCycleForTest();
				var ediMessage = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging)).OrderByDescending(x => ((BusinessObject)x)[EDIMessageSchema.EM_SystemCreateTimeUtc]).FirstOrDefault();

				AssertNotNull(ediMessage);

				var xmlEvent = ediMessage.GetEM_MessageTextReader().Parse<Event>();
				var eventParams = xmlEvent.EventParameters;

				AssertsEqualEventParameters(eventParameters, eventParams);
				AssertEqualsCommonContextParameters(referenceITN, contextParameters, xmlEvent);
				AssertsEqualContainerContextParameters(contextParameters, xmlEvent);
				AssertsEqualVesselVoyageContextParameters(contextParameters, xmlEvent);
			}
		}

		void AssertsEqualVesselVoyageContextParameters(IDictionary<string, string> contextParameters, Event xmlEvent)
		{
			AssertEquals(contextParameters["VesselName"], xmlEvent.ContextCollection.First(c => c.Type == nameof(Event.ContextTypes.VesselName)).Value);
			AssertEquals(contextParameters["LloydsNumber"], xmlEvent.ContextCollection.First(c => c.Type == nameof(Event.ContextTypes.LloydsNumber)).Value);
			AssertEquals(contextParameters["VoyageNumber"], xmlEvent.ContextCollection.First(c => c.Type == nameof(Event.ContextTypes.VoyageNumber)).Value);
			AssertEquals(contextParameters["LegOriginUNLOCO"], xmlEvent.ContextCollection.First(c => c.Type == nameof(Event.ContextTypes.LegOriginUNLOCO)).Value);
			AssertEquals(contextParameters["LegDestinationUNLOCO"], xmlEvent.ContextCollection.First(c => c.Type == nameof(Event.ContextTypes.LegDestinationUNLOCO)).Value);
		}

		public void AssertsEqualContainerContextParameters(IDictionary<string, string> contextParameters, Event xmlEvent)
		{
			AssertEquals(contextParameters["ContainerNumber"], xmlEvent.ContextCollection.First(c => c.Type == nameof(Event.ContextTypes.ContainerNumber)).Value);
			AssertEquals(contextParameters["ContainerISOCode"], xmlEvent.ContextCollection.First(c => c.Type == nameof(Event.ContextTypes.ContainerISOCode)).Value);
		}

		void AssertsEqualEventParameters(IDictionary<string, string> eventParameters, EventParameters eventParams)
		{
			AssertEquals(eventParameters[Constants.EventReferenceParameters.Codes.Location], eventParams.GetEventParameter(Constants.EventReferenceParameters.Codes.Location));
			AssertEquals(eventParameters[Constants.EventReferenceParameters.Codes.Facility], eventParams.GetEventParameter(Constants.EventReferenceParameters.Codes.Facility));
			AssertEquals(eventParameters[Constants.EventReferenceParameters.Codes.Department], eventParams.GetEventParameter(Constants.EventReferenceParameters.Codes.Department));
			AssertEquals(eventParameters[Constants.EventReferenceParameters.Codes.Type], eventParams.GetEventParameter(Constants.EventReferenceParameters.Codes.Type));
			AssertEquals(eventParameters[Constants.EventReferenceParameters.Codes.Mode], eventParams.GetEventParameter(Constants.EventReferenceParameters.Codes.Mode));
		}

		void AssertEqualsCommonContextParameters(string referenceITN, IDictionary<string, string> contextParameters, Event xmlEvent)
		{
			AssertEquals("CargoWise", xmlEvent.ContextCollection.First(c => c.Type == ShipmentEventContextType.EventSource).Value);
			AssertEquals(referenceITN, xmlEvent.ContextCollection.First(c => c.Type == ShipmentEventContextType.Reference).Value.ToString());
			AssertEquals(contextParameters["CarrierC1CCode"], xmlEvent.ContextCollection.First(c => c.Type == nameof(Event.ContextTypes.CarrierC1CCode)).Value);
			AssertEquals(contextParameters["CarriersBookingReference"], xmlEvent.ContextCollection.First(c => c.Type == nameof(Event.ContextTypes.CarriersBookingReference)).Value);
			AssertEquals(contextParameters["MBOLNumber"], xmlEvent.ContextCollection.First(c => c.Type == nameof(Event.ContextTypes.MBOLNumber)).Value);
			AssertEquals(contextParameters["MBOLOriginUNLOCO"], xmlEvent.ContextCollection.First(c => c.Type == nameof(Event.ContextTypes.MBOLOriginUNLOCO)).Value);
			AssertEquals(contextParameters["MBOLDestinationUNLOCO"], xmlEvent.ContextCollection.First(c => c.Type == nameof(Event.ContextTypes.MBOLDestinationUNLOCO)).Value);
		}

		void GenerateTestLog(string eventCode, string parentTableName)
		{
			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log[StmALogSchema.SL_Parent] = ZGuid.NewZGuid();
				log[StmALogSchema.SL_SE_NKEvent] = eventCode;
				log[StmALogSchema.SL_Table] = parentTableName;
				log[StmALogSchema.SL_EventTime] = ZDateTime.Now;
			}
		}

		[Serializable]
		class ShipmentEventLogProcessorForTest : ShipmentEventLogProcessor
		{
			public int InvokeCount { get; private set; }

			protected override string GetPackingMode(BusinessObject logParent)
			{
				return null;
			}

			protected override BusinessObject GetLogParentForBusinessObject(BusinessObject logParent, IQueuedLog log)
			{
				return null;
			}

			protected override bool ShouldProcess(BusinessObject logParent)
			{
				return false;
			}

			protected override BusinessObject GetLogParent(IQueuedLog log)
			{
				InvokeCount++;
				return null;
			}

			protected override IEnumerable<BusinessObject> GetContainerShipmentParent(CommonContainer container)
			{
				return null;
			}

			protected override IEnumerable<CommonContainer> GetContainers(BusinessObject logParent)
			{
				return null;
			}

			protected override bool IsLCLDatesOverrideConsol(BusinessObject logParent)
			{
				return false;
			}
		}

		protected static IDisposable SetEHubId()
		{
			return FreightDataRegistry.Instance.ContainerAutomationEHubID.SetTemporaryValue(
				Guid.Empty,
				Guid.Empty,
				Guid.Empty,
				"Test");
		}

		protected static IDisposable SetContainerAutomationEnabled()
		{
			return FreightDataRegistry.Instance.GlobalTrackingShipmentVisibility.SetTemporaryValue(Guid.Empty,
				Guid.Empty, Guid.Empty, new GlobalTrackingShipmentVisibilityOptions
				{
					IsActive = true,
					IsDefaultContainerAutomation = false,
					ServiceEhubIDs = new GlobalTrackingShipmentVisibilityServiceEhubIDCollection
					{
						{ "CA", "CA",  _eHubIdForCA },
						{ "AWBA", "AWBA", _eHubIdForAWBA }
					}
				});
		}

		protected static IDisposable SetEventVisibilityOverride()
		{
			var eventVisibilityOverrideCollection = new EventVisibilityOverrideCollection();
			eventVisibilityOverrideCollection.AddNew("SHP");

			return WebDataRegistry.Instance.EventVisibilityOverride.SetTemporaryValue(Guid.Empty, Guid.Empty,
				Guid.Empty, eventVisibilityOverrideCollection);
		}
	}
}
