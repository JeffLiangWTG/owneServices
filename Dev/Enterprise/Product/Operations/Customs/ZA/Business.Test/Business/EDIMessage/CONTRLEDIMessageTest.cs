using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.ZA.Business.SARSEDIMessage;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CONTRLEDIMessage))]
	sealed class CONTRLEDIMessageTest : Messaging.Testing.EDIMessageTest
	{
		public void TestParentMessageNumber()
		{
			var contrlMessage = Factory.New<CONTRLEDIMessage>();
			contrlMessage.EM_MessageText = ZAMessageTest.CONTRLTestMessage.Replace("\r\n", "");
			AssertEquals("00000000000062", contrlMessage.ParentMessageNumber);
		}

		public void TestIsRejectionMessage()
		{
			var contrlMessage = Factory.New<CONTRLEDIMessage>();
			contrlMessage.EM_MessageText = ZAMessageTest.CONTRLTestMessage.Replace("\r\n", "");
			AssertEquals(false, contrlMessage.IsRejectionMessage);
			AssertEquals("00000000000062", contrlMessage.ParentMessageNumber);
			contrlMessage = Factory.New<CONTRLEDIMessage>();
			contrlMessage.EM_MessageText = ZAMessageTest.CONTRLTestMessage.Replace("\r\n", "").Replace("+7", "+6");
			AssertEquals(false, contrlMessage.IsRejectionMessage);
			AssertEquals("00000000000062", contrlMessage.ParentMessageNumber);
			contrlMessage = Factory.New<CONTRLEDIMessage>();
			contrlMessage.EM_MessageText = ZAMessageTest.CONTRLTestMessage.Replace("\r\n", "").Replace("+7", "+4");
			AssertEquals(true, contrlMessage.IsRejectionMessage);
			AssertEquals("00000000000062", contrlMessage.ParentMessageNumber);
			contrlMessage = Factory.New<CONTRLEDIMessage>();
			contrlMessage.EM_MessageText = "RandomText";
			AssertEquals(false, contrlMessage.IsRejectionMessage);
			AssertEquals("", contrlMessage.ParentMessageNumber);
		}

		public void TestCONTRLHelper()
		{
			var contrlMessage = Factory.New<CONTRLEDIMessage>();
			contrlMessage.EM_MessageText = ZAMessageTest.CONTRLTestMessage.Replace("\r\n", "");
			var helper = contrlMessage.CONTRLHelper;
			AssertNotNull(helper);
			AssertType<CONTRLMessageHelper>(helper);
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestDeveloperErrorWhenSettingEM_MessageTypeWithSomethingOtherThanCTL()
		{
			CONTRLEDIMessage controlMessage = Factory.New<CONTRLEDIMessage>();
			controlMessage.EM_MessageType = "IMP";
		}

		public void TestIfFactorySaveFailsControlMessageShouldBeRemoved()
		{
			CONTRLEDIMessage cTRLMessage = (CONTRLEDIMessage)GetNewBusinessObject();
			cTRLMessage.EM_MessageNum = "1234xxx";
			try
			{
				var loadedCTRLMessage = Factory.LoadTop1<CONTRLEDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageNum, "1234xxx"));
				AssertEquals("1234xxx", loadedCTRLMessage.EM_MessageNum);
				var entry = Factory.New<CusEntryHeader>();
				entry.CH_BGMReference = "1";
				Factory.Save();
				Fail("Factory should not be able to save");
			}
			catch
			{
				var loadedCTRLMessage = Factory.LoadTop1<CONTRLEDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageNum, "1234xxx"));
				AssertNull(loadedCTRLMessage);
			}
		}

		public void TestOnFactorySaveNewReferenceNumberIsAllocated()
		{
			CONTRLEDIMessage cTRLMessage = (CONTRLEDIMessage)GetNewBusinessObject();
			cTRLMessage.EM_ApplicationCode = "1";
			cTRLMessage.EM_MessageType = "CTL";
			cTRLMessage.EM_MessageSubType = "ACK";
			cTRLMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			cTRLMessage.EM_Status = "QUE";
			cTRLMessage.EM_MessageText += EDIMessage.MessageNumberPlaceHolder;
			cTRLMessage.EM_GB = GlbBranch.CurrentBranch.PK;
			cTRLMessage.EM_GE = GlbDepartment.CurrentDepartment.PK;
			AssertEquals(true, cTRLMessage.EM_MessageNum.IsEmpty);
			Factory.Save();
			AssertEquals(false, cTRLMessage.EM_MessageNum.IsEmpty);
		}

		public void TestCTLIsTheDefaultValueOfEM_MessageType()
		{
			var controlMessage = Factory.New<CONTRLEDIMessage>();
			AssertEquals(CONTRLEDIMessage.MessageTypes.CONTRL, controlMessage.EM_MessageType);
			AssertEquals(CONTRLEDIMessage.Direction.Receive, controlMessage.EM_ReceiveTransmit);
		}

		public void TestSaveOtherBOFailedNotDeleteMessage()
		{
			var testInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			testInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ZACustoms;
			testInterchange.EI_InterchangeType = "ZAC";
			Factory.Save();
			AssertEquals("Interchange.IsInDatabase should be true", true, testInterchange.IsInDatabase);
			var testMessage = Factory.NewWithValidTestData<CONTRLEDIMessage>();
			testMessage.EM_ApplicationCode = ApplicationCodeList.Codes.ZACustoms;
			testMessage.EM_MessageType = MessageTypes.CONTRL;
			testMessage.EM_MessageSubType = "XXX";
			testMessage.EM_EI = testInterchange.PK;
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_Status = "QUE";
			Factory.Save();
			AssertEquals("EDIMessage.IsInDatabase should be true", true, testMessage.IsInDatabase);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001234";
			var entry = declaration.ActiveEntryHeaders.AddNew();
			testMessage.EM_LinkedObject = entry;
			declaration.JE_GB = ZGuid.Empty;
			try
			{
				Factory.Save();
			}
			catch (Exception)
			{
			}

			AssertNotContains("The EDIMessage linked to a previously persisted EDIInterchange shouldn't be deleted.", ErrorReporter.LastKeyReported);
		}
	}
}
