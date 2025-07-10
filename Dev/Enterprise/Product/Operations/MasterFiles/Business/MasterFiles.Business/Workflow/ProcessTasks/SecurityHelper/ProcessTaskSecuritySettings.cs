using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Workflow;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business
{
	class ProcessTaskSecuritySettings : IProcessTaskSecuritySettings
	{
		public string StaffCode => GlbStaff.CurrentUser.GS_Code;

		public ICollection<Guid> StaffCapabilities => GlbStaff.CurrentUser.CapabilityPivots.Select(c => c.G5_G4_Capability.ToGuid()).ToArray();

		public bool CanCloseTaskNotAssignedToSelf => Env.Security.WorkflowTasksCloseTaskNotAssignedToSelf.IsAllowed;

		public bool CanCancelAllTasks => Env.Security.WorkflowTasksCanCancelAllTasks.IsAllowed;
	}
}
