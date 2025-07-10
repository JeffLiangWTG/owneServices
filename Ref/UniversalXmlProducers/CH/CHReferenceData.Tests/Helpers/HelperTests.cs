using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.RefDbRepo.CHReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests
{
	[TestFixture]
	class HelperTests
	{
		[Test]
		public void EndOfDayWithTime()
		{
			Assert.That(new DateTime(2021, 6, 23, 23, 59, 00), Is.EqualTo(new DateTime(2021, 6, 23, 10, 28, 45).EndOfDay()) );
		}

		[Test]
		public void MaximumDate()
		{
			Assert.That(Helper.MaximumDateTime, Is.EqualTo(new DateTime(2079, 6, 6, 0, 0, 0)));
		}

		[Test]
		public void MinimumDate()
		{
			Assert.That(Helper.MinimumDateTime, Is.EqualTo(new DateTime(1900, 1, 1, 0, 0, 0)));
		}

		[Test]
		public void TestTruncateOverMaximum()
		{
			Assert.That(Helper.MaximumDateTime, Is.EqualTo(new DateTime(2080, 1, 1, 11, 45, 32).Truncate()));
		}

		[Test]
		public void TestTruncateBelowMinimum()
		{
			Assert.That(Helper.MinimumDateTime, Is.EqualTo(new DateTime(1899, 2, 23, 10, 47, 34).Truncate()));
		}

		[Test]
		public void TestTruncateText()
		{
			Assert.That("AAAAA", Is.EqualTo("AAAAAAAAAA".Truncate(5)));
		}

		[Test]
		public void TestPadLeftMissingZero()
		{
			Assert.That("0.27", Is.EqualTo("0.27".PadLeftMissingZero()));
			Assert.That("0.27", Is.EqualTo(".27".PadLeftMissingZero()));
		}

		[Test]
		public void TestMinDate()
		{
			Assert.That(DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture), Is.EqualTo(Helper.Min(DateTime.Parse("2021-01-01", CultureInfo.InvariantCulture), DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture))));
			Assert.That(DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture), Is.EqualTo(Helper.Min(DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture), DateTime.Parse("2021-01-01", CultureInfo.InvariantCulture))));
		}

		[Test]
		public void TestMaxDate()
		{
			Assert.That(DateTime.Parse("2021-01-01", CultureInfo.InvariantCulture), Is.EqualTo(Helper.Max(DateTime.Parse("2021-01-01", CultureInfo.InvariantCulture), DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture))));
			Assert.That(DateTime.Parse("2021-01-01", CultureInfo.InvariantCulture), Is.EqualTo(Helper.Max(DateTime.Parse("2020-01-01", CultureInfo.InvariantCulture), DateTime.Parse("2021-01-01", CultureInfo.InvariantCulture))));
		}

		[Test]
		public void TestSetDynamicListItem()
		{
			var list = new List<int>();
			list.SetDynamicListItem(3, 33);
			CollectionAssert.AreEqual(new[] { 0, 0, 0, 33 }, list);
			list.SetDynamicListItem(5, 55);
			CollectionAssert.AreEqual(new[] { 0, 0, 0, 33, 0, 55 }, list);
			list.SetDynamicListItem(2, 22);
			CollectionAssert.AreEqual(new[] { 0, 0, 22, 33, 0, 55 }, list);
		}
	}
}
