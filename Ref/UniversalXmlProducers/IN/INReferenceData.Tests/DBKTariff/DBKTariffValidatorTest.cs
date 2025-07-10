using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.INReferenceData.Business;
using CargoWise.RefDbRepo.INReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.INReferenceData.Tests
{
	public class DBKTariffValidatorTest
	{
		[Test]
		public void TestValidate_NoData()
		{
			var tariffData = new List<DBKTariff>();
			Validator.ValidateAndRemoveInvalidData(tariffData);
			LoggerMock.Verify(l => l.Log(LogType.ReviewRequired, "No data found in PDF", It.IsAny<string>()), Times.Once);
		}

		[Test]
		public void TestValidate_InvalidData()
		{
			Assert.Multiple(() =>
			{
				var tariffData = new List<DBKTariff>();
				var validTariffItem = new DBKTariff
				{
					TariffCode = "1234",
					TariffDescription = "Tariff Description",
					Unit = "kg"
				};
				tariffData.Add(validTariffItem);
				var invalidTariffItem = new DBKTariff
				{
					TariffCode = "123",
					TariffDescription = "",
					Unit = "ab"
				};
				tariffData.Add(invalidTariffItem);
				LoggerMock.Reset();
				var result = Validator.ValidateAndRemoveInvalidData(tariffData);
				LoggerMock.Verify(l => l.Log(LogType.ReviewRequired, @"Invalid TariffItem, Code: 123,
Error: The tariff number is not a four-digit or six-digit number.
The tariff description is empty.
No matching unit can be found.
", It.IsAny<object>()), Times.Once);
				Assert.AreEqual(1, result.Count, "Invalid TariffItem should be removed from the list");
				Assert.AreEqual("1234", result[0].TariffCode, "Valid TariffItem should remain in the list");
			});
		}


		DBKTariffValidator Validator => validator ?? (validator = new DBKTariffValidator(LoggerMock.Object));
		DBKTariffValidator validator;

		Mock<ILogger> LoggerMock => loggerMock ?? (loggerMock = new Mock<ILogger>());
		Mock<ILogger> loggerMock;
	}
}
