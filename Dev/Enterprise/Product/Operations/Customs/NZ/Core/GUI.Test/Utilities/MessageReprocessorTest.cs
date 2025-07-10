using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NZ.GUI.Testing
{
	public class MessageReprocessorTest : TestCaseWithFactory
	{
		public void TestReprocessMessageNull()
		{
			MessageReprocessor.CanReprocessMessage(null, null);
			AssertEquals("Please select a valid Message.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestReprocessMessageTransmit()
		{
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			MessageReprocessor.CanReprocessMessage(message, null);
			AssertEquals("Cannot reprocess this message. Only response messages can be reprocessed.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestReprocessMessageReceive()
		{
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			MessageReprocessor.CanReprocessMessage(message, message);
			AssertNull("Message Is reprocessed", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestQueueMessageForReprocess()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "00001234Z"))
			{
				message.QueueMessageForReprocess();
				AssertEquals(EDIMessage.Status.Queued, message.EM_Status);
				AssertNotNull(message.Logs.Find(x => x.SL_SE_NKEvent == ZArchitecture.Business.AutoEvents.MessagePendingProcessing.Code));
				Assert(message.IsInDatabase);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = Factory.New<TSWMessage>();
		}

		TSWMessage message;
	}
}
