using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(OrderCopyVoyageEventsLogSubscriber))]
	sealed class OrderCopyVoyageEventsLogSubscriberTest : LogSubscriberTest<OrderCopyVoyageEventsLogSubscriber>
	{
		public void TestUpdateOrderMilestoneOnScheduleDepartureRaised()
		{
			Order.JD_JS = Shipment.PK;
			Shipment.JS_RL_NKOrigin = "AUSYD";
			Shipment.JS_RL_NKDestination = "HKHKG";
			Factory.Save();
			Factory.RefreshEnabled = false;

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			JobVoyage voyage = factory2.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "HKHKG";
			voyage.GenerateSailings();
			var consol = factory2.New<ForwardingConsol>();
			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "HKHKG";
			transport.JW_JX = voyage.Sailings[0].PK;
			var loadedShipment = factory2.Load<ForwardingShipment>(Shipment.PK);
			consol.Shipments.Add(loadedShipment);
			factory2.Save();

			factory2 = new BusinessObjectFactory();
			JobSailing sailing = factory2.Load<JobSailing>(transport.JW_JX);
			sailing.Origin.JA_E_DEP = new ZDateTime(2010, 03, 04);
			sailing.Origin.JA_A_DEP = new ZDateTime(2010, 03, 06);
			factory2.Save();

			var task = CreateEventsAndRunLogWalker(Events.Departure);
			AssertEquals(new ZDateTimeOffset(sailing.Origin.JA_E_DEP), new ZDateTimeOffset(task.P9_ScheduledDateInfo.PersistentValue));
			AssertEquals(new ZDateTimeOffset(sailing.Origin.JA_A_DEP), new ZDateTimeOffset(task.P9_ActualDateInfo.PersistentValue));

			transport = Factory.Load<Transport>(transport.PK);
			AssertNull("Precondition - no departure log exists yet", transport.Logs.MostRecentLogByEventTime(Events.Departure));
		}

		public void TestUpdateOrderMilestoneOnScheduleDepartureDateWhenEmpty()
		{
			Order.JD_JS = Shipment.PK;
			Order.JD_Milestone_A_DEP = ZDateTime.Today;
			Order.JD_Milestone_E_DEP = ZDateTime.Today.AddDays(12);

			Shipment.JS_RL_NKOrigin = "AUSYD";
			Shipment.JS_RL_NKDestination = "HKHKG";
			Factory.Save();
			Factory.RefreshEnabled = false;

			var factory2 = new BusinessObjectFactory();
			var voyage = factory2.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "HKHKG";
			voyage.GenerateSailings();
			var consol = factory2.New<ForwardingConsol>();
			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "HKHKG";
			transport.JW_JX = voyage.Sailings[0].PK;
			var loadedShipment = factory2.Load<ForwardingShipment>(Shipment.PK);
			consol.Shipments.Add(loadedShipment);
			factory2.Save();

			factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var sailing = factory2.Load<JobSailing>(transport.JW_JX);
			sailing.Origin.JA_E_DEP = ZDateTime.Empty;
			sailing.Origin.JA_A_DEP = ZDateTime.Empty;
			sailing.Destination.JB_E_ARV = ZDateTime.Today;

			factory2.Save();

			var task = CreateEventsAndRunLogWalker(Events.Departure);
			AssertEquals(ZDateTimeOffset.Empty, new ZDateTimeOffset(task.P9_ScheduledDateInfo.PersistentValue));
			AssertEquals(ZDateTimeOffset.Empty, new ZDateTimeOffset(task.P9_ActualDateInfo.PersistentValue));
		}

		public void TestUpdateOrderMilestoneOnScheduleArrivalDateWhenEmpty()
		{
			Order.JD_JS = Shipment.PK;
			Order.JD_Milestone_A_ARV = ZDateTime.Empty;
			Order.JD_Milestone_E_ARV = ZDateTime.Today.AddDays(11);

			Shipment.JS_RL_NKOrigin = "AUSYD";
			Shipment.JS_RL_NKDestination = "HKHKG";
			Factory.Save();
			Factory.RefreshEnabled = false;

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var voyage = factory2.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "HKHKG";
			voyage.GenerateSailings();
			var consol = factory2.New<ForwardingConsol>();
			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "HKHKG";
			transport.JW_JX = voyage.Sailings[0].PK;
			var loadedShipment = factory2.Load<ForwardingShipment>(Shipment.PK);
			consol.Shipments.Add(loadedShipment);
			factory2.Save();

			factory2 = new BusinessObjectFactory();
			JobVoyage loadedVoyage = factory2.Load<JobVoyage>(voyage.PK);
			loadedVoyage.Origins[0].JA_A_DEP = ZDateTime.Today;
			loadedVoyage.Destinations[0].JB_E_ARV = ZDateTime.Empty;
			loadedVoyage.Destinations[0].JB_A_ARV = ZDateTime.Empty;
			factory2.Save();

			var task = CreateEventsAndRunLogWalker(Events.Arrival);
			AssertEquals(ZDateTimeOffset.Empty, new ZDateTimeOffset(task.P9_ScheduledDateInfo.PersistentValue));
			AssertEquals(ZDateTimeOffset.Empty, new ZDateTimeOffset(task.P9_ActualDateInfo.PersistentValue));
		}

		public void TestUpdateOrderMilestoneOnScheduleArrivalRaised()
		{
			Order.JD_JS = Shipment.PK;
			Shipment.JS_RL_NKOrigin = "AUSYD";
			Shipment.JS_RL_NKDestination = "HKHKG";
			Factory.Save();
			Factory.RefreshEnabled = false;

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			JobVoyage voyage = factory2.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "HKHKG";
			voyage.GenerateSailings();
			var consol = factory2.New<ForwardingConsol>();
			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "HKHKG";
			transport.JW_JX = voyage.Sailings[0].PK;
			var loadedShipment = factory2.Load<ForwardingShipment>(Shipment.PK);
			consol.Shipments.Add(loadedShipment);
			factory2.Save();

			factory2 = new BusinessObjectFactory();
			JobVoyage loadedVoyage = factory2.Load<JobVoyage>(voyage.PK);
			loadedVoyage.Destinations[0].JB_E_ARV = new ZDateTime(2010, 03, 04);
			loadedVoyage.Destinations[0].JB_A_ARV = new ZDateTime(2010, 03, 06);
			factory2.Save();

			var task = CreateEventsAndRunLogWalker(Events.Arrival);
			AssertEquals(new ZDateTimeOffset(loadedVoyage.Destinations[0].JB_E_ARV), new ZDateTimeOffset(task.P9_ScheduledDateInfo.PersistentValue));
			AssertEquals(new ZDateTimeOffset(loadedVoyage.Destinations[0].JB_A_ARV), new ZDateTimeOffset(task.P9_ActualDateInfo.PersistentValue));

			transport = Factory.Load<Transport>(transport.PK);
			AssertNull("Precondition - no arrival log exists yet", transport.Logs.MostRecentLogByEventTime(Events.Arrival));
		}

		public void TestUpdateOrderMilestoneOnDepartureRaised_FromDeclarationTransport()
		{
			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.AU.IJobDeclaration>();
			declaration[JobDeclarationSchema.Constants.JE_RL_NKOrigin] = "AUSYD";
			declaration[JobDeclarationSchema.Constants.JE_RL_NKFinalDestination] = "USCHI";

			Order.JD_JE = declaration.PK;

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var voyage = factory2.NewWithValidTestData<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USCHI";
			voyage.GenerateSailings();

			var loadedDeclaration = factory2.Load<Enterprise.Integration.Customs.AU.IJobDeclaration>(declaration.PK);
			var transport = ((IRoutingSupport)loadedDeclaration).Transports.AddNew();
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "USCHI";
			transport.JW_IsLinked = true;
			transport.JW_JX = voyage.Sailings[0].PK;
			factory2.Save();

			factory2 = new BusinessObjectFactory();
			JobVoyage loadedVoyage = factory2.Load<JobVoyage>(voyage.PK);
			loadedVoyage.Origins[0].JA_E_DEP = new ZDateTime(2013, 12, 14);
			loadedVoyage.Origins[0].JA_A_DEP = new ZDateTime(2013, 12, 17);
			factory2.Save();

			var task = CreateEventsAndRunLogWalker(Events.Departure);
			AssertEquals("Estimated Scheduled date picks up date from Date at origin.", ZDateTimeOffset.Empty, new ZDateTimeOffset(task.P9_ScheduledDateInfo.PersistentValue));
			AssertEquals("Actual date gets updated from transport.", new ZDateTimeOffset(loadedVoyage.Origins[0].JA_A_DEP), new ZDateTimeOffset(task.P9_ActualDateInfo.PersistentValue));

			transport = Factory.Load<Transport>(transport.PK);
			AssertNull("Precondition - no departure log exists yet", transport.Logs.MostRecentLogByEventTime(Events.Departure));
		}

		public void TestUpdateOrderMilestoneOnArrivalRaised_FromDeclarationTransport()
		{
			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.AU.IJobDeclaration>();
			Order.JD_JE = declaration.PK;

			declaration[JobDeclarationSchema.Constants.JE_RL_NKOrigin] = "AUSYD";
			declaration[JobDeclarationSchema.Constants.JE_RL_NKFinalDestination] = "HKHKG";

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			JobVoyage voyage = factory2.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "HKHKG";
			voyage.GenerateSailings();

			var loadedDeclaration = factory2.Load<Enterprise.Integration.Customs.AU.IJobDeclaration>(declaration.PK);
			var transport = ((IRoutingSupport)loadedDeclaration).Transports.AddNew();
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "HKHKG";
			transport.JW_IsLinked = true;
			transport.JW_JX = voyage.Sailings[0].PK;
			factory2.Save();

			factory2 = new BusinessObjectFactory();
			JobVoyage loadedVoyage = factory2.Load<JobVoyage>(voyage.PK);
			loadedVoyage.Destinations[0].JB_E_ARV = new ZDateTime(2013, 12, 14);
			loadedVoyage.Destinations[0].JB_A_ARV = new ZDateTime(2013, 12, 16);
			factory2.Save();

			var task = CreateEventsAndRunLogWalker(Events.Arrival);
			AssertEquals("Estimated Scheduled date picks up date from Date at final destination.", ZDateTimeOffset.Empty, new ZDateTimeOffset(task.P9_ScheduledDateInfo.PersistentValue));
			AssertEquals("Actual date gets updated from transport.", new ZDateTimeOffset(loadedVoyage.Destinations[0].JB_A_ARV), new ZDateTimeOffset(task.P9_ActualDateInfo.PersistentValue));

			transport = Factory.Load<Transport>(transport.PK);
			AssertNull("Precondition - no departure log exists yet", transport.Logs.MostRecentLogByEventTime(Events.Arrival));
		}

		public void TestDoNotUpdateTriggerDates()
		{
			var trigger = Order.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Dep Trigger";
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.DepartureCode;

			Order.JD_JS = Shipment.PK;
			Shipment.JS_RL_NKOrigin = "AUSYD";
			Shipment.JS_RL_NKDestination = "HKHKG";
			Factory.Save();
			Factory.RefreshEnabled = false;

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;

			var voyage = factory2.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "HKHKG";
			voyage.GenerateSailings();

			var consol = factory2.New<ForwardingConsol>();

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "HKHKG";
			transport.JW_JX = voyage.Sailings[0].PK;

			var loadedShipment = factory2.Load<ForwardingShipment>(Shipment.PK);
			consol.Shipments.Add(loadedShipment);
			factory2.Save();

			var factory3 = new BusinessObjectFactory();
			JobSailing sailing = factory3.Load<JobSailing>(transport.JW_JX);
			sailing.Origin.JA_E_DEP = new ZDateTime(2010, 03, 04);
			sailing.Origin.JA_A_DEP = new ZDateTime(2010, 03, 06);
			factory3.Save();

			AssertNoExceptionThrown(() => CreateEventsAndRunLogWalker(Events.Departure));
		}

		#region Implementation

		protected override bool IKnowEDTEventsAreUsuallyOnlyLoggedWhenAFormIsPresent
		{
			get { return true; }
		}

		ProcessTask CreateEventsAndRunLogWalker(Event ev)
		{
			Factory.Save();

			ProcessTask task = Order.WorkflowItems.Milestones[ev];
			if (task == null)
			{
				task = ((IWorkflowProvider)Order).WorkflowItems.Triggers.AddNew();
				task.TriggerConditions.TriggerEventCode = ev.Code;
			}

			Factory.Save();
			RunLogWalkerCycleForTest();
			task.Reload();
			return task;
		}

		ForwardingShipment Shipment
		{
			get
			{
				if (shipment == null)
				{
					shipment = Factory.New<ForwardingShipment>();
				}
				return shipment;
			}
		}
		ForwardingShipment shipment;

		Order Order
		{
			get
			{
				if (order == null)
				{
					order = Factory.NewWithValidTestData<Order>();
				}
				return order;
			}
		}
		Order order;

		#endregion
	}
}
