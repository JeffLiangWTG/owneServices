using System;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Interfaces;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Loader;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Common;
using Moq;
using NUnit.Framework;
using static CargoWise.RefDbRepo.ZAReferenceData.Services.Common.Constants;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff
{
	[TestFixture]
	public class CountryCodeLoaderTests
	{
		[Test]
		public void LoadCountryData()
		{
			var countriesToLoad = new string[]
			{
				"ALL COUNTRIES",
				"EU",
				"BRAZIL",
				"UNITED STATES OF AMERICA",
			};

			var mockCountryMatcher = new Mock<ICountryMatcher>();

			mockCountryMatcher.Setup(m => m.GetCountryCodes(new string[] { "BRAZIL", "UNITED STATES OF AMERICA" })).Returns(new string[] { "BR", "US" });

			var logger = new TestLogger();
			var loader = new CountryCodeLoader(mockCountryMatcher.Object, logger) as ICountryCodeLoader;

			var result = loader.GetCountryData(countriesToLoad);

			Assert.That(result, Is.Not.Null);
			Assert.That(result.Count, Is.EqualTo(4));
			Assert.That(result.ContainsKey("ALL COUNTRIES"), Is.EqualTo(true));
			Assert.That(result["ALL COUNTRIES"], Is.EqualTo(TradeGroups.Standard));
			Assert.That(result.ContainsKey("EU"), Is.EqualTo(true));
			Assert.That(result["EU"], Is.EqualTo(TradeGroups.EU));
			Assert.That(result.ContainsKey("BRAZIL"), Is.EqualTo(true));
			Assert.That(result["BRAZIL"], Is.EqualTo("BR"));
			Assert.That(result.ContainsKey("ALL COUNTRIES"), Is.EqualTo(true));
			Assert.That(result["UNITED STATES OF AMERICA"], Is.EqualTo("US"));
		}

		[Test]
		public void LoaderExceptionLogged()
		{
			var countriesToLoad = new string[]
			{
				"ALL COUNTRIES",
				"EU",
				"BRAZIL"
			};

			var mockCountryMatcher = new Mock<ICountryMatcher>();
			mockCountryMatcher.Setup(x => x.GetCountryCodes(It.IsAny<string[]>())).Callback(() => throw new Exception("I just cant do it"));

			var logger = new TestLogger();
			var loader = new CountryCodeLoader(mockCountryMatcher.Object, logger) as ICountryCodeLoader;

			var result = loader.GetCountryData(countriesToLoad);

			Assert.That(result, Is.Not.Null);
			Assert.That(result.Count, Is.EqualTo(2));
			Assert.That(logger.ErrorString, Is.EqualTo("Failed to retrieve countries: I just cant do it\r\n"));
		}
	}
}
