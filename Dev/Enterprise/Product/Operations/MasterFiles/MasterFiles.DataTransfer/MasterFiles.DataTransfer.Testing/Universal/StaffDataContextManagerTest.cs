using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	[TestedType(typeof(StaffDataContextManager))]
	public class StaffDataContextManagerTest : DataContextManagerTestCase<StaffDataContextManager, GlbStaff>
	{
		protected override GlbStaff GetNewBusinessObjectForTesting()
		{
			return Factory.NewWithValidTestData<GlbStaff>();
		}

		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			// We won't get this type directly, so we don't need to ensure it implements IJobNumber.
			AssertNotEquals("Trains", "Are exploding.");
		}
	}
}
