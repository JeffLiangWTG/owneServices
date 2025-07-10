using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.NZReferenceData.Business;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NZReferenceData.Tests
{
	class TariffLeviesBuilderTest
	{
		[Test]
		public void TestBuild()
		{
			IDataRepo dataRepo = new TariffDataRepo();
			var testTariffNeedCreateNewRate = CreateTestTariff("2106903900", new DateTime(2000, 1, 1), new DateTime(2030, 1, 1), "CU1");
			dataRepo.Add(testTariffNeedCreateNewRate.ZZ1_TariffCode, testTariffNeedCreateNewRate);

			var testTariffNeedUpdateDate = CreateTestTariff("2203003902", new DateTime(2000, 1, 1), new DateTime(2030, 1, 1), "CU2");
			testTariffNeedUpdateDate.RefCusRates = new RefCusRate[]
			{
				new RefCusRate()
				{
					ZZ2_ZY1_NKRateCode = "AS",
					ZZ2_ZY1_ZZR_NKRateType = Constants.TariffRateCodes.LVY,
					ZZ2_RateFormula = "0.015854*[KG]",
				}
			};
			dataRepo.Add(testTariffNeedUpdateDate.ZZ1_TariffCode, testTariffNeedUpdateDate);

			var builder = new TariffLeviesBuilder(DateProvider, new Logger(), new TariffLeviesTestFileReader(
				new[] {
					"21~06~90~39~00~AL~6~Dec  7 2000 12:00AM~Aug  1 2003  1:16PM",
					"22~03~00~39~02~AS~291~Jul  1 2008 12:00AM~Jun 30 2009 11:59PM",
					"22~03~00~39~02~AS~291~Jul  1 2019 12:00AM~Jun 30 2020 11:59PM",
				}, new[] {
					"6~0.000000",
					"291~0.015854"
				}
			));

			builder.Build(dataRepo, FilePaths, ProcessingData);

			Assert.That(testTariffNeedCreateNewRate.RefCusRates, Has.Length.EqualTo(1));
			var rate = testTariffNeedCreateNewRate.RefCusRates[0];
			Assert.That(rate.RefCusApplicabilities, Has.Length.EqualTo(1));
			AssertApplicability("testTariffNeedCreateNewRate", rate.RefCusApplicabilities[0], new DateTime(2000, 12, 7), new DateTime(2003, 8, 1, 13, 16, 0));

			Assert.That(testTariffNeedUpdateDate.RefCusRates, Has.Length.EqualTo(1));
			rate = testTariffNeedUpdateDate.RefCusRates[0];
			Assert.That(rate.RefCusApplicabilities, Has.Length.EqualTo(2));
			AssertApplicability("testTariffNeedCreateNewRate, Applicability.EndDate before rate.EndDate", rate.RefCusApplicabilities[0], new DateTime(2008, 7, 1), new DateTime(2009, 6, 30, 23, 59, 0));
			AssertApplicability("testTariffNeedCreateNewRate, Applicability.EndDate after rate.EndDate", rate.RefCusApplicabilities[1], new DateTime(2019, 7, 1), new DateTime(2020, 6, 30, 23, 59, 0));

			void AssertApplicability(string message, RefCusApplicability applicability, DateTime expectedStartDate, DateTime expectedEndDate)
			{
				Assert.AreEqual(expectedStartDate, applicability.ZZT_StartDate, $"{message}.ZZT_StartDate");
				Assert.AreEqual(expectedEndDate, applicability.ZZT_EndDate, $"{message}.ZZT_EndDate");
				Assert.AreEqual(Constants.TariffTradeGroups.NML, applicability.ZZT_ZZA_NKTradeGroup, $"{message}.ZZT_ZZA_NKTradeGroup");
			}
		}

		RefCusTariff CreateTestTariff(string tariffCode, DateTime startDate, DateTime endDate, string uomType)
		{
			var result = new RefCusTariff();
			result.ZZ1_TariffCode = tariffCode;
			result.ZZ1_StartDate = startDate;
			result.ZZ1_EndDate = endDate;
			result.RefCusTariffUOMs = new RefCusTariffUOM[]
			{
				new RefCusTariffUOM() { ZZ8_Type = uomType, ZZ8_UOM = "KG" }
			};

			return result;
		}

		BuildersFilePath[] FilePaths => new[] {
			new BuildersFilePath("Tariff_Levies.csv", BuilderFilePathSymbol.Levy),
			new BuildersFilePath("Tariff_Levy_Formulas.csv", BuilderFilePathSymbol.LevyFormula),
		};

		IDateProvider DateProvider => Mock.Of<IDateProvider>(x => x.Today == new DateTime(2010, 1, 1) && x.ActiveDate == new DateTime(2000, 1, 1));

		NZTariffProcessingData ProcessingData { get; } = new NZTariffProcessingData();

		class TariffLeviesTestFileReader : IFileReader
		{
			public const string TariffLeviesHeader = "Tlrc Tariff Level 1~Tlrc Tariff Level 2~Tlrc Tariff Level 3~Tlrc Tariff Level 4~Tlrc Tariff Level 5~Tlrc Levy Type Code~Tlrc Levy Formula Code~Tlrc Start Date~Tlrc Expiry Date";
			public const string TariffLevyFormulasHeader = "Lfc Levy Formula Codes~Lfc Levy Formula Rate";

			public TariffLeviesTestFileReader(string[] tariffLevies, string[] tariffLevyFormulas)
			{
				TariffLevies = new[] { TariffLeviesHeader }.Concat(tariffLevies).ToArray();
				TariffLevyFormulas = new[] { TariffLevyFormulasHeader }.Concat(tariffLevyFormulas).ToArray();
			}

			public string[] TariffLevies { get; }

			public string[] TariffLevyFormulas { get; }

			public string[] ReadAllLines(string path)
			{
				if (path == "Tariff_Levies.csv")
				{
					return TariffLevies;
				}
				return TariffLevyFormulas;
			}
		}
	}
}
