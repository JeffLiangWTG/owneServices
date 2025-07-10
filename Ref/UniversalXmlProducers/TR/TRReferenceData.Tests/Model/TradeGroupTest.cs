using System.Collections.Generic;
using System;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests
{
	class TradeGroupTest
	{
		[TestCase("TG1", "Test Desc", "2024-01-01", "2024-12-31", true)]
		[TestCase("TGx", "Test Desc", "2024-01-01", "2024-12-31", false)]
		[TestCase("TG1", "Test Desx", "2024-01-01", "2024-12-31", false)]
		[TestCase("TG1", "Test Desc", "2024-01-02", "2024-12-31", false)]
		[TestCase("TG1", "Test Desc", "2024-01-01", "2024-12-30", false)]
		public void EqualsObjects(string code, string desc, DateTime startDate, DateTime endDate, bool expectedResult)
		{
			var tradeGroupCountry1 = new TradeGroupCountry { Code = "WTG1", Description = "Desc1", StartDate = DateTime.Now, EndDate = DateTime.Now.AddYears(1) };
			var tradeGroupCountry2 = new TradeGroupCountry { Code = "WTG2", Description = "Desc2", StartDate = DateTime.Now, EndDate = DateTime.Now.AddYears(1) };

			var tradeGroup1 = new TradeGroup
			{
				Code = "TG1",
				Description = "Test Desc",
				StartDate = new DateTime(2024, 1, 1),
				EndDate = new DateTime(2024, 12, 31),
				Countries = new List<TradeGroupCountry> { tradeGroupCountry1, tradeGroupCountry2 },
				Excluded = false
			};

			var tradeGroup2 = new TradeGroup
			{
				Code = code,
				Description = desc,
				StartDate = startDate,
				EndDate = endDate,
				Countries = new List<TradeGroupCountry> { tradeGroupCountry1, tradeGroupCountry2 },
				Excluded = false
			};

			Assert.AreEqual(expectedResult, tradeGroup1.Equals(tradeGroup2));
			Assert.AreEqual(expectedResult, tradeGroup1.GetHashCode() == tradeGroup2.GetHashCode());
		}

		[Test]
		public void EqualsObjectsCountries()
		{
			var tradeGroupCountry1 = new TradeGroupCountry { Code = "WTG1", Description = "Desc1", StartDate = DateTime.Now, EndDate = DateTime.Now.AddYears(1) };
			var tradeGroupCountry2 = new TradeGroupCountry { Code = "WTG2", Description = "Desc2", StartDate = DateTime.Now, EndDate = DateTime.Now.AddYears(1) };
			var tradeGroupCountry3 = new TradeGroupCountry { Code = "WTG3", Description = "Desc3", StartDate = DateTime.Now, EndDate = DateTime.Now.AddYears(1) };

			var tradeGroup1 = new TradeGroup
			{
				Code = "TG1",
				Description = "Test Desc",
				StartDate = new DateTime(2024, 1, 1),
				EndDate = new DateTime(2024, 12, 31),
				Countries = new List<TradeGroupCountry> { tradeGroupCountry1, tradeGroupCountry2 },
				Excluded = false
			};

			var tradeGroup2 = new TradeGroup
			{
				Code = "TG1",
				Description = "Test Desc",
				StartDate = new DateTime(2024, 1, 1),
				EndDate = new DateTime(2024, 12, 31),
				Countries = new List<TradeGroupCountry> { tradeGroupCountry1, tradeGroupCountry2 },
				Excluded = false
			};

			var tradeGroup3 = new TradeGroup
			{
				Code = "TG1",
				Description = "Test Desc",
				StartDate = new DateTime(2024, 1, 1),
				EndDate = new DateTime(2024, 12, 31),
				Countries = new List<TradeGroupCountry> { tradeGroupCountry1, tradeGroupCountry2, tradeGroupCountry3 },
				Excluded = false
			};

			Assert.AreEqual(true, tradeGroup1.Equals(tradeGroup2));
			Assert.AreEqual(true, tradeGroup2.Equals(tradeGroup1));
			Assert.AreEqual(false, tradeGroup1.Equals(tradeGroup3));
			Assert.AreEqual(false, tradeGroup3.Equals(tradeGroup1));

			Assert.AreEqual(tradeGroup1.GetHashCode(), tradeGroup2.GetHashCode());
			Assert.AreNotEqual(tradeGroup1.GetHashCode(), tradeGroup3.GetHashCode());
		}
	}
}
