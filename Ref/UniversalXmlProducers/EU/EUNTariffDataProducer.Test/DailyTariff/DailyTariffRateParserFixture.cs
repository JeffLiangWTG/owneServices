using System;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test.DailyTariff
{
	[TestFixture]
	sealed class DailyTariffRateParserFixture
	{
		[Test]
		public void Parse_SimpleUpdateInsert()
		{
			var dailyTariffParser = new DailyTariffRateParser();
			dailyTariffParser.Parse(DailyTariffTestFiles.SampleDailyMeasuresDailyPath);

			var parsedRecords = dailyTariffParser.GetParsedRecords();

			Assert.That(parsedRecords.Count(), Is.EqualTo(5));

			AssertStartEndDate(
				parsedRecords.ElementAt(0),
				new DateTime(2017, 01, 01),
				new DateTime(2023, 06, 30, 23, 59, 00));

			AssertStartEndDate(
				parsedRecords.ElementAt(1),
				new DateTime(2023, 07, 01),
				new DateTime(2079, 06, 06, 23, 59, 00));

			AssertStartEndDate(
				parsedRecords.ElementAt(2),
				new DateTime(2023, 07, 01),
				new DateTime(2023, 06, 30, 23, 59, 00));

			AssertStartEndDate(
				parsedRecords.ElementAt(3),
				new DateTime(2024, 11, 20),
				new DateTime(2079, 06, 06, 23, 59, 00));

			AssertStartEndDate(
				parsedRecords.ElementAt(4),
				new DateTime(2024, 11, 18),
				new DateTime(2024, 11, 19, 23, 59, 00));
		}

		[Test]
		public void Parse_MultipleUpdateOneInsert()
		{
			var dailyTariffParser = new DailyTariffRateParser();
			dailyTariffParser.Parse(DailyTariffTestFiles.MeasuresTwoUpdates);

			var parsedRecords = dailyTariffParser.GetParsedRecords();

			Assert.That(parsedRecords.Count(), Is.EqualTo(1));
			Assert.That(parsedRecords.ElementAt(0).Rate, Is.EqualTo("7.000 %"));
		}

		[Test]
		public void Parse_MultipleUpdateBeforeInsert()
		{
			var dailyTariffParser = new DailyTariffRateParser();
			dailyTariffParser.Parse(DailyTariffTestFiles.MeasuresTwoUpdatesBeforeInsert);

			var parsedRecords = dailyTariffParser.GetParsedRecords();
			Assert.That(parsedRecords.Count(), Is.EqualTo(2));

			AssertStartEndDate(
				parsedRecords.ElementAt(0),
				new DateTime(2024, 06, 06),
				new DateTime(2079, 06, 06, 23, 59, 00));

			AssertStartEndDate(
				parsedRecords.ElementAt(1),
				new DateTime(2024, 01, 25),
				new DateTime(2024, 06, 05, 23, 59, 00));
		}

		[Test]
		public void Parse_DuplicatedInsert()
		{
			var dailyTariffParser = new DailyTariffRateParser();
			Assert.Throws<ArgumentException>(
				() => dailyTariffParser.Parse(DailyTariffTestFiles.MeasuresTwoDuplicatedInsert),
				"Cannot insert a duplicate record with RecordType INSERT.");
		}

		[Test]
		public void Parse_DeleteInsertTariff()
		{
			var dailyTariffParser = new DailyTariffRateParser();
			dailyTariffParser.Parse(DailyTariffTestFiles.MeasuresWithDeletionsPath);

			var parsedRecords = dailyTariffParser.GetParsedRecords();
			Assert.That(parsedRecords.Count(), Is.EqualTo(2));

			var firstRate = parsedRecords.ElementAt(0);
			Assert.That(firstRate.TariffHeader, Is.EqualTo("7222202100"));
			Assert.That(firstRate.Rate, Is.EqualTo("2.000%"));

			var secondRate = parsedRecords.ElementAt(1);
			Assert.That(secondRate.TariffHeader, Is.EqualTo("7222202900"));
			Assert.That(secondRate.Rate, Is.EqualTo("3.400%"));

		}

		void AssertStartEndDate(IRawRateRecord rawRecord, DateTime startDate, DateTime endDate)
		{
			Assert.That(rawRecord.StartDate, Is.EqualTo(startDate));
			Assert.That(rawRecord.EndDate, Is.EqualTo(endDate));
		}
	}
}
