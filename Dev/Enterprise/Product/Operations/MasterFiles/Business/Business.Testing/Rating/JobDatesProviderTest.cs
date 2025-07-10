using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class JobDatesProviderTest : TestCaseWithFactory
	{
		public void TestArrivalDate()
		{
			var autoRating = new Mock<IBusiness>(MockBehavior.Strict);
			var jobDatesProvider = new JobDatesProvider<IBusiness>(autoRating.Object);
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate, "IMP"));
		}

		public void TestDepartureDate()
		{
			var autoRating = new Mock<IBusiness>(MockBehavior.Strict);
			var jobDatesProvider = new JobDatesProvider<IBusiness>(autoRating.Object);
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate, "IMP"));
		}

		public void TestEstimatedArrivalAndDepartureDate()
		{
			var autoRating = new Mock<IBusiness>(MockBehavior.Strict);
			var jobDatesProvider = new JobDatesProvider<IBusiness>(autoRating.Object);
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.EstimatedArrivalDate));
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.EstimatedArrivalDate, "IMP"));
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.EstimatedDepartureDate));
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.EstimatedDepartureDate, "IMP"));
		}

		public void TestAWBIssueDate()
		{
			var autoRating = new Mock<IBusiness>(MockBehavior.Strict);
			var jobDatesProvider = new JobDatesProvider<IBusiness>(autoRating.Object);
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.AWBIssueDate));
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.AWBIssueDate, "IMP"));
		}

		public void TestCustomsClearanceDate()
		{
			var autoRating = new Mock<IBusiness>(MockBehavior.Strict);
			var jobDatesProvider = new JobDatesProvider<IBusiness>(autoRating.Object);
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CustomsClearanceDate));
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CustomsClearanceDate, "IMP"));
		}

		public void TestPickupDate()
		{
			var autoRating = new Mock<IBusiness>(MockBehavior.Strict);
			var jobDatesProvider = new JobDatesProvider<IBusiness>(autoRating.Object);
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.PickupDate));
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.PickupDate, "IMP"));
		}

		public void TestDeliveryDate()
		{
			var autoRating = new Mock<IBusiness>(MockBehavior.Strict);
			var jobDatesProvider = new JobDatesProvider<IBusiness>(autoRating.Object);
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DeliveryDate));
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DeliveryDate, "IMP"));
		}

		public void TestVesselArrivalDate()
		{
			var autoRating = new Mock<IBusiness>(MockBehavior.Strict);
			var jobDatesProvider = new JobDatesProvider<IBusiness>(autoRating.Object);
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.VesselArrivalDate));
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.VesselArrivalDate, "IMP"));
		}

		public void TestVesselDepartureDate()
		{
			var autoRating = new Mock<IBusiness>(MockBehavior.Strict);
			var jobDatesProvider = new JobDatesProvider<IBusiness>(autoRating.Object);
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.VesselDepartureDate));
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.VesselDepartureDate, "IMP"));
		}

		public void TestHouseBillIssueDate()
		{
			var autoRating = new Mock<IBusiness>(MockBehavior.Strict);
			var jobDatesProvider = new JobDatesProvider<IBusiness>(autoRating.Object);
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.HouseBillIssueDate));
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.HouseBillIssueDate, "IMP"));
		}

		[TestDate(2014, 2, 19)]
		public void TestEarliestPossibleDate()
		{
			var businessObject = Factory.New<DummyBusinessObject>();
			var jobDatesProvider = new JobDatesProviderForTest(businessObject);

			AssertEquals(ZDateTime.Today, jobDatesProvider.EarliestPossibleDate);

			var departureDate = new ZDateTime(2014, 5, 14);
			jobDatesProvider = new JobDatesProviderForTest(businessObject) { DepartureDate = departureDate };

			AssertEquals(departureDate, jobDatesProvider.EarliestPossibleDate);
		}

		[TestDate(2014, 2, 19)]
		public void TestLatestPossibleDate()
		{
			var businessObject = Factory.New<DummyBusinessObject>();
			var jobDatesProvider = new JobDatesProviderForTest(businessObject);

			AssertEquals(ZDateTime.Today, jobDatesProvider.EarliestPossibleDate);

			var arrivalDate = new ZDateTime(2014, 5, 14);
			jobDatesProvider = new JobDatesProviderForTest(businessObject) { ArrivalDate = arrivalDate };

			AssertEquals(arrivalDate, jobDatesProvider.LatestPossibleDate);
		}

		public void TestJobOpenDate()
		{
			var autoRating = new Mock<IBusiness>(MockBehavior.Strict);
			var jobDatesProvider = new JobDatesProvider<IBusiness>(autoRating.Object);

			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate));
		}

		public void TestFirstContainerGateInDate()
		{
			var autoRating = new Mock<IBusiness>(MockBehavior.Strict);
			var jobDatesProvider = new JobDatesProvider<IBusiness>(autoRating.Object);

			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.FirstContainerGateInDate));
		}

		public void TestLastContainerGateInDate()
		{
			var autoRating = new Mock<IBusiness>(MockBehavior.Strict);
			var jobDatesProvider = new JobDatesProvider<IBusiness>(autoRating.Object);

			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.LastContainerGateInDate));
		}

		public void TestCFSReceivalStartDate()
		{
			var autoRating = new Mock<IBusiness>(MockBehavior.Strict);
			var jobDatesProvider = new JobDatesProvider<IBusiness>(autoRating.Object);

			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CFSReceivalStartDate));
		}

		public void TestHBLPlaceOfReceiptArrivalDate()
		{
			var autoRating = new Mock<IBusiness>(MockBehavior.Strict);
			var jobDatesProvider = new JobDatesProvider<IBusiness>(autoRating.Object);

			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.HBLPlaceOfReceiptArrivalDate));
		}

		public void TestTransitTime()
		{
			var autoRating = new Mock<IBusiness>(MockBehavior.Strict);
			var jobDatesProvider = new JobDatesProvider<IBusiness>(autoRating.Object);

			AssertEquals(string.Empty, jobDatesProvider.TransitTime);
		}

		public void TestInterimReceiptDate()
		{
			var autoRating = new Mock<IBusiness>(MockBehavior.Strict);
			var jobDatesProvider = new JobDatesProvider<IBusiness>(autoRating.Object);

			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.InterimReceiptDate));
		}

		public void TestCostingAutoratingDateOverride()
		{
			var businessObject = Factory.New<DummyBusinessObject>();
			var jobDatesProvider = new JobDatesProviderForTest(businessObject);

			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CostingAutoratingDateOverride));

			var testDate = new ZDateTime(2014, 5, 14);
			jobDatesProvider.CostingAutoratingDate = testDate;
			AssertEquals(testDate, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CostingAutoratingDateOverride));
		}

		public void TestRevenueAutoratingDateOverride()
		{
			var businessObject = Factory.New<DummyBusinessObject>();
			var jobDatesProvider = new JobDatesProviderForTest(businessObject);

			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.RevenueAutoratingDateOverride));

			var testDate = new ZDateTime(2014, 5, 14);
			jobDatesProvider.RevenueAutoratingDate = testDate;
			AssertEquals(testDate, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.RevenueAutoratingDateOverride));
		}

		public class JobDatesProviderForTest : JobDatesProvider<DummyBusinessObject>
		{
			public JobDatesProviderForTest(DummyBusinessObject parent)
				: base(parent)
			{
			}

			public ZDateTime DepartureDate { get; set; }
			public ZDateTime ArrivalDate { get; set; }
			public ZDateTime CostingAutoratingDate { get; set; }
			public ZDateTime RevenueAutoratingDate { get; set; }

			protected override ZDateTime GetDepartureDateCore()
			{
				return DepartureDate;
			}

			protected override ZDateTime GetArrivalDateCore()
			{
				return ArrivalDate;
			}

			protected override ZDateTime GetCostingAutoratingDateOverrideCore()
			{
				return CostingAutoratingDate;
			}

			protected override ZDateTime GetRevenueAutoratingDateOverrideCore()
			{
				return RevenueAutoratingDate;
			}
		}
	}
}
