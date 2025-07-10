using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class CFSLoadListConsolJobDatesProviderTests : TestCaseWithFactory
	{
		public void TestArrivalDate()
		{
			var consol = Factory.New<CFSLoadListConsol>();
			var jobDatesProvider = new CFSLoadListConsolJobDatesProvider(consol);

			AssertEquals(false, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate).IsValid);

			var dateToTest = new ZDateTime(2006, 5, 6);
			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_ETA = dateToTest;

			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));
		}

		public void TestDepartureDate()
		{
			var consol = Factory.New<CFSLoadListConsol>();
			var jobDatesProvider = new CFSLoadListConsolJobDatesProvider(consol);

			AssertEquals(false, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate).IsValid);

			var dateToTest = new ZDateTime(2006, 5, 8);
			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_ETD = dateToTest;

			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
		}

		public void TestJobOpenDateWhenCFS()
		{
			var consol = Factory.New<CFSLoadListConsol>();
			var jobDatesProvider = new CFSLoadListConsolJobDatesProvider(consol);

			AssertEquals(false, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate).IsValid);

			var dateToTest = new ZDateTime(2006, 5, 8);
			var jobHeader = new JobHeader.Loader(consol).TryLoadOrCreate();
			jobHeader.JH_A_JOP = dateToTest;

			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate));
		}

		public void TestFirstContainerGateInDateWhenCFS()
		{
			var consol = Factory.New<CFSLoadListConsol>();
			var jobDatesProvider = new CFSLoadListConsolJobDatesProvider(consol);

			AssertEquals(false, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.FirstContainerGateInDate).IsValid);

			var dateToTest = ZDateTime.Today;

			var container1 = consol.Containers.AddNew();
			container1.JC_FCLWharfGateIn = dateToTest;

			var container2 = consol.Containers.AddNew();
			container2.JC_FCLWharfGateIn = dateToTest.AddDays(8);

			var container3 = consol.Containers.AddNew();
			container3.JC_FCLWharfGateIn = dateToTest.AddDays(5);

			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.FirstContainerGateInDate));
		}

		public void TestLastContainerGateInDateWhenCFS()
		{
			var consol = Factory.New<CFSLoadListConsol>();
			var jobDatesProvider = new CFSLoadListConsolJobDatesProvider(consol);

			AssertEquals(false, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.FirstContainerGateInDate).IsValid);

			var dateToTest = ZDateTime.Today;

			var container1 = consol.Containers.AddNew();
			container1.JC_FCLWharfGateIn = dateToTest;

			var container2 = consol.Containers.AddNew();
			container2.JC_FCLWharfGateIn = dateToTest.AddDays(-8);

			var container3 = consol.Containers.AddNew();
			container3.JC_FCLWharfGateIn = dateToTest.AddDays(-5);

			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.LastContainerGateInDate));
		}

		public void TestCFSReceivalStartDateWhenCFS()
		{
			var consol = Factory.NewWithValidTestData<CFSLoadListConsol>();
			var jobDatesProvider = new CFSLoadListConsolJobDatesProvider(consol);

			AssertEquals(false, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CFSReceivalStartDate).IsValid);

			var dateToTest = ZDateTime.Today;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AUBNE";
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
			transport2.JW_DepotReceivalCommences = dateToTest.AddDays(5);

			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CFSReceivalStartDate));
		}

		public void TestJobOpenDateWhenGatePass()
		{
			var consol = Factory.New<GatePassLoadListConsol>();
			var jobDatesProvider = new CFSLoadListConsolJobDatesProvider(consol);

			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate));

			var dateToTest = new ZDateTime(2006, 5, 8);
			var jobHeader = new JobHeader.Loader(consol).TryLoadOrCreate();
			jobHeader.JH_A_JOP = dateToTest;

			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate));
		}

		public void TestFirstContainerGateInDateWhenGatePass()
		{
			var consol = Factory.New<GatePassLoadListConsol>();
			var jobDatesProvider = new CFSLoadListConsolJobDatesProvider(consol);

			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.FirstContainerGateInDate));

			var dateToTest = ZDateTime.Today.AddDays(-1);

			var container1 = consol.Containers.AddNew();
			container1.JC_FCLWharfGateIn = dateToTest;

			var container2 = consol.Containers.AddNew();
			container2.JC_FCLWharfGateIn = ZDateTime.Empty;

			var container3 = consol.Containers.AddNew();
			container3.JC_FCLWharfGateIn = dateToTest.AddDays(5);

			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.FirstContainerGateInDate));
		}

		public void TestLastContainerGateInDateWhenGatePass()
		{
			var consol = Factory.New<GatePassLoadListConsol>();
			var jobDatesProvider = new CFSLoadListConsolJobDatesProvider(consol);

			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.FirstContainerGateInDate));

			var dateToTest = ZDateTime.Today;

			var container1 = consol.Containers.AddNew();
			container1.JC_FCLWharfGateIn = dateToTest;

			var container2 = consol.Containers.AddNew();
			container2.JC_FCLWharfGateIn = dateToTest.AddDays(-2);

			var container3 = consol.Containers.AddNew();
			container3.JC_FCLWharfGateIn = ZDateTime.Empty;

			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.LastContainerGateInDate));
		}

		public void TestCFSReceivalStartDateWhenGatePass()
		{
			var consol = Factory.NewWithValidTestData<GatePassLoadListConsol>();
			var jobDatesProvider = new CFSLoadListConsolJobDatesProvider(consol);

			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CFSReceivalStartDate));

			var dateToTest = ZDateTime.Today;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AUBNE";
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

			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CFSReceivalStartDate));
		}
	}
}
