using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Confirmations.GUI.Testing
{
	[TestedType(typeof(PickupConfirmControlTestForm))]
	sealed class PickupConfirmControlTestFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new PickupConfirmControlTestForm(Factory.New<CommonShipment>());
		}
	}
}
