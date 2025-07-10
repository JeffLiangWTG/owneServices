using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Models;
using NUnit.Framework;
using static CargoWise.RefDbRepo.ZAReferenceData.Services.Common.Constants;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff
{
	[TestFixture]
	internal class RuleMappingTests
	{
		[Test]
		public void IsMatch_TariffCode()
		{
			var ruleMapping = new RuleMapping { TariffCode = "", TariffType = null, CheckDigit = null };

			var isMatch = ruleMapping.IsMatch("", "", "");
			Assert.That(isMatch, Is.EqualTo(false));

			ruleMapping.TariffCode = "12345";
			isMatch = ruleMapping.IsMatch("", "", "");
			Assert.That(isMatch, Is.EqualTo(false));

			isMatch = ruleMapping.IsMatch("12345", "", "");
			Assert.That(isMatch, Is.EqualTo(true));

			isMatch = ruleMapping.IsMatch("1234567890", "", "");
			Assert.That(isMatch, Is.EqualTo(true));
		}

		[Test]
		public void IsMatch_TariffType()
		{
			var ruleMapping = new RuleMapping { TariffCode = "12345", TariffType = null, CheckDigit = null };

			var isMatch = ruleMapping.IsMatch("12345", "1P1", "");
			Assert.That(isMatch, Is.EqualTo(true));

			ruleMapping.TariffType = "17A";

			isMatch = ruleMapping.IsMatch("12345", "1P1", "");
			Assert.That(isMatch, Is.EqualTo(false));

			isMatch = ruleMapping.IsMatch("12345", "17A", "");
			Assert.That(isMatch, Is.EqualTo(true));
		}

		[Test]
		public void IsMatch_CheckDigit()
		{
			var ruleMapping = new RuleMapping { TariffCode = "12345", TariffType = null, CheckDigit = null };

			var isMatch = ruleMapping.IsMatch("12345", "1P1", "");
			Assert.That(isMatch, Is.EqualTo(true));

			isMatch = ruleMapping.IsMatch("12345", "1P1", "5");
			Assert.That(isMatch, Is.EqualTo(true));

			ruleMapping.CheckDigit = "5";

			isMatch = ruleMapping.IsMatch("12345", "1P1", "");
			Assert.That(isMatch, Is.EqualTo(false));

			isMatch = ruleMapping.IsMatch("12345", "1P1", "5");
			Assert.That(isMatch, Is.EqualTo(true));
		}

		[TestCase("XXX", "XXX")]
		[TestCase(null, "")]
		[TestCase(RuleSelectors.EUQuota, RateTypes.EUQuota)]
		[TestCase(RuleSelectors.EFTAQuota, RateTypes.EFTAQuota)]
		public void GetRateTypeFromSelector(string selector, string expected)
		{
			var result = RuleMapping.GetRateTypeFromSelector(selector);
			Assert.That(result, Is.EqualTo(expected), $"{selector} Mapping");
		}
	}
}
