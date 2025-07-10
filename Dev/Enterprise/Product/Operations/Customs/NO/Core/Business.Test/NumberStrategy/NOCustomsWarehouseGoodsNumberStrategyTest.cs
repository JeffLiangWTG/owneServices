using System;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.NumberFountain;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(NOCustomsWarehouseGoodsNumberStrategy))]
sealed class NOCustomsWarehouseGoodsNumberStrategyTest : TestCaseWithFactory
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		var exception = AssertExceptionThrown<ArgumentNullException>(() => { _ = new NOCustomsWarehouseGoodsNumberStrategy(null); });
		AssertEquals("fountainConnection", exception.ParamName);

		_ = new NOCustomsWarehouseGoodsNumberStrategy(Factory);
	});

	public void TestGetCustomsWarehouseGoodsNumber_ShouldCheckArrivalDate()
	{
		var arrivalDate = ZDateTime.Invalid;
		var exception = AssertExceptionThrown<ArgumentException>(() => Strategy.GetCustomsWarehouseGoodsNumber(arrivalDate, ValidGrantId));
		AssertEquals("arrivalDate", exception.ParamName);
	}

	public void TestGetCustomsWarehouseGoodsNumber_ShouldCheckGrantId()
	{
		const string grantId = "42";
		var exception = AssertExceptionThrown<ArgumentException>(() => Strategy.GetCustomsWarehouseGoodsNumber(ValidArrivalDate, grantId));
		AssertEquals("grantId", exception.ParamName);
	}

	[UseSnapshotProtection]
	public void TestGetCustomsWarehouseGoodsNumber_ShouldReturnSequencedGoodsNumber() => CombineAssertions(() =>
	{
		Env.NumberFountains.NOCustomsWarehouseGoodsNumber(ValidArrivalDate.ToDateTime(), ValidGrantId).SetNext(TestConnection, 1);
		AssertEquals("001", Strategy.GetCustomsWarehouseGoodsNumber(ValidArrivalDate, ValidGrantId));
		AssertEquals("002", Strategy.GetCustomsWarehouseGoodsNumber(ValidArrivalDate, ValidGrantId));
	});

	[UseSnapshotProtection]
	public void TestGetCustomsWarehouseGoodsNumber_ShouldNotOverflow() => CombineAssertions(() =>
	{
		Env.NumberFountains.NOCustomsWarehouseGoodsNumber(ValidArrivalDate.ToDateTime(), ValidGrantId).SetNext(TestConnection, 998);
		AssertEquals("After 999 iterations", "998", Strategy.GetCustomsWarehouseGoodsNumber(ValidArrivalDate, ValidGrantId));
		AssertEquals("After 999 iterations", "999", Strategy.GetCustomsWarehouseGoodsNumber(ValidArrivalDate, ValidGrantId));
		AssertExceptionThrown<NumberFountainMaximumValueReachedException>("After 1000 iterations", () => Strategy.GetCustomsWarehouseGoodsNumber(ValidArrivalDate, ValidGrantId));
	});

	NOCustomsWarehouseGoodsNumberStrategy Strategy => strategy ??= new(Factory);
	NOCustomsWarehouseGoodsNumberStrategy strategy;

	ZDateTime ValidArrivalDate { get; } = new (2024, 08, 23);
	const string ValidGrantId = "12345";
}
