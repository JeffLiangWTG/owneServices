using Enterprise.Freight.Business;

namespace Enterprise.Freight.CFS.Business.Testing
{
	internal class PackUnpackShipmentBaseShipmentTest : CFSShipmentTest
	{
		protected override CommonShipment GetShipment()
		{
			return Factory.New<PackUnpackShipment>();
		}
	}
}
