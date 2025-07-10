using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface ICycleCountLocationTaskCreationFactLoader : ICycleCountLocationFactLoader
	{
	}

	public interface ICycleCountLocationTaskBreakdownFactLoader : ICycleCountLocationFactLoader
	{
	}

	public interface ICycleCountLocationFactLoader
	{
		IEnumerable<IInputFact> LoadInputFacts(ReadOnlyBusinessObjectFactory factory, ZGuid warehousePK, CancellationToken cancellationToken);
	}
}
