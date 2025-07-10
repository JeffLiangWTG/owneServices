using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(GlbAccreditationAttemptFilterProvider))]
	public class GlbAccreditationAttemptFilterProviderTest : FilterStripBusinessObjectTestCase
	{
		#region Implementation
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GlbAccreditationAttemptFilterProvider();
		}
		#endregion
	}
}
