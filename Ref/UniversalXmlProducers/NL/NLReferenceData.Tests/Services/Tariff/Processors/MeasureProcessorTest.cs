using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.NLReferenceData.Business;
using CargoWise.RefDbRepo.NLReferenceData.Business.Testing;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NLReferenceData.Services.Testing
{
	[TestFixture]
	sealed class MeasureProcessorTests
	{
		[Test]
		public void ConvertXmlElementToModel()
		{
			var processor = new MeasureProcessor();
			var element = TestHelper.GetXmlElement("measure", "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.UT_Measure_001.xml");
			var model = processor.ConvertXElementToModel(element);

			Assert.That(model.AdditionalCodeId, Is.EqualTo("341"), "AdditionalCodeId");
			Assert.That(model.AdditionalCodeType, Is.EqualTo("V"), "AdditionalCodeType");
			Assert.That(model.ChangeType, Is.EqualTo("U"), "ChangeType");
			Assert.That(model.Components, Is.Not.Null.And.Empty, "Components");
			Assert.That(model.DateStart, Is.EqualTo(new DateTime(2022, 01, 01)), "DateStart");
			Assert.That(model.DateEnd, Is.EqualTo(new DateTime(2100, 01, 01)), "DateEnd");
			Assert.That(model.GeographicalAreaId, Is.EqualTo("1011"), "GeographicalAreaId");
			Assert.That(model.GoodsNomenclatureCode, Is.EqualTo("0101000000"), "GoodsNomenclatureCode");
			Assert.That(model.MeasureType, Is.EqualTo("NLACC"), "MeasureType");
			Assert.That(model.National, Is.EqualTo("1"), "National");
			Assert.That(model.RegulationId, Is.EqualTo("1NLWETAC"), "RegulationId");
			Assert.That(model.RegulationRoleType, Is.EqualTo("1"), "RegulationRoleType");
			Assert.That(model.SID, Is.EqualTo("-40993"), "SID");
			Assert.That(model.SIDAdditionalCode, Is.EqualTo("-10043"), "SIDAdditionalCode");
			Assert.That(model.SIDGeographicalArea, Is.EqualTo("400"), "SIDGeographicalArea");
			Assert.That(model.SIDGoodsNomenclature, Is.EqualTo("27624"), "SIDGoodsNomenclature");
			Assert.That(model.StoppedFlag, Is.EqualTo("0"), "StoppedFlag");
			Assert.That(model.Expression, Is.EqualTo("$BASE1=?063; $RATE1=AMOUNT(917.31,\"EUR\")/1000.00;  [$BASE1,$RATE1,ED,063] "), "Expression");

			element = TestHelper.GetXmlElement("measure", "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.UT_Measure_001.xml", 1);
			model = processor.ConvertXElementToModel(element);

			Assert.That(model.Components, Is.Not.Null, "Components (element 2)");
			Assert.That(model.Components.Count, Is.EqualTo(1), "Components (element 2) Count");

			//Check Components
			var comList = model.Components.ToList();
			Assert.That(comList[0].DutyAmount, Is.EqualTo(38.86d), "DutyAmount");
			Assert.That(comList[0].DutyExpressionId, Is.EqualTo("01"), "DutyExpressionId");
			Assert.That(comList[0].MeasurementUnitCode, Is.EqualTo("TNE"), "MeasurementUnitCode");
			Assert.That(comList[0].MonetaryUnitCode, Is.EqualTo("EUR"), "MonetaryUnitCode");
			Assert.That(comList[0].National, Is.EqualTo("1"), "National");
		}


		[Test]
		public void LoadData()
		{
			var filePath = Path.Combine(ContentFolder, "UT_Measure_001.xml");
			TestHelper.SimulateDownload(filePath, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.UT_Measure_001.xml");
			var processor = new MeasureProcessor();
			processor.LoadData(filePath);

			Assert.That(processor.VATModels.Count, Is.EqualTo(1), "All VAT elements from XML loaded");
			Assert.That(processor.DutiesModels.Count, Is.EqualTo(4), "All Duties elements from XML loaded");
		}

		[Test]
		public void UpdateModels()
		{
			var filePath = Path.Combine(ContentFolder, "UT_Measure_001.xml");
			TestHelper.SimulateDownload(filePath, "CargoWise.RefDbRepo.NLReferenceData.Tests.TestFiles.Tariff.Input.UT_Measure_001.xml");
			var processor = new MeasureProcessor();
			processor.LoadData(filePath);
			processor.UpdateModels();

			var model1 = processor.DutiesModels[0];
			var model3 = processor.DutiesModels[1];
			var model4 = processor.VATModels[0];
			var model6 = processor.DutiesModels[2];
			var model7 = processor.DutiesModels[3];

			Assert.That(model3.UnitOfMeasure, Is.Not.Null, "Unit Of Measure (element 3)");
			Assert.That(model3.UnitOfMeasure.Count, Is.EqualTo(2), "Unit Of Measure (element 3) Count");

			//Check Unit Of Measure
			var unitOfMeasureList = model3.UnitOfMeasure.ToList();
			Assert.That(unitOfMeasureList[0], Is.EqualTo("003"), "First base");
			Assert.That(unitOfMeasureList[1], Is.EqualTo("MIL"), "Second an third base");

			Assert.That(model1.CleanId, Is.EqualTo("0101"), "Model 1 CleanID");
			Assert.That(model3.CleanId, Is.EqualTo("240220"), "Model 3 CleanID");
			Assert.That(model4.CleanId, Is.EqualTo("010121"), "Model 4 CleanID");
			Assert.That(model6.CleanId, Is.EqualTo("2009"), "Model 6 CleanID");
			Assert.That(model7.CleanId, Is.EqualTo("2701"), "Model 7 CleanID");

			Assert.That(model1.Formula, Is.EqualTo("917.31 * [063] / 1000.00"), "Model 1 Formula");
			Assert.That(model3.Formula, Is.EqualTo("MAX([003] * 223.82 / 100 + [MIL] * 223.82, [MIL] * 243.25)"), "Model 3 Formula");
			Assert.That(model6.Formula, Is.EqualTo("8.83 * [LTR] / 100.00"), "Model 6 Formula");
			Assert.That(model7.Formula, Is.EqualTo("15.49 * [TNE]"), "Model 7 Formula");

			Assert.That(model1.AdditionalCode, Is.EqualTo("V341"), "Model 1 Additional Code");
			Assert.That(model3.AdditionalCode, Is.EqualTo("U375"), "Model 3 Additional Code");
			Assert.That(model4.AdditionalCode, Is.EqualTo("Q200"), "Model 4 Additional Code");
			Assert.That(model6.AdditionalCode, Is.EqualTo("U283"), "Model 6 Additional Code");
			Assert.That(model7.AdditionalCode, Is.EqualTo(""), "Model 7 Additional Code");

			Assert.That(model4.TaxOrFeeCode, Is.EqualTo("LOW"), "Model 4 Tax or fee Code");

			Assert.That(model1.RateCode, Is.EqualTo("030"), "Model 1 RateCode");
			Assert.That(model3.RateCode, Is.EqualTo("032"), "Model 3 RateCode");
			Assert.That(model6.RateCode, Is.EqualTo("028"), "Model 6 RateCode");

			Assert.That(model1.RateType, Is.EqualTo("MSC"), "Model 1 RateType");
			Assert.That(model3.RateType, Is.EqualTo("EXC"), "Model 3 RateType");
			Assert.That(model6.RateType, Is.EqualTo("MSC"), "Model 6 RateType");
		}

		[Test]
		public void GetTaxOrFeeCode()
		{
			var model = new Measure();
			var compList = new List<MeasureComponent>();
			var component = new MeasureComponent();
			var processor = new MeasureProcessor();
			var vatModels = new List<Measure>();

			compList.Add(component);
			model.Components = compList;
			vatModels.Add(model);
			processor.VATModels = vatModels;

			component.DutyAmount = 0m;
			processor.UpdateModels();
			Assert.That(model.TaxOrFeeCode, Is.EqualTo("ZER"), "TaxOrFeeCode for 0% vat");

			component.DutyAmount = 9m;
			processor.UpdateModels();
			Assert.That(model.TaxOrFeeCode, Is.EqualTo("LOW"), "TaxOrFeeCode for 9% vat");

			component.DutyAmount = 21m;
			processor.UpdateModels();
			Assert.That(model.TaxOrFeeCode, Is.EqualTo("STD"), "TaxOrFeeCode for 21% vat");

			component.DutyAmount = 6m;
			processor.UpdateModels();
			Assert.That(model.TaxOrFeeCode, Is.EqualTo(""), "TaxOrFeeCode for invalid vat percentage");
		}

		[Test]
		public void CleanCommodityCode()
		{
			var model = new Measure();
			var processor = new MeasureProcessor();
			var vatModels = new List<Measure>
			{
				model
			};
			processor.VATModels = vatModels;

			model.GoodsNomenclatureCode = "0101000000";
			processor.UpdateModels();
			Assert.That(model.CleanId, Is.EqualTo("0101"), "GoodsNomenclatureCode 0101000000");

			model.GoodsNomenclatureCode = "0101210000";
			processor.UpdateModels();
			Assert.That(model.CleanId, Is.EqualTo("010121"), "GoodsNomenclatureCode 0101210000");

			model.GoodsNomenclatureCode = "0209900000";
			processor.UpdateModels();
			Assert.That(model.CleanId, Is.EqualTo("020990"), "GoodsNomenclatureCode 0209900000");

			model.GoodsNomenclatureCode = "0210000000";
			processor.UpdateModels();
			Assert.That(model.CleanId, Is.EqualTo("0210"), "GoodsNomenclatureCode 0210000000");

			model.GoodsNomenclatureCode = "0301991700";
			processor.UpdateModels();
			Assert.That(model.CleanId, Is.EqualTo("03019917"), "GoodsNomenclatureCode 0301991700");

			model.GoodsNomenclatureCode = "0301998510";
			processor.UpdateModels();
			Assert.That(model.CleanId, Is.EqualTo("0301998510"), "GoodsNomenclatureCode 0301998510");

			model.GoodsNomenclatureCode = "0301998522";
			processor.UpdateModels();
			Assert.That(model.CleanId, Is.EqualTo("0301998522"), "GoodsNomenclatureCode 0301998522");
		}

		[Test]
		public void GenerateFormula()
		{
			var model = new Measure();
			var compList = new List<MeasureComponent>();
			var component = new MeasureComponent();
			var processor = new MeasureProcessor();
			var dutiesModels = new List<Measure>();

			compList.Add(component);
			model.Components = compList;
			dutiesModels.Add(model);
			processor.DutiesModels = dutiesModels;

			component.DutyAmount = 23.98m;
			component.MeasurementUnitCode = "ABC";
			processor.UpdateModels();
			Assert.That(model.Formula, Is.EqualTo("23.98 * [ABC]"), "Formula coming out of component");

			model.Components = new List<MeasureComponent>();
			model.Expression = "$Rate=(?ASV*AMOUNT(16.86,&#34;EUR&#34;))/100.00; $Base=?060; [$Base, $Rate, AA,060] ";
			processor.UpdateModels();
			Assert.That(model.Formula, Is.EqualTo("16.86 * [060] / 100.00"), "Formula format 1");

			model.Expression = "$BASE1=?063; $RATE1=AMOUNT(8.00,&#34;EUR&#34;)/1000.00; [$BASE1,$RATE1,ED,063]";
			processor.UpdateModels();
			Assert.That(model.Formula, Is.EqualTo("8.00 * [063] / 1000.00"), "Formula format 2");

			model.Expression = "$BASE1=?003;  $RATE1=5.0;  $BASE2=?MIL;  $RATE2=AMOUNT(223.82,&#34;EUR&#34;);  $BASE3=?MIL;  $RATE3=AMOUNT(243.25,&#34;EUR&#34;);  AMAX([$BASE1, $RATE1 %, KP,003] [$BASE2, $RATE2, EM,MIL] ,  [$BASE3, $RATE3, EM,MIL]) ";
			processor.UpdateModels();
			Assert.That(model.Formula, Is.EqualTo("MAX([003] * 223.82 / 100 + [MIL] * 223.82, [MIL] * 243.25)"), "Formula format 3");

			model.Expression = "$BASE1=?003;  $RATE1=9.0;  [$BASE1,$RATE1 %,KP,003] ";
			processor.UpdateModels();
			Assert.That(model.Formula, Is.EqualTo("9.0 * [003] / 100"), "Formula format 4");

			model.Expression = "$Base=?LTR; $Rate= (AMOUNT(8.83, &#34;EUR&#34;))/100.00; [$Base, $Rate, VB]";
			processor.UpdateModels();
			Assert.That(model.Formula, Is.EqualTo("8.83 * [LTR] / 100.00"), "Formula format 5");

			model.Expression = "$Base=?LTR; $Rate= (AMOUNT(8.83, &#34;EUR&#34;))/100.00; [$Base, $Rate, VB]";
			processor.UpdateModels();
			Assert.That(model.Formula, Is.EqualTo("8.83 * [LTR] / 100.00"), "Formula format 6");
		}

		[Test]
		public void FindRateCodeAndTypeNLACC()
		{
			var model = new Measure();
			var processor = new MeasureProcessor();
			var dutiesModels = new List<Measure>
			{
				model
			};
			processor.DutiesModels = dutiesModels;
			model.MeasureType = "NLACC";

			model.AdditionalCodeId = "X01";
			processor.UpdateModels();
			Assert.That(model.RateCode, Is.EqualTo("035"), "Rate code linked to additionalCodeId 01");
			Assert.That(model.RateType, Is.EqualTo("EXC"), "Rate type linked to additionalCodeId 01");

			model.AdditionalCodeId = "X02";
			processor.UpdateModels();
			Assert.That(model.RateCode, Is.EqualTo("035"), "Rate code linked to additionalCodeId 02");
			Assert.That(model.RateType, Is.EqualTo("EXC"), "Rate type linked to additionalCodeId 02");

			model.AdditionalCodeId = "X06";
			processor.UpdateModels();
			Assert.That(model.RateCode, Is.EqualTo("035"), "Rate code linked to additionalCodeId 06");
			Assert.That(model.RateType, Is.EqualTo("EXC"), "Rate type linked to additionalCodeId 06");

			model.AdditionalCodeId = "X07";
			processor.UpdateModels();
			Assert.That(model.RateCode, Is.EqualTo("035"), "Rate code linked to additionalCodeId 07");
			Assert.That(model.RateType, Is.EqualTo("EXC"), "Rate type linked to additionalCodeId 07");

			model.AdditionalCodeId = "X09";
			processor.UpdateModels();
			Assert.That(model.RateCode, Is.EqualTo("035"), "Rate code linked to additionalCodeId 09");
			Assert.That(model.RateType, Is.EqualTo("EXC"), "Rate type linked to additionalCodeId 09");

			model.AdditionalCodeId = "X11";
			processor.UpdateModels();
			Assert.That(model.RateCode, Is.EqualTo("035"), "Rate code linked to additionalCodeId 11");
			Assert.That(model.RateType, Is.EqualTo("EXC"), "Rate type linked to additionalCodeId 11");

			model.AdditionalCodeId = "X13";
			processor.UpdateModels();
			Assert.That(model.RateCode, Is.EqualTo("035"), "Rate code linked to additionalCodeId 13");
			Assert.That(model.RateType, Is.EqualTo("EXC"), "Rate type linked to additionalCodeId 13");

			model.AdditionalCodeId = "X19";
			processor.UpdateModels();
			Assert.That(model.RateCode, Is.EqualTo("035"), "Rate code linked to additionalCodeId 19");
			Assert.That(model.RateType, Is.EqualTo("EXC"), "Rate type linked to additionalCodeId 19");

			model.AdditionalCodeId = "X21";
			processor.UpdateModels();
			Assert.That(model.RateCode, Is.EqualTo("036"), "Rate code linked to additionalCodeId 21");
			Assert.That(model.RateType, Is.EqualTo("EXC"), "Rate type linked to additionalCodeId 21");

			model.AdditionalCodeId = "X23";
			processor.UpdateModels();
			Assert.That(model.RateCode, Is.EqualTo("036"), "Rate code linked to additionalCodeId 23");
			Assert.That(model.RateType, Is.EqualTo("EXC"), "Rate type linked to additionalCodeId 23");

			model.AdditionalCodeId = "X36";
			processor.UpdateModels();
			Assert.That(model.RateCode, Is.EqualTo("065"), "Rate code linked to additionalCodeId 36");
			Assert.That(model.RateType, Is.EqualTo("EXC"), "Rate type linked to additionalCodeId 36");

			model.AdditionalCodeId = "X38";
			processor.UpdateModels();
			Assert.That(model.RateCode, Is.EqualTo("065"), "Rate code linked to additionalCodeId 38");
			Assert.That(model.RateType, Is.EqualTo("EXC"), "Rate type linked to additionalCodeId 38");

			model.AdditionalCodeId = "X40";
			processor.UpdateModels();
			Assert.That(model.RateCode, Is.EqualTo("066"), "Rate code linked to additionalCodeId 40");
			Assert.That(model.RateType, Is.EqualTo("EXC"), "Rate type linked to additionalCodeId 40");

			model.AdditionalCodeId = "X41";
			processor.UpdateModels();
			Assert.That(model.RateCode, Is.EqualTo("030"), "Rate code linked to additionalCodeId 41");
			Assert.That(model.RateType, Is.EqualTo("MSC"), "Rate type linked to additionalCodeId 41");

			model.AdditionalCodeId = "X42";
			processor.UpdateModels();
			Assert.That(model.RateCode, Is.EqualTo("030"), "Rate code linked to additionalCodeId 42");
			Assert.That(model.RateType, Is.EqualTo("MSC"), "Rate type linked to additionalCodeId 42");

			model.AdditionalCodeId = "X44";
			processor.UpdateModels();
			Assert.That(model.RateCode, Is.EqualTo("030"), "Rate code linked to additionalCodeId 44");
			Assert.That(model.RateType, Is.EqualTo("MSC"), "Rate type linked to additionalCodeId 44");

			model.AdditionalCodeId = "X46";
			processor.UpdateModels();
			Assert.That(model.RateCode, Is.EqualTo("030"), "Rate code linked to additionalCodeId 46");
			Assert.That(model.RateType, Is.EqualTo("MSC"), "Rate type linked to additionalCodeId 46");

			model.AdditionalCodeId = "X66";
			processor.UpdateModels();
			Assert.That(model.RateCode, Is.EqualTo("030"), "Rate code linked to additionalCodeId 66");
			Assert.That(model.RateType, Is.EqualTo("MSC"), "Rate type linked to additionalCodeId 66");

			model.AdditionalCodeId = "X69";
			processor.UpdateModels();
			Assert.That(model.RateCode, Is.EqualTo("030"), "Rate code linked to additionalCodeId 69");
			Assert.That(model.RateType, Is.EqualTo("MSC"), "Rate type linked to additionalCodeId 69");

			model.AdditionalCodeId = "X70";
			processor.UpdateModels();
			Assert.That(model.RateCode, Is.EqualTo("032"), "Rate code linked to additionalCodeId 70");
			Assert.That(model.RateType, Is.EqualTo("EXC"), "Rate type linked to additionalCodeId 70");

			model.AdditionalCodeId = "X75";
			processor.UpdateModels();
			Assert.That(model.RateCode, Is.EqualTo("032"), "Rate code linked to additionalCodeId 75");
			Assert.That(model.RateType, Is.EqualTo("EXC"), "Rate type linked to additionalCodeId 75");

			model.AdditionalCodeId = "X79";
			processor.UpdateModels();
			Assert.That(model.RateCode, Is.EqualTo("032"), "Rate code linked to additionalCodeId 79");
			Assert.That(model.RateType, Is.EqualTo("EXC"), "Rate type linked to additionalCodeId 79");
		}

		[Test]
		public void FindRateCodeAndTypeNLACVH()
		{
			var model = new Measure();
			var processor = new MeasureProcessor();
			var dutiesModels = new List<Measure>
			{
				model
			};
			processor.DutiesModels = dutiesModels;
			model.MeasureType = "NLACVH";

			model.AdditionalCodeId = "X41";
			processor.UpdateModels();
			Assert.That(model.RateCode, Is.EqualTo("080"), "Rate code linked to additionalCodeId 41");
			Assert.That(model.RateType, Is.EqualTo("LVY"), "Rate type linked to additionalCodeId 41");

			model.AdditionalCodeId = "X42";
			processor.UpdateModels();
			Assert.That(model.RateCode, Is.EqualTo("081"), "Rate code linked to additionalCodeId 42");
			Assert.That(model.RateType, Is.EqualTo("LVY"), "Rate type linked to additionalCodeId 42");

			model.AdditionalCodeId = "X44";
			processor.UpdateModels();
			Assert.That(model.RateCode, Is.EqualTo("082"), "Rate code linked to additionalCodeId 44");
			Assert.That(model.RateType, Is.EqualTo("LVY"), "Rate type linked to additionalCodeId 44");

			model.AdditionalCodeId = "X46";
			processor.UpdateModels();
			Assert.That(model.RateCode, Is.EqualTo("083"), "Rate code linked to additionalCodeId 46");
			Assert.That(model.RateType, Is.EqualTo("LVY"), "Rate type linked to additionalCodeId 46");

			model.AdditionalCodeId = "X69";
			processor.UpdateModels();
			Assert.That(model.RateCode, Is.EqualTo("084"), "Rate code linked to additionalCodeId 69");
			Assert.That(model.RateType, Is.EqualTo("LVY"), "Rate type linked to additionalCodeId 69");
		}

		[Test]
		public void FindRateCodeAndTypeNLVBB()
		{
			var model = new Measure();
			var processor = new MeasureProcessor();
			var dutiesModels = new List<Measure>
			{
				model
			};
			processor.DutiesModels = dutiesModels;
			model.MeasureType = "NLVBB";

			model.AdditionalCodeId = "X80";
			processor.UpdateModels();
			Assert.That(model.RateCode, Is.EqualTo("028"), "Rate code linked to additionalCodeId 80");
			Assert.That(model.RateType, Is.EqualTo("MSC"), "Rate type linked to additionalCodeId 80");

			model.AdditionalCodeId = "X81";
			processor.UpdateModels();
			Assert.That(model.RateCode, Is.EqualTo("028"), "Rate code linked to additionalCodeId 81");
			Assert.That(model.RateType, Is.EqualTo("MSC"), "Rate type linked to additionalCodeId 81");

			model.AdditionalCodeId = "X82";
			processor.UpdateModels();
			Assert.That(model.RateCode, Is.EqualTo("028"), "Rate code linked to additionalCodeId 82");
			Assert.That(model.RateType, Is.EqualTo("MSC"), "Rate type linked to additionalCodeId 82");

			model.AdditionalCodeId = "X83";
			processor.UpdateModels();
			Assert.That(model.RateCode, Is.EqualTo("028"), "Rate code linked to additionalCodeId 83");
			Assert.That(model.RateType, Is.EqualTo("MSC"), "Rate type linked to additionalCodeId 83");

			model.AdditionalCodeId = "X85";
			processor.UpdateModels();
			Assert.That(model.RateCode, Is.EqualTo("028"), "Rate code linked to additionalCodeId 85");
			Assert.That(model.RateType, Is.EqualTo("MSC"), "Rate type linked to additionalCodeId 85");

			model.AdditionalCodeId = "X89";
			processor.UpdateModels();
			Assert.That(model.RateCode, Is.EqualTo("028"), "Rate code linked to additionalCodeId 89");
			Assert.That(model.RateType, Is.EqualTo("MSC"), "Rate type linked to additionalCodeId 89");
		}

		[Test]
		public void FindRateCodeAndTypeNLKOBE()
		{
			var model = new Measure();
			var processor = new MeasureProcessor();
			var dutiesModels = new List<Measure>
			{
				model
			};
			processor.DutiesModels = dutiesModels;
			model.MeasureType = "NLKOBE";
			model.AdditionalCodeId = "XXX";

			processor.UpdateModels();
			Assert.That(model.RateCode, Is.EqualTo("050"), "Rate code linked to MeasureTYpe NLKOBE");
			Assert.That(model.RateType, Is.EqualTo("MSC"), "Rate type linked to MeasureTYpe NLKOBE");
		}

		#region Setup
		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(TempFolder);
		}

		[SetUp]
		public void Setup()
		{
			OutputFolder = Path.Combine(TempFolder, Path.GetRandomFileName());
			Directory.CreateDirectory(OutputFolder);
			ContentFolder = Path.Combine(TempFolder, Path.GetRandomFileName());
			Directory.CreateDirectory(ContentFolder);
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		string OutputFolder;
		string ContentFolder;
		string TempFolder;
		#endregion
	}
}
