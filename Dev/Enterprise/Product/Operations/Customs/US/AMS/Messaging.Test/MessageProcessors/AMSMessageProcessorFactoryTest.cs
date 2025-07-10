using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AMS.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	sealed class AMSMessageProcessorFactoryTest : TestCaseWithFactory
	{
		public void TestMessageFriendlyName()
		{
			var processor = new AMSMessageProcessorFactory(new LoggingInformation());
			AssertEquals("US Customs AMS Message Processor", processor.MessageFriendlyName);
		}

		public void TestApplicationCode()
		{
			var processor = new AMSMessageProcessorFactory(new LoggingInformation());
			AssertEquals(AMSEDIMessage.ApplicationCodes.AMS, processor.ApplicationCode);
		}

		public void TestProcessingGoodMessage()
		{
			var currentStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentStaff.GS_EmailAddress = "bob@where.com";
			var group = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			group.Staff.Add(currentStaff);
			var originalMessage = Factory.New<AMSEDIMessageTestClass>();
			originalMessage.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreate;
			originalMessage.EM_MessageText = "A " + EDIMessage.MessageNumberPlaceHolder + " B";
			originalMessage.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			originalMessage.EM_MessageNum = "DN1234567890";
			Factory.Save();
			var message = Factory.New<AMSEDIMessage>();
			message.EM_MessageNum = "DN1234567890";
			message.EM_MessageText = new OUTR01().Serialise();
			message.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateResponse;
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			new AMSMessageProcessorFactory(new LoggingInformation()).ProcessMessage(message);
			AssertEquals(AMSEDIMessage.Status.Discarded, message.EM_Status);
		}

		public void TestPreProcessing()
		{
			var currentStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentStaff.GS_EmailAddress = "bob@where.com";
			var group = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			group.Staff.Add(currentStaff);
			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_Code = "Z!Z";
			branch1.GB_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_Code = "Z!2";
			branch2.GB_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			var header1 = (CusInBondHeader)Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			header1.BH_GB = branch1.PK;
			var moveHeader1 = header1.MovementHeader;
			var header2 = (CusInBondHeader)Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			header2.BH_GB = branch2.PK;
			var moveHeader2 = header2.MovementHeader;
			var originalMessage = Factory.New<AMSEDIMessageTestClass>();
			originalMessage.EM_GB = branch1.PK;
			originalMessage.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreate;
			originalMessage.EM_MessageText = "A " + EDIMessage.MessageNumberPlaceHolder + " B";
			originalMessage.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			originalMessage.EM_MessageNum = "DN1234567890";
			originalMessage.EM_LinkedObject = moveHeader1;
			Factory.Save();
			var originalMessage2 = Factory.New<AMSEDIMessageTestClass>();
			originalMessage2.EM_GB = branch2.PK;
			originalMessage2.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreate;
			originalMessage2.EM_MessageText = "A " + EDIMessage.MessageNumberPlaceHolder + " B";
			originalMessage2.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			originalMessage2.EM_MessageNum = "DN1234567890";
			originalMessage2.EM_LinkedObject = moveHeader2;
			originalMessage2.EM_ApplicationCode = "K@K";
			var originalMessage3 = Factory.New<AMSEDIMessageTestClass>();
			originalMessage3.EM_GB = branch2.PK;
			originalMessage3.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreate;
			originalMessage3.EM_MessageText = "A " + EDIMessage.MessageNumberPlaceHolder + " B";
			originalMessage3.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			originalMessage3.EM_MessageNum = "DN1234567890";
			originalMessage3.EM_LinkedObject = moveHeader2;
			var originalMessage4 = Factory.New<AMSEDIMessageTestClass>();
			originalMessage4.EM_GB = branch2.PK;
			originalMessage4.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreate;
			originalMessage4.EM_MessageText = "A " + EDIMessage.MessageNumberPlaceHolder + " B";
			originalMessage4.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			originalMessage4.EM_MessageNum = "DN1234567891";
			originalMessage4.EM_LinkedObject = moveHeader2;
			Factory.Save();
			var message = Factory.New<AMSEDIMessageTestClass>();
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_MessageText = new OUTR01().Serialise();
			message.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateResponse;
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			var em_GBSetCount = 0;
			message.EM_GBInfo.ValueChanged += (object sender, EventArgs e) => em_GBSetCount++;
			var messageProcessorFactory = new AMSMessageProcessorFactory(new LoggingInformation());
			AssertEquals("RequiresPreProcessing", true, messageProcessorFactory.RequiresPreProcessing);
			messageProcessorFactory.ProcessMessage(message);
			AssertEquals("EM_GB should not changed as message number is empty", GlbBranch.CurrentBranch.PK, message.EM_GB);
			AssertNull("EM_LinkedObject should not changed as message number is empty", message.EM_LinkedObject);
			AssertEquals("EM_LinkTable should not changed as message number is empty", ZString.Empty, message.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID should not changed as message number is empty", ZGuid.Empty, message.EM_LinkUniqueID);
			AssertEquals("AfterNewObjectIsLinkedCalledCountForTesting should not changed as message number is empty", 0, message.AfterNewObjectIsLinkedCalledCountForTesting);
			AssertEquals("em_GBSetCount should not changed as message number is empty", 0, em_GBSetCount);
			message.EM_MessageNum = "DN1234567892";
			messageProcessorFactory.ProcessMessage(message);
			AssertEquals("EM_GB should not changed as no original message match", GlbBranch.CurrentBranch.PK, message.EM_GB);
			AssertNull("EM_LinkedObject should not changed as no original message match", message.EM_LinkedObject);
			AssertEquals("EM_LinkTable should not changed as no original message match", ZString.Empty, message.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID should not changed as no original message match", ZGuid.Empty, message.EM_LinkUniqueID);
			AssertEquals("AfterNewObjectIsLinkedCalledCountForTesting should not changed as no original message match", 0, message.AfterNewObjectIsLinkedCalledCountForTesting);
			AssertEquals("em_GBSetCount should not changed as no original message match", 0, em_GBSetCount);
			message.EM_MessageNum = "DN1234567890";
			messageProcessorFactory.ProcessMessage(message);
			AssertEquals("EM_GB should have changed as OriginalMessage was matched", branch1.PK, message.EM_GB);
			AssertSame("EM_LinkedObject should have changed as OriginalMessage was matched", moveHeader1, message.EM_LinkedObject);
			AssertEquals("EM_LinkTable should have changed as OriginalMessage was matched", moveHeader1.TableName, message.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID should have changed as OriginalMessage was matched", moveHeader1.PK, message.EM_LinkUniqueID);
			AssertEquals("AfterNewObjectIsLinkedCalledCountForTesting should have changed as OriginalMessage was matched", 1, message.AfterNewObjectIsLinkedCalledCountForTesting);
			AssertEquals("em_GBSetCount should have changed as OriginalMessage was matched", 1, em_GBSetCount);
			messageProcessorFactory.ProcessMessage(message);
			AssertEquals("EM_GB", branch1.PK, message.EM_GB);
			AssertSame("EM_LinkedObject", moveHeader1, message.EM_LinkedObject);
			AssertEquals("EM_LinkTable", moveHeader1.TableName, message.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID", moveHeader1.PK, message.EM_LinkUniqueID);
			AssertEquals("AfterNewObjectIsLinkedCalledCountForTesting should not have changed EM_LinkedObject was not changed", 1, message.AfterNewObjectIsLinkedCalledCountForTesting);
			AssertEquals("em_GBSetCount should not have changed as EM_GB was not changed", 1, em_GBSetCount);
		}

		public void TestPreProcessMessage()
		{
			var bo = Factory.New<DummyBusinessObject>();
			var currentStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentStaff.GS_EmailAddress = "bob@where.com";
			var group = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			group.Staff.Add(currentStaff);
			var originalMessage = Factory.New<AMSEDIMessageTestClass>();
			originalMessage.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreate;
			originalMessage.EM_MessageText = "A " + EDIMessage.MessageNumberPlaceHolder + " B";
			originalMessage.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			originalMessage.EM_MessageNum = "DN1234567890";
			originalMessage.EM_ApplicationReference = Guid.NewGuid().ToString();
			originalMessage.EM_LinkedObject = bo;
			Factory.Save();
			var message = Factory.New<AMSEDIMessage>();
			message.EM_MessageNum = "DN1234567890";
			message.EM_MessageText = new OUTR01().Serialise();
			message.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateResponse;
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			message.EM_GB = ZGuid.Empty;
			AssertEquals(ZString.Empty, message.EM_ApplicationReference);
			AssertEquals(null, message.EM_LinkedObject);
			new AMSMessageProcessorFactory(new LoggingInformation()).PreProcessMessage(message);
			AssertEquals(originalMessage.EM_GB, message.EM_GB);
			AssertEquals(originalMessage.EM_ApplicationReference, message.EM_ApplicationReference);
			AssertEquals(bo, message.EM_LinkedObject);
		}

		public void TestProcessingUnknownMessage()
		{
			var currentStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentStaff.GS_EmailAddress = "bob@where.com";
			var group = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			group.Staff.Add(currentStaff);
			Factory.Save();
			var message = Factory.New<AMSEDIMessage>();
			message.EM_MessageText = @"XYZ123".PadRight(80);
			message.EM_MessageType = "XX";
			message.EM_Status = AMSEDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			ErrorReporter.Clear();
			new AMSMessageProcessorFactory(new LoggingInformation()).ProcessMessage(message);
			AssertEquals(AMSEDIMessage.Status.Failed, message.EM_Status);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject == "Message Response (Failure)"));
			AssertEquals(true, email.Recipients.Contains("bob@where.com"));
		}

		sealed class AMSEDIMessageTestClass : AMSEDIMessage
		{
			public AMSEDIMessageTestClass(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public int AfterNewObjectIsLinkedCalledCountForTesting;
			protected override void AfterNewObjectIsLinked(BusinessObject newBizObj)
			{
				base.AfterNewObjectIsLinked(newBizObj);
				AfterNewObjectIsLinkedCalledCountForTesting++;
			}
		}
	}
}
