using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Integration;

namespace Enterprise.MasterFiles.Integration
{
	public static class IQueuedLogExtensions
	{
		public static string PrintBasic(this IQueuedLog log)
		{
			return log.Print((type, _) => type == typeof(IEnumerable<IStmChangeLog>));
		}
	}
}
