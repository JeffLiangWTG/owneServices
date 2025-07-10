using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.TRReferenceData.Business;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs
{
	[TestFixture]
	class HsnTariffDTYRatesProcessorTest
	{
		[TestCaseSource(nameof(TestCases))]
		public void TestProcess(dynamic testCaseObject)
		{
			var tariff = new RefCusTariff { ZZ1_TariffCode = testCaseObject.ZZ1_TariffCode, RefCusRates = Array.Empty<RefCusRate>() };
			HsnTariffDTYRatesProcessor.AttachDTYRates(new[] { tariff });

			var expectedRates = ((JArray)testCaseObject.RefCusRates).ToList();
			var actualRates = tariff.RefCusRates;

			var unmatchedExpectedRates = expectedRates.FindNonMatchingElements(actualRates, TestHelper.AreEquivalent);
			if (unmatchedExpectedRates.Any())
			{
				var messageBuilder = new StringBuilder($"ZZ1_TariffCode: {tariff.ZZ1_TariffCode}, Expecting a RefCusRate like below but not found: {Environment.NewLine}");
				foreach (var unmatchedExpectedRate in unmatchedExpectedRates)
				{
					messageBuilder.AppendLine(unmatchedExpectedRate.ToString(formatting: Formatting.Indented));
				}
				Assert.Fail(messageBuilder.ToString());
			}
		}

		static IEnumerable<TestCaseData> TestCases()
		{
			return TestCasesFromJson().Select(testCase =>
			{
				var testCaseData = new TestCaseData(testCase);
				testCaseData.SetArgDisplayNames($"ZZ1_TariffCode: {testCase.ZZ1_TariffCode}");
				return testCaseData;
			});
		}

		static IEnumerable<dynamic> TestCasesFromJson()
		{
			var testCasesText = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.TRReferenceData.Tests.Tariffs.HsnTariffDTYRatesProcessorTestCases.json");
			return JsonConvert.DeserializeObject<dynamic>(testCasesText).Cases;
		}
	}
}
