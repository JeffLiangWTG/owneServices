using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Moq.Protected;

namespace Enterprise.Customs.Business.MessagingProcess.Testing
{
	public class CustomsMessageGeneratorImplForTest : ICustomsMessageGenerator
	{
		public CustomsMessageGeneratorImplForTest(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		public bool UseMessageForTest { get; set; }
		public EDIMessage GenerateMessageForTest { get; set; }

		public EDIMessage GenerateMessage() => UseMessageForTest ? GenerateMessageForTest : GenerateNewMessage();

		public EDIMessage GenerateNewMessage()
		{
			var msg = factory.NewMoq<EDIMessage>();
			msg.Object.EM_MessageText = $"Hello World: ({EDIMessage.MessageNumberPlaceHolder})";
			msg.Protected().Setup<string>("GetMessageReferenceNumber").Returns("123");

			return msg.Object;
		}
	}
}
