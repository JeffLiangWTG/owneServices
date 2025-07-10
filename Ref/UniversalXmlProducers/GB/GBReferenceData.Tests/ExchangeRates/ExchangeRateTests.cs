using CargoWise.RefDbRepo.GBReferenceData.Services.ExchangeRates;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.ExchangeRates
{
	[TestFixture]
	internal class ExchangeRateTests
	{
		[Test]
		public void IsValid()
		{
			var model = new ExchangeRate();

			Assert.That(model.IsValid(), Is.False);
			model.Currency = "EUR";
			Assert.That(model.IsValid(), Is.False);
			model.Rate = 123.45m;
			Assert.That(model.IsValid(), Is.True);
		}
	}
}
