using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.OceanCarrier.Business
{
	public sealed class CarrierService : AutoCarrierService
	{
		public CarrierService(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
