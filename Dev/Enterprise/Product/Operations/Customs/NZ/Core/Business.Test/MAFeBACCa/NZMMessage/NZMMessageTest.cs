using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Registry;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.Testing
{
	using Enterprise.MasterFiles.Business;
	using Enterprise.Messaging.Testing;
	using NUnit.Framework;

	[TestedType(typeof(NZMMessage))]
	public class NZMMessageTest : EDIMessageTest
	{
		public void TestMsgTransMode()
		{
			var message = Factory.New<NZMMessage>();
			message.MsgTransMode = "XXX";
			AssertEquals("XXX", message.MsgTransMode);
			Factory.Save();

			var factory = new BusinessObjectFactory();
			message = factory.Load<NZMMessage>(message.PK);
			AssertEquals("XXX", message.MsgTransMode);

			message.MsgTransMode = "";
			factory.Save();

			factory = new BusinessObjectFactory();
			message = factory.Load<NZMMessage>(message.PK);
			AssertEquals("", message.MsgTransMode);
		}

		public void TestFormattedMessageTextIncludesEM_MessageInterpretation()
		{
			var message = Factory.New<NZMMessage>();
			message.EM_MessageText = @"
<Line1>Hello<\Line1>
	<Child1>thing<\Child1>
<Something Else>FUGGER<\Something Else>
".Trim();
			message.EM_MessageInterpretation = "Interpreted\r\nMessage";

			var expectedresult = @"
Interpreted
Message


Raw Message Text:
-----------------
<Line1>Hello<\Line1>
   <Child1>thing<\Child1>
<Something Else>FUGGER<\Something Else>
".Trim();
			AssertMultilineASCIIEquals("message.EM_FormattedMessageText Includes EM_MessageInterpretation", expectedresult.Trim(), message.EM_FormattedMessageText);
		}

		public void TestFormattedMessageTextConvertsTabsTo3Spaces()
		{
			var message = Factory.New<NZMMessage>();
			message.EM_MessageText = @"
<Line1>Hello<\Line1>
	<Child1>thing<\Child1>
<Something Else>FUGGER<\Something Else>
".Trim();
			var expectedresult = @"
<Line1>Hello<\Line1>
   <Child1>thing<\Child1>
<Something Else>FUGGER<\Something Else>
".Trim();
			AssertMultilineASCIIEquals("message.EM_FormattedMessageText converts tabs to 3 spaces", expectedresult.Trim(), message.EM_FormattedMessageText);
		}

		public void TestMessageDefaults()
		{
			var message = Factory.New<NZMMessage>();
			AssertEquals("EM_ApplicationCode", NZMMessage.ApplicationCodes.NewZealandMAFeBACCa, message.EM_ApplicationCode);
			AssertEquals("EM_ReceiveTransmit", NZMMessage.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertEquals("EM_Status", NZMMessage.Status.Queued, message.EM_Status);
			AssertEquals("EM_IsTestMessage", true, message.EM_IsTestMessage);
		}

		public void TestEM_MessageSubTypeDescription()
		{
			var message = Factory.New<NZMMessage>();
			AssertEquals("message.EM_MessageSubTypeDescription", ZString.Empty, message.EM_MessageSubTypeDescription);
			message.EM_MessageSubType = NZMMessage.MessageTypes.Transmit.MessageSubTypes.Original;
			AssertEquals("message.EM_MessageSubTypeDescription", CodedLists.MessageSubTypeList.Descriptions.Original, message.EM_MessageSubTypeDescription);
			message.EM_MessageSubType = NZMMessage.MessageTypes.Transmit.MessageSubTypes.Replacement;
			AssertEquals("message.EM_MessageSubTypeDescription", CodedLists.MessageSubTypeList.Descriptions.Replacement, message.EM_MessageSubTypeDescription);
			message.EM_MessageSubType = NZMMessage.MessageTypes.Receive.MessageSubTypes.Cancellation;
			AssertEquals("message.EM_MessageSubTypeDescription", CodedLists.MessageSubTypeList.Descriptions.Cancellation, message.EM_MessageSubTypeDescription);
			message.EM_MessageSubType = "ZXZ";
			AssertEquals("message.EM_MessageSubTypeDescription", "ZXZ", message.EM_MessageSubTypeDescription);
		}

		public void TestNZMMessageGetADeclarationReferenceWhenNZMMessageIsSavedAndTheBranchComesFromTheDeclaration()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_GB = branch.PK;

			var message = TestDataBuilder.GetMAFMessaging(declaration).Messages.AddNew();
			message.EM_MessageText = NZMMessage.SendersReferencePlaceHolder + ":" + NZMMessage.MessageNumberPlaceHolder + "'";
			Factory.Save();
			AssertEquals("Precondition: Declaration.JE_DeclarationReference", "B00001000", declaration.JE_DeclarationReference);
			AssertEquals("Precondition: Message.EM_MessageNum", "1", message.EM_MessageNum);
			AssertEquals("B00001000:1'", message.EM_MessageText);
			AssertEquals("Message.EM_GB", branch.PK, message.EM_GB);
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
