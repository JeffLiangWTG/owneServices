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
	class TariffDetailsBuilderTest
	{
		[SetUp]
		public void Setup()
		{
			builder = new TariffDetailsBuilder(DateProvider, Mock.Of<ILogger>());
			dataRepo = new TariffDataRepo();
		}

		[TearDown]
		public void TearDown()
		{
			foreach (var builderFilePath in TariffBuilderFilePaths)
			{
				File.Delete(builderFilePath.FilePath);
			}
		}

		[Test]
		public void TestBuild()
		{
			var tariffDetails = new[]
			{
				"01~01~90~00~90~D~1~NMB~~~~N~Jan  1 2022 12:00AM~Dec  8 2024 11:59PM~Generic description",
				"01~01~90~00~90~D~1~NMB~~~~N~Dec  9 2024 12:00AM~Dec  9 2024 11:59PM~Generic description"
			};
			CreateTariffBuilderFiles(tariffDetails);

			builder.Build(dataRepo, TariffBuilderFilePaths, ProcessingData);

			var tariff = dataRepo.Load("0101900090");

			Assert.That(tariff, Is.Not.Null, "Expected tariff to be loaded by code without check digit.");
			Assert.That(tariff.ZZ1_TariffCode, Is.EqualTo("0101900090D"));
			Assert.That(tariff.ZZ1_StartDate, Is.EqualTo(DateTime.Parse("Dec  9 2024 12:00AM", CultureInfo.InvariantCulture)));
			Assert.That(tariff.ZZ1_EndDate, Is.EqualTo(DateTime.Parse("Dec  9 2024 11:59PM", CultureInfo.InvariantCulture)));
			Assert.That(tariff.ZZ1_Description, Is.EqualTo("Generic description"));
			Assert.That(tariff.RefCusTariffUOMs, Has.Length.EqualTo(1));
			Assert.That(tariff.RefCusTariffUOMs[0].ZZ8_Type, Is.EqualTo(Constants.TariffUOMTypes.CU1));
			Assert.That(tariff.RefCusTariffUOMs[0].ZZ8_UOM, Is.EqualTo("NMB"));
		}

		[Test]
		public void TestBuild_OverridesDescription()
		{
			var tariffDetails = new[]
			{
				"01~01~90~00~90~D~1~NMB~~~~N~Jan  1 2022 12:00AM~Dec  8 2024 11:59PM~Original description"
			};

			var tariffDetailsOverridesJsonPath = Path.GetTempFileName();
			const string tariffOverrideJson = "[{ \"Code\": \"0101900090D\", \"Description\": \"Overridden description\" }]";
			CreateTariffBuilderFiles(tariffDetails, tariffOverrideJson);

			builder.Build(dataRepo, TariffBuilderFilePaths, ProcessingData);

			var tariff = dataRepo.Load("0101900090");

			Assert.That(tariff, Is.Not.Null);
			Assert.That(tariff.ZZ1_Description, Is.EqualTo("Overridden description"));
		}

		[Test]
		public void TestBuild_UsesOriginalDescription_WhenOverrideIsNull()
		{
			const string originalDescription = "Original description";
			var tariffDetails = new[]
			{
				"01~01~90~00~90~D~1~NMB~~~~N~Jan  1 2022 12:00AM~Dec  8 2024 11:59PM~" + originalDescription
			};
			
			CreateTariffBuilderFiles(tariffDetails);

			builder.Build(dataRepo, TariffBuilderFilePaths, ProcessingData);

			var tariff = dataRepo.Load("0101900090");

			Assert.That(tariff, Is.Not.Null);
			Assert.That(tariff.ZZ1_Description, Is.EqualTo(originalDescription));
		}

		TariffDetailsBuilder builder;
		ITopLevelDataRepo<RefCusTariff> dataRepo;

		static BuildersFilePath[] TariffBuilderFilePaths { get; } = [
			new(Path.GetTempFileName(), BuilderFilePathSymbol.TariffDetail),
			new(Path.GetTempFileName(), BuilderFilePathSymbol.TariffOverride)
		];

		static IDateProvider DateProvider { get; } = Mock.Of<IDateProvider>(x => x.Today == new DateTime(2025, 1, 1) && x.ActiveDate == x.Today.AddYears(-5));

		static NZTariffProcessingData ProcessingData { get; } = new();

		static void CreateTariffBuilderFiles(string[] tariffDetails, string tariffOverride = null)
		{
			if (tariffDetails != null && tariffDetails.Any())
			{
				TariffTestHelper.CreateTestFile(tariffDetails, TariffBuilderFilePaths.FirstOrDefault(f => f.Symbol == BuilderFilePathSymbol.TariffDetail), string.Empty);
			}
			
			if (!string.IsNullOrEmpty(tariffOverride))
			{
				var overrideFilePath = TariffBuilderFilePaths.FirstOrDefault(f => f.Symbol == BuilderFilePathSymbol.TariffOverride)?.FilePath;
				if (overrideFilePath != null)
				{
					File.WriteAllText(overrideFilePath, tariffOverride);
				}
			}
		}
	}
}
