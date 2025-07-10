using System;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.ITReferenceData.Business.AdditionalCode;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ITReferenceData.Test.AdditionalCode
{
	[TestFixture]
	class NationalRawAdditionalCodeRequestObjectParametersFixture
	{
		[Test]
		public void ConstructorGuardClause()
		{
			Assert.Throws<ArgumentNullException>(() => NationalRawAdditionalCodeRequestObjectParameters.Build(regexMatch: null), "When regexMatch is null");
		}

		[Test]
		public void Build()
		{
			var htmlTestFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"AdditionalCode\TestFiles\NationalAdditionalCodeListOfTypeQ.html");
			var rawHtml = File.ReadAllText(htmlTestFile);
			var matches = Regex.Matches(rawHtml, Constants.Regex.AdditionalCodeLink);
			var parameters = NationalRawAdditionalCodeRequestObjectParameters.Build(matches[0]);
			Assert.Multiple(() =>
			{
				Assert.AreEqual("11", parameters.UC, nameof(parameters.UC));
				Assert.AreEqual("1", parameters.SC, nameof(parameters.SC));
				Assert.AreEqual("-1", parameters.ST, nameof(parameters.ST));
				Assert.AreEqual("102", parameters.Label, nameof(parameters.Label));
				Assert.AreEqual("001", parameters.AdditionalCodeSequentialNumber, nameof(parameters.AdditionalCodeSequentialNumber));
				Assert.AreEqual("Q", parameters.AdditionalCodeType, nameof(parameters.AdditionalCodeType));
				Assert.AreEqual("01/07/2003", parameters.ValidityStartDate, nameof(parameters.ValidityStartDate));
				Assert.AreEqual("", parameters.SidCad, nameof(parameters.SidCad));
			});
		}
	}
}
