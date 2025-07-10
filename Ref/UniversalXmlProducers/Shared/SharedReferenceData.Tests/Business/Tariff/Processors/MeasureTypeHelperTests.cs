using System.Collections.Generic;
using NUnit.Framework;
using static CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Helpers.Tests.TestHelperClasses;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors.Tests
{
	[TestFixture]
	class MeasureTypeHelperTests
	{
		[TestCase("No Match", "000", "")]
		[TestCase("Match 1", "001", "CC1")]
		[TestCase("Match 2", "002", "")]
		public void GetConditionClass(string description, string measureType, string expectedValue)
		{
			Assert.That(helper.GetConditionClass(measureType), Is.EqualTo(expectedValue), description);
		}

		[TestCase("No Match", "000", "")]
		[TestCase("Match 1", "001", "RT1")]
		[TestCase("Match 2", "002", "")]
		public void GetRateType(string description, string measureType, string expectedValue)
		{
			Assert.That(helper.GetRateType(measureType), Is.EqualTo(expectedValue), description);
		}

		[TestCase("No Match", "000", "")]
		[TestCase("Match 1", "001", "RC1")]
		[TestCase("Match 2", "002", "")]
		public void GetRateCode(string description, string measureType, string expectedValue)
		{
			Assert.That(helper.GetRateCode(measureType), Is.EqualTo(expectedValue), description);
		}

		[TestCase("No Match", "000", true, false)]
		[TestCase("Match 1", "001", true, false)]
		[TestCase("Match 2", "002", false, false)]
		[TestCase("No Match", "000", false, true)]
		public void ShouldProcessMeasureType(string description, string measureType, bool expectedValue, bool configuredOnly)
		{
			Assert.That(helper.ShouldProcessMeasureType(measureType, configuredOnly), Is.EqualTo(expectedValue), description);
		}

		[TestCase("No Match", "000", false)]
		[TestCase("Match 1", "001", false)]
		[TestCase("Match 2", "002", true)]
		public void IsSupplementaryUnit(string description, string measureType, bool expectedValue)
		{
			Assert.That(helper.IsSupplementaryUnit(measureType), Is.EqualTo(expectedValue), description);
		}

		[TestCase("No Match", "000", new string[] { null })]
		[TestCase("Match 1", "001", new string[] { "P1", "P2" })]
		[TestCase("Match 2", "002", new string[] { null })]
		public void GetPreferences(string description, string measureType, string[] expectedValues)
		{
			Assert.That(helper.GetPreferences(measureType), Is.EquivalentTo(expectedValues), description);
		}

		[TestCase("No Match", "000", new string[] { })]
		[TestCase("Match 1", "001", new string[] { "AP1", "AP2" })]
		[TestCase("Match 2", "002", new string[] { })]
		public void GetAuthorisedUsePreferences(string description, string measureType, string[] expectedValues)
		{
			Assert.That(helper.GetAuthorisedUsePreferences(measureType), Is.EquivalentTo(expectedValues), description);
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			provider = new MeasureMappingTestDataProvider();
			helper = new MeasureTypeHelper(provider);

			provider.MeasureTypeMappingsForTest = new Dictionary<string, MeasureTypeMapping>
			{
				{ "001", new MeasureTypeMapping{ ConditionClass = "CC1", RateType = "RT1", RateCode = "RC1", Skip = false, SupplementaryUnit = false, Preferences = new[] { "P1", "P2" }, AuthorisedUsePreferences = new[] { "AP1", "AP2" } } },
				{ "002", new MeasureTypeMapping{ Skip = true, SupplementaryUnit = true } }
			};
		}

		MeasureMappingTestDataProvider provider;
		MeasureTypeHelper helper;
	}
}
