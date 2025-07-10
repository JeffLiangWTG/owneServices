using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DtbConsignmentJobDatesProviderTest : TestCaseWithFactory
	{
		#region TestDepartureDate

		public void TestDepartureDate()
		{
			var nowDTO = ZDateTimeOffset.Now;
			var now = nowDTO.ToLocalZDateTime();
			var consignment = Helper.CreateConsignment("LTC001");
			var picAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var dlvAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);
			var picAction1 = Helper.CreateConsignmentAction(picAddress, ActionTypes.Codes.PickUp);
			var picAction2 = Helper.CreateConsignmentAction(picAddress, ActionTypes.Codes.PickUp);
			var dlvAction = Helper.CreateConsignmentAction(dlvAddress, ActionTypes.Codes.Delivery);

			var jobDatesProvider = new DtbConsignmentJobDatesProvider(consignment, picAddress, dlvAddress);
			AssertEquals("Precondition", ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));

			picAction1.LTA_EstimatedTime = nowDTO.AddDays(3);
			picAction2.LTA_EstimatedTime = nowDTO.AddDays(2);
			dlvAction.LTA_EstimatedTime = nowDTO.AddDays(1);
			AssertEquals("No Actuals, so use Earliest Estimated from PickUp address.", now.AddDays(2), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));

			picAction1.LTA_ActualTime = nowDTO.AddDays(3);
			picAction2.LTA_ActualTime = nowDTO.AddDays(4);
			dlvAction.LTA_ActualTime = nowDTO.AddDays(2);
			AssertEquals("Has Actuals, so use Earliest Actual from PickUp address.", now.AddDays(3), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
		}

		#endregion

		#region TestArrivalDate

		public void TestArrivalDate()
		{
			var nowDTO = ZDateTimeOffset.Now;
			var now = nowDTO.ToLocalZDateTime();
			var consignment = Helper.CreateConsignment("LTC001");
			var picAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var dlvAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);
			var picAction = Helper.CreateConsignmentAction(picAddress, ActionTypes.Codes.PickUp);
			var dlvAction1 = Helper.CreateConsignmentAction(dlvAddress, ActionTypes.Codes.Delivery);
			var dlvAction2 = Helper.CreateConsignmentAction(dlvAddress, ActionTypes.Codes.Delivery);

			var jobDatesProvider = new DtbConsignmentJobDatesProvider(consignment, picAddress, dlvAddress);
			AssertEquals("Precondition", ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));

			picAction.LTA_EstimatedTime = nowDTO.AddDays(3);
			dlvAction1.LTA_EstimatedTime = nowDTO.AddDays(2);
			dlvAction2.LTA_EstimatedTime = nowDTO.AddDays(1);
			AssertEquals("No Actuals, so use Lastest Estimated from Delivery address.", now.AddDays(2), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));

			picAction.LTA_ActualTime = nowDTO.AddDays(4);
			dlvAction1.LTA_ActualTime = nowDTO.AddDays(2);
			dlvAction2.LTA_ActualTime = nowDTO.AddDays(3);
			AssertEquals("Has Actuals, so use Lastest Actual from Delivery address.", now.AddDays(3), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));
		}

		#endregion

		#region TestJobOpenDate

		public void TestJobOpenDate()
		{
			var consignment = Helper.CreateConsignment("Con0001");
			var fromAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var toAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);
			var jobDatesProvider = new DtbConsignmentJobDatesProvider(consignment, fromAddress, toAddress);

			AssertEquals(false, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate).IsValid);

			var dateToTest = ZDateTime.Today;
			var jobHeader = new JobHeader.Loader(consignment).TryLoadOrCreate();
			jobHeader.JH_A_JOP = dateToTest;

			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate));
		}

		#endregion

		#region Implementation

		TransportConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportConsignmentTestHelper(Factory)); }
		}

		TransportConsignmentTestHelper helper;

		#endregion
	}
}
