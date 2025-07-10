using System;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.NZReferenceData.Business;
using CargoWise.RefDbRepo.NZReferenceData.Tests.Tariff;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NZReferenceData.Tests
{
	class TariffRatesBuilderTest
	{
		[SetUp]
		public void Setup()
		{
			builder = new TariffRatesBuilder(DateProvider, Mock.Of<ILogger>());
			dataRepo = new TariffDataRepo();
		}

		[TearDown]
		public void TearDown()
		{
			File.Delete(FilePaths.First().FilePath);
		}

		[Test]
		public void TestBuild()
		{
			var tariffCodes = new[]
			{
				"0101210010",
				"0101100013",
				"0101100022",
				"0101100024",
				"0101100031"
			};

			foreach (var code in tariffCodes)
			{
				var tariff = CreateTariff(code);
				dataRepo.Add(tariff.ZZ1_TariffCode, tariff);
			}

			CreateTariffsRateFile(TariffRates, FilePaths);

			builder.Build(dataRepo, FilePaths, ProcessingData);
			var tariffs = dataRepo.Get();

			Assert.That(tariffs, Has.Exactly(5).Items);

			foreach (var code in tariffCodes)
			{
				var tariff = dataRepo.Load(code);
				Assert.That(tariff, Is.Not.Null, $"Tariff {code} should exist.");
				Assert.That(tariff.RefCusRates.Count(r => r.ZZ2_ZZS_NKPreference == "NML"), Is.EqualTo(1), $"Tariff {code} should have 1 NML rate.");
				Assert.That(
					tariff.RefCusRates.Count(r => r.RefCusRateUOMs.Any(uom => uom.ZXG_UOM == "NMB")),
					Is.EqualTo(1),
					$"Tariff {code} should have 1 rate with UOM NMB."
				);

				var rate = tariff.RefCusRates.First();
				Assert.That(rate.RefCusApplicabilities.Count, Is.EqualTo(1), $"Tariff {code} should have 1 applicability.");
				TariffTestHelper.AssertHasApplicability(rate, "NML", StartDate, EndDate, $"Tariff {code} applicability check failed.");
			}
		}

		[Test]
		public void TestBuild_ReusesExistingCusRateWithMatchingApplicability()
		{
			var existingRate = CreateCusRate("0", "NML");
			var tariffWithExistingRate = CreateTariff(TariffCode, [existingRate]);
			dataRepo.Add(tariffWithExistingRate.ZZ1_TariffCode, tariffWithExistingRate);

			CreateTariffsRateFile(TariffRates, FilePaths);
			
			builder.Build(dataRepo, FilePaths, ProcessingData);

			var tariff = dataRepo.Load(TariffCode);
			Assert.That(tariff.RefCusRates.Count, Is.EqualTo(1), "Expected no new RefCusRate to be created; the existing rate should be reused.");

			var rate = tariff.RefCusRates.First();
			Assert.That(rate.RefCusApplicabilities.Count, Is.EqualTo(1), "Expected the RefCusApplicabilities count to remain 1.");
			TariffTestHelper.AssertHasApplicability(rate, "NML", StartDate, EndDate);
		}

		[Test]
		public void TestBuild_AddsApplicabilityToExistingRate()
		{
			var existingRate = CreateCusRate("0", "NML");
			var tariffWithExistingRate = CreateTariff(TariffCode, [existingRate]);
			dataRepo.Add(tariffWithExistingRate.ZZ1_TariffCode, tariffWithExistingRate);

			CreateTariffsRateFile(TariffRates, FilePaths);
			builder.Build(dataRepo, FilePaths, ProcessingData);

			var tariff = dataRepo.Load(TariffCode);
			Assert.That(tariff.RefCusRates.Count, Is.EqualTo(1), "Expected no new RefCusRate to be created; the existing rate should be reused.");

			var rate = tariff.RefCusRates.First();
			Assert.That(rate.RefCusApplicabilities.Count, Is.EqualTo(1), "Expected a new RefCusRateApplicability to be created.");
			TariffTestHelper.AssertHasApplicability(rate, "NML", StartDate, EndDate);
		}

		[Test]
		public void TestBuild_CreatesNewRateWhenFormulaDoesNotMatch()
		{
			const string matchingPreference = "NML";
			const string nonMatchingFormula = "1*VFD";
			var existingRate = CreateCusRate(nonMatchingFormula, matchingPreference);
			var tariffWithExistingRate = CreateTariff(TariffCode, [existingRate]);
			dataRepo.Add(tariffWithExistingRate.ZZ1_TariffCode, tariffWithExistingRate);

			CreateTariffsRateFile(TariffRates, FilePaths);
			
			builder.Build(dataRepo, FilePaths, ProcessingData);

			var tariff = dataRepo.Load(TariffCode);
			Assert.That(tariff.RefCusRates.Count, Is.EqualTo(2), "Expected a new RefCusRate to be created when the preference or formula code does not match.");

			var newRate = tariff.RefCusRates.FirstOrDefault(r => r.ZZ2_ZZS_NKPreference == "NML" && r.ZZ2_RateFormula == "0");
			Assert.IsNotNull(newRate, "Expected a new RefCusRate with preference 'NML' and formula '0' to be created.");
			Assert.That(newRate.RefCusApplicabilities.Count, Is.EqualTo(1), "Expected the new RefCusRate to have one applicability.");
			TariffTestHelper.AssertHasApplicability(newRate, "NML", StartDate, EndDate);
		}

		[Test]
		public void TestBuild_CreatesNewRateWhenPreferenceDoesNotMatch()
		{
			const string nonMatchingPreference = "AAN";
			const string matchingFormula = "0";
			var existingRate = CreateCusRate(matchingFormula, nonMatchingPreference);
			var tariffWithExistingRate = CreateTariff(TariffCode, [existingRate]);
			dataRepo.Add(tariffWithExistingRate.ZZ1_TariffCode, tariffWithExistingRate);

			CreateTariffsRateFile(TariffRates,FilePaths);
			
			builder.Build(dataRepo, FilePaths, ProcessingData);

			var tariff = dataRepo.Load(TariffCode);
			Assert.That(tariff.RefCusRates.Count, Is.EqualTo(2), "Expected a new RefCusRate to be created when the preference or formula code does not match.");

			var newRate = tariff.RefCusRates.FirstOrDefault(r => r.ZZ2_ZZS_NKPreference == "NML" && r.ZZ2_RateFormula == "0");
			Assert.IsNotNull(newRate, "Expected a new RefCusRate with preference 'NML' and formula '0' to be created.");
			Assert.That(newRate.RefCusApplicabilities.Count, Is.EqualTo(1), "Expected the new RefCusRate to have one applicability.");
			TariffTestHelper.AssertHasApplicability(newRate, "NML", StartDate, EndDate);
		}

		[Test]
		public void TestBuild_CreatesDistinctRatesForEachUniqueFactor()
		{
			var tariffCode = "224299019";
			var tariff = CreateTariff(tariffCode);
			dataRepo.Add(tariff.ZZ1_TariffCode, tariff);

			var tariffRates = new[]
			{
				"22~42~99~01~9~AAN~Jul  1 2019 12:00AM~Jun 30 2020 11:59PM~~4~2.9839~~~~~",
				"22~42~99~01~9~AAN~Jul  1 2020 12:00AM~Jun 30 2021 11:59PM~~4~3.0624~~~~~",
				"22~42~99~01~9~AAN~Jul  1 2021 12:00AM~Jun 30 2022 11:59PM~~4~3.1089~~~~~",
				"22~42~99~01~9~AAN~Jul  1 2022 12:00AM~Jun 30 2023 11:59PM~~4~3.3241~~~~~",
				"22~42~99~01~9~AAN~Jul  1 2023 12:00AM~Jun 30 2024 11:59PM~~4~3.5451~~~~~",
				"22~42~99~01~9~AAN~Jul  1 2024 12:00AM~Jun 30 2025 11:59PM~~4~3.6905~~~~~"
			};

			CreateTariffsRateFile(tariffRates, FilePaths);
			builder.Build(dataRepo, FilePaths, ProcessingData);

			var updatedTariff = dataRepo.Load(tariffCode);
			Assert.That(updatedTariff.RefCusRates.Count, Is.EqualTo(6), "Expected 6 distinct RefCusRate records.");

			var expectedStartAndEndDates = new[]
			{
				("2019-07-01 00:00:00", "2020-06-30 23:59:00"),
				("2020-07-01 00:00:00", "2021-06-30 23:59:00"),
				("2021-07-01 00:00:00", "2022-06-30 23:59:00"),
				("2022-07-01 00:00:00", "2023-06-30 23:59:00"),
				("2023-07-01 00:00:00", "2024-06-30 23:59:00"),
				("2024-07-01 00:00:00", "2025-06-30 23:59:00")
			};

			foreach (var (rate, (start, end)) in updatedTariff.RefCusRates.Zip(expectedStartAndEndDates))
			{
				var startDate = DateTime.Parse(start, CultureInfo.InvariantCulture);
				var endDate = DateTime.Parse(end, CultureInfo.InvariantCulture);
				TariffTestHelper.AssertHasApplicability(rate, "AAN", startDate, endDate);
			}
		}

		[Test]
		public void TestBuild_ReusesRateForIdenticalFormulaAndFactor()
		{
			var tariffCode = "224299019";
			var tariff = CreateTariff(tariffCode);
			dataRepo.Add(tariff.ZZ1_TariffCode, tariff);

			var tariffRates = new[]
			{
				"22~42~99~01~9~AAN~Jul  1 2019 12:00AM~Jun 30 2020 11:59PM~~4~2.000~~~~~",
				"22~42~99~01~9~AAN~Jul  1 2020 12:00AM~Jun 30 2021 11:59PM~~4~3.0624~~~~~",
				"22~42~99~01~9~AAN~Jul  1 2021 12:00AM~Jun 30 2022 11:59PM~~4~2.000~~~~~"
			};

			CreateTariffsRateFile(tariffRates, FilePaths);
			builder.Build(dataRepo, FilePaths, ProcessingData);

			var updatedTariff = dataRepo.Load(tariffCode);
			Assert.That(updatedTariff.RefCusRates.Count, Is.EqualTo(2), "Expected 2 RefCusRate records.");

			var reusedRate = updatedTariff.RefCusRates.First(r => r.ZZ2_RateFormula == "2*[NMB]");
			TariffTestHelper.AssertHasApplicability(
				reusedRate,
				"AAN",
				DateTime.Parse("Jul  1 2019 12:00AM", CultureInfo.InvariantCulture),
				DateTime.Parse("Jun 30 2020 11:59PM", CultureInfo.InvariantCulture)
			);
			TariffTestHelper.AssertHasApplicability(
				reusedRate,
				"AAN",
				DateTime.Parse("Jul  1 2021 12:00AM", CultureInfo.InvariantCulture),
				DateTime.Parse("Jun 30 2022 11:59PM", CultureInfo.InvariantCulture)
			);

			var otherRate = updatedTariff.RefCusRates.First(r => r.ZZ2_RateFormula == "3.0624*[NMB]");
			TariffTestHelper.AssertHasApplicability(
				otherRate,
				"AAN",
				DateTime.Parse("Jul  1 2020 12:00AM", CultureInfo.InvariantCulture),
				DateTime.Parse("Jun 30 2021 11:59PM", CultureInfo.InvariantCulture)
			);
		}

		[Test]
		public void TestBuild_IncludesExpiredRatesWithinFiveYears()
		{
			var tariffCode = "224299019";
			var tariff = CreateTariff(tariffCode);
			dataRepo.Add(tariff.ZZ1_TariffCode, tariff);

			var tariffRates = new[]
			{
				// Expired more than 5 years ago — should be ignored
				"22~42~99~01~9~AAN~Jul  1 2018 12:00AM~Dec 31 2019 11:59PM~~4~1.0000~~~~~",

				// Expired within 5 years — should be included
				"22~42~99~01~9~AAN~Jan  1 2020 12:00AM~Dec 31 2020 11:59PM~~4~2.0000~~~~~",
				"22~42~99~01~9~AAN~Jan  1 2021 12:00AM~Dec 31 2021 11:59PM~~4~2.5000~~~~~",

				// Still current — should be included
				"22~42~99~01~9~AAN~Jan  1 2024 12:00AM~Dec 31 2025 11:59PM~~4~3.0000~~~~~"
			};

			CreateTariffsRateFile(tariffRates, FilePaths);
			builder.Build(dataRepo, FilePaths, ProcessingData);

			var updatedTariff = dataRepo.Load(tariffCode);
			Assert.That(updatedTariff.RefCusRates.Count, Is.EqualTo(3), "Expected 3 RefCusRate records (excluding the one expired more than 5 years ago).");

			var expected = new[]
			{
				("2*[NMB]", "Jan  1 2020 12:00AM", "Dec 31 2020 11:59PM"),
				("2.5*[NMB]", "Jan  1 2021 12:00AM", "Dec 31 2021 11:59PM"),
				("3*[NMB]", "Jan  1 2024 12:00AM", "Dec 31 2025 11:59PM")
			};

			foreach (var (formula, start, end) in expected)
			{
				var rate = updatedTariff.RefCusRates.FirstOrDefault(r => r.ZZ2_RateFormula == formula);
				Assert.IsNotNull(rate, $"Expected rate with formula {formula}.");
				TariffTestHelper.AssertHasApplicability(
					rate,
					"AAN",
					DateTime.Parse(start, CultureInfo.InvariantCulture),
					DateTime.Parse(end, CultureInfo.InvariantCulture)
				);
			}
		}

		[Test]
		public void TestBuild_SortsApplicabilitiesByEndDate()
		{
			var tariffCode = "224299019";
			var tariff = CreateTariff(tariffCode);
			dataRepo.Add(tariff.ZZ1_TariffCode, tariff);

			var tariffRates = new[]
			{
				"22~42~99~01~9~RCEP~Jul  1 2023 12:00AM~Jun 30 2024 11:59PM~~4~3.5451~~~~~",
				"22~42~99~01~9~AAN~Jul  1 2022 12:00AM~Jun 30 2023 11:59PM~~4~3.3241~~~~~",
				"22~42~99~01~9~AAN~Jul  1 2023 12:00AM~Jun 30 2024 11:59PM~~4~3.5451~~~~~",
				"22~42~99~01~9~AAN~Jul  1 2021 12:00AM~Jun 30 2022 11:59PM~~4~3.1089~~~~~",
				"22~42~99~01~9~RCEP~Jul  1 2022 12:00AM~Jun 30 2023 11:59PM~~4~3.3241~~~~~",
				"22~42~99~01~9~RCEP~Jul  1 2021 12:00AM~Jun 30 2022 11:59PM~~4~3.1089~~~~~"
			};
			CreateTariffsRateFile(tariffRates, FilePaths);
			builder.Build(dataRepo, FilePaths, ProcessingData);

			var updatedTariff = dataRepo.Load(tariffCode);

			foreach (var rate in updatedTariff.RefCusRates)
			{
				var sortedApplicabilities = rate.RefCusApplicabilities.OrderBy(a => a.ZZT_EndDate).ToList();
				Assert.That(rate.RefCusApplicabilities, Is.EqualTo(sortedApplicabilities), "Applicabilities should still be sorted by end date.");
			}
		}

		[TestCase("1", "", "", "0")]
		[TestCase("2", "", "", "0")]
		[TestCase("3", "100", "", "1*VFD")]
		[TestCase("4", "200", "", "200*[NMB]")]
		[TestCase("5", "300", "400", "(3*VFD)+(400*[NMB])")]
		[TestCase("6", "500", "600", "(500*[NMB])-(6*VFD)")]
		public void TestBuild_CreatesRateWithExpectedFormula(string formulaCode, string factorA, string factorB, string expectedFormula)
		{
			var tariff = CreateTariff();
			dataRepo.Add(tariff.ZZ1_TariffCode, tariff);

			var tariffRates = new[]
			{
				$"01~01~21~00~10~NML~Jan  1 2012 12:00AM~Dec 31 3000 11:59PM~~{formulaCode}~{factorA}~{factorB}~~~~"
			};

			CreateTariffsRateFile(tariffRates, FilePaths);

			builder.Build(dataRepo, FilePaths, ProcessingData);

			var updatedTariff = dataRepo.Load(TariffCode);

			var rate = updatedTariff.RefCusRates.FirstOrDefault();
			Assert.IsNotNull(rate, "Expected a RefCusRate to be created.");
			Assert.That(rate.ZZ2_RateFormula, Is.EqualTo(expectedFormula), $"Expected formula '{expectedFormula}' for formula code '{formulaCode}'.");
			if (formulaCode == "2")
			{
				var applicability = rate.RefCusApplicabilities.FirstOrDefault();
				Assert.IsNotNull(applicability);
				Assert.That(applicability.ZZT_AdditionalCode, Is.EqualTo(Constants.ApplicabilityAdditionalCodes.IsManual));
			}
		}

		TariffRatesBuilder builder;
		ITopLevelDataRepo<RefCusTariff> dataRepo;

		static readonly string TempFilePath = Path.GetTempFileName();
		static BuildersFilePath[] FilePaths { get; } = [new(TempFilePath)];

		const string TariffCode = "0101210010";

		static IDateProvider DateProvider { get; } = Mock.Of<IDateProvider>(x => x.Today == new DateTime(2025, 1, 1) && x.ActiveDate == x.Today.AddYears(-5));

		static string[] TariffRates { get; }= [
			"01~01~21~00~10~NML~Jan  1 2012 12:00AM~Dec 31 2030 11:59PM~~1~~~~~~",
			"01~01~10~00~13~NML~Jan  1 2012 12:00AM~Dec 31 2030 11:59PM~~1~~~~~~",
			"01~01~10~00~22~NML~Jan  1 2012 12:00AM~Dec 31 2030 11:59PM~~1~~~~~~",
			"01~01~10~00~24~NML~Jan  1 2012 12:00AM~Dec 31 2030 11:59PM~~1~~~~~~",
			"01~01~10~00~31~NML~Jan  1 2012 12:00AM~Dec 31 2030 11:59PM~~1~~~~~~"
		];

		static DateTime StartDate => DateTime.Parse("Jan  1 2012 12:00AM", CultureInfo.InvariantCulture);

		static DateTime EndDate => DateTime.Parse("Dec 31 2030 11:59PM", CultureInfo.InvariantCulture);

		static RefCusTariffUOM[] RefCusTariffUOMs => [
			new() {
					ZZ8_Type = Constants.TariffUOMTypes.CU1,
					ZZ8_UOM = Constants.TariffUOMs.NMB
				}
			];

		static NZTariffProcessingData ProcessingData { get; } = new NZTariffProcessingData();

		static RefCusTariff CreateTariff(string tariffCode = TariffCode, RefCusRate[] refCusRates = null) => TariffTestHelper.CreateTestTariff(tariffCode, RefCusTariffUOMs, StartDate, EndDate, refCusRates);

		static RefCusRate CreateCusRate(string formula, string preference, bool createApplicability = true) => TariffTestHelper.CreateTestCusRate(formula, preference, createApplicability, StartDate, EndDate);

		static void CreateTariffsRateFile(string[] tariffRates, BuildersFilePath[] filePaths) => TariffTestHelper.CreateTestFile(tariffRates, filePaths.First(), string.Empty);
	}
}
