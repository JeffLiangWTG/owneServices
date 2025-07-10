using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(CombineShipmentsHelper))]
	sealed class CombineShipmentsBOHelperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonShipment shipment = CommonShipment.New(Factory);
			CombineShipmentsHelper helper = new CombineShipmentsHelper(shipment, consol);
			return helper;
		}
	}
}
