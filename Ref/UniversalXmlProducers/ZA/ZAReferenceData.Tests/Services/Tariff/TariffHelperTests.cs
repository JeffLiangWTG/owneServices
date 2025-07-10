using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Interfaces;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff.TestClasses;
using NUnit.Framework;
using static CargoWise.RefDbRepo.ZAReferenceData.Services.Common.Constants;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff
{
	[TestFixture]
	public class TariffHelperTests
	{
		[TestCase("01010101", "99", "9999",	  -1, false, "No Match")]
		[TestCase("20010101", "",   "",		  2, true,  "TariffCode only")]
		[TestCase("20010101", "03", "",		  3, true,  "TC & CHK")]
		[TestCase("20010101", "",	"1004",   4, true,  "TC & Rel")]
		[TestCase("20010101", "05", "100502", 5, true,  "TC Chk & Rel")]
		[TestCase("30010101", "",   "",       6, true,  "Different Code")]
		[TestCase("20010101", "55", "123456", 5, false,  "Tariffcode match & no overlap on UniqueId")]
		public void GetUniqueId(string tariffCode, string chkDigit, string relationCode, short expectedId, bool expectedMatch, string testName)
		{
			var helper = new TariffHelperForTest() as ITariffHelper;

			var result = helper.GetUniqueId(tariffCode, chkDigit, relationCode);
			Assert.That(result.UniqueId, Is.EqualTo(expectedId), $"{testName}: UniqueId");
			Assert.That(result.Exists, Is.EqualTo(expectedMatch), $"{testName}: Exists");
		}

		[Test]
		public void GetRules()
		{
			var helper = new TariffHelperForTest() as ITariffHelper;

			var rules = helper.GetRules();

			Assert.That(rules, Is.Not.Null);
			Assert.That(rules.Count, Is.EqualTo(4));

			var rule1 = rules.FirstOrDefault(x => x.TariffCode == "010101");
			Assert.That(rule1, Is.Not.Null);

			Assert.That(rule1.CheckDigit, Is.EqualTo("1"));
			Assert.That(rule1.DataGrouping, Is.EqualTo("ZA"));
			Assert.That(rule1.TariffType, Is.Null);

			Assert.That(rule1.Attributes, Is.Not.Null);
			Assert.That(rule1.Attributes.Count, Is.EqualTo(2));
			Assert.That(rule1.Attributes.FirstOrDefault(x => x.AttributeName == "ATNAME1" && x.AttributeValue == "ATVALUE1"), Is.Not.Null);
			Assert.That(rule1.Attributes.FirstOrDefault(x => x.AttributeName == "ATNAME2" && x.AttributeValue == "ATVALUE2"), Is.Not.Null);

			Assert.That(rule1.UnitsOfMeasure, Is.Not.Null);
			Assert.That(rule1.UnitsOfMeasure.Count, Is.EqualTo(2));
			Assert.That(rule1.UnitsOfMeasure.FirstOrDefault(x => x.UOMType == "UO1" && x.UOMValue == "ONE"), Is.Not.Null);
			Assert.That(rule1.UnitsOfMeasure.FirstOrDefault(x => x.UOMType == "UO2" && x.UOMValue == "TWO"), Is.Not.Null);

			Assert.That(rule1.Rates, Is.Not.Null);
			Assert.That(rule1.Rates.Count, Is.EqualTo(3));
			Assert.That(rule1.Rates.FirstOrDefault(x => x.RateFormula == "NEW-RATE-FORMULA1" && x.SelectorFormula == "pp='EUQUOTA'" && x.Description == "Rate Rule 1"), Is.Not.Null);
			Assert.That(rule1.Rates.FirstOrDefault(x => x.RateFormula == "REPLACE-RATE-FORMULA" && string.IsNullOrEmpty(x.SelectorFormula) && x.Description == "Rate Rule 2"), Is.Not.Null);
			Assert.That(rule1.Rates.FirstOrDefault(x => x.RateFormula == "NEW-RATE-FORMULA2" && x.SelectorFormula == "pp='EFTAQUOTA'" && x.Description == "Rate Rule 3"), Is.Not.Null);

			var rule2 = rules.FirstOrDefault(x => x.TariffCode == "020202");
			Assert.That(rule2, Is.Not.Null);
			Assert.That(rule2.CheckDigit, Is.Null);

			var rule3 = rules.FirstOrDefault(x => x.TariffCode == "030303");
			Assert.That(rule3, Is.Not.Null);
			Assert.That(rule3.TariffType, Is.EqualTo("17A"));
		}

		[Test]
		public void GetTariffs()
		{
			var helper = new TariffHelperForTest() as ITariffHelper;
			var results = helper.GetTariffs("20010101");
			Assert.That(results.Count, Is.EqualTo(6));
			Assert.That(results.Select(r => r.TariffCode), Is.All.EqualTo("20010101"));

			var tariff5 = results.First(r => r.UniqueId == 5);
			Assert.That(tariff5.SubHeading, Is.EqualTo("20010101"));
			Assert.That(tariff5.TariffCode, Is.EqualTo("20010101"));
			Assert.That(tariff5.Description, Is.EqualTo("ZA Tariff 5 - Chk & Relations"));
			Assert.That(tariff5.Schedule.ScheduleType, Is.EqualTo(Schedules.S1P1));
			Assert.That(tariff5.StartDate, Is.EqualTo(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
			Assert.That(tariff5.EndDate, Is.EqualTo(new DateTime(2024, 1, 31, 23, 59, 00, DateTimeKind.Utc)));
			Assert.That(tariff5.TariffKeyExists, Is.EqualTo(true));
			Assert.That(tariff5.CheckDigit, Is.EqualTo("05"));
		}

		[Test]
		public void GetRates()
		{
			var helper = new TariffHelperForTest() as ITariffHelper;
			var results = helper.GetRates("20010101", new DateTime(2023, 12, 1), new DateTime(2023, 12, 31));
			Assert.That(results.Count, Is.EqualTo(0));

			results = helper.GetRates("20010101", new DateTime(2025, 1, 1), CommonHelper.MaximumDateTime);
			Assert.That(results.Count, Is.EqualTo(4));

			var rate1 = results[0];
			Assert.That(rate1.Formula, Is.EqualTo("0.10 * VFD"));
			Assert.That(rate1.Description, Is.EqualTo("10%"));
			Assert.That(rate1.Preference, Is.EqualTo(Preferences.None));
			Assert.That(rate1.RateType, Is.EqualTo(RateTypes.Standard));
			Assert.That(rate1.CountryCodes, Is.Empty);
			Assert.That(rate1.StartDate, Is.EqualTo(new DateTime(2024, 1, 1, 0, 0, 0)));
			Assert.That(rate1.EndDate, Is.EqualTo(new DateTime(2024, 12, 31, 23, 59, 0)));
			Assert.That(rate1.Key, Is.EqualTo(RateTypes.Standard));

			var rate4 = results[3];
			Assert.That(rate4.Formula, Is.EqualTo("0.2 * VFD"));
			Assert.That(rate4.Description, Is.EqualTo("20%"));
			Assert.That(rate4.Preference, Is.EqualTo(Preferences.PreferentialRate));
			Assert.That(rate4.RateType, Is.Empty);
			Assert.That(rate4.CountryCodes, Is.EquivalentTo(new[] { "PY", "UY" }));
			Assert.That(rate4.StartDate, Is.EqualTo(new DateTime(2025, 1, 1, 0, 0, 0)));
			Assert.That(rate4.EndDate, Is.EqualTo(CommonHelper.MaximumDateTime));
			Assert.That(rate4.Key, Is.EqualTo("PY,UY"));
		}

		[Test]
		public void TariffQuery()
		{
			var helper = new TariffHelperForTest();
			Assert.That(helper.GetTariffQuery_Exposed("621210301"), Is.EqualTo("RefCusTariffUpdate?$filter=ZZ1_ZZZ_NKDataGrouping eq 'ZA' and ZZ1_TariffCode eq '621210301'"));
		}

		[Test]
		public void TariffAttributeQuery()
		{
			var helper = new TariffHelperForTest();
			Assert.That(helper.GetTariffAttributeQuery_Exposed(), Is.EqualTo("RefCusTariffAttributeUpdate?$filter=ZZ3_Name eq 'CheckDigit'"));
		}

		[Test]
		public void TariffRelationshipQuery()
		{
			var helper = new TariffHelperForTest();
			Assert.That(helper.GetTariffRelationshipQuery_Exposed(), Is.EqualTo("RefCusTariffRelationshipUpdate"));
		}

		[Test]
		public void TariffRuleQuery()
		{
			var helper = new TariffHelperForTest();
			Assert.That(helper.GetTariffRuleQuery_Exposed(), Is.EqualTo("RefCusTariffRuleUpdate?$filter=ZZ1_ZZZ_NKDataGrouping eq 'ZA'"));
		}

		[Test]
		public void TariffAttributeRuleQuery()
		{
			var helper = new TariffHelperForTest();
			var pk = new Guid("FCB0BC38-CA23-483E-B116-953D42705003");
			Assert.That(helper.GetTariffAttributeRuleQuery_Exposed(pk), Is.EqualTo("RefCusTariffAttributeRuleUpdate?$filter=ZZ3_ZZ1_Tariff eq fcb0bc38-ca23-483e-b116-953d42705003"));
		}

		[Test]
		public void TariffTypeQuery()
		{
			var helper = new TariffHelperForTest();
			var pk = new Guid("55b566ca-bde9-4cc5-bb53-504ec1373f0d");
			Assert.That(helper.GetTariffTypeQuery_Exposed(pk), Is.EqualTo("RefCusTariffTypeUpdate?$filter=ZZI_PK eq 55b566ca-bde9-4cc5-bb53-504ec1373f0d"));
		}

		[Test]
		public void TariffUOMRuleQuery()
		{
			var helper = new TariffHelperForTest();
			var pk = new Guid("03601c79-6f7e-49f2-874b-88e1ae3a465d");
			Assert.That(helper.GetTariffUOMRuleQuery_Exposed(pk), Is.EqualTo("RefCusTariffUOMRuleUpdate?$filter=ZZ8_ZZ1_Tariff eq 03601c79-6f7e-49f2-874b-88e1ae3a465d"));
		}

		[Test]
		public void RateRuleQuery()
		{
			var helper = new TariffHelperForTest();
			var pk = new Guid("A6C5573C-3EF4-480D-9876-C188706E4977");
			Assert.That(helper.GetRateRuleQuery_Exposed(pk), Is.EqualTo("RefCusRateRuleUpdate?$filter=ZZ2_ZZ1_Tariff eq a6c5573c-3ef4-480d-9876-c188706e4977"));
		}

		[Test]
		public void TariffExpandedTariffQuery()
		{
			var helper = new TariffHelperForTest();
			var startDate = new DateTime(2025, 1, 1);
			var endDate = new DateTime(2025, 12, 31);
			Assert.That(helper.GetExpandedTariffQuery_Exposed("621210301", startDate, endDate),
				Is.EqualTo("RefCusTariffUpdate?$filter=ZZ1_ZZZ_NKDataGrouping eq 'ZA' and ZZ1_TariffCode eq '621210301' and ZZ1_StartDate le 2025-01-01 and ZZ1_EndDate ge 2025-12-31" +
					"&$expand=RefCusRates($expand=RefCusApplicabilities($expand=RefCusTradeGroup),RefCusPreference,RefCusRateCode)"));
		}

		[Test]
		public void LogErrorAndThrowException()
		{
			var helper = new TariffHelperForTest();

			var ex = Assert.Throws<ReferenceDataException>(() => ((ITariffHelper)helper).GetUniqueId("CRASH", "", ""));

			var msg = "Failed to load query [RefCusTariffUpdate?$filter=ZZ1_ZZZ_NKDataGrouping eq 'ZA' and ZZ1_TariffCode eq 'CRASH'] from RefDataLoader";

			Assert.That(helper.Logger.ErrorString, Contains.Substring(msg));
			Assert.That(ex.Message, Is.EqualTo(msg));
			Assert.That(ex.GetBaseException().Message, Is.EqualTo("Simulated Crash"));
		}

		[Test]
		public void DuplicateDataInSafeDoesNotCauseExceptions()
		{
			var helper = new TariffHelperForTest(true) as ITariffHelper;

			Assert.DoesNotThrow(() =>  (_, _) = helper.GetUniqueId("20010101", "03", ""), "No exception should be thrown");
		}

		[Test]
		[Explicit("Developer Integration Test")]
		public void ZZZ_DevTest_GetRefCusTariff()
		{
			var dataLoader = new ZAReferenceData.Services.Tariff.Loader.RefDataLoader("http://refdbrepoupdate.wisecloud.zone/Update/odata/", false);
			var helper = new TariffHelperForTest(dataLoader);

			var tariffs = helper.Get_Exposed<RefCusTariff>(helper.GetTariffQuery_Exposed("621210301")).ToList();

			Assert.That(tariffs, Is.Not.Null.And.Not.Empty);
			Assert.That(tariffs.Count, Is.EqualTo(1));

			var t = tariffs.First();
			Assert.That(t.ZZ1_TariffCode, Is.EqualTo("621210301"));
		}

		[Test]
		[Explicit("Developer Integration Test")]
		public void ZZZ_DevTest_GetRefCusTariffAttribute()
		{
			var dataLoader = new ZAReferenceData.Services.Tariff.Loader.RefDataLoader("http://refdbrepoupdate.wisecloud.zone/Update/odata/", false);
			var helper = new TariffHelperForTest(dataLoader);

			var attrribs = helper.Get_Exposed<RefCusTariffAttribute>(helper.GetTariffAttributeQuery_Exposed()).ToList();

			Assert.That(attrribs, Is.Not.Null.And.Not.Empty);
			Assert.That(attrribs.Count, Is.GreaterThan(0));
		}

		[Test]
		[Explicit("Developer Integration Test")]
		public void ZZZ_DevTest_GetRefCusTariffRelationship()
		{
			var dataLoader = new ZAReferenceData.Services.Tariff.Loader.RefDataLoader("http://refdbrepoupdate.wisecloud.zone/Update/odata/", false);
			var helper = new TariffHelperForTest(dataLoader);

			var relations = helper.Get_Exposed<RefCusTariffRelationship>(helper.GetTariffRelationshipQuery_Exposed()).ToList();

			Assert.That(relations, Is.Not.Null.And.Not.Empty);
			Assert.That(relations.Count, Is.GreaterThan(0));
		}

		[Test]
		[Explicit("Developer Integration Test")]
		public void ZZZ_DevTest_GetRefCusTariffRule()
		{
			var dataLoader = new ZAReferenceData.Services.Tariff.Loader.RefDataLoader("http://refdbrepoupdate.wisecloud.zone/Update/odata/", false);
			var helper = new TariffHelperForTest(dataLoader);

			var rules = helper.Get_Exposed<RefCusTariffRule>(helper.GetTariffRuleQuery_Exposed()).ToList();

			Assert.That(rules, Is.Not.Null.And.Not.Empty);
			Assert.That(rules.Count, Is.GreaterThan(0));
		}

		[Test]
		[Explicit("Developer Integration Test")]
		public void ZZZ_DevTest_GetRefCusTariffAttrbuteRule()
		{
			var dataLoader = new ZAReferenceData.Services.Tariff.Loader.RefDataLoader("http://refdbrepoupdate.wisecloud.zone/Update/odata/", false);
			var helper = new TariffHelperForTest(dataLoader);
			var pk = new Guid("FCB0BC38-CA23-483E-B116-953D42705003");

			var rules = helper.Get_Exposed<RefCusTariffAttributeRule>(helper.GetTariffAttributeRuleQuery_Exposed(pk)).ToList();

			Assert.That(rules, Is.Not.Null.And.Not.Empty);
			Assert.That(rules.Count, Is.GreaterThan(0));
		}

		[Test]
		[Explicit("Developer Integration Test")]
		public void ZZZ_DevTest_GetRefCusTariffType()
		{
			var dataLoader = new ZAReferenceData.Services.Tariff.Loader.RefDataLoader("http://refdbrepoupdate.wisecloud.zone/Update/odata/", false);
			var helper = new TariffHelperForTest(dataLoader);
			var pk = new Guid("55b566ca-bde9-4cc5-bb53-504ec1373f0d");

			var tt = helper.Get_Exposed<RefCusTariffType>(helper.GetTariffTypeQuery_Exposed(pk)).ToList();

			Assert.That(tt, Is.Not.Null.And.Not.Empty);
			Assert.That(tt.Count, Is.GreaterThan(0));
		}

		[Test]
		[Explicit("Developer Integration Test")]
		public void ZZZ_DevTest_GetRefCusTariffUOMRule()
		{
			var dataLoader = new ZAReferenceData.Services.Tariff.Loader.RefDataLoader("http://refdbrepoupdate.wisecloud.zone/Update/odata/", false);
			var helper = new TariffHelperForTest(dataLoader);
			var pk = new Guid("03601c79-6f7e-49f2-874b-88e1ae3a465d");

			var uom = helper.Get_Exposed<RefCusTariffUOMRule>(helper.GetTariffUOMRuleQuery_Exposed(pk)).ToList();

			Assert.That(uom, Is.Not.Null.And.Not.Empty);
			Assert.That(uom.Count, Is.GreaterThan(0));
		}

		[Test]
		[Explicit("Developer Integration Test")]
		public void ZZZ_DevTest_GetRefRateRule()
		{
			var dataLoader = new ZAReferenceData.Services.Tariff.Loader.RefDataLoader("http://refdbrepoupdate.wisecloud.zone/Update/odata/", false);
			var helper = new TariffHelperForTest(dataLoader);
			var pk = new Guid("A6C5573C-3EF4-480D-9876-C188706E4977");

			var rule = helper.Get_Exposed<RefCusRateRule>(helper.GetRateRuleQuery_Exposed(pk)).ToList();

			Assert.That(rule, Is.Not.Null.And.Not.Empty);
			Assert.That(rule.Count, Is.GreaterThan(0));
		}
	}
}
