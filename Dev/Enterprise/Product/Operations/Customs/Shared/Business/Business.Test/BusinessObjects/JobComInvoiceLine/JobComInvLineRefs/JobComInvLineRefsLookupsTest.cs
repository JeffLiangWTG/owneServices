using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.Testing
{
	class JobComInvLineRefsLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestReferenceTypeList()
		{
			CombineAssertions(() =>
			{
				var referenceTypeList = lookups.ReferenceTypeList;
				AssertEquals("Values", string.Empty, referenceTypeList.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<CodeDescriptionPairList>(), referenceTypeList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var jobComInvLineRefs = Factory.New<JobComInvLineRefs>();
			lookups = new JobComInvLineRefsLookups(jobComInvLineRefs);
		}
		JobComInvLineRefsLookups lookups;
	}
}
