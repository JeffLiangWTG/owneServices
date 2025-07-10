using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface ILineWithMatchingLines<T> : ILineWithCommittedPickLines
		where T : ILineWithCommittedPickLines
	{
		// Matching Lines
		ZGuid MatchingLinePK { get; set; }
		IEnumerable<T> MatchingLines { get; }

		// Quantities
		new ZDecimal PerPackageQty { get; }
	}
}
