using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eHubDeploymentTools
{
	public delegate void TaskUpdated(object sender, bool IsValid);

	interface ITask
	{
		void Execute();
		event TaskUpdated Updated;
	}
}
