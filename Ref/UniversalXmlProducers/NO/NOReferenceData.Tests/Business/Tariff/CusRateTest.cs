using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.NOReferenceData.Services.TradeGroups.LandGruppe;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NOReferenceData.Business.Tariff.Tests;

sealed class CusRateTest
{
	[Test]
	public void TestPreference()
	{
		var land = new LandgruppeListe()
		{
			CountryGroup = new Landgruppe[]
			{
				new Landgruppe() { CountryGroupCode = "CODE1", PreferenceCode = "A" },
				new Landgruppe() { CountryGroupCode = "CODE2", PreferenceCode = "B" }
			}
		};

		Assert.AreEqual("A", CusRate.GetPreferenceCodeForTradeGroup("CODE1", land));
		Assert.AreNotEqual("C", CusRate.GetPreferenceCodeForTradeGroup("CODE2", land));
	}

	[Test]
	public void TestRefCusRate_hasMultipleRatesForSameTariffRateAndTradeGroup() => Assert.Multiple(() =>
	{
		var refCusRate = CusRate.ConvertRefCusRateDuty("Kg", "Per kilo.", "TALL", "444", "K", "N", "DTP", "DTY", "1995-01-01", string.Empty);
		Assert.That(refCusRate.RefCusApplicabilities[0]?.ZZT_AdditionalCode, Is.EqualTo(string.Empty));
		Assert.That(refCusRate.ZZ2_ZY1_NKRateCode, Is.EqualTo("DTP"));
	});

	[Test]
	public void TestGenerateRefCusRateUomRecords()
	{
		var allUomForTariff = new List<string> { "KGM", "LTR" };
		var refCusConditions = new RefCusCondition[]
		{
			new RefCusCondition {
				ZX1_ZX2_NKConditionType = "GP200",
				RefCusConditionValues = new RefCusConditionValue[]
				{
					new RefCusConditionValue { ZX3_Value = "([ASV] &gt; 4.7 &amp; [ASV] &lt;= 10)" },
					new RefCusConditionValue { ZX3_Value = "[RET] &lt; 75" }
				}
			},
			new RefCusCondition {
				ZX1_ZX2_NKConditionType = "MP200",
				RefCusConditionValues = new RefCusConditionValue[]
				{
					new RefCusConditionValue { ZX3_Value = "([ASV] &gt; 4.7 &amp; [ASV] &lt;= 10)" },
				}
			}
		};

		Assert.Multiple(() =>
		{
			var refCusRateUOMs = CusRate.GenerateRefCusRateUomRecords(allUomForTariff, refCusConditions, "L", "GP200");
			Assert.That(refCusRateUOMs.Length, Is.EqualTo(2));
			Assert.That(refCusRateUOMs[0].ZXG_UOM, Is.EqualTo("ASV"));
			Assert.That(refCusRateUOMs[1].ZXG_UOM, Is.EqualTo("RET"));

			refCusRateUOMs = CusRate.GenerateRefCusRateUomRecords(allUomForTariff, refCusConditions, "L", "MP200");
			Assert.That(refCusRateUOMs.Length, Is.EqualTo(1));
			Assert.That(refCusRateUOMs[0].ZXG_UOM, Is.EqualTo("ASV"));

			refCusRateUOMs = CusRate.GenerateRefCusRateUomRecords(allUomForTariff, refCusConditions, "L", "XX200");
			Assert.That(refCusRateUOMs.Length, Is.EqualTo(0));
		});
	}

	[Test]
	public void TestFormulaIsValid() => Assert.Multiple(() =>
	{
		var (valid, formula, _) = CusRate.CreateFormula("S", "123,456", "DTY", 1);
		Assert.That(valid, Is.EqualTo(true));
		Assert.That(formula, Is.EqualTo("123.456 * [NMB]"));

		(valid, _, _) = CusRate.CreateFormula("S", "RandomString", "DTY", 1);
		Assert.That(valid, Is.EqualTo(false));

		(valid, _, _) = CusRate.CreateFormula("S", "0", "DTY", 0);
		Assert.That(valid, Is.EqualTo(false));
	});

	[Test]
	public void TestRefCusRate()
	{
		Assert.Multiple(() =>
		{
			AssertTestRefCusRate("Kg",  "Per kilo.",      "TALL", "A", "999999,99", string.Empty, "DTY",   "DTY", "1995-01-01", string.Empty, Constants.MinimumDateTime,  Constants.MaximumDateTime,  "999999.99",    "",    "");
			AssertTestRefCusRate("STK", "Antall enheter", "TALL", "A", "5000,00",   "S",          "DTY",   "DTY", "2012-01-01", string.Empty, new DateTime(2012, 01, 01), Constants.MaximumDateTime,  "5000 * [NMB]", "NMB", "");
			AssertTestRefCusRate("L",   "Antall enheter", "TALL", "A", "1234,00",   "L",          "MB200", "EXC", "2012-01-01", string.Empty, new DateTime(2012, 01, 01), Constants.MaximumDateTime,  "1234 * [LTR]", "LTR", "MB200");
			AssertTestRefCusRate("K",   "Per kilo.",      "TGS7", "A", "0,00",      "K",          "MB200", "EXC", "2023-04-15", "2023-04-15", new DateTime(2023, 04, 15), new DateTime(2023, 04, 15, 23, 59, 0), "0",            "",    "MB200");
			AssertTestRefCusRateIsInvalid("Kg", "Per kilo.", "TALL",       "A", "999999,99", string.Empty, "DTY", "DTY", string.Empty, string.Empty, Constants.MinimumDateTime, Constants.MaximumDateTime, "999999.99 * [KGM]", "");
			AssertTestRefCusRateIsInvalid("Kg", "Per kilo.", string.Empty, "A", "999999,99", string.Empty, "DTY",   "DTY", "1995-01-01", string.Empty, Constants.MinimumDateTime,  Constants.MaximumDateTime,  "999999.99 * [KGM]", "");
			AssertTestRefCusRateIsInvalid("Kg", "Per kilo.", string.Empty, "A", "999999,99", string.Empty, "DTY",   "EXC", "1995-01-01", string.Empty, Constants.MinimumDateTime,  Constants.MaximumDateTime,  "999999.99 * [KGM]", "");
			AssertTestRefCusRateIsInvalid("Kg", "Per kilo.", "TALL", "A", null, string.Empty, "DTY", "DTY", "1995-01-01", string.Empty, Constants.MinimumDateTime, Constants.MaximumDateTime, "999999.99", "");
		});
	}

	[Test]
	public void TestRateFormulaDerivedFrom()
	{
		Assert.Multiple(() =>
		{
			AssertRateGivenInFractionsOfKroner("false", string.Empty);
			AssertRateGivenInFractionsOfKroner("true", "RateGivenInFractionsOfKroner");
			AssertRateGivenInFractionsOfKroner(null, string.Empty);
		});

		static void AssertRateGivenInFractionsOfKroner(string rateGivenInFractions, string expectedValue)
		{
			var refCusRate = CusRate.ConvertRefCusRateExcise("2000-01-01", string.Empty, "1", rateGivenInFractions, "S", "DTY", "DTY", string.Empty);
			Assert.That(refCusRate.ZZ2_RateFormulaDerivedFrom, Is.EqualTo(expectedValue));
		}
	}

	void AssertTestRefCusRate(string uom, string uomDesc, string countryGroup, string preference, string rateValue, string rateUnit, string rateCode, string rateType, string startDate, string endDate, DateTime expectedStartDate, DateTime expectedEndDate, string expectedSatsverdi, string expectedUOM, string expectedAdditionalCode)
	{
		var refCusRate = CusRate.ConvertRefCusRateDuty(uom, uomDesc, countryGroup, rateValue, rateUnit, preference, rateCode, rateType, startDate, endDate);

		Assert.That(refCusRate.ZZ2_StartDate, Is.EqualTo(expectedStartDate).NoClip);
		Assert.That(refCusRate.ZZ2_EndDate, Is.EqualTo(expectedEndDate).NoClip);
		Assert.That(refCusRate.ZZ2_RateFormula, Is.EqualTo(expectedSatsverdi));
		Assert.That(refCusRate.ZZ2_SelectorFormula, Is.EqualTo(rateUnit));
		Assert.That(refCusRate.ZZ2_ZY1_NKRateCode, Is.EqualTo(rateCode));
		Assert.That(refCusRate.ZZ2_ZY1_ZZR_NKRateType, Is.EqualTo(rateType));
		Assert.That(refCusRate.ZZ2_ZZS_NKPreference, Is.EqualTo(preference));
		var refCusApplicabilities = refCusRate.RefCusApplicabilities.First();
		Assert.That(refCusApplicabilities, Is.TypeOf<RefCusApplicability>());

		Assert.That(refCusApplicabilities.ZZT_EndDate, Is.EqualTo(expectedEndDate).NoClip);
		Assert.That(refCusApplicabilities.ZZT_StartDate, Is.EqualTo(expectedStartDate).NoClip);
		Assert.That(refCusApplicabilities.ZZT_ZZA_NKTradeGroup, Is.EqualTo(countryGroup));
		Assert.That(refCusApplicabilities.ZZT_AdditionalCode, Is.EqualTo(expectedAdditionalCode));

		if (refCusRate.RefCusRateUOMs != null)
		{
			var refCusRateUOM = refCusRate.RefCusRateUOMs.First();
			Assert.That(refCusRateUOM, Is.TypeOf<RefCusRateUOM>());
			Assert.That(refCusRateUOM.ZXG_UOM, Is.EqualTo(expectedUOM).NoClip);
		}
	}
	void AssertTestRefCusRateIsInvalid(string uom, string uomDesc, string countryGroup, string preference, string rateValue, string rateUnit, string rateCode, string rateType, string startDate, string endDate, DateTime expectedStartDate, DateTime expectedEndDate, string expectedSatsverdi, string expectedUOM)
	{
		TariffParser.ErrorBuilder.Clear();
		var errMsg = $@"Unable to parse Rate code due to empty code, empty description or invalid Dates.
DETAILS:
Unit code: {uom}
Description: {uomDesc}
Value: {rateValue}
Unit: {rateUnit}
Countrygroup: {countryGroup}
Start Date: {startDate}
End Date: {endDate}
";
		var refCusRate = CusRate.ConvertRefCusRateDuty(uom, uomDesc, countryGroup, rateValue, rateUnit, preference, rateCode, rateType, startDate, endDate);
		Assert.That(refCusRate, Is.EqualTo(null));
		Assert.That(TariffParser.ErrorBuilder.ToString(), Is.EqualTo(errMsg).NoClip);
	}


	[Test]
	public void TestRefCusRateExcise()
	{
		Assert.Multiple(() =>
		{
			AssertTestRefCusRateExcise("1995-01-01", string.Empty, "1234,00", "false", "L", "TYP", "100", "100Description", new DateTime(2000, 01, 01), Constants.MaximumDateTime,  "1234 * [LTR]");
			AssertTestRefCusRateExcise("2023-01-01", string.Empty, "4,92",    "false", "N", "TYP", "101", "101Description", new DateTime(2023, 01, 01), Constants.MaximumDateTime,  "4.92 * [MLT]");
			AssertTestRefCusRateExcise("1995-01-01", string.Empty, "1234,00", "true",  "L", "TYP", "100", "100Description", new DateTime(2000, 01, 01), Constants.MaximumDateTime,  "12.34 * [LTR]");
			AssertTestRefCusRateExcise("2023-01-01", string.Empty, "4,92",    "true",  "N", "TYP", "101", "101Description", new DateTime(2023, 01, 01), Constants.MaximumDateTime,  "0.0492 * [MLT]");
			AssertTestRefCusRateExcise("2023-01-01", string.Empty, "4,92",    null,    "N", "TYP", "101", "101Description", new DateTime(2023, 01, 01), Constants.MaximumDateTime,  "4.92 * [MLT]");
			AssertTestRefCusRateExcise("2023-01-01", "2029-09-09", "1,23",    "false", "M", "TYP", "102", "102Description", new DateTime(2023, 01, 01), new DateTime(2029, 09, 09, 23, 59, 0), "1.23 * [MTQ]");
			AssertTestRefCusRateExcise("1995-01-01", string.Empty, "1234,00", "false", "P", "TYP", "100", "100Description", new DateTime(2000, 01, 01), Constants.MaximumDateTime,  "12.34 * VFD");
			AssertTestRefCusRateExcise("1995-01-01", string.Empty, "2345,00", "true",  "P", "TYP", "100", "100Description", new DateTime(2000, 01, 01), Constants.MaximumDateTime,  "23.45 * VFD");
			AssertTestRefCusRateExciseIsInvalid("2023-01-01", string.Empty, "1,23", "false", "X", "TYP", "103", "Unknown enhet", new DateTime(2023, 01, 01), Constants.MaximumDateTime,  string.Empty);
			AssertTestRefCusRateExciseIsInvalid("2023-01-01", string.Empty, "1,23", "false", "H", "TYP", "103", "Unknown enhet", new DateTime(2023, 01, 01), Constants.MaximumDateTime,  string.Empty);
			AssertTestRefCusRateExciseIsInvalid(string.Empty, string.Empty, "1,23", "false", "X", "TYP", "103", "No dates",      new DateTime(2023, 01, 01), Constants.MaximumDateTime,  string.Empty);
			AssertTestRefCusRateExciseIsInvalid("2023-01-01", "2029-09-09", "1,23", "false", "M", "",    "104", "No RateType",   new DateTime(2023, 01, 01), new DateTime(2029, 09, 09), string.Empty);
		});
	}

	[Test]
	public void TestRefCusRateExciseCodeVFDUOMs()
	{
		Assert.Multiple(() =>
		{
			AssertTestRefCusRateExciseVFDUOMs("1995-01-01", string.Empty, "1234,00", "false", "P", "TYP", "100", "100Description", new DateTime(2000, 01, 01), Constants.MaximumDateTime, "12.34 * VFD");
			AssertTestRefCusRateExciseVFDUOMs("1995-01-01", string.Empty, "2345,00", "true", "P", "TYP", "100", "100Description", new DateTime(2000, 01, 01), Constants.MaximumDateTime, "23.45 * VFD");
		});
	}

	void AssertTestRefCusRateExciseVFDUOMs(string startDate, string endDate, string rateValue, string rateIsGivenAsPercentageOfNOKString, string rateUnit, string rateType, string rateGroup, string rateGroupDescription, DateTime expectedStartDate, DateTime expectedEndDate, string expectedSatsverdi)
	{
		var refCusRate = CusRate.ConvertRefCusRateExcise(startDate, endDate, rateValue, rateIsGivenAsPercentageOfNOKString, rateUnit, rateType, rateGroup, rateGroupDescription);
		Assert.NotNull(refCusRate);
		Assert.That(refCusRate.ZZ2_StartDate, Is.EqualTo(expectedStartDate).NoClip);
		Assert.That(refCusRate.ZZ2_EndDate, Is.EqualTo(expectedEndDate).NoClip);
		Assert.That(refCusRate.ZZ2_RateFormula, Is.EqualTo(expectedSatsverdi));
		Assert.That(refCusRate.ZZ2_SelectorFormula, Is.EqualTo(rateUnit));
		Assert.That(refCusRate.ZZ2_ZY1_NKRateCode, Is.EqualTo(rateType));
		Assert.That(refCusRate.ZZ2_ZY1_ZZR_NKRateType, Is.EqualTo(rateGroup));

		var refCusRateUOMs = CusRate.GenerateRefCusRateUomRecords([ "KGM", "LTR" ], [], rateUnit, refCusRate.ZZ2_ZY1_NKRateCode);
		Assert.NotNull(refCusRateUOMs);
		Assert.That(refCusRateUOMs.Length, Is.EqualTo(1));
		Assert.That(refCusRateUOMs[0].ZXG_UOM, Is.EqualTo("VFD"));
	}


	void AssertTestRefCusRateExcise(string startDate, string endDate, string rateValue, string rateIsGivenAsPercentageOfNOKString, string rateUnit, string rateType, string rateGroup, string rateGroupDescription, DateTime expectedStartDate, DateTime expectedEndDate, string expectedSatsverdi)
	{
		var refCusRate = CusRate.ConvertRefCusRateExcise(startDate, endDate, rateValue, rateIsGivenAsPercentageOfNOKString, rateUnit, rateType, rateGroup, rateGroupDescription);
		Assert.That(refCusRate.ZZ2_StartDate, Is.EqualTo(expectedStartDate).NoClip);
		Assert.That(refCusRate.ZZ2_EndDate, Is.EqualTo(expectedEndDate).NoClip);
		Assert.That(refCusRate.ZZ2_RateFormula, Is.EqualTo(expectedSatsverdi));
		Assert.That(refCusRate.ZZ2_SelectorFormula, Is.EqualTo(rateUnit));
		Assert.That(refCusRate.ZZ2_ZY1_NKRateCode, Is.EqualTo(rateType));
		Assert.That(refCusRate.ZZ2_ZY1_ZZR_NKRateType, Is.EqualTo(rateGroup));

		var refCusApplicability = refCusRate?.RefCusApplicabilities?.First() ?? null;
		if (refCusApplicability != null)
		{
			Assert.That(refCusApplicability, Is.TypeOf<RefCusApplicability>());
			Assert.That(refCusApplicability.ZZT_EndDate, Is.EqualTo(expectedEndDate).NoClip);
			Assert.That(refCusApplicability.ZZT_StartDate, Is.EqualTo(expectedStartDate).NoClip);
			Assert.That(refCusApplicability.ZZT_AdditionalCode, Is.EqualTo(rateType).NoClip);
		}
	}

	void AssertTestRefCusRateExciseIsInvalid(string startDate, string endDate, string ratevalue, string rateIsGivenAsPercentageOfNOKString, string rateUnit, string rateType, string rateGroup, string rateGroupDescription, DateTime expectedStartDate, DateTime expectedEndDate, string expectedSatsverdi)
	{
		TariffParser.ErrorBuilder.Clear();
		var errMsg = $@"Unable to parse Rate code due to empty code, empty description or invalid Dates.
DETAILS:
Fee code: {rateType}
Value: {ratevalue}
Unit: {rateUnit}
Fee Type: {rateGroup}
Fee Type Description: {rateGroupDescription}
Start Date: {startDate}
End Date: {endDate}
";
		var refCusRate = CusRate.ConvertRefCusRateExcise(startDate, endDate, ratevalue, rateIsGivenAsPercentageOfNOKString, rateUnit, rateType, rateGroup, rateGroupDescription);
		Assert.That(TariffParser.ErrorBuilder.ToString(), Is.EqualTo(errMsg).NoClip);
	}
}
