using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	class WorkTaskRelatedItemHelperTest : TestCase
	{
		public void TestGetSelectionCriterionString_WithNoMatchingCodeInLookups_ShouldReturnOnlyCode()
		{
			var lookups = new CodeDescriptionPairList();
			var result = WorkTaskRelatedItemHelper.GetSelectionCriterionString(lookups, "AAA");
			AssertEquals("AAA", result);
		}

		public void TestGetSelectionCriterionString_WithMatchingCodeWithBlankDescription_ShouldReturnOnlyCode()
		{
			var lookups = new CodeDescriptionPairList { new CodeDescriptionPair("AAA", string.Empty) };
			var result = WorkTaskRelatedItemHelper.GetSelectionCriterionString(lookups, "AAA");
			AssertEquals("AAA", result);
		}

		public void TestGetSelectionCriterionString_WithCodeAndDescription_ShouldReturnFormattedCodeAndDescription()
		{
			var lookups = new CodeDescriptionPairList { new CodeDescriptionPair("AAA", "Fake Doors") };
			var result = WorkTaskRelatedItemHelper.GetSelectionCriterionString(lookups, "AAA");
			AssertEquals("AAA - Fake Doors", result);
		}
	}
}
