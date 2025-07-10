using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(PrepareDispatchInstructionForm))]
	sealed class PrepareDispatchInstructionFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new PrepareDispatchInstructionForm(new ConsolPrepareForDispatchInstruction(
				Factory.NewWithValidTestData<ForwardingConsol>(),
				TransitWarehouseInstructionHelper.Direction.Pickup));
		}
	}
}
