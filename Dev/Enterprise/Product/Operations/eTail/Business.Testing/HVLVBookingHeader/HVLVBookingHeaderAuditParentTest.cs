using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing;

[TestedType(typeof(HVLVBookingHeader))]
public class HVLVBookingHeaderAuditParentTest : AuditParentTest<HVLVBookingHeader>
{
	protected override HVLVBookingHeader NewTestAuditParent()
	{
		return Factory.New<HVLVBookingHeader>();
	}
}
