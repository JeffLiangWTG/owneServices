using CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests.Tariffs
{
	internal class PermitInformationTest
	{
		[Test]
		public void TestImport() => Assert.Multiple(() =>
		{
			DownloadResult download = new DownloadResult
			{
				Content = GetType().GetTestArray("TestFiles.Input.statistical_keys_import_with_Bew_and_Tol_latest_format.xlsx"),
			};

			var permitInformation = new PermitInformation(download);

			var key1 = "0101.2110|911|26|1";
			Assert.IsTrue(permitInformation.TryGetValue(key1, out Description description1), key1);
			Assert.That(description1.TextD, Is.EqualTo(@"GGED, Bewilligung oder Gesundheitsbescheinigung erforderlich (s. ""Bemerkungen"", ""Veterinärrecht"")"), $"{key1}: {nameof(description1.TextD)}");
			Assert.That(description1.TextF, Is.EqualTo(@"DSCE, permis ou certificat sanitaire nécessaire (v. ""Remarques"", ""Législation vétérinaire"")"), $"{key1}: {nameof(description1.TextF)}");
			Assert.That(description1.TextI, Is.EqualTo(@"DSCE, permesso o certificato sanitario necessario (v. ""Osservazioni"", ""Legislazione veterinaria"")"), $"{key1}: {nameof(description1.TextI)}");
			Assert.That(description1.TextE, Is.EqualTo(@"CHED, permit or health certificate necessary (cf. ""Remarks"", ""Veterinary legislation"")"), $"{key1}: {nameof(description1.TextE)}");

			var key2 = "0602.1000|0|44|1";
			Assert.IsTrue(permitInformation.TryGetValue(key2, out Description description2), key2);
			Assert.That(description2.TextD, Is.EqualTo(@"Waldbäume (Anhang 1 der Verordnung über forstliches Vermehrungsgut; SR 921.552.1), in Sendungen von über 200 Stück"), $"{key2}: {nameof(description2.TextD)}");
			Assert.That(description2.TextF, Is.EqualTo(@"arbres forestiers (annexe 1 de l'Ordonnance sur le matériel forestier de reproduction; RS 921.552.1), en envois de plus de 200 pièces"), $"{key2}: {nameof(description2.TextF)}");
			Assert.That(description2.TextI, Is.EqualTo(@"alberi forestali (allegato 1 dell'Ordinanza sul materiale di riproduzione forestale; RS 921.552.1), in invii eccedenti 200 capi"), $"{key2}: {nameof(description2.TextI)}");
			Assert.That(description2.TextE, Is.EqualTo(@"forest trees (Annex 1 of the Ordinance on Forest Reproductive Material; SR 921.552.1), in consignments of over 200 units"), $"{key2}: {nameof(description2.TextE)}");
		});

		[Test]
		public void TestExport() => Assert.Multiple(() =>
		{
			DownloadResult download = new DownloadResult
			{
				Content = GetType().GetTestArray("TestFiles.Input.statistical_keys_export_with_Bew_and_Tol_latest_format.xlsx"),
			};

			var permitInformation = new PermitInformation(download);

			var key1 = "0106.1100|0|31|1";
			Assert.IsTrue(permitInformation.TryGetValue(key1, out Description description1), key1);
			Assert.That(description1.TextD, Is.EqualTo(@"Artenschutz (CITES Fauna) (siehe ""Bemerkungen"", ""Veterinärrecht"", ""CITES"")"), $"{key1}: {nameof(description1.TextD)}");
			Assert.That(description1.TextF, Is.EqualTo(@"conservation des espèces (CITES Fauna) (voir ""Remarques"", ""Législation vétérinaire"", ""CITES"")"), $"{key1}: {nameof(description1.TextF)}");
			Assert.That(description1.TextI, Is.EqualTo(@"conservazione delle specie (CITES Fauna) (vedi ""Osservazioni"", ""Legislazione veterinaria"", ""CITES"")"), $"{key1}: {nameof(description1.TextI)}");
			Assert.That(description1.TextE, Is.EqualTo(@"protection of species (CITES Fauna) (see ""Remarks"", ""Veterinary Legislation"", ""CITES"")"), $"{key1}: {nameof(description1.TextE)}");
		});
	}
}
