using System;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Services.Loaders
{
	[TestFixture]
	public class ETradeExemptionCodesLoaderTest
	{
		[Test]
		public void LoadData()
		{
			var endDate = new DateTime(2024, 5, 4, 23, 59, 00);
			var records = ETradeExemptionCodesLoader.LoadData(DataFilePath);
			Assert.AreEqual(22, records.Count());

			var pairs = records.Where(r => r.ExemptionCode == "ILAC18");
			Assert.AreEqual(1, pairs.Count());

			var pair = pairs.First();
			Assert.AreEqual("The scope of article 62/1/a of the decision is a medicine that comes from European Union countries and whose value does not exceed 1500 Euros and whose weight does not exceed 30 kilograms.", pair.ExemptionDescEnglish);
			Assert.AreEqual("VFD * 0.18", pair.DutyFormula);
			Assert.AreEqual(endDate, pair.EndDate);

			pairs = records.Where(r => r.ExemptionCode == "DIPL");
			pair = pairs.First();
			Assert.AreEqual(1, pairs.Count());
			Assert.AreEqual("Diplomatic goods arriving on behalf of embassies, consulates or international organizations within the scope of article 126/1/a of the decision.", pair.ExemptionDescEnglish);
			Assert.AreEqual("Kararın 126/1/a maddesi kapsamı elçilik, konsolosluk veya milletlerarası kuruluşlar adına gelen diplomatik eşya.", pair.ExemptionDescTurkish);
			Assert.AreEqual("0", pair.DutyFormula);
			Assert.AreEqual("0", pair.DutyPercent);
			Assert.AreEqual("10", pair.RateCode);
			Assert.AreEqual("ETR", pair.RateType);
			Assert.AreEqual("All Countries", pair.TradeGroup);

			pairs = records.Where(r => r.ExemptionCode == "HK18");
			pair = pairs.First();
			Assert.AreEqual(1, pairs.Count());
			Assert.AreEqual("The scope of article 62/1/a of the decision is the goods coming from European Union countries, whose value does not exceed 150 Euros and whose weight does not exceed 30 kilograms.", pair.ExemptionDescEnglish);
			Assert.AreEqual("Kararın 62/1/a maddesi kapsamı Avrupa Birliği ülkelerinden gelen, kıymeti 150 Avro’yu, ağırlığı 30 kilogramı geçmeyen eşya.", pair.ExemptionDescTurkish);
			Assert.AreEqual("VFD * 0.18", pair.DutyFormula);
			Assert.AreEqual("18", pair.DutyPercent);
			Assert.AreEqual("10", pair.RateCode);
			Assert.AreEqual("ETR", pair.RateType);
			Assert.AreEqual("EU", pair.TradeGroup);
			Assert.AreEqual(endDate, pair.EndDate);

			pairs = records.Where(r => r.ExemptionCode == "ILAC20");
			Assert.AreEqual(1, pairs.Count());

			pair = pairs.First();
			Assert.AreEqual("The scope of article 62/1/a of the decision is a medicine that comes from European Union countries and whose value does not exceed 1500 Euros and whose weight does not exceed 30 kilograms.", pair.ExemptionDescEnglish);
			Assert.AreEqual("VFD * 0.20", pair.DutyFormula);

			pairs = records.Where(r => r.ExemptionCode == "HK20");
			pair = pairs.First();
			Assert.AreEqual(1, pairs.Count());
			Assert.AreEqual("The scope of article 62/1/a of the decision is the goods coming from European Union countries, whose value does not exceed 150 Euros and whose weight does not exceed 30 kilograms.", pair.ExemptionDescEnglish);
			Assert.AreEqual("Kararın 62/1/a maddesi kapsamı Avrupa Birliği ülkelerinden gelen, kıymeti 150 Avro’yu, ağırlığı 30 kilogramı geçmeyen eşya.", pair.ExemptionDescTurkish);
			Assert.AreEqual("VFD * 0.20", pair.DutyFormula);
			Assert.AreEqual("20%", pair.DutyPercent);
			Assert.AreEqual("10", pair.RateCode);
			Assert.AreEqual("ETR", pair.RateType);
			Assert.AreEqual("EU", pair.TradeGroup);
		}


		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempFolder);
			DataFilePath = Path.Combine(TempFolder, "ETradeExemptionCodes.xlsx");
			TestHelper.SimulateDownload(DataFilePath, "CargoWise.RefDbRepo.TRReferenceData.Tests.ETradeExemptionCodes.TestFiles.Input.ETradeExemptionCodes.xlsx");
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
