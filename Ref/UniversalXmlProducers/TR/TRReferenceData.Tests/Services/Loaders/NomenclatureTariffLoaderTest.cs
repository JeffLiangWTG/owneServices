using System;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Services
{
	[TestFixture]
	public class NomenclatureTariffLoaderTest
	{
		[Test]
		public void LoadData_SkipHeadersForMultipleDataFiles()
		{
			TestHelper.SimulateDownload(DataFilePathForMultiple, "CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs.TestFiles.Input.NomenclatureTariff.fasil.05 fasil 2021.xls");
			var records = NomenclatureTariffLoader.LoadData(DataFilePathForMultiple, "05").Records;
			var firstRow = records.OrderBy(r => r.SeqNum).First();
			Assert.That(firstRow.SeqNum, Is.EqualTo(6));
		}

		[Test]
		public void LoadData_DescriptionIsNotEmptyForMultipleDataFiles()
		{
			TestHelper.SimulateDownload(DataFilePathForMultiple, "CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs.TestFiles.Input.NomenclatureTariff.fasil.05 fasil 2021.xls");
			var records = NomenclatureTariffLoader.LoadData(DataFilePathForMultiple, "05").Records;
			Assert.That(records.All(r => !string.IsNullOrEmpty(r.Description)));
		}

		[Test]
		public void LoadData_LoadedCorrectlyForMultipleDataFiles()
		{
			TestHelper.SimulateDownload(DataFilePathForMultiple, "CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs.TestFiles.Input.NomenclatureTariff.fasil.05 fasil 2021.xls");
			var records = NomenclatureTariffLoader.LoadData(DataFilePathForMultiple, "05").Records;
			Assert.That(records.Count(), Is.EqualTo(86));

			var row6 = records.Single(r => r.SeqNum == 6);
			Assert.That(row6.Code, Is.EqualTo("0501.00.00.00.00"));
			Assert.That(row6.Description, Is.EqualTo("İnsan saçı (işlenmemiş, yıkanmış veya yağı alınmış olsun"));

			var row7 = records.Single(r => r.SeqNum == 7);
			Assert.That(row7.Code, Is.EqualTo(string.Empty));
			Assert.That(row7.Description, Is.EqualTo("olmasın) ; insan saçı döküntüleri"));
			Assert.That(row7.UOM, Is.EqualTo("-"));
			Assert.That(row7.DutyRate, Is.EqualTo("25"));

			var row103 = records.Single(r => r.SeqNum == 103);
			Assert.That(row103.Code, Is.EqualTo("0511.99.85.90.18"));
			Assert.That(row103.Description, Is.EqualTo(" - - - - - Diğerleri"));
			Assert.That(row103.UOM, Is.EqualTo("-"));
			Assert.That(row103.DutyRate, Is.EqualTo("30"));
		}

		[Test]
		public void LoadData_SkipHeadersForSingleDataFile()
		{
			TestHelper.SimulateDownload(DataFilePathForSingle, "CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs.TestFiles.Input.NomenclatureTariff.fasil.2023 Tariff Codes.xls");
			var records = NomenclatureTariffLoader.LoadData(DataFilePathForSingle);
			var firstRow = records.OrderBy(r => r.SeqNum).First();
			Assert.That(firstRow.SeqNum, Is.EqualTo(2));
		}

		[Test]
		public void LoadData_CodeDoesNotHaveLeadingNsbp()
		{
			TestHelper.SimulateDownload(DataFilePathForSingle, "CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs.TestFiles.Input.NomenclatureTariff.fasil.2023 Tariff Codes.xls");
			var records = NomenclatureTariffLoader.LoadData(DataFilePathForSingle);
			Assert.That(records.All(r => string.IsNullOrEmpty(r.Code) || r.Code.TrimStart()[0] != (char)0xA0));
		}

		[Test]
		public void LoadData_DescriptionIsNotEmptyForSingleDataFile()
		{
			TestHelper.SimulateDownload(DataFilePathForSingle, "CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs.TestFiles.Input.NomenclatureTariff.fasil.2023 Tariff Codes.xls");
			var records = NomenclatureTariffLoader.LoadData(DataFilePathForSingle);
			Assert.That(records.All(r => !string.IsNullOrEmpty(r.Description)));
		}

		[Test]
		public void LoadData_LoadedCorrectlyForSingleDataFile()
		{
			TestHelper.SimulateDownload(DataFilePathForSingle, "CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs.TestFiles.Input.NomenclatureTariff.fasil.2023 Tariff Codes.xls");
			var records = NomenclatureTariffLoader.LoadData(DataFilePathForSingle);
			Assert.That(records.Count(), Is.EqualTo(189));

			var row6 = records.Single(r => r.SeqNum == 6);
			Assert.That(row6.Code, Is.EqualTo("0101.29.10.00.00"));
			Assert.That(row6.Description, Is.EqualTo("- - - Kesimlik atlar"));

			var row7 = records.Single(r => r.SeqNum == 7);
			Assert.That(row7.Code, Is.EqualTo("0101.29.90.00.00"));
			Assert.That(row7.Description, Is.EqualTo("- - - Diğerleri"));
			Assert.That(row7.UOM, Is.EqualTo("Baş"));
			Assert.That(row7.DutyRate, Is.EqualTo("35"));

			var row103 = records.Single(r => r.SeqNum == 103);
			Assert.That(row103.Code, Is.EqualTo("0105.13"));
			Assert.That(row103.Description, Is.EqualTo("- - Ördekler"));
			Assert.That(row103.UOM, Is.EqualTo(""));
			Assert.That(row103.DutyRate, Is.EqualTo(""));
		}

		[Test]
		public void LoadPublicationTime()
		{
			var publicationTime = NomenclatureTariffLoader.LoadPublicationTime(PublicationTimeFilePath);
			Assert.That(publicationTime, Is.EqualTo(new DateTime(2023, 1, 1)));
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempFolder);
			DataFilePathForMultiple = Path.Combine(TempFolder, "05 fasil 2021.xls");
			DataFilePathForSingle = Path.Combine(TempFolder, "2023 Tariff Codes.xls");

			PublicationTimeFilePath = Path.Combine(TempFolder, "Tariff Publication Time.xlsx");
			TestHelper.SimulateDownload(PublicationTimeFilePath, "CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs.TestFiles.Input.NomenclatureTariff.Tariff Publication Time.xlsx");
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		string TempFolder;
		string PublicationTimeFilePath;
		string DataFilePathForMultiple;
		string DataFilePathForSingle;
	}
}
