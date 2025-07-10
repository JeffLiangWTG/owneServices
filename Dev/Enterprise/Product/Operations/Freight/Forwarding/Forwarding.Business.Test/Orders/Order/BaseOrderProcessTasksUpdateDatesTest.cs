using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	public abstract class BaseOrderProcessTasksUpdateDatesTest : TestCaseWithFactory
	{
		public void TestUpdateDatesNoExceptionThrownWhenParentIsNull()
		{
			string[] events = { Events.CustomsClearedCode, Events.CustomsCommencedCode,
								 Events.DepartureCode, Events.ArrivalCode,
								 Events.CargoAvailableCode, Events.DeliveryCartageAdvisedCode,
								 Events.DeliveryCartageCompleteFinalisedCode };

			foreach (string eventType in events)
			{
				OrderProcessTasks task = Factory.New<OrderProcessTasks>();
				task.P9_Type = Constants.Workflow.WorkflowTriggerType;
				task.TriggerConditions.TriggerEventCode = eventType;

				AssertNoExceptionThrown(task.UpdateScheduledDate);
			}
		}

		protected OrderProcessTasks GetMilestone(Event eventType)
		{
			OrderProcessTasks result = Order.WorkflowItems.Milestones[eventType] as OrderProcessTasks;

			if (result == null)
			{
				result = Order.WorkflowItems.Milestones.AddNew() as OrderProcessTasks;
				result.TriggerConditions.TriggerEventCode = eventType.Code;
			}

			return result;
		}

		#region Implementation

		protected Order Order
		{
			get { return order ?? (order = Factory.NewWithValidTestData<Order>()); }
		}
		Order order;

		protected ForwardingShipment shipment;
		protected CommonConsol consol;
		protected Transport transport;
		protected EnterpriseBusinessObject declaration;

		protected static ZDateTime TestOrderDate = new ZDateTime(2016, 1, 1);
		protected static ZDateTime TestShipmentDate = new ZDateTime(2016, 12, 31);
		protected static ZDateTime TestNewShipmentDate = new ZDateTime(2016, 12, 25);
		protected static ZDateTime TestDeclarationDate = new ZDateTime(2016, 12, 21);
		protected static ZDateTime TestTaskDate = new ZDateTime(2016, 4, 20);

		protected override void SetUp()
		{
			base.SetUp();
			Order.JD_OrderNumber = "123";
			Order.BuyerPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			shipment = Factory.New<ForwardingShipment>();
			consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			transport = consol.Transports[0];
			declaration = (EnterpriseBusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(Order);
		}

		protected ForwardingConsol CreateConsolWithTransports()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			Transport transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "CNSHA";
			transport1.JW_ETA = TestTaskDate;
			transport1.JW_ETD = TestTaskDate.AddDays(2).ToZDateTime();
			transport1.JW_VoyageFlight = "057";
			transport1.JW_Vessel = "TESTVESSEL1";
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			Transport transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "CNSHA";
			transport2.JW_RL_NKDiscPort = "INBOM";
			transport2.JW_ETA = TestTaskDate.AddDays(3).ToZDateTime();
			transport2.JW_ETD = TestTaskDate.AddDays(8).ToZDateTime();
			transport2.JW_VoyageFlight = "058";
			transport2.JW_Vessel = "TESTVESSEL2";
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			Transport transport3 = consol.Transports.AddNew();
			transport3.JW_RL_NKLoadPort = "INBOM";
			transport3.JW_RL_NKDiscPort = "AUMEL";
			transport3.JW_ETA = TestTaskDate.AddDays(9).ToZDateTime();
			transport3.JW_ETD = TestTaskDate.AddDays(12).ToZDateTime();
			transport3.JW_VoyageFlight = "059";
			transport3.JW_Vessel = "TESTVESSEL3";
			transport3.JW_TransportType = Core.Constants.TransportPlanningType.Other;

			return consol;
		}
		#endregion
	}
}
