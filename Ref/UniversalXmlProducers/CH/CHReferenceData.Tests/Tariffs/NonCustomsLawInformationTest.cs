using CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests.Tariffs
{
	internal class NonCustomsLawInformationTest
	{
		[Test]
		public void TestImport() => Assert.Multiple(() =>
		{
			DownloadResult download = new DownloadResult
			{
				Content = GetType().GetTestArray("TestFiles.Input.statistical_keys_import_with_nze_latest_format.xlsx"),
			};

			var nonCustomsLawInformation = new NonCustomsLawInformation(download);

			var key = "0106.1200|0|190";
			Assert.IsTrue(nonCustomsLawInformation.TryGetValue(key, out Description description), key);
			Assert.That(description.TextD, Is.EqualTo("Wale, Delfine und Tümmler (Säugetiere der Ordnung der Cetacea): Einfurverbot"), nameof(description.TextD));
			Assert.That(description.TextF, Is.EqualTo("baleines, dauphins et marsouins (mammifères de l'ordre des cétacés): interdiction d'importation"), nameof(description.TextF));
			Assert.That(description.TextI, Is.EqualTo("balene, delfini e marsovini (mammiferi della specie dei cetacei): divieto d'importazione"), nameof(description.TextI));
			Assert.That(description.TextE, Is.EqualTo("whales, dolphins and porpoises (mammals of the order Cetacea): import ban"), nameof(description.TextE));
		});

		[Test]
		public void TestExport() => Assert.Multiple(() =>
		{
			DownloadResult download = new DownloadResult
			{
				Content = GetType().GetTestArray("TestFiles.Input.statistical_keys_export_with_nze_latest_format.xlsx"),
			};

			var nonCustomsLawInformation = new NonCustomsLawInformation(download);

			var key = "0502.1000|0|66";
			Assert.IsTrue(nonCustomsLawInformation.TryGetValue(key, out Description description), key);
			Assert.That(description.TextD, Is.EqualTo(@"s. ""Bemerkungen"", ""Abfallrecht"""), nameof(description.TextD));
			Assert.That(description.TextF, Is.EqualTo(@"v. ""Remarques"", "" Législation sur les déchets"""), nameof(description.TextF));
			Assert.That(description.TextI, Is.EqualTo(@"v. ""Osservazioni"", ""Legislazione sui rifiuti"""), nameof(description.TextI));
			Assert.That(description.TextE, Is.EqualTo(@"s. ""Remarks"", "" Waste legislation"""), nameof(description.TextE));
		});
	}
}
