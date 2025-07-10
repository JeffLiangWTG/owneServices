using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgHeaderForEnquiryMatching))]
	sealed class OrgHeaderForEnquiryMatchingTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPatterMatchesType()
		{
			var orgForMatching = Factory.New<OrgHeaderForEnquiryMatching>();
			AssertType(typeof(OrgPatternMatchCollectionForEnquiryMatching), orgForMatching.PatternMatchesForThisOrg);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var org = factory.New<OrgHeader>();
			org.OH_Code = "OrgForDelete";
			return org;
		}
	}
}
