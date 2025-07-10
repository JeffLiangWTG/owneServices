using System;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Model
{
	class TradeGroupCountryTest
	{
		[TestCase("WTG", "WTG Desc", "2024-01-01", "2024-12-31", true)]
		[TestCase("WTG1", "WTG Desc", "2024-01-01", "2024-12-31", false)]
		[TestCase("WTG", "WTG Desc1", "2024-01-01", "2024-12-31", false)]
		[TestCase("WTG", "WTG Desc", "2024-01-02", "2024-12-31", false)]
		[TestCase("WTG", "WTG Desc", "2024-01-01", "2024-12-30", false)]
		[TestCase(null, "WTG Desc", "2024-01-01", "2024-12-31", false)]
		[TestCase("WTG", null, "2024-01-01", "2024-12-31", false)]
		[TestCase("WTG", "WTG Desc", null, "2024-12-31", false)]
		[TestCase("WTG", "WTG Desc", "2024-01-01", null, false)]
		public void EqualsObjects(string code, string description, DateTime startDate, DateTime endDate, bool expectedResult)
		{
			var tgc1 = new TradeGroupCountry
			{
				Code = "WTG",
				Description = "WTG Desc",
				StartDate = new DateTime(2024, 1, 1),
				EndDate = new DateTime(2024, 12, 31)
			};

			var tgc2 = new TradeGroupCountry
			{
				Code = code,
				Description = description,
				StartDate = startDate,
				EndDate = endDate
			};

			Assert.AreEqual(expectedResult, tgc1.Equals(tgc2));
			Assert.AreEqual(expectedResult, tgc1.GetHashCode() == tgc2.GetHashCode());
		}
	}
}
