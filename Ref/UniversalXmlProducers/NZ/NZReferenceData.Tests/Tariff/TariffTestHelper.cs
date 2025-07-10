using System;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.NZReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NZReferenceData.Tests.Tariff
{
	static class TariffTestHelper
	{
		public static void CreateTestFile(string[] lines, BuildersFilePath filePath, string header)
		{
			var tempFilePath = filePath.FilePath;
			File.WriteAllLines(tempFilePath, header != null ? new[] { header }.Concat(lines) : lines);
		}

		public static RefCusTariff CreateTestTariff(string tariffCode, RefCusTariffUOM[] refCusTariffUOMs, DateTime startDate, DateTime endDate, RefCusRate[] refCusRates = null)
		{
			return new RefCusTariff
			{
				ZZ1_TariffCode = tariffCode,
				RefCusTariffUOMs = refCusTariffUOMs,
				ZZ1_StartDate = startDate,
				ZZ1_EndDate = endDate,
				RefCusRates = refCusRates
			};
		}

		public static RefCusRate CreateTestCusRate(string formula, string preference, bool createApplicability, DateTime applicabilityStart, DateTime applicabilityEnd)
		{
			return new RefCusRate
			{
				ZZ2_RateFormula = formula,
				ZZ2_ZZS_NKPreference = preference,
				RefCusApplicabilities = createApplicability ?
				[
					new RefCusApplicability
					{
						ZZT_ZZA_NKTradeGroup = preference,
						ZZT_StartDate = applicabilityStart,
						ZZT_EndDate = applicabilityEnd
					}
				] : []
			};
		}

		public static void AssertHasApplicability(RefCusRate rate, string expectedTradeGroup, DateTime expectedStartDate, DateTime expectedEndDate, string message = null)
		{
			Assert.That(
				rate.RefCusApplicabilities.Any(app =>
					app.ZZT_ZZA_NKTradeGroup == expectedTradeGroup &&
					app.ZZT_StartDate == expectedStartDate &&
					app.ZZT_EndDate == expectedEndDate),
				Is.True,
				message ?? $"Expected at least one RefCusApplicability with trade group '{expectedTradeGroup}' and matching start and end dates."
			);
		}
	}
}
