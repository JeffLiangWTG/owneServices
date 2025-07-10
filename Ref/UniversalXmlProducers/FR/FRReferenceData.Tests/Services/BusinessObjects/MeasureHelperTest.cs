using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.BusinessObjects
{
	[TestFixture]
	class MeasureHelperTest
	{
		[Test]
		public void TestIsLookingIncorrect()
		{
			var formula = "1 2";
			Assert.IsTrue(MeasureHelper.IsLookingIncorrect(formula), "This formula doesn't make sense");
			formula = "1 * 2";
			Assert.IsFalse(MeasureHelper.IsLookingIncorrect(formula), "This formula is unrealistic but makes sense");

			formula = "1 * MAX(2,) * 4";
			Assert.IsTrue(MeasureHelper.IsLookingIncorrect(formula), "This formula is missing something in MAX function.");
			formula = "1 * MAX(,3) * 4";
			Assert.IsTrue(MeasureHelper.IsLookingIncorrect(formula), "This formula is missing something in MAX function.");
			formula = "1 * MAX(2,3) * 4";
			Assert.IsFalse(MeasureHelper.IsLookingIncorrect(formula), "This formula is unrealistic but makes sense");

			formula = "[1 + MAX(2,3) * 4";
			Assert.IsTrue(MeasureHelper.IsLookingIncorrect(formula), "This formula is missing a closing bracket");
			formula = "1 + MAX(2,3) * 4]";
			Assert.IsTrue(MeasureHelper.IsLookingIncorrect(formula), "This formula shows an extra closing bracket");
			formula = "1 + [MAX(2,3) * 4]";
			Assert.IsFalse(MeasureHelper.IsLookingIncorrect(formula), "This formula is unrealistic but makes sense");

			formula = "(1 + MAX(2,3) * 4";
			Assert.IsTrue(MeasureHelper.IsLookingIncorrect(formula), "This formula is missing a closing parenthesis");
			formula = "1 + MAX(2,3) * 4)";
			Assert.IsTrue(MeasureHelper.IsLookingIncorrect(formula), "This formula shows an extra closing parenthesis");
			formula = "1 + (MAX(2,3) * 4)";
			Assert.IsFalse(MeasureHelper.IsLookingIncorrect(formula), "This formula is unrealistic but makes sense");

			formula = "{1 + MAX(2,3) * 4";
			Assert.IsTrue(MeasureHelper.IsLookingIncorrect(formula), "This formula is missing a closing bracket");
			formula = "1 + MAX(2,3) * 4}";
			Assert.IsTrue(MeasureHelper.IsLookingIncorrect(formula), "This formula shows an extra closing bracket");
			formula = "1 + {MAX(2,3) * 4}";
			Assert.IsFalse(MeasureHelper.IsLookingIncorrect(formula), "This formula is unrealistic but makes sense");

			formula = "IF(HAS(\"CERT, \"2001\") {\"PRECALCULE\"})";
			Assert.IsTrue(MeasureHelper.IsLookingIncorrect(formula), "This formula is missing a double quote");
			formula = "IF(HAS(\"CERT\", \"2001\") {\"PRECALCULE\"})";
			Assert.IsFalse(MeasureHelper.IsLookingIncorrect(formula), "This formula is unrealistic but makes sense");
		}

		[Test]
		public void SplitRegionWhereNecessary()
		{
			var region1 = "";
			var region2 = "";
			MeasureHelper.SplitRegionWhereNecessary("AAAAA", out region1, out region2);
			Assert.AreEqual(region1, "AAAAA");
			Assert.AreEqual(region2, "");

			MeasureHelper.SplitRegionWhereNecessary("METRO", out region1, out region2);
			Assert.AreEqual(region1, "CONTI");
			Assert.AreEqual(region2, "CORSE");
		}

		[Test]
		public void IsRateFormula()
		{
			var measure = EmptyMeasure();
			measure.Conditions.Add(EmptyCondition());
			measure.Conditions.Add(EmptyCondition());

			measure.Conditions[0].Code = "Z";
			measure.Conditions[1].Code = "Z";
			measure.Conditions[0].ActionCode = "99";
			measure.Conditions[1].ActionCode = "98";

			Assert.IsFalse(MeasureHelper.IsRateFormula(measure.Conditions, "Z"));
			measure.Conditions[1].ActionCode = "01";
			Assert.IsTrue(MeasureHelper.IsRateFormula(measure.Conditions, "Z"));
		}

		[Test]
		public void GenerateExportRateFormula()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			var analyser = new RITADataParser();

			var measures = analyser.GetMeasuresAndConditions("2208409900", "E");
			var result = MeasureHelper.GenerateFormula(measures[8], false);
			Assert.AreEqual("304.9 * [005]", result);
			result = MeasureHelper.GenerateFormula(measures[8], true);
			Assert.AreEqual("304,9 EUR/Hectolitre d'alcool pur", result);
			result = MeasureHelper.GenerateFormula(measures[10], false);
			Assert.AreEqual("0", result);
			result = MeasureHelper.GenerateFormula(measures[10], true);
			Assert.AreEqual("0", result);

			measures = analyser.GetMeasuresAndConditions("9706900000", "E");
			result = MeasureHelper.GenerateFormula(measures[17], false);
			Assert.AreEqual("If(has(\"CERT\", \"5003\"), 0, VFD * 0.005)", result);
			result = MeasureHelper.GenerateFormula(measures[17], true);
			Assert.AreEqual("Si présentation du document 5003 alors le montant à percevoir est 0 sinon le montant à percevoir est 0,5 %", result);
			result = MeasureHelper.GenerateFormula(measures[24], false);
			Assert.AreEqual("If(has(\"CERT\", \"5003\"), 0, VFD * 0.06)", result);
			result = MeasureHelper.GenerateFormula(measures[24], true);
			Assert.AreEqual("Si présentation du document 5003 alors le montant à percevoir est 0 sinon le montant à percevoir est 6 %", result);
		}

		[Test]
		public void GenerateFormula()
		{
			ApplicationConfig.Instance.DownloadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			var analyser = new RITADataParser();

			//Edge case
			var measures = analyser.GetMeasuresAndConditions("2402100000", "I");
			var result = MeasureHelper.GenerateFormula(measures[90], false);
			Assert.AreEqual("If(has(\"CERT\", \"2001\") || has(\"CERT\", \"2003\") || has(\"CERT\", \"2002\"), {\"Precalcule\"})", result);
			result = MeasureHelper.GenerateFormula(measures[90], true);
			Assert.AreEqual("Si présentation du document 2001 ou présentation du document 2003 ou présentation du document 2002 alors le montant à percevoir est précalculé sinon la mise à la consommation est interdite", result);

			//Result for flat rate
			measures = analyser.GetMeasuresAndConditions("0101210000", "I");
			result = MeasureHelper.GenerateFormula(measures[65], false);
			Assert.AreEqual("MIN(350 * [FLAT], MAX(5 * [TNE3], 30 * [FLAT]))", result);
			result = MeasureHelper.GenerateFormula(measures[65], true);
			Assert.AreEqual("5 EUR/Tonne lot - Minimum : 30 EUR - Maximum : 350 EUR", result);
			result = MeasureHelper.GenerateFormula(measures[67], false);
			Assert.AreEqual("457.35 * [FLAT]", result);
			result = MeasureHelper.GenerateFormula(measures[67], true);
			Assert.AreEqual("457,35 EUR", result);

			//Result for RED tax G065 with Q206 cana for Erga Omnes.
			measures = analyser.GetMeasuresAndConditions("0304992310", "I");
			result = MeasureHelper.GenerateFormula(measures[90], false);
			Assert.AreEqual("If([TNE3] > 100, 2.52 * [TNE3] - 252 * [FLAT] + 610 * [FLAT], MAX(6.1 * [TNE3], 30.49 * [FLAT]))", result);
			result = MeasureHelper.GenerateFormula(measures[90], true);
			Assert.AreEqual("Si la quantité est supérieure à 100 alors le montant à percevoir est 2,52 EUR/Tonne lot - 252 EUR + 610 EUR sinon le montant à percevoir est 6,1 EUR/Tonne lot - Minimum : 30,49 EUR", result);

			//Result for AMC tax L440 with Q038 cana for FR05/METRO.
			measures = analyser.GetMeasuresAndConditions("2208409900", "I");
			result = MeasureHelper.GenerateFormula(measures[66], false);
			Assert.AreEqual("If(has(\"CERT\", \"2005\") && (has(\"CERT\", \"2001\") || has(\"CERT\", \"2003\") || has(\"CERT\", \"5005\")), 0, 901.84 * [005])", result);
			result = MeasureHelper.GenerateFormula(measures[66], true);
			Assert.AreEqual("Si présentation du document 2005 et (présentation du document 2001 ou présentation du document 2003 ou présentation du document 5005) alors le montant à percevoir est 0 sinon le montant à percevoir est 901,84 EUR/Hectolitre d'alcool pur", result);
			result = MeasureHelper.GenerateFormula(measures[57], false);
			Assert.AreEqual("If(has(\"CERT\", \"2001\") || has(\"CERT\", \"2003\") || has(\"CERT\", \"5005\"), 0, 1802.67 * [005])", result);
			result = MeasureHelper.GenerateFormula(measures[57], true);
			Assert.AreEqual("Si présentation du document 2001 ou présentation du document 2003 ou présentation du document 5005 alors le montant à percevoir est 0 sinon le montant à percevoir est 1802,67 EUR/Hectolitre d'alcool pur", result);

			//Result for RVT tax G065 with Q213 additional code for NZ.
			measures = analyser.GetMeasuresAndConditions("0208903000", "I");
			result = MeasureHelper.GenerateFormula(measures[70], false);
			Assert.AreEqual("MIN(350 * [FLAT], MAX(1.5 * [TNE3], 30 * [FLAT]))", result);
			result = MeasureHelper.GenerateFormula(measures[70], true);
			Assert.AreEqual("1,5 EUR/Tonne lot - Minimum : 30 EUR - Maximum : 350 EUR", result);

			//Result for ORB tax K944 with Z910 additional code for MGPRE
			result = MeasureHelper.GenerateFormula(measures[64], true);
			Assert.AreEqual("1,5 %", result);
			result = MeasureHelper.GenerateFormula(measures[64], false);
			Assert.AreEqual("VFD * 0.015", result);

			//Result for RVT tax G065 with Q202 additional code for Erga Omnes.
			result = MeasureHelper.GenerateFormula(measures[71], true);
			Assert.AreEqual("6,1 EUR/Tonne lot - Minimum : 30,49 EUR - Maximum : 457,35 EUR", result);
			result = MeasureHelper.GenerateFormula(measures[71], false);
			Assert.AreEqual("MIN(457.35 * [FLAT], MAX(6.1 * [TNE3], 30.49 * [FLAT]))", result);

			//Result for RVT tax G065 with Q210 additional code for Erga Omnes.
			result = MeasureHelper.GenerateFormula(measures[72], true);
			Assert.AreEqual("0", result);
			result = MeasureHelper.GenerateFormula(measures[72], false);
			Assert.AreEqual("0", result);

			//Result for ORA tax K942 with 4800 doc for Guadeloupe
			measures = analyser.GetMeasuresAndConditions("3006400000", "I");
			result = MeasureHelper.GenerateFormula(measures[45], true);
			Assert.AreEqual("0", result);
			result = MeasureHelper.GenerateFormula(measures[45], false);
			Assert.AreEqual("0", result);

			//Result for ORB tax K937 with Z917 additional code for Mayotte
			result = MeasureHelper.GenerateFormula(measures[60], true);
			Assert.AreEqual("2,5 %", result);
			result = MeasureHelper.GenerateFormula(measures[60], false);
			Assert.AreEqual("VFD * 0.025", result);

			//Fix case "0 If(|| )".
			measures = analyser.GetMeasuresAndConditions("8411110090", "I");
			result = MeasureHelper.GenerateFormula(measures[21], false);
			Assert.AreEqual("If(has(\"CERT\", \"U090\") || has(\"CERT\", \"U091\"), 0)", result);
			result = MeasureHelper.GenerateFormula(measures[21], true);
			Assert.AreEqual("Si présentation du document U090 ou présentation du document U091 alors le montant à percevoir est 0", result);

			//Fix case"* [FLAT]" formula
			result = MeasureHelper.GenerateFormula(measures[60], false);
			Assert.AreEqual("0", result);
			result = MeasureHelper.GenerateFormula(measures[60], true);
			Assert.AreEqual("0", result);

			//Fix case "0 If(has("CERT", "4504") || has("CERT", "4505") || has("CERT", ""), 0"
			measures = analyser.GetMeasuresAndConditions("9030200000", "I");
			result = MeasureHelper.GenerateFormula(measures[63], false);
			Assert.AreEqual("If(has(\"CERT\", \"4504\") && has(\"CERT\", \"4505\"), 0)", result);
			result = MeasureHelper.GenerateFormula(measures[63], true);
			Assert.AreEqual("Si présentation du document 4504 et présentation du document 4505 alors le montant à percevoir est 0 sinon la mesure n'est pas applicable", result);

			//fix case If(|| || || 0)'
			measures = analyser.GetMeasuresAndConditions("1905907000", "I");
			result = MeasureHelper.GenerateFormula(measures[74], true);
			Assert.AreEqual("Si présentation du document 2001 ou présentation du document 2003 ou présentation du document 5005 alors le montant à percevoir est 0", result);
			result = MeasureHelper.GenerateFormula(measures[74], false);
			Assert.AreEqual("If(has(\"CERT\", \"2001\") || has(\"CERT\", \"2003\") || has(\"CERT\", \"5005\"), 0)", result);

			//Fix case If(has("CERT", "4505")has("CERT", "4504"), 0, has("CERT", ""), 0)
			measures = analyser.GetMeasuresAndConditions("8443133400", "I");
			result = MeasureHelper.GenerateFormula(measures[99], true);
			Assert.AreEqual("Si présentation du document 4505 et présentation du document 4504 alors le montant à percevoir est 0 sinon la mesure n'est pas applicable", result);
			result = MeasureHelper.GenerateFormula(measures[99], false);
			Assert.AreEqual("If(has(\"CERT\", \"4505\") && has(\"CERT\", \"4504\"), 0)", result);
		}

		Condition EmptyCondition()
		{
			return new Condition(string.Empty, string.Empty, 0, string.Empty, string.Empty, string.Empty, string.Empty, null, string.Empty, string.Empty, string.Empty, string.Empty, new List<Component>());
		}

		Measure EmptyMeasure()
		{
			return new Measure(string.Empty, string.Empty, string.Empty, string.Empty, new List<string>(), string.Empty, string.Empty, string.Empty, string.Empty, new DateTime(1900, 01, 01, 00, 00, 00), new DateTime(2079, 06, 06, 23, 59, 00), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, new List<string>(), new List<string>(), new List<Component>(), new List<Condition>());
		}
	}
}
