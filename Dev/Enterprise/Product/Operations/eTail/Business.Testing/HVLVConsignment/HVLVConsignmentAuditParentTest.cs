using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVConsignment))]
	public class HVLVConsignmentAuditParentTest : AuditParentTest<HVLVConsignment>
	{
		protected override HVLVConsignment NewTestAuditParent()
		{
			return Factory.New<HVLVConsignment>();
		}
	}
}
