using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Helpers;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Models;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff.TestClasses;
using NUnit.Framework;
using static CargoWise.RefDbRepo.ZAReferenceData.Services.Common.Constants;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff
{
	[TestFixture]
	class TariffDataTests
	{
		[TestCase("1P1", "1P1", "", "1P1")]
		[TestCase("2 NoNum dodgy", "2", "", "0P0")]
		[TestCase("2 0-217", "2", "201", "2P1")]
		[TestCase("2 250-260", "2", "250", "2P3")]
		[TestCase("2P1", "2P1", "", "2P1")]
		public void ConvertScheduleType(string testName, string scheduleTypeCode, string itemNum, string expectedSchedule)
		{
			var logger = new TestLogger();
			var tariff = new TariffData { ScheduleTypeCode = scheduleTypeCode, ItemNumber = itemNum };

			tariff.ProcessUpdates(null, new TariffHelperForTest(), logger);

			Assert.That(tariff.Schedule.ScheduleType, Is.EqualTo(expectedSchedule), $"{testName}: Schedule");
		}

		[TestCase("", "71.12", "", "7112.99.10", "71129910", "")]
		[TestCase("", "2710.12.02", "", "", "27101202", "")]
		[TestCase("215.02", "7324.10.00", "05.06", "", "215020506", "732410")]
		[TestCase("104.17.90", "2206.90.00", "", "", "1041790", "220690")]
		[TestCase("", "71.12", "", "7112.00.00.10", "7112000010", "")]
		[TestCase("", "71.12", "", "7112.10.00.00", "7112100000", "")]
		[TestCase("104.17.90", "000.00.00", "", "", "1041790", "")]
		public void UpdateTariffCodeDetails(string itemNum, string heading, string code, string subHeading, string expectedCode, string expectedRelationCode)
		{
			var logger = new TestLogger();
			var tariff = new TariffData { ItemNumber = itemNum, Heading = heading, Code = code, SubHeading = subHeading };

			tariff.ProcessUpdates(null, new TariffHelperForTest(), logger);

			Assert.That(tariff.TariffCode, Is.EqualTo(expectedCode), "TariffCode");
			Assert.That(tariff.RelationshipTariffCode, Is.EqualTo(expectedRelationCode), "RelationshipTariffCode");
		}

		[Test]
		public void SetupImportedCountries()
		{
			var countryData = new Dictionary<string, string>()
			{
				{ "BRAZIL", "BR" },
				{ "EU", "EUTRADE" }
			};
			var logger = new TestLogger();
			var tariff = new TariffData { ImportedFrom = "brazil, eu" };

			tariff.ProcessUpdates(countryData, new TariffHelperForTest(), logger);

			Assert.That(tariff.ImportCountries, Is.Not.Null);
			Assert.That(tariff.ImportCountries.Count, Is.EqualTo(2));
			Assert.That(tariff.ImportCountries[0], Is.EqualTo("BR"));
			Assert.That(tariff.ImportCountries[1], Is.EqualTo("EUTRADE"));
		}

		[Test]
		public void CalculateDates()
		{
			var t = new TariffData();

			t.StartDate = new DateTime(2022, 1, 1, 12, 13, 14);
			t.EndDate = new DateTime(2022, 12, 31, 23, 59, 59);

			Assert.That(t.CalcStartDate(), Is.EqualTo(new DateTime(2022, 1, 1, 12, 13, 0)));
			Assert.That(t.CalcEndDate(), Is.EqualTo(new DateTime(2022, 12, 31, 23, 59, 0)));

			t.EndDate = new DateTime(2022, 12, 31, 0, 0, 0);

			Assert.That(t.CalcEndDate(), Is.EqualTo(new DateTime(2022, 12, 31, 23, 59, 0)));

			t.StartDate = DateTime.MinValue;
			t.EndDate = DateTime.MaxValue;

			Assert.That(t.CalcStartDate(), Is.EqualTo(new DateTime(1900, 1, 1)));
			Assert.That(t.CalcEndDate(), Is.EqualTo(new DateTime(2079, 6, 6, 23, 59, 0)));
		}

		[Test]
		public void StatisticalUOM()
		{
			var t = new TariffData();
			var logger = new TestLogger();

			t.ProcessUpdates(null, new TariffHelperForTest(), logger);

			Assert.That(t.StatisticalUnitConverted, Is.EqualTo(string.Empty));

			t.StatisticalUnitOriginal = "UNIT";
			t.ProcessUpdates(null, new TariffHelperForTest(), logger);

			Assert.That(t.StatisticalUnitConverted, Is.EqualTo("NO"));
		}

		[Test]
		public void StatisticalUOMEmptyWithRate()
		{
			var t = new TariffData();
			var logger = new TestLogger();

			t.ProcessUpdates(null, new TariffHelperForTest(), logger);

			Assert.That(t.StatisticalUnitConverted, Is.EqualTo(string.Empty), "No rates, no stat unit");

			t = new TariffData();
			t.Rates = new List<Rate>()
			{
				new Rate { RateType = RateTypes.Standard, FormulaCode = "1216", Description = "R123.45/ML" }
			};

			t.ProcessUpdates(null, new TariffHelperForTest(), logger);

			Assert.That(t.StatisticalUnitConverted, Is.EqualTo("ML"), "1 rate, stat unit updated");

			t = new TariffData();
			t.Rates = new List<Rate>()
			{
				new Rate { RateType = RateTypes.Standard, FormulaCode = "1216", Description = "R123.45/ML" },
				new Rate { RateType = RateTypes.Standard, FormulaCode = "1216", Description = "R12.34/KG" }
			};

			t.ProcessUpdates(null, new TariffHelperForTest(), logger);

			Assert.That(t.StatisticalUnitConverted, Is.EqualTo(string.Empty), "2 or more rates, no stat unit");
		}

		[TestCase("01", "2345", "01", -1, false)]
		[TestCase("2001", "0101", "03", 3, true)]
		[TestCase("2001", "0101", "99", 5, false)]
		public void UniqueIdTariffExists(string itemNum, string code, string chkDigit, short expectedUniqueId, bool expectedKeyExists)
		{
			var logger = new TestLogger();
			var tariff = new TariffData { ItemNumber = itemNum, Code = code, CheckDigit = chkDigit };

			tariff.ProcessUpdates(null, new TariffHelperForTest(), logger);

			Assert.That(tariff.UniqueId, Is.EqualTo(expectedUniqueId), "UniqueId");
			Assert.That(tariff.TariffKeyExists, Is.EqualTo(expectedKeyExists), "TariffKeyExists");
		}

		[Test]
		public void IsValid()
		{
			var logger = new TestLogger();
			var header = new Header { TransactionType = TransactionType.Original };
			var t = new TariffData();

			t.Schedule = SARSSchedule.S1P1;

			t.LineNumber = "911";
			t.TariffCode = "123";
			t.ItemNumber = "456.67.35";
			t.Code = "23.45";
			t.Heading = "123";
			t.SubHeading = "123";
			t.CheckDigit = "4";
			t.Description = "Tariff 1";
			t.StartDate = new DateTime(2022, 1, 1);
			t.EndDate = new DateTime(2022, 12, 31);

			t.Rates = new List<Rate> { new Rate() };

			Func<bool> ClearAndValidate = () =>
			{
				logger.Errors.Clear();
				return t.IsValidTariff(header, logger);
			};

			Assert.That(ClearAndValidate(), Is.EqualTo(true));
			Assert.That(logger.ErrorString, Is.EqualTo(string.Empty));

			t.Schedule = SARSSchedule.Get("XXX", "");

			Assert.That(ClearAndValidate(), Is.EqualTo(false));
			Assert.That(logger.ErrorString, Is.EqualTo("Line: [911] Tariff: [123] Validation error: Invalid schedule\r\n"));
			t.Schedule = SARSSchedule.S1P1;

			t.TariffCode = "";
			Assert.That(ClearAndValidate(), Is.EqualTo(false));
			Assert.That(logger.ErrorString, Is.EqualTo("Line: [911] Tariff: [] Validation error: Tariff code required\r\n"));
			t.TariffCode = "123";

			t.Description = "";
			Assert.That(ClearAndValidate(), Is.EqualTo(false));
			Assert.That(logger.ErrorString, Is.EqualTo("Line: [911] Tariff: [123] Validation error: Description is required\r\n"));
			t.Description = "Fixed";

			t.CheckDigit = "";
			Assert.That(ClearAndValidate(), Is.EqualTo(false));
			Assert.That(logger.ErrorString, Is.EqualTo("Line: [911] Tariff: [123] Validation error: CheckDigit required for 1P1 tariff\r\n"));
			t.CheckDigit = "4";

			t.Rates.Clear();
			Assert.That(ClearAndValidate(), Is.EqualTo(false));
			Assert.That(logger.ErrorString, Is.EqualTo("Line: [911] Tariff: [123] Validation error: No rates\r\n"));

			t.IsAddedForExpiration = true;
			Assert.That(ClearAndValidate(), Is.EqualTo(true));
			t.IsAddedForExpiration = false;

			header.TransactionType = TransactionType.Deletion;
			Assert.That(ClearAndValidate(), Is.EqualTo(true));
			Assert.That(logger.ErrorString, Is.EqualTo(string.Empty));
			header.TransactionType = TransactionType.Addition;
			Assert.That(ClearAndValidate(), Is.EqualTo(false));
			Assert.That(logger.ErrorString, Is.EqualTo("Line: [911] Tariff: [123] Validation error: No rates\r\n"));

			t.Schedule = SARSSchedule.Get("1P2A", "");
			t.Rates = new List<Rate> { new Rate() };
			t.CheckDigit = "";
			Assert.That(ClearAndValidate(), Is.EqualTo(true));
			Assert.That(logger.ErrorString, Is.EqualTo(string.Empty));

			t.ItemNumber = "";
			Assert.That(ClearAndValidate(), Is.EqualTo(true));
			Assert.That(logger.ErrorString, Is.EqualTo(string.Empty));
			t.SubHeading = "";
			Assert.That(ClearAndValidate(), Is.EqualTo(false));
			Assert.That(logger.ErrorString, Is.EqualTo(string.Empty));

			t.ItemNumber = "123";
			Assert.That(ClearAndValidate(), Is.EqualTo(false));
			Assert.That(logger.ErrorString, Is.EqualTo("Line: [911] Tariff: [123] Validation error: Invalid item number\r\n"));
			t.ItemNumber = "123.45";
			Assert.That(ClearAndValidate(), Is.EqualTo(true));
			Assert.That(logger.ErrorString, Is.EqualTo(string.Empty));
			t.Code = "";
			t.Rates.Clear();
			Assert.That(ClearAndValidate(), Is.EqualTo(false));
			Assert.That(logger.ErrorString, Is.EqualTo(string.Empty));

			t.Code = "12";
			t.Rates = new List<Rate> { new Rate() };
			t.EndDate = t.StartDate.AddMinutes(1).AddSeconds(59);
			Assert.That(ClearAndValidate(), Is.EqualTo(false));
			Assert.That(logger.ErrorString, Is.EqualTo("Line: [911] Tariff: [123] Validation error: End date should be at least a minute after start date\r\n"));

			t.EndDate = t.StartDate.AddYears(1);
			t.Rates.Clear();
			t.CheckDigit = "";
			Assert.That(ClearAndValidate(), Is.EqualTo(false));
			Assert.That(logger.ErrorString, Is.EqualTo(string.Empty));
		}

		[Test]
		public void Rules_ApplyAttributeRules()
		{
			var logger = new TestLogger();
			var helper = new TariffHelperForTest();
			var t = new TariffData() { Heading = "12345", CheckDigit = "1" };
			t.ProcessUpdates(null, helper, logger);

			Assert.That(t.AdditionalAttributes, Is.Not.Null.And.Empty);

			t.Heading = "010101";
			t.ProcessUpdates(null, helper, logger);

			Assert.That(t.AdditionalAttributes, Is.Not.Null);
			Assert.That(t.AdditionalAttributes.Count, Is.EqualTo(2));
			Assert.That(t.AdditionalAttributes.ContainsKey("ATNAME1"));
			Assert.That(t.AdditionalAttributes.ContainsKey("ATNAME2"));
		}

		[Test]
		public void Rules_ApplyUOMRules()
		{
			var logger = new TestLogger();
			var helper = new TariffHelperForTest();
			var t = new TariffData() { Heading = "12345", CheckDigit = "1" };
			t.ProcessUpdates(null, helper, logger);

			Assert.That(t.AdditionalUOMs, Is.Not.Null.And.Empty);

			t.Heading = "010101";
			t.ProcessUpdates(null, helper, logger);

			Assert.That(t.AdditionalUOMs, Is.Not.Null);
			Assert.That(t.AdditionalUOMs.Count, Is.EqualTo(2));
			Assert.That(t.AdditionalUOMs.ContainsKey("UO1"));
			Assert.That(t.AdditionalUOMs.ContainsKey("UO2"));
		}

		[Test]
		public void Rules_ApplyRateRules()
		{
			var helper = new TariffHelperForTest();
			var t = new TariffDataForTest() { Schedule = SARSSchedule.S1P1, TariffCode = "010101", CheckDigit = "1" };
			var origRate1 = new Rate { RateType = RateTypes.Standard, Formula = "ORIG-FORM1", Description = "Original1" };
			var origRate2 = new Rate { RateType = RateTypes.EFTA, Formula = "ORIG-FORM2", Description = "Original2" };
			t.Rates.Add(origRate1);
			t.Rates.Add(origRate2);

			Assert.That(t.Rates.Count, Is.EqualTo(2));

			t.ApplyRulesExposed(helper);

			Assert.That(t.Rates.Count, Is.EqualTo(4));
			var newRate1 = t.Rates.FirstOrDefault(x => x.Formula == "NEW-RATE-FORMULA1");
			Assert.That(newRate1, Is.Not.Null);
			Assert.That(newRate1.RateType, Is.EqualTo(RateTypes.EUQuota));
			Assert.That(newRate1.Preference, Is.EqualTo(Preferences.PreferentialQuota));
			Assert.That(newRate1.Description, Is.EqualTo("Rate Rule 1"));

			var newRate2 = t.Rates.FirstOrDefault(x => x.Formula == "NEW-RATE-FORMULA2");
			Assert.That(newRate2, Is.Not.Null);
			Assert.That(newRate2.RateType, Is.EqualTo(RateTypes.EFTAQuota));
			Assert.That(newRate2.Preference, Is.EqualTo(Preferences.PreferentialQuota));
			Assert.That(newRate2.Description, Is.EqualTo("Rate Rule 3"));

			Assert.That(origRate1.Formula, Is.EqualTo("ORIG-FORM1"));
			Assert.That(origRate2.Formula, Is.EqualTo("ORIG-FORM2"));
		}

		[Test]
		public void Rules_ApplyRateRules_EmptySelector()
		{
			var helper = new TariffHelperForTest();
			var t = new TariffDataForTest() { Schedule = SARSSchedule.Get("12A", ""), TariffCode = "010101", CheckDigit = "1" };
			var origRate1 = new Rate { RateType = RateTypes.Standard, Formula = "ORIG-FORM1", Description = "Original1" };
			var origRate2 = new Rate { RateType = RateTypes.EFTA, Formula = "ORIG-FORM2", Description = "Original2" };
			t.Rates.Add(origRate1);
			t.Rates.Add(origRate2);

			Assert.That(t.Rates.Count, Is.EqualTo(2));

			t.ApplyRulesExposed(helper);

			Assert.That(t.Rates.Count, Is.EqualTo(4));
			Assert.That(origRate1.Formula, Is.EqualTo("REPLACE-RATE-FORMULA"));
			Assert.That(origRate1.Description, Is.EqualTo("Rate Rule 2"));
			Assert.That(origRate2.Formula, Is.EqualTo("ORIG-FORM2"));
		}

		[Test]
		public void Rules_Match_TariffCode()
		{
			var helper = new TariffHelperForTest();
			var t1 = new TariffDataForTest() { Schedule = SARSSchedule.S1P1, TariffCode = "02020202" };
			t1.ApplyRulesExposed(helper);

			Assert.That(t1.AdditionalUOMs.Count, Is.EqualTo(2));
			Assert.That(t1.AdditionalUOMs.ContainsKey("UO3"));
			Assert.That(t1.AdditionalUOMs.ContainsKey("UO4"));
		}

		[Test]
		public void Rules_Match_EmptyCheckDigitOnRule()
		{
			var helper = new TariffHelperForTest();
			var t1 = new TariffDataForTest() { Schedule = SARSSchedule.S1P1, TariffCode = "020202", CheckDigit = "2" };

			t1.ApplyRulesExposed(helper);
			Assert.That(t1.AdditionalUOMs.Count, Is.EqualTo(1));
			Assert.That(t1.AdditionalUOMs.ContainsKey("UO3"));

			var t2 = new TariffDataForTest() { Schedule = SARSSchedule.S1P1, TariffCode = "020202", CheckDigit = "" };
			t2.ApplyRulesExposed(helper);
			Assert.That(t2.AdditionalUOMs.Count, Is.EqualTo(1));
		}

		[Test]
		public void Rules_Match_TariffType()
		{
			var helper = new TariffHelperForTest();
			var t1 = new TariffDataForTest() { Schedule = SARSSchedule.S1P1, TariffCode = "030303", CheckDigit = "3" };

			t1.ApplyRulesExposed(helper);
			Assert.That(t1.AdditionalAttributes.Count, Is.EqualTo(0));

			var t2 = new TariffDataForTest() { Schedule = SARSSchedule.Get("17A", ""), TariffCode = "030303", CheckDigit = "3" };

			t2.ApplyRulesExposed(helper);
			Assert.That(t2.AdditionalAttributes.Count, Is.EqualTo(1));
			Assert.That(t2.AdditionalAttributes.ContainsKey("ATNAME3"));
		}

		[Test]
		public void RemoveDuplicateRates()
		{
			var logger = new TestLogger();
			var t1 = new TariffData() { ScheduleTypeCode = "1P1", TariffCode = "030303", CheckDigit = "3" };
			t1.Rates = new List<Rate>
			{
				new Rate { RateType = RateTypes.Standard, Formula = "123", FormulaCode = "3410", Description = "10% OR 55C/KG LESS 90%" },
				new Rate { RateType = RateTypes.EFTA, Formula = "123", FormulaCode = "3410", Description = "10% OR 55C/KG LESS 90%" },
				new Rate { RateType = RateTypes.SADC, Formula = "456", FormulaCode = "3423", Description = "\"860C/KG LESS 85% WITH A MAXIMUM OF 44%\"" },
			};
			Assert.That(t1.Rates.Count, Is.EqualTo(3));
			t1.ProcessUpdates(null, new TariffHelperForTest(), logger);
			Assert.That(t1.Rates.Count, Is.EqualTo(2));
			Assert.That(t1.Rates.FirstOrDefault(x => x.RateType == RateTypes.EFTA), Is.Null);

			var t2 = new TariffDataForTest() { Schedule = SARSSchedule.Get("17A", ""), TariffCode = "030303", CheckDigit = "3" };
			t2.Rates = new List<Rate>
			{
				new Rate { RateType = RateTypes.Standard, Formula = "123", FormulaCode = "1216", Description = "3,817C/LI" },
				new Rate { RateType = RateTypes.EFTA, Formula = "123", FormulaCode = "1216", Description = "3,817C/LI" },
				new Rate { RateType = RateTypes.SADC, Formula = "3436", Description = "{(0,00003 X A) - 0,75}% WITH A MAXIMUM OF 30% (SEE NOTE 1 TO THIS PART" },
			};
			Assert.That(t2.Rates.Count, Is.EqualTo(3));
			t2.ProcessUpdates(null, new TariffHelperForTest(), logger);
			Assert.That(t2.Rates.Count, Is.EqualTo(1));
			Assert.That(t2.Rates.FirstOrDefault()?.RateType, Is.EqualTo(RateTypes.Standard));
		}

		[Test]
		public void IsHeading()
		{
			var t1 = new TariffData() { ScheduleTypeCode = "1P1", Heading = "9403", SubHeading = "940370", TariffCode = "940370" };
			Assert.That(t1.IsHeading, Is.True);

			var t2 = new TariffData() { ScheduleTypeCode = "1P1", ItemNumber = "940370" };
			Assert.That(t2.IsHeading, Is.True);

			var t3 = new TariffData() { ScheduleTypeCode = "1P1", ItemNumber = "940370", Code = "1" };
			Assert.That(t3.IsHeading, Is.False);

			var t4 = new TariffData() { ScheduleTypeCode = "1P1", ItemNumber = "940370", CheckDigit = "1" };
			Assert.That(t4.IsHeading, Is.False);
		}

		[Test]
		public void AllowForExpiration()
		{
			var t1 = new TariffData() { Schedule = SARSSchedule.S1P1 };
			Assert.That(t1.AllowForExpiration, Is.True);

			var t2 = new TariffData();
			Assert.That(t2.AllowForExpiration, Is.False);
		}

		[Test]
		public void ExpireNonStandardApplicabilities()
		{
			var logger = new TestLogger();
			var tariff = new TariffData
			{
				ScheduleTypeCode = Schedules.S1P1,
				Heading = "20010101",
				StartDate = new DateTime(2025, 2, 1),
				EndDate = CommonHelper.MaximumDateTime,
				Rates = new List<Rate>
				{
					new Rate
					{
						RateType = RateTypes.Standard,
						Preference = Preferences.None,
						FormulaCode = "1302",
						Formula = "0.15 * VFD",
						Description = "15%"
					},
					new Rate
					{
						RateType = RateTypes.SADC,
						Preference = Preferences.PreferentialRate,
						FormulaCode = "1302",
						Formula = "0.15 * VFD",
						Description = "15%"
					},
				}
			};
			tariff.ProcessUpdates(null, new TariffHelperForTest(), logger);

			Assert.That(tariff.Rates.Count, Is.EqualTo(2));

			var standardRate = tariff.Rates[0];
			Assert.That(standardRate.Formula, Is.EqualTo("0.15 * VFD"));
			Assert.That(standardRate.Description, Is.EqualTo("15%"));
			Assert.That(standardRate.Preference, Is.EqualTo(Preferences.None));
			Assert.That(standardRate.RateType, Is.EqualTo(RateTypes.Standard));
			Assert.That(standardRate.StartDate, Is.Null);
			Assert.That(standardRate.EndDate, Is.Null);

			var rateToExpire = tariff.Rates[1];
			Assert.That(rateToExpire.Formula, Is.EqualTo("0"));
			Assert.That(rateToExpire.Description, Is.EqualTo("FREE"));
			Assert.That(rateToExpire.Preference, Is.EqualTo(Preferences.PreferentialRate));
			Assert.That(rateToExpire.RateType, Is.EqualTo(RateTypes.SADC));
			Assert.That(rateToExpire.StartDate, Is.EqualTo(new DateTime(2025, 1, 1)));
			Assert.That(rateToExpire.EndDate, Is.EqualTo(new DateTime(2025, 1, 31, 23, 59, 0)));
		}
	}
}
