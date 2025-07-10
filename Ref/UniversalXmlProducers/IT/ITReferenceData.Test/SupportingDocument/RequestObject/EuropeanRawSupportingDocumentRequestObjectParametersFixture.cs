using System;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.SupportingDocument
{
	[TestFixture]
	class EuropeanRawSupportingDocumentRequestObjectParametersFixture

	{
		[Test]
		public void ConstructorGuardClause()
		{
			Assert.Throws<ArgumentNullException>(() => EuropeanRawSupportingDocumentRequestObjectParameters.Build(regexMatch: null), "When regexMatch is null");
		}

		[Test]
		public void Build()
		{
			var htmlTestFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"SupportingDocument\TestFiles\EuropeanSupportingDocumentList.html");
			var rawHtml = File.ReadAllText(htmlTestFile);
			var matches = Regex.Matches(rawHtml, Constants.Regex.EuropeanCertificateLink);
			var parameters = EuropeanRawSupportingDocumentRequestObjectParameters.Build(matches[0]);
			Assert.Multiple(() =>
			{
				Assert.AreEqual("01/01/1995", parameters.DescriptionValidityStartDate, nameof(parameters.DescriptionValidityStartDate));
				Assert.AreEqual("102", parameters.Label, nameof(parameters.Label));
				Assert.AreEqual("001", parameters.ProgressiveNumber, nameof(parameters.ProgressiveNumber));
				Assert.AreEqual("1", parameters.SC, nameof(parameters.SC));
				Assert.AreEqual("-1", parameters.ST, nameof(parameters.ST));
				Assert.AreEqual("A", parameters.Suffix, nameof(parameters.Suffix));
				Assert.AreEqual("7", parameters.UC, nameof(parameters.UC));
				Assert.AreEqual("", parameters.RegGrpCountryCode, nameof(parameters.RegGrpCountryCode));
			});
		}
	}
}
