using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Confirmations.GUI.Testing
{
	[TestedType(typeof(DeliveryConfirmControlTestForm))]
	sealed class DeliveryConfirmControlTestFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new DeliveryConfirmControlTestForm(Factory.New<CommonShipment>());
		}
	}
}
