using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(MQEDIMessageWithRelatedMessageDetailsForm))]
	sealed class MQEDIMessageWithRelatedMessageDetailsFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var bizO = Factory.New<MQEDIMessage>();
			bizO.EM_ReceiveTransmit = EDIMessage.Status.Received;
			bizO.ClearHasChanges();
			return new MQEDIMessageWithRelatedMessageDetailsForm(bizO);
		}
	}
}
