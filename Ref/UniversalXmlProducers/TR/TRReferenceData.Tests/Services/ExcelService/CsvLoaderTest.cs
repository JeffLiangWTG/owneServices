using System.Linq;
using CargoWise.RefDbRepo.TRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Services
{
	[TestFixture]
	class CsvLoaderTest
	{
		[Test]
		public void GetTariffRates()
		{
			var records = CsvLoader.GetBanderolTariffRates();
			Assert.That(records.Count(), Is.EqualTo(37));
			var first = records.First();
			Assert.That(first.TariffCode, Is.EqualTo("1.051"));
			Assert.That(first.Description, Is.EqualTo("Televizyon - 51 Ekrana kadar (51 dahil)"));
			Assert.That(first.RateFormula, Is.EqualTo("10*[BI]"));
			Assert.That(first.UOM, Is.EqualTo("BI"));
			Assert.That(first.Currency, Is.EqualTo("EUR"));
		}

		[Test]
		public void GetTariffHSN()
		{
			var records = CsvLoader.GetDeclarationTariffs();
			Assert.That(records.Count(), Is.EqualTo(308));
			var first = records.First();
			Assert.That(first.TariffCode, Is.EqualTo("392210000011"));
			Assert.That(first.Description, Is.EqualTo("BANYO KÜVETLERI"));
			Assert.That(first.AdditionalCode, Is.EqualTo(string.Empty));
			Assert.That(first.Formula, Is.EqualTo("VFD *  0.08"));
			Assert.That(first.Percent, Is.EqualTo("8"));
		}

		[Test]
		public void GetTradeGroupCountryCodes()
		{
			var records = CsvLoader.GetTradeGroupCountryCodes();
			Assert.That(records.Count(), Is.EqualTo(658));
			Assert.That(records.All(r => r.OriginControl == "YES" || r.OriginControl == "NO"));
			Assert.That(records.All(r => r.ExitCountryControl == "YES" || r.ExitCountryControl == "NO"));
			var last = records.Last();
			Assert.That(last.TradeGroupCode, Is.EqualTo("URD"));
			Assert.That(last.TradeGroupDescription, Is.EqualTo("Ürdün STA"));
			Assert.That(last.CountryCode, Is.EqualTo("JO"));
			Assert.That(last.CountryName, Is.EqualTo("Jordan"));
		}
	}
}
