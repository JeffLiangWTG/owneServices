using System;
using System.Linq;
using CargoWise.RefDbRepo.PLReferenceData.Business.Quota;
using CargoWise.RefDbRepo.PLReferenceData.Services.Interfaces;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Quota;

[TestFixture]
sealed class PLQuotaExtractorTest
{
	[Test]
	public void TestGeneratePLQuotaData_AddToResult() => Assert.Multiple(() =>
	{
		var quotaResponseHistory = new findQuotaDefinitionByDatesResponseHistory();
		var testResponse = new IsztarHistoryResponse
		{
			IsztarHistoryItem = [new IsztarHistoryResponseIsztarHistoryItem { Item = quotaResponseHistory }]
		};

		var testQuotaDefinition = new quotaDefinition
		{
			validityEndDateSpecified = true,
			validityEndDate = new DateTime(2022, 02, 01),
			quotaOrderNumber = new(),
			measurementUnit = new(),
			quotaBalanceEvent = [
				CreateBalanceEvent(new DateTime(2021, 01, 01), 1.2222m),
			]
		};

		var dateTimeProviderMock = Mock.Of<IDateTimeProvider>(x => x.CurrentLocalDateTime == new DateTime(2021, 01, 01));

		var result = PLQuotaExtractor.GeneratePLQuotaData(testResponse, dateTimeProviderMock);
		Assert.AreEqual(0, result.Count(), "Quota should not be added if QuotaDefinition is not defined.");

		quotaResponseHistory.QuotaDefinition = [];
		result = PLQuotaExtractor.GeneratePLQuotaData(testResponse, dateTimeProviderMock);
		Assert.AreEqual(0, result.Count(), "Quota should not be added if QuotaDefinition is empty.");

		quotaResponseHistory.QuotaDefinition = [testQuotaDefinition];
		result = PLQuotaExtractor.GeneratePLQuotaData(testResponse, dateTimeProviderMock);
		Assert.AreEqual(0, result.Count(), "Quota without OrderNumber and UnitOfMeasure should not be added.");

		testQuotaDefinition.quotaOrderNumber.quotaOrderNumberId = "111";
		result = PLQuotaExtractor.GeneratePLQuotaData(testResponse, dateTimeProviderMock);
		Assert.AreEqual(0, result.Count(), "Quota without UnitOfMeasure should not be added.");

		testQuotaDefinition.quotaOrderNumber.quotaOrderNumberId = string.Empty;
		testQuotaDefinition.measurementUnit.measurementUnitCode = "unit1";
		result = PLQuotaExtractor.GeneratePLQuotaData(testResponse, dateTimeProviderMock);
		Assert.AreEqual(0, result.Count(), "Quota without OrderNumber should not be added.");

		testQuotaDefinition.quotaOrderNumber.quotaOrderNumberId = "111";
		result = PLQuotaExtractor.GeneratePLQuotaData(testResponse, dateTimeProviderMock);
		Assert.AreEqual(1, result.Count(), "Quota with OrderNumber and UnitOfMeasure should be added.");
	});

	[Test]
	public void TestGeneratePLQuotaData_ExcludeExpired() => Assert.Multiple(() =>
	{
		var testQuotaDefinition = new quotaDefinition
		{
			validityEndDateSpecified = true,
			quotaOrderNumber = new() { quotaOrderNumberId = "id1" },
			measurementUnit = new() { measurementUnitCode = "unit1" },
			quotaBalanceEvent = [CreateBalanceEvent(new DateTime(2021, 01, 01), 1.2222m)]
		};

		var testResponse = new IsztarHistoryResponse
		{
			IsztarHistoryItem = [new() { Item = new findQuotaDefinitionByDatesResponseHistory { QuotaDefinition = [testQuotaDefinition] } }]
		};

		var ExpirationYearBeforeLatestVatChangeDate = Business.Constants.LatestVatChangeDate.AddMonths(2);
		var dateTimeProviderMock = Mock.Of<IDateTimeProvider>(x => x.CurrentLocalDateTime == ExpirationYearBeforeLatestVatChangeDate);

		testQuotaDefinition.validityEndDate = dateTimeProviderMock.CurrentLocalDateTime.AddYears(-1 - Constants.ExpiredYears);
		var result = PLQuotaExtractor.GeneratePLQuotaData(testResponse, dateTimeProviderMock);
		Assert.AreEqual(0, result.Count(), "Should be excluded if before ExpiredYears and before LatestVatChangeDate.");

		testQuotaDefinition.validityEndDate = Business.Constants.LatestVatChangeDate.AddMonths(-1);
		result = PLQuotaExtractor.GeneratePLQuotaData(testResponse, dateTimeProviderMock);
		Assert.AreEqual(0, result.Count(), "Should be excluded if after ExpiredYears but before LatestVatChangeDate.");

		testQuotaDefinition.validityEndDate = Business.Constants.LatestVatChangeDate;
		result = PLQuotaExtractor.GeneratePLQuotaData(testResponse, dateTimeProviderMock);
		Assert.AreEqual(1, result.Count(), "Should be added if after ExpiredYears and equals to LatestVatChangeDate.");

		testQuotaDefinition.validityEndDate = Business.Constants.LatestVatChangeDate.AddMonths(1);
		result = PLQuotaExtractor.GeneratePLQuotaData(testResponse, dateTimeProviderMock);
		Assert.AreEqual(1, result.Count(), "Should be added if after ExpiredYears and after LatestVatChangeDate.");

		var LatestVatChangeDateBeforeExpirationYear = Business.Constants.LatestVatChangeDate.AddYears(1 + Constants.ExpiredYears);
		dateTimeProviderMock = Mock.Of<IDateTimeProvider>(x => x.CurrentLocalDateTime == LatestVatChangeDateBeforeExpirationYear);

		testQuotaDefinition.validityEndDate = Business.Constants.LatestVatChangeDate.AddMonths(-1);
		result = PLQuotaExtractor.GeneratePLQuotaData(testResponse, dateTimeProviderMock);
		Assert.AreEqual(0, result.Count(), "Should be excluded if before LatestVatChangeDate and before ExpiredYears.");

		testQuotaDefinition.validityEndDate = Business.Constants.LatestVatChangeDate;
		result = PLQuotaExtractor.GeneratePLQuotaData(testResponse, dateTimeProviderMock);
		Assert.AreEqual(0, result.Count(), "Should be excluded if equals to LatestVatChangeDate but before ExpiredYears.");

		testQuotaDefinition.validityEndDate = Business.Constants.LatestVatChangeDate.AddMonths(1);
		result = PLQuotaExtractor.GeneratePLQuotaData(testResponse, dateTimeProviderMock);
		Assert.AreEqual(0, result.Count(), "Should be excluded if after LatestVatChangeDate but before ExpiredYears.");

		testQuotaDefinition.validityEndDate = dateTimeProviderMock.CurrentLocalDateTime.AddYears(-1);
		result = PLQuotaExtractor.GeneratePLQuotaData(testResponse, dateTimeProviderMock);
		Assert.AreEqual(1, result.Count(), "Should be added if after LatestVatChangeDate and after ExpiredYears.");
	});

	[Test]
	public void TestGeneratePLQuotaData_Sort()
	{
		var testResponse = new IsztarHistoryResponse
		{
			IsztarHistoryItem = [
				new IsztarHistoryResponseIsztarHistoryItem
				{
					Item = new findQuotaDefinitionByDatesResponseHistory
					{
						QuotaDefinition = [
							CreateQuotaDefinition("789"),
							CreateQuotaDefinition("123"),
							CreateQuotaDefinition("456"),
						]
					}
				}
			]
		};

		var dateTimeProviderMock = Mock.Of<IDateTimeProvider>(x => x.CurrentLocalDateTime == new DateTime(2021, 01, 01));

		var result = PLQuotaExtractor.GeneratePLQuotaData(testResponse, dateTimeProviderMock);

		Assert.Multiple(() =>
		{
			Assert.AreEqual(3, result.Count(), "all quotas are added");
			Assert.AreEqual("123", result.First().ZXQ_OrderNumber, "First item should be correct.");
			Assert.AreEqual("789", result.Last().ZXQ_OrderNumber, "Last item should be correct.");
		});

		static quotaDefinition CreateQuotaDefinition(string id) => new()
		{
			validityEndDateSpecified = true,
			validityEndDate = new DateTime(2022, 02, 01),
			quotaOrderNumber = new() { quotaOrderNumberId = id },
			measurementUnit = new() { measurementUnitCode = "unit" },
			quotaBalanceEvent = [CreateBalanceEvent(new DateTime(2021, 01, 01), 1m)]
		};
	}

	static quotaBalanceEvent CreateBalanceEvent(DateTime occurrence, decimal balance) => new()
	{
		occurrenceTimestampSpecified = true,
		occurrenceTimestamp = occurrence,
		newBalance = balance,
	};
}
