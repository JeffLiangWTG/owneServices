using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IStatementDeletedEntries
	{
		ZString StatementNumber { get; }
		IEnumerable<KeyValuePair<ZString, ZString>> DeletedEntries { get; }
	}
}
