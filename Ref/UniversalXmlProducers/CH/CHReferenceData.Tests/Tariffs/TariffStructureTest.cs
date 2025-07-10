using CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests.Tariffs
{
	[TestFixture]
	class TariffStructureTest
	{
		[Test]
		public void TestDescriptions() => Assert.Multiple(() =>
		{
			DownloadResult tariffStructureDownload = new DownloadResult
			{
				Content = GetType().GetTestArray("TestFiles.Input.Tarifstruktur.xlsx"),
			};

			var tariffStructure = new TariffStructure().Load(tariffStructureDownload);
			Description description;

			Assert.IsTrue(tariffStructure.TryGetValue("0101.2911", out description), "Contains 0101.2911");
			Assert.AreEqual("innerhalb des Zollkontingents (K-Nr. 5) eingeführt", description.TextD);
			Assert.AreEqual("importés dans les limites du contingent tarifaire (c. n° 5)", description.TextF);
			Assert.AreEqual("importati nei limiti del contingente doganale (n. cont. 5)", description.TextI);
			Assert.AreEqual("within the limits of the tariff quota (Q. No. 5)", description.TextE);
			Assert.AreEqual("01.01.01.29.00.01.01", description.SortKey);

			Assert.IsTrue(tariffStructure.TryGetValue("9706.0000", out description));
			Assert.AreEqual("Antiquitäten, mehr als 100 Jahre alt", description.TextD);
			Assert.AreEqual("Objets d'antiquité ayant plus de 100 ans d'âge", description.TextF);
			Assert.AreEqual("Oggetti di antichità aventi più di cento anni di età", description.TextI);
			Assert.AreEqual("Antiques of an age exceeding one hundred years", description.TextE);
			Assert.AreEqual("21.97.03.03.03", description.SortKey);
		});

		[Test]
		public void TestDescriptions_LatestFormat() => Assert.Multiple(() =>
		{
			DownloadResult tariffStructureDownload = new DownloadResult
			{
				Content = GetType().GetTestArray("TestFiles.Input.Tarifstruktur_latest_format.xlsx"),
			};

			var tariffStructure = new TariffStructure().Load(tariffStructureDownload);
			Description description;

			Assert.IsTrue(tariffStructure.TryGetValue("0101.2110", out description), "Contains 0101.2110");
			Assert.AreEqual("innerhalb des Zollkontingents (K-Nr. 1) eingeführt", description.TextD);
			Assert.AreEqual("importés dans les limites du contingent tarifaire (c. n° 1)", description.TextF);
			Assert.AreEqual("importati nei limiti del contingente doganale (n. cont. 1)", description.TextI);
			Assert.AreEqual("within the limits of the tariff quota (Q. No. 1)", description.TextE);
			Assert.AreEqual("01.01.01.21.00.01", description.SortKey);
		});
	}
}
