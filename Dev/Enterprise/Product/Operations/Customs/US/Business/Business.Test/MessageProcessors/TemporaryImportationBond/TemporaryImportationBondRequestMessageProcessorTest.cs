using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class TemporaryImportationBondRequestMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcess()
		{
			DeclarationTestHelper.SetupForSendMessage();
			outgoingMessage.EM_SystemCreateUser = DeclarationTestHelper.CreateStaff(Factory).GS_Code;
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			incomingMessage.Reload();
			AssertEquals("message is processed", "RCV", incomingMessage.EM_Status);
			AssertEquals("MessageSubType is set", EM_MessageSubTypeList.Codes.TemporaryImportationBondRequestToExtend, incomingMessage.EM_MessageSubType);
			AssertEquals("message is linked to the entry", entryHeader.PK, incomingMessage.EM_LinkedObject.PK);
			AssertNotNull("An email with subject containing 'Request for TIB Expiry Extension' should have been created.",
				Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject.Contains("Request for TIB Expiry Extension"); })));
		}

		public void TestProcessFailedResponse()
		{
			DeclarationTestHelper.SetupForSendMessage();
			outgoingMessage.EM_SystemCreateUser = DeclarationTestHelper.CreateStaff(Factory).GS_Code;
			outgoingMessage.EM_MessageText = "B018888XJ5XS                                               ~15000             XA8888XJ500001095                                                               XO8888XJ500001095 8WANO ENTRY EXISTS OR ENTRY CLOSED                            XO8888XJ500001095 8WITRANSACTION DATA REJECTED                                  Y  8888XJ5XS00003";
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			incomingMessage.Reload();
			AssertEquals("message is processed", "RCV", incomingMessage.EM_Status);
			AssertEquals("MessageSubType is set", EM_MessageSubTypeList.Codes.TemporaryImportationBondRequestToExtend, incomingMessage.EM_MessageSubType);
			AssertEquals("message is linked to the entry", entryHeader.PK, incomingMessage.EM_LinkedObject.PK);
			AssertNotNull("An email with subject containing 'Request for TIB Expiry Extension' should have been created.",
				Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
				{ return emailToMatched.Subject.Contains("Request for TIB Expiry Extension"); })));
		}

		JobDeclaration dec;
		CusEntryHeader entryHeader;

		MQEDIMessage outgoingMessage;
		MQEDIMessage incomingMessage;

		protected override void SetUp()
		{
			base.SetUp();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			entryHeader = dec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader.EntryNumber = "~23456789";
			dec.MergeManager.DisablePreSaveMergeRequirementForTesting();

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			outgoingMessage = mock.Object;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageNum = "~15000";
			outgoingMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.TemporaryImportationBondRequestToExtend;
			entryHeader.Messages.Add(outgoingMessage);

			incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageNum = "~15000";
			incomingMessage.EM_MessageText = "B018888XJ5XS                                               ~15000               " +
"XO8888XJ5~23456789   ACCEPTED".PadRight(80) +
"Y  8888XJ5X100007000000000000000000000000";
			incomingMessage.EM_Status = MQEDIMessage.Status.Queued;
			Factory.Save();
		}
	}
}
