using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class InboundMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcessMessage()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageType = MessageTypeList.Codes.DocumentReviewResponse;
			message.EM_MessageText = TestHelper.DocumentReviewRejectedResponseXml;
			Factory.Save();
			new InboundMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(EDIMessage.Status.Received, message.EM_Status);
		}

		public void TestProcessMessageWhenMessageTypeIsUnknown()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "Z~";
			var staff = group.Staff.AddNew();
			staff.GS_Code = "ZAC";
			staff.GS_LoginName = "~2";
			staff.GS_EmailAddress = "staff@pretendemail.com";
			USCustomsDataRegistry.Instance.DISMessagesGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, group.PK.ToGuid());
			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageType = "XXX";
			Factory.Save();
			var processor = new InboundMessageProcessor();
			processor.ExecuteBatch();
			message.Reload();
			AssertEquals(EDIMessage.Status.Queued, message.EM_Status);
			var log = processor.Logger.UserLogStrings.Cast<string>().FirstOrDefault(x => x.Contains("Cannot process: Unidentified message type:XXX"));
			AssertNotNull(log);
			AssertNotNull(Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.Contains("Error processing DIS message")));
		}
	}
}
