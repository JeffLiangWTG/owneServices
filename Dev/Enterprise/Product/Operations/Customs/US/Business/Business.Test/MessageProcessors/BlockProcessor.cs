using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	static class BlockProcessor
	{
		internal static OrgHeader ProcessBlock<ProcessorT>(BusinessObjectFactory factory, BlockControlGenerator block, string expectedSubject, string expectedBodyContains, bool linkedObjectIsNull = false)
			where ProcessorT : ACSABIProcessor, new()
		{
			ACSABIProcessor processor = new ProcessorT();
			MQEDIMessage outgoingMessage = factory.New<MQEDIMessage>();
			outgoingMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder + " B";
			factory.Save();
			GlbStaff staff = factory.New<GlbStaff>();
			staff.GS_Code = "XYZ";
			staff.GS_EmailAddress = "hello@hello";
			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;
			outgoingMessage.EM_LinkedObject = (linkedObjectIsNull) ? null : GlbCompany.CurrentCompany.OrgProxy;
			MQEDIMessage responseMessage = factory.New<MQEDIMessage>();
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageNum = outgoingMessage.EM_MessageNum;
			processor.Message = responseMessage;
			foreach (MessageBlock messageBlock in block.MessageBlocks)
			{
				processor.AddMessageBlock(messageBlock);
			}
			processor.Process();

			factory.Save();
			if (!linkedObjectIsNull)
			{
				NUnit.Framework.TestCase.AssertEquals(GlbCompany.CurrentCompany.OrgProxy.PK, responseMessage.EM_LinkUniqueID);
			}
			EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject == expectedSubject; }));
			NUnit.Framework.TestCase.AssertEquals(true, email.Body.Contains(expectedBodyContains));

			return GlbCompany.CurrentCompany.OrgProxy;
		}
	}
}
