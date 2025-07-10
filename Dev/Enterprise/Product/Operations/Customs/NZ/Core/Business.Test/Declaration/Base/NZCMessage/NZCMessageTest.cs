using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Messaging.Business;
using CusMAWB = Enterprise.Customs.NZ.Business.Express.CusMAWB;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using Enterprise.Messaging.Testing;
	using NUnit.Framework;

	[TestedType(typeof(NZCMessage))]
	public class NZCMessageTest : EDIMessageTest
	{
		public void TestNZCMessageCanStoreAtLeast1MBInEM_MessageInterpretation()
		{
			NZCMessage message = Factory.New<NZCMessage>();
			message.EM_MessageInterpretation = new ZString('Z', 1048576);
			AssertEquals("message.EM_MessageInterpretation.Length", 1048576, message.EM_MessageInterpretation.Length);
		}

		public void TestMsgTransMode()
		{
			var message = Factory.New<NZCMessage>();
			message.MsgTransMode = "XXX";
			AssertEquals("XXX", message.MsgTransMode);
			Factory.Save();

			var factory = new BusinessObjectFactory();
			message = factory.Load<NZCMessage>(message.PK);
			AssertEquals("XXX", message.MsgTransMode);

			message.MsgTransMode = "";
			factory.Save();

			factory = new BusinessObjectFactory();
			message = factory.Load<NZCMessage>(message.PK);
			AssertEquals("", message.MsgTransMode);
		}

		public void TestEM_MessageSubTypeDescription()
		{
			NZCMessage message = Factory.New<NZCMessage>();
			AssertEquals("message.EM_MessageSubTypeDescription", ZString.Empty, message.EM_MessageSubTypeDescription);
			message.EM_MessageSubType = Business.MessageSubTypeList.Codes.Original;
			AssertEquals("message.EM_MessageSubTypeDescription", Business.MessageSubTypeList.Descriptions.Original, message.EM_MessageSubTypeDescription);
			message.EM_MessageSubType = Business.MessageSubTypeList.Codes.ReplaceHeader;
			AssertEquals("message.EM_MessageSubTypeDescription", Business.MessageSubTypeList.Descriptions.ReplaceHeader, message.EM_MessageSubTypeDescription);
			message.EM_MessageSubType = "ZXZ";
			AssertEquals("message.EM_MessageSubTypeDescription", "ZXZ", message.EM_MessageSubTypeDescription);
		}

		public void TestBaseNZCMessageStillGetsJobNumber()
		{
			NZCMessage message = Factory.New<NZCMessage>();
			CusMAWB mawb = Factory.New<CusMAWB>();
			mawb.CM_MessageReference = "X05932929";
			mawb.Messages.Add(message);
			AssertEquals("message.GetJobNumber()", "X05932929", message.GetJobNumber());
		}

		public void TestIsQueuedToBeSentLater()
		{
			NZCMessage message = Factory.New<NZCMessage>();
			message.EM_ReceiveTransmit = NZCMessage.Direction.Receive;
			message.EM_Status = NZCMessage.Status.Queued;
			AssertEquals(false, message.IsQueuedToBeSentLater);
			message.EM_ReceiveTransmit = NZCMessage.Direction.Transmit;
			AssertEquals(true, message.IsQueuedToBeSentLater);
			message.EM_Status = NZCMessage.Status.Sent;
			AssertEquals(false, message.IsQueuedToBeSentLater);
		}

		public void TestNZCMessageGetADeclarationReferenceWhenNZCMessageIsSavedFirst()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			NZCMessage message = declaration.CusEntryHeader.Messages.AddNew();
			message.EM_MessageText = NZCMessage.SendersReferencePlaceHolder + ":" + NZCMessage.MessageNumberPlaceHolder + "'";
			Factory.Save();
			AssertEquals("Precondition: Declaration.JE_DeclarationReference", "B00001000", declaration.JE_DeclarationReference);
			AssertEquals("Precondition: Message.EM_MessageNum", "1", message.EM_MessageNum);
			AssertEquals("B00001000:1'", message.EM_MessageText);
		}

		public void TestNZCMessagePutsTheRightReferenceIn()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_DeclarationReference = "B0-DECREF";
			CusEntryHeader entryHeader = declaration.CusEntryHeader;
			entryHeader.CH_BGMReference = "B0-ENTREF";
			NZCMessage message = entryHeader.Messages.AddNew();
			message.EM_MessageText = EDIMessage.MessageNumberPlaceHolder + "/" + EDIMessage.SendersReferencePlaceHolder;
			Factory.Save();
			AssertEquals("Precondition: Declaration.JE_DeclarationReference", "B0-DECREF", declaration.JE_DeclarationReference);
			AssertEquals("Precondition: EntryHeader.CH_BGMReference", "B0-ENTREF", entryHeader.CH_BGMReference);
			AssertEquals("Message.EM_MessageText", message.EM_MessageNum + "/" + "B0-ENTREF", message.EM_MessageText);
		}

		public void TestContainedChecksumPlaceHolder()
		{
			NZCMessage message = Factory.New<NZCMessage>();
			Assert("PreCondition Empty Message Number", message.EM_MessageNum.IsEmpty);
			message.EM_MessageText = TestContainedChecksumPlaceHolderExampleText;
			Assert("PreCondition Place Holder Exists", message.EM_MessageText.IndexOf(EDIMessage.ContainedChecksumPlaceHolder) > -1);
			Factory.Save();
			Assert("PreCondition Place Holder Not Exists", message.EM_MessageText.IndexOf(EDIMessage.ContainedChecksumPlaceHolder) == -1);
		}

		public void TestMessageAsCUSRESD98A()
		{
			NZCMessage testMessage = Factory.New<NZCMessage>();
			testMessage.EM_MessageText = @"UNH+2447+CUSRES:D:98A:UN+B01001001'BGM+963+00000000'UNT+3+2447'";
			AssertNotNull("MessageAsCUSRESD98A should not be null", testMessage.MessageAsCUSRESD98A);
			var edifactMessage = testMessage.MessageAsCUSRESD98A;
			AssertEquals("MessageAsCUSRESD98A UNH segment reference", "2447", edifactMessage.UNH[0].MessageReferenceNumber);
			AssertEquals("MessageAsCUSRESD98A BGM segment response type", "963", edifactMessage.BGM[0].DocumentMessageName.DocumentMessageNameCoded.ToString());
		}

		public void TestMessageAsCUSRESD96B()
		{
			NZCMessage testMessage = Factory.New<NZCMessage>();
			testMessage.EM_MessageText = @"UNH+553211+CUSRES:D:96B:UN+22326971872015'BGM+932+61352032:01'FTX+DIN+++39 LOOSE PACKAGE(S) OR ITEM(S)'TDT+20++4+++++:::NZ99'LOC+9+NZAKL'GIS+819:120:143'NAD+AL+40342956C:ZZZ:143+FONTERRA LIMITED'NAD+CB+40342956C:ZZZ:143+FONTERRA LIMITED'DOC+964+1'PAC+39++CT'RFF+HWB:08651411091'UNT+12+553211'";
			AssertNotNull("MessageAsCUSRESD96B should not be null", testMessage.MessageAsCUSRESD96B);
			var edifactMessage = testMessage.MessageAsCUSRESD96B;
			AssertEquals("MessageAsCUSRESD96B UNH segment reference", "553211", edifactMessage.UNH[0].MessageReferenceNumber);
			AssertEquals("MessageAsCUSRESD96B BGM segment response type", "932", edifactMessage.BGM[0].DocumentMessageName.DocumentMessageNameCoded.ToString());
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
		}
		#endregion
	}
}
