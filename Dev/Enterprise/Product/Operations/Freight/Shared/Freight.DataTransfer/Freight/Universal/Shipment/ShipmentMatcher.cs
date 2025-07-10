using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class ShipmentMatcher<T> : BaseShipmentMatcher<T>
		where T : CommonShipment
	{
		public ShipmentMatcher(BusinessObjectFactory factory, ShipmentReferences references, IXmlImportLogger logger, IUniversalFreightHelper helper)
			: base(factory, references, logger, helper)
		{
		}

		protected override void AddJobShipmentTypeFilter(ZQuery query)
		{
			helper.AddShipmentParameters(query);
		}
	}
}
