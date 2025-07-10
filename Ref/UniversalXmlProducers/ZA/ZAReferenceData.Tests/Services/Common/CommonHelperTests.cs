using System;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Common
{
	[TestFixture]
	public class CommonHelperTests
	{
		[Test]
		public void CalcMinDate()
		{
			DateTime? dt = null;

			Assert.That(CommonHelper.CalcMinDate(dt), Is.EqualTo(CommonHelper.MinimumDateTime));
			dt = new DateTime();
			Assert.That(CommonHelper.CalcMinDate(dt), Is.EqualTo(CommonHelper.MinimumDateTime));
			dt = DateTime.MinValue;
			Assert.That(CommonHelper.CalcMinDate(dt), Is.EqualTo(CommonHelper.MinimumDateTime));
			dt = CommonHelper.MinimumDateTime.AddDays(-1);
			Assert.That(CommonHelper.CalcMinDate(dt), Is.EqualTo(CommonHelper.MinimumDateTime));
			dt = CommonHelper.MinimumDateTime.AddDays(1);
			Assert.That(CommonHelper.CalcMinDate(dt), Is.EqualTo(CommonHelper.MinimumDateTime.AddDays(1)));

			dt = new DateTime(2022, 2, 14, 15, 16, 17);
			Assert.That(CommonHelper.CalcMinDate(dt), Is.EqualTo(new DateTime(2022, 2, 14, 15, 16, 0)));
		}

		[Test]
		public void CalcMaxDate()
		{
			DateTime? dt = null;

			Assert.That(CommonHelper.CalcMaxDate(dt), Is.EqualTo(CommonHelper.MaximumDateTime));
			dt = new DateTime();
			Assert.That(CommonHelper.CalcMaxDate(dt), Is.EqualTo(CommonHelper.MaximumDateTime));
			dt = DateTime.MaxValue;
			Assert.That(CommonHelper.CalcMaxDate(dt), Is.EqualTo(CommonHelper.MaximumDateTime));
			dt = CommonHelper.MaximumDateTime.AddDays(1);
			Assert.That(CommonHelper.CalcMaxDate(dt), Is.EqualTo(CommonHelper.MaximumDateTime));
			dt = CommonHelper.MaximumDateTime.AddDays(-1);
			Assert.That(CommonHelper.CalcMaxDate(dt), Is.EqualTo(CommonHelper.MaximumDateTime.AddDays(-1)));

			dt = new DateTime(2022, 2, 14, 23, 59, 59);
			Assert.That(CommonHelper.CalcMaxDate(dt), Is.EqualTo(new DateTime(2022, 2, 14, 23, 59, 0)));

			dt = new DateTime(2022, 2, 14, 0, 0, 0);
			Assert.That(CommonHelper.CalcMaxDate(dt), Is.EqualTo(new DateTime(2022, 2, 14, 23, 59, 0)));
		}

		[Test]
		public void DropSeconds()
		{
			var dt = new DateTime(2022, 2, 14, 23, 59, 59);
			Assert.That(CommonHelper.DropSeconds(dt), Is.EqualTo(new DateTime(2022, 2, 14, 23, 59, 0)));
		}

		[Test]
		public void BackInAMinute()
		{
			var dt = new DateTime(2022, 2, 14, 23, 59, 59);
			Assert.That(CommonHelper.BackInAMinute(dt), Is.EqualTo(new DateTime(2022, 2, 14, 23, 59, 59)));

			dt = new DateTime(2022, 2, 14, 23, 59, 0);
			Assert.That(CommonHelper.BackInAMinute(dt), Is.EqualTo(new DateTime(2022, 2, 14, 23, 59, 0)));

			dt = new DateTime(2022, 2, 14, 23, 0, 0);
			Assert.That(CommonHelper.BackInAMinute(dt), Is.EqualTo(new DateTime(2022, 2, 14, 23, 0, 0)));

			dt = new DateTime(2022, 2, 14, 0, 0, 0);
			Assert.That(CommonHelper.BackInAMinute(dt), Is.EqualTo(new DateTime(2022, 2, 13, 23, 59, 0)));
		}

		[Test]
		public void GetMinDate()
		{
			var dt1 = new DateTime(2025, 1, 1, 0, 0, 0);
			var dt2 = new DateTime(2025, 1, 1, 0, 0, 1);
			Assert.That(CommonHelper.GetMinDate(dt1, dt2), Is.EqualTo(dt1));
		}

		[Test]
		public void GetMaxDate()
		{
			var dt1 = new DateTime(2025, 1, 1, 0, 0, 0);
			var dt2 = new DateTime(2025, 1, 1, 0, 0, 1);
			Assert.That(CommonHelper.GetMaxDate(dt1, dt2), Is.EqualTo(dt2));
		}
	}
}
