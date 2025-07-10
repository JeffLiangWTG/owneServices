using System.Collections.Generic;

namespace Enterprise.MasterFiles.Business
{
	public abstract class TaskPenetrationResetEventParametersStrategy
	{
		public abstract IEnumerable<KeyValuePair<string, string>> GetLogReferenceParameters(ProcessTask processTask);
	}
}
