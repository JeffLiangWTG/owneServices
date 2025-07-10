using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbCapability))]
	sealed class GlbCapabilityTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAddingResourceToPivotTriggersHasChanges()
		{
			var capability = Factory.New<GlbCapability>();

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.Capabilities.Add(capability);

			Factory.Save();

			AssertEquals(1, capability.ResourcesWithCapability.Count);
			AssertEquals(false, capability.HasChanges);

			capability.ResourcesWithCapability.Add(staff2);

			AssertEquals(true, capability.HasChanges);
			AssertEquals(2, capability.ResourcesWithCapability.Count);
		}

		public void TestHumanReadableName_ShouldIncludeCode()
		{
			var capability = Factory.New<GlbCapability>();
			capability.G4_Code = "ABC";
			AssertEquals("Resource Capability - ABC", capability.HumanReadableName);
		}
	}
}
