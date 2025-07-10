using System.Linq;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;
using Moq;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Services.Loaders
{

	[TestFixture]
	public class HsnTariffDTYRatePreferenceExplainerTest
		: HsnTariffDTYRateExplainerBaseTests<HsnTariffDTYRatePreferenceExplainer>
	{
		protected override HsnTariffDTYRatePreferenceExplainer CreateExplainer()
		{
			var path = ExtractEmbeddedExcel("CargoWise.RefDbRepo.TRReferenceData.Tests.Services.TestFiles.DTYRateExplanation.xlsx");
			return new HsnTariffDTYRatePreferenceExplainer(path, LoggerMock.Object);
		}

		[Test]
		public void AllRules_ShouldHaveAllRows()
		{
			var rules = Explainer.AllRules.ToList();
			Assert.That(rules, Is.Not.Null);
			Assert.That(rules.Count, Is.EqualTo(12));
			Assert.That(rules[0].Formula, Is.EqualTo("VFD*/100"));
			Assert.That(rules[10].CodeInExcel, Is.EqualTo("D-8"));
		}

		[Test]
		public void Explain_ShouldReturnRowBasedOnCodeInExcel()
		{
			string codeInExcel = "AB";
			string formulaValue = "(1)(2)";

			var results = Explainer.Explain(codeInExcel, formulaValue).ToList();
			Assert.That(results, Is.Not.Null);
			var rule = results[0];

			Assert.That(rule.CodeInExcel, Is.EqualTo("AB"));
			Assert.That(rule.Preference, Is.EqualTo("AT"));
			Assert.That(rule.Formula, Is.EqualTo("VFD*(1)(2)/100"));
			Assert.That(rule.RateType, Is.EqualTo("DTY"));
			Assert.That(rule.RateCode, Is.EqualTo("10"));
			Assert.That(rule.StartDate, Is.EqualTo(DtyRateConstants.DefaultStartDate));
			Assert.That(rule.EndDate, Is.EqualTo(DtyRateConstants.DefaultEndDate));
		}

		[Test]
		public void Explain_ShouldLogErrorWhenCodeNotPresent()
		{
			string codeInExcel = "AAAA";
			string formulaValue = "(7)";

			var results = Explainer.Explain(codeInExcel, formulaValue);
			Assert.That(results, Is.Null);
			LoggerMock.Verify(
					l => l.Log(
						LogLevel.Error,
						It.IsAny<EventId>(),
						It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Unable to find a rule for Code In Excel: {codeInExcel}")),
						It.IsAny<Exception>(),
						It.IsAny<Func<It.IsAnyType, Exception, string>>()
					),
					Times.Once
				);
		}
	}
}
