using System;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing
{
	public class SGMessageProcessorTest : MessageProcessorTestCase
	{
		protected void AssertProcessValidMessage(string entryStatus, string applicationReference, string messageText)
		{
			ProcessMessage(entryStatus, applicationReference, messageText);
			AssertEquals(2, EntryHeader.Messages.Count);
			AssertEquals(EDIMessage.Status.Received, IncomingMessage.EM_Status);
			AssertEquals(applicationReference, IncomingMessage.EM_ApplicationReference);
			AssertEquals(EntryHeader.PK, IncomingMessage.EM_LinkUniqueID);
			AssertEquals(CusEntryHeaderSchema.Constants.TableName, IncomingMessage.EM_LinkTable);
		}

		protected void ProcessMessage(string entryStatus, string applicationReference, string messageText)
		{
			EntryHeader.CH_Status = entryStatus;
			OutgoingMessage.EM_ApplicationReference = applicationReference;
			Factory.Save();
			IncomingMessage.EM_MessageText = messageText;
			MessageProcessor.ProcessMessage(IncomingMessage);
		}

		#region Declaration
		protected JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					CusEntryHeader entry = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
					entry.Messages.Add(Factory.New<CUSDECEDIMessage>());
					OutgoingMessage.Factory.Save(); //Save outgoing message so Message Number is replaced
				}

				return declaration;
			}
		}

		JobDeclaration declaration;
		#endregion
		#region EntryHeader
		protected CusEntryHeader EntryHeader
		{
			get
			{
				return (CusEntryHeader)Declaration.ActiveEntryHeaders[0];
			}
		}

		#endregion
		#region Messages
		protected CUSDECEDIMessage OutgoingMessage
		{
			get
			{
				CUSDECEDIMessage result = (CUSDECEDIMessage)EntryHeader.Messages[0];
				result.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
				return result;
			}
		}

		protected SGEDIMessage IncomingMessage
		{
			get
			{
				if (incomingMessage == null)
				{
					incomingMessage = Factory.New<SGEDIMessage>();
					incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				}

				return incomingMessage;
			}
		}

		SGEDIMessage incomingMessage;
		protected void ResetIncomingMessage()
		{
			incomingMessage = null;
		}

		#endregion
		#region Message Processor
		protected SGMessageProcessor MessageProcessor
		{
			get
			{
				return messageProcessor ?? (messageProcessor = GetMessageProcessor());
			}
		}

		protected SGMessageProcessor messageProcessor;
		protected virtual SGMessageProcessor GetMessageProcessor()
		{
			return new SGMessageProcessorTestClass();
		}

		#endregion
		#region SGMessageProcessorTestClass
		protected class SGMessageProcessorTestClass : SGMessageProcessor
		{
			public SGMessageProcessorTestClass() : base(new LoggingInformation(), "XXX", "TESTMESSAGE")
			{
			}

			protected override void SetEntryStatus()
			{
				throw new Exception("The method or operation is not implemented.");
			}

			protected override string URN
			{
				get
				{
					throw new Exception("The method or operation is not implemented.");
				}
			}

			protected override EDIMessage IncomingMessage
			{
				get
				{
					throw new Exception("The method or operation is not implemented.");
				}
			}
		}
		#endregion
	}
}
