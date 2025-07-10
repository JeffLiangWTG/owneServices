using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(DocumentPickupDeliveryConfirmOptions))]
	sealed class DocumentPickupDeliveryConfirmOptionsTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.OuterPackLines.AddNew();
			CommonPickupDeliveryConfirmCollection confirms = new CommonPickupDeliveryConfirmCollection(Factory, shipment, Constants.PickupDeliveryConfirmTypes.DestinationDelivery);
			DocumentPickupDeliveryConfirmCollection documentConfirms = new DocumentPickupDeliveryConfirmCollection(confirms);
			return new DocumentPickupDeliveryConfirmOptions(documentConfirms);
		}
	}
}
