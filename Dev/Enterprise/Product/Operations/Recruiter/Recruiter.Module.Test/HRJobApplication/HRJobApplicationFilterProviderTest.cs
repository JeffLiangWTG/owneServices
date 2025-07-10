using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(HRJobApplicationFilterProvider))]
	public class HRJobApplicationFilterProviderTest : FilterStripBusinessObjectTestCase
	{
		public void TestReferringParty()
		{
			var provider = new HRJobApplicationFilterProvider();
			AssertNotNull(provider.ReferringOrganisations);
			AssertNotNull(provider.ReferringPersons);
			AssertNotNull(provider.ReferringStaffs);
		}

		#region Implementation
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new HRJobApplicationFilterProvider();
		}
		#endregion
	}
}
