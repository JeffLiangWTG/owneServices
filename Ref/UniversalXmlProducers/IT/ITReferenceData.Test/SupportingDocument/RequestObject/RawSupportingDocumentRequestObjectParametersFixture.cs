using System;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.ITReferenceData.Business.SupportingDocument;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.SupportingDocument
{
	[TestFixture]
	class RawSupportingDocumentRequestObjectParametersFixture
	{
		[Test]
		public void ConstructorGuardClause()
		{
			Assert.Throws<ArgumentNullException>(() => RawSupportingDocumentRequestObjectParameters.Build(regexMatch: null), "When regexMatch is null");
		}

		[Test]
		public void Build()
		{
			var htmlTestFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"SupportingDocument\TestFiles\NationalSupportingDocumentList.html");
			var rawHtml = File.ReadAllText(htmlTestFile);
			var matches = Regex.Matches(rawHtml, Constants.Regex.NationalCertificateLink);
			var parameters = RawSupportingDocumentRequestObjectParameters.Build(matches[0]);
			Assert.Multiple(() =>
			{
				Assert.AreEqual("31/05/2013", parameters.DescriptionValidityStartDate, nameof(parameters.DescriptionValidityStartDate));
				Assert.AreEqual("102", parameters.Label, nameof(parameters.Label));
				Assert.AreEqual("01", parameters.ProgressiveNumber, nameof(parameters.ProgressiveNumber));
				Assert.AreEqual("1", parameters.SC, nameof(parameters.SC));
				Assert.AreEqual("-1", parameters.ST, nameof(parameters.ST));
				Assert.AreEqual("AO", parameters.Suffix, nameof(parameters.Suffix));
				Assert.AreEqual("17", parameters.UC, nameof(parameters.UC));
			});
		}
	}
}
