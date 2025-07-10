using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsProductParamsByWhsAndClientCollection : IBusinessObjectCollection
	{
		IWhsProductParamsByWhsAndClient FindWhsProductParamsByWhsAndClient(ZGuid clientPK, ZGuid whsPK);

		IEnumerable<IWhsProductParamsByWhsAndClient> FindWhsProductParamsByWhsAndClients(IEnumerable<ZGuid> clientPks, ZGuid warehousePK);
	}
}
