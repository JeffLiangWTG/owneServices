using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.RefDbRepo.TRReferenceData.Services.Models;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests
{
	class HsnTariffSCDListIVDutyRateTest
	{
		[Test]
		public void ShouldInitializePropertiesCorrectly()
		{
			var expectedStartDate = DateTime.Parse("2024-01-01", CultureInfo.InvariantCulture);
			var expectedEndDate = DateTime.Parse("2024-12-31", CultureInfo.InvariantCulture);

			var dutyRate = new HsnTariffSCDListIVDutyRate(
				"pattern",
				"desc",
				1.1m,
				new List<string> { "c1", "c2" },
				"adc",
				"2024-01-01",
				"2024-12-31"
			);

			Assert.AreEqual("pattern", dutyRate.TariffCodePattern);
			Assert.AreEqual("desc", dutyRate.Description);
			Assert.AreEqual(1.1m, dutyRate.Rate);
			Assert.AreEqual(new List<string> { "c1", "c2" }, dutyRate.ExemptedTariffCodes);
			Assert.AreEqual("adc", dutyRate.AdditionalCode);
			Assert.AreEqual(expectedStartDate, dutyRate.StartDate);
			Assert.AreEqual(expectedEndDate, dutyRate.EndDate);
		}

		[Test]
		public void InvalidArguments()
		{
			string tariffCodePattern = "pattern";
			string description = "Valid Description";
			decimal rate = 1.0m;
			IEnumerable<string> tariffCodes = new List<string>();
			string additionalCode = "AdditionalCode";
			string startDate = DateTime.Now.ToString(CultureInfo.InvariantCulture);
			string endDate = DateTime.Now.AddDays(1).ToString(CultureInfo.InvariantCulture);


			var ex1 = Assert.Throws<ArgumentNullException>(() => new HsnTariffSCDListIVDutyRate(null, description, rate, tariffCodes, additionalCode, startDate, endDate));
			Assert.That(ex1.ParamName, Is.EqualTo("tariffCodePattern"));

			var ex2 = Assert.Throws<ArgumentException>(() => new HsnTariffSCDListIVDutyRate(string.Empty, description, rate, tariffCodes, additionalCode, startDate, endDate));
			Assert.That(ex2.ParamName, Is.EqualTo("tariffCodePattern"));

			var ex3 = Assert.Throws<ArgumentNullException>(() => new HsnTariffSCDListIVDutyRate(tariffCodePattern, null, rate, tariffCodes, additionalCode, startDate, endDate));
			Assert.That(ex3.ParamName, Is.EqualTo("description"));

			var ex4 = Assert.Throws<ArgumentException>(() => new HsnTariffSCDListIVDutyRate(tariffCodePattern, string.Empty, rate, tariffCodes, additionalCode, startDate, endDate));
			Assert.That(ex4.ParamName, Is.EqualTo("description"));
		}
	}
}
