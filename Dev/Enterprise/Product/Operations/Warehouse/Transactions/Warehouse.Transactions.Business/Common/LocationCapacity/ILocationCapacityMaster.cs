using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface ILocationCapacityMaster<U> where U : ILocationCapacityLine
	{
		ZGuid PKToExclude { get; }
		IEnumerable<U> GetValidationLines(WhsLocation location);
		ZBool ShowRFMessage { get; }
		ZString GetLocationQuantityExceededErrorMessage(ZDecimal requiredValue, ZDecimal availableValue, WhsLocation location);
		ZString GetLocationWeightExceededErrorMessage(ZDecimal requiredValue, ZDecimal availableValue, WhsLocation location);
		ZString GetLocationVolumeExceededErrorMessage(ZDecimal requiredValue, ZDecimal availableValue, WhsLocation location);
	}
}
