using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(SlotAllocationAspectCollection))]
	sealed class SlotAllocationAspectCollectionTest : ActiveBusinessObjectCollectionTestCase<SlotAllocationAspectCollection>
	{
		public void TestFindByCode()
		{
			SlotAllocation allocation = Factory.New<SlotAllocation>();

			SlotAllocationAspect aspect1 = allocation.Aspects.AddNew();
			aspect1.D5_Type = "AS1";

			SlotAllocationAspect aspect2 = allocation.Aspects.AddNew();
			aspect2.D5_Type = "AS2";

			SlotAllocationAspect aspect3 = allocation.Aspects.AddNew();
			aspect3.D5_Type = "AS3";

			AssertEquals("AS1", aspect1, allocation.Aspects["AS1"]);
			AssertEquals("AS2", aspect2, allocation.Aspects["AS2"]);
			AssertEquals("AS3", aspect3, allocation.Aspects["AS3"]);
		}

		protected override SlotAllocationAspectCollection GetCollectionToTest()
		{
			return Factory.New<SlotAllocation>().Aspects;
		}
	}
}
