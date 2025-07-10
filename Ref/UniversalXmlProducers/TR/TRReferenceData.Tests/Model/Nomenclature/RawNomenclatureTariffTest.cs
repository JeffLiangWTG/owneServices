using System.IO;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;
using FlexCel.Core;
using FlexCel.XlsAdapter;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests
{
	class RawNomenclatureTariffTest
	{
		[TestCase(THFlxAlignment.center, true)]
		[TestCase(THFlxAlignment.left, false)]
		[TestCase(THFlxAlignment.right, false)]
		[TestCase(null, false)]
		public void IsHorizontalCenter(THFlxAlignment? alignment, bool expectedResult)
		{
			var excelFile = new XlsFile(DataFilePath);
			TFlxFormat format = excelFile.GetDefaultFormat;

			if (alignment.HasValue)
			{
				format.HAlignment = alignment.Value;
			}

			var tariff = new RawNomenclatureTariff { DescriptionFormat = format };
			var result = tariff.IsHorizontalCenter;

			Assert.AreEqual(expectedResult, result, $"Expected IsHorizontalCenter to be {expectedResult} for alignment {alignment?.ToString() ?? "null"}.");
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempFolder);
			DataFilePath = Path.Combine(TempFolder, "fasil.2023 Tariff Codes.xls");
			TestHelper.SimulateDownload(DataFilePath, "CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs.TestFiles.Input.NomenclatureTariff.fasil.2023 Tariff Codes.xls");
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
		string DataFilePath;
	}
}
