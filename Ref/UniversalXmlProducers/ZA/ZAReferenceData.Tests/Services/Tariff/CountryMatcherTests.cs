using System;
using System.Linq;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Loader;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff
{
	[TestFixture]
	public class CountryMatcherTests
	{
		[Test]
		[Explicit("Developer Integration test")]
		public void FetchCountryCodes()
		{
			var matcher = new CountryMatcher();

			try
			{
				var results = matcher.GetCountryCodes(new string[] { "EGYPT", "ARGENTINA" }).ToList();

				Console.WriteLine("Country Codes:");
				results.ForEach(x => Console.WriteLine(x));
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Failed to get country codes: {ex.GetBaseException().Message}");
			}
		}
	}
}
