using System;
using CargoWise.RefDbRepo.ITReferenceData.Business;
using HtmlAgilityPack;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test
{
	[TestFixture]
	class ExtensionsFixture
	{
		[Test]
		public void ToDateTime()
		{
			Assert.AreEqual(new DateTime(2022, 01, 01), "01/01/2022".ToDateTime());
			Assert.AreEqual(null, "01/01/".ToDateTime());
		}

		[Test]
		public void HasCheckedAttribute()
		{
			Assert.Multiple(() =>
			{
				Assert.AreEqual(false, (null as HtmlNode).HasCheckedAttribute(), "When HtmlNode is null");

				var htmlNode = HtmlNode.CreateNode("<input type=\"checkbox\" />");
				Assert.AreEqual(false, htmlNode.HasCheckedAttribute(), "When HtmlNode does not have the 'checked' attribute");

				htmlNode = HtmlNode.CreateNode("<input type=\"checkbox\" checked />");
				Assert.AreEqual(true, htmlNode.HasCheckedAttribute(), "When HtmlNode has the 'checked' attribute");
			});
		}

		[Test]
		public void HasYesValueAttribute()
		{
			Assert.Multiple(() =>
			{
				Assert.AreEqual(false, (null as HtmlNode).HasYesValueAttribute(), "When HtmlNode is null");

				var htmlNode = HtmlNode.CreateNode("<input type=\"text\" />");
				Assert.AreEqual(false, htmlNode.HasYesValueAttribute(), "When HtmlNode does not have the 'value' attribute");

				htmlNode = HtmlNode.CreateNode("<input type=\"text\" value=\"XYZ\"/>");
				Assert.AreEqual(false, htmlNode.HasYesValueAttribute(), "When HtmlNode does not have the 'value' attribute");

				htmlNode = HtmlNode.CreateNode("<input type=\"text\" value=\"SI\"/>");
				Assert.AreEqual(true, htmlNode.HasYesValueAttribute(), "When HtmlNode has the 'value' attribute");

				htmlNode = HtmlNode.CreateNode("<input type=\"text\" value=\"CONDIZIONE\"/>");
				Assert.AreEqual(true, htmlNode.HasYesValueAttribute(), "When HtmlNode has the 'value' attribute");
			});
		}

		[SetCulture("de-DE")]
		[TestCase("2012-1-20", ExpectedResult = "20/01/2012")]
		public string ToItalianShortDateString(DateTime dateTime)
		{
			return dateTime.ToItalianShortDateString();
		}

		[TestCase(null, ExpectedResult = "1900-01-01")]
		[TestCase("1899-12-31", ExpectedResult = "1900-01-01")]
		[TestCase("2022-01-01", ExpectedResult = "2022-01-01")]
		public DateTime GetDateOrFallbackToMinSmallDateTime(DateTime? dateTime) => dateTime.GetDateOrFallbackToMinSmallDateTime();

		[TestCase(null, ExpectedResult = "2079-06-06 23:59:00")]
		[TestCase("2079-12-31", ExpectedResult = "2079-06-06 23:59:00")]
		[TestCase("2022-01-01", ExpectedResult = "2022-01-01")]
		public DateTime GetDateOrFallbackToMaxSmallDateTime(DateTime? dateTime) => dateTime.GetDateOrFallbackToMaxSmallDateTime();
	}
}
