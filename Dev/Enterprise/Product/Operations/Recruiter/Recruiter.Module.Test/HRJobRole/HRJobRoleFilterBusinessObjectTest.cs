using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(HRJobRoleFilterBusinessObject))]
	public class HRJobRoleFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Implementation
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new HRJobRoleFilterBusinessObject();
		}
		#endregion
	}
}
