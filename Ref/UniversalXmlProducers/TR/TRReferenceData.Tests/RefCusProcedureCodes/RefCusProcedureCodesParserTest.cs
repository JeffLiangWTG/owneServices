using CargoWise.RefDbRepo.TRReferenceData.Business;
using System.IO;
using System;
using NUnit.Framework;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.TRReferenceData.Business.RefCusProcedureCodesParser;
using System.Linq;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests
{
	[TestFixture]
	public class RefCusProcedureCodesParserTest
	{
		[Test]
		[TestCase(
		"EXM", "97AKS", "", "10,59", "2009/15481 s BKK Article 97-2 Component-Parts Exemption",
		"TR", "IMP", true, "", false,
		"N", "N", 2023, 1, 1, 2079, 6, 6,
		"N", "N", "N", "N", "N", "N", "N", "N", false,
		"N", "N", "N", "N", "N",
		"2009/15481 s BKK 97-2 Maddesi Aksam-Parça Muafiyeti", "TR",
		"", "")]

		[TestCase(
		"EXM", "BSGDC", "GENL", "10,39", "General Tariff Quota for EMY on Iron and Steel Products",
		"TR", "IMP", true, "", false,
		"N", "N", 2023, 1, 1, 2079, 6, 6,
		"N", "N", "N", "N", "N", "N", "N", "N", true,
		"N", "N", "N", "N", "N",
		"Demir Çelik ürünlerinde EMY için Genel Tarife Kontenjanı", "TR",
		"", "")]

		[TestCase(
		"EXM", "D1A", "", "89,991", "DIR Conditional exemption",
		"TR", "IMP,EXP", true, "", false,
		"N", "N", 2023, 1, 1, 2079, 6, 6,
		"N", "N", "N", "N", "N", "N", "N", "N", true,
		"N", "N", "N", "N", "N",
		"DİR Şartlı muafiyet", "TR",
		"PAYMENTCODEALL", "L")]

		[TestCase(
		"PRO", "21", "00", "", "Temporary export of goods in free circulation under the outward processing regime",
		"TR", "EXP", false, "", false,
		"N", "N", 1900, 1, 1, 2079, 6, 6,
		"N", "N", "N", "N", "N", "N", "N", "N", false,
		"N", "N", "N", "N", "N",
		"Serbest dolaşımda bulunan eşyanın hariçte işleme rejimi kapsamında geçici ihracatı", "TR",
		"IsValuationCodeMandatory", "Y")]

		[TestCase(
		"PRO", "80", "00", "", "Import accrual paper",
		"TR", "IMP", true, "", false,
		"N", "N", 1900, 1, 1, 2079, 6, 6,
		"N", "N", "N", "N", "N", "N", "N", "N", true,
		"N", "N", "N", "N", "N",
		"Ithalat tahakkuk kağıdı", "TR",
		"", "")]
		public void GetEntities(
			string category, string procedureCode, string previousProcedureCode, string concession, string description,
			string dataGrouping, string shipmentType, bool calculateDuty, string group, bool landedCost,
			string intoWarehouse, string outOfWarehouse, int startYear, int startMonth, int startDay, int endYear, int endMonth, int endDay,
			string intoTempImport, string outOfTempImport, string intoTempExport, string outOfTempExport,
			string intoInwardProcessing, string outOfInwardProcessing, string intoOutwardProcessing, string outOfOutwardProcessing,
			bool calculateVAT, string guaranteeConsumed, string guaranteeReleased, string isTransit,
			string intoVATWarehouse, string outOfVATWarehouse,
			string langDescription, string lang,
			string attrName, string attrValue)
		{
			var refCusProcedureList = RefCusProcedureCodesParser.GetEntities().Cast<RefCusProcedure>().ToList();
			Assert.IsNotNull(refCusProcedureList, "refCusProcedureList should not be null after parsing exemption codes.");

			Assert.AreEqual(280, refCusProcedureList.Count);

			var refCusProcedure = refCusProcedureList.FirstOrDefault(p => p.ZZ6_ProcedureCode+p.ZZ6_PreviousProcedureCode == procedureCode+previousProcedureCode);
			Assert.IsNotNull(refCusProcedure, $"Procedure with code '{procedureCode+previousProcedureCode}' not found");

			Assert.AreEqual(category, refCusProcedure.ZZ6_Category);
			Assert.AreEqual(procedureCode, refCusProcedure.ZZ6_ProcedureCode);
			Assert.AreEqual(previousProcedureCode, refCusProcedure.ZZ6_PreviousProcedureCode);
			Assert.AreEqual(concession, refCusProcedure.ZZ6_Concession);
			Assert.AreEqual(description, refCusProcedure.ZZ6_Description);
			Assert.AreEqual(dataGrouping, refCusProcedure.ZZ6_ZZZ_NKDataGrouping);
			Assert.AreEqual(shipmentType, refCusProcedure.ZZ6_ShipmentType);
			Assert.AreEqual(calculateDuty, refCusProcedure.ZZ6_CalculateDuty);
			Assert.AreEqual(group, refCusProcedure.ZZ6_Group);
			Assert.AreEqual(landedCost, refCusProcedure.ZZ6_LandedCost);
			Assert.AreEqual(intoWarehouse, refCusProcedure.ZZ6_IntoWarehouse);
			Assert.AreEqual(outOfWarehouse, refCusProcedure.ZZ6_OutOfWarehouse);
			Assert.AreEqual(new DateTime(startYear, startMonth, startDay), refCusProcedure.ZZ6_StartDate.Date);
			Assert.AreEqual(new DateTime(endYear, endMonth, endDay), refCusProcedure.ZZ6_EndDate.Date);
			Assert.AreEqual(intoTempImport, refCusProcedure.ZZ6_IntoTemporaryImport);
			Assert.AreEqual(outOfTempImport, refCusProcedure.ZZ6_OutOfTemporaryImport);
			Assert.AreEqual(intoTempExport, refCusProcedure.ZZ6_IntoTemporaryExport);
			Assert.AreEqual(outOfTempExport, refCusProcedure.ZZ6_OutOfTemporaryExport);
			Assert.AreEqual(intoInwardProcessing, refCusProcedure.ZZ6_IntoInwardProcessing);
			Assert.AreEqual(outOfInwardProcessing, refCusProcedure.ZZ6_OutOfInwardProcessing);
			Assert.AreEqual(intoOutwardProcessing, refCusProcedure.ZZ6_IntoOutwardProcessing);
			Assert.AreEqual(outOfOutwardProcessing, refCusProcedure.ZZ6_OutofOutwardProcessing);
			Assert.AreEqual(calculateVAT, refCusProcedure.ZZ6_CalculateVAT);
			Assert.AreEqual(guaranteeConsumed, refCusProcedure.ZZ6_IsGuaranteeConsumed);
			Assert.AreEqual(guaranteeReleased, refCusProcedure.ZZ6_IsGuaranteeReleased);
			Assert.AreEqual(isTransit, refCusProcedure.ZZ6_IsTransit);
			Assert.AreEqual(intoVATWarehouse, refCusProcedure.ZZ6_IntoVATWarehouse);
			Assert.AreEqual(outOfVATWarehouse, refCusProcedure.ZZ6_OutOfVATWarehouse);

			var refCusProcedureLanguage = refCusProcedure.RefCusProcedureLanguages.FirstOrDefault();
			Assert.IsNotNull(refCusProcedureLanguage, "Language entry is missing");
			Assert.AreEqual(langDescription, refCusProcedureLanguage.ZXV_Description);
			Assert.AreEqual(lang, refCusProcedureLanguage.ZXV_ZX6_NKLanguage);

			var refCusProcedureAttribute = refCusProcedure.RefCusProcedureAttributes
				.FirstOrDefault(a => a.ZXB_Name == attrName && a.ZXB_Value == attrValue);
			if (string.IsNullOrWhiteSpace(attrName) && string.IsNullOrWhiteSpace(attrValue))
			{
				Assert.IsNull(refCusProcedureAttribute, "Empty attribute (both name and value) should not exist.");
			}
			else
			{
				Assert.IsNotNull(refCusProcedureAttribute, "Expected attribute entry is missing.");
				Assert.AreEqual(attrName, refCusProcedureAttribute.ZXB_Name);
				Assert.AreEqual(attrValue, refCusProcedureAttribute.ZXB_Value);
			}
		}

		[Test]
		public void GetXmlWriterConfiguration()
		{
			var config = RefCusProcedureCodesParser.GetXmlWriterConfiguration();
			Assert.IsNotNull(config, "XML Write Configuration is missing");

			var refType = typeof(RefCusProcedure);
			var entityConfig = config.GetConfiguration(refType);

			Assert.IsNotNull(config, "RefCusProcedure Configuration is missing");
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_Category))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_ProcedureCode))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_PreviousProcedureCode))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_Concession))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_Description))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_ZZZ_NKDataGrouping))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_ShipmentType))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_CalculateDuty))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_Group))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_LandedCost))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_IntoWarehouse))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_OutOfWarehouse))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_StartDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_EndDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_IntoTemporaryImport))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_OutOfTemporaryImport))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_IntoTemporaryExport))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_OutOfTemporaryExport))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_IntoInwardProcessing))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_OutOfInwardProcessing))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_IntoOutwardProcessing))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_OutofOutwardProcessing))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_CalculateVAT))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_IsGuaranteeConsumed))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_IsGuaranteeReleased))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_IsTransit))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_IntoVATWarehouse))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_OutOfVATWarehouse))));
			TestHelper.AssertKeySets(nameof(RefCusProcedure), entityConfig.GetKeySets(),
				nameof(RefCusProcedure.ZZ6_Category),
				nameof(RefCusProcedure.ZZ6_ProcedureCode),
				nameof(RefCusProcedure.ZZ6_PreviousProcedureCode),
				nameof(RefCusProcedure.ZZ6_Concession),
				nameof(RefCusProcedure.ZZ6_ZZZ_NKDataGrouping));

			refType = typeof(RefCusProcedureLanguage);
			entityConfig = config.GetConfiguration(refType);
			Assert.IsNotNull(entityConfig, "RefCusProcedureLanguage Configuration is missing");
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedureLanguage.ZXV_ZX6_NKLanguage))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedureLanguage.ZXV_Description))));
			TestHelper.AssertKeySets(nameof(RefCusProcedureLanguage), entityConfig.GetKeySets(),
				nameof(RefCusProcedureLanguage.ZXV_ZX6_NKLanguage));

			refType = typeof(RefCusProcedureAttribute);
			entityConfig = config.GetConfiguration(refType);
			Assert.IsNotNull(entityConfig, "RefCusProcedureAttribute Configuration is missing");
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedureAttribute.ZXB_Name))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedureAttribute.ZXB_Value))));
			TestHelper.AssertKeySets(nameof(RefCusProcedureAttribute), entityConfig.GetKeySets(),
				nameof(RefCusProcedureAttribute.ZXB_Name));
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(tempFolder);
			TestHelper.SimulateDownload(DataFileName, "CargoWise.RefDbRepo.TRReferenceData.Tests.RefCusProcedureCodes.TestFiles.Input.TR - RefCusProcedureCodes.xlsx");
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

		string DataFileName => Path.Combine(tempFolder, "TR - RefCusProcedureCodes.xlsx");

		IReferenceDataParser RefCusProcedureCodesParser => fRefCusProcedureCodesParser ?? (fRefCusProcedureCodesParser = new RefCusProcedureCodesParser(DataFileName));
		RefCusProcedureCodesParser fRefCusProcedureCodesParser;
	}
}
