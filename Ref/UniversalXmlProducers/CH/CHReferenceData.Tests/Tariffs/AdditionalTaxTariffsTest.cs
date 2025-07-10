using CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests.Tariffs
{
	internal class AdditionalTaxTariffsTest
	{
		[Test]
		public void TestImport_Edec() => Assert.Multiple(() =>
		{
			DownloadResult download = new DownloadResult
			{
				Content = GetType().GetTestArray("TestFiles.Input.statistical_keys_import_with_Zusabg_Edec.xlsx"),
			};

			var additionalTaxTariff = new AdditionalTaxTariffs(download);

			Assert.IsTrue(additionalTaxTariff.TryGetValue(("290", "002"), out AdditionalTaxTariff tariff), "290-002");
			Assert.That(tariff?.assessmentCode, Is.EqualTo(11), nameof(tariff.assessmentCode));
			Assert.That(tariff?.rate, Is.EqualTo(1.47m), nameof(tariff.rate));
			Assert.That(tariff?.rateMin, Is.EqualTo(88m), nameof(tariff.rateMin));
			Assert.That(tariff?.rateMax, Is.EqualTo(676m), nameof(tariff.rateMax));
			Assert.That(tariff?.hasExclusions, Is.EqualTo(true), nameof(tariff.hasExclusions));
			Assert.That(tariff?.relationships.Count, Is.EqualTo(17), nameof(tariff.relationships.Count));
		});

		[Test]
		public void TestImport_Passar() => Assert.Multiple(() =>
		{
			DownloadResult download = new DownloadResult
			{
				Content = GetType().GetTestArray("TestFiles.Input.statistical_keys_import_with_Zusabg_Passar.xlsx"),
			};

			var additionalTaxTariff = new AdditionalTaxTariffs(download);

			Assert.IsTrue(additionalTaxTariff.TryGetValue(("290", "002"), out AdditionalTaxTariff tariff), "290-002");
			Assert.That(tariff?.assessmentCode, Is.EqualTo(206), nameof(tariff.assessmentCode));
			Assert.That(tariff?.rate, Is.EqualTo(1.47m), nameof(tariff.rate));
			Assert.That(tariff?.rateMin, Is.EqualTo(88m), nameof(tariff.rateMin));
			Assert.That(tariff?.rateMax, Is.EqualTo(676m), nameof(tariff.rateMax));
			Assert.That(tariff?.hasExclusions, Is.EqualTo(true), nameof(tariff.hasExclusions));
			Assert.That(tariff?.relationships.Count, Is.EqualTo(17), nameof(tariff.relationships.Count));
		});
	}
}
