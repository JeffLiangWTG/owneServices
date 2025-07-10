using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;

namespace Enterprise.Customs.TW.GUI.Testing
{
	class MessageSendingFormForTest : MessageSendingForm
	{
		public MessageSendingFormForTest(JobDeclarationMessageSendingObjectParent declarationWrapper) : base(declarationWrapper)
		{
		}

		public MessageSendingNotificationCollection RunPreSendValidationCore()
		{
			return RunPreSendValidation();
		}
	}
}
