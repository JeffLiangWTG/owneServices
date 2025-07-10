using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Customs.US.Module.Testing
{
	abstract class MQEDIMessageCommonFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestCurrentCompanyFilter()
		{
			var currentCompanyPK = GlbCompany.CurrentCompany.PK;
			var currentCompanyBranch1 = Factory.NewWithValidTestData<GlbBranch>();
			currentCompanyBranch1.GB_GC = currentCompanyPK;
			var currentCompanyBranch2 = Factory.NewWithValidTestData<GlbBranch>();
			currentCompanyBranch2.GB_GC = currentCompanyPK;
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			var company2Branch1 = Factory.NewWithValidTestData<GlbBranch>();
			company2Branch1.GB_GC = company2.PK;
			var message1 = GetNewMessageWithSpecificBranchToTest(currentCompanyBranch1);
			var message2 = GetNewMessageWithSpecificBranchToTest(currentCompanyBranch2);
			var message3 = GetNewMessageWithSpecificBranchToTest(company2Branch1);
			Factory.Save();
			var filterObj = GetNewFilterStripBusinessObject();
			Assert("message1 matches filter", message1.MatchesFilter(filterObj.Filter));
			Assert("message2 matches filter", message2.MatchesFilter(filterObj.Filter));
			Assert("message3 does not match filter", !message3.MatchesFilter(filterObj.Filter));
		}

		protected abstract MQEDIMessage GetNewMessageWithSpecificBranchToTest(GlbBranch branch);
	}
}
