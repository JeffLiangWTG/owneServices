using System.Collections.Generic;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IValidateParentWithLines
	{
		ProductPackageTotals GetParentProductPackageTotals(IEnumerable<ICalculateProductPackageTotals> lines);
	}
}
