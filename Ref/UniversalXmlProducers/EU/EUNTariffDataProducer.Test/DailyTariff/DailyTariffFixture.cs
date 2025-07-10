using System;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test.DailyTariff
{
	[TestFixture]
	class DailyTariffFixture
	{
		[Test]
		public void CanParseExcelFile()
		{
			var dailyTariffRateParser = new DailyTariffRateParser();
			dailyTariffRateParser.Parse(DailyTariffTestFiles.SampleMeasuresPath);
			var results = dailyTariffRateParser.GetParsedRecords().ToList();

			Assert.IsNotNull(results);

			var firstRecord = results[0];
			Assert.AreEqual(firstRecord.TariffHeader, "702000000");
			Assert.AreEqual(firstRecord.AdditionalCode, "");
			Assert.AreEqual(firstRecord.OrderNumber, string.Empty);
			Assert.AreEqual(firstRecord.StartDate, new DateTime(2021, 07, 02));
			Assert.AreEqual(firstRecord.EndDate, new DateTime(2021, 07, 02, 23, 59, 00));
			Assert.AreEqual(firstRecord.TradeGroup, "1011");
			Assert.AreEqual(firstRecord.MeasureTypeId, "490");
			Assert.AreEqual(firstRecord.Description2, string.Empty);
			Assert.AreEqual(firstRecord.LegalBase, "REGULATION R0892/17");
			Assert.AreEqual(firstRecord.RateCode, "A00");
		}

		[Test]
		public void AvoidsDuplicationBetweenInsertedAndUpdatedRecords()
		{
			var dailyTariffRateParser = new DailyTariffRateParser();
			dailyTariffRateParser.Parse(DailyTariffTestFiles.DuplicatedMeasuresPath);
			var results = dailyTariffRateParser.GetParsedRecords().ToList();
			Assert.IsNotNull(results);

			var tariffRecords = results.Where(x => x.TariffHeader == "7222202100");

			foreach (var tariff in tariffRecords)
			{
				//check their uniqueness
				Assert.That(tariffRecords.Count(x => x.Equals(tariff)), Is.EqualTo(1));
			}
		}

		[Test]
		public void ConsiderRecordsMarkedAsDeletedAndRemoveThem()
		{
			var dailyTariffRateParser = new DailyTariffRateParser();
			dailyTariffRateParser.Parse(DailyTariffTestFiles.MeasuresWithDeletionsPath);
			var results = dailyTariffRateParser.GetParsedRecords().ToList();

			Assert.IsNotNull(results);

			var tariffRecords7222202100 = results.Where(x => x.TariffHeader == "7222202100");
			var tariffRecords7222202900 = results.Where(x => x.TariffHeader == "7222202900");

			Assert.That(tariffRecords7222202100.Count(), Is.EqualTo(1));
			Assert.That(tariffRecords7222202900.Count(), Is.EqualTo(1));
		}

		[Test]
		public void ConsiderRecordsWithStartDateInTheFuture()
		{
			var dailyTariffRateParser = new DailyTariffRateParser();
			dailyTariffRateParser.Parse(DailyTariffTestFiles.MeasuresWithFutureStartDatePath);
			var results = dailyTariffRateParser.GetParsedRecords().ToList();

			Assert.IsNotNull(results);

			var tariffRecords7222202100 = results.Where(x => x.TariffHeader == "7222202100");
			var tariffRecords7222202900 = results.Where(x => x.TariffHeader == "7222202900");

			Assert.That(tariffRecords7222202100.Count(), Is.EqualTo(1));
			Assert.That(tariffRecords7222202900.Count(), Is.EqualTo(1));
		}

		[Test]
		public void ConsiderLatterUpdateRecord()
		{
			var dailyTariffRateParser = new DailyTariffRateParser();
			dailyTariffRateParser.Parse(DailyTariffTestFiles.MeasuresTwoUpdates);
			var results = dailyTariffRateParser.GetParsedRecords().ToList();

			Assert.IsNotNull(results);

			var tariffRecords7222202100 = results.Where(x => x.TariffHeader == "7222202100");

			Assert.That(tariffRecords7222202100.Count(), Is.EqualTo(1));
			var record = tariffRecords7222202100.First();
			Assert.That(record.Rate.Trim() == "7.000 %");
		}

		[Test]
		public void IgnoresRecordsWithEmptyGoodsCode()
		{
			var dailyTariffRateParser = new DailyTariffRateParser();
			dailyTariffRateParser.Parse(DailyTariffTestFiles.TariffDataWithMissingCodePath);
			var results = dailyTariffRateParser.GetParsedRecords().ToList();

			Assert.IsNotNull(results);
			Assert.That(results.Count, Is.EqualTo(1));

			var tariffRecords7222202900 = results.Where(x => x.TariffHeader == "7222202900");
			Assert.That(tariffRecords7222202900.Count(), Is.EqualTo(1));

			var tariffRecordsBlankCode = results.Where(x => string.IsNullOrEmpty(x.TariffHeader));
			Assert.That(tariffRecordsBlankCode.Count(), Is.EqualTo(0));
		}

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			ApplicationConfig.ConfigEnvironment();
		}
	}
}
