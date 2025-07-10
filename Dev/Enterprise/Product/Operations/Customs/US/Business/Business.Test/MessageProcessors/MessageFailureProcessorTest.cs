using System;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public abstract class MessageFailureProcessorTest<T, ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY> : ABIProcessorTest<T, ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
			where T : MessageFailureProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
			where ControlMessageBlockA : MessageBlock, IControlMessageBlockA, new()
			where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
			where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		protected void ProcessAndAssert(string applicationIdentifier, string subject)
		{
			ProcessAndAssert(applicationIdentifier, subject, null);
		}

		protected void ProcessAndAssert(string applicationIdentifier, string subject, SetupExtraSendingMessageSettings extraSendingMessageSettings, bool isACE = false)
		{
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var sendingMessage = mock.Object;
			sendingMessage.EM_MessageType = applicationIdentifier;
			sendingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			sendingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			sendingMessage.EM_MessageNum = "~15000";
			sendingMessage.EM_MessageText =
				"B018888XJ5" + applicationIdentifier + "                                               ~15000               " +
				"Y  8888XJ5" + applicationIdentifier + "00001                                                               ";

			if (extraSendingMessageSettings != null)
			{
				extraSendingMessageSettings(sendingMessage);
			}

			var generator = new ABIOutputBlockControlGenerator();
			generator.B.ApplicationIdentifier = applicationIdentifier;
			generator.B.ProcessingDistrictPortCode = "8888";
			generator.B.UserData = "~15000";

			if (isACE)
			{
				generator.AddMessageBlock(CreateAABIX0("X0 BLOCK       1 REF ID:      286    AE YASYUSPRD_70018"));
				generator.AddMessageBlock(CreateAABIX1("X1 FX18   PROC PORT/FLR NOT AUTHRZD FOR SENDR/RCVR"));
				generator.AddMessageBlock(CreateAABIX1("X1RF999   BATCH REJECTED"));
			}
			else
			{
				generator.AddMessageBlock(CreateENSEB("EBINVALID DIST/PORT/BROKER/OFFICE"));
				generator.AddMessageBlock(CreateENSEB("EBTRANSACTION DATA REJECTED"));
			}

			var message = CreateInterchangeAndMessageResponse(applicationIdentifier, "A3901SV9      05140701   051407014539                                00000000039", generator.Serialise(), "Z3901SV9      05140701   051407014539                                00000000039", "~15000");
			CreateNewIncomingMessageProcessor().ExecuteBatch();

			message.Reload();
			AssertEquals(EDIMessage.Status.Received, message.EM_Status);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains(subject); }));
			var expectedBody = isACE ? @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Error Description</th></tr></thead><tr><td>PROC PORT/FLR NOT AUTHRZD FOR SENDR/RCVR</td></tr><tr><td>BATCH REJECTED</td></tr></table>"
										: @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Error Description</th></tr></thead><tr><td>INVALID DIST/PORT/BROKER/OFFICE</td></tr><tr><td>TRANSACTION DATA REJECTED</td></tr></table>";
			AssertContains(expectedBody, email.Body);
			Assert(email.Recipients.Contains(staffZ1.GS_EmailAddress));
			Assert(email.Recipients.Contains(staffZ2.GS_EmailAddress));
		}

		protected virtual IncomingMessageProcessor CreateNewIncomingMessageProcessor()
		{
			return new ABIIncomingMessageProcessor();
		}

		protected override ZInt EmailsExpectedAtCompletionOfEndToEndTest
		{
			get { return 1; }
		}

		public delegate void SetupExtraSendingMessageSettings(MQEDIMessage sendingMessage);

		protected ENSEB CreateENSEB(ZString data)
		{
			var response = new ENSEB();
			response.Deserialise(BlockPadder.Pad(data));
			return response;
		}

		protected AABIX0 CreateAABIX0(ZString data)
		{
			var response = new AABIX0();
			response.Deserialise(BlockPadder.Pad(data));
			return response;
		}

		protected AABIOutputX1 CreateAABIX1(ZString data)
		{
			var response = new AABIOutputX1();
			response.Deserialise(BlockPadder.Pad(data));
			return response;
		}

		protected override void SetUp()
		{
			base.SetUp();
			USCustomsDataRegistry.Instance.ABIMessagesGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, groupZZ1.PK, false));
		}
	}
}
