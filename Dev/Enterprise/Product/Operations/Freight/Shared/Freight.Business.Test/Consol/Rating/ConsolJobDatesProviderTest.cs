using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ConsolJobDatesProviderTest : TestCaseWithFactory
	{
		public void TestDepartureAndArrivalDates()
		{
			var consol = Factory.New<CommonConsol>();

			Transport transport = consol.Transports[0];

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_ATD = new ZDateTime(2006, 5, 1);
			transport.JW_ATA = new ZDateTime(2006, 5, 7);
			transport.JW_ETD = new ZDateTime(2006, 5, 2);
			transport.JW_ETA = new ZDateTime(2006, 5, 6);

			var jobDatesProvider = new ConsolJobDatesProvider(consol);
			AssertEquals(new ZDateTime(2006, 5, 1), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
			AssertEquals(new ZDateTime(2006, 5, 7), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));

			transport.JW_ATA = ZDateTime.Empty;
			transport.JW_ATD = ZDateTime.Empty;
			AssertEquals(new ZDateTime(2006, 5, 2), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
			AssertEquals(new ZDateTime(2006, 5, 6), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));
		}

		public void TestEstimatedDepartureAndArrivalDates()
		{
			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports[0];
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_ATD = new ZDateTime(2006, 5, 1);
			transport.JW_ATA = new ZDateTime(2006, 5, 7);
			transport.JW_ETD = new ZDateTime(2006, 5, 2);
			transport.JW_ETA = new ZDateTime(2006, 5, 6);

			var jobDatesProvider = new ConsolJobDatesProvider(consol);
			AssertEquals(new ZDateTime(2006, 5, 2), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.EstimatedDepartureDate));
			AssertEquals(new ZDateTime(2006, 5, 6), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.EstimatedArrivalDate));

			//no fallback values expected
			transport.JW_ETA = ZDateTime.Empty;
			transport.JW_ETD = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.EstimatedDepartureDate));
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.EstimatedArrivalDate));
		}

		public void TestAWBIssueDate()
		{
			var dateToTest = new ZDateTime(2014, 5, 7);
			var consol = Factory.New<CommonConsol>();
			consol.JK_MasterBillIssueDate = dateToTest;
			var jobDatesProvider = new ConsolJobDatesProvider(consol);

			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.AWBIssueDate));
		}

		public void TestFirstContainerGateInDate()
		{
			var consol = Factory.New<CommonConsol>();
			var jobDatesProvider = new ConsolJobDatesProvider(consol);

			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.FirstContainerGateInDate));

			var dateToTest = ZDateTime.Today.AddDays(-2);

			var container1 = consol.Containers.AddNew();
			container1.JC_FCLWharfGateIn = dateToTest;

			var container2 = consol.Containers.AddNew();
			container2.JC_FCLWharfGateIn = ZDateTime.Today;

			var container3 = consol.Containers.AddNew();
			container3.JC_FCLWharfGateIn = ZDateTime.Today.AddDays(3);

			var container4 = consol.Containers.AddNew();
			container4.JC_FCLWharfGateIn = ZDateTime.Empty;

			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.FirstContainerGateInDate));
		}

		public void TestLastContainerGateInDate()
		{
			var consol = Factory.New<CommonConsol>();
			var jobDatesProvider = new ConsolJobDatesProvider(consol);

			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.LastContainerGateInDate));

			var dateToTest = ZDateTime.Today.AddDays(5);

			var container1 = consol.Containers.AddNew();
			container1.JC_FCLWharfGateIn = dateToTest;

			var container2 = consol.Containers.AddNew();
			container2.JC_FCLWharfGateIn = ZDateTime.Today.AddDays(-8);

			var container3 = consol.Containers.AddNew();
			container3.JC_FCLWharfGateIn = ZDateTime.Today;

			var container4 = consol.Containers.AddNew();
			container4.JC_FCLWharfGateIn = ZDateTime.Empty;

			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.LastContainerGateInDate));
		}

		public void TestCFSReceivalStartDate()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var jobDatesProvider = new ConsolJobDatesProvider(consol);

			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CFSReceivalStartDate));

			var dateToTest = ZDateTime.Today;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AUBNE";
			transport1.JW_VoyageFlight = "MainVoy";
			transport1.JW_CarrierBookingReference = "1243";
			transport1.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport1.JW_DepotReceivalCommences = dateToTest;

			var transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "AUBNE";
			transport2.JW_RL_NKDiscPort = "NZAKL";
			transport2.JW_VoyageFlight = "PreVoy";
			transport2.JW_CarrierBookingReference = "1245";
			transport2.JW_TransportType = Constants.TransportPlanningType.PreCarriage;
			transport2.JW_DepotReceivalCommences = dateToTest.AddDays(5);

			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CFSReceivalStartDate));
		}

		public void TestInterimReceiptDate_ShouldBeEmpty_NoShipmentAttached()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var jobDatesProvider = new ConsolJobDatesProvider(consol);
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.InterimReceiptDate));

			var dateToTest = ZDateTime.Today;
			var shipment = consol.Shipments.AddNew();
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.InterimReceiptDate));

			shipment.JS_A_RCV = dateToTest;
			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.InterimReceiptDate));
		}

		public void TestInterimReceiptDate_ShouldBeShipmentInterimReceiptDate_SingleShipmentAttached()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var jobDatesProvider = new ConsolJobDatesProvider(consol);
			var dateToTest = ZDateTime.Today;
			var shipment = consol.Shipments.AddNew();

			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.InterimReceiptDate));

			shipment.JS_A_RCV = dateToTest;

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUMEL";
			AssertEquals(consol.IsDomestic(), true);
			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.InterimReceiptDate));

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			AssertEquals(consol.IsExport(), true);
			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.InterimReceiptDate));

			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			AssertEquals(consol.IsImport(), true);
			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.InterimReceiptDate));

			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "USLAX";
			AssertEquals(consol.IsCrossTrade(), true);
			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.InterimReceiptDate));
		}

		public void TestInterimReceiptDate_ShouldBeEmpty_DomesticAndCrossTradeConsol_MultipleShipmentAttached()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var jobDatesProvider = new ConsolJobDatesProvider(consol);
			var dateToTest = ZDateTime.Today;
			var shipment = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();

			shipment.JS_A_RCV = dateToTest;
			shipment2.JS_A_RCV = dateToTest.AddDays(5);

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUMEL";
			AssertEquals(consol.IsDomestic(), true);
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.InterimReceiptDate));

			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "USLAX";
			AssertEquals(consol.IsCrossTrade(), true);
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.InterimReceiptDate));
		}

		public void TestInterimReceiptDate_ShouldBeLatestShipmentInterimReceiptDate_ImportConsol_MultipleShipmentAttached()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var jobDatesProvider = new ConsolJobDatesProvider(consol);
			var dateToTest = ZDateTime.Today;
			var dateToTestLater = ZDateTime.Today.AddDays(5);
			var shipment = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";

			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.InterimReceiptDate));

			shipment.JS_A_RCV = dateToTest;
			shipment2.JS_A_RCV = dateToTestLater;

			AssertEquals(consol.IsImport(), true);
			AssertEquals(dateToTestLater, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.InterimReceiptDate));
		}

		public void TestInterimReceiptDate_ShouldBeEarliestShipmentInterimReceiptDate_ExportConsol_MultipleShipmentAttached()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var jobDatesProvider = new ConsolJobDatesProvider(consol);
			var dateToTest = ZDateTime.Today;
			var dateToTestLater = ZDateTime.Today.AddDays(5);
			var shipment = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.InterimReceiptDate));

			shipment.JS_A_RCV = dateToTest;
			shipment2.JS_A_RCV = dateToTestLater;

			AssertEquals(consol.IsExport(), true);
			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.InterimReceiptDate));
		}

		[TestDate(2023, 4, 4)]
		public void TestCostingAutoratingDate()
		{
			var testDate = ZDate.Today.AddDays(2);
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var jobDatesProvider = new ConsolJobDatesProvider(consol);

			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CostingAutoratingDateOverride));

			consol.AutoratingDate = testDate;
			AssertEquals(testDate, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CostingAutoratingDateOverride));
		}
	}
}
