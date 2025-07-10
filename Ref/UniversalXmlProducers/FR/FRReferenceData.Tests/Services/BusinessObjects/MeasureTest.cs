using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using CargoWise.RefDbRepo.FRReferenceData.Services.Exceptions;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.BusinessObjects
{
	[TestFixture]
	class MeasureTest
	{
		[Test]
		public void TestGetStatisticalAdditionalCodes()
		{
			var measure = EmptyMeasure();
			measure.MeasureType = "SIP";
			measure.SupplementaryCode = "S049";
			measure.SupplementaryCodeDescription = "Pécharmant";
			var output = measure.GetStatisticalAdditionalCodes();
			Assert.IsNotNull(output);
			Assert.That(output.Count, Is.EqualTo(1));

			var result = output[0];
			Assert.That(result.ZY2_AdditionalCode, Is.EqualTo("S049"));
			Assert.That(result.ZY2_Description, Is.EqualTo("Pécharmant"));
			Assert.That(result.ZY2_ZY3_NKCategory, Is.EqualTo("SIP"));
		}

		[Test]
		public void TestIsExportStatistical()
		{
			var measure = EmptyMeasure();
			measure.MeasureType = "ZZZ";
			Assert.IsFalse(measure.IsExportStatistical);
			measure.MeasureType = "SEP";
			Assert.IsTrue(measure.IsExportStatistical);
			measure.MeasureType = "SIP";
			Assert.IsFalse(measure.IsExportStatistical);
		}

		[Test]
		public void TestIsStatistical()
		{
			var measure = EmptyMeasure();
			measure.MeasureType = "ZZZ";
			Assert.IsFalse(measure.IsStatistical);
			measure.MeasureType = "SEP";
			Assert.IsTrue(measure.IsStatistical);
			measure.MeasureType = "SIP";
			Assert.IsTrue(measure.IsStatistical);
		}

		[Test]
		public void TestIsGrantingOfSea()
		{
			var measure = EmptyMeasure();
			measure.MeasureType = "ZZZ";
			Assert.IsFalse(measure.IsGrantingOfSea);
			measure.MeasureType = "OEA";
			Assert.IsTrue(measure.IsGrantingOfSea);
			measure.MeasureType = "OEB";
			Assert.IsTrue(measure.IsGrantingOfSea);
			measure.MeasureType = "ORA";
			Assert.IsTrue(measure.IsGrantingOfSea);
			measure.MeasureType = "ORB";
			Assert.IsTrue(measure.IsGrantingOfSea);
		}

		[Test]
		public void TestIsImportSupported()
		{
			var measure = EmptyMeasure();
			measure.MeasureType = "ZZZ";
			Assert.IsFalse(measure.IsImportSupported);
			measure.MeasureType = "OEA";
			Assert.IsTrue(measure.IsImportSupported);
			measure.MeasureType = "OEB";
			Assert.IsTrue(measure.IsImportSupported);
			measure.MeasureType = "ORA";
			Assert.IsTrue(measure.IsImportSupported);
			measure.MeasureType = "ORB";
			Assert.IsTrue(measure.IsImportSupported);
			measure.MeasureType = "ADE";
			Assert.IsFalse(measure.IsImportSupported);
			measure.MeasureType = "AAN";
			Assert.IsTrue(measure.IsImportSupported);
			measure.MeasureType = "AMC";
			Assert.IsTrue(measure.IsImportSupported);
			measure.MeasureType = "RCA";
			Assert.IsTrue(measure.IsImportSupported);
			measure.MeasureType = "ROC";
			Assert.IsTrue(measure.IsImportSupported);
			measure.MeasureType = "TVA";
			Assert.IsTrue(measure.IsImportSupported);
			measure.MeasureType = "RCP";
			Assert.IsTrue(measure.IsImportSupported);
		}

		[Test]
		public void TestGetConditionsForGrantingOfSea()
		{
			var excludedTradeGroups = new List<string>() { "GF", "YT" };
			var condition = new Condition("B", "", 1, "Présentation d'un certificat/licence/document", "4502", "SUP", "01", null, "", "", "", "", new List<Component>());
			var conditions = new List<Condition>() { condition };
			var measure = new Measure("-783905", "ORB", "RATE", "FR01", excludedTradeGroups, "GUYAN", "K915", "Octroi de mer régional", "N2000008", new DateTime(2000, 01, 01), new DateTime(2079, 01, 01), "", "Z916", "Bien destinés à l'accomplissement des...", "3006400000", "I", null, null, null, conditions);
			var outputConditions = measure.GetConditionsForGrantingOfSea();
			Assert.IsNotNull(outputConditions);
			Assert.That(outputConditions.Count, Is.EqualTo(1));

			var outputCondition = outputConditions.First();
			Assert.That(outputCondition.ZX1_Comment, Is.EqualTo("Si présentation de l'un des documents ou dispositions tarifaires particulières suivants alors les droits sont suspendus"));
			Assert.That(outputCondition.ZX1_EndDate, Is.EqualTo(new DateTime(2079, 01, 01, 23, 59, 00)), "Condition end date should match measure end date.");
			Assert.That(outputCondition.ZX1_IsExport, Is.EqualTo(false), "Condition direction should match measure direction.");
			Assert.That(outputCondition.ZX1_IsImport, Is.EqualTo(true), "Condition direction should match measure direction.");
			Assert.That(outputCondition.ZX1_StartDate, Is.EqualTo(new DateTime(2000, 01, 01)), "Condition start date should match measure start date.");
			Assert.That(outputCondition.ZX1_ZX2_NKConditionType, Is.EqualTo("ORB"), "Condition type should match measure type.");
			Assert.That(outputCondition.ZX1_ZX2_ZZZ_NKDataGrouping, Is.EqualTo("FR"), "Condition data grouping should always be FR.");
			Assert.That(outputCondition.RefCusApplicabilities.Length, Is.EqualTo(1), "Applicabilities count depends on the condition application territory is metropolitan (2) or a DOM (1). ");
			Assert.That(outputCondition.RefCusConditionValues.Length, Is.EqualTo(1), "There should be as many condition values than conditions in measure.");

			var applicability = outputCondition.RefCusApplicabilities.First();
			Assert.That(applicability.ZZT_AdditionalCode, Is.EqualTo("Z916"), "Applicability additional code date should match measure additional code.");
			Assert.That(applicability.ZZT_EndDate, Is.EqualTo(new DateTime(2079, 01, 01, 23, 59, 00)), "Applicability end date should match measure end date.");
			Assert.That(applicability.ZZT_StartDate, Is.EqualTo(new DateTime(2000, 01, 01)), "Applicability start date should match measure start date.");
			Assert.That(applicability.ZZT_ZZA_NKTradeGroup, Is.EqualTo("FR01"), "Applicability trade group date should match measure trade group.");
			Assert.That(applicability.ZZT_ZZA_NKSecondTradeGroup, Is.EqualTo("GUYAN"), "Applicability second trade group date should match measure application territory." );

			var conditionValue = outputCondition.RefCusConditionValues.First();
			Assert.That(conditionValue.ZX3_ZX4_NKValueType, Is.EqualTo("SUP"), "Condition value type should match condition document type.");
			Assert.That(conditionValue.ZX3_Value, Is.EqualTo("4502"), "Condition value should match condition document code.");

			measure.EndDate = new DateTime(1900, 1, 1);
			outputConditions = measure.GetConditionsForGrantingOfSea();
			Assert.IsEmpty(outputConditions, "There should not be any condition when measure dates are irrelevant.");
		}

		[Test]
		public void TestExceptionWhenRateTypeNotFound()
		{
			var measure = EmptyMeasure();
			measure.Nomenclature = "0000000001";
			measure.Sid = "-1256487";
			measure.TaxCode = "Z000";
			Assert.Throws<MissingInfoException>(() => measure.GetRate(), "No rate type found for tax code Z000 when parsing measure -1256487 of tariff 0000000001.");
		}

		[Test]
		public void GetVatRateType()
		{
			var measure = EmptyMeasure();
			var component = new Component(string.Empty, null, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			var componentList = new List<Component>();
			componentList.Add(component);
			var condition = new Condition(string.Empty, string.Empty, 0, string.Empty, string.Empty, string.Empty, string.Empty, null, string.Empty, string.Empty, string.Empty, string.Empty, componentList);
			measure.Conditions.Add(condition);

			component.Amount = ApplicationConfig.Instance.StandardVatRate;
			Assert.AreEqual(Measure.standardVatRateCode, measure.GetVatRateType());
			component.Amount = ApplicationConfig.Instance.PetroleumVatRate;
			Assert.AreEqual(Measure.petroleumVatRateCode, measure.GetVatRateType());
			component.Amount = ApplicationConfig.Instance.HalfVatRate;
			Assert.AreEqual(Measure.halfVatRateCode, measure.GetVatRateType());
			component.Amount = ApplicationConfig.Instance.DOMStandardVatRate;
			Assert.AreEqual(Measure.domStandardVatRateCode, measure.GetVatRateType());
			component.Amount = ApplicationConfig.Instance.ReducedVatRate;
			Assert.AreEqual(Measure.reducedVatRateCode, measure.GetVatRateType());
			component.Amount = ApplicationConfig.Instance.SuperReducedVatRate;
			Assert.AreEqual(Measure.superReducedVatRateCode, measure.GetVatRateType());
			component.Amount = ApplicationConfig.Instance.DOMLiveStockVatRate;
			Assert.AreEqual(Measure.domLiveStockVatRateCode, measure.GetVatRateType());
			component.Amount = ApplicationConfig.Instance.DOMPressVatRate;
			Assert.AreEqual(Measure.domPressVatRateCode, measure.GetVatRateType());
			component.Amount = ApplicationConfig.Instance.CorsicaSuperReducedVatRate;
			Assert.AreEqual(Measure.corsicaSuperReducedVatRateCode, measure.GetVatRateType());
			component.Amount = ApplicationConfig.Instance.FreeVatRate;
			Assert.AreEqual(Measure.freeVatRateCode, measure.GetVatRateType());
		}

		[Test]
		public void GetVatApplicabilities()
		{
			var measure = EmptyMeasure();
			measure.ApplicationTerritory = "MGPRE";
			measure.TaxCode = "A465";
			SetVatRate(measure, ApplicationConfig.Instance.DOMLiveStockVatRate);
			measure.SupplementaryCode = "V905";
			measure.SupplementaryCodeDescription = "Special rate";
			var result = measure.GetVatApplicabilities();
			Assert.AreEqual(1, result.Length);
			Assert.AreEqual("V905", result[0].ZX5_AdditionalCode);
			Assert.AreEqual("Special rate", result[0].ZX5_Description);
			Assert.AreEqual("06-06-2079", result[0].ZX5_EndDate.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture));
			Assert.AreEqual("01-01-1900", result[0].ZX5_StartDate.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture));
			Assert.AreEqual("A465", result[0].ZX5_VATCategory);
			Assert.AreEqual("MGPRE", result[0].ZX5_ZZA_NKTradeGroup);
			Assert.AreEqual("DAN", result[0].ZX5_ZZF_NKTaxOrFeeCode);
		}

		[Test]
		public void GetVatApplicabilitiesIfInconsistentDates()
		{
			var measure = EmptyMeasure();
			measure.ApplicationTerritory = "MGPRE";
			measure.TaxCode = "A465";
			SetVatRate(measure, ApplicationConfig.Instance.DOMLiveStockVatRate);
			measure.SupplementaryCode = "V905";
			measure.SupplementaryCodeDescription = "Special rate";
			measure.StartDate = DateTime.Today.AddDays(1);
			measure.EndDate = DateTime.Today;
			var result = measure.GetVatApplicabilities();
			Assert.AreEqual(0, result.Length);
		}

		[Test]
		public void TestZZT_EndDate_ZX1_EndDate_MidnightToEndOfDay()
		{
			var condition = EmptyCondition();
			condition.DocumentCode = "C401";
			var measure = EmptyMeasure();
			measure.TaxCode = "A325";
			measure.EndDate = new DateTime(2025, 07, 01, 00, 00, 00);
			measure.Conditions.Add(condition);
			var conditions = measure.GetConditionsForVat();
			Assert.AreEqual(new DateTime(2025, 07, 01, 23, 59, 00), conditions.First().ZX1_EndDate);
			Assert.AreEqual(new DateTime(2025, 07, 01, 23, 59, 00), conditions.First().RefCusApplicabilities.First().ZZT_EndDate);

			conditions = measure.GetConditionsForExcise();
			Assert.AreEqual(new DateTime(2025, 07, 01, 23, 59, 00), conditions.First().ZX1_EndDate);
			Assert.AreEqual(new DateTime(2025, 07, 01, 23, 59, 00), conditions.First().RefCusApplicabilities.First().ZZT_EndDate);

			conditions = measure.GetConditionsForProhibition();
			Assert.AreEqual(new DateTime(2025, 07, 01, 23, 59, 00), conditions.First().ZX1_EndDate);
			Assert.AreEqual(new DateTime(2025, 07, 01, 23, 59, 00), conditions.First().RefCusApplicabilities.First().ZZT_EndDate);

			var additionalCodes = measure.GetStatisticalAdditionalCodes();
			Assert.AreEqual(new DateTime(2025, 07, 01, 23, 59, 00), additionalCodes.First().RefCusApplicabilities.First().ZZT_EndDate);

			var rates = measure.GetRate();
			Assert.AreEqual(new DateTime(2025, 07, 01, 23, 59, 00), rates.First().RefCusApplicabilities.First().ZZT_EndDate);
		}

		Measure EmptyMeasure()
		{
			return new Measure(string.Empty, string.Empty, string.Empty, string.Empty, new List<string>(), string.Empty, string.Empty, string.Empty, string.Empty, new DateTime(1900, 01, 01, 00, 00, 00), new DateTime(2079, 06, 06, 23, 59, 00), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, new List<string>(), new List<string>(), new List<Component>(), new List<Condition>());
		}

		void SetVatRate(Measure measure, decimal vatRate)
		{
			var component = new Component(string.Empty, vatRate, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			var componentList = new List<Component>();
			componentList.Add(component);
			var condition = new Condition(string.Empty, string.Empty, 0, string.Empty, string.Empty, string.Empty, string.Empty, null, string.Empty, string.Empty, string.Empty, string.Empty, componentList);
			measure.Conditions.Add(condition);
		}

		Condition EmptyCondition()
		{
			return new Condition(string.Empty, string.Empty, 0, string.Empty, string.Empty, string.Empty, string.Empty, null, string.Empty, string.Empty, string.Empty, string.Empty, new List<Component>());
		}
	}
}

