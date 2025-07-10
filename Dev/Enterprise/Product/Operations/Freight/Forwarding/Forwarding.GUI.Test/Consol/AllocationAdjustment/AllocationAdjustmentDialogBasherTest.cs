using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(AllocationAdjustmentDialog))]
	internal class AllocationAdjustmentDialogBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			AllocationAdjustmentsSecurity adjustmentsSecurity = new AllocationAdjustmentsSecurity();
			return new AllocationAdjustmentDialog(adjustmentsSecurity);
		}

		#endregion
	}
}
