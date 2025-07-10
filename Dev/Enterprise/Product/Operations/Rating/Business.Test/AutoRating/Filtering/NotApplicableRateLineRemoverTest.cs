using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Testing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Rating.Business.NotApplicableRateLineRemover;

namespace Enterprise.Rating.Business.Test
{
	public class NotApplicableRateLineRemoverTest : RatingTestCase
	{
		protected TestObjectCreator TestObjectCreator;

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory);
		}

		[TestDate(2020, 1, 1)]
		public void TestRemoveRateLinesOutOfDateRange_CarrierContract_AutoratingDateFilter()
		{
			var org = TestHelper.NewOrgHeader();
			var ratingContract = TestHelper.NewRatingContract(org, "C1234", RatingContractTypes.Provider, transportMode: TransportModes.Air);
			ratingContract.RCT_AutoratingDateFiltering = RatingDateFilterTypes.Codes.Arrival;
			TestHelper.Factory.Save();

			var ratingHeader = TestHelper.NewCosting(TransportProvider1);
			var rateEntry1 = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "BAF", 10m);
			rateEntry1.RateLines[0].TL_RateStartDate = new ZDate(2020, 2, 10);
			rateEntry1.RateLines[0].TL_RateEndDate = new ZDate(2020, 2, 10);

			var rateEntry2 = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "CAF", 20m);
			rateEntry2.RateLines[0].TL_RateStartDate = new ZDate(2020, 2, 20);
			rateEntry2.RateLines[0].TL_RateEndDate = new ZDate(2020, 2, 20);

			var mockRepository = new MockRepository(MockBehavior.Loose);
			var jobDatesMock = mockRepository.Create<IJobDatesProvider>();
			jobDatesMock.Setup(jobDatesProvider => jobDatesProvider
				.GetJobDateByType(JobDateTypes.Codes.ArrivalDate, It.IsAny<string>()))
				.Returns(new ZDate(2020, 2, 10));
			var criteria = new TestRatingCriteria();
			criteria.JobDatesProvider = jobDatesMock.Object;
			criteria.Carrier = TransportProvider1;
			criteria.SetCarrierContractNumber("C1234");
			criteria.ShouldUseCarrierContractDateFilter = true;

			using (FreightConfigurationRegistry.Instance.EnableCarrierContractTariffsAndRates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var rateLinesRepository = new RateLinesRepository(criteria, new List<IRateEntry>(new[] { rateEntry1, rateEntry2 }), new TestLogger());
				NotApplicableRateLineRemover.RemoveRateLinesOutOfDateRange(criteria, rateLinesRepository, isCosting: true);
				AssertContainsExactElementsInAnyOrder
				(
					"RemoveRateLineOutOfDateRange should use Carrier's Contract AutoratingDateFilter",
					expected: new[] { "BAF-FLT-Costing TRASPROV1" },
					rateLinesRepository.GetLines().Select(fastLine => fastLine.DisplayInfo())
				);
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestRemoveRateLinesOutOfDateRange_CarrierContract_AutoratingDateFilter_Custom()
		{
			var org = TestHelper.NewOrgHeader();
			var ratingContract = TestHelper.NewRatingContract(org, "C1234", RatingContractTypes.Provider, transportMode: TransportModes.Air);
			ratingContract.RCT_AutoratingDateFiltering = RatingDateFilterTypes.Codes.Custom;

			var ratingDateConfig1 = TestHelper.Factory.New<RatingDateConfig>();
			ratingDateConfig1.RDT_ParentTableCode = RatingContractSchema.Constants.Prefix;
			ratingDateConfig1.RDT_ParentID = ratingContract.PK;
			ratingDateConfig1.RDT_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			ratingDateConfig1.RDT_JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			ratingDateConfig1.RDT_Direction = FreightShipmentDirection.Code.Import;
			ratingDateConfig1.RDT_TransportMode = TransportModes.Sea;
			ratingDateConfig1.RDT_RateType = JobRateTypes.Codes.Cost;
			ratingDateConfig1.RDT_AutoratingDate = JobDateTypes.Codes.DepartureDate;

			var ratingDateConfig2 = TestHelper.Factory.New<RatingDateConfig>();
			ratingDateConfig2.RDT_ParentTableCode = RatingContractSchema.Constants.Prefix;
			ratingDateConfig2.RDT_ParentID = ratingContract.PK;
			ratingDateConfig2.RDT_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			ratingDateConfig2.RDT_JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			ratingDateConfig2.RDT_Direction = FreightShipmentDirection.Code.Export;
			ratingDateConfig2.RDT_TransportMode = TransportModes.Sea;
			ratingDateConfig2.RDT_RateType = JobRateTypes.Codes.Cost;
			ratingDateConfig2.RDT_AutoratingDate = JobDateTypes.Codes.ArrivalDate;

			TestHelper.Factory.Save();

			var ratingHeader = TestHelper.NewCosting(TransportProvider1);
			var rateEntry1 = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "BAF", 10m);
			rateEntry1.RateLines[0].TL_RateStartDate = new ZDate(2020, 2, 10);
			rateEntry1.RateLines[0].TL_RateEndDate = new ZDate(2020, 2, 10);

			var rateEntry2 = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "CAF", 20m);
			rateEntry2.RateLines[0].TL_RateStartDate = new ZDate(2020, 2, 20);
			rateEntry2.RateLines[0].TL_RateEndDate = new ZDate(2020, 2, 20);

			var mockRepository = new MockRepository(MockBehavior.Loose);
			var jobDatesMock = mockRepository.Create<IJobDatesProvider>();
			jobDatesMock.Setup(jobDatesProvider => jobDatesProvider
				.GetJobDateByType(JobDateTypes.Codes.ArrivalDate, It.IsAny<string>()))
				.Returns(new ZDate(2020, 2, 10));
			var criteria = new TestRatingCriteria();
			criteria.JobDatesProvider = jobDatesMock.Object;
			criteria.ConsumerType = JobInvoicingConsumerTypes.ForwardingConsol;
			criteria.FreightMode = FreightMode.SEA;
			criteria.Carrier = TransportProvider1;
			criteria.JobDirection = Directions.Export;
			criteria.SetCarrierContractNumber("C1234");
			criteria.ShouldUseCarrierContractDateFilter = true;

			using (_Rating.Start(new LoggerDecorator()))
			{
				_Rating.StartCost();
				var rateLinesRepository = new RateLinesRepository(criteria, new List<IRateEntry>(new[] { rateEntry1, rateEntry2 }), new TestLogger());
				NotApplicableRateLineRemover.RemoveRateLinesOutOfDateRange(criteria, rateLinesRepository, isCosting: true);
				AssertContainsExactElementsInAnyOrder
				(
					"RemoveRateLineOutOfDateRange should use Carrier's Contract > Custom",
					expected: new[] { "BAF-FLT-Costing TRASPROV1" },
					rateLinesRepository.GetLines().Select(fastLine => fastLine.DisplayInfo())
				);
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestRemoveRateLinesOutOfDateRange_ContractTakesPriorityOverCarrier_AutoratingDateFilter()
		{
			var org = TestHelper.NewOrgHeader();
			var ratingContract = TestHelper.NewRatingContract(org, "C1234", RatingContractTypes.Provider, transportMode: TransportModes.Air);
			ratingContract.RCT_AutoratingDateFiltering = RatingDateFilterTypes.Codes.Departure;
			TestHelper.Factory.Save();

			var ratingHeader = TestHelper.NewCosting(TransportProvider1);
			var rateEntry1 = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "BAF", 10m);
			rateEntry1.RateLines[0].TL_RateStartDate = new ZDate(2020, 2, 10);
			rateEntry1.RateLines[0].TL_RateEndDate = new ZDate(2020, 2, 10);

			var rateEntry2 = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "CAF", 20m);
			rateEntry2.RateLines[0].TL_RateStartDate = new ZDate(2020, 2, 20);
			rateEntry2.RateLines[0].TL_RateEndDate = new ZDate(2020, 2, 20);

			var mockRepository = new MockRepository(MockBehavior.Loose);
			var jobDatesMock = mockRepository.Create<IJobDatesProvider>();
			jobDatesMock.Setup(jobDatesProvider => jobDatesProvider
				.GetJobDateByType(JobDateTypes.Codes.ArrivalDate, It.IsAny<string>()))
				.Returns(new ZDate(2020, 2, 10));
			var criteria = new TestRatingCriteria();
			criteria.JobDatesProvider = jobDatesMock.Object;
			criteria.Carrier = TransportProvider1;

			TransportProvider1.MiscServ.OM_AutoratingDateFiltering = RatingDateFilterTypes.Codes.Arrival;

			var rateLinesRepository = new RateLinesRepository(criteria, new List<IRateEntry>(new[] { rateEntry1, rateEntry2 }), new TestLogger());
			NotApplicableRateLineRemover.RemoveRateLinesOutOfDateRange(criteria, rateLinesRepository, isCosting: true);
			AssertContainsExactElementsInAnyOrder
			(
				"RemoveRateLineOutOfDateRange should use Organization AutoratingDateFilter",
				expected: new[] { "BAF-FLT-Costing TRASPROV1" },
				rateLinesRepository.GetLines().Select(fastLine => fastLine.DisplayInfo())
			);

			jobDatesMock.Setup(jobDatesProvider => jobDatesProvider
			.GetJobDateByType(JobDateTypes.Codes.DepartureDate, It.IsAny<string>()))
			.Returns(new ZDate(2020, 2, 20));

			criteria.SetCarrierContractNumber("C1234");
			criteria.ShouldUseCarrierContractDateFilter = true;

			rateLinesRepository = new RateLinesRepository(criteria, new List<IRateEntry>(new[] { rateEntry1, rateEntry2 }), new TestLogger());
			NotApplicableRateLineRemover.RemoveRateLinesOutOfDateRange(criteria, rateLinesRepository, isCosting: true);

			AssertContainsExactElementsInAnyOrder
			(
				"RemoveRateLineOutOfDateRange should use Contract AutoratingDateFilter instead of Organization AutoratingDateFilter",
				expected: new[] { "CAF-FLT-Costing TRASPROV1" },
				rateLinesRepository.GetLines().Select(fastLine => fastLine.DisplayInfo())
			);
		}

		[TestDate(2020, 1, 1)]
		public void TestRemoveRateLinesOutOfDateRange_Organization_AutoratingDateFilter_Creditor()
		{
			var ratingHeader = TestHelper.NewCosting(TransportProvider1);
			var rateEntry1 = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "BAF", 10m);
			rateEntry1.RateLines[0].TL_RateStartDate = new ZDate(2020, 2, 10);
			rateEntry1.RateLines[0].TL_RateEndDate = new ZDate(2020, 2, 10);

			var rateEntry2 = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "CAF", 20m);
			rateEntry2.RateLines[0].TL_RateStartDate = new ZDate(2020, 2, 20);
			rateEntry2.RateLines[0].TL_RateEndDate = new ZDate(2020, 2, 20);

			var mockRepository = new MockRepository(MockBehavior.Loose);
			var jobDatesMock = mockRepository.Create<IJobDatesProvider>();
			jobDatesMock.Setup(jobDatesProvider => jobDatesProvider
				.GetJobDateByType(JobDateTypes.Codes.ArrivalDate, It.IsAny<string>()))
				.Returns(new ZDate(2020, 2, 10));
			var criteria = new TestRatingCriteria();
			criteria.JobDatesProvider = jobDatesMock.Object;
			criteria.Creditors = Creditors.New(OrgWithSource.New(TransportProvider1, new List<string>() { "Provider" }));

			TransportProvider1.MiscServ.OM_AutoratingDateFiltering = RatingDateFilterTypes.Codes.Arrival;

			var rateLinesRepository = new RateLinesRepository(criteria, new List<IRateEntry>(new[] { rateEntry1, rateEntry2 }), new TestLogger());
			NotApplicableRateLineRemover.RemoveRateLinesOutOfDateRange(criteria, rateLinesRepository, true);
			AssertContainsExactElementsInAnyOrder
			(
				"RemoveRateLineOutOfDateRange should use Organization AutoratingDateFilter",
				expected: new[] { "BAF-FLT-Costing TRASPROV1" },
				rateLinesRepository.GetLines().Select(fastLine => fastLine.DisplayInfo())
			);
		}

		[TestDate(2020, 1, 1)]
		public void TestRemoveRateLinesOutOfDateRange_Organization_AutoratingDateFilter_CarrierAndCreditor()
		{
			var transportProvider1 = TransportProvider1;
			var transportProvider2 = TransportProvider2;

			var ratingHeader1 = TestHelper.NewCosting(transportProvider1);
			var rateEntry11 = ratingHeader1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "BAF", 11m);
			rateEntry11.RateLines[0].TL_RateStartDate = new ZDate(2020, 2, 10);
			rateEntry11.RateLines[0].TL_RateEndDate = new ZDate(2020, 2, 10);
			var rateEntry12 = ratingHeader1.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "CAF", 12m);
			rateEntry12.RateLines[0].TL_RateStartDate = new ZDate(2020, 2, 20);
			rateEntry12.RateLines[0].TL_RateEndDate = new ZDate(2020, 2, 20);

			var ratingHeader2 = TestHelper.NewCosting(TransportProvider2);
			var rateEntry21 = ratingHeader2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", 21m);
			rateEntry21.RateLines[0].TL_RateStartDate = new ZDate(2020, 4, 10);
			rateEntry21.RateLines[0].TL_RateEndDate = new ZDate(2020, 4, 10);
			var rateEntry22 = ratingHeader2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "WAR", 22m);
			rateEntry22.RateLines[0].TL_RateStartDate = new ZDate(2020, 4, 20);
			rateEntry22.RateLines[0].TL_RateEndDate = new ZDate(2020, 4, 20);

			var mockRepository = new MockRepository(MockBehavior.Loose);
			var jobDatesMock = mockRepository.Create<IJobDatesProvider>();
			jobDatesMock.Setup(jobDatesProvider => jobDatesProvider
				.GetJobDateByType(JobDateTypes.Codes.ArrivalDate, It.IsAny<string>()))
				.Returns(new ZDate(2020, 2, 10));
			jobDatesMock.Setup(jobDatesProvider => jobDatesProvider
				.GetJobDateByType(JobDateTypes.Codes.DepartureDate, It.IsAny<string>()))
				.Returns(new ZDate(2020, 4, 10));
			var criteria = new TestRatingCriteria();
			criteria.JobDatesProvider = jobDatesMock.Object;
			criteria.Carrier = TransportProvider1;
			criteria.Creditors = Creditors.New(OrgWithSource.New(transportProvider2, new List<string>() { "Provider" }));

			transportProvider1.MiscServ.OM_AutoratingDateFiltering = RatingDateFilterTypes.Codes.Arrival;
			transportProvider2.MiscServ.OM_AutoratingDateFiltering = RatingDateFilterTypes.Codes.Departure;

			var rateLinesRepository = new RateLinesRepository(criteria, new List<IRateEntry>(new[] { rateEntry11, rateEntry12, rateEntry21, rateEntry22 }), new TestLogger());
			NotApplicableRateLineRemover.RemoveRateLinesOutOfDateRange(criteria, rateLinesRepository, true);
			AssertContainsExactElementsInAnyOrder
			(
				"RemoveRateLineOutOfDateRange should use Organization AutoratingDateFilter",
				expected: new[] { "BAF-FLT-Costing TRASPROV1", "FRT-FLT-Costing TRASPROV2" },
				rateLinesRepository.GetLines().Select(fastLine => fastLine.DisplayInfo())
			);
		}

		[TestDate(2020, 1, 1)]
		public void TestRemoveRateLinesOutOfDateRange_Organization_AutoratingDateFilter()
		{
			var ratingHeader = TestHelper.NewCosting(TransportProvider1);
			var rateEntry1 = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "BAF", 10m);
			rateEntry1.RateLines[0].TL_RateStartDate = new ZDate(2020, 2, 10);
			rateEntry1.RateLines[0].TL_RateEndDate = new ZDate(2020, 2, 10);

			var rateEntry2 = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "CAF", 20m);
			rateEntry2.RateLines[0].TL_RateStartDate = new ZDate(2020, 2, 20);
			rateEntry2.RateLines[0].TL_RateEndDate = new ZDate(2020, 2, 20);

			var mockRepository = new MockRepository(MockBehavior.Loose);
			var jobDatesMock = mockRepository.Create<IJobDatesProvider>();
			jobDatesMock.Setup(jobDatesProvider => jobDatesProvider
				.GetJobDateByType(JobDateTypes.Codes.ArrivalDate, It.IsAny<string>()))
				.Returns(new ZDate(2020, 2, 10));
			var criteria = new TestRatingCriteria();
			criteria.JobDatesProvider = jobDatesMock.Object;
			criteria.Carrier = TransportProvider1;

			TransportProvider1.MiscServ.OM_AutoratingDateFiltering = RatingDateFilterTypes.Codes.Arrival;

			var rateLinesRepository = new RateLinesRepository(criteria, new List<IRateEntry>(new[] { rateEntry1, rateEntry2 }), new TestLogger());
			NotApplicableRateLineRemover.RemoveRateLinesOutOfDateRange(criteria, rateLinesRepository, true);
			AssertContainsExactElementsInAnyOrder
			(
				"RemoveRateLineOutOfDateRange should use Organization AutoratingDateFilter",
				expected: new[] { "BAF-FLT-Costing TRASPROV1" },
				rateLinesRepository.GetLines().Select(fastLine => fastLine.DisplayInfo())
			);
		}

		[TestDate(2020, 1, 1)]
		public void TestRemoveRateLinesOutOfDateRange_Organization_AutoratingDateFilter_Custom()
		{
			var ratingHeader = TestHelper.NewCosting(TransportProvider1);
			var rateEntry1 = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "BAF", 10m);
			rateEntry1.RateLines[0].TL_RateStartDate = new ZDate(2020, 2, 10);
			rateEntry1.RateLines[0].TL_RateEndDate = new ZDate(2020, 2, 10);

			var rateEntry2 = ratingHeader.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "CAF", 20m);
			rateEntry2.RateLines[0].TL_RateStartDate = new ZDate(2020, 2, 20);
			rateEntry2.RateLines[0].TL_RateEndDate = new ZDate(2020, 2, 20);

			var mockRepository = new MockRepository(MockBehavior.Loose);
			var jobDatesMock = mockRepository.Create<IJobDatesProvider>();
			jobDatesMock.Setup(jobDatesProvider => jobDatesProvider
				.GetJobDateByType(JobDateTypes.Codes.ArrivalDate, It.IsAny<string>()))
				.Returns(new ZDate(2020, 2, 10));
			var criteria = new TestRatingCriteria();
			criteria.JobDatesProvider = jobDatesMock.Object;
			criteria.ConsumerType = JobInvoicingConsumerTypes.ForwardingConsol;
			criteria.FreightMode = FreightMode.SEA;
			criteria.Carrier = TransportProvider1;
			criteria.JobDirection = Directions.Export;

			TransportProvider1.MiscServ.OM_AutoratingDateFiltering = RatingDateFilterTypes.Codes.Custom;

			var ratingDateConfig1 = Factory.New<RatingDateConfig>();
			ratingDateConfig1.RDT_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			ratingDateConfig1.RDT_ParentID = TransportProvider1.PK;
			ratingDateConfig1.RDT_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			ratingDateConfig1.RDT_JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			ratingDateConfig1.RDT_Direction = FreightShipmentDirection.Code.Import;
			ratingDateConfig1.RDT_TransportMode = TransportModes.Sea;
			ratingDateConfig1.RDT_RateType = JobRateTypes.Codes.Cost;
			ratingDateConfig1.RDT_AutoratingDate = JobDateTypes.Codes.DepartureDate;

			var ratingDateConfig2 = Factory.New<RatingDateConfig>();
			ratingDateConfig2.RDT_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			ratingDateConfig2.RDT_ParentID = TransportProvider1.PK;
			ratingDateConfig2.RDT_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			ratingDateConfig2.RDT_JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			ratingDateConfig2.RDT_Direction = FreightShipmentDirection.Code.Export;
			ratingDateConfig2.RDT_TransportMode = TransportModes.Sea;
			ratingDateConfig2.RDT_RateType = JobRateTypes.Codes.Cost;
			ratingDateConfig2.RDT_AutoratingDate = JobDateTypes.Codes.ArrivalDate;

			using (_Rating.Start(new LoggerDecorator()))
			{
				_Rating.StartCost();
				var rateLinesRepository = new RateLinesRepository(criteria, new List<IRateEntry>(new[] { rateEntry1, rateEntry2 }), new TestLogger());
				NotApplicableRateLineRemover.RemoveRateLinesOutOfDateRange(criteria, rateLinesRepository, true);
				AssertContainsExactElementsInAnyOrder
				(
					"RemoveRateLineOutOfDateRange should use Organization > AutoratingDateFilter > Custom",
					expected: new[] { "BAF-FLT-Costing TRASPROV1" },
					rateLinesRepository.GetLines().Select(fastLine => fastLine.DisplayInfo())
				);
			}
		}

		public void TestRemoveRateLinesOutOfDateRange_RateLineDates()
		{
			var ratingHeader = TestHelper.NewCosting(null);
			var entry = ratingHeader.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			var jobDate = entry.TI_RateStartDate.AddDays(10);

			var mockRepository = new MockRepository(MockBehavior.Loose);
			var logger = new TestLogger();
			var criteria = new TestRatingCriteria();
			var jobDatesMock = mockRepository.Create<IJobDatesProvider>();
			jobDatesMock.Setup(x => x.GetJobDateByType(It.IsAny<ZString>(), It.IsAny<string>())).Returns(jobDate);
			criteria.JobDatesProvider = jobDatesMock.Object;

			var line = entry.AddFlatRateLine("FRT", 1);

			CombineAssertions(() =>
			{
				AssertEquals("no dates", false, IsLineRemoved(criteria, line, ZDate.Empty, ZDate.Empty));

				AssertEquals("start date in range", false, IsLineRemoved(criteria, line, jobDate, ZDate.Empty));
				AssertEquals("start date after job date", true, IsLineRemoved(criteria, line, jobDate.AddDays(1), ZDate.Empty));

				AssertEquals("end date in range", false, IsLineRemoved(criteria, line, ZDate.Empty, jobDate));
				AssertEquals("end date before job date", true, IsLineRemoved(criteria, line, ZDate.Empty, jobDate.AddDays(-1)));

				AssertEquals("both dates as job date", false, IsLineRemoved(criteria, line, jobDate, jobDate));
				AssertEquals("job date in middle", false, IsLineRemoved(criteria, line, jobDate.AddDays(-1), jobDate.AddDays(1)));
				AssertEquals("both dates too high", true, IsLineRemoved(criteria, line, jobDate.AddDays(1), jobDate.AddDays(2)));
				AssertEquals("both dates too low", true, IsLineRemoved(criteria, line, jobDate.AddDays(-2), jobDate.AddDays(-1)));
			});
		}

		[TestDate(2020, 1, 1)]
		public void TestRemoveRateLinesOutOfDateRange_AutoratingDateOverride()
		{
			var ratingHeader = TestHelper.NewCosting(null);
			var entry = ratingHeader.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			var line = entry.AddFlatRateLine("FRT", 1);

			// autorating date override is 20 days before job date
			var autoratingDateOverride = entry.TI_RateStartDate.AddDays(-10);
			var jobDate = entry.TI_RateStartDate.AddDays(10);

			var mockRepository = new MockRepository(MockBehavior.Loose);
			var criteria = new TestRatingCriteria();
			var jobDatesMock = mockRepository.Create<IJobDatesProvider>();
			jobDatesMock.Setup(x => x.GetJobDateByType(It.IsAny<ZString>(), It.IsAny<string>())).Returns(jobDate);
			criteria.JobDatesProvider = jobDatesMock.Object;

			jobDatesMock.Setup(x => x.GetJobDateByType(JobDateTypes.Codes.CostingAutoratingDateOverride, It.IsAny<string>())).Returns(ZDate.Empty);
			CombineAssertions("When Autorating Date Override presents but does not override job date", () =>
			{
				Assert("no dates", !IsLineRemoved(criteria, line, ZDate.Empty, ZDate.Empty));

				Assert("start date = autorating date override", !IsLineRemoved(criteria, line, autoratingDateOverride, ZDate.Empty));
				Assert("autorating date override < start date < job date", !IsLineRemoved(criteria, line, jobDate.AddDays(-10), ZDate.Empty));
				Assert("start date = job date", !IsLineRemoved(criteria, line, jobDate, ZDate.Empty));
				Assert("job date < start date", IsLineRemoved(criteria, line, jobDate.AddDays(1), ZDate.Empty));

				Assert("end date < autorating date override", IsLineRemoved(criteria, line, ZDate.Empty, autoratingDateOverride.AddDays(-1)));
				Assert("end date = autorating date override", IsLineRemoved(criteria, line, ZDate.Empty, autoratingDateOverride));
				Assert("autorating date override < end date < job date", IsLineRemoved(criteria, line, ZDate.Empty, jobDate.AddDays(-1)));
				Assert("end date = job date", !IsLineRemoved(criteria, line, ZDate.Empty, jobDate));

				Assert("both dates as autorating date override", IsLineRemoved(criteria, line, autoratingDateOverride, autoratingDateOverride));
				Assert("both dates as job date", !IsLineRemoved(criteria, line, jobDate, jobDate));
				Assert("start date < autorating date override < end date < job date", IsLineRemoved(criteria, line, autoratingDateOverride.AddDays(-1), autoratingDateOverride.AddDays(1)));
				Assert("job date in middle, start date < autorating date override", !IsLineRemoved(criteria, line, autoratingDateOverride.AddDays(-1), jobDate.AddDays(1)));
				Assert("job date in middle, start date > autorating date override", !IsLineRemoved(criteria, line, autoratingDateOverride.AddDays(1), jobDate.AddDays(1)));
				Assert("both dates too high", IsLineRemoved(criteria, line, jobDate.AddDays(1), jobDate.AddDays(2)));
				Assert("both dates too low", IsLineRemoved(criteria, line, autoratingDateOverride.AddDays(-2), autoratingDateOverride.AddDays(-1)));
			});

			jobDatesMock.Setup(x => x.GetJobDateByType(JobDateTypes.Codes.CostingAutoratingDateOverride, It.IsAny<string>())).Returns(autoratingDateOverride);
			CombineAssertions("When Autorating Date Override presents and overrides job date", () =>
			{
				Assert("no dates, start date falls back to rate entry start date (today): autorate date override < start date", IsLineRemoved(criteria, line, ZDate.Empty, ZDate.Empty));
				entry.TI_RateStartDate = autoratingDateOverride.AddDays(-10);
				Assert("no dates, start date (fallback) < autorate date override", !IsLineRemoved(criteria, line, ZDate.Empty, ZDate.Empty));

				Assert("start date = autorating date override", !IsLineRemoved(criteria, line, autoratingDateOverride, ZDate.Empty));
				Assert("autorating date override < start date < job date", IsLineRemoved(criteria, line, jobDate.AddDays(-10), ZDate.Empty));
				Assert("start date = job date", IsLineRemoved(criteria, line, jobDate, ZDate.Empty));
				Assert("job date < start date", IsLineRemoved(criteria, line, jobDate.AddDays(1), ZDate.Empty));

				Assert("end date < autorating date override", IsLineRemoved(criteria, line, ZDate.Empty, autoratingDateOverride.AddDays(-1)));
				Assert("end date = autorating date override", !IsLineRemoved(criteria, line, ZDate.Empty, autoratingDateOverride));
				Assert("autorating date override < end date < job date", !IsLineRemoved(criteria, line, ZDate.Empty, jobDate.AddDays(-1)));
				Assert("end date = job date", !IsLineRemoved(criteria, line, ZDate.Empty, jobDate));

				Assert("both dates as autorating date override", !IsLineRemoved(criteria, line, autoratingDateOverride, autoratingDateOverride));
				Assert("both dates as job date", IsLineRemoved(criteria, line, jobDate, jobDate));
				Assert("start date < autorating date override < job date < end date", !IsLineRemoved(criteria, line, autoratingDateOverride.AddDays(-1), jobDate.AddDays(1)));
				Assert("start date < autorating date override < end date < job date", !IsLineRemoved(criteria, line, autoratingDateOverride.AddDays(-1), autoratingDateOverride.AddDays(1)));
				Assert("autorating date override < start date < job date < end date", IsLineRemoved(criteria, line, autoratingDateOverride.AddDays(1), jobDate.AddDays(1)));
				Assert("both dates too high", IsLineRemoved(criteria, line, jobDate.AddDays(1), jobDate.AddDays(2)));
				Assert("both dates too low", IsLineRemoved(criteria, line, autoratingDateOverride.AddDays(-2), autoratingDateOverride.AddDays(-1)));
			});
		}

		static bool IsLineRemoved(RatingCriteria criteria, RateLine line, ZDate lineStartDate, ZDate lineEndDate)
		{
			line.TL_RateStartDate = lineStartDate;
			line.TL_RateEndDate = lineEndDate;
			var entryList = new List<IRateEntry>() { line.ParentRateEntry };
			var repo = new RateLinesRepository(criteria, entryList, new TestLogger());
			NotApplicableRateLineRemover.RemoveRateLinesOutOfDateRange(criteria, repo, true);
			return repo.GetLines().Count == 0;
		}

		public void TestRemoveRateLines_InaccessibleToUser()
		{
			var charge = Helper.ChargeCodes.New("TCHRG", "Test Charge", UnitCalculator.Code);
			charge.AC_DepartmentFilterList = "FEA";
			var ratingHeader = TestHelper.NewCosting(null);
			var entry = ratingHeader.AddRateEntry(RatingConstants.RateCategory.AIR, removeLines: true);
			var line = entry.AddFlatRateLine("TCHRG", 1);
			var criteria = new TestRatingCriteria();

			#region Security Setup
			var testUser = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var securityFactory = new BusinessObjectFactory();

			var security = new UserLoginController().GetSecurityForUser(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			// Don't allow user to access FEA.
			var loginSecurityCurrentBranch = securityFactory.New<GlbSecurity>();
			loginSecurityCurrentBranch.GU_GB = Env.CurrentBranch.PK;
			loginSecurityCurrentBranch.GU_GE = TestObjectCreator.FEADepartment.PK;
			loginSecurityCurrentBranch.GU_GS = testUser.PK;
			loginSecurityCurrentBranch.GU_SecurityRight = security.Login.Code;
			loginSecurityCurrentBranch.GU_SecurityItemIsAllowed = false;

			// Disallow the user modifying charges outside their department (FES).
			var chargeSecurity = securityFactory.New<GlbSecurity>();
			chargeSecurity.GU_GB = Env.CurrentBranch.PK;
			chargeSecurity.GU_GE = TestObjectCreator.FESDepartment.PK;
			chargeSecurity.GU_GS = testUser.PK;
			chargeSecurity.GU_SecurityRight = security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowEnterModifyCharges).Code;
			chargeSecurity.GU_SecurityItemIsAllowed = false;
			securityFactory.Save();
			#endregion

			var entryList = new List<IRateEntry>() { line.ParentRateEntry };
			var repo = new RateLinesRepository(criteria, entryList, new TestLogger());

			using (Env.SetTemporaryUserContext(testUser.GS_LoginName, Env.CurrentBranch.PK, TestObjectCreator.FESDepartment.PK.ToGuid()))
			{
				NotApplicableRateLineRemover.RemoveChargeCodesFromUnauthorizedDepartments(criteria, repo);

				AssertEquals(message: "testUser shouldn't have access to autorate this charge code", 0, repo.GetLines().Count);
			}

			entryList = new List<IRateEntry>() { line.ParentRateEntry };
			repo = new RateLinesRepository(criteria, entryList, new TestLogger());
			NotApplicableRateLineRemover.RemoveChargeCodesFromUnauthorizedDepartments(criteria, repo);

			AssertEquals(message: "the normal test environment should have permission to autorate this charge code", 1, repo.GetLines().Count);
		}

		public void TestRemoveRateLines_WhenRegistryPermits_DontRemoveDuplicateChargeCodes()
		{
			var collection = new SameChargeCodeDifferentProviderCollection();
			var rule = collection.AddNew();
			rule.JobType = "SHP";
			rule.TransportMode = "SEA";
			rule.Direction = "ALL";
			rule.IsEnabled = true;

			var provider = Helper.NewAirlineOrg("QF");

			var standardCost = Helper.NewCosting(null);
			var standardCostEntry = standardCost.AddRateEntry("FCL", mode: "SEA", removeLines: true);
			var standardCostLine = standardCostEntry.AddFlatCharge("DFSC", 250);
			standardCostEntry.TI_OH_TransportProvider = provider.PK;

			var costing1 = Helper.NewCosting(provider);
			var costEntry1 = costing1.AddRateEntry("FCL", mode: "SEA", serviceLevel: "STD", removeLines: true);
			var costLine1 = costEntry1.AddFlatCharge("DFSC", 100);

			var costing2 = Helper.NewCosting(Helper.NewOrgHeader());
			var costEntry2 = costing2.AddRateEntry("FCL", mode: "SEA", removeLines: true);
			var costLine2 = costEntry2.AddUnitCharge("DFSC", 100, "CN");

			var criteria = new TestRatingCriteria { FreightMode = FreightMode.FCL, Creditors = Creditors.New(OrgWithSource.New(provider, new List<string> { "provider" })) };
			criteria.RateableMeasures.AddContainerGroup(Helper.Containers["20GP"].PK, new[]
			{
				new MeasureInfo.ContainerInfo(100m, "KG", 1m, "M3", 1, 1, "12345")
			});

			var linesRepository = new RateLinesRepository(criteria, new[] { (IRateEntry)costEntry1, costEntry2, standardCostEntry }.ToList(), new DummyLogger());
			var matcher = new NotApplicableRateLineRemover();
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);

			// No registry value set - only one line should be returned.
			matcher.RemoveNotApplicable(parameters, linesRepository, new FilterOptions(), null);
			var leftLines = linesRepository.GetLines().Select(l => l.Line.PK).ToList();

			AssertEquals(1, leftLines.Count);
			AssertContainsExactElementsInAnyOrder(
				"Only one cost line should come through as the registry does not allow duplicates.",
				new[] { costLine1.PK },
				leftLines);

			// Registry value set - both lines should be returned.
			using (DataRegistryRating.Instance.AllowChargesWithSameChargeCodeForDifferentProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				linesRepository = new RateLinesRepository(criteria, new[] { (IRateEntry)costEntry1, costEntry2, standardCostEntry }.ToList(), new DummyLogger());
				matcher.RemoveNotApplicable(parameters, linesRepository, new FilterOptions(), null);
				leftLines = linesRepository.GetLines().Select(l => l.Line.PK).ToList();

				AssertEquals(2, leftLines.Count);
				AssertContainsExactElementsInAnyOrder(
					"Both cost lines should come through as they have different service providers and the registry allows it.",
					new[] { costLine1.PK, costLine2.PK },
					leftLines);
			}
		}

		public void TestRemoveIrrelevantCosts()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_IsShippingLine = true;
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_IsShippingLine = true;

			// For costs, FeeChargeType/Level is only used (editable) for standard costs, not provider specific costs
			var standardCosts = TestHelper.NewCosting(null);
			var entry = standardCosts.AddRateEntry(RatingConstants.RateCategory.ORG, removeLines: true);
			var line = entry.AddFlatRateLine("FRT", 100);
			line.TL_FeeChargeType = "FSE";
			line.TL_FeeChargeLevel = "STD";

			var feeChargeLevel = carrier1.CompanyData.RateFeeChargeLevels.AddNew();
			feeChargeLevel.ORF_ServiceType = "FSE";
			feeChargeLevel.ORF_Level = "STD";

			var criteria = new TestRatingCriteria();
			criteria.PossibleCarriers = new[] { carrier1, carrier2 };

			var entryList = new List<IRateEntry>() { line.ParentRateEntry };
			var repo = new RateLinesRepository(criteria, entryList, new TestLogger());
			NotApplicableRateLineRemover.RemoveIrrelevantCosts(criteria, repo);
			Assert("cost is not removed since it matches a possible carrier FeeChargeLevel", repo.GetLines().Count == 1);
		}

		public void TestRemoveInvalidLines_WhenRateLineMissRequiredMeasures()
		{
			var org = Helper.NewOrgHeader();

			var rate = Helper.NewClientRate(null);
			var rateEntry = rate.AddRateEntry("AIR", "LSE", "AUSYD", "KRSEL", "", "");

			var rateLine = rateEntry.RateLines[0];
			rateLine.TL_RateCalculator = UnitCalculator.Code;
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 5m;
			rate.TH_OH = org.PK;
			rateLine.TL_WeightVolume = "";

			var criteria = new TestRatingCriteria();
			var testLogger = new TestLogger();
			var linesRepository = new RateLinesRepository(criteria, new List<IRateEntry> { rateEntry }, testLogger);
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var remover = new NotApplicableRateLineRemover();
			var filterOptions = new FilterOptions { DisableSpotFilter = true };
			var ratingContext = new RatingContext();
			remover.RemoveNotApplicable(parameters, linesRepository, filterOptions, ratingContext.DialogService);
			var leftLines = linesRepository.GetLines();

			AssertEquals("There should no rate lines.", 0, leftLines.Count);

			linesRepository.Dispose(); // for closing internal logs and write items to the main log

			AssertCollectionContains(
				"Info:RateLine Filtered FRT-UNT-Client Rate TESTORG1\treason:\tPlease enter a Units.\r\n",
				testLogger.Infos
			);
		}

		public void TestRemoveIrrelevantCompanyTariffs_WhenCompanyTariffLevelOverrideIsNotZero()
		{
			var glbTariff = Factory.New<GlobalTariff>();
			var glbTariff2 = Factory.New<GlobalTariff>();
			var rateEntry = glbTariff2.AddRateEntry("AIR", "LSE", "AUSYD", "KRSEL", "", "");

			var rateLine = rateEntry.RateLines[0];
			rateLine.TL_RateCalculator = FlatCalculator.Code;
			((FlatCalculator)rateLine.Calculator).BaseRate = 100;
			var autoRatingMock = new Mock<IAutoRating>();
			var mockTariffLevelProvider = autoRatingMock.As<IAutoRatingCompanyTariffLevelProvider>();
			mockTariffLevelProvider.Setup(a => a.TariffLevel).Returns(2);
			var criteria = new TestRatingCriteria(autoRatingMock.Object);
			criteria.OverseasAgent = Factory.New<OrgHeader>();
			criteria.LocalClient = Factory.New<OrgHeader>();
			criteria.Consignee = Factory.New<OrgHeader>();
			criteria.Consignor = Factory.New<OrgHeader>();
			var testLogger = new TestLogger();
			var linesRepository = new RateLinesRepository(criteria, new List<IRateEntry> { rateEntry }, testLogger);
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var remover = new NotApplicableRateLineRemover();
			var filterOptions = new FilterOptions { DisableSpotFilter = true };
			filterOptions.DisablePaymentTermsFilter = true;
			var ratingContext = new RatingContext();

			remover.RemoveNotApplicable(parameters, linesRepository, filterOptions, ratingContext.DialogService);
			var leftLines = linesRepository.GetLines();
			AssertEquals("We should not remove the RateLines from Glb Company Tariff 2 when we set Company Tariff Level Override to 2", 1, leftLines.Count);
		}

		public void TestRemoveCompanyTariffsIrrelevantLinesBasedOnClientRate_WhenClientRateWithEXLCalculatorAndItsConditionIsNotMet_ThenCompanyTariffShouldNotBeFiltered()
		{
			var client = Helper.NewOrgHeader(1);
			var tariff = Helper.NewCompanyTariff();
			var tariffRateEntry = tariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUBNE", "FRT", 200);

			var clientRate = Helper.NewClientRate(client);
			var clientRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUBNE", removeLines: true);
			var clientRateLine = clientRateEntry.AddFlatRateLine("FRT", 200);
			clientRateLine.TL_Condition = RateLineConditions.UserDefined;
			clientRateLine.TL_ConditionalExpression = "\"<Consignee.OH_Code>\"==\"VBCIMPHAM\"";

			var autoRatingMock = new Mock<IAutoRating>();
			var mockTariffLevelProvider = autoRatingMock.As<IAutoRatingCompanyTariffLevelProvider>();
			mockTariffLevelProvider.Setup(a => a.TariffLevel).Returns(1);
			var criteria = new TestRatingCriteria(autoRatingMock.Object);
			criteria.OverseasAgent = Factory.New<OrgHeader>();
			criteria.LocalClient = client;
			criteria.Consignee = Consignee;
			criteria.Consignor = Consignor;
			var testLogger = new TestLogger();

			var linesRepository = new RateLinesRepository(criteria, new List<IRateEntry> { tariffRateEntry, clientRateEntry }, testLogger);
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var remover = new NotApplicableRateLineRemover();
			var filterOptions = new FilterOptions { DisableSpotFilter = true };
			filterOptions.DisablePaymentTermsFilter = true;
			var ratingContext = new RatingContext();

			remover.RemoveNotApplicable(parameters, linesRepository, filterOptions, ratingContext.DialogService);
			var leftLines = linesRepository.GetLines().Select(l => l.Line.ParentRateEntry.ParentRatingHeader.TH_GlobalRateDescription).ToList();

			AssertContainsExactElementsInAnyOrder
			(
				"Company Tariff's rates should not be filtered out when there is a corresponding Client Rate with 'EXL calculator' and its condition is not met.",
				new ZString[] { "Base Company Tariff" },
				leftLines
			);
		}

		public void TestRemoveRemoveIrrelevantGroupClientRates_NoChargeAgentAlwaysCode() =>
			TestRemoveRemoveIrrelevantGroupClientRates(hasChargeAgentAlwaysCodes: false);

		public void TestRemoveRemoveIrrelevantGroupClientRates_HasChargeAgentAlwaysCode() =>
			TestRemoveRemoveIrrelevantGroupClientRates(hasChargeAgentAlwaysCodes: true);

		void TestRemoveRemoveIrrelevantGroupClientRates(bool hasChargeAgentAlwaysCodes)
		{
			var chargeCodePSR = Helper.ChargeCodes["PRS"];
			if (hasChargeAgentAlwaysCodes)
			{
				IncoTermRegistry.Instance.ChargeAgentAlwaysCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeCodePSR.PK.ToString());
			}

			var overseasAgent = Helper.NewOrgHeader("AGENT");
			var managedOrg = Helper.NewOrgHeader("MNGORG");
			var newParty = managedOrg.AllRelatedParties.AddNew();
			newParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			newParty.PR_OH_RelatedParty = Consignee.PK;
			newParty.CompanyLevel = "ENT";

			var clientRateCNE = Helper.NewClientRate(Consignee);
			var rateEntryCNE = clientRateCNE.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUBNE", chargeCodePSR.AC_Code, 10);

			var remover = new NotApplicableRateLineRemover();
			var filterOptions = new FilterOptions { DisableSpotFilter = true };
			filterOptions.DisablePaymentTermsFilter = true;
			var ratingContext = new RatingContext();

			var criteria = new TestRatingCriteria();
			criteria.OverseasAgent = overseasAgent;
			criteria.Consignee = Consignee;
			criteria.Consignor = Consignor;
			var parameters = new AutoRatingCalculatorParametersForTesting(criteria);
			var linesRepository = new RateLinesRepository(criteria, new List<IRateEntry> { rateEntryCNE }, new DummyLogger());
			remover.RemoveNotApplicable(parameters, linesRepository, filterOptions, ratingContext.DialogService);
			AssertEquals("The lines repository should contain exactly one line after removal", 1, linesRepository.GetLines().Count);
		}

		public void TestRemoveIrrelevantCompanyTariffRateLinesBasedOnLevelAndOutOfDateRange()
		{
			var overseasAgent = Helper.NewOrgHeader("AGENT");
			var companyTariff1 = Helper.NewLevel1CompanyTariffWithSingleRateLine(RatingConstants.RateCategory.AIR, "LSE", "USLAX", "AUBNE", "FRT", 100m);
			var companyTariff2 = Helper.NewNonLevel1CompanyTariff(2, RatingConstants.RateCategory.AIR, 10m);
			var rateEntry2 = companyTariff2.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, "LSE", "USLAX", "AUBNE", "FRT", 100m);
			var rateEntrys = companyTariff1.ChildRateEntries.ToList();
			rateEntrys.Add(rateEntry2);

			overseasAgent.CompanyData.RateTariffLevels.SetLevel("DEF", "ALL", "ALL", ZDate.Today, ZDate.Today.AddDays(1), 1);
			overseasAgent.CompanyData.RateTariffLevels.SetLevel("FRT", "ALL", "ALL", ZDate.Today.AddDays(2), ZDate.Today.AddDays(3), 2);

			Factory.Save();

			var mockRepository = new MockRepository(MockBehavior.Loose);
			var criteria = new TestRatingCriteria();
			var jobDatesMock = mockRepository.Create<IJobDatesProvider>();
			jobDatesMock.Setup(x => x.GetJobDateByType(It.IsAny<ZString>(), It.IsAny<string>())).Returns(ZDate.Today);
			criteria.JobDatesProvider = jobDatesMock.Object;
			criteria.OverseasAgent = overseasAgent;
			criteria.JobDatesProvider = jobDatesMock.Object;
			criteria.ConsumerType = JobInvoicingConsumerTypes.ForwardingConsol;
			criteria.FreightMode = FreightMode.AIR;
			criteria.Carrier = TransportProvider1;
			criteria.JobDirection = Directions.Import;
			var linesRepository = new RateLinesRepository(criteria, rateEntrys, new DummyLogger());

			AssertEquals(2, linesRepository.GetLines().Count);
			NotApplicableRateLineRemover.RemoveIrrelevantCompanyTariffRateLinesBasedOnLevelAndOutOfDateRange(criteria, linesRepository);
			AssertEquals(1, linesRepository.GetLines().Count);
			var level = linesRepository.GetLines().FirstOrDefault().ParentRateEntry.ParentRatingHeader.TH_GlobalRateLevel;
			AssertEquals(1, (int)level);
		}

		// Changing RemoveLinesWithInvalidCalculators not only to consider Highst Rate, adding more calculators
		// Then changing this test to add more calculator
		public void TestRemoveLinesWithInvalidHRCMissingUnit()
		{
			var org = Helper.NewOrgHeader();

			var rate = Helper.NewClientRate(null);
			rate.TH_OH = org.PK;
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, "AIR", "AUSYD", "USLAX", "STD", commodity: "GEN", removeLines: true);
			rateEntry.AddHighestRateCharge("OQUAR", 10m, "KG", 10m, "");
			var line2 = rateEntry.AddHighestRateCharge("OCART", 10m, "KG", 10m, "M3");
			line2.Calculator.AddRateLineItem(Calculator.Items.Operator.MIN, 0m, 10m, 0m);

			var criteria = new TestRatingCriteria();
			var testLogger = new TestLogger();
			var linesRepository = new RateLinesRepository(criteria, new List<IRateEntry> { rateEntry }, testLogger);
			RemoveLinesWithInvalidCalculators(linesRepository);
			var leftLines = linesRepository.GetLines();
			AssertEquals("There should be 1 rate line.", 1, leftLines.Count);
			linesRepository.Dispose(); // for closing internal logs and write items to the main log
			AssertCollectionContains(
				"Info:RateLine Filtered OQUAR-HRC-Client Rate TESTORG1\treason:\tHighest rate calculator missing required units.",
				testLogger.Infos
			);
		}

		#region RemoveInternationalFreightChargeCodeWithSkipFreightCharge

		public void TestRemove_SkipFreightChargeIsTrue_IsCosting_ShouldRemoveFreightCharge()
		{
			var org = Helper.NewOrgHeader();

			var localOtherFRTChargeCode = Factory.NewWithValidTestData<AccChargeCode>();

			localOtherFRTChargeCode.AC_Code = "OtherFRT";
			localOtherFRTChargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			localOtherFRTChargeCode.AC_ChargeGroup = "FRT";

			Helper.ChargeCodes.CreateGlobalCharge("OtherFRT", chargeGroup: "FRT");
			Helper.ChargeCodes.CreateGlobalCharge("DST", chargeGroup: "DST");

			Factory.Save();

			var rate = Helper.NewCosting(org);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.FCL, removeLines: true);
			var testLogger = new TestLogger();

			rateEntry.AddFlatRateLine("FRT", 1);
			rateEntry.AddFlatRateLine("DST", 1);

			rateEntry.AddFlatRateLine(localOtherFRTChargeCode.AC_Code, 1);

			var criteria = new TestRatingCriteria();
			criteria.SkipFreightCharge = true;
			criteria.AdapterType = AdapterType.Consolidation;

			var rateLinesRepository = new RateLinesRepository(criteria, new List<IRateEntry> { rateEntry }, testLogger);

			using (Env.Registry.RawRegistry.FreightChargeCode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, localOtherFRTChargeCode.PK.ToGuid())) {
				RemoveInternationalFreightChargeCodeWithSkipFreightCharge(criteria, rateLinesRepository);
			}

			rateLinesRepository.Dispose(); // for closing internal logs and write items to the main log

			AssertCollectionContains(
				"Info:RateLine Filtered OtherFRT-FLT-Costing TESTORG1\treason:\tConsolidation consumes an allocation with spot rate offered by the carrier to be manually input.",
				testLogger.Infos
			);

			AssertEquals(2, rateLinesRepository.GetLines().Count);
			AssertCollectionNotContains(rateLinesRepository.GetLines(), line => line.ChargeCode.PK == localOtherFRTChargeCode.PK);
		}

		#endregion

		#region Implementation

		TestHelper TestHelper => testHelper ?? (testHelper = new TestHelper(Factory));
		TestHelper testHelper;

		#endregion
	}
}
