using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Universal.Module.Testing
{
	class RefCusTariffControllerForTest : RefCusTariffController
	{
		public IBusiness LoadBusinessEntityExposed(BusinessObjectFactory factory, ZGuid sourceEntityPK) => LoadBusinessEntity(factory, sourceEntityPK);
	}
}
