using System;

namespace Enterprise.MasterFiles.Integration
{
	public interface IUpdateScreeningStatusNotifier
	{
		event EventHandler ShouldUpdateScreeningStatusChangedToTrue;
	}
}
