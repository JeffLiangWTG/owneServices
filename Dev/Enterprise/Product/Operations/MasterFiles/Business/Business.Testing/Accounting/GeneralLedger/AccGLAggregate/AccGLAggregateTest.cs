using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccGLAggregate))]
	sealed class AccGLAggregateTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCanDelete()
		{
			Assert(!BusinessObject.CanDelete);
		}
	}
}
