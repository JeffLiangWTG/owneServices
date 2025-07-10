using System.Windows.Forms;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(EDIMessageForm))]
sealed class EDIMessageFormTest : ZFormBasherTest
{
	public void TestEDIMessagFormContainsEDocsPlugIn()
	{
		var message = Factory.New<EDIMessage>();

		using var form = new EDIMessageForm(message);
		form.Show();
		AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn));
	}

	protected override Form GetFormToBashCore()
	{
		var message = Factory.New<EDIMessage>();
		message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		Factory.Save();
		return new EDIMessageForm(message);
	}
}
