using CargoWise.Types;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.NZ.Business.MessageProcessors.Testing
{
	using System;
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Customs.NZ.Business.Declaration;
	using Enterprise.Customs.NZ.Registry;
	using Enterprise.Messaging.Business;
	using NUnit.Framework;

	public abstract class ECIManifestDelegatorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestNullMessageDoesntCauseException()
		{
			NZCMessage message = declaration.CusEntryHeader.Messages.AddNew();
			messageProcessor.ProcessMessage(message);
		}

		#region Implementation
		abstract protected void SetupMessageProcessor(LoggingInformation logger);

		protected JobDeclaration declaration;
		protected MessageProcessor messageProcessor;

		protected override void SetUp()
		{
			base.SetUp();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00001234A");
			SetupMessageProcessor(new LoggingInformation());
		}

		protected void ProcessMessageAndCompareAgainstExpectedResult(string eDIFACTMessage, string expectedUserMessage, bool expectedProcessMessageResult)
		{
			ProcessMessageAndCompareAgainstExpectedResult(GetNZCMessage(eDIFACTMessage), expectedUserMessage, expectedProcessMessageResult);
		}

		protected void ProcessMessageAndCompareAgainstExpectedResult(NZCMessage message, string eDIFACTMessage, string expectedUserMessage, bool expectedProcessMessageResult)
		{
			ProcessMessageAndCompareAgainstExpectedResult(SetupNZCMessage(eDIFACTMessage, message), expectedUserMessage, expectedProcessMessageResult);
		}

		void ProcessMessageAndCompareAgainstExpectedResult(NZCMessage message, string expectedUserMessage, bool expectedProcessMessageResult)
		{
			message.Factory.Save();
			messageProcessor.ProcessMessage(message);
			AssertMultilineASCIIEquals("Returned Human Readable Message Generated from EDIFACT Message", expectedUserMessage, message.EM_MessageInterpretation);
			AssertEquals("Result from ProcessMessage", expectedProcessMessageResult ? EDIMessage.Status.Received : EDIMessage.Status.Error, message.EM_Status.ToString());
		}

		protected NZCMessage GetNZCMessage(ZString messageText)
		{
			return SetupNZCMessage(messageText, declaration.CusEntryHeader.Messages.AddNew());
		}

		protected NZCMessage SetupNZCMessage(ZString messageText, NZCMessage message)
		{
			message.IsTransmitMessage = false;
			message.EM_MessageText = messageText.Replace("\r", "").Replace("\n", "");
			message.EM_ApplicationCode = NZCMessage.ApplicationCodes.NewZealandCustoms;
			message.EM_ReceiveTransmit = NZCMessage.Direction.Receive;
			return message;
		}
		#endregion
	}
}
