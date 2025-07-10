using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business.Tests
{
	public class DeduplicationOrgBrandOrRelatedNameTest : TestCaseWithFactory
	{
		public void TestConstructorSetRawNameCorrectly()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var relatedName = Factory.NewWithValidTestData<OrgBrandOrRelatedName>();

			org.BrandsOrRelatedNames.Add(relatedName);
			relatedName.P1_RelatedName = "ABC";

			var dedupRelatedName = new DeduplicationOrgBrandOrRelatedName(relatedName, new DeduplicationOrgHeader(org));
			AssertEquals("ABC", dedupRelatedName.P1_RelatedName);
			AssertEquals("ABC", dedupRelatedName.RawName);
		}
	}
}
