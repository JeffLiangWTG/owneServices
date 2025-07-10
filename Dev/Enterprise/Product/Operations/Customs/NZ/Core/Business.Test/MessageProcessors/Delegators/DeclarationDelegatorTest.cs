using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.MessageProcessors.Testing
{
	class DeclarationDelegatorTest : TransactionedTestCase
	{
		public void TestResetMessageToQueuedAllowsFixForMismatchedMessageWhenUserHasChangedDeclarationToFormalToECIThenBackAgain()
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			JobDeclaration declaration = factory1.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00010000";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;

			CusEntryHeader formalHeader = declaration.CusEntryHeader;
			NZCMessage formalSentMessage = formalHeader.Messages.AddNew();
			formalSentMessage.EM_ReceiveTransmit = NZCMessage.Direction.Receive;

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			CusEntryHeader eciHeader = declaration.CusEntryHeader;
			AssertEquals("Precondition: declaration.CustomsEntryHeaders.Count", 2, declaration.CustomsEntryHeaders.Count);
			AssertNotEquals("formalHeader and eciHeader should be 2 different headers", formalHeader, eciHeader);

			NZCMessage incomingResponseMessage = factory1.New<NZCMessage>();
			incomingResponseMessage.EM_ReceiveTransmit = NZCMessage.Direction.Receive;
			incomingResponseMessage.EM_MessageText = @"
UNH+293602+CUSRES:D:96B:UN+B00010000'
BGM+932+47975057:01'
FTX+DIN+++2 FCL(S) SAID TO CONTAIN 258 PACKAGE(S) OR ITEM(S)'
GIS+819:120:143'
TAX+4+TOT'
MOA+161:11319.5'
GIS+B:134:143'
UNT+8+293602'
".Replace("\r", "").Replace("\n", "");

			AssertNotNull("Precondition: incomingResponseMessage.MessageAsCUSRESD98A", incomingResponseMessage.MessageAsCUSRESD98A);

			DeclarationDelegator delegator1 = new DeclarationDelegator();
			AssertEquals("delegator.CanProcess(incomingResponseMessage)", true, delegator1.CanProcess(incomingResponseMessage));
			LoggingInformation logger = new LoggingInformation();
			delegator1.Process(logger, incomingResponseMessage);

			AssertEquals("eciHeader.EntryNumber", "47975057", eciHeader.EntryNumber);
			AssertEquals("formalHeader.EntryNumber", "", formalHeader.EntryNumber);
			AssertEquals("Message should be linked to eciHeader", eciHeader.PK, incomingResponseMessage.EM_LinkUniqueID);
			factory1.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration declaration2 = factory2.Load<JobDeclaration>(declaration.PK);
			declaration2.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			factory2.Save();

			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			NZCMessage message3 = factory3.Load<NZCMessage>(incomingResponseMessage.PK);
			message3.ResetToQueuedStatus();
			factory3.Save();

			BusinessObjectFactory factory4 = new BusinessObjectFactory();
			NZCMessage message4 = factory4.Load<NZCMessage>(incomingResponseMessage.PK);
			DeclarationDelegator delegator2 = new DeclarationDelegator();
			AssertEquals("delegator2.CanProcess(incomingResponseMessage)", true, delegator2.CanProcess(message4));
			LoggingInformation logger2 = new LoggingInformation();
			delegator2.Process(logger2, message4);
			factory4.Save();
			AssertEquals("message4.EM_LinkUniqueID should be linked to the Formal Header now.", formalHeader.PK, message4.EM_LinkUniqueID);
			formalHeader = factory4.Load<CusEntryHeader>(message4.EM_LinkUniqueID);
			AssertEquals("formalHeader.EntryNumber should be filled in now", "47975057", formalHeader.EntryNumber);
		}

		public void TestCanProcess()
		{
			var factory = new BusinessObjectFactory();
			JobDeclaration declaration = factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00002852";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;

			CusEntryHeader entryHeader = declaration.CusEntryHeader;
			entryHeader.CH_BGMReference = "B00002852E";
			factory.Save();

			var message = factory.NewWithValidTestData<NZCMessage>();
			message.EM_ApplicationCode = NZCMessage.ApplicationCodes.NewZealandCustoms;
			message.EM_ReceiveTransmit = NZCMessage.Direction.Receive;
			message.EM_MessageText = @"UNH+1+CUSRES:D:98A:UN+B00002852E'BGM+932+17638865'GIS+844:120:143'CNT+10:0'UNT+5+1'";

			DeclarationDelegator delegator1 = new DeclarationDelegator();
			AssertEquals("delegator.CanProcess(B00002852E)", true, delegator1.CanProcess(message));

			var message2 = factory.NewWithValidTestData<NZCMessage>();
			message2.EM_ApplicationCode = NZCMessage.ApplicationCodes.NewZealandCustoms;
			message2.EM_ReceiveTransmit = NZCMessage.Direction.Receive;
			message2.EM_MessageText = @"UNH+1+CUSRES:D:98A:UN+B00002852'BGM+932+17638865'GIS+844:120:143'CNT+10:0'UNT+5+1'";

			DeclarationDelegator delegator2 = new DeclarationDelegator();
			AssertEquals("delegator.CanProcess(B00002852)", true, delegator2.CanProcess(message));

			var message3 = factory.NewWithValidTestData<NZCMessage>();
			message3.EM_ApplicationCode = NZCMessage.ApplicationCodes.NewZealandCustoms;
			message3.EM_ReceiveTransmit = NZCMessage.Direction.Receive;
			message3.EM_MessageText = @"UNH+1+CUSRES:D:98A:UN+B00002852F'BGM+932+17638865'GIS+844:120:143'CNT+10:0'UNT+5+1'";

			DeclarationDelegator delegator3 = new DeclarationDelegator();
			AssertEquals("delegator.CanProcess(B00002852F)", true, delegator3.CanProcess(message));
		}

		public void TestCanProcessUnsolicitedMessage()
		{
			var factory = new BusinessObjectFactory();
			var message = factory.NewWithValidTestData<NZCMessage>();
			message.EM_ApplicationCode = NZCMessage.ApplicationCodes.NewZealandCustoms;
			message.EM_ReceiveTransmit = NZCMessage.Direction.Receive;
			message.EM_MessageText = @"UNH+1+CUSRES:D:96B:UN+B00002209'BGM+963+50317008:01'GIS+801:120:143'ERP+001::004'ERC+607::143'UNT+6+1'";
			var delegator = new DeclarationDelegator();
			AssertEquals("Non Cusres D96B message should not be assumed as an unsolicited message", false, delegator.CanProcessUnsolicitedMessage(message));

			var unsolicitedMessage = factory.NewWithValidTestData<NZCMessage>();
			unsolicitedMessage.EM_ApplicationCode = NZCMessage.ApplicationCodes.NewZealandCustoms;
			unsolicitedMessage.EM_ReceiveTransmit = NZCMessage.Direction.Receive;
			unsolicitedMessage.EM_MessageText = @"UNH+553211+CUSRES:D:96B:UN+22326971872015'BGM+932+61352032:01'FTX+DIN+++39 LOOSE PACKAGE(S) OR ITEM(S)'TDT+20++4+++++:::NZ99'LOC+9+NZAKL'GIS+819:120:143'NAD+AL+40342956C:ZZZ:143+FONTERRA LIMITED'NAD+CB+40342956C:ZZZ:143+FONTERRA LIMITED'DOC+964+1'PAC+39++CT'RFF+HWB:08651411091'UNT+12+553211'";
			delegator = new DeclarationDelegator();
			AssertEquals("Cusres D96B Delivery Order message should be processed as an unsolicited message", true, delegator.CanProcessUnsolicitedMessage(unsolicitedMessage));
		}

		[ExpectNoExceptions]
		public void TestProcessUnsolicitedMessage()
		{
			var factory = new BusinessObjectFactory();
			var message = factory.New<NZCMessage>();
			message.EM_ApplicationCode = NZCMessage.ApplicationCodes.NewZealandCustoms;
			message.EM_ReceiveTransmit = NZCMessage.Direction.Receive;
			message.EM_MessageText = @"UNH+553211+CUSRES:D:96B:UN+22326971872015'BGM+932+61352032:01'FTX+DIN+++39 LOOSE PACKAGE(S) OR ITEM(S)'TDT+20++4+++++:::NZ99'LOC+9+NZAKL'GIS+819:120:143'NAD+AL+40342956C:ZZZ:143+FONTERRA LIMITED'NAD+CB+40342956C:ZZZ:143+FONTERRA LIMITED'DOC+964+1'PAC+39++CT'RFF+HWB:08651411091'UNT+12+553211'";
			var logger = new LoggingInformation();
			var delegator = new DeclarationDelegator();
			delegator.ProcessUnsolicitedDeliveryOrder(logger, message);
		}
	}
}
