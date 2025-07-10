using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

public class ZDateExtensionsTests : TestCase
{
	[TestDate(2024, 10, 1, 1, 30, 0)]
	public void TestToZDateTimeOffsetWithCurrentTimeIfDateIsToday_UtcDateIsToday()
	{
		var date = new ZDate(new DateTime(2024, 10, 1, 0, 0, 0), DateTimeKind.Utc);

		var dateTimeOffset = date.ToZDateTimeOffsetWithCurrentTimeIfDateIsToday();

		AssertEquals(new ZDateTimeOffset(2024, 10, 1, 1, 30, 0, TimeSpan.Zero), dateTimeOffset);
	}

	[TestDate(2024, 10, 1, 1, 30, 0)]
	public void TestToZDateTimeOffsetWithCurrentTimeIfDateIsToday_UtcDateIsYesterday()
	{
		var date = new ZDate(new DateTime(2024, 9, 30, 0, 0, 0), DateTimeKind.Utc);

		var dateTimeOffset = date.ToZDateTimeOffsetWithCurrentTimeIfDateIsToday();

		AssertEquals(new ZDateTimeOffset(2024, 9, 30, 0, 0, 0, TimeSpan.Zero), dateTimeOffset);
	}

	[TestDate(2024, 10, 1, 1, 30, 0)]
	public void TestToZDateTimeOffsetWithCurrentTimeIfDateIsToday_UtcDateIsTomorrow()
	{
		var date = new ZDate(new DateTime(2024, 10, 2, 0, 0, 0), DateTimeKind.Utc);

		var dateTimeOffset = date.ToZDateTimeOffsetWithCurrentTimeIfDateIsToday();

		AssertEquals(new ZDateTimeOffset(2024, 10, 2, 0, 0, 0, TimeSpan.Zero), dateTimeOffset);
	}

	[TestDate(2024, 10, 1, 1, 30, 0)]
	public void TestToZDateTimeOffsetWithCurrentTimeIfDateIsToday_LocalDateIsToday()
	{
		var date = new ZDate(new DateTime(2024, 10, 1, 0, 0, 0), DateTimeKind.Local);

		var dateTimeOffset = date.ToZDateTimeOffsetWithCurrentTimeIfDateIsToday();

		AssertEquals(new ZDateTimeOffset(2024, 10, 1, 1, 30, 0, TimeSpan.Zero), dateTimeOffset);
	}

	[TestDate(2024, 10, 1, 1, 30, 0)]
	public void TestToZDateTimeOffsetWithCurrentTimeIfDateIsToday_LocalDateIsYesterday()
	{
		var date = new ZDate(new DateTime(2024, 9, 30, 0, 0, 0), DateTimeKind.Local);

		var dateTimeOffset = date.ToZDateTimeOffsetWithCurrentTimeIfDateIsToday();

		AssertEquals(new ZDateTimeOffset(2024, 9, 30, 0, 0, 0, TimeSpan.Zero), dateTimeOffset);
	}

	[TestDate(2024, 10, 1, 1, 30, 0)]
	public void TestToZDateTimeOffsetWithCurrentTimeIfDateIsToday_LocalDateIsTomorrow()
	{
		var date = new ZDate(new DateTime(2024, 10, 2, 0, 0, 0), DateTimeKind.Local);

		var dateTimeOffset = date.ToZDateTimeOffsetWithCurrentTimeIfDateIsToday();

		AssertEquals(new ZDateTimeOffset(2024, 10, 2, 0, 0, 0, TimeSpan.Zero), dateTimeOffset);
	}
}
