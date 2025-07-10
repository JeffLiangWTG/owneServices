using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Services
{
	class HsnTariffSCDListIVDutyRatesLoaderTest
	{
		[Test]
		public void TestExplain_ShouldParse_RowsSuccessfully()
		{
			var logger = new StringBuilder();
			var result = HsnTariffSCDListIVDutyRatesLoader.GetRates(ListFilePath, logger).ToList();

			Assert.That(result, Is.Not.Null);
			Assert.That(result.Count, Is.EqualTo(3));

			var parentRow = result[0];
			Assert.Multiple(() =>
			{
				Assert.That(parentRow.TariffCodePattern, Is.EqualTo("1234"));
				Assert.That(parentRow.Description, Is.EqualTo("Parent Row"));
				Assert.That(parentRow.Rate, Is.EqualTo(0.10m));
				Assert.That(parentRow.ExemptedTariffCodes, Is.EquivalentTo(new[] { "A", "B" }));
				Assert.That(parentRow.AdditionalCode, Is.EqualTo("ADD1"));
			});

			var child1 = result[1];
			Assert.Multiple(() =>
			{
				Assert.That(child1.TariffCodePattern, Is.EqualTo("1234"));
				Assert.That(child1.Description, Is.EqualTo("Parent RowChild 1"));
				Assert.That(child1.Rate, Is.EqualTo(0.10m));
			});

			var child2 = result[2];
			Assert.Multiple(() =>
			{
				Assert.That(child2.TariffCodePattern, Is.EqualTo("1234"));
				Assert.That(child2.Description, Is.EqualTo("Parent RowChild 2"));
				Assert.That(child2.Rate, Is.EqualTo(0.05m));
			});
		}

		[Test]
		public void TestExplain_ShouldLog_WhenRateIsMissing()
		{
			var logger = new StringBuilder();
			var result = HsnTariffSCDListIVDutyRatesLoader.GetRates(ListFilePath, logger).ToList();

			Assert.That(logger.ToString(), Does.Contain("unable to find a rate"));
			Assert.That(logger.Length, Is.GreaterThan(0), "Logger should have captured an error for missing rate.");
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempFolder);
			ListFilePath = Path.Combine(TempFolder, "HsnTariffSCDListIVDutyRates.xlsx");
			TestHelper.SimulateDownload(ListFilePath, "CargoWise.RefDbRepo.TRReferenceData.Tests.Services.TestFiles.HsnTariffSCDListIVDutyRates.xlsx");
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		string TempFolder;
		string ListFilePath;
	}
}
