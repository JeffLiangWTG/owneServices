using System.Windows.Forms;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	[TestedType(typeof(AllocationAdjustmentDialog))]
	internal class AllocationAdjustmentDialogBasherTest : ZFormBasherTest
	{
		#region Implementation
		protected override Form GetFormToBashCore()
		{
			SlotAllocation allocation = Factory.New<SlotAllocation>();
			AllocationUsage required = new AllocationUsage();
			AllocationAdjustmentDetails details = new AllocationAdjustmentDetails(allocation, required);
			return new AllocationAdjustmentDialog(details);
		}
		#endregion
	}
}
