using System;
using System.Globalization;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests
{
	class HsnTariffEXCListDutyRateTest
	{
		[Test]
		public void ShouldInitializePropertiesCorrectly()
		{
			var expectedStartDate = DateTime.Parse("2024-01-01", CultureInfo.InvariantCulture);
			var expectedEndDate = DateTime.Parse("2024-12-31", CultureInfo.InvariantCulture);
			var expectedExemptedTariffCodes = new[] { "c1", "c2" };

			var dutyRate = new HsnTariffEXCListDutyRate(
				"tariffCode",
				"desc",
				"100",
				"uomCU3",
				"uomCU4",
				"uomCU5",
				expectedExemptedTariffCodes,
				"additional",
				"type",
				"code",
				"formula",
				"2024-01-01",
				"2024-12-31"
			);

			Assert.AreEqual("tariffCode", dutyRate.TariffNo);
			Assert.AreEqual("desc", dutyRate.Description);
			Assert.AreEqual("100", dutyRate.DutyAmount);
			Assert.AreEqual("uomCU3", dutyRate.UomCU3);
			Assert.AreEqual("uomCU4", dutyRate.UomCU4);
			Assert.AreEqual("uomCU5", dutyRate.UomCU5);
			Assert.AreEqual(expectedExemptedTariffCodes, dutyRate.ExemptedTariffCodes);
			Assert.AreEqual("additional", dutyRate.AdditionalCode);
			Assert.AreEqual("type", dutyRate.RateType);
			Assert.AreEqual("code", dutyRate.RateCode);
			Assert.AreEqual("formula", dutyRate.RateFormula);
			Assert.AreEqual(expectedStartDate, dutyRate.StartDate);
			Assert.AreEqual(expectedEndDate, dutyRate.EndDate);
		}

		[Test]
		public void ShouldSetMinAndMaxValuesWhenEmpty()
		{
			var dutyRate = new HsnTariffEXCListDutyRate(
				"tariffCode",
				"desc",
				"100",
				"uomCU3",
				"uomCU4",
				"uomCU5",
				new[] { "c1", "c2" },
				"additional",
				"type",
				"code",
				"formula",
				"",
				null
			);

			Assert.AreEqual(DateTime.MinValue, dutyRate.StartDate);
			Assert.AreEqual(DateTime.MaxValue, dutyRate.EndDate);
		}
	}
}
