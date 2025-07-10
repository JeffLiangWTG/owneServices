using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsVASOrderJobDatesProviderTest : WhsTestCaseWithFactory
	{
		[TestDate(2020, 6, 11)]
		public void TestArrivalDate()
		{
			TestJobProviderDateCore(JobDateTypes.Codes.ArrivalDate);
		}

		[TestDate(2020, 6, 11)]
		public void TestDepartureDate()
		{
			TestJobProviderDateCore(JobDateTypes.Codes.DepartureDate);
		}

		void TestJobProviderDateCore(ZString jobDateType)
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse", "A", 1, 2);
			var product = Helper.CreateProduct("P1", client);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			Helper.CreateWhsVASOrderLine(vasOrder, product, 5m);
			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", product, 5m);
			Factory.Save();
			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			vasOrder.MarkVASOrderAsCompleted(Notify);
			AssertEquals("Precondition: VAS Order is complete.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
			Factory.Save();

			var jobDatesProvider = new WhsVASOrderJobDatesProvider(vasOrder);
			AssertEquals(ZDateTime.Today, jobDatesProvider.GetJobDateByType(jobDateType));

			WhsTransfer returnTransfer;
			using (Helper.GetPutawayEngineManagerForVASTransferLineMock())
			{
				returnTransfer = vasOrder.GetOrCreateReturnTransfer(Notify);
			}

			returnTransfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			AssertEquals("Precondition: VAS Order is finalised.", true, vasOrder.IsFinalised);
			AssertNotEquals("Precondition: VAS Order should have finalised date.", ZDateTime.Empty,
				vasOrder.WVO_FinalizedTimeUtc);

			AssertEquals(vasOrder.WVO_FinalizedTimeUtc, jobDatesProvider.GetJobDateByType(jobDateType));
		}

		public void TestJobOpenDate()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			var jobDatesProvider = new WhsVASOrderJobDatesProvider(vasOrder);

			AssertEquals(false, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate).IsValid);

			var dateToTest = ZDateTime.Today;
			var jobHeader = new JobHeader.Loader(vasOrder).TryLoadOrCreate();
			jobHeader.JH_A_JOP = dateToTest;

			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate));
		}
	}
}
