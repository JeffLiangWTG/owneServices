using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AllocationAdjustmentDetails))]
	internal class AllocationAdjustmentDetailsBOTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			SlotAllocation allocation = Factory.New<SlotAllocation>();
			AllocationUsage required = new AllocationUsage();
			return new AllocationAdjustmentDetails(allocation, required);
		}
		#endregion
	}
}
