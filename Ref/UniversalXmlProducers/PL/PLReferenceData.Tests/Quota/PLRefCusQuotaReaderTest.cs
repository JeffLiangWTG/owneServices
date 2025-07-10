using System;
using CargoWise.RefDbRepo.PLReferenceData.Business.Quota;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Quota;

[TestFixture]
sealed class PLRefCusQuotaReaderTest
{
	[Test]
	public void TestToRefCusQuota_OrderNumber() => Assert.Multiple(() =>
	{
		var data = new quotaDefinition();
		var result = data.ToRefCusQuota();
		Assert.IsNull(result.ZXQ_OrderNumber, "OrderNumber is not defined.");

		data.quotaOrderNumber = new();
		result = data.ToRefCusQuota();
		Assert.IsNull(result.ZXQ_OrderNumber, "OrderNumberId is not defined.");

		data.quotaOrderNumber.quotaOrderNumberId = string.Empty;
		result = data.ToRefCusQuota();
		Assert.AreEqual(string.Empty, result.ZXQ_OrderNumber, "OrderNumberId is empty.");

		data.quotaOrderNumber.quotaOrderNumberId = "111";
		result = data.ToRefCusQuota();
		Assert.AreEqual("111", result.ZXQ_OrderNumber, "OrderNumberId is not empty.");
	});

	[Test]
	public void TestToRefCusQuota_InitialAmount() => Assert.Multiple(() =>
	{
		var data = new quotaDefinition();
		var result = data.ToRefCusQuota();
		Assert.AreEqual(0.0m, result.ZXQ_InitialAmount, "Default InitialVolume.");

		data.initialVolume = 123.456m;
		result = data.ToRefCusQuota();
		Assert.AreEqual(123.456m, result.ZXQ_InitialAmount, "InitialVolume is not default.");
	});

	[Test]
	public void TestToRefCusQuota_StartDate() => Assert.Multiple(() =>
	{
		var data = new quotaDefinition();
		var result = data.ToRefCusQuota();
		Assert.AreEqual(DateTime.MinValue, result.ZXQ_StartDate, "ValidityStartDate is not defined.");

		data.validityStartDate = new DateTime(2020, 02, 02);
		result = data.ToRefCusQuota();
		Assert.AreEqual(new DateTime(2020, 02, 02), result.ZXQ_StartDate, "ValidityStartDate is defined.");
	});

	[Test]
	public void TestToRefCusQuota_EndDate() => Assert.Multiple(() =>
	{
		var smallDateTimeMaxValue = new DateTime(2079, 06, 06, 23, 59, 00);
		var data = new quotaDefinition();
		var result = data.ToRefCusQuota();
		Assert.AreEqual(smallDateTimeMaxValue, result.ZXQ_EndDate, "validityEndDateSpecified and validityEndDate are not defined.");

		data.validityEndDateSpecified = true;
		result = data.ToRefCusQuota();
		Assert.AreEqual(DateTime.MinValue, result.ZXQ_EndDate, "ValidityEndDate is not defined.");

		data.validityEndDate = new DateTime(2020, 02, 02);
		result = data.ToRefCusQuota();
		Assert.AreEqual(new DateTime(2020, 02, 02), result.ZXQ_EndDate, "ValidityEndDate is defined.");

		data.validityEndDateSpecified = false;
		result = data.ToRefCusQuota();
		Assert.AreEqual(smallDateTimeMaxValue, result.ZXQ_EndDate, "validityEndDateSpecified is 'false'.");
	});

	[Test]
	public void TestToRefCusQuota_Balance() => Assert.Multiple(() =>
	{
		var data = new quotaDefinition();
		var result = data.ToRefCusQuota();
		Assert.AreEqual(0.0m, result.ZXQ_Balance, "BalanceEvents is not defined.");

		data.quotaBalanceEvent = [];
		result = data.ToRefCusQuota();
		Assert.AreEqual(0.0m, result.ZXQ_Balance, "BalanceEvents is empty.");

		data.quotaBalanceEvent = [CreateBalanceEvent(new DateTime(2020, 01, 01), 1.1111m)];
		result = data.ToRefCusQuota();
		Assert.AreEqual(1.1111m, result.ZXQ_Balance, "BalanceEvents contains single item.");

		data.quotaBalanceEvent = [
			CreateBalanceEvent(new DateTime(2020, 01, 01), 1.1111m),
			CreateBalanceEvent(new DateTime(2020, 01, 02), 1.2222m)
		];
		result = data.ToRefCusQuota();
		Assert.AreEqual(1.2222m, result.ZXQ_Balance, "BalanceEvents contains multiple sorted items.");

		data.quotaBalanceEvent = [
			CreateBalanceEvent(new DateTime(2020, 01, 01), 1.1111m),
			CreateBalanceEvent(new DateTime(2020, 03, 01), 1.3333m),
			CreateBalanceEvent(new DateTime(2020, 02, 01), 1.2222m)
		];
		result = data.ToRefCusQuota();
		Assert.AreEqual(1.3333m, result.ZXQ_Balance, "BalanceEvents contains multiple unsorted items.");
	});

	[Test]
	public void TestToRefCusQuota_UnitOfMeasure() => Assert.Multiple(() =>
	{
		var data = new quotaDefinition();
		var result = data.ToRefCusQuota();
		Assert.IsNull(result.ZXQ_UnitOfMeasure, "UnitCode and UnitQualifierCode are not defined.");

		data.measurementUnit = new measurementUnit();
		result = data.ToRefCusQuota();
		Assert.IsNull(result.ZXQ_UnitOfMeasure, "UnitCode is empty; UnitQualifierCode is not defined.");

		data.measurementUnit.measurementUnitCode = "unit1";
		result = data.ToRefCusQuota();
		Assert.AreEqual("unit1", result.ZXQ_UnitOfMeasure, "UnitCode is not empty; UnitQualifierCode is not defined.");

		data.measurementUnitQualifier = new measurementUnitQualifier();
		result = data.ToRefCusQuota();
		Assert.AreEqual("unit1", result.ZXQ_UnitOfMeasure, "UnitCode is not empty; UnitQualifierCode is empty.");

		data.measurementUnitQualifier.measurementUnitQualifierCode = "A";
		result = data.ToRefCusQuota();
		Assert.AreEqual("unit1A", result.ZXQ_UnitOfMeasure, "UnitCode and UnitQualifierCode are not empty.");

		data.measurementUnit = null;
		result = data.ToRefCusQuota();
		Assert.IsNull(result.ZXQ_UnitOfMeasure, "UnitCode is not defined; UnitQualifierCode is not empty.");
	});

	static quotaBalanceEvent CreateBalanceEvent(DateTime occurrence, decimal balance) => new quotaBalanceEvent()
	{
		occurrenceTimestampSpecified = true,
		occurrenceTimestamp = occurrence,
		newBalance = balance,
	};
}
