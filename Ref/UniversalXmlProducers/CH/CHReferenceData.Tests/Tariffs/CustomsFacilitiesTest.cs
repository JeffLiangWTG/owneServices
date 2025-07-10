using CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests.Tariffs
{
	internal class CustomsFacilitiesTest
	{
		[Test]
		public void Test_customs_facilities() => Assert.Multiple(() =>
		{
			DownloadResult tariffStructureDownload = new DownloadResult
			{
				Content = GetType().GetTestArray("TestFiles.Input.customs_facilities_latest_format.xlsx"),
			};

			var customsFacilities = new CustomsFacilities(tariffStructureDownload);
			Description description;

			Assert.IsTrue(customsFacilities.TryGetValue(("0103.1090", 1), out description), "Contains 0103.1090 / 01");
			Assert.AreEqual("zu Forschungs- oder medizinischen Zwecken", description.TextD, "TextD");
			Assert.AreEqual("pour la recherche ou des buts médicaux", description.TextF, "TextF");
			Assert.AreEqual("per la ricerca o la medicina", description.TextI, "TextI");
			Assert.AreEqual("for research or for medical purposes", description.TextE, "TextE");
		});

		[Test]
		public void Test_customs_facilities_2() => Assert.Multiple(() =>
		{
			DownloadResult tariffStructureDownload = new DownloadResult
			{
				Content = GetType().GetTestArray("TestFiles.Input.customs_facilities_2.xlsx"),
			};

			var customsFacilities = new CustomsFacilities(tariffStructureDownload);
			Description description;

			Assert.IsTrue(customsFacilities.TryGetValue(("0103.1090", 1), out description), "Contains 0103.1090 / 01");
			Assert.AreEqual("zu Forschungs- oder medizinischen Zwecken", description.TextD, "TextD");
			Assert.AreEqual("pour la recherche ou des buts médicaux", description.TextF, "TextF");
			Assert.AreEqual("per la ricerca o la medicina", description.TextI, "TextI");
			Assert.AreEqual("for research or for medical purposes", description.TextE, "TextE");
		});
	}
}
