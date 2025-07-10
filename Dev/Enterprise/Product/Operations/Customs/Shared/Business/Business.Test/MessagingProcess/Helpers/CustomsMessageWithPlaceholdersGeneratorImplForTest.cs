using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business.MessagingProcess.Testing
{
	sealed class CustomsMessageWithPlaceholdersGeneratorImplForTest : CustomsMessageGeneratorImplForTest, ICustomsMessageWithPlaceholdersGenerator
	{
		public CustomsMessageWithPlaceholdersGeneratorImplForTest(BusinessObjectFactory factory) : base(factory)
		{
		}

		public void UpdateMessagePlaceholders(EDIMessage message)
		{
			message.EM_MessageText = message.EM_MessageText.Replace(EDIMessage.MessageNumberPlaceHolder, message.EM_MessageNum);
		}
	}
}
