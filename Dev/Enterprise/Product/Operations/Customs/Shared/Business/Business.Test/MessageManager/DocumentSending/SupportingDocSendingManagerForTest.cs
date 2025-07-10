using CargoWise.Types;
using Enterprise.Customs.Business.MessageBuilders;

namespace Enterprise.Customs.Business.MessageManagers.DocumentSending.Testing
{
	sealed class SupportingDocSendingManagerForTest : SupportingDocSendingManager
	{
		public SupportingDocSendingManagerForTest(ISupportingDocSendingObjectParent declarationWrapper, IMessageNotificationCollector notification) : base(declarationWrapper, notification)
		{
		}

		protected override ZString GetDestination(ISupportingDocumentMessageDataProvider dataWrapper) => "TestDestination";
	}
}
