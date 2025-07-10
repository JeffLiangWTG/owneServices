using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.INReferenceData.Business;
using CargoWise.RefDbRepo.INReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.INReferenceData.Tests
{
	[TestFixture]
	public class ExchangeRateXmlProducerTest
	{

		[Test]
		public void TestProduceXml_Success()
		{
			using (var program = new ExchangeRateXmlProducerForTest("IN Exchange Rate"))
			{
				var failDateStorage = program.FailDateStorage;
				var errors = program.ProduceXml();
				Assert.Null(failDateStorage.Load<List<DateTime>>());
				Assert.IsEmpty(errors);
			}
		}

		[Test]
		public void TestProduceXml_Failure()
		{
			using (var program = new ExchangeRateXmlProducerForTest("IN Exchange Rate", false))
			{
				var failDateStorage = program.FailDateStorage;

				var today = new DateTimeProvider().GetIndiaToday();
				var errors = program.ProduceXml();
				var failDates = failDateStorage.Load<List<DateTime>>();
				Assert.AreEqual(1, failDates.Count);
				Assert.AreEqual(today, failDates[0]);
				Assert.AreEqual(1, errors.Count);
			}
		}

		[Test]
		public void TestProduceXml_HandlePreviousFailure()
		{
			using (var program = new ExchangeRateXmlProducerForTest("IN Exchange Rate"))
			{
				var failDateStorage = program.FailDateStorage;
				var today = new DateTime(2024, 1, 1);
				failDateStorage.Save(new List<DateTime> { today });
				var errors = program.ProduceXml();
				Assert.Null(failDateStorage.Load<List<DateTime>>());
				Assert.IsEmpty(errors);
			}
		}

		[Test]
		public void TestExportToXml()
		{
			using (var producer = new ExchangeRateXmlProducerForTest("IN Exchange Rate"))
			{
				var currencyDate = new DateTime(2024, 9, 20);

				var input = TestHelper.ReadContentString("ExchangeRate\\INTestFiles\\Input\\ICEGATE_Api_igexratesubscribe_Response.json");
				var exchangeRates = ExchangeRateParser.ParseResponse(input);
				producer.ExportToXml(currencyDate, exchangeRates);

				var cueFilePath = producer.GetOutputFilePath(currencyDate, Constants.ExchangeRate.Types.CustomsExport);
				Assert.True(File.Exists(cueFilePath));
				var expectedCueFileContent = TestHelper.ReadContentString("ExchangeRate\\INTestFiles\\Output\\RefExchangeRateZZ_IN_CUE_06092024.xml");
				Assert.AreEqual(expectedCueFileContent, TestHelper.RemoveIgnoredArgsFromXml(File.ReadAllText(cueFilePath)));

				var cusFilePath = producer.GetOutputFilePath(currencyDate, Constants.ExchangeRate.Types.Customs);
				Assert.True(File.Exists(cusFilePath));
				var expectedCusFileContent = TestHelper.ReadContentString("ExchangeRate\\INTestFiles\\Output\\RefExchangeRateZZ_IN_CUS_06092024.xml");
				Assert.AreEqual(expectedCusFileContent, TestHelper.RemoveIgnoredArgsFromXml(File.ReadAllText(cusFilePath)));
			}
		}

		class ExchangeRateXmlProducerForTest : ExchangeRateXmlProducer, IDisposable
		{
			bool RunSuccess { get; }

			public ExchangeRateXmlProducerForTest(string dataSource, bool runSuccess = true) : base(dataSource)
			{
				RunSuccess = runSuccess;
			}

			public new string GetOutputFilePath(DateTime currencyDate, string rateType)
			{
				return base.GetOutputFilePath(currencyDate, rateType);
			}

			public void Dispose()
			{
				DeleteOutputFiles();
				FailDateStorage.ClearData();
			}

			protected override void ProduceXmlForOneDay(DateTime currencyDate)
			{
				if (!RunSuccess)
				{
					throw new UnhandledApplicationException("Test Exception");
				}
			}

			public new LocalFileStorage FailDateStorage => base.FailDateStorage;

			public new void ExportToXml(DateTime currencyDate, List<RefExchangeRateZZ> exchangeRates)
			{
				base.ExportToXml(currencyDate, exchangeRates);
			}
		}
	}
}
