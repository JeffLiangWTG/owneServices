using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(SlotAllocationAspect))]
	sealed class SlotAllocationAspectTest : EnterpriseBusinessObjectTestCase
	{
		public void TestClone()
		{
			SlotAllocationAspect aspect = Factory.New<SlotAllocation>().Aspects.AddNew();
			aspect.D5_Type = "TPE";
			aspect.D5_Value = 6;

			SlotAllocationAspect clone = (SlotAllocationAspect)aspect.Clone();
			AssertEquals("Type", "TPE", clone.D5_Type);
			AssertEquals("Value", 6m, clone.D5_Value);
			AssertEquals("Parent", ZGuid.Empty, clone.D5_E0);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = factory.NewWithValidTestData<SlotAllocation>();

			var now = ZDateTime.Now;
			result.E0_ParentID = ZGuid.NewZGuid();
			result.E0_ParentTableCode = "JX";

			return result;
		}
	}
}
