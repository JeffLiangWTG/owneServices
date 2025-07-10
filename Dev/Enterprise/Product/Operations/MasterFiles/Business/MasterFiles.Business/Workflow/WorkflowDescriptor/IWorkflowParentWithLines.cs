
using System.Collections.Generic;

namespace Enterprise.MasterFiles.Business
{
	public interface IWorkflowParentWithLines
	{
		IEnumerable<string> SupportedTriggerLineTypes { get; }
	}
}