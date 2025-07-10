using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsLocationDataHelper
	{
		void ValidateLocation(IBusiness parent, ZPropertyInfo locationStringInfo);
		ZGuid FindLocationPK(BusinessObjectFactory factory, ZString locationString, ZGuid whsPK);
	}
}
