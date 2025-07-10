using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.HRM.Module.Test
{
	[TestedType(typeof(ReviewProcessNodeFilterStripBusinessObject))]
	public class ReviewProcessNodeFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new ReviewProcessNodeFilterStripBusinessObject();
	}
}
