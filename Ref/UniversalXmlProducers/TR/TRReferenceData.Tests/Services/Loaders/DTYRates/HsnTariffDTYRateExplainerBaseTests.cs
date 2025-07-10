using System.IO;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests.Services.Loaders
{
	public abstract class HsnTariffDTYRateExplainerBaseTests<TExplainer> where TExplainer : HsnTariffDTYRateExplainer
	{
		internal TExplainer Explainer;
		internal Mock<ILogger> LoggerMock;

		[SetUp]
		public void BaseSetUp()
		{
			LoggerMock = new Mock<ILogger>();
			Explainer = CreateExplainer();
		}

		protected abstract TExplainer CreateExplainer();

		protected string ExtractEmbeddedExcel(string resourceName)
		{
			var assembly = GetType().Assembly;
			using var stream = assembly.GetManifestResourceStream(resourceName);
			if (stream == null)
				Assert.Fail($"Embedded resource not found: {resourceName}");

			var tempFile = Path.GetTempFileName();
			using var outFile = File.OpenWrite(tempFile);
			stream.CopyTo(outFile);
			return tempFile;
		}

		[TestCase("0 (1)", true, "0 ", new[] { "1" })]
		[TestCase("0 (1)(2)", true, "0 ", new[] { "1", "2" })]
		[TestCase("(1)(2)", true, "", new[] { "1", "2" })]
		[TestCase("0 (1)(2)", false, null, new[] { "0", "1", "2" })]
		public void GetValueAndFootnotes_ShouldReturnExpected(string cellValue, bool isPreference, string expectedValue, string[] expectedFootnotes)
		{
			var (value, footnotes) = HsnTariffDTYRateExplainer.GetValueAndFootnotes(cellValue, isPreference);
			Assert.That(value, Is.EqualTo(expectedValue));
			Assert.That(footnotes, Is.EqualTo(expectedFootnotes));
		}

		[TestCase("The number in the XYZ column cell", "5", "The5")]
		[TestCase("Total amount is 0", "0", "0")]
		[TestCase("Some fixed value", "3", "Some fixed value")]
		public void GetFormula_ShouldReturnExpected(string formulaTemplate, string formulaValue, string expected)
		{
			var result = HsnTariffDTYRateExplainer.GetFormula(formulaTemplate, formulaValue, col => formulaValue);
			Assert.That(result, Is.EqualTo(expected));
		}
	}
}
