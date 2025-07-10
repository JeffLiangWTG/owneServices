using System.Windows.Forms;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.GUI.Testing;

[TestedType(typeof(HVLVTestShipmentDataCreatorForm))]
public class HVLVTestShipmentDataCreatorFormTest : ZFormBasherTest
{
	protected override Form GetFormToBashCore()
	{
		var configuraion = new HVLVTestDataConfiguration(Factory.New<ForwardingConsol>());
		return new HVLVTestShipmentDataCreatorForm(configuraion);
	}
}
