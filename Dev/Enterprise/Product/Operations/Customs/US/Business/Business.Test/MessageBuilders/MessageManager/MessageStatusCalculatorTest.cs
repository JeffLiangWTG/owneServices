using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestsSubclassesOf(typeof(MessageStatusCalculator))]
	public abstract class MessageStatusCalculatorTest : TestCaseWithFactory
	{
		public abstract void TestCalculateStatusForOutgoingMessage();
		public abstract void TestCalculateStatusForIncomingClearedMessage();
		public abstract void TestCalculateStatusForIncomingPartialClearedMessage();
		public abstract void TestCalculateStatusForIncomingRejectedMessage();

		public virtual void TestCalculateStatusForIncomingUndefinedMessage()
		{
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			AssertEquals(false, message.IsTransmitMessage);

			calculator.CalculateStatus(message, ABIResponseStatus.Undefined);
			AssertEquals("", messageAttachee.MessageStatus);
		}

		protected IMessageAttachee messageAttachee;
		protected MQEDIMessage message;
		protected MessageStatusCalculator calculator;

		protected override void SetUp()
		{
			base.SetUp();
			messageAttachee = GetMessageAttachee();
			message = Factory.New<MQEDIMessage>();
			message.EM_MessageType = GetApplicationIdentifierCode();
			calculator = GetNewMessageStatusCalculator(messageAttachee);
		}

		protected virtual IMessageAttachee GetMessageAttachee()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			return declaration.CustomsEntryHeaders.AddNew();
		}

		protected abstract MessageStatusCalculator GetNewMessageStatusCalculator(IMessageAttachee messageAttachee);
		protected abstract ZString GetApplicationIdentifierCode();
	}
}
