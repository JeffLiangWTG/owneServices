using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Services.Loaders
{
	[TestFixture]
	public class DutyCodesLoaderTest
	{
		[Test]
		public void LoadDataTotalCount()
		{
			var records = DutyCodesLoader.LoadData(DataFileName).ToList();
			Assert.AreEqual(26, records.Sum(r => r.RefCusRateCodes.Length), "Total loaded row count is different than expected.");
		}

		[Test]
		[TestCase("10", "DTY", "Customs Duty", "Gümrük Vergisi")]
		[TestCase("12", "DTY", "Single and Cut off Tax", "Tek ve Maktu Vergi")]
		[TestCase("20", "ADD", "Duty for Anti-Dumping", "Dampinge Karşı Vergi")]
		[TestCase("21", "CVD", "Duty for Subsidy", "Sübvansiyona Karşı Vergi")]
		[TestCase("29", "LEV", "Environmental Contribution", "Çevre Katkı Payı")]
		[TestCase("39", "DTY", "Additional Financial Liability", "Ek Mali Yükümlülük")]
		public void LoadData(string rateCode, string rateType, string descriptionEN, string descriptionTR)
		{
			var records = DutyCodesLoader.LoadData(DataFileName).ToList();

			Assert.IsTrue(records.Count > 0, "Loader should not return empty records");

			var refCusRateType = records.FirstOrDefault(x => x.ZZR_RateType == rateType);
			Assert.IsNotNull(refCusRateType, $"RateType {rateType} could not be found");

			var refCusRateCode = refCusRateType.RefCusRateCodes.FirstOrDefault(x => x.ZY1_RateCode == rateCode);
			Assert.IsNotNull(refCusRateCode, $"RateCode {rateCode} could not be found");

			Assert.AreEqual(rateCode, refCusRateCode.ZY1_RateCode);
			Assert.AreEqual(descriptionEN, refCusRateCode.ZY1_Description, $"EN description does not match with: {rateType}");

			var refCusRateCodeLang = refCusRateCode.RefCusRateCodeLanguages.FirstOrDefault();
			Assert.IsNotNull(refCusRateCodeLang, "Turkish description is missing");
			Assert.AreEqual(descriptionTR, refCusRateCodeLang.ZXC_Description, $"TR description does not match with: {rateType}");
		}

		[Test]
		public void LoadDataShouldReadCorrectlyWhenColumnOrderChanges()
		{
			var records = DutyCodesLoader.LoadData(DataFileNameColumnShuffled).ToList();
			Assert.IsTrue(records.Count > 0, "Loader should still return records even if column order changes");

			var rateCode = "10";
			var rateType = "DTY";
			var descriptionEN = "Customs Duty";
			var descriptionTR = "Gümrük Vergisi";

			var refCusRateType = records.FirstOrDefault(x => x.ZZR_RateType == rateType);
			Assert.IsNotNull(refCusRateType, "RateType should be found even if column order changes");

			var refCusRateCode = refCusRateType.RefCusRateCodes.FirstOrDefault(x => x.ZY1_RateCode == rateCode);
			Assert.IsNotNull(refCusRateCode, "RateCode should be found even if column order changes");

			Assert.AreEqual(descriptionEN, refCusRateCode.ZY1_Description, "EN description should match even if column order changes");
			Assert.AreEqual(descriptionTR, refCusRateCode.RefCusRateCodeLanguages.First().ZXC_Description, "TR description should match even if column order changes");
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(tempFolder);
			TestHelper.SimulateDownload(DataFileName, "CargoWise.RefDbRepo.TRReferenceData.Tests.DutyCodes.TestFiles.Input.DutyCodes.xlsx");
			TestHelper.SimulateDownload(DataFileNameColumnShuffled, "CargoWise.RefDbRepo.TRReferenceData.Tests.DutyCodes.TestFiles.Input.DutyCodes_Shuffled.xlsx");
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			if (Directory.Exists(tempFolder))
			{
				Directory.Delete(tempFolder, true);
			}
		}

		string tempFolder;
		string DataFileName => Path.Combine(tempFolder, "DutyCodes.xlsx");
		string DataFileNameColumnShuffled => Path.Combine(tempFolder, "DutyCodes_Shuffled.xlsx");
	}
}
