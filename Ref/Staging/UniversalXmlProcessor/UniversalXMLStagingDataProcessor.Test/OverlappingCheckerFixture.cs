using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using CargoWise.RefDbRepo.DataProcessingExplanation;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	[TestFixture]
	class OverlappingCheckerFixture
	{
		[Test]
		public void IsValid()
		{
			var tariffPK1 = Guid.NewGuid();
			var tariffPK2 = Guid.NewGuid();
			var stagingPK = Guid.NewGuid();
			var tariffs = new[] { new RefCusTariff { ZZ1_PK = tariffPK1 }, new RefCusTariff { ZZ1_PK = tariffPK2 } };
			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.Setup(x => x.GetWrapperValue("ZZ1_PK")).Returns(stagingPK);
			var wrapperEffectiveDateRange = new DateTimeRange(new DateTime(2000, 01, 01), new DateTime(2010, 01, 01));
			var historicalDateTimeRanges = new[]
				{
					Tuple.Create( new DateTimeRange(new DateTime(1990, 01, 01), new DateTime(2000, 01, 01)), tariffPK1),
					Tuple.Create( new DateTimeRange(new DateTime(2010, 01, 01), new DateTime(2020, 01, 01)), tariffPK2)
				};
			var overlappingChecker = new OverlappingChecker(wrapper.Object, overlappingCalculator, tariffs, wrapperEffectiveDateRange, historicalDateTimeRanges);
			Assert.True(overlappingChecker.IsValid());
		}

		[Test]
		public void IsValid_RateApplicability()
		{
			var ratePK1 = Guid.NewGuid();
			var ratePK2 = Guid.NewGuid();
			var appPK1 = Guid.NewGuid();
			var appPK2 = Guid.NewGuid();
			var rate1 = new RefCusRate { ZZ2_PK = ratePK1 };
			var rate2 = new RefCusRate { ZZ2_PK = ratePK2 };
			var app1 = new RefCusApplicability { ZZT_PK = appPK1 };
			var app2 = new RefCusApplicability { ZZT_PK = appPK2 };
			var rateApp1 = new RefCusRateApplicability(rate1, app1, safeRepository);
			var rateApp2 = new RefCusRateApplicability(rate2, app2, safeRepository);
			var stagingPK1 = Guid.NewGuid();
			var stagingPK2 = Guid.NewGuid();

			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.Setup(x => x.GetWrapperValue("ZZ2_PK")).Returns(stagingPK1);
			wrapper.Setup(x => x.GetWrapperValue("ZZT_PK")).Returns(stagingPK2);
			var wrapperEffectiveDateRange = new DateTimeRange(new DateTime(2000, 01, 01), new DateTime(2010, 01, 01));
			var historicalDateTimeRanges = new[]
				{
					Tuple.Create( new DateTimeRange(new DateTime(1990, 01, 01), new DateTime(2000, 01, 01)), rateApp1.S01_PK),
					Tuple.Create( new DateTimeRange(new DateTime(2010, 01, 01), new DateTime(2020, 01, 01)), rateApp2.S01_PK)
				};
			var overlappingChecker = new OverlappingChecker(wrapper.Object, overlappingCalculator, new[] { rateApp1, rateApp2 }, wrapperEffectiveDateRange, historicalDateTimeRanges);
			Assert.True(overlappingChecker.IsValid());
		}

		[Test]
		public void IsValid_ConditionApplicability()
		{
			var condPK1 = Guid.NewGuid();
			var condPK2 = Guid.NewGuid();
			var appPK1 = Guid.NewGuid();
			var appPK2 = Guid.NewGuid();
			var cond1 = new RefCusCondition { ZX1_PK = condPK1 };
			var cond2 = new RefCusCondition { ZX1_PK = condPK2 };
			var app1 = new RefCusApplicability { ZZT_PK = appPK1 };
			var app2 = new RefCusApplicability { ZZT_PK = appPK2 };
			var condApp1 = new RefCusConditionApplicability(cond1, app1, safeRepository);
			var condApp2 = new RefCusConditionApplicability(cond2, app2, safeRepository);
			var stagingPK1 = Guid.NewGuid();
			var stagingPK2 = Guid.NewGuid();

			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.Setup(x => x.GetWrapperValue("ZX1_PK")).Returns(stagingPK1);
			wrapper.Setup(x => x.GetWrapperValue("ZZT_PK")).Returns(stagingPK2);
			var wrapperEffectiveDateRange = new DateTimeRange(new DateTime(2000, 01, 01), new DateTime(2010, 01, 01));
			var historicalDateTimeRanges = new[]
				{
					Tuple.Create( new DateTimeRange(new DateTime(1990, 01, 01), new DateTime(2000, 01, 01)), condApp1.S07_PK),
					Tuple.Create( new DateTimeRange(new DateTime(2010, 01, 01), new DateTime(2020, 01, 01)), condApp2.S07_PK),
				};
			var overlappingChecker = new OverlappingChecker(wrapper.Object, overlappingCalculator, new[] { condApp1, condApp2 }, wrapperEffectiveDateRange, historicalDateTimeRanges);
			Assert.True(overlappingChecker.IsValid());
		}

		[Test]
		public void IsValid_HistoricalOverlapping()
		{
			var tariffPK1 = Guid.NewGuid();
			var tariffPK2 = Guid.NewGuid();
			var tariffs = new[] { new RefCusTariff { ZZ1_PK = tariffPK1 }, new RefCusTariff { ZZ1_PK = tariffPK2 } };
			var stagingPK = Guid.NewGuid();
			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.Setup(x => x.GetWrapperValue("ZZ1_PK")).Returns(stagingPK);
			var wrapperEffectiveDateRange = new DateTimeRange(new DateTime(2000, 01, 01), new DateTime(2010, 01, 01));
			var historicalDateTimeRanges = new[]
				{
					Tuple.Create( new DateTimeRange(new DateTime(1990, 01, 01), new DateTime(2011, 01, 01)), tariffPK1),
					Tuple.Create( new DateTimeRange(new DateTime(2010, 01, 01), new DateTime(2020, 01, 01)), tariffPK2)
				};
			var overlappingChecker = new OverlappingChecker(wrapper.Object, overlappingCalculator, tariffs, wrapperEffectiveDateRange, historicalDateTimeRanges);
			var expectedMessage = $@"Overlapping of existing data in SafeDB is identified.
RefCusTariff has overlapping of Effective DateRange;
Existing DateRanges in SafeDB:
[1990/01/01 00:00:00~2011/01/01 00:00:00, {tariffPK1}]
[2010/01/01 00:00:00~2020/01/01 00:00:00, {tariffPK2}]
ErrorCode: {ErrorCodes.OverlappingDateRange}";
			Assert.Throws(Is.TypeOf<RefDataProcessingException>().And.Message.StartsWith(expectedMessage), () => overlappingChecker.IsValid());
		}

		[Test]
		public void IsValid_NewDataOverlapping()
		{
			var tariffPK1 = Guid.NewGuid();
			var tariffPK2 = Guid.NewGuid();
			var tariffs = new[] { new RefCusTariff { ZZ1_PK = tariffPK1 }, new RefCusTariff { ZZ1_PK = tariffPK2 } };
			var stagingPK = Guid.NewGuid();
			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.Setup(x => x.GetWrapperValue<Guid>("ZZ1_PK")).Returns(stagingPK);
			wrapper.Setup(x => x.GetStagingTypeName()).Returns(nameof(RefCusTariff));
			var wrapperEffectiveDateRange = new DateTimeRange(new DateTime(1999, 01, 01), new DateTime(2010, 01, 01));
			var historicalDateTimeRanges = new[]
				{
					Tuple.Create( new DateTimeRange(new DateTime(1990, 01, 01), new DateTime(2000, 01, 01)), tariffPK1),
					Tuple.Create( new DateTimeRange(new DateTime(2010, 01, 01), new DateTime(2020, 01, 01)), tariffPK2)
				};
			var overlappingChecker = new OverlappingChecker(wrapper.Object, overlappingCalculator, tariffs, wrapperEffectiveDateRange, historicalDateTimeRanges);
			var expectedMessage = $@"Overlapping is caused by new data from XML.
RefCusTariff has overlapping of Effective DateRange;
New data in StageDB:
[{wrapperEffectiveDateRange.StartDate.GetFormatString()}~{wrapperEffectiveDateRange.EndDate.GetFormatString()}, {stagingPK}]
Existing DateRanges in SafeDB:
[1990/01/01 00:00:00~2000/01/01 00:00:00, {tariffPK1}]
[2010/01/01 00:00:00~2020/01/01 00:00:00, {tariffPK2}]
ErrorCode: {ErrorCodes.OverlappingDateRange}";
			Assert.Throws(Is.TypeOf<RefDataProcessingException>().And.Message.StartsWith(expectedMessage), () => overlappingChecker.IsValid());
		}

		[Test]
		public void IsValid_HistoricalOverlapping_RateApplicability()
		{
			var ratePK1 = Guid.NewGuid();
			var ratePK2 = Guid.NewGuid();
			var appPK1 = Guid.NewGuid();
			var appPK2 = Guid.NewGuid();
			var rate1 = new RefCusRate { ZZ2_PK = ratePK1, ZZ2_StartDate = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero), ZZ2_EndDate = new DateTimeOffset(2011, 1, 1, 0, 0, 0, TimeSpan.Zero) };
			var rate2 = new RefCusRate { ZZ2_PK = ratePK2, ZZ2_StartDate = new DateTimeOffset(2010, 1, 1, 0, 0, 0, TimeSpan.Zero), ZZ2_EndDate = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero) };
			var app1 = new RefCusApplicability { ZZT_PK = appPK1, ZZT_StartDate = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero), ZZT_EndDate = new DateTimeOffset(2011, 1, 1, 0, 0, 0, TimeSpan.Zero) };
			var app2 = new RefCusApplicability { ZZT_PK = appPK2, ZZT_StartDate = new DateTimeOffset(2010, 1, 1, 0, 0, 0, TimeSpan.Zero), ZZT_EndDate = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero) };
			var rateApp1 = new RefCusRateApplicability(rate1, app1, safeRepository);
			var rateApp2 = new RefCusRateApplicability(rate2, app2, safeRepository);
			var stagingPK1 = Guid.NewGuid();
			var stagingPK2 = Guid.NewGuid();

			var wrapper = new Mock<IStagingDataWrapper>();
			var wrapperEffectiveDateRange = new DateTimeRange(new DateTime(2000, 01, 01), new DateTime(2010, 01, 01));
			var historicalDateTimeRanges = new[]
				{
					Tuple.Create( new DateTimeRange(new DateTime(1990, 01, 01), new DateTime(2011, 01, 01)), rateApp1.S01_PK),
					Tuple.Create( new DateTimeRange(new DateTime(2010, 01, 01), new DateTime(2020, 01, 01)), rateApp2.S01_PK),
				};
			var expectedMessage = $@"Overlapping of existing data in SafeDB is identified.
RefCusApplicability has overlapping of Effective DateRange;
Existing DateRanges in SafeDB:
[1990/01/01 00:00:00~2011/01/01 00:00:00, {appPK1}]
[2010/01/01 00:00:00~2020/01/01 00:00:00, {appPK2}]
ErrorCode: {ErrorCodes.OverlappingDateRange}";

			var overlappingChecker = new OverlappingChecker(wrapper.Object, overlappingCalculator, new[] { rateApp1, rateApp2 }, wrapperEffectiveDateRange, historicalDateTimeRanges);
			Assert.Throws(Is.TypeOf<RefDataProcessingException>().And.Message.StartsWith(expectedMessage), () => overlappingChecker.IsValid());
		}

		[Test]
		public void IsValid_NewDataOverlapping_RateApplicability()
		{
			var ratePK1 = Guid.NewGuid();
			var ratePK2 = Guid.NewGuid();
			var appPK1 = Guid.NewGuid();
			var appPK2 = Guid.NewGuid();
			var rate1 = new RefCusRate { ZZ2_PK = ratePK1, ZZ2_StartDate = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero), ZZ2_EndDate = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero) };
			var rate2 = new RefCusRate { ZZ2_PK = ratePK2, ZZ2_StartDate = new DateTimeOffset(2010, 1, 1, 0, 0, 0, TimeSpan.Zero), ZZ2_EndDate = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero) };
			var app1 = new RefCusApplicability { ZZT_PK = appPK1, ZZT_StartDate = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero), ZZT_EndDate = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero) };
			var app2 = new RefCusApplicability { ZZT_PK = appPK2, ZZT_StartDate = new DateTimeOffset(2010, 1, 1, 0, 0, 0, TimeSpan.Zero), ZZT_EndDate = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero) };
			var rateApp1 = new RefCusRateApplicability(rate1, app1, safeRepository);
			var rateApp2 = new RefCusRateApplicability(rate2, app2, safeRepository);
			var rateApp3 = new RefCusRateApplicability(safeRepository);
			var stagingPK = Guid.NewGuid();

			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.Setup(x => x.GetWrapperValue("ZZ2_PK")).Returns(null);
			wrapper.Setup(x => x.GetWrapperValue("ZZT_PK")).Returns(null);
			wrapper.Setup(x => x.GetStagingTypeName()).Returns(nameof(RefCusRateApplicability));
			wrapper.Setup(x => x.GetWrapperOriginalPK()).Returns(stagingPK);
			var wrapperEffectiveDateRange = new DateTimeRange(new DateTime(1999, 01, 01), new DateTime(2010, 01, 01));
			var historicalDateTimeRanges = new[]
				{
					Tuple.Create( new DateTimeRange(new DateTime(1990, 01, 01), new DateTime(2000, 01, 01)), rateApp1.S01_PK),
					Tuple.Create( new DateTimeRange(new DateTime(2010, 01, 01), new DateTime(2020, 01, 01)), rateApp2.S01_PK),
					Tuple.Create( new DateTimeRange(new DateTime(2020, 01, 01), new DateTime(2021, 01, 01)), rateApp3.S01_PK)
				};

			var expectedMessage = $@"Overlapping is caused by new data from XML.
RefCusApplicability has overlapping of Effective DateRange;
New data in StageDB:
[{wrapperEffectiveDateRange.StartDate.GetFormatString()}~{wrapperEffectiveDateRange.EndDate.GetFormatString()}, {stagingPK}]
Data processed but not yet committed to SafeDB:
[2020/01/01 00:00:00~2021/01/01 00:00:00]
Existing DateRanges in SafeDB:
[1990/01/01 00:00:00~2000/01/01 00:00:00, {appPK1}]
[2010/01/01 00:00:00~2020/01/01 00:00:00, {appPK2}]
ErrorCode: {ErrorCodes.OverlappingDateRange}";

			var overlappingChecker = new OverlappingChecker(wrapper.Object, overlappingCalculator, new[] { rateApp1, rateApp2, rateApp3 }, wrapperEffectiveDateRange, historicalDateTimeRanges);
			Assert.Throws(Is.TypeOf<RefDataProcessingException>().And.Message.StartsWith(expectedMessage), () => overlappingChecker.IsValid());
		}

		[Test]
		public void IsValid_HistoricalOverlapping_CondApplicability()
		{
			var condPK1 = Guid.NewGuid();
			var condPK2 = Guid.NewGuid();
			var appPK1 = Guid.NewGuid();
			var appPK2 = Guid.NewGuid();
			var cond1 = new RefCusCondition { ZX1_PK = condPK1, ZX1_StartDate = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero), ZX1_EndDate = new DateTimeOffset(2011, 1, 1, 0, 0, 0, TimeSpan.Zero) };
			var cond2 = new RefCusCondition { ZX1_PK = condPK2, ZX1_StartDate = new DateTimeOffset(2010, 1, 1, 0, 0, 0, TimeSpan.Zero), ZX1_EndDate = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero) };
			var app1 = new RefCusApplicability { ZZT_PK = appPK1, ZZT_StartDate = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero), ZZT_EndDate = new DateTimeOffset(2011, 1, 1, 0, 0, 0, TimeSpan.Zero) };
			var app2 = new RefCusApplicability { ZZT_PK = appPK2, ZZT_StartDate = new DateTimeOffset(2010, 1, 1, 0, 0, 0, TimeSpan.Zero), ZZT_EndDate = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero) };
			var condApp1 = new RefCusConditionApplicability(cond1, app1, safeRepository);
			var condApp2 = new RefCusConditionApplicability(cond2, app2, safeRepository);
			var stagingPK1 = Guid.NewGuid();
			var stagingPK2 = Guid.NewGuid();

			var wrapper = new Mock<IStagingDataWrapper>();
			var wrapperEffectiveDateRange = new DateTimeRange(new DateTime(2000, 01, 01), new DateTime(2010, 01, 01));
			var historicalDateTimeRanges = new[]
				{
					Tuple.Create( new DateTimeRange(new DateTime(1990, 01, 01), new DateTime(2011, 01, 01)), condApp1.S07_PK),
					Tuple.Create( new DateTimeRange(new DateTime(2010, 01, 01), new DateTime(2020, 01, 01)), condApp2.S07_PK),
				};
			var expectedMessage = $@"Overlapping of existing data in SafeDB is identified.
RefCusApplicability has overlapping of Effective DateRange;
Existing DateRanges in SafeDB:
[1990/01/01 00:00:00~2011/01/01 00:00:00, {appPK1}]
[2010/01/01 00:00:00~2020/01/01 00:00:00, {appPK2}]
ErrorCode: {ErrorCodes.OverlappingDateRange}";

			var overlappingChecker = new OverlappingChecker(wrapper.Object, overlappingCalculator, new[] { condApp1, condApp2 }, wrapperEffectiveDateRange, historicalDateTimeRanges);
			Assert.Throws(Is.TypeOf<RefDataProcessingException>().And.Message.StartsWith(expectedMessage), () => overlappingChecker.IsValid());
		}

		[Test]
		public void IsValid_NewDataOverlapping_CondApplicability()
		{
			var condPK1 = Guid.NewGuid();
			var condPK2 = Guid.NewGuid();
			var appPK1 = Guid.NewGuid();
			var appPK2 = Guid.NewGuid();
			var cond1 = new RefCusCondition { ZX1_PK = condPK1, ZX1_StartDate = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero), ZX1_EndDate = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero) };
			var cond2 = new RefCusCondition { ZX1_PK = condPK2, ZX1_StartDate = new DateTimeOffset(2010, 1, 1, 0, 0, 0, TimeSpan.Zero), ZX1_EndDate = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero) };
			var app1 = new RefCusApplicability { ZZT_PK = appPK1, ZZT_StartDate = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero), ZZT_EndDate = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero) };
			var app2 = new RefCusApplicability { ZZT_PK = appPK2, ZZT_StartDate = new DateTimeOffset(2010, 1, 1, 0, 0, 0, TimeSpan.Zero), ZZT_EndDate = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero) };
			var condApp1 = new RefCusConditionApplicability(cond1, app1, safeRepository);
			var condApp2 = new RefCusConditionApplicability(cond2, app2, safeRepository);
			var condApp3 = new RefCusConditionApplicability(safeRepository);
			var stagingPK = Guid.NewGuid();

			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.Setup(x => x.GetWrapperValue<Guid>("ZX1_PK")).Returns(null);
			wrapper.Setup(x => x.GetWrapperValue<Guid>("ZZT_PK")).Returns(null);
			wrapper.Setup(x => x.GetStagingTypeName()).Returns(nameof(RefCusConditionApplicability));
			wrapper.Setup(x => x.GetWrapperOriginalPK()).Returns(stagingPK);
			var wrapperEffectiveDateRange = new DateTimeRange(new DateTime(1999, 01, 01), new DateTime(2010, 01, 01));
			var historicalDateTimeRanges = new[]
				{
					Tuple.Create( new DateTimeRange(new DateTime(1990, 01, 01), new DateTime(2000, 01, 01)), condApp1.S07_PK),
					Tuple.Create( new DateTimeRange(new DateTime(2010, 01, 01), new DateTime(2020, 01, 01)), condApp2.S07_PK),
					Tuple.Create( new DateTimeRange(new DateTime(2020, 01, 01), new DateTime(2021, 01, 01)), condApp3.S07_PK)
				};

			var expectedMessage = $@"Overlapping is caused by new data from XML.
RefCusApplicability has overlapping of Effective DateRange;
New data in StageDB:
[{wrapperEffectiveDateRange.StartDate.GetFormatString()}~{wrapperEffectiveDateRange.EndDate.GetFormatString()}, {stagingPK}]
Data processed but not yet committed to SafeDB:
[2020/01/01 00:00:00~2021/01/01 00:00:00]
Existing DateRanges in SafeDB:
[1990/01/01 00:00:00~2000/01/01 00:00:00, {appPK1}]
[2010/01/01 00:00:00~2020/01/01 00:00:00, {appPK2}]
ErrorCode: {ErrorCodes.OverlappingDateRange}";

			var overlappingChecker = new OverlappingChecker(wrapper.Object, overlappingCalculator, new[] { condApp1, condApp2, condApp3 }, wrapperEffectiveDateRange, historicalDateTimeRanges);
			Assert.Throws(Is.TypeOf<RefDataProcessingException>().And.Message.StartsWith(expectedMessage), () => overlappingChecker.IsValid());
		}

		IOverlappingCalculator overlappingCalculator;
		ISafeRepository safeRepository;
		ISafeDataProvider safeDataProvider;
		[SetUp]
		public void SetUp()
		{
			overlappingCalculator = new OverlappingCalculator(false);
			safeRepository = new Mock<ISafeRepository>().Object;
			var mockSafeDataProvider = new Mock<ISafeDataProvider>();
			mockSafeDataProvider.Setup(x => x.GetDateTimeRange(It.IsAny<object>())).Returns<object>((object x) => new DateTimeRange(GetDateValue(x, "StartDate"), GetDateValue(x, "EndDate")));
			safeDataProvider = mockSafeDataProvider.Object;
		}
		static DateTimeOffset GetDateValue<T>(T safeObj, string suffix)
		{
			var dateProperty = safeObj.GetEntityType().GetProperties().FirstOrDefault(x => x.Name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
			return (DateTimeOffset)safeObj.GetValue(dateProperty.Name);
		}
	}
}
