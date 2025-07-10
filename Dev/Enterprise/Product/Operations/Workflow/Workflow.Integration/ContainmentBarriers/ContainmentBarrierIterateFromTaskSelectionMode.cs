using System;

namespace Enterprise.Workflow.Integration
{
	[Flags]
	public enum ContainmentBarrierIterateFromTaskSelectionMode
	{
		None = 0,
		ExcludeSameResourceAsQcbTask = 1 << 0,
		ExcludeDifferentResourceAsQcbTask = 1 << 1,
		ExcludeContainmentBarrierTasks = 1 << 2
	}
}
