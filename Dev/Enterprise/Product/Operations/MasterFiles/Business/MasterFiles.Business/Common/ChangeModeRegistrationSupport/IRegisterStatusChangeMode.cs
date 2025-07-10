using System;

namespace Enterprise.MasterFiles.Business
{
	public interface IRegisterStatusChangeMode
	{
		IDisposable TemporarilySetStatusChangeModeToChangedByOperationalAction();
		IDisposable TemporarilySetStatusChangeModeToChangedByTriggerOrMilestone();
	}
}
