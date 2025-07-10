using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class MessageProcessorTestCase : Messaging.MessageProcessors.Testing.MessageProcessorTestCase
	{
		protected override BusinessObjectFactory NewFactory()
		{
			return new DeclarationsCreatedCancelledBusinessObjectFactory();
		}
	}
}
