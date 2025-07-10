using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IStatementFees
	{
		IEnumerable<KeyValuePair<ZString, ZDecimal>> Fees { get; }
	}
}
