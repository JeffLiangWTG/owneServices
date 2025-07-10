using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Services.Loaders
{
	[TestFixture]
	public class RefCusTaxOrFeeLoaderTest
	{
		[Test]
		[TestCase("89", "Stamp Duty", "Damga Vergisi", 898.20, 2025, 2025)]
		[TestCase("OBS", "Ordino Stamp Duty", "Ordino Damga Vergisi", 4.80, 2025, 2025)]
		[TestCase("GMS", "Global Manifest Stamp Duty", "ÖZBY Damga Vergisi", 100.40, 2025, 2025)]
		[TestCase("SBS", "Sea Bill Stamp Duty", "Deniz Taşıma Senedi Damga Vergisi", 136.10, 2025, 2025)]
		[TestCase("ABS", "Air Bill Stamp Duty", "Havayolu Taşıma Senedi Damga Vergisi", 4.80, 2025, 2025)]
		[TestCase("23", "D-PPI amount according to Law No. 7143", "7143 sayılı Kanuna göre Yİ-ÜFE tutarı", 0.00, 1990, 2079)]
		[TestCase("951", "Open Space And Warehouse Use. Waist. Fees (USD)", "Açik Alan Ve Depo Kul. Bel. ÜcreTLeri (USD)", 0.00, 1990, 2079)]
		[TestCase("994", "S.B. Share Of Goods Inflow-Output Rda (01% And 09%) (TL)", "S.B. Mal Giriş-Çikiş Bki Payi (%01 Ve %09) (TL)", 0.00, 1990, 2079)]
		public void LoadData(string code, string description, string descriptionTR, decimal value, int startDate, int endDate)
		{
			var refCusTaxOrFeeList = RefCusTaxOrFeeLoader.LoadData(DataFileName).ToList();

			Assert.AreEqual(42, refCusTaxOrFeeList.Count);

			var refCusTaxOrFee = refCusTaxOrFeeList.FirstOrDefault(x => x.ZZF_Code == code);

			Assert.IsNotNull(refCusTaxOrFee, $" RefCusTaxOrFee with ZZ_Code {code} should be found.");

			Assert.AreEqual(code, refCusTaxOrFee.ZZF_Code);
			Assert.AreEqual(description, refCusTaxOrFee.ZZF_Description);
			Assert.AreEqual(value, refCusTaxOrFee.ZZF_Value);
			Assert.AreEqual(startDate, refCusTaxOrFee.ZZF_StartDate.Year);
			Assert.AreEqual(endDate, refCusTaxOrFee.ZZF_EndDate.Year);
			Assert.AreEqual("TR", refCusTaxOrFee.ZZF_ZZZ_NKDataGrouping);
			Assert.AreEqual(0m, refCusTaxOrFee.ZZF_Minimum);
			Assert.AreEqual(0m, refCusTaxOrFee.ZZF_Maximum);
			Assert.AreEqual(0m, refCusTaxOrFee.ZZF_Threshold);
			Assert.AreEqual("OTH", refCusTaxOrFee.ZZF_ZX0_NKTaxOrFeeType);

			var refCusTaxOrFeeLanguage = refCusTaxOrFee.RefCusTaxOrFeeLanguages.FirstOrDefault();
			Assert.IsNotNull(refCusTaxOrFeeLanguage, "Turkish description is missing");
			Assert.AreEqual(descriptionTR, refCusTaxOrFeeLanguage.ZXU_Description, $"TR description does not match with: {descriptionTR}");
			Assert.AreEqual(Business.Constants.CountryCodeTR, refCusTaxOrFeeLanguage.ZXU_ZX6_NKLanguage, "Language not correct");
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(tempFolder);
			TestHelper.SimulateDownload(DataFileName, "CargoWise.RefDbRepo.TRReferenceData.Tests.RefCusTaxOrFee.TestFiles.Input.TR Stamp Duty.xlsx");
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

		string DataFileName => Path.Combine(tempFolder, "TR Stamp Duty.xlsx");
	}
}
