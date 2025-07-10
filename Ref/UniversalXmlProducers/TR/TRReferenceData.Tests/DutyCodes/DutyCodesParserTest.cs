using System;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.TRReferenceData.Business;
using CargoWise.RefDbRepo.TRReferenceData.Business.DutyCodesParser;
using CargoWise.RefDbRepo.TRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests
{
	[TestFixture]
	public class DutyCodesParserTest
	{
		[Test]
		[TestCase("10", "DTY", "Customs Duty", "Gümrük Vergisi")]
		[TestCase("51", "EXC", "SCT (Sepecial Consumtion Tax) List III", "Özel Tüketim Vergisi liste-III")]
		[TestCase("72", "TOB", "Tobacco Fund", "Tütün Fonu")]
		public void GetEntities(string rateCode, string rateType, string descriptionEN, string descriptionTR)
		{
			var refCusRateTypeList = DutyCodesParser.GetEntities().Cast<RefCusRateType>();

			Assert.AreEqual(9, refCusRateTypeList.Count());

			var refCusRateType = refCusRateTypeList.FirstOrDefault(x => x.ZZR_RateType == rateType);
			Assert.IsNotNull(refCusRateType, $"RefCusRateType with ZZR_RateType {rateType} should be found.");

			var refCusRateCode = refCusRateType.RefCusRateCodes.FirstOrDefault(x => x.ZY1_RateCode == rateCode);
			Assert.IsNotNull(refCusRateCode, $"RefCusRateCode with ZY1_RateCode {rateCode} should be found for rateType {rateType}.");

			Assert.AreEqual(descriptionEN, refCusRateCode.ZY1_Description, $"EN description mismatch for rateType {rateType}.");
			var refCusRateCodeLang = refCusRateCode.RefCusRateCodeLanguages.FirstOrDefault();

			Assert.IsNotNull(refCusRateCodeLang, $"Language data not found for rateType {rateType}.");
			Assert.AreEqual(descriptionTR, refCusRateCodeLang.ZXC_Description, $"TR description mismatch for rateType {rateType}.");
		}


		[Test]
		public void GetXmlWriterConfiguration()
		{
			var writerConfig = DutyCodesParser.GetXmlWriterConfiguration();
			Assert.That(writerConfig, Is.Not.Null);

			var refType = typeof(RefCusRateType);
			var entityConfig = writerConfig.GetConfiguration(refType);

			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRateType.ZZR_RateType))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRateType.ZZR_ZZZ_NKDataGrouping))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRateType.RefCusRateCodes))));

			refType = typeof(RefCusRateCode);
			entityConfig = writerConfig.GetConfiguration(refType);

			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRateCode.ZY1_RateCode))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRateCode.ZY1_Description))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRateCode.ZY1_ZZZ_NKDataGrouping))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRateCode.ZY1_InternalUse))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRateCode.RefCusRateCodeLanguages))));

			refType = typeof(RefCusRateCodeLanguage);
			entityConfig = writerConfig.GetConfiguration(refType);

			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRateCodeLanguage.ZXC_ZX6_NKLanguage))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusRateCodeLanguage.ZXC_Description))));

			TestHelper.AssertKeySets(nameof(RefCusRateCodeLanguage), entityConfig.GetKeySets(),
				nameof(RefCusRateCodeLanguage.ZXC_ZX6_NKLanguage));
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(tempFolder);
			TestHelper.SimulateDownload(DataFileName, "CargoWise.RefDbRepo.TRReferenceData.Tests.DutyCodes.TestFiles.Input.DutyCodes.xlsx");
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

		IDateTimeProvider DateTimeProvider => TestHelper.MockDateTimeProvider(DateTime.Now);

		IReferenceDataParser DutyCodesParser => fDutyCodesParser ?? (fDutyCodesParser = new DutyCodesParser(DateTimeProvider, DataFileName));
		DutyCodesParser fDutyCodesParser;
	}
}
