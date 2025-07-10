using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ShipmentJobDatesProviderTest : ShipmentJobDatesProviderBaseTest
	{
		public void TestDepartureAndArrivalDates()
		{
			var consol = Factory.New<CommonConsol>();
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			Transport transport1 = consol.Transports[0];
			Transport transport2 = consol.Transports.AddNew();

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "SGSIN";
			transport1.JW_ATD = new ZDateTime(2006, 5, 1);
			transport1.JW_ETD = new ZDateTime(2006, 5, 2);
			transport1.JW_ATA = new ZDateTime(2006, 5, 7);

			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "USLAX";
			transport2.JW_ATD = new ZDateTime(2006, 5, 7);
			transport2.JW_ATA = new ZDateTime(2006, 5, 15);
			transport2.JW_ETA = new ZDateTime(2006, 5, 16);

			shipment.JS_E_DEP = new ZDateTime(2006, 5, 5);
			shipment.JS_E_ARV = new ZDateTime(2006, 5, 20);

			var jobDatesProvider = new CommonShipmentJobDatesProvider(shipment);
			AssertEquals(new ZDateTime(2006, 5, 1), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
			AssertEquals(new ZDateTime(2006, 5, 15), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));

			transport2.JW_ATA = ZDateTime.Empty;
			transport1.JW_ATD = ZDateTime.Empty;
			AssertEquals(new ZDateTime(2006, 5, 2), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
			AssertEquals(new ZDateTime(2006, 5, 16), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));

			transport2.JW_RL_NKDiscPort = "INBOM";
			AssertEquals(new ZDateTime(2006, 5, 2), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
			AssertEquals(new ZDateTime(2006, 5, 20), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));

			transport2.JW_RL_NKDiscPort = "USLAX";
			transport2.JW_ETA = ZDateTime.Empty;
			transport1.JW_ETD = ZDateTime.Empty;
			AssertEquals(new ZDateTime(2006, 5, 5), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
			AssertEquals(new ZDateTime(2006, 5, 20), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));
		}

		public void TestEstimatedDepartureAndArrivalDates()
		{
			var consol = Factory.New<CommonConsol>();
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			Transport transport1 = consol.Transports[0];
			Transport transport2 = consol.Transports.AddNew();

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "SGSIN";
			transport1.JW_ATD = new ZDateTime(2006, 5, 1);
			transport1.JW_ETD = new ZDateTime(2006, 5, 2);
			transport1.JW_ATA = new ZDateTime(2006, 5, 7);
			transport1.JW_ETA = new ZDateTime(2006, 5, 6);

			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "USLAX";
			transport2.JW_ATD = new ZDateTime(2006, 5, 7);
			transport2.JW_ATA = new ZDateTime(2006, 5, 15);
			transport2.JW_ETA = new ZDateTime(2006, 5, 16);
			transport2.JW_ETD = new ZDateTime(2006, 5, 14);

			shipment.JS_E_DEP = new ZDateTime(2006, 5, 5);
			shipment.JS_E_ARV = new ZDateTime(2006, 5, 20);

			var jobDatesProvider = new CommonShipmentJobDatesProvider(shipment);
			AssertEquals(new ZDateTime(2006, 5, 2), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.EstimatedDepartureDate));
			AssertEquals(new ZDateTime(2006, 5, 16), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.EstimatedArrivalDate));

			transport2.JW_RL_NKDiscPort = "INBOM";
			AssertEquals(new ZDateTime(2006, 5, 2), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.EstimatedDepartureDate));
			AssertEquals(new ZDateTime(2006, 5, 20), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.EstimatedArrivalDate));

			transport2.JW_RL_NKDiscPort = "USLAX";
			transport2.JW_ETA = ZDateTime.Empty;
			transport1.JW_ETD = ZDateTime.Empty;
			AssertEquals(new ZDateTime(2006, 5, 5), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.EstimatedDepartureDate));
			AssertEquals(new ZDateTime(2006, 5, 20), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.EstimatedArrivalDate));
		}

		public void TestNullDepartureAndArrivalTransports()
		{
			var consol = Factory.New<CommonConsol>();
			CommonShipment shipment = consol.Shipments.AddNew();

			var jobDatesProvider = new CommonShipmentJobDatesProvider(shipment);
			consol.Transports.ParentDeleting();
			AssertNoExceptionThrown(() => jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
			AssertNoExceptionThrown(() => jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));
		}

		public void TestNullEstimatedDepartureAndArrivalTransports()
		{
			var consol = Factory.New<CommonConsol>();
			CommonShipment shipment = consol.Shipments.AddNew();

			var jobDatesProvider = new CommonShipmentJobDatesProvider(shipment);
			consol.Transports.ParentDeleting();
			AssertNoExceptionThrown(() => jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.EstimatedDepartureDate));
			AssertNoExceptionThrown(() => jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.EstimatedArrivalDate));
		}

		public void TestAWBIssueDate()
		{
			var consol = Factory.New<CommonConsol>();
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var jobDatesProvider = new CommonShipmentJobDatesProvider(shipment);
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.AWBIssueDate));

			var expectedDate = new ZDateTime(2011, 06, 20);
			consol.JK_MasterBillIssueDate = expectedDate;
			AssertEquals(expectedDate, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.AWBIssueDate));
		}

		public void TestPickupAndDeliveryDate()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var jobDatesProvider = new CommonShipmentJobDatesProvider(shipment);
			AssertEquals("OperationsRevenueRecognitionDate", ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.PickupDate));

			shipment.DocsAndCartage.JP_EstimatedPickup = ZDateTime.Today.AddDays(1);
			shipment.DocsAndCartage.JP_EstimatedDelivery = ZDateTime.Today.AddDays(4);
			AssertEquals("OperationsRevenueRecognitionDate", ZDateTime.Today.AddDays(1), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.PickupDate));

			shipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.Today.AddDays(2);
			AssertEquals("OperationsRevenueRecognitionDate", ZDateTime.Today.AddDays(2), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.PickupDate));

			AssertEquals("OperationsRevenueRecognitionDate", ZDateTime.Today.AddDays(4), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DeliveryDate));

			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Today.AddDays(5);
			AssertEquals("OperationsRevenueRecognitionDate", ZDateTime.Today.AddDays(5), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DeliveryDate));
		}

		public void TestCustomClearenceDateByDirection()
		{
			#region Export

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			var jobDatesProvider = new CommonShipmentJobDatesProvider(shipment);

			AssertEquals("OperationsRevenueRecognitionDate", ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CustomsClearanceDate, "EXP"));

			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-1), false);
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset(), false);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-2), false);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-3), false);
			AssertEquals("OperationsRevenueRecognitionDate", ZDateTime.BrettsBirthday.AddMonths(-2), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CustomsClearanceDate, "EXP"));

			shipment.Logs.CancelAll();
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-1), false);
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset(), true);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-2), false);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-3), false);
			AssertEquals("OperationsRevenueRecognitionDate", ZDateTime.BrettsBirthday.AddMonths(-2), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CustomsClearanceDate, "EXP"));

			shipment.Logs.CancelAll();
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-1), true);
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset(), true);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-2), false);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-3), false);
			AssertEquals("OperationsRevenueRecognitionDate", ZDateTime.BrettsBirthday.AddMonths(-2), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CustomsClearanceDate, "EXP"));

			shipment.Logs.CancelAll();
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-1), true);
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset(), true);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-2), true);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-3), false);
			AssertEquals("OperationsRevenueRecognitionDate", ZDateTime.BrettsBirthday.AddMonths(-3), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CustomsClearanceDate, "EXP"));

			shipment.Logs.CancelAll();
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-1), true);
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset(), true);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-2), true);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-3), true);
			AssertEquals("OperationsRevenueRecognitionDate", ZDateTime.BrettsBirthday.AddMonths(-2), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CustomsClearanceDate, "EXP"));

			shipment.Logs.CancelAll();
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-1), true);
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset(), true);
			var log1 = shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-2), true);
			var log2 = shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-3), true);
			log1.Cancel();
			log2.Cancel();
			AssertEquals("OperationsRevenueRecognitionDate", ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CustomsClearanceDate, "EXP"));

			#endregion

			#region Import

			shipment.Logs.CancelAll();
			AssertEquals("OperationsRevenueRecognitionDate", ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CustomsClearanceDate, "IMP"));

			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-1), false);
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset(), false);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-2), false);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-3), false);
			AssertEquals("OperationsRevenueRecognitionDate", ZDateTime.BrettsBirthday, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CustomsClearanceDate, "IMP"));

			shipment.Logs.CancelAll();
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-1), false);
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset(), true);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-2), false);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-3), false);
			AssertEquals("OperationsRevenueRecognitionDate", ZDateTime.BrettsBirthday.AddMonths(-1), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CustomsClearanceDate, "IMP"));

			shipment.Logs.CancelAll();
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-1), true);
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset(), true);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-2), false);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-3), false);
			AssertEquals("OperationsRevenueRecognitionDate", ZDateTime.BrettsBirthday, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CustomsClearanceDate, "IMP"));

			shipment.Logs.CancelAll();
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-1), true);
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset(), true);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-2), true);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-3), false);
			AssertEquals("OperationsRevenueRecognitionDate", ZDateTime.BrettsBirthday, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CustomsClearanceDate, "IMP"));

			shipment.Logs.CancelAll();
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-1), true);
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset(), true);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-2), true);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-3), true);
			AssertEquals("OperationsRevenueRecognitionDate", ZDateTime.BrettsBirthday, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CustomsClearanceDate, "IMP"));

			shipment.Logs.CancelAll();
			log1 = shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-1), true);
			log2 = shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset(), true);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-2), true);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-3), true);
			log1.Cancel();
			log2.Cancel();
			AssertEquals("OperationsRevenueRecognitionDate", ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CustomsClearanceDate, "IMP"));

			#endregion
		}

		public void TestCustomClearenceDate()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var jobDatesProvider = new CommonShipmentJobDatesProvider(shipment);

			AssertEquals("OperationsRevenueRecognitionDate", ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CustomsClearanceDate));

			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-1), false);
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset(), false);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-2), false);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-3), false);
			AssertEquals("OperationsRevenueRecognitionDate", ZDateTime.BrettsBirthday, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CustomsClearanceDate));

			shipment.Logs.CancelAll();
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-1), false);
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset(), true);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-2), false);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-3), false);
			AssertEquals("OperationsRevenueRecognitionDate", ZDateTime.BrettsBirthday.AddMonths(-1), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CustomsClearanceDate));

			shipment.Logs.CancelAll();
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-1), true);
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset(), true);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-2), false);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-3), false);
			AssertEquals("OperationsRevenueRecognitionDate", ZDateTime.BrettsBirthday.AddMonths(-2), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CustomsClearanceDate));

			shipment.Logs.CancelAll();
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-1), true);
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset(), true);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-2), true);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-3), false);
			AssertEquals("OperationsRevenueRecognitionDate", ZDateTime.BrettsBirthday.AddMonths(-3), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CustomsClearanceDate));

			shipment.Logs.CancelAll();
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-1), true);
			shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset(), true);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-2), true);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-3), true);
			AssertEquals("OperationsRevenueRecognitionDate", ZDateTime.BrettsBirthday, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CustomsClearanceDate));

			shipment.Logs.CancelAll();
			var log1 = shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-1), true);
			var log2 = shipment.Logs.AddNew(Events.CustomsCleared, ZDateTime.BrettsBirthday.ToOffset(), true);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-2), true);
			shipment.Logs.AddNew(Events.ExportCustomsCleared, ZDateTime.BrettsBirthday.ToOffset().AddMonths(-3), true);
			log1.Cancel();
			log2.Cancel();
			AssertEquals("OperationsRevenueRecognitionDate", ZDateTime.BrettsBirthday.AddMonths(-2), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CustomsClearanceDate));
		}

		[TestDate(2013, 10, 27)]
		public void TestHouseBillIssueDate()
		{
			var shipment = Factory.New<CommonShipment>();

			var jobDatesProvider = new CommonShipmentJobDatesProvider(shipment);
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.HouseBillIssueDate));

			shipment.JS_HouseBillIssueDate = DateTime.Today.AddDays(-15);

			AssertEquals(DateTime.Today.AddDays(-15), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.HouseBillIssueDate));
		}

		public void TestJobOpenDate()
		{
			var shipment = Factory.New<CommonShipment>();
			var jobDatesProvider = new CommonShipmentJobDatesProvider(shipment);

			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate));

			var dateToTest = ZDateTime.Today;
			var jobHeader = new JobHeader.Loader(shipment).TryLoadOrCreate();
			jobHeader.JH_A_JOP = dateToTest;

			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate));
		}

		#region First/Last Container Gate In Date

		public void TestFirstContainerGateInDate()
		{
			var dateToTest = ZDateTime.Today.AddDays(-2);

			var consol = Factory.New<CommonConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "UCR001";
			shipment.JS_HouseBill = "HB001";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var container1 = consol.Containers.AddNew();
			container1.JC_FCLWharfGateIn = ZDateTime.Today.AddDays(5);

			var container2 = consol.Containers.AddNew();
			container2.JC_FCLWharfGateIn = ZDateTime.Empty;

			var container3 = consol.Containers.AddNew();
			container3.JC_FCLWharfGateIn = dateToTest;

			var jobDatesProvider = new CommonShipmentJobDatesProvider(shipment);
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.FirstContainerGateInDate));

			shipment.OuterPackLines.AddNew().Containers.Add(container1);
			shipment.OuterPackLines.AddNew().Containers.Add(container2);
			shipment.OuterPackLines.AddNew().Containers.Add(container3);

			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.FirstContainerGateInDate));
		}

		public void TestLastContainerGateInDate()
		{
			var dateToTest = ZDateTime.Today.AddDays(2);

			var consol = Factory.New<CommonConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "UCR001";
			shipment.JS_HouseBill = "HB001";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var container1 = consol.Containers.AddNew();
			container1.JC_FCLWharfGateIn = dateToTest;

			var container2 = consol.Containers.AddNew();
			container2.JC_FCLWharfGateIn = ZDateTime.Empty;

			var container3 = consol.Containers.AddNew();
			container3.JC_FCLWharfGateIn = ZDateTime.Today.AddDays(-5);

			var jobDatesProvider = new CommonShipmentJobDatesProvider(shipment);
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.LastContainerGateInDate));

			shipment.OuterPackLines.AddNew().Containers.Add(container1);
			shipment.OuterPackLines.AddNew().Containers.Add(container2);
			shipment.OuterPackLines.AddNew().Containers.Add(container3);

			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.LastContainerGateInDate));
		}

		[TestDate(2020, 1, 10)]
		public void TestContainerGateInDate_NoContainer()
		{
			AssertJobDateByType(Shipment, JobDateTypes.Codes.FirstContainerGateInDate, expectedJobDateByType: ZDateTime.Empty);
			AssertJobDateByType(Shipment, JobDateTypes.Codes.LastContainerGateInDate, expectedJobDateByType: ZDateTime.Empty);
		}

		[TestDate(2020, 1, 10)]
		public void TestContainerGateInDate_HasContainer_NotPacked()
		{
			var container1 = Consol.Containers.AddNew();
			container1.JC_FCLWharfGateIn = new ZDateTime(2020, 1, 15);
			AssertJobDateByType(Shipment, JobDateTypes.Codes.FirstContainerGateInDate, expectedJobDateByType: ZDateTime.Empty);
			AssertJobDateByType(Shipment, JobDateTypes.Codes.LastContainerGateInDate, expectedJobDateByType: ZDateTime.Empty);
		}

		[TestDate(2020, 1, 10)]
		public void TestContainerGateInDate_HasContainer_Packed_NoGateInDate()
		{
			var container1 = Consol.Containers.AddNew();
			Shipment.OuterPackLines.AddNew().Containers.Add(container1);
			AssertJobDateByType(Shipment, JobDateTypes.Codes.FirstContainerGateInDate, expectedJobDateByType: ZDateTime.Empty);
			AssertJobDateByType(Shipment, JobDateTypes.Codes.LastContainerGateInDate, expectedJobDateByType: ZDateTime.Empty);
		}

		[TestDate(2020, 1, 10)]
		public void TestContainerGateInDate_HasContainer_Packed_HasGateInDate()
		{
			var container1 = Consol.Containers.AddNew();
			container1.JC_FCLWharfGateIn = new ZDateTime(2020, 1, 15);
			Shipment.OuterPackLines.AddNew().Containers.Add(container1);
			AssertJobDateByType(Shipment, JobDateTypes.Codes.FirstContainerGateInDate, expectedJobDateByType: new ZDateTime(2020, 1, 15), message: "JobDateByType should be container-gate-in-date");
			AssertJobDateByType(Shipment, JobDateTypes.Codes.LastContainerGateInDate, expectedJobDateByType: new ZDateTime(2020, 1, 15), message: "JobDateByType should be container-gate-in-date");
		}

		[TestDate(2020, 1, 10)]
		public void TestContainerGateInDate_HasMultipleContainers_Packed_HasGateInDate()
		{
			var container1 = Consol.Containers.AddNew();
			container1.JC_FCLWharfGateIn = new ZDateTime(2020, 1, 15);
			Shipment.OuterPackLines.AddNew().Containers.Add(container1);

			var container2 = Consol.Containers.AddNew();
			container2.JC_FCLWharfGateIn = ZDateTime.Empty;
			Shipment.OuterPackLines.AddNew().Containers.Add(container2);

			var container3 = Consol.Containers.AddNew();
			container3.JC_FCLWharfGateIn = new ZDateTime(2020, 1, 5);
			Shipment.OuterPackLines.AddNew().Containers.Add(container3);

			AssertJobDateByType(Shipment, JobDateTypes.Codes.FirstContainerGateInDate, expectedJobDateByType: new ZDateTime(2020, 1, 5), message: "JobDateByType should be the first container-gate-in-date");
			AssertJobDateByType(Shipment, JobDateTypes.Codes.LastContainerGateInDate, expectedJobDateByType: new ZDateTime(2020, 1, 15), message: "JobDateByType should be the last container-gate-in-date");
		}

		[TestDate(2020, 1, 10)]
		public void TestFirstContainerGateInDate_Complex()
		{
			var consol1 = CreateConsol("C00001", Constants.TransportModes.Sea, "AUSYD", "NZAKL");
			var shipment1 = AddShipment(consol1, "SHP001", "HB001", "AUSYD", "NZAKL");

			var consol2 = CreateConsol("C00002", Constants.TransportModes.Sea, "AUSYD", "NZAKL");
			consol2.Shipments.Add(shipment1);

			var consol3 = CreateConsol("C00003", Constants.TransportModes.Sea, "AUSYD", "NZAKL");

			var container11 = consol1.Containers.AddNew();
			container11.JC_Description = "container11";
			container11.JC_FCLWharfGateIn = new ZDateTime(2020, 1, 5);
			shipment1.OuterPackLines.AddNew().Containers.Add(container11);

			var container12 = consol1.Containers.AddNew();
			container12.JC_Description = "container12";
			container12.JC_FCLWharfGateIn = ZDateTime.Empty;
			shipment1.OuterPackLines.AddNew().Containers.Add(container12);

			var container21 = consol2.Containers.AddNew();
			container21.JC_Description = "container21";
			container21.JC_FCLWharfGateIn = new ZDateTime(2020, 1, 4);
			shipment1.OuterPackLines.AddNew().Containers.Add(container21);

			var container22 = consol2.Containers.AddNew();
			container22.JC_Description = "container22";
			container22.JC_FCLWharfGateIn = new ZDateTime(2020, 1, 3);
			shipment1.OuterPackLines.AddNew().Containers.Add(container22);

			// shipment is not packed to this container
			var container23 = consol2.Containers.AddNew();
			container23.JC_Description = "container23";
			container23.JC_FCLWharfGateIn = new ZDateTime(2020, 1, 2);

			// Unrelated consol+container
			var container31 = consol3.Containers.AddNew();
			container31.JC_Description = "container31";
			container31.JC_FCLWharfGateIn = new ZDateTime(2020, 1, 1);

			AssertJobDateByType(shipment1, JobDateTypes.Codes.FirstContainerGateInDate, expectedJobDateByType: new ZDateTime(2020, 1, 3), message: "JobDateByType should be the first gate-in-date containers that the shipment is packed");
			AssertJobDateByType(shipment1, JobDateTypes.Codes.LastContainerGateInDate, expectedJobDateByType: new ZDateTime(2020, 1, 5), message: "JobDateByType should be the last gate-in-date containers that the shipment is packed");
		}

		CommonConsol Consol => consol ?? (consol = CreateConsol("C00001", Constants.TransportModes.Sea, "AUSYD", "NZAKL"));
		CommonConsol consol;

		CommonShipment Shipment => shipment ?? (shipment = AddShipment(Consol, "SHP001", "HB001", "AUSYD", "NZAKL"));
		CommonShipment shipment;

		CommonConsol CreateConsol(string uniqueConsignRef, string transportMode, string origin, string destination)
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_UniqueConsignRef = uniqueConsignRef;
			consol.JK_TransportMode = transportMode;
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;

			return consol;
		}

		CommonShipment AddShipment(CommonConsol consol, string uniqueConsignRef, string houseBill, string origin, string destination)
		{
			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = uniqueConsignRef;
			shipment.JS_HouseBill = houseBill;
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;

			return shipment;
		}

		static void AssertJobDateByType(CommonShipment shipment, string jobDateType, ZDateTime expectedJobDateByType, string message = default)
		{
			var jobDatesProvider = new CommonShipmentJobDatesProvider(shipment);
			AssertEquals($"{jobDateType}: {message}", expectedJobDateByType, jobDatesProvider.GetJobDateByType(jobDateType));
		}

		#endregion

		public void TestHBLPlaceOfReceiptArrivalDate()
		{
			var dateToTest = ZDateTime.Today.AddDays(8);

			var consol = Factory.New<CommonConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_BookingReference = "CN100";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "UCR001";
			shipment.JS_HouseBill = "HB001";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_HouseBillIssueDate = new ZDateTime(2018, 10, 1);
			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR;
			shipment.JS_INCO = Core.Constants.IncoTerms.CostAndFreight;
			shipment.DocsAndCartage.JP_PickupCartageCompleted = dateToTest;

			var transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AUMEL";
			transport1.JW_VoyageFlight = "MainVoy";
			transport1.JW_CarrierBookingReference = "1243";
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport1.JW_DepotReceivalCommences = dateToTest;

			var transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "AUBNE";
			transport2.JW_RL_NKDiscPort = "NZAKL";
			transport2.JW_VoyageFlight = "PreVoy";
			transport2.JW_CarrierBookingReference = "1245";
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.PreCarriage;
			transport2.JW_DepotReceivalCommences = ZDateTime.Empty;

			var container1 = consol.Containers.AddNew();
			container1.JC_FCLWharfGateIn = dateToTest;

			var container2 = consol.Containers.AddNew();
			container2.JC_FCLWharfGateIn = ZDateTime.Today.AddDays(-2);

			var container3 = consol.Containers.AddNew();
			container3.JC_FCLWharfGateIn = ZDateTime.Empty;

			var jobDatesProvider = new CommonShipmentJobDatesProvider(shipment);
			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.HBLPlaceOfReceiptArrivalDate));

			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CY_CFS;
			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.HBLPlaceOfReceiptArrivalDate));

			shipment.JS_HBLContainerPackModeOverride = Core.Constants.HBLDeliveryModes.Codes.CY_CY;
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.HBLPlaceOfReceiptArrivalDate));

			shipment.OuterPackLines.AddNew().Containers.Add(container1);
			shipment.OuterPackLines.AddNew().Containers.Add(container2);
			shipment.OuterPackLines.AddNew().Containers.Add(container3);

			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.HBLPlaceOfReceiptArrivalDate));
		}

		public void TestInterimReceiptDate()
		{
			var consol = Factory.New<CommonConsol>();
			var shipment = consol.Shipments.AddNew();

			var jobDatesProvider = new CommonShipmentJobDatesProvider(shipment);
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.InterimReceiptDate));

			var expectedDate = new ZDateTime(2023, 02, 23);
			shipment.JS_A_RCV = expectedDate;
			AssertEquals(expectedDate, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.InterimReceiptDate));
		}
	}
}
