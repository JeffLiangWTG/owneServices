using System;
using System.Linq;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.ExchangeRates;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Helpers;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.ExchangeRates
{
	[TestFixture]
	sealed class GesmesLoaderTests
	{
		[Test]
		public void LoadExchangeRates()
		{
			var msgContent = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.ExchangeRates.TestFiles.Input.Gesmes_Valid_01.txt");
			var msg = EdifactLoader.LoadGesmesMessage(msgContent);

			var effectiveDt = new DateTime(2016, 4, 3);

			var rates = GesmesLoader.PopulateExchange(msg);
			Assert.That(rates, Is.Not.Null);
			Assert.That(rates.Count, Is.EqualTo(18));

			var rate = rates.FirstOrDefault(x => x.Currency == "MWK");

			Assert.That(rate, Is.Not.Null);
			Assert.That(rate.CountryCode, Is.Null.Or.Empty);
			Assert.That(rate.Rate, Is.EqualTo(45.273950m));
			Assert.That(rate.StartDate, Is.EqualTo(effectiveDt));
			Assert.That(rate.EndDate, Is.EqualTo(effectiveDt));
		}
	}
}
