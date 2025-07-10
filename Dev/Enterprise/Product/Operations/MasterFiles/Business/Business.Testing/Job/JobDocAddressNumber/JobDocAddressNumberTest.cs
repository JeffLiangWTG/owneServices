using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobDocAddressNumber))]
	sealed class JobDocAddressNumberTest : ZArchitecture.Business.Testing.EnterpriseBusinessObjectTestCase
	{
		public void TestSupportsClone()
		{
			var jobDocAddressNumber = Factory.New<JobDocAddressNumber>();
			AssertEquals("Should supports clone", true, jobDocAddressNumber.SupportsClone());
		}

		public void TestGetPropertiesToExcludeFromCloning()
		{
			var jobDocAddressNumber = Factory.New<JobDocAddressNumberForTest>();
			AssertContainsExactElementsInAnyOrder("E2N_E2 should be excluded from cloning", new[]
			{
				JobDocAddressNumberSchema.Constants.E2N_E2
			}, jobDocAddressNumber.GetPropertiesToExcludeFromCloning_Exposed());
		}

		public void TestE2N_E2ReadOnly()
		{
			var jobDocAddressNumber = Factory.New<JobDocAddressNumberForTest>();
			AssertEquals("E2N_E2 should be read-only to avoid creating new JobDocAddress when saving", true, jobDocAddressNumber.E2N_E2Info.ReadOnly);
		}
	}
}
