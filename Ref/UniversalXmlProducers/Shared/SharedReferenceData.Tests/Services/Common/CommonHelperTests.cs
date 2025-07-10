using System;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Common.Tests
{
	[TestFixture]
	class CommonHelperTests
	{
		[Test]
		public void CleanCommodityCode()
		{
			var code = "0100000010";
			Assert.That(CommonHelper.CleanCommodityCode(code), Is.EqualTo("0100000010"));

			code = "010000001";
			Assert.That(CommonHelper.CleanCommodityCode(code), Is.EqualTo("010000001"));

			code = "01000000";
			Assert.That(CommonHelper.CleanCommodityCode(code), Is.EqualTo("01"));
		}

		[Test]
		public void CleanHtmlTags()
		{
			var subScriptData = "Subscript X<sub>0</sub><sub>1</sub><sub>2</sub><sub>3</sub><sub>4</sub><sub>5</sub><sub>6</sub><sub>7</sub><sub>8</sub><sub>9</sub>";
			var supScriptData = "Superscript X<sup>0</sup><sup>1</sup><sup>2</sup><sup>3</sup><sup>4</sup><sup>5</sup><sup>6</sup><sup>7</sup><sup>8</sup><sup>9</sup><sup>o</sup>";
			var otherData = "BR-P X<br>Y<p/>Z";

			Assert.That(CommonHelper.CleanHtmlTags(subScriptData), Is.EqualTo("Subscript X\u2080\u2081\u2082\u2083\u2084\u2085\u2086\u2087\u2088\u2089"));
			Assert.That(CommonHelper.CleanHtmlTags(supScriptData), Is.EqualTo("Superscript X\u2070\u00B9\u00B2\u00B3\u2074\u2075\u2076\u2077\u2078\u2079\u00B0"));
			Assert.That(CommonHelper.CleanHtmlTags(otherData), Is.EqualTo("BR-P X\r\nY\r\nZ"));
			Assert.That(CommonHelper.CleanHtmlTags(otherData, false), Is.EqualTo("BR-P X; Y; Z"));
		}

		[Test]
		public void CalcMinDate()
		{
			DateTime? dt = null;

			Assert.That(CommonHelper.CalcMinDate(dt), Is.EqualTo(CommonHelper.DefaultValues.MinimumDateTime));
			dt = new DateTime();
			Assert.That(CommonHelper.CalcMinDate(dt), Is.EqualTo(CommonHelper.DefaultValues.MinimumDateTime));
			dt = DateTime.MinValue;
			Assert.That(CommonHelper.CalcMinDate(dt), Is.EqualTo(CommonHelper.DefaultValues.MinimumDateTime));
			dt = CommonHelper.DefaultValues.MinimumDateTime.AddDays(-1);
			Assert.That(CommonHelper.CalcMinDate(dt), Is.EqualTo(CommonHelper.DefaultValues.MinimumDateTime));
			dt = CommonHelper.DefaultValues.MinimumDateTime.AddDays(1);
			Assert.That(CommonHelper.CalcMinDate(dt), Is.EqualTo(CommonHelper.DefaultValues.MinimumDateTime.AddDays(1)));

			dt = new DateTime(2022, 2, 14, 15, 16, 17);
			Assert.That(CommonHelper.CalcMinDate(dt), Is.EqualTo(new DateTime(2022, 2, 14, 15, 16, 0)));
		}

		[Test]
		public void CalcMaxDate()
		{
			DateTime? dt = null;

			Assert.That(CommonHelper.CalcMaxDate(dt), Is.EqualTo(CommonHelper.DefaultValues.MaximumDateTime));
			dt = DateTime.MaxValue;
			Assert.That(CommonHelper.CalcMaxDate(dt), Is.EqualTo(CommonHelper.DefaultValues.MaximumDateTime));
			dt = CommonHelper.DefaultValues.MaximumDateTime.AddDays(1);
			Assert.That(CommonHelper.CalcMaxDate(dt), Is.EqualTo(CommonHelper.DefaultValues.MaximumDateTime));
			dt = CommonHelper.DefaultValues.MaximumDateTime.AddDays(-1);
			Assert.That(CommonHelper.CalcMaxDate(dt), Is.EqualTo(CommonHelper.DefaultValues.MaximumDateTime.AddDays(-1)));

			dt = new DateTime(2022, 2, 14, 0, 0, 0);
			Assert.That(CommonHelper.CalcMaxDate(dt), Is.EqualTo(new DateTime(2022, 2, 13, 23, 59, 0)));

			dt = new DateTime(2022, 1, 1, 0, 0, 0);
			Assert.That(CommonHelper.CalcMaxDate(dt), Is.EqualTo(new DateTime(2021, 12, 31, 23, 59, 0)));

			dt = DateTime.MinValue;
			Assert.That(CommonHelper.CalcMaxDate(dt), Is.EqualTo(DateTime.MinValue));

			dt = CommonHelper.DefaultValues.MinimumDateTime;
			Assert.That(CommonHelper.CalcMaxDate(dt), Is.EqualTo(CommonHelper.DefaultValues.MinimumDateTime));

			dt = new DateTime(2022, 2, 14, 23, 59, 59);
			Assert.That(CommonHelper.CalcMaxDate(dt), Is.EqualTo(new DateTime(2022, 2, 14, 23, 59, 0)));
		}

		[Test]
		public void DropSeconds()
		{
			var dt = new DateTime(2022, 2, 14, 23, 59, 59);
			Assert.That(CommonHelper.DropSeconds(dt), Is.EqualTo(new DateTime(2022, 2, 14, 23, 59, 0)));
		}
	}
}
