using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.MasterFiles.Business.OrgPatternMatching.Testing
{
	sealed class PatternMatchRowBuilderTest : TestCaseWithFactory
	{
		public void TestConstructorChecksForNulls()
		{
			var mockPatternMatch = new Mock<IOrgPatternMatch>();

			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{ new PatternMatchRowBuilder(null); });
			AssertNoExceptionThrown(delegate
			{ new PatternMatchRowBuilder(mockPatternMatch.Object); });
		}

		public void TestCompanyNameTruncatedWhenExceedMaximumLength()
		{
			var mockPatternMatch = Factory.NewWithValidTestData<OrgPatternMatch>();
			var patternMatchRowBuilder = new PatternMatchRowBuilder(mockPatternMatch);
			var longCompanyName = "L".PadRight(OrgPatternMatchSchema.OS_FullCompanyName.MaxLength + 1, 'O');
			var longCompanyNameWithLanguage = new StringWithLanguage(longCompanyName, "EN");
			var patternMatchGenerationHelper = OrgPatternMatchGenerationHelper.Get("EN", string.Empty, (NoResString)string.Empty);
			patternMatchRowBuilder.EncodeName(longCompanyNameWithLanguage, patternMatchGenerationHelper, true);

			AssertEquals(longCompanyName.Substring(0, OrgPatternMatchSchema.OS_FullCompanyName.MaxLength), mockPatternMatch.OS_FullCompanyName);
		}
	}
}
