using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsLocationDataHelper : IWhsLocationDataHelper
	{
		public void ValidateLocation(IBusiness parent, ZPropertyInfo locationStringInfo)
		{
			WhsLocation.ValidateLocation(parent, locationStringInfo);
		}

		public ZGuid FindLocationPK(BusinessObjectFactory factory, ZString locationString, ZGuid whsPK)
		{
			return WhsLocation.FindLocationPK(factory, locationString, whsPK);
		}
	}
}
