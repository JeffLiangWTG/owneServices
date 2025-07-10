using System;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IProcessJobTriggerLink : IWorkflowTrigger
	{
		ZGuid P9L_P9T_TemplateTrigger { get; set; }
		ZGuid P9L_ParentId { get; set; }
		ZString P9L_ParentTableCode { get; set; }
		event EventHandler OnDeleted;
	}
}
