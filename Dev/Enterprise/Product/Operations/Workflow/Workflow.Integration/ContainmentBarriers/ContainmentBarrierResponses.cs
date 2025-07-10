using System;

namespace Enterprise.Workflow.Integration
{
	[Flags]
	public enum ContainmentBarrierResponses
	{
		None = 0,
		Passed = 1 << 0,
		IterationRequired = 1 << 1,
		DeferredToAnotherResource = 1 << 2,
		Canceled = 1 << 3,
		AcceptIterationCreatedByOtherResource = 1 << 4,
	}
}
