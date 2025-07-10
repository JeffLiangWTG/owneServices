using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.MasterFiles.Module.ProcessManagement.ProcessTasks
{
	public class ProcessTasksOperationalActionSupporter : OperationalActionSupporter
	{
		public override Type RootType
		{
			get { return typeof(ProcessTask); }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.TaskList; }
		}

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.WorkflowTasks;
	}
}
