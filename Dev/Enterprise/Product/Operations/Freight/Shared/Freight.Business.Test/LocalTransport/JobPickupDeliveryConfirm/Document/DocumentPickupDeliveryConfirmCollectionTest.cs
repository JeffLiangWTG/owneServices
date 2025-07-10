using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(DocumentPickupDeliveryConfirmCollection))]
	sealed class DocumentPickupDeliveryConfirmCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocumentPickupDeliveryConfirmCollection>
	{
		protected override DocumentPickupDeliveryConfirmCollection GetCollectionToTest()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.OuterPackLines.AddNew();
			CommonPickupDeliveryConfirmCollection confirms = new CommonPickupDeliveryConfirmCollection(Factory, shipment, Constants.PickupDeliveryConfirmTypes.DestinationDelivery);
			return new DocumentPickupDeliveryConfirmCollection(confirms);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CommonPickupDeliveryConfirm confirm = Factory.New<CommonPickupDeliveryConfirm>();
			return new DocumentPickupDeliveryConfirm(confirm);
		}
	}
}
