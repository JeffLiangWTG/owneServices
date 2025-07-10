using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierPreferredRoute : AutoCarrierPreferredRoute
	{
		public CarrierPreferredRoute(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
