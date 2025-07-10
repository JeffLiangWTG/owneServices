using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Helpers.Tests;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using CargoWise.RefDbRepo.SharedReferenceData.Tests;
using NUnit.Framework;
using static CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Helpers.Tests.TestHelperClasses;
using static CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors.MeasureHelper;
using static CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Helpers.Tests.TestHelperClasses;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors.Tests
{
	[TestFixture]
	class MeasureHelperTests
	{
		[TestCase("00", "No Components or Conditions", "")]
		//Components
		[TestCase("01", "MCOM-DE 01 - No Units", "VFD * 0.065")]
		[TestCase("02", "MCOM-DE 01 - Zero value", "0")]
		[TestCase("03", "MCOM-DE 01 - With UOM", "123.560 * [TNE]")]
		[TestCase("04", "MCOM-DE 04 - Std", "VFD * 0.096 + 125.800 * [DTN]")]
		[TestCase("05", "MCOM-DE 04 - No 01 value", "0 + 125.800 * [DTN]")]
		[TestCase("06", "MCOM-DE 04 - No 04 value", "VFD * 0.096 + 0.000 * [DTN]")]
		[TestCase("07", "MCOM-DE 04 - UnitQualifer", "VFD * 0.140 + 44.362 * [DTNM]")]
		[TestCase("08", "MCOM-DE 12", "VFD * 0.076 + #EA(1)#")]
		[TestCase("09", "MCOM-DE 14", "0 + #EAR(1)#")]
		[TestCase("10", "MCOM-DE 15 - MIN", "MAX(VFD * 0.038, 0.600 * [DTNG])")]
		[TestCase("11", "MCOM-DE 17 - MAX", "MIN(VFD * 0.149, 24.000 * [DTN])")]
		[TestCase("12", "MCOM-DE 19", "VFD * 0.224 + 20.600 * [DTN]")]
		[TestCase("13", "MCOM-DE 21 - Additional duty on sugar", "VFD * 0.083 + #ADSZ(1)#")]
		[TestCase("14", "MCOM-DE 25 - Reduced Additional duty on sugar", "0 + #ADSZR(1)#")]
		[TestCase("15", "MCOM-DE 27 - Additional duty on flour", "0 + #ADFM(1)#")]
		[TestCase("16", "MCOM-DE 29 - Reduced Additional duty on flour", "0 + #ADFMR(1)#")]
		[TestCase("17", "MCOM-DE 35 - MAX", "MIN(VFD * 0.045, 35.150 * [DTN])")]
		// Active DutyExpressions not being used by MeasureComponent
		[TestCase("18", "MCOM-DE 02 - minus % or amount", "VFD * 0.096 - 125.800 * [DTN]")]
		[TestCase("19", "MCOM-DE 20 - + % or amount", "VFD * 0.096 + 125.800 * [DTN]")]
		//Combinations
		[TestCase("20", "MCOM-DE 17,15,01 Code:2401108510", "MIN(24.000 * [DTN], MAX(VFD * 0.184, 22.000 * [DTN]))")]
		[TestCase("21", "MCOM-DE 04,01,19,17,01,01 Code:1702500000", "MIN(VFD * 0.140 + 44.362 * [DTNM], VFD * 0.125 + 50.700 * [DTNM])")]
		[TestCase("22", "MCOM-DE 01,04,17 Code:3809103000", "MIN(VFD * 0.128, VFD * 0.083 + 12.400 * [DTN])")]
		[TestCase("23", "MCOM-DE 01,25,17,14 Code:1704905100", "MIN(VFD * 0.075 + #EAR(1)#, VFD * 0.155 + #ADSZR(1)#)")]
		[TestCase("24", "MCOM-DE 01,14,35,25,17 Code:1905313000", "MIN(VFD * 0.121 + #ADSZR(1)#, MIN(35.150 * [DTN], VFD * 0.045 + #EAR(1)#))")]
		//Conditions
		[TestCase("25", "CON-AC-01,11 CC-A,F Code: 7225110015", "If(VFD/[TNE] >= 1873.000, 0.000 * [TNE], If(VFD/[TNE] >= 1540.300, (1873.000 - VFD/[TNE]) * [TNE], VFD * 0.216 + 0 * [TNE]))")]
		[TestCase("26", "CON-AC-01,11 CC-F Code: 8305100023", "If(VFD/[MIL] >= 325.000, 0.000 * [MIL], (325.000 - VFD/[MIL]) * [MIL])")]
		[TestCase("27", "CON-AC-01 CC-V Code: 2204309600", "If(VFD/[HLT] >= 212.400, 0 + 20.600 * [DTN] + 0 * [HLT], If(VFD/[HLT] >= 208.200, 0 + 4.200 * [HLT] + 20.600 * [DTN], If(VFD/[HLT] >= 203.900, 0 + 8.500 * [HLT] + 20.600 * [DTN], If(VFD/[HLT] >= 199.700, 0 + 12.700 * [HLT] + 20.600 * [DTN], If(VFD/[HLT] >= 195.400, 0 + 17.000 * [HLT] + 20.600 * [DTN], 0 + 121.000 * [HLT] + 20.600 * [DTN])))))")]
		[TestCase("28", "CON-AC-01,11 CC-M Code: 7226110016", "If(VFD/[TNE] >= 1536.000, 0.000 * [TNE], If(VFD/[TNE] >= 1130.250, (1536.000 - VFD/[TNE]) * [TNE], VFD * 0.359 + 0 * [TNE]))")]
		[TestCase("29", "CON-AC-01 CC-L CIF Code:1602321100 BR", "If(CIF/[DTN] >= 286.740, 0.000 * [DTN], If(CIF/[DTN] >= 191.160, 86.022 * [DTN] - CIF * 0.300, If(CIF/[DTN] >= 127.440, 124.254 * [DTN] - CIF * 0.500, If(CIF/[DTN] >= 79.650, 149.742 * [DTN] - CIF * 0.700, 165.672 * [DTN] - CIF * 0.900))))")]
		[TestCase("30", "CON-AC-01 CC-A HasCert Code:7208370000 CN AddCode 160", @"If(has(""CERT"", ""D008""), VFD * 0.103, 0)")]
		[TestCase("31", "CON-AC-01 CC-A HasCert Code:7208370000 CN V2", @"If(has(""CERT"", ""D008""), VFD * 0.103, VFD * 0.050)")]
		[TestCase("32", "CON-AC-01 CC-C Code:1006302700 KH", @"If(has(""CERT"", ""Y223""), 0 * [TNE], 150.000 * [TNE])")]
		[TestCase("34", "CON-AC-10 CC-R CClass-Class", "[KGM]/[SUPU] < 168125.001 & [KGM]/[SUPU] >= 167875.000")]
		[TestCase("35", "Class-Class No Components or Conditions", "")]
		[TestCase("36", "Class-CTRL No Components or Conditions", "")]
		[TestCase("37", "More than 3 decimals", "VFD * 0.250 + VFD * 0.00012")]
		[TestCase("38", "ConditionCode A 1516209821 AR", @"If(has(""CERT"", ""D017""), 0, If(has(""CERT"", ""D018""), VFD * 0.334, VFD * 0.334))")]
		[TestCase("39", "Beer - Multi-conditions-2203000100", "If([GP1] <= 5000.000, 9.540 * [ASVX] + 0 * [GP1] + 0 * [FC1X], If([GP1] <= 30000.000, 19.080 * [ASVX] - 47700.000 * [FC1X] + 0 * [GP1], If([GP1] <= 60000.000, 20.669 * [ASVX] - 95380.920 * [FC1X] + 0 * [GP1], If([GP1] <= 600000.000, 19.080 * [ASVX] + 0 * [GP1] + 0 * [FC1X], 19.080 * [ASVX] + 0 * [GP1] + 0 * [FC1X]))))")]
		[TestCase("40", "Beer (fixed 2023 rules) 2203000100", "If([ASV] <= 3.490, 0 * [ASV] + 0 * [ASVX], If([ASV] <= 8.490, 21.010 * [ASVX] + 0 * [ASV], 0 * [ASV] + 0 * [ASVX]))")]
		public void RateFormula(string skipStr, string description, string expected)
		{
			var errorCollector = new StringBuilder();
			var mappingProvider = new MeasureMappingTestDataProvider();

			mappingProvider.MeasureTypeMappingsForTest = new Dictionary<string, MeasureTypeMapping>
			{
				{ "482", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Class, RateType = string.Empty } }
			};

			var processor = new MeasureProcessorTester(mappingProvider);

			int skip = int.Parse(skipStr, CultureInfo.CurrentCulture);
			var xElement = TestHelper.GetXmlElement("Measure", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Business.Tariff.TestFiles.Input.UT_RateFormula.xml", skip);
			var measure = loader.ConvertXElementToModel(xElement);

			processor.Models = new List<ITariffModel>() { measure };
			processor.UpdateModels(string.Empty, TestHelperClasses.TestData.CreateReferenceData(), errorCollector);

			var actual = measureHelper.GenerateFormula(measure, errorCollector);

			Assert.That(actual, Is.EqualTo(expected), description);
		}

		[TestCase("00", "Mixed Cert and Formula", new string[] { "", "", "", "[KGM] <= 20.000", "" })]
		[TestCase("01", "CClass-Rate - No formulas", new string[] { "", "" })]
		[TestCase("02", "CClass-Class - No formulas", new string[] { "", "" })]
		public void ConditionFormula(string skipStr, string description, string[] expectedConditionFormulas)
		{
			var errorCollector = new StringBuilder();
			var mappingProvider = new MeasureMappingTestDataProvider();

			mappingProvider.MeasureTypeMappingsForTest = new Dictionary<string, MeasureTypeMapping>
			{
				{ "755", new MeasureTypeMapping { ConditionClass = MeasureHelper.ConditionClass.Control, RateType = string.Empty } }
			};
			var processor = new MeasureProcessorTester(mappingProvider);

			int skip = int.Parse(skipStr, CultureInfo.CurrentCulture);
			var xElement = TestHelper.GetXmlElement("Measure", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Business.Tariff.TestFiles.Input.UT_ConditionFormula.xml", skip);
			var measure = loader.ConvertXElementToModel(xElement);

			processor.Models = new List<ITariffModel>() { measure };
			processor.UpdateModels(string.Empty, TestHelperClasses.TestData.CreateReferenceData(), errorCollector);

			Assert.That(measure.Conditions.Count, Is.EqualTo(expectedConditionFormulas.Length));
			var conList = measure.Conditions.ToList();
			for (int i = 0; i < expectedConditionFormulas.Length; i++)
			{
				Assert.That(conList[i].Formula, Is.EqualTo(expectedConditionFormulas[i]), $"{description} Condition:{(i + 1)}");
			}
		}

		[TestCase(0, "{InvalidSourceData:Condition for measure HJID=942664 has no else part: If(has(\"CERT\", \"Y223\"), VFD * 1.000, ELSEPART)}", "If(has(\"CERT\", \"Y223\"), VFD * 1.000, 0)")]
		[TestCase(1, "{InvalidSourceData:Condition for measure HJID=942665 has no else part: If(VFD/[TNE] >= 1873.000, 0.120 * [TNE], ELSEPART)}", "If(VFD/[TNE] >= 1873.000, 0.120 * [TNE], 0 * [TNE])")]
		public void ConditionWithNoElse(int skip, string expectedError, string resultFormula)
		{
			var errorCollector = new StringBuilder();
			var processor = new MeasureProcessorTester(new MeasureMappingTestDataProvider());
			var xElement = TestHelper.GetXmlElement("Measure", "CargoWise.RefDbRepo.SharedReferenceData.Tests.Business.Tariff.TestFiles.Input.UT_ConditionWithNoElse.xml", skip);
			var measure = loader.ConvertXElementToModel(xElement);
			processor.Models = new List<ITariffModel>() { measure };
			processor.UpdateModels(string.Empty, TestData.CreateReferenceData(), errorCollector);
			Assert.That(measure.Conditions.Count, Is.EqualTo(1), "Conditions.Count");
			Assert.That(measure.Formula, Does.Not.Contain("ELSEPART"), "Formula");
			Assert.That(errorCollector.ToString(), Does.Contain(expectedError));
			Assert.That(measure.Formula, Is.EqualTo(resultFormula), "Formula");
		}

		[Test]
		public void GetTariffType()
		{
			var result = MeasureHelper.GetTariffTypes("XXX").ToList();
			Assert.That(result, Is.Not.Null.And.Empty);

			result = MeasureHelper.GetTariffTypes("0").ToList();
			Assert.That(result.Count, Is.EqualTo(1));
			Assert.That(result[0], Is.EqualTo("IMP"));

			result = MeasureHelper.GetTariffTypes("1").ToList();
			Assert.That(result.Count, Is.EqualTo(1));
			Assert.That(result[0], Is.EqualTo("EXP"));

			result = MeasureHelper.GetTariffTypes("2").ToList();
			Assert.That(result.Count, Is.EqualTo(2));
			Assert.That(result.Contains("IMP"));
			Assert.That(result.Contains("EXP"));
		}

		[TestCase("Default 01", "01", RateCodes.Default)]
		[TestCase("Default 55", "55", RateCodes.Default)]
		[TestCase("AgriculturalComponent", "12", RateCodes.AgriculturalComponent)]
		[TestCase("ReducedAgricultureComponent", "14", RateCodes.ReducedAgricultureComponent)]
		[TestCase("AdditionalDutyOnSugar", "21", RateCodes.AdditionalDutyOnSugar)]
		[TestCase("ReducedAdditionalDutyOnSugar", "25", RateCodes.ReducedAdditionalDutyOnSugar)]
		[TestCase("AdditionalDutyOnFlour", "27", RateCodes.AdditionalDutyOnFlour)]
		[TestCase("ReducedAdditionalDutyOnFlour", "29", RateCodes.ReducedAdditionalDutyOnFlour)]
		public void GetDutyRateCode(string description, string dutyExpression, string expectedRateCode)
		{
			var measure = new Measure {	MeasureTypeSeries = "C", MeasureType = "103" };

			var actual = MeasureHelper.GetDutyRateCode(measure);
			Assert.That(actual, Is.EqualTo(RateCodes.Default), $"No DutyExpresion - {description}");

			measure.SetComponents(new[] { new MeasureComponent { DutyExpression = dutyExpression, HJID = "1" } });

			actual = MeasureHelper.GetDutyRateCode(measure);
			Assert.That(actual, Is.EqualTo(expectedRateCode), $"Component DutyExpresion - {description}");

			measure.SetComponents(new MeasureComponent[] { });
			measure.SetConditions(new[] { new MeasureCondition { HJID = "1" }.SetComponents(new[] { new MeasureComponent { DutyExpression = dutyExpression, HJID = "1" } }) });

			actual = MeasureHelper.GetDutyRateCode(measure);
			Assert.That(actual, Is.EqualTo(expectedRateCode), $"Condition DutyExpresion - {description}");
		}

		[Test]
		public void RateCodeWithConditionDutyExpression()
		{
			var measure = new Measure
			{
				MeasureTypeSeries = "C",
				MeasureType = "103",
			}
			.SetConditions(new[]
			{
				new MeasureCondition { HJID = "1" }
				.SetComponents(new[]
				{
					new MeasureComponent { DutyExpression = "12", HJID = "1" }
				})
			});

			var actual = MeasureHelper.GetDutyRateCode(measure);

			Assert.That(actual, Is.EqualTo("EA"));
		}

		[TestCase("", false)]
		[TestCase("01", false)]
		[TestCase("02", false)]
		[TestCase("03", false)]
		[TestCase("04", true)]
		[TestCase("05", true)]
		[TestCase("06", true)]
		[TestCase("07", true)]
		[TestCase("08", true)]
		[TestCase("09", true)]
		[TestCase("10", true)]
		[TestCase("11", false)]
		[TestCase("12", false)]
		[TestCase("13", false)]
		[TestCase("14", true)]
		[TestCase("15", false)]
		[TestCase("16", true)]
		[TestCase("24", false)]
		[TestCase("25", false)]
		[TestCase("26", false)]
		[TestCase("27", false)]
		[TestCase("28", false)]
		[TestCase("29", false)]
		[TestCase("30", false)]
		[TestCase("34", false)]
		[TestCase("36", false)]
		public void NegativeConditions(string actionCode, bool expected)
		{
			Assert.That(MeasureHelper.IsNegativeAction(actionCode), Is.EqualTo(expected));
		}

		[TestCase("A", true)]
		[TestCase("B", true)]
		[TestCase("C", true)]
		[TestCase("H", true)]
		[TestCase("Q", true)]
		[TestCase("Z", true)]
		[TestCase("D", false)]
		[TestCase("", false)]
		public void IsCertificateCondition(string conditionCode, bool expected)
		{
			Assert.That(MeasureHelper.IsCertificateCondition(conditionCode), Is.EqualTo(expected));
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			measureHelper = new MeasureHelper();
			loader = new MeasureLoaderTester();
		}

		MeasureHelper measureHelper;
		MeasureLoaderTester loader;
	}
}
