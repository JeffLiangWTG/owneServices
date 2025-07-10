using CargoWise.Types;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Business.MessageDelivery.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business.MessageManagers.Testing
{
	sealed class SupportingDocSendingSupportingDocEHubDeliveryTests : EServicesDeliveryTest
	{
		protected override EServicesDelivery Delivery
		{
			get
			{
				return new SupportingDocEHubDelivery(null);
			}
		}

		protected override ZString MessageDataLogLinkerEventCode()
		{
			return Events.DocumentSentCode;
		}
	}
}
