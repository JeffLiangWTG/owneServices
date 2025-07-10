using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Movements = Enterprise.Freight.Agency.Business.ContainerMovementTypes.Codes;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyShipmentContainerMovementEventSynchroniserTest : BaseAgencyTest
	{
		public void TestUpdateContainerMovementEvents()
		{
			AssertEventsSynchronization(Movements.Discharge, Events.FreightUnloaded, Constants.Facilities.Code.Terminal);
			AssertEventsSynchronization(Movements.DepotGateIn, Events.GateIn, Constants.Facilities.Code.Depot);
			AssertEventsSynchronization(Movements.DepotGateOut, Events.GateOut, Constants.Facilities.Code.Depot);
			AssertEventsSynchronization(Movements.Load, Events.FreightLoaded, Constants.Facilities.Code.Terminal);
			AssertEventsSynchronization(Movements.WharfGateOut, Events.GateOut, Constants.Facilities.Code.Terminal);
			AssertEventsSynchronization(Movements.WharfGateIn, Events.GateIn, Constants.Facilities.Code.Terminal);
			AssertEventsSynchronization(Movements.ReturnToWharf, Events.GateIn, Constants.Facilities.Code.ContainerYard);
			AssertEventsSynchronization(Movements.ReturnToWharf, Events.Dehire, Constants.Facilities.Code.ContainerYard);
			AssertEventsSynchronization(Movements.YardGateIn, Events.GateIn, Constants.Facilities.Code.ContainerYard);
			AssertEventsSynchronization(Movements.YardGateIn, Events.Dehire, Constants.Facilities.Code.ContainerYard);
			AssertEventsSynchronization(Movements.YardGateOut, Events.GateOut, Constants.Facilities.Code.ContainerYard);
			AssertEventsSynchronization(Movements.ReturnedUnshipped, Events.Dehire, Constants.Facilities.Code.ContainerYard);
			AssertEventsSynchronization(Movements.ReShipRequested, Events.Dehire, Constants.Facilities.Code.Consignee);
		}

		void AssertEventsSynchronization(string movementType, Event eventType, string facility)
		{
			var lOC = Constants.EventReferenceParameters.Codes.Location;
			var fAC = Constants.EventReferenceParameters.Codes.Facility;
			new TestCase(Factory).WithMovement(movementType, 1.DaysAgo(), "UAIEV").WithEvent(eventType, 1.DaysAgo(), lOC.As("UAIEV"), fAC.As(facility)).WithMovement(movementType, 2.DaysAgo(), "AUSYD").WithMovement(movementType, 2.DaysAgo(), null).WithMovement("sss", 2.DaysAgo(), "AUSYD").WithEvent(eventType, 3.DaysAgo(), lOC.As("AUSYD"), fAC.As(facility)).WithEvent(eventType, 2.DaysAgo(), lOC.As("AUSYD")).WithEvent(eventType, 2.DaysAgo(), fAC.As(facility)).ExpectEvent(eventType, 1.DaysAgo(), lOC.As("UAIEV"), fAC.As(facility)).ExpectEvent(eventType, 2.DaysAgo(), lOC.As("AUSYD"), fAC.As(facility)).VerifyExpectations();
		}

		#region Container Events Cancelled From Movements
		[TestDate(2014, 12, 1)]
		public void TestContainerEventsCancelledWhenMovementChangesVoyage()
		{
			var movement1 = ContainerStock1.Movements.AddNew();
			movement1.E9_OA_Depot = DepotAddress.PK;
			movement1.E9_JV = ImportVoyage.PK;
			movement1.E9_MovementDate = ZDateTime.Today;
			movement1.E9_MovementType = ContainerMovementTypes.Codes.Discharge;

			var shipment1 = Factory.New<AgencyShipment>();
			shipment1.JS_JX = ImportVoyage.Sailings[0].PK;

			var container1 = shipment1.RealContainers.AddNew();
			container1.JC_ContainerNum = ContainerStock1.R6_ContainerNum;

			var newVoyage = Factory.NewWithValidTestData<JobVoyage>();
			newVoyage.Origins.AddNew().JA_RL_NKPortOfLoading = "GBLON";
			newVoyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUMEL";
			newVoyage.GenerateSailings();

			var shipment2 = Factory.New<AgencyShipment>();
			shipment2.JS_JX = newVoyage.Sailings[0].PK;

			var container2 = shipment2.RealContainers.AddNew();
			container2.JC_ContainerNum = ContainerStock1.R6_ContainerNum;

			Factory.Save();

			var filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, AgencyShipmentContainerMovementEventSynchroniser.MovementTypeCodeEventMap[ContainerMovementTypes.Codes.Discharge]);
			var container1DischargeEvents = container1.Logs.Find(filter);

			AssertNotNull(container1DischargeEvents);
			Assert("Expected to find a discharge event", container1DischargeEvents.Length == 1);
			Assert("Expected the event to be active", !container1DischargeEvents[0].SL_IsCancelled);
			Assert("Expected not to find any discharge event on container 2", !container2.Logs.Find(filter).Any());

			movement1.E9_JV = newVoyage.PK;
			Factory.Save();

			var container2DischargeEvents = container2.Logs.Find(filter);
			AssertNotNull(container2DischargeEvents);
			Assert("Expected to find a discharge event", container2DischargeEvents.Length == 1);
			Assert("Expected the event to be active", !container2DischargeEvents[0].SL_IsCancelled);
			Assert("Expected the event on container 1 to be cancelled", container1DischargeEvents[0].SL_IsCancelled);
			movement1.E9_JV = ImportVoyage.PK;
			movement1.E9_JV = Factory.New<JobVoyage>().PK;

			Factory.Save();

			container2DischargeEvents = container2.Logs.Find(filter);
			Assert("Expected the event to now be cancelled as the movement no longer links to new voyage", container2DischargeEvents[0].SL_IsCancelled);
			container1DischargeEvents = container1.Logs.Find(filter);
			Assert("Expected still to only have one discharge event as we didn't save the change", container1DischargeEvents.Length == 1);
		}

		[TestDate(2014, 12, 1)]
		public void TestContainerEventsWithMultipleMovements()
		{
			var movement1 = ContainerStock1.Movements.AddNew();
			movement1.E9_OA_Depot = DepotAddress.PK;
			movement1.E9_JV = ImportVoyage.PK;
			movement1.E9_MovementDate = ZDateTime.Today;
			movement1.E9_MovementType = ContainerMovementTypes.Codes.Discharge;

			var shipment1 = Factory.New<AgencyShipment>();
			shipment1.JS_JX = ImportVoyage.Sailings[0].PK;

			var container1 = shipment1.RealContainers.AddNew();
			container1.JC_ContainerNum = ContainerStock1.R6_ContainerNum;

			Factory.Save();

			var filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, AgencyShipmentContainerMovementEventSynchroniser.MovementTypeCodeEventMap[ContainerMovementTypes.Codes.Discharge]);
			var container1DischargeEvents = container1.Logs.Find(filter);

			AssertNotNull(container1DischargeEvents);
			Assert("Expected to find a discharge event", container1DischargeEvents.Length == 1);
			Assert("Expected the event to be active", !container1DischargeEvents[0].SL_IsCancelled);

			var movement2 = ContainerStock1.Movements.AddNew();
			movement2.E9_OA_Depot = DepotAddress.PK;
			movement2.E9_JV = ImportVoyage.PK;
			movement2.E9_MovementDate = ZDateTime.Today.AddDays(1);
			movement2.E9_MovementType = ContainerMovementTypes.Codes.Discharge;

			Factory.Save();

			container1DischargeEvents = container1.Logs.Find(filter);

			AssertNotNull(container1DischargeEvents);
			Assert("Expected to find 2 discharge event", container1DischargeEvents.Length == 2);
			Assert("Expected the event to be active", !container1DischargeEvents[0].SL_IsCancelled);
			Assert("Expected the event to be active", !container1DischargeEvents[1].SL_IsCancelled);
		}

		#endregion
		#region Top Level Packs do not Update Movement Events
		public void TestMovementEvents_ContainerIsTopLevelPacksMode_DoNotUpdateMovementEvents()
		{
			var movement = ContainerStock1.Movements.AddNew();
			movement.E9_JV = ImportVoyage.PK;
			movement.E9_MovementDate = ZDateTime.Now.AddDays(-10);
			movement.E9_MovementType = AgencyShipmentContainerMovementEventSynchroniser.MovementTypeCodeEventMap.Keys.First();
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_JX = ImportVoyage.Sailings[0].PK;
			var container = shipment.RealContainers.AddNew();
			container.JC_ContainerNum = ContainerStock1.R6_ContainerNum;
			var filter = new ZQuery(StmALogSchema.SL_EventTime, movement.E9_MovementDate);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, AgencyShipmentContainerMovementEventSynchroniser.MovementTypeCodeEventMap[movement.E9_MovementType]);
			foreach (var mode in AgencyCargoTypeCodeDescriptionPairList.TopLevelPackCargoTypes)
			{
				container.JC_ContainerMode = mode;
				Factory.Save();
				AssertEquals(0, container.Logs.Find(filter).Length);
			}
		}

		#endregion
		#region Implementation
		RefContainerStock ContainerStock1
		{
			get
			{
				if (containerStock1 == null)
				{
					containerStock1 = Factory.NewWithValidTestData<RefContainerStock>();
					containerStock1.R6_ContainerNum = "TEST4100013";
					containerStock1.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				}

				return containerStock1;
			}
		}

		RefContainerStock containerStock1;
		JobVoyage ImportVoyage
		{
			get
			{
				if (importVoyage == null)
				{
					importVoyage = Factory.NewWithValidTestData<JobVoyage>();
					importVoyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
					importVoyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUMEL";
					importVoyage.GenerateSailings();
				}

				return importVoyage;
			}
		}

		JobVoyage importVoyage;
		OrgAddress DepotAddress
		{
			get
			{
				if (depotAddress == null)
				{
					var melbourneDepot = Factory.New<OrgHeader>();
					melbourneDepot.OH_Code = "MELDEPO";
					melbourneDepot.MainAddress.OA_RL_NKRelatedPortCode = "AUMEL";
					depotAddress = melbourneDepot.MainAddress;
				}

				return depotAddress;
			}
		}

		OrgAddress depotAddress;
		#endregion
		#region Types
		class TestCase
		{
			public TestCase(BusinessObjectFactory factory)
			{
				this.factory = factory;
				Setup();
			}

			public TestCase WithMovement(string movementType, ZDateTime date, string depot)
			{
				var movement = containerStock.Movements.AddNew();
				movement.E9_JV = voyage.PK;
				movement.E9_MovementDate = date;
				movement.E9_MovementType = movementType;
				movement.E9_OA_Depot = factory.NewWithValidTestData<OrgHeader>().MainAddress.With(oA_RL_NKRelatedPortCode: depot).PK;
				return this;
			}

			public TestCase WithEvent(Event eventType, ZDateTime date, params KeyValuePair<string, string>[] parameters)
			{
				container.Logs.AddNew(eventType, date.ToOffset(), parameters);
				SaveRelatedEvent(eventType);
				return this;
			}

			public TestCase WithEvent(Event eventType, ZDateTime date, string reference, params KeyValuePair<string, string>[] parameters)
			{
				container.Logs.AddNew(eventType, reference, date.ToOffset(), parameters);
				SaveRelatedEvent(eventType);
				return this;
			}

			public TestCase ExpectEvent(Event eventType, ZDateTime date, params KeyValuePair<string, string>[] parameters)
			{
				expectations.Add(new ExpectedEvent(eventType, date, string.Empty, parameters));
				SaveRelatedEvent(eventType);
				return this;
			}

			public TestCase ExpectEvent(Event eventType, ZDateTime date, string reference, params KeyValuePair<string, string>[] parameters)
			{
				expectations.Add(new ExpectedEvent(eventType, date, reference, parameters));
				SaveRelatedEvent(eventType);
				return this;
			}

			public void VerifyExpectations()
			{
				var synchronizer = new AgencyShipmentContainerMovementEventSynchroniser(container);
				synchronizer.UpdateContainerMovementEvents(null);
				var updatedEvents = container.Logs.Find(log => relatedEventCodes.Contains(log.SL_SE_NKEvent)).ToList();
				var expectedButNotFoundEvents = new StringBuilder();
				var foundButNotExpectedEvents = new StringBuilder();
				foreach (var expectedEvent in expectations)
				{
					var evnt = updatedEvents.FirstOrDefault(e => expectedEvent.IsMatching(e) && !e.IsCancelled);
					if (evnt != null)
					{
						updatedEvents.Remove(evnt);
					}
					else
					{
						expectedButNotFoundEvents.AppendLine(GetEventAsString(expectedEvent.EventType, expectedEvent.EventTime, expectedEvent.Reference, expectedEvent.Parameters));
					}
				}

				foreach (var evnt in updatedEvents)
				{
					if (evnt.IsCancelled)
					{
						continue;
					}

					foundButNotExpectedEvents.AppendLine(GetEventAsString(Events.All[evnt.SL_SE_NKEvent], evnt.SL_EventTime.ToDateTime(), evnt.ReferenceFreeText, evnt.Parameters.ToArray()));
				}

				var msg = new StringBuilder();
				if (expectedButNotFoundEvents.Length > 0)
				{
					msg.AppendLine();
					msg.AppendLine("The next events were expected, but not found:");
					msg.AppendLine(expectedButNotFoundEvents.ToString());
				}

				if (foundButNotExpectedEvents.Length > 0)
				{
					msg.AppendLine();
					msg.AppendLine("The next actuve events were found, but not expected:");
					msg.AppendLine(foundButNotExpectedEvents.ToString());
				}

				Assert(msg.ToString(), msg.Length == 0);
			}

			static string GetEventAsString(Event evnt, ZDateTime date, string reference, params KeyValuePair<string, string>[] parameters)
			{
				return string.Format("Event='{0}', EventTime='{1}', Reference='{2}'", evnt.Code, date, StmALog.GenerateEventReference(reference, parameters));
			}

			void SaveRelatedEvent(Event evnt)
			{
				if (!relatedEventCodes.Contains(evnt.Code))
				{
					relatedEventCodes.Add(evnt.Code);
				}
			}

			void Setup()
			{
				voyage = factory.NewWithValidTestData<JobVoyage>();
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUMEL";
				voyage.GenerateSailings();
				containerStock = factory.NewWithValidTestData<RefContainerStock>();
				shipment = factory.New<AgencyShipment>();
				shipment.JS_JX = voyage.Sailings[0].PK;
				container = shipment.RealContainers.AddNew();
				container.JC_ContainerNum = containerStock.R6_ContainerNum;
			}

			readonly BusinessObjectFactory factory;
			RefContainerStock containerStock;
			AgencyShipmentContainer container;
			AgencyShipment shipment;
			JobVoyage voyage;
			readonly List<string> relatedEventCodes = new List<string>();
			readonly List<ExpectedEvent> expectations = new List<ExpectedEvent>();
			#region Types
			class ExpectedEvent
			{
				public ExpectedEvent(Event evntType, ZDateTime date, string reference, params KeyValuePair<string, string>[] parameters)
				{
					EventType = evntType;
					EventTime = date;
					Reference = reference;
					Parameters = parameters;
				}

				public Event EventType { get; private set; }

				public ZDateTime EventTime { get; private set; }

				public string Reference { get; private set; }

				public KeyValuePair<string, string>[] Parameters { get; private set; }

				public bool IsMatching(StmALog log)
				{
					return log.SL_SE_NKEvent == EventType.Code && log.SL_EventTime == EventTime && (string.IsNullOrEmpty(log.ReferenceFreeText) && string.IsNullOrEmpty(Reference) || log.ReferenceFreeText == Reference) && log.Parameters.All(p => log.Parameters.ContainsKey(p.Key) && log.Parameters[p.Key] == p.Value);
				}
			}
			#endregion
		}
		#endregion
	}
}
