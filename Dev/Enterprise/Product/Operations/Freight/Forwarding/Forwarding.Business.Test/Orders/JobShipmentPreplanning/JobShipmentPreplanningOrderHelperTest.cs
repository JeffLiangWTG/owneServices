using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class JobShipmentPreplanningOrderHelperTest : TestCaseWithFactory
	{
		public void TestSetValuesOnOrder()
		{
			JobShipmentPreplanning preAdvice = CreatePreAdviceWithRoutings(true, true);

			Order order = preAdvice.Orders.AddNew();
			preAdvice.SetValuesOnOrder(order);

			AssertEquals("1234", order.JD_MasterWaybill);
			AssertEquals(Org1.PK, order.JD_OH_ReceivingAgent);
			AssertEquals(Org2.PK, order.JD_OH_SendingAgent);
			AssertEquals(Org3.PK, order.JD_OH_Carrier);
			AssertEquals(Org4.PK, order.BuyerPK);
			AssertEquals(ZGuid.Empty, order.JD_JS);
			AssertEquals(ZGuid.Empty, order.JD_JE);
			AssertEquals("AUSYD", order.JD_RL_NKPortOfLoading);
			AssertEquals("USLAX", order.JD_RL_NKPortOfDischarge);
			AssertEquals("AUSYD", order.JD_RL_NKGoodsAvailableAt);
			AssertEquals("USLAX", order.JD_RL_NKGoodsDeliveredTo);
			AssertEquals("435324", order.JD_Waybill);
			AssertEquals("TEST1", order.PlannedContainers[0].J1_ContainerNumber);
			AssertEquals("TEST2", order.PlannedContainers[1].J1_ContainerNumber);

			AssertEquals("123456", order.JD_DepartureVoyage);
			AssertEquals("ABC", order.JD_RV_NKDepartureVessel);

			AssertEquals("9999", order.JD_ArrivalVoyage);
			AssertEquals("ZUB", order.JD_RV_NKArrivalVessel);

			AssertEquals("1111", order.JD_IntermediateVoyage);
			AssertEquals("RAK", order.JD_RV_NKIntermediateVessel);

			AssertEquals(DepartureTransport.JW_ETD, order.GetMilestoneEstimatedDate(Events.Departure).ToZDateTime());
			AssertEquals(DepartureTransport.JW_ETA, order.JD_E_ARV_1stIntermediate);
			AssertEquals(ArrivalRouting.JW_ETD, order.JD_E_DEP_3);
			AssertEquals(ArrivalRouting.JW_ETA, order.GetMilestoneEstimatedDate(Events.Arrival).ToZDateTime());

			AssertEquals(DepartureTransport.JW_ATD, order.GetMilestoneActualDate(Events.Departure).ToZDateTime());
			AssertEquals(ArrivalRouting.JW_ATA, order.GetMilestoneActualDate(Events.Arrival).ToZDateTime());

			AssertEquals(OtherRouting.JW_ETD, order.JD_E_DEP_2);
			AssertEquals(OtherRouting.JW_ETA, order.JD_E_ARV_2ndIntermediate);

			Factory.Save();

			preAdvice.EF_JE = Dec.PK;
			preAdvice.EF_HouseBill = "33333";
			preAdvice.SetValuesOnOrder(order);
			AssertEquals("Changed as they were the same originall", "33333", order.JD_Waybill);
			AssertEquals(ZGuid.Empty, order.JD_JS);
			AssertEquals(Dec.PK, order.JD_JE);

			Factory.Save();

			order.JD_Waybill = "919191";
			Factory.Save();

			preAdvice.EF_HouseBill = "91111111111";
			preAdvice.SetValuesOnOrder(order);
			AssertEquals("NOT Changed as they were NOT same originaly", "919191", order.JD_Waybill);
		}

		public void TestSetValuesOnOrder_ContainerFieldsArePopulated()
		{
			var preAdvice = CreatePreAdviceWithRoutings(false, false);
			var order = preAdvice.Orders.AddNew();
			Factory.Save();

			var container1 = order.PlannedContainers.OfType<OrderContainer>().Single(x => x.J1_ContainerNumber == "TEST1");
			CombineAssertions(() => {
				AssertEquals("TEST1", container1.J1_ContainerNumber);
				AssertEquals("TEST1_SEAL1", container1.J1_SealNum);
				AssertEquals("TEST1_SEAL2", container1.J1_AdditionalSealNum);
				AssertEquals("TEST1_SEAL3", container1.J1_Additional2SealNum);
				AssertEquals((short)1, container1.J1_ContainerCount);
				AssertEquals(TestContainerType, container1.J1_RC);
			});

			preAdvice.Containers.OfType<OrderContainer>().Single(x => x.J1_ContainerNumber == "TEST1").J1_SealNum += "_X";
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			container1 = newFactory.LoadTop1<OrderContainer>(new ZQuery(JobOrderContainerSchema.PK, container1.PK));
			CombineAssertions(() => {
				AssertEquals("TEST1", container1.J1_ContainerNumber);
				AssertEquals("Using a new factory to make sure the changes is saved in DB",
					"TEST1_SEAL1_X", container1.J1_SealNum);
				AssertEquals("TEST1_SEAL2", container1.J1_AdditionalSealNum);
				AssertEquals("TEST1_SEAL3", container1.J1_Additional2SealNum);
				AssertEquals((short)1, container1.J1_ContainerCount);
				AssertEquals(TestContainerType, container1.J1_RC);
			});
		}

		public void TestSetValuesOnOrder_DeletingPreAdviceContainersUpdatesOrder()
		{
			var preAdvice = CreatePreAdviceWithRoutings(false, false);
			var order = preAdvice.Orders.AddNew();
			AssertEquals(2, order.PlannedContainers.Count);

			preAdvice.Containers.RemoveAndDelete(preAdvice.Containers[0]);
			Factory.Save();
			AssertEquals(1, order.PlannedContainers.Count);

			preAdvice.Containers.RemoveAndDeleteAll();
			Factory.Save();
			AssertEquals(0, order.PlannedContainers.Count);
		}

		public void TestDepartureEstimatedDateOnOrderGetsEmpty()
		{
			var order = Factory.NewWithValidTestData<Order>();
			Factory.Save();

			var notifier = new JobShipmentPreplanningTest.TestNotificationSubscriber();
			order.CreateAndLinkPreAdviceToOrder(notifier, Order.TargetObjectForCreate.PreAdvice);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			var transport = consol.Transports.AddNew();
			transport.JW_ETD = new ZDateTime(2013, 06, 28);
			order.JD_JS = shipment.PK;

			Factory.Save();

			AssertEquals(new ZDateTime(2013, 06, 28), order.JD_Milestone_E_DEP);
		}

		public void TestEstimatedDatesDefaultToOrderFromPreAdvice()
		{
			var preAdvice = CreatePreAdviceWithRoutings(false, false);
			var order = preAdvice.Orders.AddNew();
			preAdvice.SetValuesOnOrder(order);

			AssertEquals(new ZDateTime(2007, 1, 4), order.JD_Milestone_E_DEP);
			AssertEquals(new ZDateTime(2007, 1, 5), order.JD_Milestone_E_ARV);

			DepartureTransport.JW_ETA = new ZDateTime(2013, 1, 5);
			DepartureTransport.JW_ETD = new ZDateTime(2013, 1, 4);

			preAdvice.SetValuesOnOrder(order);

			AssertEquals(new ZDateTime(2013, 1, 4), order.JD_Milestone_E_DEP);
			AssertEquals(new ZDateTime(2013, 1, 5), order.JD_Milestone_E_ARV);
		}

		public void TestActualAndEstimatedDatesAreOnlyPropagatedToOrderWhenChanged()
		{
			var preAdvice = CreatePreAdviceWithLinkedTransport();
			var order = preAdvice.Orders.AddNew();
			preAdvice.SetValuesOnOrder(order);

			AssertEquals(new ZDateTime(2007, 1, 2), order.JD_Milestone_E_DEP);
			AssertEquals(new ZDateTime(2007, 1, 3), order.JD_Milestone_A_DEP);
			AssertEquals(new ZDateTime(2007, 1, 4), order.JD_Milestone_E_ARV);
			AssertEquals(new ZDateTime(2007, 1, 5), order.JD_Milestone_A_ARV);

			order.JD_Milestone_E_DEP = new ZDateTime(2007, 1, 15);
			order.JD_Milestone_A_DEP = new ZDateTime(2007, 1, 16);
			order.JD_Milestone_E_ARV = new ZDateTime(2007, 1, 17);
			order.JD_Milestone_A_ARV = new ZDateTime(2007, 1, 18);

			preAdvice.SetValuesOnOrder(order);

			AssertEquals("Order E_DEP is not updated as pre-advice date has not changed", new ZDateTime(2007, 1, 15), order.JD_Milestone_E_DEP);
			AssertEquals("Order A_DEP is not updated as pre-advice date has not changed", new ZDateTime(2007, 1, 16), order.JD_Milestone_A_DEP);
			AssertEquals("Order E_ARV is not updated as pre-advice date has not changed", new ZDateTime(2007, 1, 17), order.JD_Milestone_E_ARV);
			AssertEquals("Order A_ARV is not updated as pre-advice date has not changed", new ZDateTime(2007, 1, 18), order.JD_Milestone_A_ARV);

			preAdvice.PreAdviceTransports[0].JW_ETD = new ZDateTime(2007, 1, 25);
			preAdvice.PreAdviceTransports[0].JW_ATD = new ZDateTime(2007, 1, 26);
			preAdvice.PreAdviceTransports[0].JW_ETA = new ZDateTime(2007, 1, 27);
			preAdvice.PreAdviceTransports[0].JW_ATA = new ZDateTime(2007, 1, 28);

			preAdvice.SetValuesOnOrder(order);

			AssertEquals("Order E_DEP is updated as pre-advice date has changed", new ZDateTime(2007, 1, 25), order.JD_Milestone_E_DEP);
			AssertEquals("Order A_DEP is updated as pre-advice date has changed", new ZDateTime(2007, 1, 26), order.JD_Milestone_A_DEP);
			AssertEquals("Order E_ARV is updated as pre-advice date has changed", new ZDateTime(2007, 1, 27), order.JD_Milestone_E_ARV);
			AssertEquals("Order A_ARV is updated as pre-advice date has changed", new ZDateTime(2007, 1, 28), order.JD_Milestone_A_ARV);

			Factory.Save();

			var reloadedOrder = Factory.Load<Order>(order.PK);

			reloadedOrder.JD_Milestone_E_DEP = new ZDateTime(2007, 1, 10);
			reloadedOrder.JD_Milestone_A_DEP = new ZDateTime(2007, 1, 11);
			reloadedOrder.JD_Milestone_E_ARV = new ZDateTime(2007, 1, 12);
			reloadedOrder.JD_Milestone_A_ARV = new ZDateTime(2007, 1, 13);

			reloadedOrder.PreAdvice.SetValuesOnOrder(reloadedOrder);

			AssertEquals("Order E_DEP is not updated as pre-advice date has not changed", new ZDateTime(2007, 1, 10), reloadedOrder.JD_Milestone_E_DEP);
			AssertEquals("Order A_DEP is not updated as pre-advice date has not changed", new ZDateTime(2007, 1, 11), reloadedOrder.JD_Milestone_A_DEP);
			AssertEquals("Order E_ARV is not updated as pre-advice date has not changed", new ZDateTime(2007, 1, 12), reloadedOrder.JD_Milestone_E_ARV);
			AssertEquals("Order A_ARV is not updated as pre-advice date has not changed", new ZDateTime(2007, 1, 13), reloadedOrder.JD_Milestone_A_ARV);

			reloadedOrder.PreAdvice.PreAdviceTransports[0].JW_ETD = new ZDateTime(2007, 1, 20);
			reloadedOrder.PreAdvice.PreAdviceTransports[0].JW_ATD = new ZDateTime(2007, 1, 21);
			reloadedOrder.PreAdvice.PreAdviceTransports[0].JW_ETA = new ZDateTime(2007, 1, 22);
			reloadedOrder.PreAdvice.PreAdviceTransports[0].JW_ATA = new ZDateTime(2007, 1, 23);

			reloadedOrder.PreAdvice.SetValuesOnOrder(reloadedOrder);

			AssertEquals("Order E_DEP is updated as pre-advice date has changed", new ZDateTime(2007, 1, 20), reloadedOrder.JD_Milestone_E_DEP);
			AssertEquals("Order A_DEP is updated as pre-advice date has changed", new ZDateTime(2007, 1, 21), reloadedOrder.JD_Milestone_A_DEP);
			AssertEquals("Order E_ARV is updated as pre-advice date has changed", new ZDateTime(2007, 1, 22), reloadedOrder.JD_Milestone_E_ARV);
			AssertEquals("Order A_ARV is updated as pre-advice date has changed", new ZDateTime(2007, 1, 23), reloadedOrder.JD_Milestone_A_ARV);
		}

		public void TestSetValuesOnOrder_GoodsPorts()
		{
			JobShipmentPreplanning preAdvice = Factory.New<JobShipmentPreplanning>();
			preAdvice.EF_RL_NKPortLoad = "AUSYD";
			preAdvice.EF_RL_NKPortDisch = "USLAX";

			Order order = preAdvice.Orders.AddNew();
			preAdvice.SetValuesOnOrder(order);

			AssertEquals("Defaulted from pre-advice", "AUSYD", order.JD_RL_NKGoodsAvailableAt);
			AssertEquals("Defaulted from pre-advice", "USLAX", order.JD_RL_NKGoodsDeliveredTo);

			order.JD_RL_NKGoodsAvailableAt = "NZAKL";
			order.JD_RL_NKGoodsDeliveredTo = "DEFRA";
			preAdvice.SetValuesOnOrder(order);

			AssertEquals("Non-empty property wasn't overriden", "NZAKL", order.JD_RL_NKGoodsAvailableAt);
			AssertEquals("Non-empty property wasn't overriden", "DEFRA", order.JD_RL_NKGoodsDeliveredTo);

			order.JD_RL_NKPortOfLoading = "NZAKL";
			order.JD_RL_NKPortOfDischarge = "DEFRA";
			preAdvice.SetValuesOnOrder(order);

			AssertEquals("Defaulted from pre-advice", "AUSYD", order.JD_RL_NKGoodsAvailableAt);
			AssertEquals("Defaulted from pre-advice", "USLAX", order.JD_RL_NKGoodsDeliveredTo);
		}

		public void TestSetValuesOnOrder_BlankVessels()
		{
			JobShipmentPreplanning preAdvice = CreatePreAdviceWithRoutings(true, false);

			Order order = preAdvice.Orders.AddNew();
			preAdvice.SetValuesOnOrder(order);

			AssertEquals("", order.JD_RV_NKDepartureVessel);
			AssertEquals("", order.JD_RV_NKArrivalVessel);
			AssertEquals("", order.JD_RV_NKIntermediateVessel);

			order.JD_RV_NKDepartureVessel = "Zubin";
			Factory.Save();

			AssertEquals("Zubin", order.JD_RV_NKDepartureVessel);
			AssertEquals("", order.JD_RV_NKArrivalVessel);
			AssertEquals("", order.JD_RV_NKIntermediateVessel);
		}

		public void TestSetValuesOnOrder_RoutingValuesForSingleRoute()
		{
			JobShipmentPreplanning preAdvice = CreatePreAdviceWithRoutings(false, true);

			Order order = preAdvice.Orders.AddNew();
			preAdvice.SetValuesOnOrder(order);

			AssertEquals("1234", order.JD_MasterWaybill);
			AssertEquals(Org1.PK, order.JD_OH_ReceivingAgent);
			AssertEquals(Org2.PK, order.JD_OH_SendingAgent);
			AssertEquals(Org3.PK, order.JD_OH_Carrier);
			AssertEquals(Org4.PK, order.BuyerPK);
			AssertEquals(ZGuid.Empty, order.JD_JS);
			AssertEquals(ZGuid.Empty, order.JD_JE);
			AssertEquals("AUSYD", order.JD_RL_NKPortOfLoading);
			AssertEquals("USLAX", order.JD_RL_NKPortOfDischarge);
			AssertEquals("AUSYD", order.JD_RL_NKGoodsAvailableAt);
			AssertEquals("USLAX", order.JD_RL_NKGoodsDeliveredTo);
			AssertEquals("435324", order.JD_Waybill);

			AssertEquals("123456", order.JD_DepartureVoyage);
			AssertEquals("ABC", order.JD_RV_NKDepartureVessel);

			AssertEquals("123456", order.JD_ArrivalVoyage);
			AssertEquals("ABC", order.JD_RV_NKArrivalVessel);

			AssertEquals("", order.JD_IntermediateVoyage);
			AssertEquals("", order.JD_RV_NKIntermediateVessel);

			AssertEquals(DepartureTransport.JW_ETD, order.GetMilestoneEstimatedDate(Events.Departure).ToZDateTime());
			AssertEquals(DepartureTransport.JW_ETA, order.GetMilestoneEstimatedDate(Events.Arrival).ToZDateTime());

			AssertEquals(DepartureTransport.JW_ATD, order.GetMilestoneActualDate(Events.Departure).ToZDateTime());
			AssertEquals(DepartureTransport.JW_ATA, order.GetMilestoneActualDate(Events.Arrival).ToZDateTime());

			AssertEquals(ZDateTime.Empty, order.JD_E_DEP_2);
			AssertEquals(ZDateTime.Empty, order.JD_E_ARV_2ndIntermediate);
		}

		public void TestSetValuesOnOrder_2Routings()
		{
			//Pre-Advice

			Org1 = Factory.NewWithValidTestData<OrgHeader>();
			Org2 = Factory.NewWithValidTestData<OrgHeader>();
			Org3 = Factory.NewWithValidTestData<OrgHeader>();
			Org4 = Factory.NewWithValidTestData<OrgHeader>();

			Dec = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));

			JobShipmentPreplanning preAdvice = Factory.New<JobShipmentPreplanning>();
			preAdvice.EF_MasterBill = "1234";
			preAdvice.EF_OH_ReceivingAgent = Org1.PK;
			preAdvice.EF_OH_SendingAgent = Org2.PK;
			preAdvice.EF_OH_Carrier = Org3.PK;
			preAdvice.EF_RL_NKPortLoad = "AUSYD";
			preAdvice.EF_RL_NKPortDisch = "USLAX";
			preAdvice.EF_HouseBill = "435324";
			preAdvice.BuyerPK = Org4.PK;

			Transport mainTransport = preAdvice.PreAdviceTransports[0];
			SetupTransport(mainTransport, "AUSYD", "SGSIN", "SEA", Core.Constants.TransportPlanningType.MainVessel, "123456", "ABC", new ZDateTime(2007, 1, 5));

			Transport arrivalTransport = preAdvice.PreAdviceTransports.AddNew();
			SetupTransport(arrivalTransport, "SGSIN", "USLAX", "SEA", Core.Constants.TransportPlanningType.Other, "9999", "DEF", new ZDateTime(2007, 1, 7));

			OrderContainer container1 = preAdvice.Containers.AddNew();
			container1.J1_RC = new ZGuid();
			container1.J1_ContainerNumber = "TEST1";
			OrderContainer container2 = preAdvice.Containers.AddNew();
			container2.J1_RC = new ZGuid();
			container2.J1_ContainerNumber = "TEST2";
			Factory.Save();

			//Order

			Order order = preAdvice.Orders.AddNew();
			preAdvice.SetValuesOnOrder(order);

			AssertEquals("1234", order.JD_MasterWaybill);
			AssertEquals(Org1.PK, order.JD_OH_ReceivingAgent);
			AssertEquals(Org2.PK, order.JD_OH_SendingAgent);
			AssertEquals(Org3.PK, order.JD_OH_Carrier);
			AssertEquals(Org4.PK, order.BuyerPK);
			AssertEquals(ZGuid.Empty, order.JD_JS);
			AssertEquals(ZGuid.Empty, order.JD_JE);
			AssertEquals("AUSYD", order.JD_RL_NKPortOfLoading);
			AssertEquals("USLAX", order.JD_RL_NKPortOfDischarge);
			AssertEquals("AUSYD", order.JD_RL_NKGoodsAvailableAt);
			AssertEquals("USLAX", order.JD_RL_NKGoodsDeliveredTo);
			AssertEquals("435324", order.JD_Waybill);
			AssertEquals("TEST1", order.PlannedContainers[0].J1_ContainerNumber);
			AssertEquals("TEST2", order.PlannedContainers[1].J1_ContainerNumber);

			AssertEquals("123456", order.JD_DepartureVoyage);
			AssertEquals("ABC", order.JD_RV_NKDepartureVessel);

			AssertEquals("", order.JD_IntermediateVoyage);
			AssertEquals("", order.JD_RV_NKIntermediateVessel);

			AssertEquals("9999", order.JD_ArrivalVoyage);
			AssertEquals("DEF", order.JD_RV_NKArrivalVessel);

			AssertEquals(mainTransport.JW_ETD, order.GetMilestoneEstimatedDate(Events.Departure).ToZDateTime());
			AssertEquals(mainTransport.JW_ETA, order.JD_E_ARV_1stIntermediate);

			AssertEquals(ZDateTime.Empty, order.JD_E_DEP_2);
			AssertEquals(ZDateTime.Empty, order.JD_E_ARV_2ndIntermediate);

			AssertEquals(arrivalTransport.JW_ETD, order.JD_E_DEP_3);
			AssertEquals(arrivalTransport.JW_ETA, order.GetMilestoneEstimatedDate(Events.Arrival).ToZDateTime());

			AssertEquals(mainTransport.JW_ATD, order.GetMilestoneActualDate(Events.Departure).ToZDateTime());
			AssertEquals(arrivalTransport.JW_ATA, order.GetMilestoneActualDate(Events.Arrival).ToZDateTime());

			//user error, make 1 routing leg
			preAdvice.PreAdviceTransports.Remove(arrivalTransport);
			SetupTransport(mainTransport, "AUSYD", "USLAX", "SEA", Core.Constants.TransportPlanningType.MainVessel, "another", "GHI", new ZDateTime(2007, 2, 10));
			preAdvice.SetValuesOnOrder(order);

			AssertEquals("1234", order.JD_MasterWaybill);
			AssertEquals(Org1.PK, order.JD_OH_ReceivingAgent);
			AssertEquals(Org2.PK, order.JD_OH_SendingAgent);
			AssertEquals(Org3.PK, order.JD_OH_Carrier);
			AssertEquals(Org4.PK, order.BuyerPK);
			AssertEquals(ZGuid.Empty, order.JD_JS);
			AssertEquals(ZGuid.Empty, order.JD_JE);
			AssertEquals("AUSYD", order.JD_RL_NKPortOfLoading);
			AssertEquals("USLAX", order.JD_RL_NKPortOfDischarge);
			AssertEquals("AUSYD", order.JD_RL_NKGoodsAvailableAt);
			AssertEquals("USLAX", order.JD_RL_NKGoodsDeliveredTo);
			AssertEquals("435324", order.JD_Waybill);
			AssertEquals("TEST1", order.PlannedContainers[0].J1_ContainerNumber);
			AssertEquals("TEST2", order.PlannedContainers[1].J1_ContainerNumber);

			AssertEquals("another", order.JD_DepartureVoyage);
			AssertEquals("GHI", order.JD_RV_NKDepartureVessel);

			AssertEquals("", order.JD_IntermediateVoyage);
			AssertEquals("", order.JD_RV_NKIntermediateVessel);

			AssertEquals("9999", order.JD_ArrivalVoyage);
			AssertEquals("DEF", order.JD_RV_NKArrivalVessel);

			AssertEquals(mainTransport.JW_ETD, order.GetMilestoneEstimatedDate(Events.Departure).ToZDateTime());
			AssertEquals(arrivalTransport.JW_ETD, order.JD_E_ARV_1stIntermediate);

			AssertEquals(ZDateTime.Empty, order.JD_E_DEP_2);
			AssertEquals(ZDateTime.Empty, order.JD_E_ARV_2ndIntermediate);

			AssertEquals(arrivalTransport.JW_ETD, order.JD_E_DEP_3);
			AssertEquals(mainTransport.JW_ETA, order.GetMilestoneEstimatedDate(Events.Arrival).ToZDateTime());

			AssertEquals(mainTransport.JW_ATD, order.GetMilestoneActualDate(Events.Departure).ToZDateTime());
			AssertEquals(mainTransport.JW_ATA, order.GetMilestoneActualDate(Events.Arrival).ToZDateTime());
		}

		void SetupTransport(Transport transport, ZString load, ZString disc, ZString transMode, ZString transType, ZString voyFlight, ZString vessel, ZDateTime aTD)
		{
			transport.JW_RL_NKLoadPort = load;
			transport.JW_RL_NKDiscPort = disc;
			transport.JW_TransportMode = transMode;
			transport.JW_TransportType = transType;
			transport.JW_VoyageFlight = voyFlight;
			transport.JW_Vessel = vessel;
			transport.JW_ATD = aTD;
			transport.JW_ETD = aTD.AddDays(1);
			transport.JW_ATA = aTD.AddDays(2);
			transport.JW_ETA = aTD.AddDays(3);
		}

		JobShipmentPreplanning CreatePreAdviceWithRoutings(bool multipleRoutings, bool includeVessels)
		{
			TestContainerType = Factory.NewWithValidTestData<RefContainer>().PK;
			Org1 = Factory.NewWithValidTestData<OrgHeader>();
			Org2 = Factory.NewWithValidTestData<OrgHeader>();
			Org3 = Factory.NewWithValidTestData<OrgHeader>();
			Org4 = Factory.NewWithValidTestData<OrgHeader>();

			Dec = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));

			JobShipmentPreplanning preAdvice = Factory.New<JobShipmentPreplanning>();
			preAdvice.EF_MasterBill = "1234";
			preAdvice.EF_OH_ReceivingAgent = Org1.PK;
			preAdvice.EF_OH_SendingAgent = Org2.PK;
			preAdvice.EF_OH_Carrier = Org3.PK;
			preAdvice.EF_RL_NKPortLoad = "AUSYD";
			preAdvice.EF_RL_NKPortDisch = "USLAX";
			preAdvice.EF_HouseBill = "435324";
			preAdvice.BuyerPK = Org4.PK;

			DepartureTransport = preAdvice.PreAdviceTransports[0];
			DepartureTransport.JW_RL_NKLoadPort = preAdvice.EF_RL_NKPortLoad;
			DepartureTransport.JW_RL_NKDiscPort = multipleRoutings ? (ZString)"SGSIN" : preAdvice.EF_RL_NKPortDisch;
			DepartureTransport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			DepartureTransport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			DepartureTransport.JW_VoyageFlight = "123456";
			if (includeVessels)
			{
				DepartureTransport.JW_Vessel = "ABC";
			}

			DepartureTransport.JW_ETA = new ZDateTime(2007, 1, 5);
			DepartureTransport.JW_ETD = new ZDateTime(2007, 1, 4);
			DepartureTransport.JW_ATA = new ZDateTime(2007, 1, 3);
			DepartureTransport.JW_ATD = new ZDateTime(2007, 1, 2);

			if (multipleRoutings)
			{
				ArrivalRouting = preAdvice.PreAdviceTransports.AddNew();
				ArrivalRouting.JW_RL_NKLoadPort = "SGSIN";
				ArrivalRouting.JW_RL_NKDiscPort = "USLAX";
				ArrivalRouting.JW_TransportMode = Core.Constants.TransportModes.Sea;
				ArrivalRouting.JW_TransportType = Core.Constants.TransportPlanningType.Other;
				ArrivalRouting.JW_VoyageFlight = "9999";
				if (includeVessels)
				{
					ArrivalRouting.JW_Vessel = "ZUB";
				}

				ArrivalRouting.JW_ETA = new ZDateTime(2007, 1, 10);
				ArrivalRouting.JW_ETD = new ZDateTime(2007, 1, 9);
				ArrivalRouting.JW_ATA = new ZDateTime(2007, 1, 8);
				ArrivalRouting.JW_ATD = new ZDateTime(2007, 1, 7);

				OtherRouting = preAdvice.PreAdviceTransports.AddNew();
				OtherRouting.JW_TransportMode = Core.Constants.TransportModes.Sea;
				OtherRouting.JW_TransportType = Core.Constants.TransportPlanningType.Other;
				OtherRouting.JW_VoyageFlight = "1111";
				if (includeVessels)
				{
					OtherRouting.JW_Vessel = "RAK";
				}

				OtherRouting.JW_ETA = new ZDateTime(2007, 1, 20);
				OtherRouting.JW_ETD = new ZDateTime(2007, 1, 19);
				OtherRouting.JW_ATA = new ZDateTime(2007, 1, 18);
				OtherRouting.JW_ATD = new ZDateTime(2007, 1, 17);
			}

			OrderContainer container1 = preAdvice.Containers.AddNew();
			container1.J1_RC = TestContainerType;
			container1.J1_ContainerNumber = "TEST1";
			container1.J1_ContainerCount = 1;
			container1.J1_SealNum = "TEST1_SEAL1";
			container1.J1_AdditionalSealNum = "TEST1_SEAL2";
			container1.J1_Additional2SealNum = "TEST1_SEAL3";
			OrderContainer container2 = preAdvice.Containers.AddNew();
			container2.J1_RC = new ZGuid();
			container2.J1_ContainerNumber = "TEST2";
			Factory.Save();

			return preAdvice;
		}

		JobShipmentPreplanning CreatePreAdviceWithLinkedTransport()
		{
			Org1 = Factory.NewWithValidTestData<OrgHeader>();
			Org2 = Factory.NewWithValidTestData<OrgHeader>();
			Org3 = Factory.NewWithValidTestData<OrgHeader>();
			Org4 = Factory.NewWithValidTestData<OrgHeader>();

			Dec = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));

			var preAdvice = Factory.New<JobShipmentPreplanning>();
			preAdvice.EF_MasterBill = "1234";
			preAdvice.EF_OH_ReceivingAgent = Org1.PK;
			preAdvice.EF_OH_SendingAgent = Org2.PK;
			preAdvice.EF_OH_Carrier = Org3.PK;
			preAdvice.EF_RL_NKPortLoad = "AUSYD";
			preAdvice.EF_RL_NKPortDisch = "USLAX";
			preAdvice.EF_HouseBill = "435324";
			preAdvice.BuyerPK = Org4.PK;

			DepartureTransport = preAdvice.PreAdviceTransports[0];
			DepartureTransport.JW_RL_NKLoadPort = preAdvice.EF_RL_NKPortLoad;
			DepartureTransport.JW_RL_NKDiscPort = preAdvice.EF_RL_NKPortDisch;
			DepartureTransport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			DepartureTransport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			DepartureTransport.JW_VoyageFlight = "123456";

			var sailing = Factory.NewWithValidTestData<JobSailing>();
			var voyOrigin = Factory.NewWithValidTestData<VoyageOrigin>();
			var voyDestination = Factory.NewWithValidTestData<VoyageDestination>();

			sailing.JX_JA = voyOrigin.PK;
			sailing.JX_JB = voyDestination.PK;

			voyOrigin.JA_RL_NKPortOfLoading = "AUSYD";
			voyDestination.JB_RL_NKPortOfDischarge = "USLAX";

			voyOrigin.JA_E_DEP = new ZDateTime(2007, 1, 2);
			voyOrigin.JA_A_DEP = new ZDateTime(2007, 1, 3);
			voyDestination.JB_E_ARV = new ZDateTime(2007, 1, 4);
			voyDestination.JB_A_ARV = new ZDateTime(2007, 1, 5);

			DepartureTransport.JW_JX = sailing.PK;

			var container1 = preAdvice.Containers.AddNew();
			container1.J1_RC = new ZGuid();
			container1.J1_ContainerNumber = "TEST1";

			Factory.Save();

			return preAdvice;
		}

		public void TestPreAdvicePropertiesReadOnly()
		{
			JobShipmentPreplanning preAdvice = Factory.New<JobShipmentPreplanning>();
			Order order = Factory.New<Order>();

			AssertEquals(false, order.JD_OA_BuyerAddressInfo.ReadOnly);
			AssertEquals(false, order.JD_MasterWaybillInfo.ReadOnly);
			AssertEquals(false, order.JD_OH_ReceivingAgentInfo.ReadOnly);
			AssertEquals(false, order.JD_OH_SendingAgentInfo.ReadOnly);
			AssertEquals(false, order.JD_OH_CarrierInfo.ReadOnly);
			AssertEquals(false, order.JD_JSInfo.ReadOnly);
			AssertEquals(false, order.JD_JEInfo.ReadOnly);
			AssertEquals(false, order.JD_RL_NKPortOfLoadingInfo.ReadOnly);
			AssertEquals(false, order.JD_RL_NKPortOfDischargeInfo.ReadOnly);
			AssertEquals(false, order.JD_WaybillInfo.ReadOnly);

			order.JD_EF_ShipmentPrePlanning = preAdvice.PK;

			AssertEquals(true, order.JD_OA_BuyerAddressInfo.ReadOnly);
			AssertEquals(true, order.JD_MasterWaybillInfo.ReadOnly);
			AssertEquals(true, order.JD_OH_ReceivingAgentInfo.ReadOnly);
			AssertEquals(true, order.JD_OH_SendingAgentInfo.ReadOnly);
			AssertEquals(true, order.JD_OH_CarrierInfo.ReadOnly);
			AssertEquals(true, order.JD_JSInfo.ReadOnly);
			AssertEquals(true, order.JD_JEInfo.ReadOnly);
			AssertEquals(true, order.JD_RL_NKPortOfLoadingInfo.ReadOnly);
			AssertEquals(true, order.JD_RL_NKPortOfDischargeInfo.ReadOnly);
			AssertEquals(true, order.JD_WaybillInfo.ReadOnly);

			order.JD_Waybill = "333";
			AssertEquals(false, order.JD_WaybillInfo.ReadOnly);
		}

		OrgHeader Org1;
		OrgHeader Org2;
		OrgHeader Org3;
		OrgHeader Org4;

		Transport DepartureTransport;
		Transport ArrivalRouting;
		Transport OtherRouting;

		BusinessObject Dec;

		ZGuid TestContainerType;
	}
}
