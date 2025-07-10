using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class MQEDIMessageWithRelatedMessageDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestSelectReceivedMessageDetailsTabWhenResponded()
		{
			var outgoing = Factory.New<MQEDIMessage>();
			outgoing.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoing.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoing.EM_MessageNum = "~123";
			using (var form = new MQEDIMessageWithRelatedMessageDetailsForm(outgoing))
			{
				form.Show();
				AssertEquals(0, form.MessageDetailsTabControlExposedForTest.SelectedIndex);
			}

			var response = Factory.New<MQEDIMessage>();
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_MessageNum = "~123";
			using (var form = new MQEDIMessageWithRelatedMessageDetailsForm(outgoing))
			{
				form.Show();
				AssertEquals(2, form.MessageDetailsTabControlExposedForTest.SelectedIndex);
			}
		}
	}
}
