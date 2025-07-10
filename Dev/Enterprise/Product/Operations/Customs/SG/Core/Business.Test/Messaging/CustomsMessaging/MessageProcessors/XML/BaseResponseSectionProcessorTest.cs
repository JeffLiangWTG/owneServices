using System.IO;
using CargoWise.EntityFramework;
using Enterprise.Customs.SG.V4.Business.Messaging.Tradenet;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.Messaging.MessageProcessors.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing
{
	abstract class BaseResponseSectionProcessorTest : MessageProcessorTestCase
	{
		protected TradenetResponse AssertProcessValidMessage(string originalEntryStatus, string newEntryStatus, string applicationReference, string messageText, string outgoingMsgStatus = EDIMessage.Status.Sent, string incomingMsgExpectedStatus = EDIMessage.Status.Received)
		{
			TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var declaration = newFactory.New<JobDeclaration>();
			var entry = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			entry.CH_Status = originalEntryStatus;
			var outgoingMessage = newFactory.New<SGXmlEDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			entry.Messages.Add(outgoingMessage);
			var incomingMessage = newFactory.New<SGXmlEDIMessage>();
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			newFactory.Save();
			outgoingMessage.EM_ApplicationReference = applicationReference;
			outgoingMessage.EM_Status = outgoingMsgStatus;
			newFactory.Save();
			TradenetResponse tradenetResponse = null;
			using (var stream = incomingMessage.GetEM_MessageTextReader())
			{
				var serializer = ZXmlSerializer.New(typeof(TradenetResponse));
				tradenetResponse = serializer.Deserialize(stream) as TradenetResponse;
				MessageProcessor = GetMessageProcessor(tradenetResponse);
				MessageProcessor.ProcessMessage(incomingMessage);
			}

			CombineAssertions(() =>
			{
				AssertEquals("Entry.CH_Status", newEntryStatus, entry.CH_Status);
				AssertCollectionContains("Messages", incomingMessage, entry.Messages);
				AssertEquals("IncomingMessage.EM_Status", incomingMsgExpectedStatus, incomingMessage.EM_Status);
				AssertEquals("IncomingMessage.EM_ApplicationReference", applicationReference, incomingMessage.EM_ApplicationReference);
				AssertEquals("IncomingMessage.EM_LinkUniqueID", entry.PK, incomingMessage.EM_LinkUniqueID);
				AssertEquals("IncomingMessage.EM_LinkTable", CusEntryHeaderSchema.Constants.TableName, incomingMessage.EM_LinkTable);
			}
			);

			newFactory.Save();
			return tradenetResponse;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
		protected string LoadXmlFile(string fileName)
		{
			return File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\SG\Core\Business.Test\Messaging\CustomsMessaging\MessageProcessors\TestMessages\XML\" + fileName);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
		protected string LoadHtmlFile(string fileName)
		{
			return File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\SG\Core\Business.Test\Messaging\CustomsMessaging\MessageProcessors\TestMessages\Html\" + fileName);
		}

		#region Message Processor
		protected MessageProcessor MessageProcessor
		{
			get;
			set;
		}

		protected abstract MessageProcessor GetMessageProcessor(TradenetResponse tradenetResponse);
		protected CusEntryHeader GetEntryHeader(MessageProcessor processor)
		{
			return GetFieldValueCore(processor, "entry") as CusEntryHeader;
		}

		protected EmailDef GetResponseEmail(MessageProcessor processor)
		{
			return GetFieldValueCore(processor, "responseEmail") as EmailDef;
		}

		protected EDIMessage GetIncomingMessage(MessageProcessor processor)
		{
			return GetFieldValueCore(processor, "incomingMessage") as EDIMessage;
		}

		protected object GetFieldValueCore(MessageProcessor processor, string fieldName)
		{
			var field = processor.GetType().GetField(fieldName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			return field.GetValue(processor);
		}

		protected object GetPropertyValueCore(MessageProcessor processor, string propertyName)
		{
			var property = processor.GetType().GetProperty(propertyName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			return property.GetValue(processor);
		}
		#endregion
	}
}
