using System.Collections.Generic;
using System.Globalization;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.INReferenceData.Business;
using CargoWise.RefDbRepo.INReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.INReferenceData.Tests
{
	sealed class TariffDataValidatorTest
	{
		[Test]
		public void TestValidateChapter()
		{
			var tariffDictionary = new Dictionary<string, List<RefCusTariff>>();
			for (var i = 1; i <= 98; i++)
			{
				if (i == 4 || i == 77)
				{
					continue;
				}
				var chapter = i.ToString("D2", CultureInfo.InvariantCulture);
				tariffDictionary[chapter] = new List<RefCusTariff>();
			}
			Validator.ValidateChapter(tariffDictionary);
			LoggerMock.Verify(l => l.Log(LogType.ReviewRequired, "Missing chapters in Tariff data", "Chapters: 04, 77"), Times.Once);
		}

		[Test]
		public void TestValidate_NoData()
		{
			var tariffData = new List<TariffDataItem>();
			Validator.Validate(tariffData);
			LoggerMock.Verify(l => l.Log(LogType.ReviewRequired, "No data found in PDF", It.IsAny<string>()), Times.Once);
		}

		[Test]
		public void TestValidate_InvalidData()
		{
			Assert.Multiple(() =>
			{
				var tariffData = new List<TariffDataItem>();
				var validTariffItem = GetTariff("1234 56 78");
				tariffData.Add(validTariffItem);
				LoggerMock.Reset();
				Validator.Validate(tariffData);
				LoggerMock.Verify(l => l.Log(LogType.Info, "Validating: Row data", It.IsAny<object>()), Times.Once);

				var invalidTariffItem = new TariffDataItem
				{
					TariffItem = "123",
					Hyphens = "---",
					Description = "",
					Unit = "kg",
					StandardRate = "70%",
					PreferentialRate = "40%"
				};
				tariffData.Add(invalidTariffItem);
				Validator.Validate(tariffData);
				LoggerMock.Verify(l => l.Log(LogType.Warning, "Possible Invalid data", It.IsAny<object>()), Times.Once);
				LoggerMock.Verify(l => l.Log(LogType.ReviewRequired, "Invalid data", It.IsAny<object>()), Times.Once);
			});
		}

		[Test]
		public void TestValidate_DuplicateData()
		{
			Assert.Multiple(() =>
			{
				var tariffData = new List<TariffDataItem>();
				var validTariffItem = GetTariff("1234 56 78");

				var emptyTariffItem = new TariffDataItem
				{
					TariffItem = "",
					Hyphens = "-",
					Description = "Tariff Description",
					Unit = "",
					StandardRate = "",
					PreferentialRate = ""
				};
				tariffData.Add(validTariffItem);
				tariffData.Add(validTariffItem);
				tariffData.Add(emptyTariffItem);
				tariffData.Add(emptyTariffItem);
				Validator.Validate(tariffData);
				LoggerMock.Verify(l => l.Log(LogType.ReviewRequired, "Duplicate TariffItem", It.IsAny<string>()), Times.Once);
			});
		}

		[Test]
		public void TestValidate_TypoData()
		{
			Assert.Multiple(() =>
			{
				var tariffData = new List<TariffDataItem>();

				for (var i = 0; i < 5; i++)
				{
					tariffData.Add(GetTariff("1234 56 78"));
					tariffData.Add(GetTariff(""));
				}
				tariffData.Add(GetTariff("1301"));
				tariffData.Add(GetTariff("1302"));
				tariffData.Add(GetTariff("1402"));
				Validator.Validate(tariffData, "chap99");
				LoggerMock.Verify(l => l.Log(LogType.ReviewRequired, "Possible typos in chap99 with tariff codes 1301, 1302, 1402", It.IsAny<string>()), Times.Once);
			});
		}

		static TariffDataItem GetTariff(string tariffItem) => new TariffDataItem
		{
			TariffItem = tariffItem,
			Hyphens = "---",
			Description = "Tariff Description",
			Unit = "kg",
			StandardRate = "70%",
			PreferentialRate = "40%"
		};

		TariffDataValidator Validator => validator ??= new TariffDataValidator(LoggerMock.Object);
		TariffDataValidator validator;

		Mock<ILogger> LoggerMock => loggerMock ??= new Mock<ILogger>();
		Mock<ILogger> loggerMock;
	}
}
