using System;

namespace Enterprise.ProcessManagement.Business
{
	/// <summary>
	/// These values are very precisely chosen, please do not modify them without carefully looking at how they're used!
	/// </summary>
	[Flags]
	public enum IssueStatusModifiers
	{
		None = 0,
		Completed = 1 << 1,
		Unstarted = 1 << 2,
		WorkInProgress = 1 << 4,
	}
}
