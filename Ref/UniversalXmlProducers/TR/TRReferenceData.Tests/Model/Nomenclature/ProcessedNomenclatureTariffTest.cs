using CargoWise.RefDbRepo.TRReferenceData.Services.Models;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests
{
	class ProcessedNomenclatureTariffTest
	{
		[TestCase("123456789012", true)]
		[TestCase("12345", false)]
		[TestCase("", false)]
		[TestCase("1234567890123456", false)]
		public void IsTariff(string code, bool expectedResult)
		{
			var tariff = new ProcessedNomenclatureTariff { Code = code };
			var result = tariff.IsTariff;
			Assert.AreEqual(expectedResult, result, $"Expected IsTariff to be {expectedResult} for code '{code}'.");
		}
	}
}
