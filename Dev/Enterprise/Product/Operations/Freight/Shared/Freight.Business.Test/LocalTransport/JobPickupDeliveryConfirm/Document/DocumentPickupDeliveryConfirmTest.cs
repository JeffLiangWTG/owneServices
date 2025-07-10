using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(DocumentPickupDeliveryConfirm))]
	sealed class DocumentPickupDeliveryConfirmTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			CommonPickupDeliveryConfirm confirm = Factory.New<CommonPickupDeliveryConfirm>();
			return new DocumentPickupDeliveryConfirm(confirm);
		}
	}
}
