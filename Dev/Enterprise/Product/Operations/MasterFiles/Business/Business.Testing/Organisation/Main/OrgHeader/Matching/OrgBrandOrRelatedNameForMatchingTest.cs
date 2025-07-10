using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgBrandOrRelatedNameForMatchingTest : TestCase
	{
		public void TestP1_RelatedNameOriginal()
		{
			OrgBrandOrRelatedNameForMatching brand = new OrgBrandOrRelatedNameForMatching();
			AssertEquals("", brand.P1_RelatedName);
			AssertEquals("", brand.P1_RelatedNameOriginal);
			brand.P1_RelatedName = "aaa";
			AssertEquals("aaa", brand.P1_RelatedName);
			AssertEquals("aaa", brand.P1_RelatedNameOriginal);
			brand.P1_RelatedName = "bbb";
			AssertEquals("bbb", brand.P1_RelatedName);
			AssertEquals("bbb", brand.P1_RelatedNameOriginal);
		}
	}
}
